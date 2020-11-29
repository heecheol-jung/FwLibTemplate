using System;
using System.Collections.Generic;

namespace FwLib.Net
{
    public enum FwLibMessageType
    {
        Unknown,

        Binary,

        Text
    }

    // FW_LIB_MSG_TYPE_XXX
    public enum FwLibMessageCategory
    {
        Command,

        Response,

        Event,

        Unknown
    }

    // FW_LIB_MSG_ID_XXX
    public enum FwLibMessageId
    {
        Unknown = 0,
        ReadHardwareVersion = 1,
        ReadFirmwareVersion = 2,
        ReadGpio = 3,
        WriteGpio = 4,
        ButtonEvent = 5,
        ReadTemperature = 6,
        ReadHumidity = 7
    }

    public enum FwLibParseState
    {
        ParseOk,

        ParseFail,

        Parsing
    }

    // FW_LIB_BUTTON_XXX
    public enum FwLibButtonState
    {
        Released,

        Pressed
    }

    public enum FwLibParserRole
    {
        Host,   // PC

        Device  // MCU
    }

    public class FwLibConstant
    {
        // FW_LIB_OK
        public const byte OK = 0;

        // FW_LIB_ERROR
        public const byte ERROR = 1;

        // FW_LIB_DEVICE_ID_UNKNOWN
        public const UInt32 DEVICE_ID_UNKNOWN = 0;

        // FW_LIB_DEVICE_ID_ALL
        public const UInt32 DEVICE_ID_ALL = 0xFFFFFFFF;

        // FW_LIB_MSG_MAX_STRING_LEN
        public const uint MSG_MAX_STRING_LEN = 32;

        // FW_LIB_BIN_MSG_STX
        public const byte BIN_MSG_STX = 0x02;

        // FW_LIB_BIN_MSG_ETX
        public const byte BIN_MSG_ETX = 0x03;

        // FW_LIB_BIN_MSG_DEVICE_ID_LENGTH(4)
        public const byte BIN_MSG_DEVICE_ID_LENGTH = 4;

        // Header.Flag1 sequence number
        // FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS
        public const byte BIN_MSG_HDR_FLG1_SEQ_NUM_POS = 0x00;

        // FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK
        public const byte BIN_MSG_HDR_FLG1_SEQ_NUM_MASK = 0x0F;

        // Header.Flag1 return expected
        // FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS
        public const byte BIN_MSG_HDR_FLG1_RET_EXPECTED_POS = 0x04;

        // FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK
        public const byte BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK = 0x10;

        // Header.Flag1 message type
        // FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS
        public const byte BIN_MSG_HDR_FLG1_MSG_TYPE_POS = 0x05;

        // FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK
        public const byte BIN_MSG_HDR_FLG1_MSG_TYPE_MASK = 0x60;

        // Header.Flag1 Reserved 
        // FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS
        public const byte BIN_MSG_HDR_FLG1_RESERVED_POS = 0x07;
                
        // FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK
        public const byte BIN_MSG_HDR_FLG1_RESERVED_MASK = 0x80;

        // Header.Flag2 error
        // FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS
        public const byte BIN_MSG_HDR_FLG2_ERROR_POS = 0x00;
                
        // FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK
        public const byte BIN_MSG_HDR_FLG2_ERROR_MASK = 0x03;

        // Header.Flag2 Reserved
        // FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS
        public const byte BIN_MSG_HDR_FLG2_RESERVED_POS = 0x02;

        // FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK
        public const byte BIN_MSG_HDR_FLG2_RESERVED_MASK = 0xFC;

        // FW_LIB_BIN_MSB_MIN_SEQUENCE
        public const byte BIN_MSG_MIN_SEQUENCE = 0;

        // FW_LIB_BIN_MSG_MAX_SEQUENCE
        public const byte BIN_MSG_MAX_SEQUENCE = 0xf;

        public const byte MessageCategoryMax = 3;

        public const byte BinMsgErrorMin = 0;
        public const byte BinMsgErrorMax = 3;

