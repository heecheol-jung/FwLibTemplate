using Fl.Net.Message;
using Fl.Net.PInvoke;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Fl.Net.Parser
{
    public class FlBinParser
    {
        enum ReceiveState
        {
            Stx,

            Uid,

            Length,

            Payload
        }

        #region Private Constants
        #endregion

        #region Private Data
        private byte[] _buf = new byte[FlConstant.FL_BIN_MSG_MAX_LENGTH];
        GCHandle _bufPinned;
        IntPtr _bufPtr;
        private int _bufPos = 0;
        private int _count = 0;
        private int _payloadLength = 0;
        private ReceiveState _receiveState;
        //private Stopwatch _stopWatch = new Stopwatch();
        private StringBuilder _sb = new StringBuilder();
        UInt16 _crc16 = 0;
        #endregion

        #region Public Properties
        public object Context { get; set; } = null;
        public FlParserRole Role { get; set; } = FlParserRole.Host;
        #endregion

        #region Constructors
        public FlBinParser()
        {
            _bufPinned = GCHandle.Alloc(_buf, GCHandleType.Pinned);
            _bufPtr = _bufPinned.AddrOfPinnedObject();
        }
        #endregion

        #region Destructors
        ~FlBinParser()
        {
            if (_bufPinned.IsAllocated == true)
            {
                _bufPinned.Free();
            }
        }
        #endregion

        #region Public Methods
        public FlParseState Parse(byte data, out IFlMessage message)
        {
            FlParseState ret = FlParseState.Parsing;

            message = null;

            switch (_receiveState)
            {
                case ReceiveState.Stx:
                    if (data == FlConstant.FL_BIN_MSG_STX)
                    {
                        _buf[_bufPos++] = data;
                        _receiveState = ReceiveState.Uid;
                        _count = 0;
                    }
                    else
                    {
                        ret = FlParseState.ParseFail;
                    }
                    break;

                case ReceiveState.Uid:
                    if (_count < FlConstant.FL_BIN_MSG_DEVICE_ID_LENGTH)
                    {
                        _buf[_bufPos++] = data;
                        _count++;
                        if (_count == FlConstant.FL_BIN_MSG_DEVICE_ID_LENGTH)
                        {
                            _count = 0;
                            _receiveState = ReceiveState.Length;
                        }
                    }
                    else
                    {
                        ret = FlParseState.ParseFail;
                    }
                    break;

                case ReceiveState.Length:
                    _buf[_bufPos++] = data;
                    _payloadLength = data;
                    _receiveState = ReceiveState.Payload;
                    break;

                case ReceiveState.Payload:
                    // Flag1 ~ FL_BIN_MSG_ETX
                    if (_count < _payloadLength)
                    {
                        // Check buffer overflow.
                        if (_bufPos < _buf.Length)
                        {
                            _buf[_bufPos++] = data;
                            _count++;
                            if (_count == _payloadLength)
                            {
                                _count = 0;
                                ret = FlParseState.ParseOk;
                            }
                        }
                        else
                        {
                            ret = FlParseState.ParseFail;
                        }
                    }
                    else
                    {
                        ret = FlParseState.ParseFail;
                    }
                    break;
            }

            if (ret != FlParseState.Parsing)
            {
                if (ret == FlParseState.ParseOk)
                {
                    if (Role == FlParserRole.Host)
                    {
                        // Process response.
                        //if (MakeResponseObject2(out message) != true)
                        if (MakeResponseEventObject(ref message) != true)
                        {
                            ret = FlParseState.ParseFail;
                        }
                    }
                    else if (Role == FlParserRole.Device)
                    {
                        // Process command.
                        if (MakeCommandObject(ref message) != true)
                        {
                            ret = FlParseState.ParseFail;
                        }
                    }
                }

                // Clear parser data.
                Clear();
            }

            return ret;
        }

        public void Clear()
        {
            Array.Clear(_buf, 0, _buf.Length);
            _bufPos = 0;
            _count = 0;
            _payloadLength = 0;
            _receiveState = ReceiveState.Stx;
            //private Stopwatch _stopWatch = new Stopwatch();
            _sb.Clear();
            _crc16 = 0;
        }
        #endregion

        #region Private Methods
        private bool CompareCrc(UInt16 expectedValue, UInt16 actualValue)
        {
            if (expectedValue == actualValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private unsafe bool MakeCommandObject(ref IFlMessage message)
        {
            bool ret = false;
            int crcDataLen = _bufPos - 4;
            int crcStartIndex = _bufPos - 3;
            List<object> arguments = null;
            FlMessageId messageId = FlMessageId.Unknown;
            ushort receivedCrc = 0;
            FlBinMsgFullStruct fullMsg = Marshal.PtrToStructure<FlBinMsgFullStruct>(_bufPtr);
            
            messageId = (FlMessageId)fullMsg.header.message_id;
            switch (messageId)
            {
                case FlMessageId.ReadHardwareVersion:
                case FlMessageId.ReadFirmwareVersion:
                case FlMessageId.Reset:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);
                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos-1]))
                        {
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.ReadGpio:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);
                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>
                            {
                                fullMsg.payload[0] // GPIO number
                            };
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.WriteGpio:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);
                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>
                            {
                                fullMsg.payload[0],  // GPIO number
                                fullMsg.payload[1]   // GPIO value
                            };
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.ReadTemperature:
                case FlMessageId.ReadHumidity:
                case FlMessageId.ReadTempAndHum:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);
                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>
                            {
                                fullMsg.payload[0] // Sensor number
                            };
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.BootMode:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);
                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>
                            {
                                fullMsg.payload[0]  // Boot mode
                            };
                            ret = true;
                        }
                    }
                    break;
            }

            if (ret == true)
            {
                FlBinMessageCommand command = new FlBinMessageCommand
                {
                    Arguments = arguments
                };
                command.Header.device_id = fullMsg.header.device_id;
                command.Header.length = fullMsg.header.length;
                command.Header.message_id = (FlMessageId)fullMsg.header.message_id;
                command.Header.flag1 = fullMsg.header.flag1;
                command.Header.flag2 = fullMsg.header.flag2;

                message = command;
            }

            return ret;
        }

        private bool MakeResponseEventObject(ref IFlMessage message)
        {
            FlBinMsgFullStruct fullMsg = Marshal.PtrToStructure<FlBinMsgFullStruct>(_bufPtr);
            
            switch (fullMsg.header.flag1.message_type)
            {
                case (byte)FlMessageCategory.Response:
                    return MakeResponseObject(fullMsg, ref message);

                case (byte)FlMessageCategory.Event:
                    return MakeEventObject(fullMsg, ref message);
            }
            
            return false;
        }

        private unsafe bool MakeEventObject(FlBinMsgFullStruct fullMsg, ref IFlMessage message)
        {
            bool ret = false;
            ushort receivedCrc = 0;
            int crcDataLen = _bufPos - 4;
            int crcStartIndex = _bufPos - 3;
            List<object> arguments = null;
            FlMessageId messageId = (FlMessageId)fullMsg.header.message_id;

            switch (messageId)
            {
                case FlMessageId.ButtonEvent:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);

                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>()
                            {
                                fullMsg.payload[0], // Button number
                                fullMsg.payload[1]  // Button value
                            };
                            ret = true;
                        }
                    }
                    break;
            }

            if (ret == true)
            {
                FlBinMessageEvent evt = new FlBinMessageEvent()
                {
                    Arguments = arguments,
                    ReceiveTime = DateTime.UtcNow
                };
                evt.Header.device_id = fullMsg.header.device_id;
                evt.Header.length = fullMsg.header.length;
                evt.Header.message_id = (FlMessageId)fullMsg.header.message_id;
                evt.Header.flag1 = fullMsg.header.flag1;
                evt.Header.flag2 = fullMsg.header.flag2;

                message = evt;
            }

            return ret;
        }

        private unsafe bool MakeResponseObject(FlBinMsgFullStruct fullMsg, ref IFlMessage message)
        {
            bool ret = false;
            ushort receivedCrc = 0;
            int crcDataLen = _bufPos - 4;
            int crcStartIndex = _bufPos - 3;
            List<object> arguments = null;
            FlMessageId messageId = (FlMessageId)fullMsg.header.message_id;

            // Payload size = header.length - 6;

            // stx header payload crc etx
            // crc : header payload, 
            // crc data length = received bytes - stx(1) - crc(2) - etx(1)
            switch (messageId)
            {
                case FlMessageId.ReadHardwareVersion:
                case FlMessageId.ReadFirmwareVersion:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);
                        
                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            string hwVerString = Encoding.ASCII.GetString(fullMsg.payload, fullMsg.header.length - 6);
                            arguments = new List<object>()
                            {
                                hwVerString
                            };
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.ReadGpio:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);

                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>()
                            {
                                fullMsg.payload[0], // GPIO number
                                fullMsg.payload[1]  // GPIO value
                            };
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.WriteGpio:
                case FlMessageId.BootMode:
                case FlMessageId.Reset:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);

                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.ReadTemperature:
                case FlMessageId.ReadHumidity:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);

                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>()
                            {
                                fullMsg.payload[0], // Sensor number
                                BitConverter.ToDouble(_buf, _bufPos - 3 - sizeof(double))   // Sensor value
                            };
                            ret = true;
                        }
                    }
                    break;

                case FlMessageId.ReadTempAndHum:
                    {
                        _crc16 = FlUtil.CRC16(_buf, 1, crcDataLen);
                        receivedCrc = BitConverter.ToUInt16(_buf, crcStartIndex);

                        if ((CompareCrc(_crc16, receivedCrc) == true) &&
                            (FlConstant.FL_BIN_MSG_ETX == _buf[_bufPos - 1]))
                        {
                            arguments = new List<object>()
                            {
                                fullMsg.payload[0], // Sensor number
                                BitConverter.ToDouble(_buf, _bufPos - 3 - sizeof(double)*2),    // Temperature
                                BitConverter.ToDouble(_buf, _bufPos - 3 - sizeof(double))       // Humidity
                            };
                            ret = true;
                        }
                    }
                    break;
            }

            if (ret == true)
            {
                FlBinMessageResponse response = new FlBinMessageResponse()
                {
                    Arguments = arguments,
                    ReceiveTime = DateTime.UtcNow
                };
                response.Header.device_id = fullMsg.header.device_id;
                response.Header.length = fullMsg.header.length;
                response.Header.message_id = (FlMessageId)fullMsg.header.message_id;
                response.Header.flag1 = fullMsg.header.flag1;
                response.Header.flag2 = fullMsg.header.flag2;

                message = response;
            }

            return ret;
        }
        #endregion
    }
}
