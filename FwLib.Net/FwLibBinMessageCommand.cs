using System;
using System.Collections.Generic;

namespace FwLib.Net
{
    public class FwLibBinMessageCommand : IFwLibBinMessage
    {
        #region Private Data
        private FwLibBinMessageHeader _header { get; set; }
        #endregion

        #region Public Properties
        public FwLibMessageType MessageType
        {
            get
            {
                return FwLibMessageType.Binary;
            }
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
        public int MaxTryCount { get; set; }
        public int TryCount
        {
            get
            {
                if (SendTimeHistory?.Count > 0)
                {
                    return SendTimeHistory.Count;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int TryInterval { get; set; }
        public bool ResponseExpected
        {
            get
            {
                return _header.ReturnExpected;
            }
            set
            {
                _header.ReturnExpected = value;
            }
        }
        public int ResponseWaitTimeout { get; set; }
        public List<DateTime> SendTimeHistory { get; set; }
        #endregion

        #region Constructors
        public FwLibBinMessageCommand()
        {
            Header = new FwLibBinMessageHeader
            {
                MessageCategory = FwLibMessageCategory.Command,
                ReturnExpected = true
            };
            Arguments = null;
            Buffer = null;
            MaxTryCount = FwLibConstant.DefaultCommandTryCount;
            TryInterval = FwLibConstant.DefaultCommandTryInterval;
            ResponseWaitTimeout = FwLibConstant.DefaultResponseWaitTimeout;
            SendTimeHistory = null;
        }
        #endregion
    }
}
