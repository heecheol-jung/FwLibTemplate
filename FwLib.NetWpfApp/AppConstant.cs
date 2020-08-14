
namespace FwLib.NetWpfApp
{
    public enum ParserType
    {
        Unknown,

        Binary,

        Text
    }

    public enum StartStatus
    {
        Stopped,

        Starting,

        Started,

        Stopping
    }

    public static class AppConstant
    {
        public const string STR_OPEN = "Open";
        public const string STR_OPENING = "Opening";
        public const string STR_CLOSE = "Close";
        public const string STR_CLOSING = "Closing";
    }
}
