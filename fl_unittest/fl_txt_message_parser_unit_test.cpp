#include "pch.h"
#include "fl_txt_message.h"
#include "fl_txt_message_parser.h"

#define DEVICE_ID   (1)
#define HW_VER      ("a.1.2.3")
#define FW_VER      ("a.2.3.4")

class Fl_Txt_Message_Parser_UnitTest : public testing::Test
{
protected:
  fl_txt_msg_parser_t  _txt_parser;
  fl_txt_msg_t         _parsed_msg;
  int                   _len;
  int                   _i;
  uint8_t               _packet_buf[FL_TXT_MSG_MAX_LENGTH];
  fl_status_t          _ret;

protected:
  void SetUp()
  {
    fl_txt_msg_parser_init(&_txt_parser);
    memset(&_parsed_msg, 0, sizeof(fl_txt_msg_t));
  }
};

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadHardwareVersionCommandParse)
{
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_HW_VERSION, NULL, 0, _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HW_VERSION, _parsed_msg.msg_id);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadHardwareVersionOkResponseParse)
{
  // Version string.
  fl_hw_ver_t hw_ver;
  sprintf(hw_ver.version, HW_VER);
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HW_VERSION, FL_OK,
    &hw_ver, sizeof(hw_ver),
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HW_VERSION, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_hw_ver_t* parsed_hw_ver = (fl_hw_ver_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)0, (uint8_t)strcmp(hw_ver.version, parsed_hw_ver->version));
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadHardwareVersionFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HW_VERSION, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HW_VERSION, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadFirmwareVersionCommandParse)
{
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_FW_VERSION, NULL, 0, _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_FW_VERSION, _parsed_msg.msg_id);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadFirmwareVersionOkResponseParse)
{
  // Version string.
  fl_fw_ver_t fw_ver;
  sprintf(fw_ver.version, FW_VER);
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_FW_VERSION, FL_OK,
    &fw_ver, sizeof(fw_ver),
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_FW_VERSION, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_fw_ver_t* parsed_fw_ver = (fl_fw_ver_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)0, (uint8_t)strcmp(fw_ver.version, parsed_fw_ver->version));
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadFirmwareVersionFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_FW_VERSION, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_FW_VERSION, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadGpioCommandParse)
{
  // GPIO number.
  fl_gpi_port_t gpi_port;
  gpi_port.port_num = 2;

  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_GPIO,
    &gpi_port, sizeof(fl_gpi_port_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_GPIO, _parsed_msg.msg_id);

  fl_gpi_port_t* payload = (fl_gpi_port_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)2, payload->port_num);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadGpioOkResponseParse)
{
  fl_gpi_port_value_t gpi_port_val;

  // GPIO number.
  gpi_port_val.port_num = 2;
  // GPIO value.
  gpi_port_val.port_value = 0;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_GPIO, FL_OK,
    &gpi_port_val, sizeof(fl_gpi_port_value_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_GPIO, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_gpi_port_value_t* payload = (fl_gpi_port_value_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)2, payload->port_num);
  EXPECT_EQ((uint8_t)0, payload->port_value);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadGpioFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_GPIO, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_GPIO, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestWriteGpioCommandParse)
{
  fl_gpo_port_value_t gpo_port_val;

  // GPIO number.
  gpo_port_val.port_num = 3;
  // GPIO value.
  gpo_port_val.port_value = 1;

  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_WRITE_GPIO,
    &gpo_port_val, sizeof(fl_gpo_port_value_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_WRITE_GPIO, _parsed_msg.msg_id);

  fl_gpo_port_value_t* payload = (fl_gpo_port_value_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)3, payload->port_num);
  EXPECT_EQ((uint8_t)1, payload->port_value);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestWriteGpioOkResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_WRITE_GPIO, FL_OK,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_WRITE_GPIO, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestWriteGpioFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_WRITE_GPIO, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_WRITE_GPIO, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadTemperatureCommandParse)
{
  fl_sensor_t sensor;

  // Sensor number.
  sensor.sensor_num = 1;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_TEMPERATURE,
    &sensor, sizeof(fl_sensor_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMPERATURE, _parsed_msg.msg_id);

  fl_sensor_t* payload = (fl_sensor_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)1, payload->sensor_num);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadTemperatureOkResponseParse)
{
  fl_temp_sensor_read_t sensor_read;

  // Sensor number.
  sensor_read.sensor_num = 1;

  // Sensor value.
  sensor_read.temperature = 12.3;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMPERATURE, FL_OK,
    &sensor_read, sizeof(fl_temp_sensor_read_t),
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMPERATURE, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_temp_sensor_read_t* payload = (fl_temp_sensor_read_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)1, payload->sensor_num);
  EXPECT_EQ(12.3, payload->temperature);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadTemperatureFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMPERATURE, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMPERATURE, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadHumidityCommandParse)
{
  fl_sensor_t sensor;

  // Sensor number.
  sensor.sensor_num = 2;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_HUMIDITY,
    &sensor, sizeof(fl_sensor_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HUMIDITY, _parsed_msg.msg_id);

  fl_sensor_t* payload = (fl_sensor_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)2, payload->sensor_num);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadHumidityOkResponseParse)
{
  fl_hum_sensor_read_t sensor_read;

  // Sensor number.
  sensor_read.sensor_num = 2;

  // Sensor value.
  sensor_read.humidity = 23.4;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HUMIDITY, FL_OK,
    &sensor_read, sizeof(fl_hum_sensor_read_t),
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HUMIDITY, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_hum_sensor_read_t* payload = (fl_hum_sensor_read_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)2, payload->sensor_num);
  EXPECT_EQ(23.4, payload->humidity);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadHumidityFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HUMIDITY, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HUMIDITY, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadTempemperatureAndHumidityCommandParse)
{
  fl_sensor_t sensor;

  // Sensor number.
  sensor.sensor_num = 3;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_TEMP_AND_HUM,
    &sensor, sizeof(fl_sensor_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.msg_id);

  fl_sensor_t* payload = (fl_sensor_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)3, payload->sensor_num);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadTempemperatureAndHumidityOkResponseParse)
{
  fl_temp_hum_sensor_read_t temp_hum_read;

  // Sensor number.
  temp_hum_read.sensor_num = 3;

  // Temperature value.
  temp_hum_read.temperature = 23.4;

  // Humidity value.
  temp_hum_read.humidity = 56.7;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMP_AND_HUM, FL_OK,
    &temp_hum_read, sizeof(fl_temp_hum_sensor_read_t),
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_temp_hum_sensor_read_t* payload = (fl_temp_hum_sensor_read_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)3, payload->sensor_num);
  EXPECT_EQ(23.4, payload->temperature);
  EXPECT_EQ(56.7, payload->humidity);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestReadTempemperatureAndHumidityFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMP_AND_HUM, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestBootModeCommandParse)
{
  fl_boot_mode_t bmode;

  // Boot mode.
  bmode.boot_mode = FL_BMODE_BOOTLOADER;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_BOOT_MODE,
    &bmode, sizeof(fl_boot_mode_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BOOT_MODE, _parsed_msg.msg_id);

  fl_boot_mode_t* payload = (fl_boot_mode_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)FL_BMODE_BOOTLOADER, payload->boot_mode);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestBootModeOkResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_BOOT_MODE, FL_OK,
    NULL, 0,
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BOOT_MODE, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestBootModeFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_BOOT_MODE, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BOOT_MODE, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestResetCommandParse)
{
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_RESET,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_RESET, _parsed_msg.msg_id);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestResetOkResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_RESET, FL_OK,
    NULL, 0,
    _packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_RESET, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestResetFailResponseParse)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_RESET, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_RESET, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.error);
}

TEST_F(Fl_Txt_Message_Parser_UnitTest, TestButtonEventParse)
{
  fl_btn_status_t button;
  button.button_num = 2;
  button.button_value = FL_BUTTON_PRESSED;

  _len = fl_txt_msg_build_event(DEVICE_ID, FL_MSG_ID_BUTTON_EVENT,
    &button, sizeof(fl_btn_status_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_txt_msg_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint32_t)DEVICE_ID, _parsed_msg.device_id);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BUTTON_EVENT, _parsed_msg.msg_id);
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.error);

  fl_btn_status_t* payload = (fl_btn_status_t*)&_parsed_msg.payload;
  EXPECT_EQ((uint8_t)2, payload->button_num);
  EXPECT_EQ((uint8_t)FL_BUTTON_PRESSED, payload->button_value);
}