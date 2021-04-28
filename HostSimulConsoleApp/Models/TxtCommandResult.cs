using Fl.Net.Message;
using System;

namespace HostSimulConsoleApp.Models
{
    public class TxtCommandResult
    {
        public Int64 Id { get; set; }
        public string LogName { get; set; }
        public FlTxtMessageCommand Command { get; set; }
        public FlTxtMessageResponse Response { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
