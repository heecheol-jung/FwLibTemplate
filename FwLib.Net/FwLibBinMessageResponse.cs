using System;
using System.Collections.Generic;

namespace FwLib.Net
{
    public class FwLibBinMessageResponse : IFwLibBinMessage
    {
        #region Private Data
        private FwLibBinMessageHeader _header { get; set; }
        #endregion

        #region Public Properties
        public FwLibMessageType MessageType
        {
            get => FwLibMessageType.Binary;
        }

        public FwLibMessageCategory MessageCategory
        {
            get
            {
                return _header.MessageCategory;
            }
            set
            {
                _header.MessageCategory = value;
            }
        }
        public FwLibMessageId MessageId
        {
            get
            {
                return _header.MessageId;
            }
            set
            {
                _header.MessageId = value;
            }
        }
        public List<object> Arguments { get; set; }
        public byte[] Buffer { get; set; }
        public FwLibBinMessageHeader Header
        {
            get => _header;
            set => _header = value;
        }
        public DateTime ReceiveTime { get; set; }
        #endregion

        #region Constructors
        public FwLibBinMessageResponse()
        {
            _header = new FwLibBinMessageHeader
            {
                MessageCategory = FwLibMessageCategory.Response,
                ReturnExpected = false
            };
            Arguments = null;
            Buffer = null;
            ReceiveTime = DateTime.MinValue;
        }
        #endregion
    }
}
