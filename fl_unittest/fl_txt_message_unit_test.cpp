#include "pch.h"
#include "fl_txt_message.h"

#define DEVICE_ID   (1)
#define HW_VER      ("a.1.2.3")
#define FW_VER      ("a.2.3.4")

class Fl_Txt_Message_UnitTest : public testing::Test
{
protected:
  uint8_t   _packet_buf[FL_TXT_MSG_MAX_LENGTH];
  uint8_t   _len;

protected:
  void SetUp()
  {
    memset(_packet_buf, 0, sizeof(_packet_buf));
  }
};

TEST_F(Fl_Txt_Message_UnitTest, TestReadHardwareVersionCommandMessageBuild) 
{
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_HW_VERSION, NULL, 0, _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)8, _len);
  EXPECT_EQ(0, strcmp("RHVER 1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadHardwareVersionOkResponseMessageBuild)
{
  fl_hw_ver_t hw_ver;

  sprintf(hw_ver.version, HW_VER);
  
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HW_VERSION, FL_OK,
    &hw_ver, sizeof(fl_hw_ver_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)18, _len);
  EXPECT_EQ(0, strcmp("RHVER 1,0,a.1.2.3\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadHardwareVersionFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HW_VERSION, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RHVER 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadFirmwareVersionCommandMessageBuild)
{
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_FW_VERSION, NULL, 0, _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)8, _len);
  EXPECT_EQ(0, strcmp("RFVER 1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadFirmwareVersionOkResponseMessageBuild)
{
  fl_fw_ver_t fw_ver;

  sprintf(fw_ver.version, FW_VER);

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_FW_VERSION, FL_OK,
    &fw_ver, sizeof(fl_fw_ver_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)18, _len);
  EXPECT_EQ(0, strcmp("RFVER 1,0,a.2.3.4\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadFirmwareVersionFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_FW_VERSION, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RFVER 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadGpioCommandMessageBuild)
{
  // GPIO number.
  fl_gpi_port_t gpi_port;
  gpi_port.port_num = 2;
  
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_GPIO,
    &gpi_port, sizeof(fl_gpi_port_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RGPIO 1,2\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadGpioOkResponseMessageBuild)
{
  fl_gpi_port_value_t gpi_port_val;

  // GPIO number.
  gpi_port_val.port_num = 2;
  // GPIO value.
  gpi_port_val.port_value = 0;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_GPIO, FL_OK,
    &gpi_port_val, sizeof(fl_gpi_port_value_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)14, _len);
  EXPECT_EQ(0, strcmp("RGPIO 1,0,2,0\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadGpioFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_GPIO, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RGPIO 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestWriteGpioCommandMessageBuild)
{
  fl_gpo_port_value_t gpo_port_val;

  // GPIO number.
  gpo_port_val.port_num = 3;
  // GPIO value.
  gpo_port_val.port_value = 1;

  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_WRITE_GPIO,
    &gpo_port_val, sizeof(fl_gpo_port_value_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);
  EXPECT_EQ(0, strcmp("WGPIO 1,3,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestWriteGpioOkResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_WRITE_GPIO, FL_OK,
    NULL, 0,
    _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("WGPIO 1,0\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestWriteGpioFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_WRITE_GPIO, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("WGPIO 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadTemperatureCommandMessageBuild)
{
  fl_sensor_t sensor;

  // Sensor number.
  sensor.sensor_num = 1;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_TEMPERATURE,
    &sensor, sizeof(fl_sensor_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RTEMP 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadTemperatureOkResponseMessageBuild)
{
  fl_temp_sensor_read_t sensor_read;

  // Sensor number.
  sensor_read.sensor_num = 1;

  // Sensor value.
  sensor_read.temperature = 12.3;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMPERATURE, FL_OK,
    &sensor_read, sizeof(fl_temp_sensor_read_t),
    _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)18, _len);
  EXPECT_EQ(0, strcmp("RTEMP 1,0,1,12.30\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadTemperatureFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMPERATURE, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RTEMP 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadHumidityCommandMessageBuild)
{
  fl_sensor_t sensor;

  // Sensor number.
  sensor.sensor_num = 2;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_HUMIDITY,
    &sensor, sizeof(fl_sensor_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)9, _len);
  EXPECT_EQ(0, strcmp("RHUM 1,2\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadHumidityOkResponseMessageBuild)
{
  fl_hum_sensor_read_t sensor_read;

  // Sensor number.
  sensor_read.sensor_num = 2;

  // Sensor value.
  sensor_read.humidity = 23.4;

  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HUMIDITY, FL_OK,
    &sensor_read, sizeof(fl_hum_sensor_read_t),
    _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)17, _len);
  EXPECT_EQ(0, strcmp("RHUM 1,0,2,23.40\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadHumidityFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_HUMIDITY, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)9, _len);
  EXPECT_EQ(0, strcmp("RHUM 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadTempemperatureAndHumidityCommandMessageBuild)
{
  fl_sensor_t sensor;

  // Sensor number.
  sensor.sensor_num = 3;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_READ_TEMP_AND_HUM,
    &sensor, sizeof(fl_sensor_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)9, _len);
  EXPECT_EQ(0, strcmp("RTAH 1,3\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadTemperatureAndHumidityOkResponseMessageBuild)
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

  EXPECT_EQ((uint8_t)23, _len);
  EXPECT_EQ(0, strcmp("RTAH 1,0,3,23.40,56.70\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestReadTemperatureAndHumidityFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_READ_TEMP_AND_HUM, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)9, _len);
  EXPECT_EQ(0, strcmp("RTAH 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestBootModeCommandMessageBuild)
{
  fl_boot_mode_t bmode;

  // Boot mode.
  bmode.boot_mode = FL_BMODE_BOOTLOADER;
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_BOOT_MODE,
    &bmode, sizeof(fl_boot_mode_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("BMODE 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestBootModeOkResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_BOOT_MODE, FL_OK,
    NULL, 0,
    _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("BMODE 1,0\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestBootModeFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_BOOT_MODE, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("BMODE 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestResetCommandMessageBuild)
{
  _len = fl_txt_msg_build_command(DEVICE_ID, FL_MSG_ID_RESET,
    NULL, 0,
    _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)8, _len);
  EXPECT_EQ(0, strcmp("RESET 1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestResetOkResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_RESET, FL_OK,
    NULL, 0,
    _packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RESET 1,0\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestResetFailResponseMessageBuild)
{
  _len = fl_txt_msg_build_response(DEVICE_ID, FL_MSG_ID_RESET, FL_ERROR,
    NULL, 0,
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)10, _len);
  EXPECT_EQ(0, strcmp("RESET 1,1\n", (const char*)_packet_buf));
}

TEST_F(Fl_Txt_Message_UnitTest, TestButtonEventMessageBuild)
{
  fl_btn_status_t button;
  button.button_num = 2;
  button.button_value = FL_BUTTON_PRESSED;

  _len = fl_txt_msg_build_event(DEVICE_ID, FL_MSG_ID_BUTTON_EVENT,
    &button, sizeof(fl_btn_status_t),
    (uint8_t*)_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)11, _len);
  EXPECT_EQ(0, strcmp("EBTN 1,2,1\n", (const char*)_packet_buf));
}