        public const int DefaultCommandTryCount = 3;
        public const int DefaultResponseWaitTimeout = 500; // millisecond.
        public const int DefaultCommandTryInterval = 10;

        // STX(1) + HEADER(8) + MAX_DATA(255)
        public const UInt16 BinMessageBufSize = 264;

        public const byte BinHeaderDeviceIdFieldIndex = 1;
        public const byte BinHeaderLengthFieldIndex = 5;
        public const byte BinHeaderMessageIdFieldIndex = 6;
        public const byte BinHeaderFlag1FieldIndex = 7;
        public const byte BinHeaderFlag2FieldIndex = 8;

        //FW_LIB_TXT_MSG_MAX_LENGTH
        public const byte TXT_MSG_MAX_LENGTH = 64;

        // FW_LIB_TXT_MSG_MAX_ARG_COUNT
        public const byte TXT_MSG_MAX_ARG_COUNT = 3;

        //FW_LIB_TXT_MSG_TAIL
        public const char TXT_MSG_TAIL = '\n';

        // FW_LIB_TXT_MSG_ID_MIN_CHAR
        public const char TXT_MSG_ID_MIN_CHAR = 'A';

        // FW_LIB_TXT_MSG_ID_MAX_CHAR
        public const char TXT_MSG_ID_MAX_CHAR = 'Z';

        // FW_LIB_TXT_DEVICE_ID_MIN_CHAR
        public const char TXT_DEVICE_ID_MIN_CHAR = '0';

        // FW_LIB_TXT_DEVICE_ID_MAX_CHAR
        public const char TXT_DEVICE_ID_MAX_CHAR = '9';

        // FW_LIB_TXT_MSG_ID_MAX_LEN
        public const byte TXT_MSG_ID_MAX_LEN = 5;

        // FW_LIB_TXT_DEVICE_ID_MAX_LEN
        public const byte TXT_DEVICE_ID_MAX_LEN = 2;

        // FW_LIB_TXT_MSG_ID_DEVICE_ID_DELIMITER
        public const char TXT_MSG_ID_DEVICE_ID_DELIMITER = ' ';

        // FW_LIB_TXT_MSG_ARG_DELIMITER
        public const char TXT_MSG_ARG_DELIMITER = ',';

        public const string STR_UNKNOWN = "UNKNOWN";
        public const string STR_RHVER = "RHVER";
        public const string STR_RFVER = "RFVER";
        public const string STR_RGPIO = "RGPIO";
        public const string STR_WGPIO = "WGPIO";
        public const string STR_EBTN = "EBTN";
        public const string STR_RTEMP = "RTEMP";
        public const string STR_RHUM = "RHUM";

        public static Dictionary<FwLibMessageId, string> MessageIdToStringTable = new Dictionary<FwLibMessageId, string>
        {
            { FwLibMessageId.Unknown, STR_UNKNOWN },
            { FwLibMessageId.ReadHardwareVersion, STR_RHVER },
            { FwLibMessageId.ReadFirmwareVersion, STR_RFVER },
            { FwLibMessageId.ReadGpio, STR_RGPIO },
            { FwLibMessageId.WriteGpio, STR_WGPIO },
            { FwLibMessageId.ButtonEvent, STR_EBTN },
            { FwLibMessageId.ReadTemperature, STR_RTEMP },
            { FwLibMessageId.ReadHumidity, STR_RHUM }
        };

        public static Dictionary<string, FwLibMessageId> StringToMessageIdTable = new Dictionary<string, FwLibMessageId>
        {
            { STR_UNKNOWN, FwLibMessageId.Unknown},
            { STR_RHVER, FwLibMessageId.ReadHardwareVersion },
            { STR_RFVER, FwLibMessageId.ReadFirmwareVersion},
            { STR_RGPIO, FwLibMessageId.ReadGpio},
            { STR_WGPIO, FwLibMessageId.WriteGpio},
            { STR_EBTN, FwLibMessageId.ButtonEvent},
            { STR_RTEMP, FwLibMessageId.ReadTemperature },
            { STR_RHUM, FwLibMessageId.ReadHumidity }
        };
    }
}
