namespace Avalanche.Api.Managers.Notifications
{
    public interface INotificationsManager
    {
        void SendDirectMessage(Ism.Broadcaster.Models.MessageRequest message);
    }
}
