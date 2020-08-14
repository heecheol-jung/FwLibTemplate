using System;
using System.Collections.Generic;
using System.Text;

namespace FwLib.Net
{
    public static class FwLibTxtPacketBuilder
    {
        public static void BuildMessagePacket(ref IFwLibMessage message)
        {
            List<byte> packet = new List<byte>();

            if (message.MessageId == FwLibMessageId.Unknown)
            {
                throw new ArgumentException("Invalid message ID.");
            }

            // Command string packet.
            packet.AddRange(Encoding.ASCII.GetBytes(FwLibConstant.MessageIdToStringTable[message.MessageId]));

            // Delimiter for a command and a device ID.
            packet.Add((byte)FwLibConstant.TXT_MSG_ID_DEVICE_ID_DELIMITER);

            //  Device ID.
            packet.AddRange(InternalProtoUtil.BuildAsciiPacketForUInt32(((IFwLibTxtMessage)message).DeviceId));

            // Arguments.
            if (message.Arguments?.Count > 0)
            {
                Type type;

                // ','
                packet.Add((byte)FwLibConstant.TXT_MSG_ARG_DELIMITER);

                for (int i = 0; i < message.Arguments.Count; i++)
                {
                    type = message.Arguments[i].GetType();
                    if (InternalProtoUtil.IsPrimitiveTyps(type) == true)
                    {
                        packet.AddRange(InternalProtoUtil.BuildAsciiPacketForPrimitiveArgument(message.Arguments[i], Type.GetTypeCode(type)));
                    }

                    if (i != (message.Arguments.Count - 1))
                    {
                        // ','
                        packet.Add((byte)FwLibConstant.TXT_MSG_ARG_DELIMITER);
                    }
                }
            }

            // '\n'
            packet.Add((byte)FwLibConstant.TXT_MSG_TAIL);

            message.Buffer = packet.ToArray();
        }
    }
}
