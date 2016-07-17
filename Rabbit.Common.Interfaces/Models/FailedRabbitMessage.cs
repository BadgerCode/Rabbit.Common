namespace Rabbit.Common.Interfaces.Models
{
    public class FailedRabbitMessage<TMessageBody>
    {
        public RabbitMessage<TMessageBody> Message { get; set; }
        public FailedRabbitMessageResponse Response { get; set; }
        

        public FailedRabbitMessage(RabbitMessage<TMessageBody> message, ushort replyCode, string replyText)
        {
            Message = message;
            Response = new FailedRabbitMessageResponse(replyCode, replyText);
        }
    }

    public class FailedRabbitMessageResponse
    {
        public ushort ReplyCode { get; set; }
        public string ReplyText { get; set; }

        public FailedRabbitMessageResponse(ushort replyCode, string replyText)
        {
            ReplyCode = replyCode;
            ReplyText = replyText;
        }
    }
}