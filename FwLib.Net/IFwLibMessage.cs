using System.Collections.Generic;

namespace FwLib.Net
{
    public interface IFwLibMessage
    {
        FwLibMessageType MessageType { get; }
        FwLibMessageCategory MessageCategory { get; set; }
        FwLibMessageId MessageId { get; set; }
        List<object> Arguments { get; set; }
        byte[] Buffer { get; set; }
    }
}
