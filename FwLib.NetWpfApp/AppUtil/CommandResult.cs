﻿using Fl.Net.Message;

namespace FwLib.NetWpfApp.AppUtil
{
    public class CommandResult
    {
        public IFlMessage Command { get; set; }
        public IFlMessage Response { get; set; }
    }
}
