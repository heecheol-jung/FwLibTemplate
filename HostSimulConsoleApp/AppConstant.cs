
namespace HostSimulConsoleApp
{
    public enum FirmwareType
    {
        Application,

        Bootloader
    }

    public enum MessageType
    {
        Binary,

        Text
    }

    public static class AppConstant
    {
        public const string STR_HOST_SIMULATOR = "host simulator";
        public const string STR_MESSAGE_TYPE = "message type";
        public const string STR_FIRMWARE_TYPE = "firmware type";
        public const string STR_COM_PORT_NAME = "com port name";
        public const string STR_START = "start";
        public const string STR_STOP = "stop";
        public const string STR_HW_VER = "hw ver";
        public const string STR_FW_VER = "fw ver";
        public const string STR_BMODE = "bmode";
        public const string STR_RESET = "reset";
        public const string STR_TEMPERATURE = "temperature";
        public const string STR_HUMIDITY = "humidity";
        public const string STR_TEMP_HUM = "temp_hum";
        public const string STR_READ_GPIO = "read gpio";
        public const string STR_WRITE_GPIO = "write gpio";
        public const string STR_FW_INFO = "fw info";
        public const string STR_FW_PKT = "fw pkt";
        public const string STR_FW_FILE_NAME = "fw file name";
        public const string STR_BIN_MSG = "bin msg";
        public const string STR_TXT_MSG = "txt msg";
        public const string STR_APP = "app";
        public const string STR_BOOTLOADER = "bootloader";
        public const string STR_GENERAL_EXAMPLE = "general example";
        public const string STR_BASE64 = "base64";
        public const string STR_MESSAGE_MANAGER = "message manager";
    }
}
