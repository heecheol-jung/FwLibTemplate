using AppCommon.Net;
using Fl.Net;
using Fl.Net.Message;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HostSimulConsoleApp
{
    internal class HostSimulatorExample1 : IExample
    {
        HostSimulator _hostSimul = new HostSimulator();
        Dictionary<string, Action> _appBinMenu = new Dictionary<string, Action>();
        Dictionary<string, Action> _appTxtMenu = new Dictionary<string, Action>();

        public HostSimulatorExample1()
        {
            _appBinMenu.Add(AppConstant.STR_MESSAGE_TYPE, SetMessageType);
            _appBinMenu.Add(AppConstant.STR_COM_PORT_NAME, SetComPortName);
            _appBinMenu.Add(AppConstant.STR_START, Start);
            _appBinMenu.Add(AppConstant.STR_STOP, Stop);
            _appBinMenu.Add(AppConstant.STR_HW_VER, AppBinHwVer);
            _appBinMenu.Add(AppConstant.STR_FW_VER, AppBinFwVer);
            _appBinMenu.Add(AppConstant.STR_BMODE, AppBinBootBmode);
            _appBinMenu.Add(AppConstant.STR_RESET, AppBinReset);
            _appBinMenu.Add(AppConstant.STR_TEMPERATURE, AppBinReadTemperature);
            _appBinMenu.Add(AppConstant.STR_HUMIDITY, AppBinReadHumidity);
            _appBinMenu.Add(AppConstant.STR_TEMP_HUM, AppBinReadTempAndHum);
            _appBinMenu.Add(AppConstant.STR_READ_GPIO, AppBinReadGpio);
            _appBinMenu.Add(AppConstant.STR_WRITE_GPIO, AppBinWriteGpio);
            _appBinMenu.Add(AppCommonConstant.STR_QUIT, null);

            _appTxtMenu.Add(AppConstant.STR_MESSAGE_TYPE, SetMessageType);
            _appTxtMenu.Add(AppConstant.STR_COM_PORT_NAME, SetComPortName);
            _appTxtMenu.Add(AppConstant.STR_START, Start);
            _appTxtMenu.Add(AppConstant.STR_STOP, Stop);
            _appTxtMenu.Add(AppConstant.STR_HW_VER, AppTxtHwVer);
            _appTxtMenu.Add(AppConstant.STR_FW_VER, AppTxtFwVer);
            _appTxtMenu.Add(AppConstant.STR_BMODE, AppTxtBootBmode);
            _appTxtMenu.Add(AppConstant.STR_RESET, AppTxtReset);
            _appTxtMenu.Add(AppConstant.STR_TEMPERATURE, AppTxtReadTemperature);
            _appTxtMenu.Add(AppConstant.STR_HUMIDITY, AppTxtReadHumidity);
            _appTxtMenu.Add(AppConstant.STR_TEMP_HUM, AppTxtReadTempAndHum);
            _appTxtMenu.Add(AppConstant.STR_READ_GPIO, AppTxtReadGpio);
            _appTxtMenu.Add(AppConstant.STR_WRITE_GPIO, AppTxtWriteGpio);
            _appTxtMenu.Add(AppCommonConstant.STR_QUIT, null);
        }

        public void Do()
        {
            bool loop = true;
            Dictionary<string, Action> menu = null;

            _hostSimul.ComPortName = "COM3";
            SetBinMsg();

            while (loop)
            {
                if (_hostSimul.MessageType == MessageType.Text)
                {
                    menu = _appTxtMenu;
                }
                else if (_hostSimul.MessageType == MessageType.Binary)
                {
                    menu = _appBinMenu;
                }
                else
                {
                    Log.Warning("Unknown message type");
                }

                if (menu == null)
                {
                    Log.Warning("No menus available");
                    break;
                }

                PrintHostSimulatorInfo(_hostSimul);
                AppCommonUtil.PrintMenu(menu);
                string command = Console.ReadLine().ToLower(); ;
                if (command == AppCommonConstant.STR_QUIT)
                {
                    break;
                }
                if (menu.ContainsKey(command) == true)
                {
                    if (menu[command] != null)
                    {
                        menu[command]();
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

            _hostSimul.Stop();
            Log.Information("HostSimulExample1 done");
        }

        #region General Private Methods
        private void PrintHostSimulatorInfo(HostSimulator hostSimul)
        {
            Console.WriteLine("==============================================================");

            if (_hostSimul.MessageType == MessageType.Binary)
            {
                Console.WriteLine("Message type : Binary message");
            }
            else if (_hostSimul.MessageType == MessageType.Text)
            {
                Console.WriteLine("Message type : Text message");
            }

            if (string.IsNullOrEmpty(_hostSimul.ComPortName) != true)
            {
                Console.WriteLine($"COM port : {_hostSimul.ComPortName}");
            }
            else
            {
                Console.WriteLine("COM port : NOT SET");
            }

            if (_hostSimul.IsComPortOpened() == true)
            {
                Console.WriteLine("COM port opened");
            }
            else
            {
                Console.WriteLine("COM port closed");
            }

            Console.WriteLine("==============================================================");
        }

        private void SetComPortName()
        {
            Console.Write("COM port name : ");
            string strLine = Console.ReadLine();
            if (string.IsNullOrEmpty(strLine) != true)
            {
                _hostSimul.ComPortName = strLine;
            }
            else
            {
                Console.WriteLine("Invalid COM port name!!!");
                _hostSimul.ComPortName = string.Empty;
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
            if (_hostSimul.MessageType != MessageType.Binary)
            {
                _hostSimul.AppBinParser.Clear();
            }
            _hostSimul.MessageType = MessageType.Binary;
            Log.Information("Binary message");
        }

        private void SetTxtMsg()
        {
            if (_hostSimul.MessageType != MessageType.Text)
            {
                _hostSimul.AppTxtParser.Clear();
            }
            _hostSimul.MessageType = MessageType.Text;
            Log.Information("Text message");
        }

        private void Start()
        {

            if ((_hostSimul.MessageType != MessageType.Binary) &&
                (_hostSimul.MessageType != MessageType.Text))
            {
                Log.Warning("Invalid message type{FirmwareType}", _hostSimul.MessageType);
                return;
            }

            if (string.IsNullOrEmpty(_hostSimul.ComPortName) == true)
            {
                Log.Warning("Invalid COM port name");
                return;
            }

            if (_hostSimul.Status == StartStatus.Stopped)
            {
                _hostSimul.Start();
            }
            else
            {
                Log.Warning("Host simulator not stopped{Status}", _hostSimul.Status);
            }
        }

        private void Stop()
        {
            if (_hostSimul.Status == StartStatus.Started)
            {
                _hostSimul.Stop();
            }
            else
            {
                Log.Warning("Host simulator not started{Status}", _hostSimul.Status);
            }
        }

        private void WaitForResponse()
        {
            int i = 0;

            while (true)
            {
                if (_hostSimul.ResponseReceived == true)
                {
                    Log.Information("Response received");
                    break;
                }
                Thread.Sleep(100);
                i++;
                if (i == 10)
                {
                    Log.Information("No response");
                    break;
                }
            }
        }

        private bool CheckStartStatus()
        {
            if ((_hostSimul.MessageType != MessageType.Binary) &&
                (_hostSimul.MessageType != MessageType.Text))
            {
                Log.Warning($"Invalid message type : {_hostSimul.MessageType}");
                return false;
            }
            

            if (string.IsNullOrEmpty(_hostSimul.ComPortName) == true)
            {
                Log.Warning("COM port is NOT SET");
                return false;
            }
            
            if (_hostSimul.IsComPortOpened() != true)
            {
                Log.Warning("COM port is NOT OPENED");
                return false;
            }

            return true;
        }
        #endregion

        #region Application Binary Protocol
        private void AppBinHwVer()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadHardwareVersion
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinFwVer()
        {
            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadFirmwareVersion
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinBootBmode()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.BootMode,
                Arguments = new List<object>()
                {
                    (byte)FlConstant.FL_BMODE_BOOTLOADER
                }
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinReset()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.Reset
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinReadTemperature()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    (byte)1
                }
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinReadHumidity()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    (byte)1
                }
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinReadTempAndHum()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    (byte)1
                }
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinReadGpio()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    (byte)1
                }
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppBinWriteGpio()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlBinMessage binMsg = new FlBinMessageCommand()
            {
                MessageId = FlMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    (byte)1,
                    (byte)1
                }
            };
            binMsg.Header.device_id = 1;
            binMsg.Header.flag1.sequence_num = _hostSimul.GetNextSeqNum();

            IFlMessage message = binMsg;
            FlBinPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }
        #endregion

        #region Application Text Protocol
        private void AppTxtHwVer()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadHardwareVersion,
                Arguments = new List<object>()
                {
                    "1" // DeviceID
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtFwVer()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadFirmwareVersion,
                Arguments = new List<object>()
                {
                    "1" // DeviceID
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtBootBmode()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.BootMode,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1"  // Bootloader
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtReset()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.Reset,
                Arguments = new List<object>()
                {
                    "1"  // DeviceID
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtReadTemperature()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadTemperature,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1"  // Sensor number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtReadHumidity()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadHumidity,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1"  // Sensor number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtReadTempAndHum()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadTempAndHum,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1"  // Sensor number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtReadGpio()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.ReadGpio,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "2"  // GPIO number
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }

        private void AppTxtWriteGpio()
        {
            if (CheckStartStatus() != true)
            {
                return;
            }

            IFlMessage message = new FlTxtMessageCommand()
            {
                MessageId = FlMessageId.WriteGpio,
                Arguments = new List<object>()
                {
                    "1", // DeviceID
                    "1", // GPIO number
                    "1"  // GPIO value
                }
            };

            FlTxtPacketBuilder.BuildMessagePacket(ref message);

            _hostSimul.ResponseReceived = false;
            _hostSimul.SendPacket(message.Buffer);

            WaitForResponse();
        }
        #endregion
    }
}