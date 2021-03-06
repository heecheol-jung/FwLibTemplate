using FwLib.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FwLib.NetUnitTest
{
    [TestClass]
    public class BinPacketBuilderUnitTest
    {
        UInt16 _crc16 = 0;
        IFwLibMessage _message;

        [TestInitialize]
        public void InitializeBeforeEveryTest()
        {
            _crc16 = 0;
            _message = null;
        }

        [TestMethod]
        public void TestReadHardwareVersionCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                // TODO : It does not work as I expected.
                //Header = new FwLibBinMessageHeader()
                //{
                //    DeviceId = 1,
                //    SequenceNumber = 1
                //},
                MessageId = FwLibMessageId.ReadHardwareVersion
            };

            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 1;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadHardwareVersion, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x06, _message.Buffer[9], "CRC16 byte1 should be 0x06.");
            Assert.AreEqual((byte)0x03, _message.Buffer[10], "CRC16 byte2 should be 0x03.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHardwareVersionOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion,
                Arguments = new List<object>()
                {
                    (byte)1,
                    (byte)2,
                    (byte)3
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 1;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;
            
            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(15, _message.Buffer.Length, "Total packet length should be 15");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)9, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 9.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadHardwareVersion, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Major
            Assert.AreEqual((byte)1, _message.Buffer[9], "Major should be 1.");
            // Minor
            Assert.AreEqual((byte)2, _message.Buffer[10], "Minor should be 2.");
            // Revision
            Assert.AreEqual((byte)3, _message.Buffer[11], "Revision should be 3.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[11])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 11);

            Assert.AreEqual((byte)0xf3, _message.Buffer[12], "CRC16 byte1 should be 0xf3.");
            Assert.AreEqual((byte)0x78, _message.Buffer[13], "CRC16 byte2 should be 0x78.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[12], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[13], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[14], "Buffer[14] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHardwareVersionErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 1;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadHardwareVersion, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x67, _message.Buffer[9], "CRC16 byte1 should be 0x67.");
            Assert.AreEqual((byte)0x33, _message.Buffer[10], "CRC16 byte2 should be 0x33.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 2;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadFirmwareVersion, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 2.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)2, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x0c, _message.Buffer[9], "CRC16 byte1 should be 0x0c.");
            Assert.AreEqual((byte)0x06, _message.Buffer[10], "CRC16 byte2 should be 0x06.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    (byte)2,    // Major
                    (byte)3,    // Minor
                    (byte)4     // Revision
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 2;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(15, _message.Buffer.Length, "Total packet length should be 15");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)9, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 9.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadFirmwareVersion, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)2, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Major
            Assert.AreEqual((byte)2, _message.Buffer[9], "Major should be 2.");
            // Minor
            Assert.AreEqual((byte)3, _message.Buffer[10], "Minor should be 3.");
            // Revision
            Assert.AreEqual((byte)4, _message.Buffer[11], "Revision should be 4.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[11])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 11);

            Assert.AreEqual((byte)0xaa, _message.Buffer[12], "CRC16 byte1 should be 0xaa.");
            Assert.AreEqual((byte)0x57, _message.Buffer[13], "CRC16 byte2 should be 0x57.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[12], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[13], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[14], "Buffer[14] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 2;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadFirmwareVersion, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 2.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)2, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x6D, _message.Buffer[9], "CRC16 byte1 should be 0x6D.");
            Assert.AreEqual((byte)0x36, _message.Buffer[10], "CRC16 byte2 should be 0x36.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadGpioCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)1 // GPIO number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 3;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _message.Buffer.Length, "Total packet length should be 13");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)7, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 7.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadGpio, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 3.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)3, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Port number
            Assert.AreEqual((byte)1, _message.Buffer[9], "GPIO number should be 1.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[9])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 9);

            Assert.AreEqual((byte)0x05, _message.Buffer[10], "CRC16 byte1 should be 0x05.");
            Assert.AreEqual((byte)0x02, _message.Buffer[11], "CRC16 byte2 should be 0x02.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadGpioOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)1,    // GPIO number
                    (byte)1     // GPIO value
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 3;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(14, _message.Buffer.Length, "Total packet length should be 14");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)8, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 8.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadGpio, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 3.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)3, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // GPIO number
            Assert.AreEqual((byte)1, _message.Buffer[9], "GPIO number should be 1.");
            // GPIO value
            Assert.AreEqual((byte)1, _message.Buffer[10], "GPIO value should be 1.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[9])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 10);

            Assert.AreEqual((byte)0x6B, _message.Buffer[11], "CRC16 byte1 should be 0x6B.");
            Assert.AreEqual((byte)0x35, _message.Buffer[12], "CRC16 byte2 should be 0x35.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[11], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[12], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[13], "Buffer[13] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadGpioErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadGpio
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 3;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadGpio, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 3.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)3, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x6B, _message.Buffer[9], "CRC16 byte1 should be 0x6B.");
            Assert.AreEqual((byte)0x35, _message.Buffer[10], "CRC16 byte2 should be 0x35.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    (byte)1,    // GPIO number,
                    (byte)1     // GPIO value
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 4;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(14, _message.Buffer.Length, "Total packet length should be 14");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)8, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 8.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.WriteGpio, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 4.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)4, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // GPIO number
            Assert.AreEqual((byte)1, _message.Buffer[9], "GPIO number should be 1.");

            // GPIO value
            Assert.AreEqual((byte)1, _message.Buffer[10], "GPIO value should be 1.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[10])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 10);

            Assert.AreEqual((byte)0xa3, _message.Buffer[11], "CRC16 byte1 should be 0xa3.");
            Assert.AreEqual((byte)0xd0, _message.Buffer[12], "CRC16 byte2 should be 0xd0.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[11], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[12], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[13], "Buffer[13] should end with 0x03.");
        }

        [TestMethod]
        public void TestWriteGpioOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.WriteGpio
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 4;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.WriteGpio, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 4.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)4, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x78, _message.Buffer[9], "CRC16 byte1 should be 0x78.");
            Assert.AreEqual((byte)0x3c, _message.Buffer[10], "CRC16 byte2 should be 0x3c.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestWriteGpioErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.WriteGpio
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 4;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.WriteGpio, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 4.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)4, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x79, _message.Buffer[9], "CRC16 byte1 should be 0x79.");
            Assert.AreEqual((byte)0x3C, _message.Buffer[10], "CRC16 byte2 should be 0x3C.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestButtonPressedEventMessageBuild()
        {
            _message = new FwLibBinMessageEvent()
            {
                MessageId = FwLibMessageId.ButtonEvent,
                Arguments = new List<object>()
                {
                    (byte)1,    // Button number
                    (byte)1     // Button status(Pressed)
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 0;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(14, _message.Buffer.Length, "Total packet length should be 14");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)1, _message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)8, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 8.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ButtonEvent, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 7.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Event, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Button number
            Assert.AreEqual((byte)1, _message.Buffer[9], "Button number should be 1.");
            // Button status
            Assert.AreEqual((byte)1, _message.Buffer[10], "Button status should be 1.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[10])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 10);

            Assert.AreEqual((byte)0x16, _message.Buffer[11], "CRC16 byte1 should be 0x16.");
            Assert.AreEqual((byte)0x0a, _message.Buffer[12], "CRC16 byte2 should be 0x0a.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[11], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[12], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[13], "Buffer[13] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)7     // Sensor number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _message.Buffer.Length, "Total packet length should be 13");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)7, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 7.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadTemperature, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 6.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual((byte)7, _message.Buffer[9], "Sensor number should be 7.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[9])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 9);

            Assert.AreEqual((byte)0xD3, _message.Buffer[10], "CRC16 byte1 should be 0x05.");
            Assert.AreEqual((byte)0x6A, _message.Buffer[11], "CRC16 byte2 should be 0x02.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)2,    // Sensor number
                    (UInt16)123 // Sensor value
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(15, _message.Buffer.Length, "Total packet length should be 15");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)9, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 9.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadTemperature, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 6.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual((byte)2, _message.Buffer[9], "Buffer[9] should be 0x02.");

            // Sensor value
            Assert.AreEqual((byte)0, _message.Buffer[10], "Buffer[10] should be 0x00.");
            Assert.AreEqual((byte)123, _message.Buffer[11], "Buffer[11] value should be 123.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[11])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 11);

            Assert.AreEqual((byte)0xA6, _message.Buffer[12], "CRC16 byte1 should be 0xA6.");
            Assert.AreEqual((byte)0x6F, _message.Buffer[13], "CRC16 byte2 should be 0x6F.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[12], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[13], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[14], "Buffer[14] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperature
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadTemperature, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 6.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x03, _message.Buffer[9], "CRC16 byte1 should be 0x03.");
            Assert.AreEqual((byte)0x01, _message.Buffer[10], "CRC16 byte2 should be 0x01.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHumidityCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)7     // Sensor number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _message.Buffer.Length, "Total packet length should be 13");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)7, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 7.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadHumidity, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 7.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual((byte)7, _message.Buffer[9], "Sensor number should be 7.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[9])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 9);

            Assert.AreEqual((byte)0xDB, _message.Buffer[10], "CRC16 byte1 should be 0xDB.");
            Assert.AreEqual((byte)0x6E, _message.Buffer[11], "CRC16 byte2 should be 0x6E.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHumidityOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)7,    // Sensor number
                    (UInt16)234 // Sensor value
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(15, _message.Buffer.Length, "Total packet length should be 15");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)9, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 9.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadHumidity, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 7.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual((byte)7, _message.Buffer[9], "Buffer[9] should be 0x07.");

            // Sensor value
            Assert.AreEqual((byte)0, _message.Buffer[10], "Buffer[10] should be 0x00.");
            Assert.AreEqual((byte)234, _message.Buffer[11], "Buffer[11] value should be 234.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[11])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 11);

            Assert.AreEqual((byte)0x03, _message.Buffer[12], "CRC16 byte1 should be 0x03.");
            Assert.AreEqual((byte)0x75, _message.Buffer[13], "CRC16 byte2 should be 0x75.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[12], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[13], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[14], "Buffer[14] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHumidityErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHumidity
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadHumidity, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 7.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x07, _message.Buffer[9], "CRC16 byte1 should be 0x07.");
            Assert.AreEqual((byte)0x03, _message.Buffer[10], "CRC16 byte2 should be 0x03.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureHumidityCommandMessageBuild()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadTemperatureAndHumidity,
                Arguments = new List<object>()
                {
                    (byte)7     // Sensor number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _message.Buffer.Length, "Total packet length should be 13");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)7, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 7.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadTemperatureAndHumidity, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 8.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Command, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)1, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual((byte)7, _message.Buffer[9], "Sensor number should be 7.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[9])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 9);

            Assert.AreEqual((byte)0xA3, _message.Buffer[10], "CRC16 byte1 should be 0xA3.");
            Assert.AreEqual((byte)0x52, _message.Buffer[11], "CRC16 byte2 should be 0x52.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureHumidityOkResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperatureAndHumidity,
                Arguments = new List<object>()
                {
                    (byte)7,        // Sensor number
                    (UInt16)123,    // Temperature value
                    (UInt16)234     // Humidity value
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(17, _message.Buffer.Length, "Total packet length should be 17.");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)11, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 11.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadTemperatureAndHumidity, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 8.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor value
            Assert.AreEqual((byte)7, _message.Buffer[9], "Buffer[9] should be 0x07.");

            // Temperature value
            Assert.AreEqual((byte)0, _message.Buffer[10], "Buffer[10] should be 0x00.");
            Assert.AreEqual((byte)123, _message.Buffer[11], "Buffer[11] value should be 123.");

            // Humidity value
            Assert.AreEqual((byte)0, _message.Buffer[12], "Buffer[12] should be 0x00.");
            Assert.AreEqual((byte)234, _message.Buffer[13], "Buffer[13] should be 234.");
            
            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[11])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 13);

            Assert.AreEqual((byte)0x28, _message.Buffer[14], "CRC16 byte1 should be 0x28.");
            Assert.AreEqual((byte)0xE5, _message.Buffer[15], "CRC16 byte2 should be 0xE5.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[14], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[15], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[16], "Buffer[13] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureAndHumidityErrorResponseMessageBuild()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperatureAndHumidity
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            byte flagExpected = 0;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            Assert.IsNotNull(_message.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _message.Buffer.Length, "Total packet length should be 12");
            // STX
            Assert.AreEqual(FwLibConstant.BIN_MSG_STX, _message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)0, _message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, _message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)6, _message.Buffer[4], "Buffer[4] should be 6.");

            // Length
            Assert.AreEqual((byte)6, _message.Buffer[5], $"Buffer[{FwLibConstant.BinHeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FwLibMessageId.ReadTemperatureAndHumidity, _message.Buffer[6], $"Buffer[{FwLibConstant.BinHeaderMessageIdFieldIndex}] should be 8.");

            // Flag1
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)FwLibMessageCategory.Response, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)0, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, (byte)5, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FwLibConstant.BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[7], $"Buffer[{FwLibConstant.BinHeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FwLibUtil.BitFieldSet(flagExpected, FwLibConstant.ERROR, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_MASK, FwLibConstant.BIN_MSG_HDR_FLG2_ERROR_POS);
            Assert.AreEqual(flagExpected, _message.Buffer[8], $"Buffer[{FwLibConstant.BinHeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FwLibUtil.CRC16(_message.Buffer, 1, 8);

            Assert.AreEqual((byte)0x3B, _message.Buffer[9], "CRC16 byte1 should be 0x3B.");
            Assert.AreEqual((byte)0x1D, _message.Buffer[10], "CRC16 byte2 should be 0x1D.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), _message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)(_crc16 & 0xff), _message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FwLibConstant.BIN_MSG_ETX, _message.Buffer[11], "Buffer[11] should end with 0x03.");
        }
    }
}
