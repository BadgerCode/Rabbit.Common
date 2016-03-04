namespace Rabbit.Common.Interfaces.Factories
{
    public interface IQueueConnectionFactory<out TConnectionManager, in TConfiguration>
    {
        TConnectionManager Create(TConfiguration config);
    }
}