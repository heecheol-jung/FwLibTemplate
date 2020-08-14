using System.Collections.Generic;

namespace FwLib.Net
{
    public class FwLibMessageTemplate
    {
        public string MessageIdForCApi { get; set; }
        public FwLibMessageId MessageId { get; set; }
        public FwLibMessageCategory MessageType { get; set; } = FwLibMessageCategory.Unknown;
        public List<FwLibArgumentTemplate> Arguments { get; set; }
    }
}
