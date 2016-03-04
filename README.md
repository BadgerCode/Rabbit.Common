Rabbit Common
=============
* **Minimum RabbitMQ .NET Client**: 3.5.7
* **Minimum (tested) RabbitMQ Server**: 3.3.5

*A library for common Rabbit components.*

![Rabbit](http://i.imgur.com/085asU3.jpg)

**Features**
* Makes connections resilient to race conditions
* Makes the consumer resilient to connections dropping
* Makes the publisher resilient to messages failing to be routed
* Encodes/decodes and serialises/deserialises messages from/to your message models
* Provides helper classes for integration tests


Examples
========

In the examples, I am using the **UpdateUserCommand** class to model the kind of messages on my queue.
You can use any class to model your messages as long as the fields you wish to publish are JSON serialisable.

```C#
// An example model of the kind of message you want to put on & read off the queue
public class UpdateUserCommand
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```
<br>

Creating a connection to Rabbit
-------------------------------


```C#
var rabbitConfig = new RabbitConfig
{
    Username = "User",
    Password = "Password",
    Host = "localhost",
    Port = 5672,
    VirtualHost = "/",
    OriginatingHost = "My.Machine.Name", // Try: System.Environment.MachineName
    ServiceName = "My.Application.Name"
};

var connection = new RabbitConnectionFactory()
                     .Create(rabbitConfig);

// You should .Connect before trying to use the connection.
// If your connection details are invalid, this will throw an exception
connection.Connect();

// You should .Abort this connection when you're done with it.
// Otherwise, it will only abort when disposed
```
<br>


Consuming messages from a queue (blocking)
------------------------------------------

The thread will stay blocked until a message is dequeued.
The dequeued message will have two properties:
* **Headers**: A dictionary of string keys and string values
* **Body**: The body of the message deserialised to the class specified when creating the consumer

If the connection drops, an exception will be thrown.
If the dequeued message does not match your model, the properties of the returned message body will be nulled out.


```C#
var connection = ... // See first example

var consumer = new BlockingRabbitConsumerFactory<UpdateUserCommand>(connection)
                   .CreateQueueConsumer("MyQueueName");

// This will block the thread until a message is read
var receivedMessage = consumer.Dequeue();
```
<br>


Publishing messages to a queue
------------------------------

This currently doesn't support routing keys.

You can optionally specify where to send messages which fail to be delivered.
Messages will fail to deliver some point after publishing them if Rabbit was unable to route the message to any queue.

The failed message will have the following properties:
* **Message**: A Rabbit message with the original **Headers** and **Body** you attempted to publish
* **Response**: A Rabbit response with the **ReplyCode** and **ReplyText**

```C#
var connection = ... // See first example

var messageHeaders = new Dictionary<string, string>
{
    { "Command", "UpdateUser" },
    { "Version", "1.0" }
};
var messageBody = new UpdateUserCommand
{
    UserId = Guid.NewGuid(),
    FirstName = "John",
    LastName = "Smith"
};
var messageToPublish = new RabbitMessage<UpdateUserCommand>(messageHeaders, messageBody);

var publisher = new RabbitQueuePublisherFactory<UpdateUserCommand>(connection)
                    .Create();

publisher.Publish(messageToPublish, "MyExchangeName", failedMessage => { ... });
```
<br>

Creating a queue
----------------

**Consume all messages sent to the exchange**
```C#
var connection = ... // See first example

var queueName = "MyQueueName";
var exchangeName = "MyExchangeName";

var queueCreator = new RabbitQueueSetupFactory().Create(connection);
queueCreator.CreateQueue(exchangeName, queueName);
```
<br>

**Consume all messages which have the following headers**

| Key     |Value       |
|---------|:-----------|
| Command |UpdateUser  |
| Version |1.0         |

``` c#
var connection = ... // See example above

var queueName = "MyQueueName";
var exchangeName = "MyExchangeName";
var mustMatchAllBindings = true;
var queueHeaderBindings = new Dictionary<string, string>
{
    { "Command", "UpdateUser" },
    { "Version",  "1.0" }
};

var queueCreator = new RabbitQueueSetupFactory().Create(connection);
queueCreator.CreateHeaderExchangeQueue(exchangeName, queueName, queueHeaderBindings, mustMatchAllBindings);
```
<br>
---


Test Helpers
============



----
In these examples, I've assumed the existence of a **TestMessageModel** class.

You can use whatever class you want to model the kinds of messages you'll be using in your tests.

Creating & deleting queues
--------------------------

```C#
var rabbitConfig = new RabbitConfig( ... );
var headerRoutingRules = new Dictionary<string, string> { { "RoutingHeader", "RoutingValue" } };
TestQueueCreator.CreateHeaderExchangeQueue(rabbitConfig, "MyExchangeName", "MyQueueName", headerRoutingRules);

TestQueueRemover.RemoveQueue(rabbitConfig, "queue.name");
```
<br>

## Consuming ##
With a temp queue:
```C#
var rabbitConfig = new RabbitConfig( ... );
var routingRules = new Dictionary<string, string> { { "RoutingHeader", "RoutingValue" } };
var testConsumer = TestMessageConsumer<TestMessageModel>
                   .CreateWithTempQueueAndStart(rabbitConfig, "MyExchangeName", routingRules);

// This will wait up to 5 seconds to read a message
var message = testConsumer.TryGetMessage(TimeSpan.FromSeconds(5));
```

From an existing queue:
```C#
var rabbitConfig = new RabbitConfig( ... );
var testConsumer = TestMessageConsumer<TestMessageModel>
                   .CreateForExistingQueueAndStart(rabbitConfig, "MyQueueName");

// This will wait up to 5 seconds to read a message
var message = testConsumer.TryGetMessage(TimeSpan.FromSeconds(5));
```
<br>

## Publishing ##
You can optionally provide a function to deal with messages which fail to publish.

The failed message will have the following properties:
* **Message**: A Rabbit message with the original **Headers** and **Body** you attempted to publish
* **Response**: A Rabbit response with the **ReplyCode** and **ReplyText**
```C#
var rabbitConfig = new RabbitConfig( ... );

var message = new RabbitMessage<TestMessageModel>(
	new Dictionary<string, string> { { "HeaderKey", "HeaderValue" } }, 
	new TestMessageModel()
);
var testQueueMessages = new List<RabbitMessage<TestMessageModel>> { 
    message
};


TestQueuePublisher.Publish(rabbitConfig, "MyExchangeName", message, failedMessage => { ... });
TestQueuePublisher.PublishMany(rabbitConfig, "MyExchangeName", testQueueMessages, failedMessage => { ... });
```