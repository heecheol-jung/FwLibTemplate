using Fl.Net;
using Fl.Net.Message;
using Fl.Net.Parser;
using Fl.Net.PInvoke;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Fl.NetUnitTest
{
    [TestClass]
    public class FlBinMessageParserUnitTest
    {
        FlBinParser _parser = new FlBinParser();
        int _i = 0;
        IFlBinMessage _binMsg = null;
        IFlMessage _message = null;
        IFlMessage _parsedMessage = null;
        IFlBinMessage _parsedBinMsg = null;
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
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadHardwareVersion
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)1, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadHardwareVersion, _parsedBinMsg.MessageId);
        }

        [TestMethod]
        public void TestReadHardwareVersionResponseParse()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadHardwareVersion,
                Arguments = new List<object>()
                {
                    "a.1.2.3" // Hardware version
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)1, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadHardwareVersion, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual("a.1.2.3", (string)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestReadFirmwareVersionCommandParse()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadFirmwareVersion
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)2, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadFirmwareVersion, _parsedBinMsg.MessageId);
        }

        [TestMethod]
        public void TestReadFirmwareVersionOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    "a.2.3.4" // Firmware version
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)2, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadFirmwareVersion, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual("a.2.3.4", (string)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestReadGpioCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)2  // GPIO number
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 3;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)3, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadGpio, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(2, (byte)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestReadGpioOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)2,    // GPIO number
                    (byte)1     // GPIO value
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 3;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)3, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadGpio, _parsedBinMsg.MessageId);

            Assert.AreEqual(2, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(2, (byte)_parsedBinMsg.Arguments[0]);
            Assert.AreEqual(1, (byte)_parsedBinMsg.Arguments[1]);
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    (byte)1,    // GPIO number
                    (byte)1     // GPIO value
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 4;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)4, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.WriteGpio, _parsedBinMsg.MessageId);

            Assert.AreEqual(2, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(1, (byte)_parsedBinMsg.Arguments[0]);
            Assert.AreEqual(1, (byte)_parsedBinMsg.Arguments[1]);
        }

        [TestMethod]
        public void TestWriteGpioOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.WriteGpio,
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 4;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)4, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.WriteGpio, _parsedBinMsg.MessageId);
            Assert.AreEqual((byte)0, _parsedBinMsg.Header.flag2.error);
        }

        [TestMethod]
        public void TestReadTemperatureCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)1    // Sensor number
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 5;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)5, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadTemperature, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(1, (byte)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestReadTemperatureOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)1,        // Sensor number
                    (double)12.3    // Temperature
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 5;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)5, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadTemperature, _parsedBinMsg.MessageId);

            Assert.AreEqual(2, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(1, (byte)_parsedBinMsg.Arguments[0]);
            Assert.AreEqual(12.3, (double)_parsedBinMsg.Arguments[1]);
        }

        [TestMethod]
        public void TestReadHumidityCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)2    // Sensor number
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 6;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)6, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadHumidity, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(2, (byte)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestReadHumidityOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)2,        // Sensor number
                    (double)23.4    // Humidity
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 6;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)6, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadHumidity, _parsedBinMsg.MessageId);

            Assert.AreEqual(2, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(2, (byte)_parsedBinMsg.Arguments[0]);
            Assert.AreEqual(23.4, (double)_parsedBinMsg.Arguments[1]);
        }

        [TestMethod]
        public void TestReadTemperatureHumidityCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    (byte)3    // Sensor number
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 7;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)7, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadTempAndHum, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(3, (byte)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestReadTemperatureHumidityOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    (byte)3,        // Sensor number
                    (double)12.3,   // Temperature
                    (double)23.4    // Humidity
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 7;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)7, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.ReadTempAndHum, _parsedBinMsg.MessageId);

            Assert.AreEqual(3, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(3, (byte)_parsedBinMsg.Arguments[0]);
            Assert.AreEqual(12.3, (double)_parsedBinMsg.Arguments[1]);
            Assert.AreEqual(23.4, (double)_parsedBinMsg.Arguments[2]);
        }

        [TestMethod]
        public void TestBootModeCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.BootMode,
                Arguments = new List<object>()
                {
                    (byte)1    // Boot mode(Bootloader)
                }
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)1, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.BootMode, _parsedBinMsg.MessageId);

            Assert.AreEqual(1, _parsedBinMsg.Arguments.Count);
            Assert.AreEqual(1, (byte)_parsedBinMsg.Arguments[0]);
        }

        [TestMethod]
        public void TestBootModeOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.BootMode,
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 1;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)1, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.BootMode, _parsedBinMsg.MessageId);
            Assert.AreEqual((byte)0, _parsedBinMsg.Header.flag2.error);
        }

        [TestMethod]
        public unsafe void TestResetCommandMessageBuild()
        {
            _binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.Reset
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Device;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Command, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)2, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.Reset, _parsedBinMsg.MessageId);
        }

        [TestMethod]
        public unsafe void TestResetOkResponseMessageBuild()
        {
            _binMsg = new FlBinMessageResponse()
            {
                MessageId = FlMessageId.Reset,
            };
            _binMsg.Header.device_id = 1;
            _binMsg.Header.flag1.sequence_num = 2;

            _message = _binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref _message);

            _parser.Role = FlParserRole.Host;
            for (_i = 0; _i < _message.Buffer.Length; _i++)
            {
                _parseResult = _parser.Parse(_message.Buffer[_i], out _parsedMessage);
            }

            _parsedBinMsg = (IFlBinMessage)_parsedMessage;
            Assert.AreEqual(FlParseState.ParseOk, _parseResult);
            Assert.AreEqual(FlMessageType.Binary, _parsedBinMsg.MessageType);
            Assert.AreEqual(FlMessageCategory.Response, _parsedBinMsg.MessageCategory);
            Assert.AreEqual((uint)1, _parsedBinMsg.Header.device_id);
            Assert.AreEqual((byte)2, _parsedBinMsg.Header.flag1.sequence_num);
            Assert.AreEqual(FlMessageId.Reset, _parsedBinMsg.MessageId);
            Assert.AreEqual((byte)0, _parsedBinMsg.Header.flag2.error);
        }
    }
}
