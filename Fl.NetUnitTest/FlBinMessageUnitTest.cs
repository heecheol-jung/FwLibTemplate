using Fl.Net;
using Fl.Net.Message;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fl.NetUnitTest
{
    [TestClass]
    public class FlBinMessageUnitTest
    {
        UInt16 _crc16 = 0;
        IFlBinMessage _binMsg = null;
        IFlMessage message = null;
        byte flagExpected = 0;

        byte calculate_length_field_value(byte msg_size)
        {
            //                                     device id field    length field 
            //      header size(8)                    size(4)            size(1)                       crc(2)             etx(1)
            return (byte)((8 - sizeof(UInt32) - sizeof(byte)) + msg_size + sizeof(UInt16) + sizeof(byte));
        }

        byte data_length_for_crc(byte msg_size)
        {
            // header size(8) + msg_size
            return (byte)(8 + msg_size);
        }

        [TestInitialize]
        public void InitializeBeforeEveryTest()
        {
            _crc16 = 0;
            _binMsg = null;
            message = null;
        }

        [TestMethod]
        public void TestReadHardwareVersionCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadHardwareVersion
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadHardwareVersion, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(0));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHardwareVersionOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadHardwareVersion,
                Arguments = new List<object>()
                {
                    "a.1.2.3"
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(19, _binMsg.Buffer.Length, "Total packet length should be 19");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            string arg1 = (string)_binMsg.Arguments[0];
            Assert.AreEqual((byte)calculate_length_field_value((byte)arg1.Length), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadHardwareVersion, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            string verString = Encoding.ASCII.GetString(message.Buffer, 9, arg1.Length);
            Assert.AreEqual(arg1, verString);
            
            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[15])
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc((byte)arg1.Length));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[16], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[17], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[18], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadFirmwareVersion
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadFirmwareVersion, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)2, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(0));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    "a.2.3.4"
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(19, _binMsg.Buffer.Length, "Total packet length should be 19");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            string arg1 = (string)_binMsg.Arguments[0];
            Assert.AreEqual((byte)calculate_length_field_value((byte)arg1.Length), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadFirmwareVersion, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)2, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            string verString = Encoding.ASCII.GetString(message.Buffer, 9, arg1.Length);
            Assert.AreEqual(arg1, verString);

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[15])
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc((byte)arg1.Length));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[16], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[17], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[18], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadGpioCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)2
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 3;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(1), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadGpio, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)3, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // GPIO number
            Assert.AreEqual(2, message.Buffer[9]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(1));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadGpioOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)2,
                    (byte)1
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 3;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(14, _binMsg.Buffer.Length, "Total packet length should be 14");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value((byte)2), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadGpio, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)3, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // GPIO number
            Assert.AreEqual(2, message.Buffer[9]);
            // GPIO value
            Assert.AreEqual(1, message.Buffer[10]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc((byte)2));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[11], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[12], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[13], "Buffer[13] should end with 0x03.");
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    (byte)1,
                    (byte)1
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 4;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(14, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(2), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.WriteGpio, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)4, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // GPIO number
            Assert.AreEqual(1, message.Buffer[9]);
            // GPIO value
            Assert.AreEqual(1, message.Buffer[10]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(2));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[11], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[12], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[13], "Buffer[13] should end with 0x03.");
        }

        [TestMethod]
        public void TestWriteGpioOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.WriteGpio
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 4;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(0), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.WriteGpio, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)4, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(0));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)1
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 5;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _binMsg.Buffer.Length, "Total packet length should be 13");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(1), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadTemperature, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)5, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual(1, message.Buffer[9]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(1));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)1,
                    (double)12.3
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 5;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(21, _binMsg.Buffer.Length, "Total packet length should be 21");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value((byte)9), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadTemperature, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)5, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual(1, message.Buffer[9]);
            // Sensor value
            double temperature = BitConverter.ToDouble(message.Buffer, 10);
            Assert.AreEqual(12.3, temperature);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc((byte)9));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[18], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[19], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[20], "Buffer[20] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHumidityCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)2
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 6;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _binMsg.Buffer.Length, "Total packet length should be 13");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(1), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadHumidity, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)6, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual(2, message.Buffer[9]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(1));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadHumidityOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)2,
                    (double)23.4
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 6;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(21, _binMsg.Buffer.Length, "Total packet length should be 21");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value((byte)9), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadHumidity, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)6, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual(2, message.Buffer[9]);
            // Sensor value
            double humidity = BitConverter.ToDouble(message.Buffer, 10);
            Assert.AreEqual(23.4, humidity);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc((byte)9));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[18], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[19], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[20], "Buffer[20] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureHumidityCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    (byte)3
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 7;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _binMsg.Buffer.Length, "Total packet length should be 13");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(1), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadTempAndHum, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)7, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual(3, message.Buffer[9]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(1));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestReadTemperatureHumidityOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    (byte)3,
                    (double)23.4,
                    (double)56.7
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 7;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(29, _binMsg.Buffer.Length, "Total packet length should be 29");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value((byte)17), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.ReadTempAndHum, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)7, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Sensor number
            Assert.AreEqual(3, message.Buffer[9]);
            // Temperature value
            double temperature = BitConverter.ToDouble(message.Buffer, 10);
            Assert.AreEqual(23.4, temperature);
            // Humidity value
            double humidity = BitConverter.ToDouble(message.Buffer, 18);
            Assert.AreEqual(56.7, humidity);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc((byte)17));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[26], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[27], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[28], "Buffer[28] should end with 0x03.");
        }

        [TestMethod]
        public void TestBootModeCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.BootMode,
                Arguments = new List<object>()
                {
                    (byte)1
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(13, _binMsg.Buffer.Length, "Total packet length should be 13");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(1), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.BootMode, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // Bootmode
            Assert.AreEqual(1, message.Buffer[9]);

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(1));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[10], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[11], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[12], "Buffer[12] should end with 0x03.");
        }

        [TestMethod]
        public void TestBootModeOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.BootMode
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(0), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.BootMode, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(0));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestResetCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.Reset
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)6, message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.Reset, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Command, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)1, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)2, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16 : Device Id(Buffer[1]) ~ BitField2(Buffer[8])
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(0));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[11], "Buffer[11] should end with 0x03.");
        }

        [TestMethod]
        public void TestResetOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.Reset
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;
            _binMsg.Header.flag2.error = FlConstant.FL_OK;

            message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            Assert.IsNotNull(_binMsg.Buffer, "Buffer property should not be null.");
            Assert.AreEqual(12, _binMsg.Buffer.Length, "Total packet length should be 12");

            // STX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_STX, message.Buffer[0], "Buffer[0] should start with 0x02.");

            // Device Id
            Assert.AreEqual((byte)1, message.Buffer[1], "Buffer[1] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[2], "Buffer[2] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[3], "Buffer[3] should be 0.");
            Assert.AreEqual((byte)0, message.Buffer[4], "Buffer[4] should be 1.");

            // Length
            Assert.AreEqual((byte)calculate_length_field_value(0), message.Buffer[5], $"Buffer[{FlConstant.HeaderLengthFieldIndex}] should be 6.");

            // Message Id
            Assert.AreEqual((byte)FlMessageId.Reset, message.Buffer[6], $"Buffer[{FlConstant.HeaderMessageIdFieldIndex}] should be 1.");

            // Flag1
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)FlMessageCategory.Response, FlConstant.FL_BIN_FLAG1_MSG_TYPE_MASK, FlConstant.FL_BIN_FLAG1_MSG_TYPE_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_MASK, FlConstant.FL_BIN_FLAG1_RETURN_EXPECTED_POS);
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)2, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_MASK, FlConstant.FL_BIN_FLAG1_SEQUENCE_NUM_POS);
            Assert.AreEqual(flagExpected, message.Buffer[7], $"Buffer[{FlConstant.HeaderFlag1FieldIndex}] and Bit field1 should be matched.");

            // Flag2
            flagExpected = 0;
            flagExpected = FlUtil.BitFieldSet(flagExpected, (byte)0, FlConstant.FL_BIN_FLAG2_ERROR_MASK, FlConstant.FL_BIN_FLAG2_ERROR_POS);
            Assert.AreEqual(flagExpected, message.Buffer[8], $"Buffer[{FlConstant.HeaderFlag2FieldIndex}] and Bit field2 should be matched.");

            // CRC16
            _crc16 = FlUtil.CRC16(message.Buffer, 1, data_length_for_crc(0));
            Assert.AreEqual((byte)(_crc16 & 0xff), message.Buffer[9], "CRC16[0] should be matched.");
            Assert.AreEqual((byte)((_crc16 >> 8) & 0xff), message.Buffer[10], "CRC16[1] should be matched.");

            // ETX
            Assert.AreEqual(FlConstant.FL_BIN_MSG_ETX, message.Buffer[11], "Buffer[11] should end with 0x03.");
        }
    }
}
