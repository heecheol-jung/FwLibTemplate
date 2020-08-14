using System;
using System.Collections.Generic;

namespace FwLib.Net
{
    public static class FwLibBinPacketBuilder
    {
        // STX(1 byte) + Device ID(4 byte) + Length(1 byte) + Message ID(1 byte) + Flag1(1 byte) + Flag2(1 byte) + CRC(2 byte) + ETX(1 byte)
        private const int BASE_FULL_LENGTH = 12;

        // Message ID(1 byte) + Flag1(1 byte) + Flag2(1 byte) + CRC(2 byte) + ETX(1 byte)
        private const int BASE_LENGTH = 6;

        private const int LENGTH_FIELD_INDEX = 5;

        public static void BuildMessagePacket(ref IFwLibMessage message)
        {
            List<byte> packet = new List<byte>();
            Type type;
            byte length = BASE_LENGTH;
            UInt16 crc;
            IFwLibBinMessage binMessage = (IFwLibBinMessage)message;

            // STX.
            packet.Add(FwLibConstant.BIN_MSG_STX);

            // Device ID.
            type = binMessage.Header.DeviceId.GetType();
            packet.AddRange(InternalProtoUtil.BuildBinPacketForPrimitiveArgument(binMessage.Header.DeviceId, Type.GetTypeCode(type)));

            // Length.
            packet.Add(length);

            // Message ID.
            packet.Add((byte)binMessage.Header.MessageId);

            // Flag1
            packet.Add(binMessage.Header.Flag1);

            // Flag2
            packet.Add(binMessage.Header.Flag2);

            // Arguments
            if (binMessage.Arguments?.Count > 0)
            {
                foreach (object arg in binMessage.Arguments)
                {
                    type = arg.GetType();
                    packet.AddRange(InternalProtoUtil.BuildBinPacketForPrimitiveArgument(arg, Type.GetTypeCode(type)));
                }
            }
            // Length value =   ( current packet length + CRC(2 byte) + ETX(1 byte) ) 
            //                - ( STX(1 byte) + Device ID(4 byte) + Length(1 byte))
            // => current packet length - 3
            packet[LENGTH_FIELD_INDEX] = (byte)(packet.Count - 3);

            // CRC16
            crc = FwLibUtil.CRC16(packet, 1, packet.Count - 1);
            packet.AddRange(InternalProtoUtil.BuildBinPacketForPrimitiveArgument(crc, crc.GetTypeCode()));

            // ETX
            packet.Add(FwLibConstant.BIN_MSG_ETX);

            binMessage.Buffer = packet.ToArray();
        }
    }
}
