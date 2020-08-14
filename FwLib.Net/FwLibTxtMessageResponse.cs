using System;
using System.Collections.Generic;

namespace FwLib.Net
{
    public class FwLibTxtMessageResponse : IFwLibTxtMessage
    {
        #region Public Properties
        public FwLibMessageType MessageType { get { return FwLibMessageType.Text; } }
        public FwLibMessageCategory MessageCategory { get; set; }
        public FwLibMessageId MessageId { get; set; }
        public UInt32 DeviceId { get; set; }
        public List<object> Arguments { get; set; }
        public byte[] Buffer { get; set; } = null;
        public DateTime ReceiveTime { get; set; }
        #endregion

        #region Constructors
        public FwLibTxtMessageResponse()
        {
            MessageCategory = FwLibMessageCategory.Response;
            MessageId = FwLibMessageId.Unknown;
            DeviceId = FwLibConstant.DEVICE_ID_UNKNOWN;
            Arguments = null;
            Buffer = null;
            ReceiveTime = DateTime.MinValue;
        }
        #endregion
    }
}
