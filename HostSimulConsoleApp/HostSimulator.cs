using AppCommon.Net;
using Fl.Net;
using Fl.Net.Message;
using Fl.Net.Parser;
using Serilog;
using System;
using System.IO.Ports;
using System.Threading;

namespace HostSimulConsoleApp
{
    internal class HostSimulator
    {
        const int MAX_BUF_LEN = 2048;

        #region Private Fields
        byte[] _tx_buf = new byte[MAX_BUF_LEN];
        UInt32 _tx_len;
        byte[] _rx_buf = new byte[MAX_BUF_LEN];
        int _rx_len;
        byte[] _general_buf = new byte[MAX_BUF_LEN];
        int _general_len;
        SerialPort _serialPort = new SerialPort();
        StartStatus _status = StartStatus.Stopped;
        Thread _messageThread = null;
        bool _messageLoop = false;
        EventWaitHandle _serialEvent;
        object _generalLock = new object();
        FlTxtParser _appTxtParser = new FlTxtParser();
        FlBinParser _appBinParser = new FlBinParser();
        IFlMessage _response = null;
        byte _seqNum = 0;
        #endregion

        #region Public Properties
        public UInt32 DeviceId { get; set; }
        public StartStatus Status => _status;
        public MessageType MessageType { get; set; }
        public bool ResponseReceived { get; set; }
        public string ComPortName { get; set; }
        public FlTxtParser AppTxtParser => _appTxtParser;
        public FlBinParser AppBinParser => _appBinParser;
        #endregion

        #region Constructors
        public HostSimulator()
        {
            InitParsers();
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            _status = StartStatus.Starting;

            _serialEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

            OpenSerialPort();

            _appBinParser.Role = FlParserRole.Host;

            _messageLoop = true;
            _messageThread = new Thread(new ThreadStart(InternalMessageProc))
            {
                Priority = ThreadPriority.Highest
            };
            _messageThread.Start();

            _status = StartStatus.Started;

            Log.Debug("Simulator started");
        }

        public void Stop()
        {
            if (_status == StartStatus.Started)
            {
                _status = StartStatus.Stopping;

                _serialEvent.Set();
                _messageLoop = false;
                _messageThread.Join();

                CloseSerialPort();

                _serialEvent.Dispose();

                _status = StartStatus.Stopped;

                Log.Debug("Simulator stopped");
            }
        }

        public void SendPacket(byte[] buf)
        {
            if (_status != StartStatus.Started)
            {
                Log.Warning("Simulator is not started");
                return;
            }

            _serialPort.Write(buf, 0, buf.Length);
            Log.Information("Message sent");
        }

        public bool IsComPortOpened()
        {
            if (_serialPort?.IsOpen == true)
            {
                return true;
            }

            return false;
        }

        public byte GetNextSeqNum()
        {
            _seqNum++;
            if (_seqNum > FlConstant.FL_BIN_MSG_MAX_SEQUENCE)
            {
                _seqNum = 1;
            }

            return _seqNum;
        }
        #endregion

        #region Private Methods
        private void InitParsers()
        {
            _appTxtParser.Clear();
            _appBinParser.Clear();
        }

        private void OpenSerialPort()
        {
            CloseSerialPort();

            _serialPort.PortName = ComPortName;
            _serialPort.BaudRate = 115200;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(OnSerialPortDataReceived);

            _serialPort.Open();

            Log.Debug("Serial port opened");
        }

        private void OnSerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _serialEvent.Set();

            switch (e.EventType)
            {
                case SerialData.Chars:
                    break;

                case SerialData.Eof:
                    Log.Debug("EOF received");
                    break;
            }
        }

        private void CloseSerialPort()
        {
            if (_serialPort.IsOpen == true)
            {
                _serialPort.DataReceived -= OnSerialPortDataReceived;

                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();

                Log.Debug("Serial port closed.");
            }
        }

        private void InternalMessageProc()
        {
            while (_messageLoop)
            {
                _serialEvent.WaitOne();

                if (!_messageLoop)
                {
                    break;
                }

                _rx_len = _serialPort.BytesToRead;
                if (_rx_len > 0)
                {
                    if (_rx_len > _rx_buf.Length)
                    {
                        _rx_len = _rx_buf.Length;
                    }

                    _serialPort.Read(_rx_buf, 0, _rx_len);
   
                    if (MessageType == MessageType.Binary)
                    {
                        ProcessAppBinMessage(_rx_len);
                    }
                    else if (MessageType == MessageType.Text)
                    {
                        ProcessAppTxtMessage(_rx_len);
                    }
                }
            }
        }

