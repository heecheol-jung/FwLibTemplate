using Fl.Net;
using System.Collections.Generic;

namespace FwLib.NetWpfApp.AppUtil
{
    public static class InternalUtil
    {
        public static List<FwLibMessageTemplate> GetMessageTemplates()
        {
            List<FwLibMessageTemplate> msgTemplates = new List<FwLibMessageTemplate>();
            FwLibMessageTemplate msgTemplate = null;

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_HW_VERSION",
                MessageId = FlMessageId.ReadHardwareVersion,
                MessageType = FlMessageCategory.Command
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_HW_VERSION",
                MessageId = FlMessageId.ReadHardwareVersion,
                MessageType = FlMessageCategory.Response,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "Hardware version",
                        DataType = "FwLib.Net.FwLibHwVersion"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_FW_VERSION",
                MessageId = FlMessageId.ReadFirmwareVersion,
                MessageType = FlMessageCategory.Command
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_FW_VERSION",
                MessageId = FlMessageId.ReadFirmwareVersion,
                MessageType = FlMessageCategory.Response,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "Firmware version",
                        DataType = "FwLib.Net.FwLibFwVersion"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_GPIO",
                MessageId = FlMessageId.ReadGpio,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "GPIO number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_GPIO",
                MessageId = FlMessageId.ReadGpio,
                MessageType = FlMessageCategory.Response,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "GPIO value",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_WRITE_GPIO",
                MessageId = FlMessageId.WriteGpio,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "GPIO number",
                        DataType = "System.Byte"
                    },
                    new FwLibArgumentTemplate()
                    {
                        Name = "GPIO value",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_BUTTON_EVENT",
                MessageId = FlMessageId.ButtonEvent,
                MessageType = FlMessageCategory.Event,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "Button number",
                        DataType = "System.Byte"
                    },
                    new FwLibArgumentTemplate()
                    {
                        Name = "Button status",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_TEMPERATURE",
                MessageId = FlMessageId.ReadTemperature,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "Sensor number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_HUMIDITY",
                MessageId = FlMessageId.ReadHumidity,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "Sensor number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new FwLibMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_TEMP_AND_HUM",
                MessageId = FlMessageId.ReadTempAndHum,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<FwLibArgumentTemplate>()
                {
                    new FwLibArgumentTemplate()
                    {
                        Name = "Sensor number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            return msgTemplates;
        }
    }
}
