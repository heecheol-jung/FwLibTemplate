using System;
using System.Collections.Generic;

namespace FwLib.Net
{
    public class FwLibTxtMessageCommand : IFwLibTxtMessage
    {
        #region Public Properties
        public FwLibMessageType MessageType { get { return FwLibMessageType.Text; } }
        public FwLibMessageCategory MessageCategory { get; set; }
        public FwLibMessageId MessageId { get; set; }
        public UInt32 DeviceId { get; set; }
        public List<object> Arguments { get; set; }
        public byte[] Buffer { get; set; }
        public int MaxTryCount { get; set; }
        public int TryCount { get; set; } = 0;
        public int TryInterval { get; set; }
        public bool ResponseExpected { get; set; }
        public int ResponseWaitTimeout { get; set; }
        public List<DateTime> SendTimeHistory { get; set; }
        #endregion

        #region Constructors
        public FwLibTxtMessageCommand()
        {
            MessageCategory = FwLibMessageCategory.Command;
            MessageId = FwLibMessageId.Unknown;
            DeviceId = FwLibConstant.DEVICE_ID_UNKNOWN;
            Arguments = null;
            Buffer = null;
            MaxTryCount = FwLibConstant.DefaultCommandTryCount;
            TryInterval = FwLibConstant.DefaultCommandTryInterval;
            ResponseExpected = true;
            ResponseWaitTimeout = FwLibConstant.DefaultResponseWaitTimeout;
            SendTimeHistory = null;
        }
        #endregion
    }
}