        #region Application Binary Message
        private void ProcessAppBinMessage(int bytesToRead)
        {
            for (int i = 0; i < bytesToRead; i++)
            {
                FlParseState ret = _appBinParser.Parse(_rx_buf[i], out _response);
                if (ret == FlParseState.ParseOk)
                {
                    ResponseReceived = true;
                    IFlBinMessage message = (IFlBinMessage)_response;
                    switch (message.MessageId)
                    {
                        case FlMessageId.ReadHardwareVersion:
                            ProcessAppBinHwVerResponse(message);
                            break;

                        case FlMessageId.ReadFirmwareVersion:
                            ProcessAppBinFwVerResponse(message);
                            break;

                        case FlMessageId.ReadGpio:
                            ProcessAppBinReadGpioResponse(message);
                            break;

                        case FlMessageId.WriteGpio:
                            ProcessAppBinWriteGpioResponse(message);
                            break;

                        case FlMessageId.ReadTemperature:
                            ProcessAppBinReadTemperatureResponse(message);
                            break;

                        case FlMessageId.ReadHumidity:
                            ProcessAppBinReadHumidityResponse(message);
                            break;

                        case FlMessageId.ReadTempAndHum:
                            ProcessAppBinReadTempAndHumResponse(message);
                            break;

                        case FlMessageId.BootMode:
                            ProcessAppBinBootModeResponse(message);
                            break;

                        case FlMessageId.Reset:
                            ProcessAppBinResetResponse(message);
                            break;

                        case FlMessageId.ButtonEvent:
                            ProcessAppBinButtonEvent(message);
                            break;
                    }
                }
                else if (ret == FlParseState.ParseFail)
                {
                    Log.Debug("Application binary response parser fail");
                }
            }
        }

        private void ProcessAppBinButtonEvent(IFlBinMessage evt)
        {
            if (evt.Arguments?.Count == 2)
            {
                Log.Information($"Button : {evt.Arguments[0]}, State : {evt.Arguments[1]}");
            }
            else
            {
                Log.Warning("Invalid button event");
            }
        }

