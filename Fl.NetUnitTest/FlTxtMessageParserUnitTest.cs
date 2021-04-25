using Fl.Net;
using Fl.Net.Message;
using Fl.Net.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Fl.NetUnitTest
{
    [TestClass]
    public class FlTxtMessageParserUnitTest
    {
        FlTxtParser _parser = new FlTxtParser();
        int _i = 0;
        IFlMessage _message = null;
        IFlMessage _parsedMessage = null;
        FlParseState _parseResult = FlParseState.Parsing;

        [TestInitialize]
        public void InitializeBeforeEveryTest()
        {
            _parseResult = FlParseState.Parsing;
            _parsedMessage = null;
            _parser.Clear();
        }

        [TestMethod]
        public void TestReadHardwareVersionCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadHardwareVersion,
                Arguments = new List<object>()
                {
                    "1" // DeviceID
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadHardwareVersion, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(1, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
        }

        [TestMethod]
        public void TestReadHardwareVersionResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadHardwareVersion,
                Arguments = new List<object>()
                {
                    1, // DeviceID(string or uint32),
                    0, // Error(string or uint8)
                    "a.1.2.3" // Hardware version
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadHardwareVersion, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(3, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("a.1.2.3", (string)_parsedMessage.Arguments[2]);
        }

        [TestMethod]
        public void TestReadFirmwareVersionCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    "1" // DeviceID
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadFirmwareVersion, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(1, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
        }

        [TestMethod]
        public void TestReadFirmwareVersionResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    1, // DeviceID(string or uint32),
                    0, // Error(string or uint8)
                    "a.2.3.4" // Hardware version
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadFirmwareVersion, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(3, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("a.2.3.4", (string)_parsedMessage.Arguments[2]);
        }

        [TestMethod]
        public void TestReadGpioCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "2"  // GPIO number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadGpio, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("2", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestReadGpioResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    "1", // DeviceID,
                    "0", // Error
                    "2", // GPIO number
                    "1"  // GPIO value
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadGpio, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(4, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("2", (string)_parsedMessage.Arguments[2]);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[3]);
        }

        [TestMethod]
        public void TestWriteGpioCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "3", // GPIO number
                    "1"  // GPIO value
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.WriteGpio, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(3, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("3", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[2]);
        }

        [TestMethod]
        public void TestWriteGpioResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    "1", // DeviceID,
                    "0"  // Error
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.WriteGpio, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestReadTemperatureCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1"  // Sensor number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadTemperature, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestReadTemperatureResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    1, // DeviceID,
                    0,   // Error
                    1,   // Sensor number
                    $"{12.3:0.00}" // Temperature value
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadTemperature, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(4, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[2]);
            Assert.AreEqual("12.30", (string)_parsedMessage.Arguments[3]);
        }

        [TestMethod]
        public void TestReadHumidityCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "2"  // Sensor number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadHumidity, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("2", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestReadHumidityResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    1, // DeviceID,
                    0,   // Error
                    2,   // Sensor number
                    $"{23.4:0.00}" // Humidity value
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadHumidity, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(4, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("2", (string)_parsedMessage.Arguments[2]);
            Assert.AreEqual("23.40", (string)_parsedMessage.Arguments[3]);
        }

        [TestMethod]
        public void TestReadTempemperatureAndHumidityCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "3"  // Sensor number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadTempAndHum, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("3", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestReadTempemperatureAndHumidityResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    1, // DeviceID,
                    0,   // Error
                    3,   // Sensor number
                    $"{23.4:0.00}", // Temperature value
                    $"{56.7:0.00}" // Humidity value
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.ReadTempAndHum, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(5, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
            Assert.AreEqual("3", (string)_parsedMessage.Arguments[2]);
            Assert.AreEqual("23.40", (string)_parsedMessage.Arguments[3]);
            Assert.AreEqual("56.70", (string)_parsedMessage.Arguments[4]);
        }

        [TestMethod]
        public void TestBootModeCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.BootMode,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1"  // Bootloader
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.BootMode, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestBootModeResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.BootMode,
                Arguments = new List<object>()
                {
                    1, // DeviceID,
                    0   // Error
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.BootMode, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
        }

        [TestMethod]
        public void TestResetCommandParse()
        {
            _message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.Reset,
                Arguments = new List<object>()
                {
                    "1" // DeviceID
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseCommand(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.Reset, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(1, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
        }

        [TestMethod]
        public void TestResetResponseParse()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.Reset,
                Arguments = new List<object>()
                {
                    1, // DeviceID,
                    0   // Error
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.ParseResponseEvent(_message.Buffer[_i], out _parsedMessage);
            }

            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Text, _parsedMessage.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedMessage.MessageCategory);
            Assert.AreEqual(FlMessageId.Reset, _parsedMessage.MessageId);
            Assert.IsNotNull(_parsedMessage.Arguments);
            Assert.AreEqual(2, _parsedMessage.Arguments.Count);
            Assert.AreEqual("1", (string)_parsedMessage.Arguments[0]);
            Assert.AreEqual("0", (string)_parsedMessage.Arguments[1]);
        }
    }
}
