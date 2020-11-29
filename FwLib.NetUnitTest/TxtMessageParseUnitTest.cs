using FwLib.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace FwLib.NetUnitTest
{
    [TestClass]
    public class TxtMessageParseUnitTest
    {
        FwLibTxtParser _parser = new FwLibTxtParser();
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
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion,
                DeviceId = 1
            };
            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageCommand parsedCommand = (FwLibTxtMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Message category should be command.");
            Assert.AreEqual(FwLibMessageId.ReadHardwareVersion, parsedCommand.MessageId, "Message Id should be ReadHardwareVersion.");
            Assert.AreEqual((uint)1, parsedCommand.DeviceId, "Device ID should 1.");
            Assert.IsNull(parsedCommand.Arguments, "Message arguments should be null.");
        }

        [TestMethod]
        public void TestReadHardwareVersionResponseMessageParse()
        {
            _message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion,
                DeviceId = 1,
                Arguments = new List<object>()
                {
                    (byte)0,    // Return code
                    "1.2.3" // Version string
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageResponse parsedCommand = (FwLibTxtMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedCommand.MessageCategory, "Message category should be response.");
            Assert.AreEqual(FwLibMessageId.ReadHardwareVersion, parsedCommand.MessageId, "Message Id should be ReadHardwareVersion.");
            Assert.AreEqual((uint)1, parsedCommand.DeviceId, "Device ID should 1.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(2, parsedCommand.Arguments.Count, "The number of message arguments should be 2.");
            Assert.AreEqual((byte)0, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 0.");
            Assert.AreEqual("1.2.3", (string)parsedCommand.Arguments[1], "Arguments[1] should be 1.2.3.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionCommandMessageParse()
        {
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion,
                DeviceId = 2
            };
            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageCommand parsedCommand = (FwLibTxtMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Message category should be command.");
            Assert.AreEqual(FwLibMessageId.ReadFirmwareVersion, parsedCommand.MessageId, "Message Id should be ReadFirmwareVersion.");
            Assert.AreEqual((uint)2, parsedCommand.DeviceId, "Device ID should 2.");
            Assert.IsNull(parsedCommand.Arguments, "Message arguments should be null.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionResponseMessageParse()
        {
            _message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion,
                DeviceId = 2,
                Arguments = new List<object>()
                {
                    (byte)0,    // Return code
                    "2.3.4"     // Version string
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageResponse parsedCommand = (FwLibTxtMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedCommand.MessageCategory, "Message category should be response.");
            Assert.AreEqual(FwLibMessageId.ReadFirmwareVersion, parsedCommand.MessageId, "Message Id should be ReadFirmwareVersion.");
            Assert.AreEqual((uint)2, parsedCommand.DeviceId, "Device ID should 2.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(2, parsedCommand.Arguments.Count, "The number of message arguments should be 2.");
            Assert.AreEqual((byte)0, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 0.");
            Assert.AreEqual("2.3.4", (string)parsedCommand.Arguments[1], "Arguments[1] should be 2.3.4.");
        }

        [TestMethod]
        public void TestReadGpioCommandMessageParse()
        {
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadGpio,
                DeviceId = 3,
                Arguments = new List<object>()
                {
                    (byte)2     // GPIO number
                }
            };
            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageCommand parsedCommand = (FwLibTxtMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Message category should be command.");
            Assert.AreEqual(FwLibMessageId.ReadGpio, parsedCommand.MessageId, "Message Id should be ReadGpio.");
            Assert.AreEqual((uint)3, parsedCommand.DeviceId, "Device ID should 3.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "The number of message arguments should be 1.");
            Assert.AreEqual((byte)2, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 2.");
        }

        [TestMethod]
        public void TestReadGpioResponsedMessageParse()
        {
            _message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadGpio,
                DeviceId = 3,
                Arguments = new List<object>()
                {
                    (byte)0,    // Return code
                    (byte)2,    // GPIO number
                    (byte)1     // GPIO value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageResponse parsedCommand = (FwLibTxtMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedCommand.MessageCategory, "Message category should be response.");
            Assert.AreEqual(FwLibMessageId.ReadGpio, parsedCommand.MessageId, "Message Id should be ReadGpio.");
            Assert.AreEqual((uint)3, parsedCommand.DeviceId, "Device ID should 3.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(3, parsedCommand.Arguments.Count, "The number of message arguments should be 3.");
            Assert.AreEqual((byte)0, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 0.");
            Assert.AreEqual((byte)2, (byte)parsedCommand.Arguments[1], "Arguments[1] should be 2.");
            Assert.AreEqual((byte)1, (byte)parsedCommand.Arguments[2], "Arguments[2] should be 2.");
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageParse()
        {
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.WriteGpio,
                DeviceId = 4,
                Arguments = new List<object>()
                {
                    (byte)3,    // GPIO number
                    (byte)1     // GPIO value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageCommand parsedCommand = (FwLibTxtMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Message category should be command.");
            Assert.AreEqual(FwLibMessageId.WriteGpio, parsedCommand.MessageId, "Message Id should be WriteGpio.");
            Assert.AreEqual((uint)4, parsedCommand.DeviceId, "Device ID should 4.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(2, parsedCommand.Arguments.Count, "The number of message arguments should be 2.");
            Assert.AreEqual((byte)3, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 3.");
            Assert.AreEqual((byte)1, (byte)parsedCommand.Arguments[1], "Arguments[1] should be 1.");
        }

        [TestMethod]
        public void TestWriteGpioResponsedMessageParse()
        {
            _message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.WriteGpio,
                DeviceId = 4,
                Arguments = new List<object>()
                {
                    (byte)0     // Return code
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageResponse parsedCommand = (FwLibTxtMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedCommand.MessageCategory, "Message category should be response.");
            Assert.AreEqual(FwLibMessageId.WriteGpio, parsedCommand.MessageId, "Message Id should be WriteGpio.");
            Assert.AreEqual((uint)4, parsedCommand.DeviceId, "Device ID should 4.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "The number of message arguments should be 1.");
            Assert.AreEqual((byte)0, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 0.");
        }

        [TestMethod]
        public void TestButtonPressedEventMessageParse()
        {
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ButtonEvent,
                DeviceId = 5,
                Arguments = new List<object>()
                {
                    (byte)4,    // Button number
                    (byte)1     // Button value(pressed)
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageEvent parsedCommand = (FwLibTxtMessageEvent)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Event, parsedCommand.MessageCategory, "Message category should be event.");
            Assert.AreEqual(FwLibMessageId.ButtonEvent, parsedCommand.MessageId, "Message Id should be ButtonEvent.");
            Assert.AreEqual((uint)5, parsedCommand.DeviceId, "Device ID should 5.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(2, parsedCommand.Arguments.Count, "The number of message arguments should be 2.");
            Assert.AreEqual((byte)4, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 4.");
            Assert.AreEqual((byte)1, (byte)parsedCommand.Arguments[1], "Arguments[1] should be 1.");
        }

        [TestMethod]
        public void TestReadTemperatureCommandMessageParse()
        {
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadTemperature,
                DeviceId = 6,
                Arguments = new List<object>()
                {
                    (byte)7     // Sensor number
                }
            };
            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageCommand parsedCommand = (FwLibTxtMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Message category should be command.");
            Assert.AreEqual(FwLibMessageId.ReadTemperature, parsedCommand.MessageId, "Message Id should be ReadTemperature.");
            Assert.AreEqual((uint)6, parsedCommand.DeviceId, "Device ID should 6.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "The number of message arguments should be 1.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 7.");
        }

        [TestMethod]
        public void TestReadTemperatureOkResponsedMessageParse()
        {
            _message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadTemperature,
                DeviceId = 6,
                Arguments = new List<object>()
                {
                    (byte)0,    // Return code
                    (byte)7,    // Sensor number
                    (double)12.3     // Temperature value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageResponse parsedCommand = (FwLibTxtMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedCommand.MessageCategory, "Message category should be response.");
            Assert.AreEqual(FwLibMessageId.ReadTemperature, parsedCommand.MessageId, "Message Id should be ReadTemperature.");
            Assert.AreEqual((uint)6, parsedCommand.DeviceId, "Device ID should 3.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(3, parsedCommand.Arguments.Count, "The number of message arguments should be 3.");
            Assert.AreEqual((byte)0, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 0.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[1], "Arguments[1] should be 7.");
            Assert.AreEqual((double)12.3, (double)parsedCommand.Arguments[2], "Arguments[2] should be 12.3.");
        }

        [TestMethod]
        public void TestReadHumidityCommandMessageParse()
        {
            _message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadHumidity,
                DeviceId = 6,
                Arguments = new List<object>()
                {
                    "7"     // Sensor number
                }
            };
            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageCommand parsedCommand = (FwLibTxtMessageCommand)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Command, parsedCommand.MessageCategory, "Message category should be command.");
            Assert.AreEqual(FwLibMessageId.ReadHumidity, parsedCommand.MessageId, "Message Id should be ReadHumidity.");
            Assert.AreEqual((uint)6, parsedCommand.DeviceId, "Device ID should 6.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(1, parsedCommand.Arguments.Count, "The number of message arguments should be 1.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 7.");
        }

        [TestMethod]
        public void TestReadHumidityOkResponsedMessageParse()
        {
            _message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHumidity,
                DeviceId = 6,
                Arguments = new List<object>()
                {
                    "0",    // Return code
                    "7",    // Sensor number
                    "23.4"  // Humidity value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (int i = 0; i < _message.Buffer.Length; i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[i], out _parsedMessage);
            }

            FwLibTxtMessageResponse parsedCommand = (FwLibTxtMessageResponse)_parsedMessage;

            Assert.AreEqual(FwLibParseState.ParseOk, _parseResult, "Parse result should be ParseOk.");
            Assert.AreEqual(FwLibMessageType.Text, parsedCommand.MessageType, "Message type field should be text.");
            Assert.AreEqual(FwLibMessageCategory.Response, parsedCommand.MessageCategory, "Message category should be response.");
            Assert.AreEqual(FwLibMessageId.ReadHumidity, parsedCommand.MessageId, "Message Id should be ReadHumidity.");
            Assert.AreEqual((uint)6, parsedCommand.DeviceId, "Device ID should 3.");
            Assert.IsNotNull(parsedCommand.Arguments, "Message arguments should not be null.");
            Assert.AreEqual(3, parsedCommand.Arguments.Count, "The number of message arguments should be 3.");
            Assert.AreEqual((byte)0, (byte)parsedCommand.Arguments[0], "Arguments[0] should be 0.");
            Assert.AreEqual((byte)7, (byte)parsedCommand.Arguments[1], "Arguments[1] should be 7.");
            Assert.AreEqual((double)23.4, (double)parsedCommand.Arguments[2], "Arguments[2] should be 23.4.");
        }
    }
}
