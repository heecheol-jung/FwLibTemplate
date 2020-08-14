using FwLib.Net;

namespace FwLib.NetWpfApp.AppUtil
{
    public class CommandResult
    {
        public IFwLibMessage Command { get; set; }
        public IFwLibMessage Response { get; set; }
    }
}