        private void ProcessAppBinResetResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                Log.Information("Reset OK");
            }
            else
            {
                Log.Warning("Reset fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinBootModeResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                Log.Information("Boot mode OK");
            }
            else
            {
                Log.Warning("Boot mode fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinReadTempAndHumResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                if (response.Arguments?.Count == 3)
                {
                    Log.Information($"Sensor number : {(byte)response.Arguments[0]}, Temperature : {(double)response.Arguments[1]:0.##}, Humidity : {(double)response.Arguments[2]:0.##}");
                }
            }
            else
            {
                Log.Warning("Read temperatrue and humidity fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinReadHumidityResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                if (response.Arguments?.Count == 2)
                {
                    Log.Information($"Sensor number : {(byte)response.Arguments[0]}, Humidity : {(double)response.Arguments[1]:0.##}");
                }
            }
            else
            {
                Log.Warning("Read humidity fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinReadTemperatureResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                if (response.Arguments?.Count == 2)
                {
                    Log.Information($"Sensor number : {(byte)response.Arguments[0]}, Temperature : {(double)response.Arguments[1]:0.##}");
                }
            }
            else
            {
                Log.Warning("Read temperature fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinWriteGpioResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                Log.Information("Write GPIO OK");
            }
            else
            {
                Log.Warning("Write GPIO fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinReadGpioResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                if (response.Arguments?.Count == 2)
                {
                    Log.Information($"GPIO number : {(byte)response.Arguments[0]}, GPIO value : {(byte)response.Arguments[1]}");
                }
            }
            else
            {
                Log.Warning("Read GPIO fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinFwVerResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                if (response.Arguments?.Count == 1)
                {
                    Log.Information($"Fw version : {(string)response.Arguments[0]}");
                }
            }
            else
            {
                Log.Warning("Hardware version read fail");
            }

            ResponseReceived = true;
        }

        private void ProcessAppBinHwVerResponse(IFlBinMessage response)
        {
            if (response.Header.flag2.error == FlConstant.FL_OK)
            {
                if (response.Arguments?.Count == 1)
                {
                    Log.Information($"Hw version : {(string)response.Arguments[0]}");
                }
            }
            else
            {
                Log.Warning("Hardware version read fail");
            }

            ResponseReceived = true;
        }
        #endregion // Application Binary Message

        #region Application Text Message
        private void ProcessAppTxtMessage(int bytesToRead)
        {
            for (int i = 0; i < bytesToRead; i++)
            {
                FlParseState ret = _appTxtParser.ParseResponseEvent(_rx_buf[i], out _response);
                if (ret == FlParseState.ParseOk)
                {
                    ResponseReceived = true;

                    switch (_response.MessageId)
                    {
                        case FlMessageId.ReadHardwareVersion:
                            ProcessAppTxtHwVerResponse(_response);
                            break;

                        case FlMessageId.ReadFirmwareVersion:
                            ProcessAppTxtFwVerResponse(_response);
                            break;

                        case FlMessageId.ReadGpio:
                            ProcessAppTxtReadGpioResponse(_response);
                            break;

                        case FlMessageId.WriteGpio:
                            ProcessAppTxtWriteGpioResponse(_response);
                            break;

                        case FlMessageId.ReadTemperature:
                            ProcessAppTxtReadTemperatureResponse(_response);
                            break;

                        case FlMessageId.ReadHumidity:
                            ProcessAppTxtReadHumidityResponse(_response);
                            break;

                        case FlMessageId.ReadTempAndHum:
                            ProcessAppTxtReadTempAndHumResponse(_response);
                            break;

                        case FlMessageId.BootMode:
                            ProcessAppTxtBootModeResponse(_response);
                            break;

                        case FlMessageId.Reset:
                            ProcessAppTxtResetResponse(_response);
                            break;

                        case FlMessageId.ButtonEvent:
                            ProcessAppTxtButtonEvent(_response);
                            break;
                    }
                }
                else if (ret == FlParseState.ParseFail)
                {
                    Log.Debug("Application text response parser fail");
                }
            }
        }

        private void ProcessAppTxtButtonEvent(IFlMessage evt)
        {
            if (evt.Arguments?.Count == 3)
            {
                Log.Information($"Button : {evt.Arguments[1]}, State : {evt.Arguments[2]}");
            }
            else
            {
                Log.Warning("Invalid button event");
            }
        }

        private void ProcessAppTxtHwVerResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 3)
            {
                Log.Information($"HW version : {(string)response.Arguments[2]}");
            }
            else
            {
                Log.Warning("Read hardware version failed");
            }
        }

        private void ProcessAppTxtFwVerResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 3)
            {
                Log.Information($"FW version : {(string)response.Arguments[2]}");
            }
            else
            {
                Log.Warning("Read firmware version failed");
            }
        }

        private void ProcessAppTxtReadGpioResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 4)
            {
                Log.Information($"GPIO number : {(string)response.Arguments[2]}, GPIO value : {(string)response.Arguments[3]}");
            }
            else
            {
                Log.Warning("Read GPIO failed");
            }
        }

        private void ProcessAppTxtWriteGpioResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 2)
            {
                Log.Information($"Write GPIO result : {(string)response.Arguments[1]}");
            }
            else
            {
                Log.Warning("Write GPIO failed");
            }
        }

        private void ProcessAppTxtReadTemperatureResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 4)
            {
                Log.Information($"Sensor number : {(string)response.Arguments[2]}, Temperature : {(string)response.Arguments[3]}");
            }
            else
            {
                Log.Warning("Read temperature failed");
            }
        }

        private void ProcessAppTxtReadHumidityResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 4)
            {
                Log.Information($"Sensor number : {(string)response.Arguments[2]}, Humidity : {(string)response.Arguments[3]}");
            }
            else
            {
                Log.Warning("Read humidity failed");
            }
        }

        private void ProcessAppTxtReadTempAndHumResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 5)
            {
                Log.Information($"Sensor number : {(string)response.Arguments[2]}, Temperature : {(string)response.Arguments[3]}, Humidity : {(string)response.Arguments[4]}");
            }
            else
            {
                Log.Warning("Read temperature and humidity failed");
            }
        }

        private void ProcessAppTxtBootModeResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 2)
            {
                Log.Information($"Boot mode result : {(string)response.Arguments[1]}");
            }
            else
            {
                Log.Warning("Boot mode failed");
            }
        }

        private void ProcessAppTxtResetResponse(IFlMessage response)
        {
            if (response.Arguments?.Count == 2)
            {
                Log.Information($"Reset result : {(string)response.Arguments[1]}");
            }
            else
            {
                Log.Warning("Reset failed");
            }
        }
        #endregion // Application Text Message

        #endregion // Private Methods
    }
}
