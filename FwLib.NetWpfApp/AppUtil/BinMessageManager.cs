using FwLib.Net;
using log4net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace FwLib.NetWpfApp.AppUtil
{
    // TODO : TxtMessageManager and BinMessageManager can share more functions.
    // TODO : Binary message management(sequence number, ...)
    public class BinMessageManager : IMessageManager
    {
        #region Private Data
        private SerialPort _serialPort = new SerialPort();
        private object _generalLock = new object();
        private Thread _messageProcessingThread = null;
        private bool _messageLoop = false;
        private FwLibBinParser _parser = new FwLibBinParser();
        private List<FwLibBinMessageCommand> _commandQ = new List<FwLibBinMessageCommand>();
        private List<FwLibBinMessageResponse> _responseQ = new List<FwLibBinMessageResponse>();
        private ILog _logger = null;
        #endregion

        #region Public Properties
        public StartStatus StartStatus { get; private set; } = StartStatus.Stopped;
        public FwLibEventReceived OnEventReceived { get; set; }
        #endregion

        #region Constructors
        public BinMessageManager()
        {
            _logger = LogManager.GetLogger(typeof(BinMessageManager));
        }
        #endregion

        #region Public Methods
        public void Start(MessageManagerSetting setting)
        {
            _logger.Debug("Start enter.");

            if (StartStatus != StartStatus.Stopped)
            {
                _logger.Debug("BinMessageManager is not stopped.");
                throw new Exception("BinMessageManager is not stopped.");
            }

            StartStatus = StartStatus.Starting;

            try
            {
                ClearAllQueues();

                OpenSerialPort(setting.ComSetting);

                _messageLoop = true;
                _parser.Role = FwLibParserRole.Host;
                _messageProcessingThread = new Thread(new ThreadStart(InternalMessageProc))
                {
                    Priority = ThreadPriority.Highest
                };
                _messageProcessingThread.Start();
            }
            catch
            {
                CloseSerialPort();

                _logger.Debug("BinMessageManager thread is not started.");

                throw;
            }

            StartStatus = StartStatus.Started;

            _logger.Debug("Start leave.");
        }

        public void Stop()
        {
            _logger.Debug("Stop enter.");

            if (StartStatus == StartStatus.Started)
            {
                StartStatus = StartStatus.Stopping;

                _messageLoop = false;
                _messageProcessingThread?.Join();

                CloseSerialPort();

                ClearAllQueues();

                StartStatus = StartStatus.Stopped;
            }

            _logger.Debug("Stop leave.");
        }

        // TODO : ReadHardwareVersion arguments.
        public CommandResult ReadHardwareVersion(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }

        // TODO : ReadFirmwareVersion arguments.
        public CommandResult ReadFirmwareVersion(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }

        // TODO : ReadGpio arguments.
        public CommandResult ReadGpio(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }

        // TODO : WriteGpio arguments.
        public CommandResult WriteGpio(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }

        public CommandResult ReadTemperature(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }

        public CommandResult ReadHumidity(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }

        public CommandResult ReadTemperatureAndHumidity(IFwLibMessage command)
        {
            CommandResult result = new CommandResult()
            {
                Command = command
            };

            IFwLibMessage response = ProcessCommand((FwLibBinMessageCommand)command);
            result.Response = (FwLibBinMessageResponse)response;

            return result;
        }
        #endregion

        #region Private Methods
        private void OpenSerialPort(SerialPortSetting serialSetting)
        {
            CloseSerialPort();

            _serialPort.PortName = serialSetting.PortName;
            _serialPort.BaudRate = serialSetting.BaudRate;
            _serialPort.DataBits = serialSetting.DataBits;
            _serialPort.Parity = serialSetting.Parity;
            _serialPort.StopBits = serialSetting.StopBits;

            _serialPort.Open();

            _logger.Debug("Serial port opened.");
        }

        private void CloseSerialPort()
        {
            if (_serialPort.IsOpen == true)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();

                _logger.Debug("Serial port closed.");
            }
        }

        private void ClearAllQueues()
        {
            lock (_generalLock)
            {
                if (_commandQ.Count > 0)
                {
                    _commandQ.Clear();
                }

                if (_responseQ.Count > 0)
                {
                    _responseQ.Clear();
                }
            }
        }

        private void InternalMessageProc()
        {
            int bytesToRead;
            int i;
            IFwLibMessage parsedMessage;
            FwLibParseState parseState;
            byte[] data = new byte[128];

            _logger.Debug("InternalMessageProc started");

            while (_messageLoop == true)
            {
                bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    _serialPort.Read(data, 0, bytesToRead);
                    for (i = 0; i < bytesToRead; i++)
                    {
                        parseState = _parser.Parse(data[i], out parsedMessage);
                        if (parseState == FwLibParseState.ParseOk)
                        {
                            lock (_generalLock)
                            {
                                switch (parsedMessage.MessageCategory)
                                {
                                    case FwLibMessageCategory.Response:
                                        int commandCount = 0;
                                        lock (_generalLock)
                                        {
                                            commandCount = _commandQ.Count;
                                        }

                                        if (commandCount > 0)
                                        {
                                            FwLibBinMessageResponse response = (FwLibBinMessageResponse)parsedMessage;
                                            _responseQ.Add(response);
                                        }
                                        else
                                        {
                                            _logger.Debug("Timeout response received");
                                        }
                                        break;

                                    case FwLibMessageCategory.Event:
                                        FwLibBinMessageEvent evt = (FwLibBinMessageEvent)parsedMessage;
                                        OnEventReceived?.Invoke(this, evt);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            _logger.Debug("InternalMessageProc stopped");
        }

        private IFwLibMessage ProcessCommand(FwLibBinMessageCommand command)
        {
            TimeSpan timeSpan;
            bool waitTimeExpired;
            IFwLibMessage response = null;
            IFwLibMessage message = (IFwLibMessage)command;

            FwLibBinPacketBuilder.BuildMessagePacket(ref message);
            lock(_generalLock)
            {
                _commandQ.Add(command);
            }
            
            for (int i = 0; i < command.MaxTryCount; i++)
            {
                _serialPort.Write(command.Buffer, 0, command.Buffer.Length);
                if (command.SendTimeHistory == null)
                {
                    command.SendTimeHistory = new List<DateTime>();
                }
                command.SendTimeHistory.Add(DateTime.UtcNow);

                waitTimeExpired = false;
                while (true)
                {
                    response = GetResponse(command);
                    if (response != null)
                    {
                        break;
                    }

                    timeSpan = DateTime.UtcNow - command.SendTimeHistory[i];
                    if (timeSpan.TotalMilliseconds > command.ResponseWaitTimeout)
                    {
                        waitTimeExpired = true;
                        Console.WriteLine($"Response wait time expired");
                        break;
                    }
                }

                if (response != null)
                {
                    break;
                }

                if ((waitTimeExpired == true) &&
                    (command.TryInterval > 0))
                {
                    Thread.Sleep(command.TryInterval);
                }
            }
            
            lock (_generalLock)
            {
                _commandQ.RemoveAt(0);
            }

            return response;
        }

        private FwLibBinMessageResponse GetResponse(FwLibBinMessageCommand command)
        {
            FwLibBinMessageResponse response = null;
            int count = 0;
            int i;

            lock (_generalLock)
            {
                count = _responseQ.Count;
            }

            if (count > 0)
            {
                Console.WriteLine($"Response count : {count}");
            }

            for (i = 0; i < count; i++)
            {
                if ((_responseQ[i].MessageId == command.MessageId) &&
                    (_responseQ[i].Header.SequenceNumber == command.Header.SequenceNumber))
                {
                    lock (_generalLock)
                    {
                        response = _responseQ[i];
                        _responseQ.RemoveAt(i);
                    }
                    break;
                }
            }

            return response;
        }
        #endregion
    }
}
