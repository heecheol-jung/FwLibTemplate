
namespace FwLib.Net
{
    public interface IFwLibBinMessage : IFwLibMessage
    {
        FwLibBinMessageHeader Header { get; set; }
    }
}
