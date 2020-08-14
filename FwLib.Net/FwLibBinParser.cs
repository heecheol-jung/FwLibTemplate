using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FwLib.Net
{
    // TODO : IFwLibParser
    public class FwLibBinParser
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
        private byte[] _buf = new byte[FwLibConstant.BinMessageBufSize];
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
        public FwLibParserRole Role { get; set; } = FwLibParserRole.Host;
        #endregion

        #region Constructors
        public FwLibBinParser()
        {
            _bufPinned = GCHandle.Alloc(_buf, GCHandleType.Pinned);
            _bufPtr = _bufPinned.AddrOfPinnedObject();
        }
        #endregion

        #region Destructors
        ~FwLibBinParser()
        {
            if (_bufPinned.IsAllocated == true)
            {
                _bufPinned.Free();
            }
        }
        #endregion

        #region Public Methods
        public FwLibParseState Parse(byte data, out IFwLibMessage message)
        {
            FwLibParseState ret = FwLibParseState.Parsing;

            message = null;

            switch (_receiveState)
            {
                case ReceiveState.Stx:
                    if (data == FwLibConstant.BIN_MSG_STX)
                    {
                        _buf[_bufPos++] = data;
                        _receiveState = ReceiveState.Uid;
                        _count = 0;
                    }
                    else
                    {
                        ret = FwLibParseState.ParseFail;
                    }
                    break;

                case ReceiveState.Uid:
                    if (_count < FwLibConstant.BIN_MSG_DEVICE_ID_LENGTH)
                    {
                        _buf[_bufPos++] = data;
                        _count++;
                        if (_count == FwLibConstant.BIN_MSG_DEVICE_ID_LENGTH)
                        {
                            _count = 0;
                            _receiveState = ReceiveState.Length;
                        }
                    }
                    else
                    {
                        ret = FwLibParseState.ParseFail;
                    }
                    break;

                case ReceiveState.Length:
                    _buf[_bufPos++] = data;
                    _payloadLength = data;
                    _receiveState = ReceiveState.Payload;
                    break;

                case ReceiveState.Payload:
                    // Flag1 ~ ETX
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
                                ret = FwLibParseState.ParseOk;
                            }
                        }
                        else
                        {
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else
                    {
                        ret = FwLibParseState.ParseFail;
                    }
                    break;
            }

            if (ret != FwLibParseState.Parsing)
            {
                if (ret == FwLibParseState.ParseOk)
                {
                    if (Role == FwLibParserRole.Host)
                    {
                        // Process response.
                        if (MakeResponseEventObject(ref message) != true)
                        {
                            ret = FwLibParseState.ParseFail;
                        }
                    }
                    else if (Role == FwLibParserRole.Device)
                    {
                        // Process command.
                        if (MakeCommandObject(ref message) != true)
                        {
                            ret = FwLibParseState.ParseFail;
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
            if (expectedValue == FwLibUtil.SwapUInt16(actualValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool MakeCommandObject(ref IFwLibMessage message)
        {
            bool ret = false;
            List<object> arguments = null;
            FwLibMessageId messageId = FwLibMessageId.Unknown;

            FwLibBinMessageHeaderStruct header = Marshal.PtrToStructure<FwLibBinMessageHeaderStruct>(_bufPtr);
            messageId = (FwLibMessageId)header.MessageId;

            switch (messageId)
            {
                case FwLibMessageId.ReadHardwareVersion:
                case FwLibMessageId.ReadFirmwareVersion:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 8);
                        FwLibBinMessageStruct msg = Marshal.PtrToStructure<FwLibBinMessageStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            ret = true;
                        }
                    }
                    break;

                case FwLibMessageId.ReadGpio:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 9);
                        FwLibBinMessageReadGpioCommandStruct msg = Marshal.PtrToStructure<FwLibBinMessageReadGpioCommandStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            arguments = new List<object>
                            {
                                (byte)msg.GpioNumber
                            };
                            ret = true;
                        }
                    }
                    break;

                case FwLibMessageId.WriteGpio:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 10);
                        FwLibBinMessageWriteGpioCommandStruct msg = Marshal.PtrToStructure<FwLibBinMessageWriteGpioCommandStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            arguments = new List<object>
                            {
                                (byte)msg.GpioNumber,
                                (byte)msg.GpioValue
                            };
                            ret = true;
                        }
                    }
                    break;
            }

            if (ret == true)
            {
                FwLibBinMessageCommand command = new FwLibBinMessageCommand
                {
                    Arguments = arguments
                };
                command.Header.DeviceId = FwLibUtil.SwapUInt32(header.DeviceId);
                command.Header.Length = header.Length;
                command.Header.MessageId = (FwLibMessageId)header.MessageId;
                command.Header.Flag1 = header.Flag1;
                command.Header.Flag2 = header.Flag2;

                message = command;
            }

            return ret;
        }

        private bool MakeResponseEventObject(ref IFwLibMessage message)
        {
            FwLibBinMessageHeaderStruct header = Marshal.PtrToStructure<FwLibBinMessageHeaderStruct>(_bufPtr);

            if (header.MessageType == FwLibMessageCategory.Response)
            {
                return MakeResponseObject(header, ref message);
            }
            else if (header.MessageType == FwLibMessageCategory.Event)
            {
                return MakeEventObject(header, ref message);
            }
            else
            {
                return false;
            }
        }

        private bool MakeEventObject(FwLibBinMessageHeaderStruct header, ref IFwLibMessage message)
        {
            bool ret = false;
            List<object> arguments = null;
            FwLibMessageId messageId = (FwLibMessageId)header.MessageId;

            switch (messageId)
            {
                case FwLibMessageId.ButtonEvent:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 10);
                        FwLibBinMessageButtonEventStruct msg = Marshal.PtrToStructure<FwLibBinMessageButtonEventStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            arguments = new List<object>()
                            {
                                msg.ButtonNumber,
                                msg.ButtonValue
                            };
                            ret = true;
                        }
                    }
                    break;
            }

            if (ret == true)
            {
                FwLibBinMessageEvent evt = new FwLibBinMessageEvent()
                {
                    Arguments = arguments,
                    ReceiveTime = DateTime.Now
                };
                evt.Header.DeviceId = FwLibUtil.SwapUInt32(header.DeviceId);
                evt.Header.Length = header.Length;
                evt.Header.MessageId = (FwLibMessageId)header.MessageId;
                evt.Header.Flag1 = header.Flag1;
                evt.Header.Flag2 = header.Flag2;

                message = evt;
            }

            return ret;
        }

        private bool MakeResponseObject(FwLibBinMessageHeaderStruct header, ref IFwLibMessage message)
        {
            bool ret = false;
            List<object> arguments = null;
            FwLibMessageId messageId = (FwLibMessageId)header.MessageId;

            //message = null;

            switch (messageId)
            {
                case FwLibMessageId.ReadHardwareVersion:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 11);
                        FwLibBinMessageReadHwVerResponseStruct msg = Marshal.PtrToStructure<FwLibBinMessageReadHwVerResponseStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            arguments = new List<object>()
                            {
                                msg.HwVersion.Major,
                                msg.HwVersion.Minor,
                                msg.HwVersion.Revision
                            };
                            ret = true;
                        }
                    }
                    break;

                case FwLibMessageId.ReadFirmwareVersion:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 11);
                        FwLibBinMessageReadFwVerResponseStruct msg = Marshal.PtrToStructure<FwLibBinMessageReadFwVerResponseStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            arguments = new List<object>()
                            {
                                msg.FwVersion.Major,
                                msg.FwVersion.Minor,
                                msg.FwVersion.Revision
                            };
                            ret = true;
                        }
                    }
                    break;

                case FwLibMessageId.ReadGpio:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 9);
                        FwLibBinMessageReadGpioResponseStruct msg = Marshal.PtrToStructure<FwLibBinMessageReadGpioResponseStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            arguments = new List<object>()
                            {
                                msg.GpioValue
                            };
                            ret = true;
                        }
                    }
                    break;

                case FwLibMessageId.WriteGpio:
                    {
                        _crc16 = FwLibUtil.CRC16(_buf, 1, 8);
                        FwLibBinMessageStruct msg = Marshal.PtrToStructure<FwLibBinMessageStruct>(_bufPtr);
                        if ((CompareCrc(_crc16, msg.Tail.Crc16) == true) &&
                            (FwLibConstant.BIN_MSG_ETX == msg.Tail.Etx))
                        {
                            ret = true;
                        }
                    }
                    break;
            }

            if (ret == true)
            {
                FwLibBinMessageResponse response = new FwLibBinMessageResponse()
                {
                    Arguments = arguments,
                    ReceiveTime = DateTime.UtcNow
                };
                response.Header.DeviceId = FwLibUtil.SwapUInt32(header.DeviceId);
                response.Header.Length = header.Length;
                response.Header.MessageId = (FwLibMessageId)header.MessageId;
                response.Header.Flag1 = header.Flag1;
                response.Header.Flag2 = header.Flag2;

                message = response;
            }

            return ret;
        }
        #endregion
    }
}
