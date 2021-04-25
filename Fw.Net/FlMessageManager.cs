using Fl.Net;
using Fl.Net.Message;
using Fl.Net.Parser;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace Fw.Net
{
    public delegate void CbCommandResultReady(FwCommandMessageResult cmdResult);

    public class FlMessageManager
    {
        #region Private Data
        private ConcurrentQueue<IFlMessage> _commandQ = new ConcurrentQueue<IFlMessage>();
        private ConcurrentQueue<IFlMessage> _responseQ = new ConcurrentQueue<IFlMessage>();
        private ConcurrentQueue<IFlMessage> _eventQ = new ConcurrentQueue<IFlMessage>();
        private ConcurrentQueue<FwCommandMessageResult> _cmdResultQ = new ConcurrentQueue<FwCommandMessageResult>();
        private object _lock = new object();
        private FwStartStatus _startStatus = FwStartStatus.Stopped;
        private SerialPort _serialPort = new SerialPort();
        private Thread _messageThread = null;
        private bool _messageLoop = false;
        private FwMessageType _messageType = FwMessageType.Text;
        private FlBinParser _appBinParser = new FlBinParser();
        private FlTxtParser _appTxtParser = new FlTxtParser();
        #endregion

        #region Public Properties
        public FwStartStatus StartStatus => _startStatus;
        public FwMessageType MessageType => _messageType;
        public CbCommandResultReady OnCommandResultReady { get; set; }
        #endregion

        #region Constructors
        public FlMessageManager()
        {
            
        }
        #endregion

        #region Public Methods
        public void EnqueueCommand(IFlMessage command)
        {
            // TODO : Check if the command properties are valid.
            _commandQ.Enqueue(command);
        }

        //public IFlMessage DequeueCommand()
        //{
        //    _commandQ.TryDequeue(out IFlMessage command);

        //    return command;
        //}

        public int GetCommandCount()
        {
            return _commandQ.Count;
        }

        //public void EnqueueResponse(IFlMessage response)
        //{
        //    _responseQ.Enqueue(response);
        //}

        //public IFlMessage DequeueResponse()
        //{
        //    _responseQ.TryDequeue(out IFlMessage response);

        //    return response;
        //}

        //public int GetResponseCount()
        //{
        //    return _responseQ.Count;
        //}

        public FwCommandMessageResult DequeueCommandResult()
        {
            _cmdResultQ.TryDequeue(out FwCommandMessageResult result);

            return result;
        }

        public int GetCommandResultCount()
        {
            return _cmdResultQ.Count;
        }

        public void Start(MessageManagerSetting setting)
        {
            if (_startStatus != FwStartStatus.Stopped)
            {
                throw new Exception("Message manager is not stopped");
            }

            _startStatus = FwStartStatus.Starting;

            _messageType = setting.MessageType;
            OpenSerialPort(setting.SerialPortSetting);

            _messageLoop = true;
            _messageThread = new Thread(new ThreadStart(InternalMessageProc))
            {
                Priority = ThreadPriority.Highest
            };
            _messageThread.Start();

            _startStatus = FwStartStatus.Started;

            Log.Information("Message manager started");
        }

        public void Stop()
        {
            if (_startStatus != FwStartStatus.Started)
            {
                throw new Exception("Message manager is not started");
            }

            _startStatus = FwStartStatus.Stopping;

            _messageLoop = false;
            _messageThread.Join();

            CloseSerialPort();

            _commandQ.Clear();
            _responseQ.Clear();
            _cmdResultQ.Clear();
            _eventQ.Clear();

            _startStatus = FwStartStatus.Stopped;

            Log.Information("Message manager stopped");
        }
        #endregion

        #region Private Methods
        private void CloseSerialPort()
        {
            if (_serialPort.IsOpen == true)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();
            }
        }

        private void OpenSerialPort(SerialPortSetting portSetting)
        {
            CloseSerialPort();

            _serialPort.PortName = portSetting.PortName;
            _serialPort.BaudRate = portSetting.BaudRate;
            _serialPort.DataBits = portSetting.DataBits;
            _serialPort.Parity = portSetting.Parity;
            _serialPort.StopBits = portSetting.StopBits;

            _serialPort.Open();
        }

        private void InternalMessageProc()
        {
            int rxLen = 0;
            byte[] rxBuf = new byte[2048];

            while (_messageLoop)
            {
                rxLen = _serialPort.BytesToRead;
                if (rxLen <= 0)
                {
                    ProcessCommand();
                    Thread.Sleep(1);
                    continue;
                }

                if (rxLen > rxBuf.Length)
                {
                    rxLen = rxBuf.Length;
                }

                int actualRead = _serialPort.Read(rxBuf, 0, rxLen);

                if (actualRead > 0)
                {
                    if (_messageType == FwMessageType.Binary)
                    {
                        ProcessAppBinResponse(rxBuf, actualRead);
                    }
                    else if (_messageType == FwMessageType.Text)
                    {
                        ProcessAppTxtResponse(rxBuf, actualRead);
                    }
                }

                ProcessCommand();
            }
        }

        private void ProcessCommand()
        {
            if (_commandQ.Count <= 0)
            {
                return;
            }

            if (_commandQ.TryPeek(out IFlMessage command))
            {
                if (_responseQ.Count > 0)
                {
                    // TODO : Command + Response => CommandResult
                    BuildCommandResult(command);
                }
                else
                {
                    // TODO : (Re)Send a command packet.
                    SendCommandPacket(command);
                }
            }
        }

        private void SendCommandPacket(IFlMessage message)
        {
            IFlMessageCommand command = (IFlMessageCommand)message;
            bool sendPacket = false;

            if (command.MaxTryCount > 0)
            {
                // First sending.
                if (command.TryCount == 0)
                {
                    sendPacket = true;
                }
                else if (command.TryCount > 0)
                {
                    if (command.SendTimeHistory?.Count > 0)
                    {
                        TimeSpan spanTime = DateTime.UtcNow - command.SendTimeHistory[command.SendTimeHistory.Count - 1];
                        if (spanTime.TotalMilliseconds >= command.TryInterval)
                        {
                            if (command.TryCount < command.MaxTryCount)
                            {
                                sendPacket = true;
                            }
                            else
                            {
                                // Remove the command with no response.
                                if (_commandQ.TryDequeue(out IFlMessage expiredCommand))
                                {
                                    FwCommandMessageResult cmdResult = new FwCommandMessageResult()
                                    {
                                        Command = expiredCommand,
                                        Response = null,
                                        CreatedDate = DateTime.UtcNow
                                    };

                                    Log.Debug("No response for a command");
                                    OnCommandResultReady?.Invoke(cmdResult);
                                    //_cmdResultQ.Enqueue(cmdResult);
                                }
                            }
                        }
                    }
                    else
                    {
                        // TODO : If TryCount is greater than 0, the command should have SendTimeHistory.
                    }
                }
            }
            
            if (sendPacket)
            {
                _serialPort.Write(command.Buffer, 0, command.Buffer.Length);
                command.TryCount++;
                if (command.SendTimeHistory == null)
                {
                    command.SendTimeHistory = new List<DateTime>();
                }
                command.SendTimeHistory.Add(DateTime.UtcNow);
                Log.Debug("Command sent");
            }
        }

        private void BuildCommandResult(IFlMessage oldestCommand)
        {
            IFlMessage response;

            if (_responseQ.TryPeek(out response))
            {
                if (oldestCommand.MessageType == response.MessageType)
                {
                    if (oldestCommand.MessageId == response.MessageId)
                    {
                        if (_commandQ.TryDequeue(out IFlMessage command))
                        {
                            if (_responseQ.TryDequeue(out response))
                            {
                                FwCommandMessageResult cmdResult = new FwCommandMessageResult
                                {
                                    Command = command,
                                    Response = response,
                                    CreatedDate = DateTime.UtcNow
                                };

                                Log.Debug("Command result processed");
                                OnCommandResultReady?.Invoke(cmdResult);
                                //_cmdResultQ.Enqueue(cmdResult);
                            }
                            else
                            {
                                // TODO : Unable to deque a response that has a matched command.
                            }
                        }
                        else
                        {
                            // TODO : Unable to dequeue a command that has a matched response.
                        }
                    }
                    else
                    {
                        // TODO : Unmatched responce received(unexpected message ID).
                        _responseQ.TryDequeue(out response);
                    }
                }
                else
                {
                    // TODO : Unmatched response received(unexpected message category).
                    _responseQ.TryDequeue(out response);
                }
            }
        }

        private void ProcessAppBinResponse(byte[] rxBuf, int bytesToRead)
        {
            for (int i = 0; i < bytesToRead; i++)
            {
                FlParseState ret = _appBinParser.Parse(rxBuf[i], out IFlMessage response);
                if (ret == FlParseState.ParseOk)
                {
                    _responseQ.Enqueue(response);
                }
                else if (ret == FlParseState.ParseFail)
                {
                    Log.Debug("Application binary response parser fail");
                }
            }
        }

        private void ProcessAppTxtResponse(byte[] rxBuf, int actualRead)
        {
            for (int i = 0; i < actualRead; i++)
            {
                FlParseState ret = _appTxtParser.ParseResponseEvent(rxBuf[i], out IFlMessage responseOrEvt);
                if (ret == FlParseState.ParseOk)
                {
                    if (responseOrEvt.MessageCategory == FlMessageCategory.Response)
                    {
                        Log.Debug("Response received");
                        _responseQ.Enqueue(responseOrEvt);
                    }
                    else
                    {
                        _eventQ.Enqueue(responseOrEvt);
                    }
                }
                else if (ret == FlParseState.ParseFail)
                {
                    Log.Debug("Application binary response parser fail");
                }
            }
        }
        #endregion
    }
}
