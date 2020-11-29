using FwLib.Net;

namespace FwLib.NetUnitTest.TestSupport
{
    public enum StartStatus
    {
        Stopped,

        Starting,

        Started,

        Stopping
    }

    public delegate void FwLibEventReceived(object sender, IFwLibMessage evt);

    public interface IMessageManager
    {
        StartStatus StartStatus { get; }
        FwLibEventReceived OnEventReceived { get; set; }

        void Start(MessageManagerSetting setting);
        void Stop();

        IFwLibMessage ProcessCommand(IFwLibMessage command);
    }
}
