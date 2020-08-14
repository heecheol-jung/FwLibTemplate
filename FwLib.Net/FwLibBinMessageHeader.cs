using System;

namespace FwLib.Net
{
    public class FwLibBinMessageHeader
    {
        #region Publie Properties
        // 4 bytes
        public UInt32 DeviceId { get; set; }
        // 1 byte
        public byte Length { get; set; }
        // 1 byte
        public FwLibMessageId MessageId { get; set; }
        // 1 byte
        public byte Flag1 { get; set; } // Return expected : true
        // 1 byte
        public byte Flag2 { get; set; }

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

        // TODO : BIN_MSG_HDR_FLG1_MSG_TYPE_MASK -> BIN_MSG_HDR_FLG1_MSG_CAT_MASK
        // TODO : BIN_MSG_HDR_FLG1_MSG_TYPE_POS -> BIN_MSG_HDR_FLG1_MSG_CAT_POS
        public FwLibMessageCategory MessageCategory
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

        #endregion

        #region Constructors
        public FwLibBinMessageHeader()
        {
            DeviceId = FwLibConstant.DEVICE_ID_UNKNOWN;
            Length = 0;
            MessageId = FwLibMessageId.Unknown;
            Flag1 = 0; // Return expected : true
            Flag2 = 0;
            MessageCategory = FwLibMessageCategory.Unknown;
        }
        #endregion
    }
}
