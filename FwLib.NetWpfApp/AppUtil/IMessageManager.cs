using FwLib.Net;

namespace FwLib.NetWpfApp.AppUtil
{
    public delegate void FwLibEventReceived(object sender, IFwLibMessage evt);

    public interface IMessageManager
    {
        StartStatus StartStatus { get; }
        FwLibEventReceived OnEventReceived { get; set; }
        
        void Start(MessageManagerSetting setting);
        void Stop();

        CommandResult ReadHardwareVersion(IFwLibMessage command);
        CommandResult ReadFirmwareVersion(IFwLibMessage command);
        CommandResult WriteGpio(IFwLibMessage command);
        CommandResult ReadGpio(IFwLibMessage command);
    }
}
