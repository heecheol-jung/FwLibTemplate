using Fl.Net;
using System.Collections.Generic;

namespace FwLib.NetWpfApp.AppUtil
{
    public class AppMessageTemplate
    {
        public string MessageIdForCApi { get; set; }
        public FlMessageId MessageId { get; set; }
        public FlMessageCategory MessageType { get; set; } = FlMessageCategory.Unknown;
        public List<AppArgumentTemplate> Arguments { get; set; }
    }
}
