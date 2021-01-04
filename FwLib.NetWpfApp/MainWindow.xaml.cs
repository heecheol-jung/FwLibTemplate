using FwLib.Net;
using FwLib.NetWpfApp.AppControl;
using FwLib.NetWpfApp.AppUtil;
using log4net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FwLib.NetWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Data
        private List<FwLibMessageTemplate> _msgTemplates = null;
        private FwLibMessageTemplate _selectedMsgTemplate = null;
        private int _lastSelectedMsgId = -1;
        private IMessageManager _msgManager = null;
        private ILog _logger = null;
        private uint _deviceId = 1;
        private byte _sequenceNumber = FwLibConstant.BIN_MSG_MIN_SEQUENCE;
        private List<UcArgumentTemplate> _argTemplates = new List<UcArgumentTemplate>();
        private List<IFwLibMessage> _eventQ = new List<IFwLibMessage>();
        private DispatcherTimer _eventTimer = new DispatcherTimer();
        private object _lock = new object();
        private ParserType _currentParserType = ParserType.Unknown;
        #endregion

        #region Constructors
        public MainWindow()
        {
            _logger = LogManager.GetLogger(typeof(MainWindow));

            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _eventTimer.Interval = TimeSpan.FromMilliseconds(100);
            _eventTimer.Tick += Event_Tick;

            _msgTemplates = FwLibUtil.GetMessageTemplates();

            var distinctMsgIds = (from msgTemplate in _msgTemplates
                                  where (msgTemplate.MessageType == FwLibMessageCategory.Command)
                                  select msgTemplate.MessageIdForCApi).Distinct();
            foreach (var msgId in distinctMsgIds)
            {
                CmbMessageId.Items.Add(msgId);
            }

            _lastSelectedMsgId = CmbMessageId.SelectedIndex;

            CmbParserType.Items.Add(ParserType.Binary.ToString());
            CmbParserType.Items.Add(ParserType.Text.ToString());
            CmbParserType.SelectedIndex = 0;

            BtnSendMessage.IsEnabled = false;

            BtnOpenClose.Content = AppConstant.STR_OPEN;

            _logger.Debug("Window_Loaded");
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_msgManager?.StartStatus == StartStatus.Started)
            {
                _msgManager.Stop();
            }
            _eventTimer.Stop();
        }

        private void BtnOpenClose_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbComPort.Text) == true)
            {
                MessageBox.Show("Invalid port name.");
                return;
            }

            if (_currentParserType == ParserType.Unknown)
            {
                MessageBox.Show("Invalid parser type.");
                return;
            }

            MessageManagerSetting setting = new MessageManagerSetting()
            {
                ComSetting = new SerialPortSetting()
                {
                    PortName = TbComPort.Text,
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One
                }
            };

            try
            {
                if (_msgManager == null)
                {
                    if (_currentParserType == ParserType.Binary)
                    {
                        _msgManager = new BinMessageManager();
                        _sequenceNumber = FwLibConstant.BIN_MSG_MIN_SEQUENCE;
                    }
                    else if (_currentParserType == ParserType.Text)
                    {
                        _msgManager = new TxtMessageManager();
                    }
                }

                if (_msgManager.StartStatus == StartStatus.Stopped)
                {
                    BtnOpenClose.Content = AppConstant.STR_OPENING;
                    BtnOpenClose.IsEnabled = false;

                    _eventTimer.Start();
                    _msgManager.OnEventReceived = OnEventMessageReceived;
                    _msgManager.Start(setting);
                }
                else if (_msgManager.StartStatus == StartStatus.Started)
                {
                    BtnOpenClose.Content = AppConstant.STR_CLOSING;
                    BtnOpenClose.IsEnabled = false;

                    _msgManager.Stop();
                    _eventTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (_msgManager != null)
                {
                    if (_msgManager.StartStatus == StartStatus.Stopped)
                    {
                        BtnOpenClose.Content = AppConstant.STR_OPEN;
                        BtnSendMessage.IsEnabled = false;
                        _msgManager = null;
                    }
                    else if (_msgManager.StartStatus == StartStatus.Started)
                    {
                        BtnOpenClose.Content = AppConstant.STR_CLOSE;
                        BtnSendMessage.IsEnabled = true;
                    }
                    BtnOpenClose.IsEnabled = true;
                }
                else
                {
                    BtnOpenClose.Content = AppConstant.STR_OPEN;
                    BtnSendMessage.IsEnabled = true;
                }
            }
        }

        private void CmbParserType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbParserType.SelectedIndex == 0)
            {
                _currentParserType = ParserType.Binary;
            }
            else if (CmbParserType.SelectedIndex == 1)
            {
                _currentParserType = ParserType.Text;
            }
            else
            {
                _currentParserType = ParserType.Unknown;
            }
        }

        private void CmbMessageId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_lastSelectedMsgId != CmbMessageId.SelectedIndex)
            {
                _argTemplates.Clear();
                SpArguments.Children.Clear();
                _selectedMsgTemplate = null;

                if (CmbMessageId.SelectedIndex >= 0)
                {
                    var selectedMsgTemplates = (from item in _msgTemplates
                                                where (item.MessageIdForCApi == (string)CmbMessageId.SelectedItem && item.MessageType == FwLibMessageCategory.Command)
                                                select item).SingleOrDefault();


                    if (string.IsNullOrEmpty(selectedMsgTemplates?.MessageIdForCApi) != true)
                    {
                        _selectedMsgTemplate = selectedMsgTemplates;
                        if (_selectedMsgTemplate.Arguments?.Count > 0)
                        {
                            foreach (var arg in _selectedMsgTemplate.Arguments)
                            {
                                UcArgumentTemplate argTemplate = new UcArgumentTemplate(arg.Name);
                                _argTemplates.Add(argTemplate);
                                SpArguments.Children.Add(argTemplate);
                            }
                        }
                    }
                }
            }
        }

        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if ((string.IsNullOrEmpty(TbDeviceId.Text) == true) ||
                (uint.TryParse(TbDeviceId.Text, out uint deviceId) != true))
            {
                MessageBox.Show("Invalid device ID.");
                return;
            }

            if (_selectedMsgTemplate != null)
            {
                bool validArguments = false;
                BtnSendMessage.IsEnabled = false;

                switch (_selectedMsgTemplate.MessageId)
                {
                    case FwLibMessageId.ReadHardwareVersion:
                        validArguments = true;
                        ReadHardwareVersion(deviceId);
                        break;

                    case FwLibMessageId.ReadFirmwareVersion:
                        validArguments = true;
                        ReadFirmwareVersion(deviceId);
                        break;

                    case FwLibMessageId.WriteGpio:
                        {
                            UcArgumentTemplate argGpioNum = (UcArgumentTemplate)SpArguments.Children[0];
                            UcArgumentTemplate argGpioValue = (UcArgumentTemplate)SpArguments.Children[1];
                            if (string.IsNullOrEmpty(argGpioNum.ArgValue) == true)
                            {
                                MessageBox.Show("Invalie port number.");
                                break;
                            }

                            if (string.IsNullOrEmpty(argGpioValue.ArgValue) == true)
                            {
                                MessageBox.Show("Invalie port value.");
                                break;
                            }

                            if (byte.TryParse(argGpioNum.ArgValue, out byte gpioNum) != true)
                            {
                                MessageBox.Show("Invalie port number.");
                                break;
                            }

                            if (byte.TryParse(argGpioValue.ArgValue, out byte gpioValue) != true)
                            {
                                MessageBox.Show("Invalie port value.");
                                break;
                            }
                            if (gpioValue != 0 && gpioValue != 1)
                            {
                                MessageBox.Show("Invalie port value.");
                                break;
                            }

                            validArguments = true;
                            WriteGpio(deviceId, gpioNum, gpioValue);
                        }
                        break;

                    case FwLibMessageId.ReadGpio:
                        {
                            UcArgumentTemplate argGpioNum = (UcArgumentTemplate)SpArguments.Children[0];
                            if (string.IsNullOrEmpty(argGpioNum.ArgValue) == true)
                            {
                                MessageBox.Show("Invalie port number.");
                                break;
                            }

                            if (byte.TryParse(argGpioNum.ArgValue, out byte gpioNum) != true)
                            {
                                MessageBox.Show("Invalie port number.");
                                break;
                            }


                            validArguments = true;
                            ReadGpio(deviceId, gpioNum);
                        }
                        break;

                    case FwLibMessageId.ReadTemperature:
                    case FwLibMessageId.ReadHumidity:
                    case FwLibMessageId.ReadTemperatureAndHumidity:
                        {
                            UcArgumentTemplate argSensorNum = (UcArgumentTemplate)SpArguments.Children[0];
                            if (string.IsNullOrEmpty(argSensorNum.ArgValue) == true)
                            {
                                MessageBox.Show("Invalie sensor number.");
                                break;
                            }

                            if (byte.TryParse(argSensorNum.ArgValue, out byte sensorNum) != true)
                            {
                                MessageBox.Show("Invalie port number.");
                                break;
                            }


                            validArguments = true;
                            if (_selectedMsgTemplate.MessageId == FwLibMessageId.ReadTemperature)
                            {
                                ReadTemperature(deviceId, sensorNum);
                            }
                            else if (_selectedMsgTemplate.MessageId == FwLibMessageId.ReadHumidity)
                            {
                                ReadHumidity(deviceId, sensorNum);
                            }
                            else if (_selectedMsgTemplate.MessageId == FwLibMessageId.ReadTemperatureAndHumidity)
                            {
                                ReadTemperatureAndHumidity(deviceId, sensorNum);
                            }
                            
                        }
                        break;
                }

                if (_currentParserType == ParserType.Binary)
                {
                    NextSequenceNumber();
                }

                if (validArguments == true)
                {
                    BtnSendMessage.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("No Message Selected.");
            }
        }

        private void BtnClearMessageHistory_Click(object sender, RoutedEventArgs e)
        {
            if (LbMessageHistory.Items.Count > 0)
            {
                LbMessageHistory.Items.Clear();
            }
        }

        private void Event_Tick(object sender, EventArgs e)
        {
            List<IFwLibMessage> events;

            lock (_lock)
            {
                events = new List<IFwLibMessage>();
                events.AddRange(_eventQ);
                _eventQ.Clear();
            }

            foreach (var evt in events)
            {
                string message = string.Empty;

                if (evt != null)
                {
                    if (evt.MessageId == FwLibMessageId.ButtonEvent)
                    {
                        if (evt.MessageType == FwLibMessageType.Binary)
                        {
                            if (evt.Arguments?.Count == 2)
                            {
                                message = $"E : {(byte)evt.Arguments[0]}.{(byte)evt.Arguments[1]}";

                            }
                            else
                            {
                                message = "E : Invalid button event(invalid argument count)";
                            }
                        }
                        else if (evt.MessageType == FwLibMessageType.Text)
                        {
                            if (evt.Arguments?.Count == 2)
                            {
                                message = $"E : {(byte)evt.Arguments[0]}.{(byte)evt.Arguments[1]}";

                            }
                            else
                            {
                                message = "E : Invalid button event(invalid argument count)";
                            }
                        }
                    }
                    else
                    {
                        message = "R : Invalid event(unknown event)";
                    }
                }

                LbMessageHistory.Items.Add(message);
            }
        }
        #endregion

        #region Private Methods
        private void OnEventMessageReceived(object sender, IFwLibMessage evt)
        {
            lock (_lock)
            {
                _eventQ.Add(evt);
            }
        }

        private byte NextSequenceNumber()
        {
            _sequenceNumber++;
            if (_sequenceNumber > FwLibConstant.BIN_MSG_MAX_SEQUENCE)
            {
                _sequenceNumber = FwLibConstant.BIN_MSG_MIN_SEQUENCE;
            }

            return _sequenceNumber;
        }

        private void ReadHardwareVersion(uint deviceId)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadHardwareVersion
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadHardwareVersion,
                    DeviceId = deviceId
                };
            }
            
            if (command != null)
            {
                CommandResult result = _msgManager.ReadHardwareVersion(command);

                LbMessageHistory.Items.Add($"S : ReadHardwareVersion");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            if (((FwLibBinMessageResponse)(result.Response)).Header.Error == FwLibConstant.OK)
                            {
                                if (result.Response.Arguments?.Count == 3)
                                {
                                    response = $"R : {(byte)result.Response.Arguments[0]}.{(byte)result.Response.Arguments[1]}.{(byte)result.Response.Arguments[2]}";

                                }
                                else
                                {
                                    response = "R : Invalid response(invalid version format)";
                                }
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.ERROR)
                                {
                                    response = "R : Error";
                                }
                                else
                                {
                                    response = $"R : Return value - {(byte)result.Response.Arguments[0]}";
                                }
                            }
                            else if (result.Response.Arguments?.Count == 2)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.OK)
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[1]}";
                                }
                                else
                                {
                                    response = $"R : Error - {result.Response.Arguments[0]}, {result.Response.Arguments[1]}";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        private void ReadFirmwareVersion(uint deviceId)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadFirmwareVersion
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadFirmwareVersion,
                    DeviceId = deviceId
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.ReadFirmwareVersion(command);

                LbMessageHistory.Items.Add($"S : ReadFirmwareVersion");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            if (((FwLibBinMessageResponse)(result.Response)).Header.Error == FwLibConstant.OK)
                            {
                                if (result.Response.Arguments?.Count == 3)
                                {
                                    response = $"R : {(byte)result.Response.Arguments[0]}.{(byte)result.Response.Arguments[1]}.{(byte)result.Response.Arguments[2]}";
                                }
                                else
                                {
                                    response = "R : Invalid response(invalid version format)";
                                }
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.ERROR)
                                {
                                    response = "R : Error";
                                }
                                else
                                {
                                    response = $"R : Return value - {(byte)result.Response.Arguments[0]}";
                                }
                            }
                            else if (result.Response.Arguments?.Count == 2)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.OK)
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[1]}";
                                }
                                else
                                {
                                    response = $"R : Error - {result.Response.Arguments[0]}, {result.Response.Arguments[1]}";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        private void WriteGpio(uint deviceId, byte gpioNum, byte gpioValue)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.WriteGpio
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.WriteGpio,
                    DeviceId = deviceId
                };
            }

            if (command != null)
            {
                command.Arguments = new List<object>() { gpioNum, gpioValue };
                CommandResult result = _msgManager.WriteGpio(command);

                LbMessageHistory.Items.Add($"S : WriteGpio");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            if (((FwLibBinMessageResponse)(result.Response)).Header.Error == FwLibConstant.OK)
                            {
                                response = $"R : OK";
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == 0)
                                {
                                    response = $"R : OK";
                                }
                                else
                                {
                                    response = $"R : Error";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        private void ReadGpio(uint deviceId, byte gpioNum)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadGpio
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadGpio,
                    DeviceId = deviceId
                };
            }

            if (command != null)
            {
                command.Arguments = new List<object>() { gpioNum };
                CommandResult result = _msgManager.ReadGpio(command);

                LbMessageHistory.Items.Add($"S : ReadGpio");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FwLibBinMessageResponse resp = (FwLibBinMessageResponse)result.Response;
                            if (resp.Header.Error == FwLibConstant.OK)
                            {
                                if (result.Response.Arguments?.Count == 2)
                                {
                                    response = $"R : {(byte)result.Response.Arguments[0]},{(byte)result.Response.Arguments[1]}";
                                }
                                else
                                {
                                    response = "R : Invalid response(invalid argument count)";
                                }
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.ERROR)
                                {
                                    response = "R : Error";
                                }
                                else
                                {
                                    response = $"R : Return value - {(byte)result.Response.Arguments[0]}";
                                }
                            }
                            else if (result.Response.Arguments?.Count == 3)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.OK)
                                {
                                    response = $"R : OK, {(byte)result.Response.Arguments[1]}, {(byte)result.Response.Arguments[2]}";
                                }
                                else
                                {
                                    response = $"R : Error, {result.Response.Arguments[0]}, {result.Response.Arguments[1]}, {result.Response.Arguments[2]}";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        private void ReadTemperature(uint deviceId, byte sensorNum)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadTemperature
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
                ((FwLibBinMessageCommand)command).TryInterval = 1000;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadTemperature,
                    DeviceId = deviceId
                };
            }

            if (command != null)
            {
                command.Arguments = new List<object>() { sensorNum };
                CommandResult result = _msgManager.ReadTemperature(command);

                LbMessageHistory.Items.Add($"S : ReadTemperature");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FwLibBinMessageResponse resp = (FwLibBinMessageResponse)result.Response;
                            if (resp.Header.Error == FwLibConstant.OK)
                            {
                                if (result.Response.Arguments?.Count == 2)
                                {
                                    UInt16 temperatureValue = (UInt16)result.Response.Arguments[1];
                                    response = $"R : {(byte)result.Response.Arguments[0]},{temperatureValue/10}.{temperatureValue%10}";
                                }
                                else
                                {
                                    response = "R : Invalid response(invalid argument count)";
                                }
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.ERROR)
                                {
                                    response = "R : Error";
                                }
                                else
                                {
                                    response = $"R : Return value - {(byte)result.Response.Arguments[0]}";
                                }
                            }
                            else if (result.Response.Arguments?.Count == 3)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.OK)
                                {
                                    response = $"R : OK, {(byte)result.Response.Arguments[1]}, {(double)result.Response.Arguments[2]}";
                                }
                                else
                                {
                                    response = $"R : Error, {result.Response.Arguments[0]}, {result.Response.Arguments[1]}, {result.Response.Arguments[2]}";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        private void ReadTemperatureAndHumidity(uint deviceId, byte sensorNum)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadTemperatureAndHumidity
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadTemperatureAndHumidity,
                    DeviceId = deviceId
                };
            }

            if (command != null)
            {
                command.Arguments = new List<object>() { sensorNum };
                CommandResult result = _msgManager.ReadTemperatureAndHumidity(command);

                LbMessageHistory.Items.Add($"S : ReadTemperature");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FwLibBinMessageResponse resp = (FwLibBinMessageResponse)result.Response;
                            if (resp.Header.Error == FwLibConstant.OK)
                            {
                                if (result.Response.Arguments?.Count == 3)
                                {
                                    UInt16 temperatureValue = (UInt16)result.Response.Arguments[1];
                                    UInt16 humidityValue = (UInt16)result.Response.Arguments[2];
                                    //response = $"R : {(byte)result.Response.Arguments[0]},{temperatureValue / 10}.{temperatureValue % 10},";
                                    response = string.Format("R : {0},{1}.{2},{3},{4}",
                                        (byte)result.Response.Arguments[0],
                                        temperatureValue / 10, temperatureValue % 10,
                                        humidityValue / 10, humidityValue % 10
                                        );
                                }
                                else
                                {
                                    response = "R : Invalid response(invalid argument count)";
                                }
                                
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.ERROR)
                                {
                                    response = "R : Error";
                                }
                                else
                                {
                                    response = $"R : Return value - {(byte)result.Response.Arguments[0]}";
                                }
                            }
                            else if (result.Response.Arguments?.Count == 4)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.OK)
                                {
                                    response = $"R : OK, {(byte)result.Response.Arguments[1]}, {(double)result.Response.Arguments[2]}, {(double)result.Response.Arguments[3]}";
                                }
                                else
                                {
                                    response = $"R : Error, {result.Response.Arguments[0]}, {result.Response.Arguments[1]}, {result.Response.Arguments[2]}, {result.Response.Arguments[3]}";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        private void ReadHumidity(uint deviceId, byte sensorNum)
        {
            IFwLibMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FwLibBinMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadHumidity
                };
                ((FwLibBinMessageCommand)command).Header.DeviceId = deviceId;
                ((FwLibBinMessageCommand)command).Header.SequenceNumber = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FwLibTxtMessageCommand()
                {
                    MessageId = FwLibMessageId.ReadHumidity,
                    DeviceId = deviceId
                };
            }

            if (command != null)
            {
                command.Arguments = new List<object>() { sensorNum };
                CommandResult result = _msgManager.ReadHumidity(command);

                LbMessageHistory.Items.Add($"S : ReadTemperatureAndHumidity");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FwLibBinMessageResponse resp = (FwLibBinMessageResponse)result.Response;
                            if (resp.Header.Error == FwLibConstant.OK)
                            {
                                if (result.Response.Arguments?.Count == 2)
                                {
                                    UInt16 temperatureValue = (UInt16)result.Response.Arguments[1];
                                    response = $"R : {(byte)result.Response.Arguments[0]},{temperatureValue / 10}.{temperatureValue % 10}";
                                }
                                else
                                {
                                    response = "R : Invalid response(invalid argument count)";
                                }
                            }
                            else
                            {
                                response = "R : Error";
                            }
                        }
                        else if (_currentParserType == ParserType.Text)
                        {
                            if (result.Response.Arguments?.Count == 1)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.ERROR)
                                {
                                    response = "R : Error";
                                }
                                else
                                {
                                    response = $"R : Return value - {(byte)result.Response.Arguments[0]}";
                                }
                            }
                            else if (result.Response.Arguments?.Count == 3)
                            {
                                if ((byte)result.Response.Arguments[0] == FwLibConstant.OK)
                                {
                                    response = $"R : OK, {(byte)result.Response.Arguments[1]}, {(double)result.Response.Arguments[2]}";
                                }
                                else
                                {
                                    response = $"R : Error, {result.Response.Arguments[0]}, {result.Response.Arguments[1]}, {result.Response.Arguments[2]}";
                                }
                            }
                            else
                            {
                                response = "R : Invalid response(invalid argument count)";
                            }
                        }
                        else
                        {
                            response = "R : Invalid parser type";
                        }
                    }
                    else
                    {
                        response = "R : No response";
                    }
                }
                else
                {
                    response = "R : Command fail";
                }

                LbMessageHistory.Items.Add(response);
            }
        }

        #endregion
    }
}
