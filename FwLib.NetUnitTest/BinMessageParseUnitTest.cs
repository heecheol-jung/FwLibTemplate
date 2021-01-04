using FwLib.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FwLib.NetUnitTest
{
    [TestClass]
    public class BinMessageParseUnitTest
    {
        FwLibBinParser _parser = new FwLibBinParser();
        IFwLibMessage _parsedMessage;
        FwLibParseState _parseResult;
        IFwLibMessage _message;

        [TestInitialize]
        public void InitializeBeforeEveryTest()
        {
            _parseResult = FwLibParseState.Parsing;
            _parsedMessage = null;

            _parser.Clear();
        }

        [TestMethod]
        public void TestReadHardwareVersionCommandMessageParse()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 1;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedCommand.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedCommand.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadHardwareVersion, parsedCommand.MessageId, "Header Message ID field should be ReadHardwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)1, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 1.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedCommand.Arguments, "Arguments property should be null.");
        }

        [TestMethod]
        public void TestReadHardwareVersionOkResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion,
                Arguments = new System.Collections.Generic.List<object>()
                {
                    (byte)1,
                    (byte)2,
                    (byte)3
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 1;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)9, parsedResponse.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadHardwareVersion, parsedResponse.MessageId, "Header Message ID field should be ReadHardwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)1, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedResponse.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(3, parsedResponse.Arguments.Count, "Argument count should be 3.");
            Assert.AreEqual((byte)1, (byte)parsedResponse.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((byte)2, (byte)parsedResponse.Arguments[1], "Argument1 should be 2.");
            Assert.AreEqual((byte)3, (byte)parsedResponse.Arguments[2], "Argument1 should be 3.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadHardwareVersionErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 1;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadHardwareVersion, parsedResponse.MessageId, "Header Message ID field should be ReadHardwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)1, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionCommandMessageParse()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 2;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedCommand.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedCommand.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadFirmwareVersion, parsedCommand.MessageId, "Header Message ID field should be ReadFirmwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)2, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 2.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedCommand.Arguments, "Arguments property should be null.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionOkResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion,
                Arguments = new System.Collections.Generic.List<object>()
                {
                    (byte)2,
                    (byte)3,
                    (byte)4
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 2;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)9, parsedResponse.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadFirmwareVersion, parsedResponse.MessageId, "Header Message ID field should be ReadFirmwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)2, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 2.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedResponse.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(3, parsedResponse.Arguments.Count, "Argument count should be 3.");
            Assert.AreEqual((byte)2, (byte)parsedResponse.Arguments[0], "Argument0 should be 2.");
            Assert.AreEqual((byte)3, (byte)parsedResponse.Arguments[1], "Argument1 should be 3.");
            Assert.AreEqual((byte)4, (byte)parsedResponse.Arguments[2], "Argument1 should be 4.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 2;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadFirmwareVersion, parsedResponse.MessageId, "Header Message ID field should be ReadFirmwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)2, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 2.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadGpioCommandMessageParse()
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

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedCommand.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)7, parsedCommand.Header.Length, "Header Length field should be 7.");
            Assert.AreEqual(FwLibMessageId.ReadGpio, parsedCommand.MessageId, "Header Message ID field should be ReadGpio.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)3, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 3.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedCommand.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "Argument count should be 1.");
            Assert.AreEqual((byte)1, (byte)parsedCommand.Arguments[0], "Argument0 should be 1.");
        }

        [TestMethod]
        public void TestReadGpioOkResponseMessageParse()
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
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)8, parsedResponse.Header.Length, "Header Length field should be 8.");
            Assert.AreEqual(FwLibMessageId.ReadGpio, parsedResponse.MessageId, "Header Message ID field should be ReadGpio.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)3, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 3.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedResponse.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, parsedResponse.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)parsedResponse.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((byte)1, (byte)parsedResponse.Arguments[1], "Argument1 should be 1.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadGpioErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadGpio
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 3;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadGpio, parsedResponse.MessageId, "Header Message ID field should be ReadGpio.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)3, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 3.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageParse()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.WriteGpio,
                Arguments = new System.Collections.Generic.List<object>()
                {
                    (byte)1,    // GPIO number,
                    (byte)1     // GPIO value
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 4;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedCommand.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)8, parsedCommand.Header.Length, "Header Length field should be 8.");
            Assert.AreEqual(FwLibMessageId.WriteGpio, parsedCommand.MessageId, "Header Message ID field should be WriteGpio.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)4, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 4.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedCommand.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, parsedCommand.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)parsedCommand.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((byte)1, (byte)parsedCommand.Arguments[1], "Argument0 should be 1.");
        }

        [TestMethod]
        public void TestWriteGpioOkResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.WriteGpio
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 4;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.WriteGpio, parsedResponse.MessageId, "Header Message ID field should be WriteGpio.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)4, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 4.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestWriteGpioErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.WriteGpio
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 1;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 4;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedResponse.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.WriteGpio, parsedResponse.MessageId, "Header Message ID field should be WriteGpio.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)4, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 4.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestButtonPressedEventMessageParse()
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

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageEvent parsedEvent = (FwLibBinMessageEvent)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)1, parsedEvent.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)8, parsedEvent.Header.Length, "Header Length field should be 8.");
            Assert.AreEqual(FwLibMessageId.ButtonEvent, parsedEvent.MessageId, "Header Message ID field should be ButtonEvent.");
            Assert.AreEqual(FwLibMessageCategory.Event, parsedEvent.MessageCategory, "Header Message type field should be Event.");
            Assert.AreEqual(false, parsedEvent.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)0, parsedEvent.Header.SequenceNumber, "Header Sequence number field should be 0.");
            Assert.AreEqual((byte)0, parsedEvent.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedEvent.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedEvent.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedEvent.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, parsedEvent.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)parsedEvent.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((byte)1, (byte)parsedEvent.Arguments[1], "Argument0 should be 1.");
            Assert.AreNotEqual(DateTime.MinValue, parsedEvent.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadTemperatureCommandMessageParse()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)7 // Sensor number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedCommand.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)7, parsedCommand.Header.Length, "Header Length field should be 7.");
            Assert.AreEqual(FwLibMessageId.ReadTemperature, parsedCommand.MessageId, "Header Message ID field should be ReadTemperature.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)5, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedCommand.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "Argument count should be 1.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[0], "Argument0 should be 7.");
        }

        [TestMethod]
        public void TestReadTemperatureOkResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)1,    // Sensor number
                    (UInt16)123 // Sensor value
                }
            };
            if (BitConverter.IsLittleEndian == true)
            {
                _message.Arguments[1] = FwLibUtil.SwapUInt16((UInt16)_message.Arguments[1]);
            }
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedResponse.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)9, parsedResponse.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadTemperature, parsedResponse.MessageId, "Header Message ID field should be ReadTemperature.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)5, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedResponse.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, parsedResponse.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)parsedResponse.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((UInt16)123, (UInt16)parsedResponse.Arguments[1], "Argument0 should be 123.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadTemperatureErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperature
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedResponse.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadTemperature, parsedResponse.MessageId, "Header Message ID field should be ReadTemperature.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)5, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadHumidityCommandMessageParse()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)7 // Sensor number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedCommand.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)7, parsedCommand.Header.Length, "Header Length field should be 7.");
            Assert.AreEqual(FwLibMessageId.ReadHumidity, parsedCommand.MessageId, "Header Message ID field should be ReadHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)5, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedCommand.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "Argument count should be 1.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[0], "Argument0 should be 7.");
        }

        [TestMethod]
        public void TestReadHumidityOkResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)1,    // Sensor number
                    (UInt16)234 // Sensor value
                }
            };

            if (BitConverter.IsLittleEndian == true)
            {
                _message.Arguments[1] = FwLibUtil.SwapUInt16((UInt16)_message.Arguments[1]);
            }
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedResponse.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)9, parsedResponse.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadHumidity, parsedResponse.MessageId, "Header Message ID field should be ReadHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)5, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedResponse.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, parsedResponse.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)parsedResponse.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((UInt16)234, (UInt16)parsedResponse.Arguments[1], "Argument0 should be 234.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadHumidityErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHumidity
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedResponse.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadHumidity, parsedResponse.MessageId, "Header Message ID field should be ReadHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)5, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadTemperatureAndHumidityCommandMessageParse()
        {
            _message = new FwLibBinMessageCommand()
            {
                MessageId = FwLibMessageId.ReadTemperatureAndHumidity,
                Arguments = new List<object>()
                {
                    (byte)7 // Sensor number
                }
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Device;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageCommand parsedCommand = (FwLibBinMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Device, _parser.Role, "Parse role should be Device.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedCommand.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)7, parsedCommand.Header.Length, "Header Length field should be 7.");
            Assert.AreEqual(FwLibMessageId.ReadTemperatureAndHumidity, parsedCommand.MessageId, "Header Message ID field should be ReadTemperatureAndHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Header Message type field should be Command.");
            Assert.AreEqual(true, parsedCommand.Header.ReturnExpected, "Header ReturnExpected field should be true.");
            Assert.AreEqual((byte)5, parsedCommand.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedCommand.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedCommand.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "Argument count should be 1.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[0], "Argument0 should be 7.");
        }

        [TestMethod]
        public void TestReadTemperatureAndHumidityOkResponseMessageParse()
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

            if (BitConverter.IsLittleEndian == true)
            {
                _message.Arguments[1] = FwLibUtil.SwapUInt16((UInt16)_message.Arguments[1]);
                _message.Arguments[2] = FwLibUtil.SwapUInt16((UInt16)_message.Arguments[2]);
            }
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.OK;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedResponse.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)11, parsedResponse.Header.Length, "Header Length field should be 11.");
            Assert.AreEqual(FwLibMessageId.ReadTemperatureAndHumidity, parsedResponse.MessageId, "Header Message ID field should be ReadTemperatureAndHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)5, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(parsedResponse.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(3, parsedResponse.Arguments.Count, "Argument count should be 3.");
            Assert.AreEqual((byte)7, (byte)parsedResponse.Arguments[0], "Argument0 should be 7.");
            Assert.AreEqual((UInt16)123, (UInt16)parsedResponse.Arguments[1], "Argument0 should be 123.");
            Assert.AreEqual((UInt16)234, (UInt16)parsedResponse.Arguments[2], "Argument0 should be 234.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadTemperatureHumidityErrorResponseMessageParse()
        {
            _message = new FwLibBinMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperatureAndHumidity
            };
            ((IFwLibBinMessage)_message).Header.DeviceId = 6;
            ((IFwLibBinMessage)_message).Header.SequenceNumber = 5;
            ((IFwLibBinMessage)_message).Header.Error = FwLibConstant.ERROR;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FwLibParserRole.Host;
            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[i], out _parsedMessage);
            }

            FwLibBinMessageResponse parsedResponse = (FwLibBinMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParserRole.Host, _parser.Role, "Parse role should be Host.");
            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual((byte)6, parsedResponse.Header.DeviceId, "Header Device Id field should be 6.");
            Assert.AreEqual((byte)6, parsedResponse.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.ReadTemperatureAndHumidity, parsedResponse.MessageId, "Header Message ID field should be ReadTemperatureAndHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedResponse.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, parsedResponse.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual((byte)5, parsedResponse.Header.SequenceNumber, "Header Sequence number field should be 5.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual(FwLibConstant.ERROR, parsedResponse.Header.Error, "Header Error field should be 1.");
            Assert.AreEqual((byte)0, parsedResponse.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(parsedResponse.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, parsedResponse.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }
    }
}
