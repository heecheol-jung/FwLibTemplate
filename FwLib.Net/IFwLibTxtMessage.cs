using System;

namespace FwLib.Net
{
    public interface IFwLibTxtMessage : IFwLibMessage
    {
        UInt32 DeviceId { get; set; }
    }
}
