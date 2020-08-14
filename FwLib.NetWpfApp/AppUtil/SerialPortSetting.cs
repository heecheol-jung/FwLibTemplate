﻿using System.IO.Ports;

namespace FwLib.NetWpfApp.AppUtil
{
    public class SerialPortSetting
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
    }
}
