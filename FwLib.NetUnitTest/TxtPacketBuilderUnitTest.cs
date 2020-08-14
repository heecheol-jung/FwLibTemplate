using FwLib.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace FwLib.NetUnitTest
{
    [TestClass]
    public class TxtPacketBuilderUnitTest
    {
        IFwLibMessage message;
        string commandString;
        byte[] expectedPacket;

        [TestInitialize]
        public void InitializeBeforeEveryTest()
        {
            message = null;
        }

        [TestMethod]
        public void TestReadHardwareVersionCommandMessageBuild()
        {
            message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion,
                DeviceId = 1
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "RHVER 1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadHardwareVersionResponseMessageBuild()
        {
            message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadHardwareVersion,
                DeviceId = 1,
                Arguments = new List<object>()
                {
                    "0",    // Return code
                    "1.2.3" // Version string
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "RHVER 1,0,1.2.3\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadFirmVersionCommandMessageBuild()
        {
            message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion,
                DeviceId = 2
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "RFVER 2\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionResponseMessageBuild()
        {
            message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadFirmwareVersion,
                DeviceId = 2,
                Arguments = new List<object>()
                {
                    "0",    // Return code
                    "2.3.4" // Version string
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "RFVER 2,0,2.3.4\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadGpioCommandMessageBuild()
        {
            message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.ReadGpio,
                DeviceId = 3,
                Arguments = new List<object>()
                {
                    "2"     // GPIO number
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "RGPIO 3,2\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestReadGpioResponsedMessageBuild()
        {
            message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.ReadGpio,
                DeviceId = 3,
                Arguments = new List<object>()
                {
                    "0",    // Return code
                    "2",    // GPIO number
                    "1"     // GPIO value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "RGPIO 3,0,2,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestWriteGpioCommandMessageBuild()
        {
            message = new FwLibTxtMessageCommand()
            {
                MessageId = FwLibMessageId.WriteGpio,
                DeviceId = 4,
                Arguments = new List<object>()
                {
                    "3",    // GPIO number
                    "1"     // GPIO value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "WGPIO 4,3,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestWriteGpioCommandResponseBuild()
        {
            message = new FwLibTxtMessageResponse()
            {
                MessageId = FwLibMessageId.WriteGpio,
                DeviceId = 4,
                Arguments = new List<object>()
                {
                    "0"     // Return code
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "WGPIO 4,0\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }

        [TestMethod]
        public void TestButtonPressedEventMessageBuild()
        {
            message = new FwLibTxtMessageEvent()
            {
                MessageId = FwLibMessageId.ButtonEvent,
                DeviceId = 5,
                Arguments = new List<object>()
                {
                    "4",    // Button number
                    "1"     // Button value
                }
            };

            FwLibTxtPacketBuilder.BuildMessagePacket(ref message);

            commandString = "EBTN 5,4,1\n";
            expectedPacket = Encoding.ASCII.GetBytes(commandString);
            CollectionAssert.AreEqual(expectedPacket, message.Buffer, "Result packet should be matched.");
        }
    }
}
