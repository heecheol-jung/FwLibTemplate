using System;
using System.Runtime.InteropServices;

namespace FwLib.Net
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageHeaderStruct
    {
        public byte Stx;
        public UInt32 DeviceId;
        public byte Length;
        public byte MessageId;
        public byte Flag1;
        public byte Flag2;

        #region Flag1 Details
        public byte Flag1Reserved
        {
            get
            {
                return FwLibUtil.BitFieldGet(Flag1, FwLibConstant.BIN_MSG_HDR_FLG1_RESERVED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RESERVED_POS);
            }
            set
            {
                Flag1 = FwLibUtil.BitFieldSet(Flag1, value, FwLibConstant.BIN_MSG_HDR_FLG1_RESERVED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RESERVED_POS);
            }
        }

        // TODO : MessageType -> MessageCategory
        public FwLibMessageCategory MessageType
        {
            get
            {
                return (FwLibMessageCategory)FwLibUtil.BitFieldGet(Flag1, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            }
            set
            {
                if ((byte)value > FwLibConstant.MessageCategoryMax)
                {
                    throw new ArgumentOutOfRangeException("MessageType out of rnage.");
                }
                Flag1 = FwLibUtil.BitFieldSet(Flag1, (byte)value, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            }
        }

        public bool ReturnExpected
        {
            get
            {
                byte ret = FwLibUtil.BitFieldGet(Flag1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);

                return ret == 0 ? false : true;
            }
            set
            {
                byte boolValue = value == true ? (byte)1 : (byte)0;

                Flag1 = FwLibUtil.BitFieldSet(Flag1, boolValue, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            }
        }

        public byte SequenceNumber
        {
            get
            {
                return FwLibUtil.BitFieldGet(Flag1, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            }
            set
            {
                if (value > FwLibConstant.BIN_MSG_MAX_SEQUENCE)
                {
                    throw new ArgumentOutOfRangeException("SequenceNumber out of rnage.");
                }
                Flag1 = FwLibUtil.BitFieldSet(Flag1, value, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            }
        }
        #endregion

        #region Flag2 Details
        public byte Flag2Reserved
        {
            get
            {
                return FwLibUtil.BitFieldGet(Flag2, FwLibConstant.BIN_MSG_HDR_FLG2_RESERVED_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_RESERVED_POS);
            }
            set
            {
                Flag2 = FwLibUtil.BitFieldSet(Flag2, value, FwLibConstant.BIN_MSG_HDR_FLG2_RESERVED_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_RESERVED_POS);
            }
        }

        public byte Error
        {
            get
            {
                return FwLibUtil.BitFieldGet(Flag2, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            }
            set
            {
                if (value > FwLibConstant.BinMsgErrorMax)
                {
                    throw new ArgumentOutOfRangeException("Error out of rnage.");
                }
                Flag2 = FwLibUtil.BitFieldSet(Flag2, value, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageTailStruct
    {
        public UInt16 Crc16;
        public byte Etx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibHwVersionStruct
    {
        public byte Major;
        public byte Minor;
        public byte Revision;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibFwVersionStruct
    {
        public byte Major;
        public byte Minor;
        public byte Revision;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadHwVerResponseStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public FwLibHwVersionStruct HwVersion;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadFwVerResponseStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public FwLibFwVersionStruct FwVersion;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadGpioCommandStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte GpioNumber;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadGpioResponseStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte GpioNumber;
        public byte GpioValue;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageWriteGpioCommandStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte GpioNumber;
        public byte GpioValue;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageButtonEventStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte ButtonNumber;
        public byte ButtonValue;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadTemperatureCommandStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte SensorNumber;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadHumidityCommandStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte SensorNumber;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadTemperatureResponseStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte SensorNumber;
        public UInt16 SensorValue;
        public FwLibBinMessageTailStruct Tail;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FwLibBinMessageReadHumidityResponseStruct
    {
        public FwLibBinMessageHeaderStruct Header;
        public byte SensorNumber;
        public UInt16 SensorValue;
        public FwLibBinMessageTailStruct Tail;
    }
}
