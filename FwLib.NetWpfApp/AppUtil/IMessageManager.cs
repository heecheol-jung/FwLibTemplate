using Fl.Net.Message;

namespace FwLib.NetWpfApp.AppUtil
{
    public delegate void FwLibEventReceived(object sender, IFlMessage evt);

    public interface IMessageManager
    {
        StartStatus StartStatus { get; }
        FwLibEventReceived OnEventReceived { get; set; }
        
        void Start(MessageManagerSetting setting);
        void Stop();

        CommandResult ReadHardwareVersion(IFlMessage command);
        CommandResult ReadFirmwareVersion(IFlMessage command);
        CommandResult WriteGpio(IFlMessage command);
        CommandResult ReadGpio(IFlMessage command);
        CommandResult ReadTemperature(IFlMessage command);
        CommandResult ReadHumidity(IFlMessage command);
        CommandResult ReadTemperatureAndHumidity(IFlMessage command);
        CommandResult BootMode(IFlMessage command);
        CommandResult Reset(IFlMessage command);
    }
}
