using Fl.Net;
using Fl.Net.Message;
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
        private List<AppMessageTemplate> _msgTemplates = null;
        private AppMessageTemplate _selectedMsgTemplate = null;
        private int _lastSelectedMsgId = -1;
        private IMessageManager _msgManager = null;
        private ILog _logger = null;
        private byte _sequenceNumber = FlConstant.FL_BIN_MSG_MIN_SEQUENCE;
        private List<UcArgumentTemplate> _argTemplates = new List<UcArgumentTemplate>();
        private List<IFlMessage> _eventQ = new List<IFlMessage>();
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

            _msgTemplates = InternalUtil.GetMessageTemplates();

            var distinctMsgIds = (from msgTemplate in _msgTemplates
                                  where (msgTemplate.MessageType == FlMessageCategory.Command)
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
                        _sequenceNumber = FlConstant.FL_BIN_MSG_MIN_SEQUENCE;
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
                                                where (item.MessageIdForCApi == (string)CmbMessageId.SelectedItem && item.MessageType == FlMessageCategory.Command)
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
                    case FlMessageId.ReadHardwareVersion:
                        validArguments = true;
                        ReadHardwareVersion(deviceId);
                        break;

                    case FlMessageId.ReadFirmwareVersion:
                        validArguments = true;
                        ReadFirmwareVersion(deviceId);
                        break;

                    case FlMessageId.WriteGpio:
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

                    case FlMessageId.ReadGpio:
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

                    case FlMessageId.ReadTemperature:
                    case FlMessageId.ReadHumidity:
                    case FlMessageId.ReadTempAndHum:
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
                            if (_selectedMsgTemplate.MessageId == FlMessageId.ReadTemperature)
                            {
                                ReadTemperature(deviceId, sensorNum);
                            }
                            else if (_selectedMsgTemplate.MessageId == FlMessageId.ReadHumidity)
                            {
                                ReadHumidity(deviceId, sensorNum);
                            }
                            else if (_selectedMsgTemplate.MessageId == FlMessageId.ReadTempAndHum)
                            {
                                ReadTempAndHum(deviceId, sensorNum);
                            }
                            
                        }
                        break;

                    case FlMessageId.BootMode:
                        {
                            UcArgumentTemplate artBootMode = (UcArgumentTemplate)SpArguments.Children[0];
                            
                            if (byte.TryParse(artBootMode.ArgValue, out byte bootMode) != true)
                            {
                                MessageBox.Show("Invalid boot mode.");
                                break;
                            }

                            validArguments = true;
                            BootMode(deviceId, bootMode);
                        }
                        break;

                    case FlMessageId.Reset:
                        {
                            validArguments = true;
                            Reset(deviceId);
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
            List<IFlMessage> events;

            lock (_lock)
            {
                events = new List<IFlMessage>();
                events.AddRange(_eventQ);
                _eventQ.Clear();
            }

            foreach (var evt in events)
            {
                string message = string.Empty;

                if (evt != null)
                {
                    if (evt.MessageId == FlMessageId.ButtonEvent)
                    {
                        if (evt.MessageType == FlMessageType.Binary)
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
                        else if (evt.MessageType == FlMessageType.Text)
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
        private void OnEventMessageReceived(object sender, IFlMessage evt)
        {
            lock (_lock)
            {
                _eventQ.Add(evt);
            }
        }

        private byte NextSequenceNumber()
        {
            _sequenceNumber++;
            if (_sequenceNumber > FlConstant.FL_BIN_MSG_MAX_SEQUENCE)
            {
                _sequenceNumber = FlConstant.FL_BIN_MSG_MIN_SEQUENCE;
            }

            return _sequenceNumber;
        }

        private void ReadHardwareVersion(uint deviceId)
        {
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadHardwareVersion
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadHardwareVersion,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString()
                    }
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
                            if (((FlBinMessageResponse)(result.Response)).Header.flag2.error == FlConstant.FL_OK)
                            {
                                if (result.Response.Arguments?.Count == 1)
                                {
                                    response = $"R : {(string)result.Response.Arguments[0]}";

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
                            if (result.Response.Arguments?.Count == 3)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[2]}";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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

        private string GetArgumentString(List<object> arguments)
        {
            string argString = string.Empty;

            for (int i = 0; i < arguments?.Count; i++)
            {
                argString += arguments[i];
                if (arguments.Count > 1)
                {
                    argString += ", ";
                }
            }

            return argString;
        }

        private void ReadFirmwareVersion(uint deviceId)
        {
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadFirmwareVersion
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadFirmwareVersion,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString()
                    }
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
                            if (((FlBinMessageResponse)(result.Response)).Header.flag2.error == FlConstant.FL_OK)
                            {
                                if (result.Response.Arguments?.Count == 1)
                                {
                                    response = $"R : {(string)result.Response.Arguments[0]}";
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
                            if (result.Response.Arguments?.Count == 3)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[2]}";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.WriteGpio,
                    Arguments = new List<object>()
                    {
                        gpioNum,
                        gpioValue
                    }
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.WriteGpio,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString(),
                        gpioNum.ToString(),
                        gpioValue.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.WriteGpio(command);

                LbMessageHistory.Items.Add($"S : WriteGpio");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            if (((FlBinMessageResponse)(result.Response)).Header.flag2.error == FlConstant.FL_OK)
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
                            if (result.Response.Arguments?.Count == 2)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadGpio,
                    Arguments = new List<object>()
                    {
                        gpioNum
                    }
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadGpio,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString(),
                        gpioNum.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.ReadGpio(command);

                LbMessageHistory.Items.Add($"S : ReadGpio");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FlBinMessageResponse resp = (FlBinMessageResponse)result.Response;
                            if (resp.Header.flag2.error == FlConstant.FL_OK)
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
                            if (result.Response.Arguments?.Count == 4)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[2]}, {(string)result.Response.Arguments[3]}";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadTemperature,
                    Arguments = new List<object>()
                    {
                        sensorNum
                    }
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
                ((FlBinMessageCommand)command).TryInterval = 1000;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadTemperature,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString(),
                        sensorNum.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.ReadTemperature(command);

                LbMessageHistory.Items.Add($"S : ReadTemperature");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FlBinMessageResponse resp = (FlBinMessageResponse)result.Response;
                            if (resp.Header.flag2.error == FlConstant.FL_OK)
                            {
                                if (result.Response.Arguments?.Count == 2)
                                {
                                    response = $"R : {(byte)result.Response.Arguments[0]},{(double)result.Response.Arguments[1]:0.##}";
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
                            if (result.Response.Arguments?.Count == 4)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[2]}, {(string)result.Response.Arguments[3]}";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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

        private void ReadTempAndHum(uint deviceId, byte sensorNum)
        {
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadTempAndHum,
                    Arguments = new List<object>()
                    {
                        sensorNum
                    }
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadTempAndHum,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString(),
                        sensorNum.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.ReadTemperatureAndHumidity(command);

                LbMessageHistory.Items.Add($"S : ReadTemperature");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FlBinMessageResponse resp = (FlBinMessageResponse)result.Response;
                            if (resp.Header.flag2.error == FlConstant.FL_OK)
                            {
                                if (result.Response.Arguments?.Count == 3)
                                {
                                    response = string.Format("R : {0},{1:0.##}.{2:0.##}",
                                        (byte)result.Response.Arguments[0],
                                        (double)result.Response.Arguments[1],
                                        (double)result.Response.Arguments[1]
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
                            if (result.Response.Arguments?.Count == 5)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[2]}, {(string)result.Response.Arguments[3]}, {(string)result.Response.Arguments[4]}";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadHumidity,
                    Arguments = new List<object>()
                    {
                        sensorNum
                    }
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadHumidity,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString(),
                        sensorNum.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.ReadHumidity(command);

                LbMessageHistory.Items.Add($"S : ReadTempAndHum");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            FlBinMessageResponse resp = (FlBinMessageResponse)result.Response;
                            if (resp.Header.flag2.error == FlConstant.FL_OK)
                            {
                                if (result.Response.Arguments?.Count == 2)
                                {
                                    response = $"R : {(byte)result.Response.Arguments[0]},{(double)result.Response.Arguments[1]:0.##}";
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
                            if (result.Response.Arguments?.Count == 4)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK, {(string)result.Response.Arguments[2]}, {(string)result.Response.Arguments[3]}";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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

        private void BootMode(uint deviceId, byte bootMode)
        {
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.BootMode
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;

                command.Arguments = new List<object>()
                {
                    bootMode
                };
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.BootMode,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString(),
                        bootMode.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.BootMode(command);

                LbMessageHistory.Items.Add($"S : Boot mode");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            if (((FlBinMessageResponse)(result.Response)).Header.flag2.error == FlConstant.FL_OK)
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
                            if (result.Response.Arguments?.Count == 2)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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

        private void Reset(uint deviceId)
        {
            IFlMessage command = null;

            if (_currentParserType == ParserType.Binary)
            {
                command = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.Reset
                };
                ((FlBinMessageCommand)command).Header.device_id = deviceId;
                ((FlBinMessageCommand)command).Header.flag1.sequence_num = _sequenceNumber;
            }
            else if (_currentParserType == ParserType.Text)
            {
                command = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.Reset,
                    Arguments = new List<object>()
                    {
                        deviceId.ToString()
                    }
                };
            }

            if (command != null)
            {
                CommandResult result = _msgManager.Reset(command);

                LbMessageHistory.Items.Add($"S : Reset");

                string response = string.Empty;
                if (result != null)
                {
                    if (result.Response != null)
                    {
                        if (_currentParserType == ParserType.Binary)
                        {
                            if (((FlBinMessageResponse)(result.Response)).Header.flag2.error == FlConstant.FL_OK)
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
                            if (result.Response.Arguments?.Count == 2)
                            {
                                if ((string)result.Response.Arguments[1] == FlConstant.FL_OK.ToString())
                                {
                                    response = $"R : OK";
                                }
                                else
                                {
                                    response = $"R : {GetArgumentString(result.Response.Arguments)}";
                                }
                            }
                            else
                            {
                                response = $"R : {GetArgumentString(result.Response.Arguments)}";
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
