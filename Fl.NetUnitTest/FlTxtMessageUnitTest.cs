using Fl.Net;
using Fl.Net.Message;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace Fl.NetUnitTest
{
    [TestClass]
    public class FlTxtMessageUnitTest
    {
        IFlMessage _message;
        string commandString;
        byte[] expectedPacket;

        [TestInitialize]
        public void InitializeBeforeEveryTest()
        {
            _message = null;
        }

        [TestMethod]
        public void TestReadHardwareVersionCommandMessageBuild()
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

            commandString = "RHVER 1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestHardwareVersionResponseMessageBuild()
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

            commandString = "RHVER 1,0,a.1.2.3\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestFirmwareVersionCommandMessageBuild()
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

            commandString = "RFVER 1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestFirmwareVersionResponseMessageBuild()
        {
            _message = new FlTxtMessageResponse()
            {
                MessageId = FlMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    "1", // DeviceID,
                    "0", // Error
                    "a.2.3.4" // Hardware version
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref _message);

            commandString = "RFVER 1,0,a.2.3.4\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadGpioCommandMessageBuild()
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

            commandString = "RGPIO 1,2\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadGpioResponseMessageBuild()
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

            commandString = "RGPIO 1,0,2,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageBuild()
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

            commandString = "WGPIO 1,3,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestWriteGpioResponseMessageBuild()
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

            commandString = "WGPIO 1,0\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadTemperatureCommandMessageBuild()
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

            commandString = "RTEMP 1,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadTemperatureResponseMessageBuild()
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

            commandString = "RTEMP 1,0,1,12.30\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadHumidityCommandMessageBuild()
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

            commandString = "RHUM 1,2\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadHumidityResponseMessageBuild()
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

            commandString = "RHUM 1,0,2,23.40\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadTempemperatureAndHumidityCommandMessageBuild()
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

            commandString = "RTAH 1,3\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadTemperatureAndHumidityResponseMessageBuild()
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

            commandString = "RTAH 1,0,3,23.40,56.70\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestBootModeCommandMessageBuild()
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

            commandString = "BMODE 1,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestBootModeResponseMessageBuild()
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

            commandString = "BMODE 1,0\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestResetCommandMessageBuild()
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

            commandString = "RESET 1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestResetResponseMessageBuild()
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

            commandString = "RESET 1,0\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);

            CollectionAssert.AreEqual(expectedPacket, _message.Buffer, "Result packet should be matched.");
        }
    }
}
