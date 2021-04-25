using AppCommon.Net;
using Fl.Net;
using Fl.Net.Message;
using Fw.Net;
using HostSimulConsoleApp.Models;
using LiteDB;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace HostSimulConsoleApp
{
    internal class MessageManagerExample : IExample
    {
        uint _deviceId = 1;
        byte _seqNum = 0;
        FlMessageManager _flMsgMgr = new FlMessageManager();
        Dictionary<string, Action> _appMenu = new Dictionary<string, Action>();
        MessageManagerSetting _msgMgrSetting = new MessageManagerSetting()
        {
            SerialPortSetting = new SerialPortSetting()
            {
                PortName = "COM3",
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One
            }
        };
        LiteDatabase db;
        ILiteCollection<TxtCommandResult> col;

        public MessageManagerExample()
        {
            _appMenu.Add(AppConstant.STR_MESSAGE_TYPE, SetMessageType);
            _appMenu.Add(AppConstant.STR_COM_PORT_NAME, SetComPortName);
            _appMenu.Add(AppConstant.STR_START, Start);
            _appMenu.Add(AppConstant.STR_STOP, Stop);
            _appMenu.Add(AppConstant.STR_HW_VER, AppHwVer);
            _appMenu.Add(AppConstant.STR_FW_VER, AppFwVer);
            _appMenu.Add(AppConstant.STR_BMODE, AppBootBmode);
            _appMenu.Add(AppConstant.STR_RESET, AppReset);
            _appMenu.Add(AppConstant.STR_TEMPERATURE, AppReadTemperature);
            _appMenu.Add(AppConstant.STR_HUMIDITY, AppReadHumidity);
            _appMenu.Add(AppConstant.STR_TEMP_HUM, AppReadTempAndHum);
            _appMenu.Add(AppConstant.STR_READ_GPIO, AppReadGpio);
            _appMenu.Add(AppConstant.STR_WRITE_GPIO, AppWriteGpio);
            _appMenu.Add(AppCommonConstant.STR_QUIT, null);
        }

        private void AppWriteGpio()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.WriteGpio,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString(), // DeviceID
                        "3", // GPIO number
                        "1"  // GPIO value
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.WriteGpio,
                    Arguments = new List<object>()
                    {
                        (byte)3,
                        (byte)1
                    }
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppReadGpio()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadGpio,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString(), // DeviceID
                        "2"  // GPIO number
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadGpio,
                    Arguments = new List<object>()
                    {
                        (byte)2
                    }
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppReadTempAndHum()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadTempAndHum,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString(), // DeviceID
                        "1"  // Sensor number
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadTempAndHum,
                    Arguments = new List<object>()
                    {
                        (byte)1
                    }
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppReadHumidity()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadHumidity,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString(), // DeviceID
                        "1"  // Sensor number
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadHumidity,
                    Arguments = new List<object>()
                    {
                        (byte)1
                    }
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppReadTemperature()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadTemperature,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString(), // DeviceID
                        "1"  // Sensor number
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadTemperature,
                    Arguments = new List<object>()
                    {
                        (byte)1
                    }
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppReset()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.Reset,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString()  // DeviceID
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.Reset
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppBootBmode()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.BootMode,
                    Arguments = new List<object>()
                    {
                        _deviceId.ToString(), // DeviceID
                        FlConstant.FL_BMODE_BOOTLOADER.ToString()  // Bootloader
                    }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.BootMode,
                    Arguments = new List<object>()
                    {
                        (byte)FlConstant.FL_BMODE_BOOTLOADER
                    }
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppFwVer()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadFirmwareVersion,
                    Arguments = new List<object>()
                {
                    _deviceId.ToString() // DeviceID
                }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadFirmwareVersion
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        private void AppHwVer()
        {
            IFlMessage message = null;

            if (_flMsgMgr.MessageType == FwMessageType.Text)
            {
                message = new FlTxtMessageCommand()
                {
                    MessageId = FlMessageId.ReadHardwareVersion,
                    Arguments = new List<object>()
                {
                    _deviceId.ToString() // DeviceID
                }
                };
                FlTxtPacketBuilder.BuildMessagePacket(ref message);
            }
            else if (_flMsgMgr.MessageType == FwMessageType.Binary)
            {
                IFlBinMessage binMsg = new FlBinMessageCommand()
                {
                    MessageId = FlMessageId.ReadHardwareVersion
                };
                binMsg.Header.device_id = _deviceId;
                binMsg.Header.flag1.sequence_num = GetNextSeqNum();

                message = binMsg;
                FlBinPacketBuilder.BuildMessagePacket(ref message);
            }

            _flMsgMgr.EnqueueCommand(message);
        }

        public void Do()
        {
            bool loop = true;

            db = new LiteDatabase(@"protocol_history.db");
            col = db.GetCollection<TxtCommandResult>("txtcmdresults");

            SetTxtMsg();

            while (loop)
            {
                PrintMessageManagerInfo();
                AppCommonUtil.PrintMenu(_appMenu);
                string command = Console.ReadLine().ToLower(); ;
                if (command == AppCommonConstant.STR_QUIT)
                {
                    break;
                }
                if (_appMenu.ContainsKey(command) == true)
                {
                    if (_appMenu[command] != null)
                    {
                        _appMenu[command]();
                    }
                    else
                    {
                        Log.Warning($"Invalid command for {command}");
                    }
                }
                else
                {
                    Log.Warning("Unknown command!!!");
                }
            }

            db.Commit();
            Stop();
            Log.Information("MessageManagerExample done");
        }

        private void PrintMessageManagerInfo()
        {
            Console.WriteLine("=========================================================");
            Console.WriteLine($"Message type : {_flMsgMgr.MessageType}");
            Console.WriteLine($"COM port name : {_msgMgrSetting.SerialPortSetting?.PortName}");
            Console.WriteLine("=========================================================");
        }

        private void Stop()
        {
            if (_flMsgMgr.StartStatus == FwStartStatus.Started)
            {
                _flMsgMgr.Stop();
            }
        }

        private void Start()
        {
            if (_flMsgMgr.StartStatus != FwStartStatus.Stopped)
            {
                Console.WriteLine("Message manager is not stopped");
                return;
            }

            if (_flMsgMgr.OnCommandResultReady == null)
            {
                _flMsgMgr.OnCommandResultReady = OnCommandResultReady;
            }
            _flMsgMgr.Start(_msgMgrSetting);
        }

        private void OnCommandResultReady(FwCommandMessageResult cmdResult)
        {
            switch (cmdResult.Command.MessageType)
            {
                case FlMessageType.Text:
                    OnTxtCommandResultReady(cmdResult);
                    break;

                case FlMessageType.Binary:
                    OnBinCommandResultReady(cmdResult);
                    break;

                default:
                    Log.Error("MessageType should be specified");
                    break;
            }
        }

        private void OnBinCommandResultReady(FwCommandMessageResult cmdResult)
        {
            FlBinMessageCommand command = (FlBinMessageCommand)cmdResult.Command;
            int i;
            TimeSpan spanTime;

            Console.WriteLine($"Command : {command.MessageId}");
            Console.WriteLine($"Try count : {command.TryCount}");
            if (command.TryCount > 1)
            {
                for (i = 0; i < command.TryCount; i++)
                {
                    if ((i + 1) < command.TryCount)
                    {
                        spanTime = command.SendTimeHistory[i + 1] - command.SendTimeHistory[i];
                        Console.WriteLine($"Try interval : {spanTime.TotalMilliseconds}");
                    }
                }
            }

            if (command.TryCount > 0)
            {
                spanTime = cmdResult.CreatedDate - command.SendTimeHistory[0];
                Console.WriteLine($"Total processing time : {spanTime.TotalMilliseconds}");
            }

            if (cmdResult.Response != null)
            {
                if (cmdResult.Response.Arguments?.Count > 0)
                {
                    foreach (var item in cmdResult.Response.Arguments)
                    {
                        Console.WriteLine($"{item}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No response");
            }
        }

        private void OnTxtCommandResultReady(FwCommandMessageResult cmdResult)
        {
            FlTxtMessageCommand command = (FlTxtMessageCommand)cmdResult.Command;
            int i;
            TimeSpan spanTime;

            DateTime now = DateTime.UtcNow;
            TxtCommandResult txtCmdResult = new TxtCommandResult()
            {
                Command = command,
                Response = (FlTxtMessageResponse)cmdResult.Response,
                CreatedDate = cmdResult.CreatedDate,
                LogName = $"{now.Year}{now.Month.ToString("00")}{now.Day.ToString("00")} {now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}.{now.Millisecond.ToString("000")}"
            };
            col.Insert(txtCmdResult);

            Console.WriteLine($"Command : {command.MessageId}");
            Console.WriteLine($"Try count : {command.TryCount}");
            if (command.TryCount > 1)
            {
                for (i = 0; i < command.TryCount; i++)
                {
                    if ((i+1) < command.TryCount)
                    {
                        spanTime = command.SendTimeHistory[i + 1] - command.SendTimeHistory[i];
                        Console.WriteLine($"Try interval : {spanTime.TotalMilliseconds}");
                    }
                }
            }

            if (command.TryCount > 0)
            {
                spanTime = cmdResult.CreatedDate - command.SendTimeHistory[0];
                Console.WriteLine($"Total processing time : {spanTime.TotalMilliseconds}");
            }

            if (cmdResult.Response != null)
            {
                if (cmdResult.Response.Arguments?.Count > 0)
                {
                    foreach (var item in cmdResult.Response.Arguments)
                    {
                        Console.WriteLine($"{item}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No response");
            }
        }

        private void SetComPortName()
        {
            Console.Write("COM port name : ");
            string strLine = Console.ReadLine();
            if (string.IsNullOrEmpty(strLine) != true)
            {
                _msgMgrSetting.SerialPortSetting.PortName = strLine;
            }
            else
            {
                Console.WriteLine("Invalid COM port name!!!");
                _msgMgrSetting.SerialPortSetting.PortName = string.Empty;
            }
        }

        private void SetMessageType()
        {
            bool loop = true;
            Dictionary<string, Action> menus = new Dictionary<string, Action>();

            menus.Add(AppConstant.STR_BIN_MSG, SetBinMsg);
            menus.Add(AppConstant.STR_TXT_MSG, SetTxtMsg);
            menus.Add(AppCommonConstant.STR_QUIT, null);

            while (loop)
            {
                AppCommonUtil.PrintMenu(menus);
                string command = Console.ReadLine().ToLower();
                if (command == AppCommonConstant.STR_QUIT)
                {
                    break;
                }
                if (menus.ContainsKey(command) == true)
                {
                    if (menus[command] != null)
                    {
                        menus[command]();
                        break;
                    }
                    else
                    {
                        Log.Warning($"Invalid command for {command}");
                    }
                }
                else
                {
                    Log.Warning("Unknown command!!!");
                }
            }
        }

        private void SetBinMsg()
        {
            _msgMgrSetting.MessageType = FwMessageType.Binary;
            Log.Information("Binary message");
        }

        private void SetTxtMsg()
        {
            _msgMgrSetting.MessageType = FwMessageType.Text;
            Log.Information("Text message");
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
    }
}