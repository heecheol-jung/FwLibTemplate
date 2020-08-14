using System;
using System.Collections.Generic;
using System.Text;

namespace FwLib.Net
{
    // TODO : IFwLibParser
    public class FwLibTxtParser
    {
        enum ReceiveState
        {
            MessageId,
            DeviceId,
            Data,
            Tail
        }

        #region Private Data
        private byte[] _buf = new byte[FwLibConstant.TXT_MSG_MAX_LENGTH];
        private int _bufPos = 0;
        private byte[] _fullPacket = new byte[FwLibConstant.TXT_MSG_MAX_LENGTH];
        private int _fullPacketLength = 0;
        private ReceiveState _receiveState;
        //private Stopwatch _stopWatch = new Stopwatch();
        private StringBuilder _sb = new StringBuilder();
        private FwLibMessageId _msgId = FwLibMessageId.Unknown;
        private UInt32 _deviceId = 0;
        private List<object> _arguments = new List<object>();
        #endregion

        #region Public Properties
        public object Context { get; set; } = null;
        #endregion

        #region Public Methods
        public FwLibParseState ParseCommand(byte data, out IFwLibMessage message)
        {
            FwLibParseState ret = FwLibParseState.Parsing;

            message = null;
            if (_fullPacketLength < FwLibConstant.TXT_MSG_MAX_LENGTH)
            {
                _fullPacket[_fullPacketLength++] = data;
            }

            switch (_receiveState)
            {
                case ReceiveState.MessageId:
                    if (IsMsgIdChar(data) == true)
                    {
                        _buf[_bufPos++] = data;
                        if (_bufPos > FwLibConstant.TXT_MSG_ID_MAX_LEN)
                        {
                            // Too long for a message ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else if (data == FwLibConstant.TXT_MSG_ID_DEVICE_ID_DELIMITER)
                    {
                        _msgId = GetMessageId();
                        if (_msgId != FwLibMessageId.Unknown)
                        {
                            _receiveState = ReceiveState.DeviceId;
                            ClearReceiveBuffer();
                        }
                        else
                        {
                            // Unknown message Id.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else
                    {
                        // Message ID parging fail.
                        ret = FwLibParseState.ParseFail;
                    }
                    break;

                case ReceiveState.DeviceId:
                    if (IsDeviceIdChar(data) == true)
                    {
                        _buf[_bufPos++] = data;
                        if (_bufPos > FwLibConstant.TXT_DEVICE_ID_MAX_LEN)
                        {
                            // Too long for a device ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else if (data == FwLibConstant.TXT_MSG_ARG_DELIMITER)
                    {
                        if (SetDeviceId() == true)
                        {
                            if (IsCommandWithArgument(_msgId) == true)
                            {
                                _receiveState = ReceiveState.Data;
                                ClearReceiveBuffer();
                            }
                            else
                            {
                                // The command does not have argument(s).
                                ret = FwLibParseState.ParseFail;
                            }
                        }
                        else
                        {
                            // Invalid device ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else if (IsTail(data) == true)
                    {
                        if (SetDeviceId() == true)
                        {
                            _receiveState = ReceiveState.Tail;
                            ret = FwLibParseState.ParseOk;
                        }
                        else
                        {
                            // Invalid device ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else
                    {
                        // Device ID parsing fail.
                        ret = FwLibParseState.ParseFail;
                    }
                    break;

                case ReceiveState.Data:
                    if (IsTail(data) != true)
                    {
                        if (data != FwLibConstant.TXT_MSG_ARG_DELIMITER)
                        {
                            _buf[_bufPos++] = data;
                            if (_bufPos >= FwLibConstant.TXT_MSG_MAX_LENGTH)
                            {
                                // Too long for data.
                                ret = FwLibParseState.ParseFail;
                            }
                        }
                        else
                        {
                            if (ProcessCommandData() == true)
                            {
                                ClearReceiveBuffer();
                            }
                            else
                            {
                                // Message argument fail.
                                ret = FwLibParseState.ParseFail;
                            }
                        }
                    }
                    else
                    {
                        if (ProcessCommandData() == true)
                        {
                            _receiveState = ReceiveState.Tail;
                            ClearReceiveBuffer();
                            ret = FwLibParseState.ParseOk;
                        }
                        else
                        {
                            // Message argument fail.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    break;

                default:
                    // Invalid parser state.
                    ret = FwLibParseState.ParseFail;
                    break;
            }

            if (ret != FwLibParseState.Parsing)
            {
                if (ret == FwLibParseState.ParseOk)
                {
                    message = new FwLibTxtMessageCommand()
                    {
                        MessageId = _msgId,
                        DeviceId = _deviceId
                    };

                    if (_arguments.Count > 0)
                    {
                        message.Arguments = new List<object>();
                        while (_arguments.Count > 0)
                        {
                            message.Arguments.Add(_arguments[0]);
                            _arguments.RemoveAt(0);
                        }
                    }
                }

                Clear();
            }

            return ret;
        }

        public FwLibParseState ParseResponseEvent(byte data, out IFwLibMessage message)
        {
            FwLibParseState ret = FwLibParseState.Parsing;

            message = null;
            if (_fullPacketLength < FwLibConstant.TXT_MSG_MAX_LENGTH)
            {
                _fullPacket[_fullPacketLength++] = data;
            }

            switch (_receiveState)
            {
                case ReceiveState.MessageId:
                    if (IsMsgIdChar(data) == true)
                    {
                        _buf[_bufPos++] = data;
                        if (_bufPos > FwLibConstant.TXT_MSG_ID_MAX_LEN)
                        {
                            // Too long for a message ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else if (data == FwLibConstant.TXT_MSG_ID_DEVICE_ID_DELIMITER)
                    {
                        _msgId = GetMessageId();
                        if (_msgId != FwLibMessageId.Unknown)
                        {
                            _receiveState = ReceiveState.DeviceId;
                            ClearReceiveBuffer();
                        }
                        else
                        {
                            // Unknown message ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else
                    {
                        // Message ID parsing fail.
                        ret = FwLibParseState.ParseFail;
                    }
                    break;

                case ReceiveState.DeviceId:
                    if (IsDeviceIdChar(data) == true)
                    {
                        _buf[_bufPos++] = data;
                        if (_bufPos > FwLibConstant.TXT_DEVICE_ID_MAX_LEN)
                        {
                            // Too long for a device ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else if (data == FwLibConstant.TXT_MSG_ARG_DELIMITER)
                    {
                        if (SetDeviceId() == true)
                        {
                            _receiveState = ReceiveState.Data;
                            ClearReceiveBuffer();
                        }
                        else
                        {
                            // Invalid device ID.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else
                    {
                        // Device ID parsing fail.
                        ret = FwLibParseState.ParseFail;
                    }
                    break;

                case ReceiveState.Data:
                    if (IsTail(data) != true)
                    {
                        if (data != FwLibConstant.TXT_MSG_ARG_DELIMITER)
                        {
                            _buf[_bufPos++] = data;
                            if (_bufPos >= FwLibConstant.TXT_MSG_MAX_LENGTH)
                            {
                                // Too long for data.
                                ret = FwLibParseState.ParseFail;
                            }
                        }
                        else
                        {
                            if (ProcessResponseEventData() == true)
                            {
                                ClearReceiveBuffer();
                            }
                            else
                            {
                                // Invalid arguemnt.
                                ret = FwLibParseState.ParseFail;
                            }
                        }
                    }
                    else
                    {
                        if (ProcessResponseEventData() == true)
                        {
                            _receiveState = ReceiveState.Tail;
                            ClearReceiveBuffer();
                            ret = FwLibParseState.ParseOk;
                        }
                        else
                        {
                            // Invalid arguemnt.
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    break;

                default:
                    // Unknown parser state.
                    ret = FwLibParseState.ParseFail;
                    break;
            }

            if (ret != FwLibParseState.Parsing)
            {
                if (ret == FwLibParseState.ParseOk)
                {
                    if (_msgId == FwLibMessageId.ButtonEvent)
                    {
                        message = new FwLibTxtMessageEvent()
                        {
                            MessageId = _msgId,
                            DeviceId = _deviceId,
                            ReceiveTime = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        message = new FwLibTxtMessageResponse()
                        {
                            MessageId = _msgId,
                            DeviceId = _deviceId,
                            ReceiveTime = DateTime.UtcNow
                        };
                    }

                    if (_arguments.Count > 0)
                    {
                        message.Arguments = new List<object>();
                        while (_arguments.Count > 0)
                        {
                            message.Arguments.Add(_arguments[0]);
                            _arguments.RemoveAt(0);
                        }
                    }
                }

                Clear();
            }

            return ret;
        }

        public void Clear()
        {
            _msgId = FwLibMessageId.Unknown;
            _deviceId = 0;
            _bufPos = 0;
            Array.Clear(_buf, 0, _buf.Length);
            _fullPacketLength = 0;
            Array.Clear(_fullPacket, 0, _fullPacket.Length);
            _receiveState = ReceiveState.MessageId;
            _arguments.Clear();
            _sb.Clear();
        }
        #endregion

        #region Private Methods
        private bool ProcessCommandData()
        {
            // Too many arguments.
            if (_arguments.Count >= FwLibConstant.TXT_MSG_MAX_ARG_COUNT)
            {
                return false;
            }

            if (_msgId == FwLibMessageId.ReadGpio)
            {
                if (_arguments.Count < 1)
                {
                    // GPIO number.
                    if (GetByteData(out byte byteData) == true)
                    {
                        _arguments.Add(byteData);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (_msgId == FwLibMessageId.WriteGpio)
            {
                // GPIO number and GPIO value.
                if (_arguments.Count < 2)
                {
                    if (GetByteData(out byte byteData) == true)
                    {
                        _arguments.Add(byteData);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool ProcessResponseEventData()
        {
            if (_arguments.Count >= FwLibConstant.TXT_MSG_MAX_ARG_COUNT)
            {
                return false;
            }

            if ((_msgId == FwLibMessageId.ReadHardwareVersion) ||
                (_msgId == FwLibMessageId.ReadFirmwareVersion))
            {
                if (_arguments.Count < 2)
                {
                    // Return code.
                    if (_arguments.Count == 0)
                    {
                        if (GetByteData(out byte byteData) == true)
                        {
                            _arguments.Add(byteData);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    // Version string.
                    else if (_arguments.Count == 1)
                    {
                        if (GetStringData(out string stringData) == true)
                        {
                            _arguments.Add(stringData);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else if (_msgId == FwLibMessageId.ReadGpio)
            {
                // Return code, GPIO number, GPIO value.
                if (_arguments.Count < 3)
                {
                    if (GetByteData(out byte byteData) == true)
                    {
                        _arguments.Add(byteData);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (_msgId == FwLibMessageId.WriteGpio)
            {
                // Return code.
                if (_arguments.Count < 1)
                {
                    if (GetByteData(out byte byteData) == true)
                    {
                        _arguments.Add(byteData);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (_msgId == FwLibMessageId.ButtonEvent)
            {
                // Button number, button status(pressed)
                if (_arguments.Count < 2)
                {
                    if (GetByteData(out byte byteData) == true)
                    {
                        _arguments.Add(byteData);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool GetStringData(out string stringData)
        {
            _sb.Clear();

            for (int i = 0; i < _bufPos; i++)
            {
                _sb.Append((char)_buf[i]);
            }

            stringData = _sb.ToString();

            return true;
        }

        private bool GetByteData(out byte byteData)
        {
            _sb.Clear();

            for (int i = 0; i < _bufPos; i++)
            {
                _sb.Append((char)_buf[i]);
            }

            return byte.TryParse(_sb.ToString(), out byteData);
        }

        private bool IsCommandWithArgument(FwLibMessageId msgId)
        {
            switch (msgId)
            {
                case FwLibMessageId.ReadGpio:
                case FwLibMessageId.WriteGpio:
                    return true;
            }
            return false;
        }

        private bool GetUInt32Data(out UInt32 uint32Data)
        {
            _sb.Clear();

            for (int i = 0; i < _bufPos; i++)
            {
                _sb.Append((char)_buf[i]);
            }

            return UInt32.TryParse(_sb.ToString(), out uint32Data);
        }

        private bool SetDeviceId()
        {
            if (GetUInt32Data(out UInt32 deviceId) == true)
            {
                _deviceId = deviceId;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsTail(byte data)
        {
            if (data == FwLibConstant.TXT_MSG_TAIL)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsDeviceIdChar(byte data)
        {
            if ((data >= FwLibConstant.TXT_DEVICE_ID_MIN_CHAR) &&
                (data <= FwLibConstant.TXT_DEVICE_ID_MAX_CHAR))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClearReceiveBuffer()
        {
            _bufPos = 0;
            Array.Clear(_buf, 0, _buf.Length);
        }

        private FwLibMessageId GetMessageId()
        {
            FwLibMessageId messageId = FwLibMessageId.Unknown;

            foreach (var item in FwLibConstant.StringToMessageIdTable)
            {
                if (_bufPos == item.Key.Length)
                {
                    int i;

                    for (i = 0; i < _bufPos; i++)
                    {
                        if (_buf[i] != (byte)item.Key[i])
                        {
                            break;
                        }
                    }

                    if (i == _bufPos)
                    {
                        messageId = item.Value;
                        break;
                    }
                }
            }

            return messageId;
        }

        bool IsMsgIdChar(byte data)
        {
            if ((data >= FwLibConstant.TXT_MSG_ID_MIN_CHAR) &&
                (data <= FwLibConstant.TXT_MSG_ID_MAX_CHAR))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
