using Fl.Net;
using System.Collections.Generic;

namespace FwLib.NetWpfApp.AppUtil
{
    public class FwLibMessageTemplate
    {
        public string MessageIdForCApi { get; set; }
        public FlMessageId MessageId { get; set; }
        public FlMessageCategory MessageType { get; set; } = FlMessageCategory.Unknown;
        public List<FwLibArgumentTemplate> Arguments { get; set; }
    }
}
