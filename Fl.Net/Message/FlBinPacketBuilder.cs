using System;
using System.Collections.Generic;

namespace Fl.Net.Message
{
    public static class FlBinPacketBuilder
    {
        // Message ID(1 byte) + Flag1(1 byte) + Flag2(1 byte) + CRC(2 byte) + ETX(1 byte)
        private const int BASE_LENGTH = 6;

        private const int LENGTH_FIELD_INDEX = 5;

        public static void BuildMessagePacket(ref IFlMessage binMessage)
        {
            List<byte> packet = new List<byte>();
            Type type;
            byte length = BASE_LENGTH;
            UInt16 crc;
            IFlBinMessage binMsg = (IFlBinMessage)binMessage;

            // TODO : Check message type(text).
            // TODO : Check message category.
            // TODO : Check DeviceID

            // STX.
            packet.Add(FlConstant.FL_BIN_MSG_STX);

            // Device ID.
            type = binMsg.Header.device_id.GetType();
            packet.AddRange(InternalProtoUtil.BuildBinPacketForPrimitiveArgument(binMsg.Header.device_id, Type.GetTypeCode(type)));

            // Length
            packet.Add(length);

            // Message ID.
            packet.Add((byte)binMsg.Header.message_id);

            // Flag1
            packet.Add(binMsg.Header.flag1.BitField1);

            // Flag2
            packet.Add(binMsg.Header.flag2.BitField1);

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
            crc = FlUtil.CRC16(packet, 1, packet.Count - 1);
            packet.AddRange(InternalProtoUtil.BuildBinPacketForPrimitiveArgument(crc, crc.GetTypeCode()));

            // ETX
            packet.Add(FlConstant.FL_BIN_MSG_ETX);

            binMsg.Buffer = packet.ToArray();
        }
    }
}
