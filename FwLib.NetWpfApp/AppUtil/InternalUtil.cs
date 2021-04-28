using Fl.Net;
using System.Collections.Generic;

namespace FwLib.NetWpfApp.AppUtil
{
    public static class InternalUtil
    {
        public static List<AppMessageTemplate> GetMessageTemplates()
        {
            List<AppMessageTemplate> msgTemplates = new List<AppMessageTemplate>();
            AppMessageTemplate msgTemplate = null;

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_READ_HW_VERSION",
                MessageId = FlMessageId.ReadHardwareVersion,
                MessageType = FlMessageCategory.Command
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_HW_VERSION",
                MessageId = FlMessageId.ReadHardwareVersion,
                MessageType = FlMessageCategory.Response,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Hardware version",
                        DataType = "System.String"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_READ_FW_VERSION",
                MessageId = FlMessageId.ReadFirmwareVersion,
                MessageType = FlMessageCategory.Command
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_FW_VERSION",
                MessageId = FlMessageId.ReadFirmwareVersion,
                MessageType = FlMessageCategory.Response,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Firmware version",
                        DataType = "System.String"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_READ_GPIO",
                MessageId = FlMessageId.ReadGpio,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "GPIO number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FW_LIB_MSG_ID_READ_GPIO",
                MessageId = FlMessageId.ReadGpio,
                MessageType = FlMessageCategory.Response,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "GPIO value",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_WRITE_GPIO",
                MessageId = FlMessageId.WriteGpio,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "GPIO number",
                        DataType = "System.Byte"
                    },
                    new AppArgumentTemplate()
                    {
                        Name = "GPIO value",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_BUTTON_EVENT",
                MessageId = FlMessageId.ButtonEvent,
                MessageType = FlMessageCategory.Event,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Button number",
                        DataType = "System.Byte"
                    },
                    new AppArgumentTemplate()
                    {
                        Name = "Button status",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_READ_TEMPERATURE",
                MessageId = FlMessageId.ReadTemperature,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Sensor number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_READ_HUMIDITY",
                MessageId = FlMessageId.ReadHumidity,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Sensor number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_READ_TEMP_AND_HUM",
                MessageId = FlMessageId.ReadTempAndHum,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Sensor number",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_BOOT_MODE",
                MessageId = FlMessageId.BootMode,
                MessageType = FlMessageCategory.Command,
                Arguments = new List<AppArgumentTemplate>()
                {
                    new AppArgumentTemplate()
                    {
                        Name = "Boot mode",
                        DataType = "System.Byte"
                    }
                }
            };
            msgTemplates.Add(msgTemplate);

            msgTemplate = new AppMessageTemplate()
            {
                MessageIdForCApi = "FL_MSG_ID_RESET",
                MessageId = FlMessageId.Reset,
                MessageType = FlMessageCategory.Command
            };
            msgTemplates.Add(msgTemplate);

            return msgTemplates;
        }
    }
}
