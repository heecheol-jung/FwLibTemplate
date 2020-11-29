using FwLib.Net;
using FwLib.NetUnitTest.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace FwLib.NetUnitTest
{
    [TestClass]
    public class BinMessageParseUnitTestWithRealDevice
    {
        private static BinMessageManager _msgMgr = new BinMessageManager();
        // Uncommenting below code will make no tests run.
        //private static TestContext Context { get; set; }  

        FwLibBinMessageCommand _binCmd = null;
        FwLibBinMessageResponse _binResp = null;
        IFwLibMessage _message;
        byte _sequenceNumber = FwLibConstant.BIN_MSG_MIN_SEQUENCE;

        byte NextSequenceNumber()
        {
            _sequenceNumber++;
            if (_sequenceNumber > FwLibConstant.BIN_MSG_MAX_SEQUENCE)
            {
                _sequenceNumber = FwLibConstant.BIN_MSG_MIN_SEQUENCE;
            }

            return _sequenceNumber;
        }

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            MessageManagerSetting setting = new MessageManagerSetting()
            {
                ComSetting = new SerialPortSetting()
                {
                    PortName = "COM3",
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One
                }
            };

            //_msgMgr.OnEventReceived = OnEventMessageReceived;
            _msgMgr.Start(setting);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _msgMgr?.Stop();
        }

        [TestInitialize]
        public void InitBeforeEveryTest()
        {
            _binCmd = new FwLibBinMessageCommand();
            _binCmd.MaxTryCount = 1;
            _binCmd.Header.DeviceId = 1;
            _binCmd.Header.SequenceNumber = _sequenceNumber;

            _binResp = null;

            NextSequenceNumber();
        }

        [TestMethod]
        public void TestReadHardwareVersionOkResponseMessageParse()
        {
            _binCmd.MessageId = FwLibMessageId.ReadHardwareVersion;
         
            _message = _binCmd;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _binResp = (FwLibBinMessageResponse)_msgMgr.ProcessCommand(_message);

            Assert.IsNotNull(_binResp, "Response should not be null.");
            Assert.AreEqual((byte)1, _binResp.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)9, _binResp.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadHardwareVersion, _binResp.MessageId, "Header Message ID field should be ReadHardwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Response, _binResp.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, _binResp.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual(_binCmd.Header.SequenceNumber, _binResp.Header.SequenceNumber, $"Header Sequence number field should be {_binCmd.Header.SequenceNumber}.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(_binResp.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(3, _binResp.Arguments.Count, "Argument count should be 3.");
            Assert.AreEqual(typeof(byte), _binResp.Arguments[0].GetType(), "Argument[0] should be byte type.");
            Assert.AreEqual(typeof(byte), _binResp.Arguments[1].GetType(), "Argument[1] should be byte type.");
            Assert.AreEqual(typeof(byte), _binResp.Arguments[2].GetType(), "Argument[2] should be byte type.");
            Assert.AreEqual((byte)0, (byte)_binResp.Arguments[0], "Argument0 should be 0.");
            Assert.AreEqual((byte)0, (byte)_binResp.Arguments[1], "Argument1 should be 0.");
            Assert.AreEqual((byte)1, (byte)_binResp.Arguments[2], "Argument1 should be 1.");
            Assert.AreNotEqual(DateTime.MinValue, _binResp.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadFirmwareVersionOkResponseMessageParse()
        {
            _binCmd.MessageId = FwLibMessageId.ReadFirmwareVersion;

            _message = _binCmd;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _binResp = (FwLibBinMessageResponse)_msgMgr.ProcessCommand(_message);

            Assert.IsNotNull(_binResp, "Response should not be null.");
            Assert.AreEqual((byte)1, _binResp.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)9, _binResp.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadFirmwareVersion, _binResp.MessageId, "Header Message ID field should be ReadFirmwareVersion.");
            Assert.AreEqual(FwLibMessageCategory.Response, _binResp.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, _binResp.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual(_binCmd.Header.SequenceNumber, _binResp.Header.SequenceNumber, $"Header Sequence number field should be {_binCmd.Header.SequenceNumber}.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(_binResp.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(3, _binResp.Arguments.Count, "Argument count should be 3.");
            Assert.AreEqual(typeof(byte), _binResp.Arguments[0].GetType(), "Argument[0] should be byte type.");
            Assert.AreEqual(typeof(byte), _binResp.Arguments[1].GetType(), "Argument[1] should be byte type.");
            Assert.AreEqual(typeof(byte), _binResp.Arguments[2].GetType(), "Argument[2] should be byte type.");
            Assert.AreEqual((byte)0, (byte)_binResp.Arguments[0], "Argument0 should be 0.");
            Assert.AreEqual((byte)1, (byte)_binResp.Arguments[1], "Argument1 should be 1.");
            Assert.AreEqual((byte)1, (byte)_binResp.Arguments[2], "Argument1 should be 1.");
            Assert.AreNotEqual(DateTime.MinValue, _binResp.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadGpioOkResponseMessageParse()
        {
            _binCmd.MessageId = FwLibMessageId.ReadGpio;
            _binCmd.Arguments = new List<object>()
            {
                (byte)1 // GPIO number
            };

            _message = _binCmd;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _binResp = (FwLibBinMessageResponse)_msgMgr.ProcessCommand(_message);

            Assert.IsNotNull(_binResp, "Response should not be null.");
            Assert.AreEqual((byte)1, _binResp.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)8, _binResp.Header.Length, "Header Length field should be 8.");
            Assert.AreEqual(FwLibMessageId.ReadGpio, _binResp.MessageId, "Header Message ID field should be ReadGpio.");
            Assert.AreEqual(FwLibMessageCategory.Response, _binResp.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, _binResp.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual(_binCmd.Header.SequenceNumber, _binResp.Header.SequenceNumber, $"Header Sequence number field should be {_binCmd.Header.SequenceNumber}.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(_binResp.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, _binResp.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)_binResp.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual((byte)0, (byte)_binResp.Arguments[1], "Argument1 should be 0.");
            Assert.AreNotEqual(DateTime.MinValue, _binResp.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestWriteGpioOkResponseMessageParse()
        {
            _binCmd.MessageId = FwLibMessageId.WriteGpio;
            _binCmd.Arguments = new List<object>()
            {
                (byte)1,    // GPIO number,
                (byte)1     // GPIO value
            };

            _message = _binCmd;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _binResp = (FwLibBinMessageResponse)_msgMgr.ProcessCommand(_message);

            Assert.IsNotNull(_binResp, "Response should not be null.");
            Assert.AreEqual((byte)1, _binResp.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)6, _binResp.Header.Length, "Header Length field should be 6.");
            Assert.AreEqual(FwLibMessageId.WriteGpio, _binResp.MessageId, "Header Message ID field should be WriteGpio.");
            Assert.AreEqual(FwLibMessageCategory.Response, _binResp.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, _binResp.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual(_binCmd.Header.SequenceNumber, _binResp.Header.SequenceNumber, $"Header Sequence number field should be {_binCmd.Header.SequenceNumber}.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNull(_binResp.Arguments, "Arguments property should be null.");
            Assert.AreNotEqual(DateTime.MinValue, _binResp.ReceiveTime, "Received time should not be DateTime.MinValue.");
        }

        [TestMethod]
        public void TestReadTemperatureOkResponseMessageParse()
        {
            _binCmd.MessageId = FwLibMessageId.ReadTemperature;
            _binCmd.Arguments = new List<object>()
            {
                (byte)1 // Sensor number
            };

            _message = _binCmd;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _binResp = (FwLibBinMessageResponse)_msgMgr.ProcessCommand(_message);

            Assert.IsNotNull(_binResp, "Response should not be null.");
            Assert.AreEqual((byte)1, _binResp.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)9, _binResp.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadTemperature, _binResp.MessageId, "Header Message ID field should be ReadTemperature.");
            Assert.AreEqual(FwLibMessageCategory.Response, _binResp.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, _binResp.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual(_binCmd.Header.SequenceNumber, _binResp.Header.SequenceNumber, $"Header Sequence number field should be {_binCmd.Header.SequenceNumber}.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(_binResp.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, _binResp.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)_binResp.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual(typeof(UInt16), _binResp.Arguments[1].GetType(), "Argument[1] should be UInt16 type.");
            Assert.AreNotEqual(DateTime.MinValue, _binResp.ReceiveTime, "Received time should not be DateTime.MinValue.");

            Thread.Sleep(1000); // Fast sensor reading can cause sensor read fail. Give some time for next sensor reading.
        }

        [TestMethod]
        public void TestReadHumidityOkResponseMessageParse()
        {
            _binCmd.MessageId = FwLibMessageId.ReadHumidity;
            _binCmd.Arguments = new List<object>()
            {
                (byte)1 // Sensor number
            };

            _message = _binCmd;

            FwLibBinPacketBuilder.BuildMessagePacket(ref _message);

            _binResp = (FwLibBinMessageResponse)_msgMgr.ProcessCommand(_message);

            Assert.IsNotNull(_binResp, "Response should not be null.");
            Assert.AreEqual((byte)1, _binResp.Header.DeviceId, "Header Device Id field should be 1.");
            Assert.AreEqual((byte)9, _binResp.Header.Length, "Header Length field should be 9.");
            Assert.AreEqual(FwLibMessageId.ReadHumidity, _binResp.MessageId, "Header Message ID field should be ReadHumidity.");
            Assert.AreEqual(FwLibMessageCategory.Response, _binResp.MessageCategory, "Header Message type field should be Response.");
            Assert.AreEqual(false, _binResp.Header.ReturnExpected, "Header ReturnExpected field should be false.");
            Assert.AreEqual(_binCmd.Header.SequenceNumber, _binResp.Header.SequenceNumber, $"Header Sequence number field should be {_binCmd.Header.SequenceNumber}.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag1Reserved, "Header flag1 reserved field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Error, "Header Error field should be 0.");
            Assert.AreEqual((byte)0, _binResp.Header.Flag2Reserved, "Header flag2 reserved field should be 0.");
            Assert.IsNotNull(_binResp.Arguments, "Arguments property should not be null.");
            Assert.AreEqual(2, _binResp.Arguments.Count, "Argument count should be 2.");
            Assert.AreEqual((byte)1, (byte)_binResp.Arguments[0], "Argument0 should be 1.");
            Assert.AreEqual(typeof(UInt16), _binResp.Arguments[1].GetType(), "Argument[1] should be UInt16 type.");
            Assert.AreNotEqual(DateTime.MinValue, _binResp.ReceiveTime, "Received time should not be DateTime.MinValue.");

            Thread.Sleep(1000); // Fast sensor reading can cause sensor read fail. Give some time for next sensor reading.
        }
    }
}
