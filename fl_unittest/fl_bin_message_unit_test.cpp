#include "pch.h"
#include "fl_bin_message.h"
#include "fl_util.h"

#define DEVICE_ID   (1)
#define HW_VER      ("a.1.2.3")
#define FW_VER      ("a.2.3.4")

class Fl_Bin_Message_UnitTest : public testing::Test
{
protected:
  uint8_t _packet_buf[FL_BIN_MSG_MAX_LENGTH];
  uint16_t _expected_crc;
  uint16_t _actual_crc;
  uint8_t _len;
  fl_bin_msg_full_t* _msg_full;

protected:
  void SetUp()
  {
    memset(_packet_buf, 0, sizeof(_packet_buf));
    _expected_crc = 0;
    _actual_crc = 0;
    _len = 0;
    _msg_full = NULL;
  }

  uint8_t calculate_length_field_value(uint8_t msg_size)
  {
    //                                     device id field    length field 
    //      header size                    size(4)            size(1)                       crc(2)             etx(1)
    return (sizeof(fl_bin_msg_header_t) - sizeof(uint32_t) - sizeof(uint8_t)) + msg_size + sizeof(uint16_t) + sizeof(uint8_t);
  }
};

TEST_F(Fl_Bin_Message_UnitTest, TestReadHardwareVersionCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_HW_VERSION;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_TRUE;
  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_HW_VERSION, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(1, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadHardwareVersionOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_HW_VERSION;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_hw_ver_t* hw_ver = (fl_hw_ver_t*)&_msg_full->payload;
  sprintf(hw_ver->version, HW_VER);

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)19, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value((uint8_t)strlen(HW_VER)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_HW_VERSION, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(1, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_hw_ver_t* payload = (fl_hw_ver_t*)&_msg_full->payload;
  EXPECT_EQ(0, strncmp(HW_VER, payload->version, strlen(HW_VER)));

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + strlen(HW_VER));
  _actual_crc = *((uint16_t*)&_packet_buf[16]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[18]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadHardwareVersionFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_HW_VERSION;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_HW_VERSION, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(1, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadFirmVersionCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_FW_VERSION;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_TRUE;
  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_FW_VERSION, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(2, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadFirmwareVersionOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_FW_VERSION;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_fw_ver_t* fw_ver = (fl_fw_ver_t*)&_msg_full->payload;
  sprintf(fw_ver->version, FW_VER);

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)19, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value((uint8_t)strlen(FW_VER)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_FW_VERSION, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(2, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_fw_ver_t* payload = (fl_fw_ver_t*)&_msg_full->payload;
  EXPECT_EQ(0, strncmp(FW_VER, payload->version, strlen(FW_VER)));

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + strlen(FW_VER));
  _actual_crc = *((uint16_t*)&_packet_buf[16]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[18]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadFirmwareVersionFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_FW_VERSION;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_FW_VERSION, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(2, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadGpioCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_GPIO;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_gpi_port_t* gpi_port = (fl_gpi_port_t*)&_msg_full->payload;
  gpi_port->port_num = 1;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)13, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_gpi_port_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_GPIO, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(3, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_gpi_port_t* payload = (fl_gpi_port_t*)&_msg_full->payload;
  EXPECT_EQ(1, payload->port_num);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_gpi_port_t));
  _actual_crc = *((uint16_t*)&_packet_buf[10]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[12]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadGpioOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_GPIO;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_gpi_port_value_t* gpi_port_value = (fl_gpi_port_value_t*)&_msg_full->payload;
  gpi_port_value->port_num = 1;
  gpi_port_value->port_value = 1;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)14, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value((uint8_t)sizeof(fl_gpi_port_value_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_GPIO, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(3, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_gpi_port_value_t* payload = (fl_gpi_port_value_t*)&_msg_full->payload;
  EXPECT_EQ(1, payload->port_num);
  EXPECT_EQ(1, payload->port_value);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_gpi_port_value_t));
  _actual_crc = *((uint16_t*)&_packet_buf[11]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[13]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadGpioFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_GPIO;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_GPIO, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(3, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestWriteGpioCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_WRITE_GPIO;
  _msg_full->header.flag1.sequence_num = 4;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_gpo_port_value_t* gpo_port_value = (fl_gpo_port_value_t*)&_msg_full->payload;
  gpo_port_value->port_num = 1;
  gpo_port_value->port_value = 1;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)14, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_gpo_port_value_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_WRITE_GPIO, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(4, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_gpo_port_value_t* payload = (fl_gpo_port_value_t*)&_msg_full->payload;
  EXPECT_EQ(1, payload->port_num);
  EXPECT_EQ(1, payload->port_value);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_gpo_port_value_t));
  _actual_crc = *((uint16_t*)&_packet_buf[11]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[13]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestWriteGpioOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_WRITE_GPIO;
  _msg_full->header.flag1.sequence_num = 4;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_WRITE_GPIO, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(4, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestWriteGpioFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_WRITE_GPIO;
  _msg_full->header.flag1.sequence_num = 4;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_WRITE_GPIO, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(4, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadTemperatureCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMPERATURE;
  _msg_full->header.flag1.sequence_num = 5;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_sensor_t* sensor = (fl_sensor_t*)&_msg_full->payload;
  sensor->sensor_num = 1;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)13, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_sensor_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_TEMPERATURE, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(5, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_sensor_t* payload = (fl_sensor_t*)&_msg_full->payload;
  EXPECT_EQ(1, payload->sensor_num);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_sensor_t));
  _actual_crc = *((uint16_t*)&_packet_buf[10]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[12]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadTemperatureOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMPERATURE;
  _msg_full->header.flag1.sequence_num = 5;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_temp_sensor_read_t* sensor_value = (fl_temp_sensor_read_t*)&_msg_full->payload;
  sensor_value->sensor_num = 1;
  sensor_value->temperature = 12.3;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)21, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value((uint8_t)sizeof(fl_temp_sensor_read_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_TEMPERATURE, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(5, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_temp_sensor_read_t* payload = (fl_temp_sensor_read_t*)&_msg_full->payload;
  EXPECT_EQ(1, payload->sensor_num);
  EXPECT_EQ(12.3, payload->temperature);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_temp_sensor_read_t));
  _actual_crc = *((uint16_t*)&_packet_buf[18]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[20]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadTemperatureFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMPERATURE;
  _msg_full->header.flag1.sequence_num = 5;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_TEMPERATURE, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(5, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadHumidityCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_HUMIDITY;
  _msg_full->header.flag1.sequence_num = 6;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_sensor_t* sensor = (fl_sensor_t*)&_msg_full->payload;
  sensor->sensor_num = 2;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)13, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_sensor_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_HUMIDITY, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(6, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_sensor_t* payload = (fl_sensor_t*)&_msg_full->payload;
  EXPECT_EQ(2, payload->sensor_num);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_sensor_t));
  _actual_crc = *((uint16_t*)&_packet_buf[10]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[12]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadHumidityOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_HUMIDITY;
  _msg_full->header.flag1.sequence_num = 6;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_hum_sensor_read_t* sensor_value = (fl_hum_sensor_read_t*)&_msg_full->payload;
  sensor_value->sensor_num = 2;
  sensor_value->humidity = 23.4;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)21, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value((uint8_t)sizeof(fl_temp_sensor_read_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_HUMIDITY, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(6, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_temp_sensor_read_t* payload = (fl_temp_sensor_read_t*)&_msg_full->payload;
  EXPECT_EQ(2, payload->sensor_num);
  EXPECT_EQ(23.4, payload->temperature);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_hum_sensor_read_t));
  _actual_crc = *((uint16_t*)&_packet_buf[18]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[20]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadHumidityFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_HUMIDITY;
  _msg_full->header.flag1.sequence_num = 6;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_HUMIDITY, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(6, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadTemperatureHumidityCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMP_AND_HUM;
  _msg_full->header.flag1.sequence_num = 7;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_sensor_t* sensor = (fl_sensor_t*)&_msg_full->payload;
  sensor->sensor_num = 3;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)13, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_sensor_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_TEMP_AND_HUM, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(7, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_sensor_t* payload = (fl_sensor_t*)&_msg_full->payload;
  EXPECT_EQ(3, payload->sensor_num);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_sensor_t));
  _actual_crc = *((uint16_t*)&_packet_buf[10]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[12]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadTemperatureHumidityOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMP_AND_HUM;
  _msg_full->header.flag1.sequence_num = 7;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_temp_hum_sensor_read_t* sensor_value = (fl_temp_hum_sensor_read_t*)&_msg_full->payload;
  sensor_value->sensor_num = 3;
  sensor_value->temperature = 12.3;
  sensor_value->humidity = 23.4;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)29, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value((uint8_t)sizeof(fl_temp_hum_sensor_read_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_TEMP_AND_HUM, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(7, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_temp_hum_sensor_read_t* payload = (fl_temp_hum_sensor_read_t*)&_msg_full->payload;
  EXPECT_EQ(3, payload->sensor_num);
  EXPECT_EQ(12.3, payload->temperature);
  EXPECT_EQ(23.4, payload->humidity);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_temp_hum_sensor_read_t));
  _actual_crc = *((uint16_t*)&_packet_buf[26]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[28]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestReadTemperatureHumidityFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMP_AND_HUM;
  _msg_full->header.flag1.sequence_num = 7;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_READ_TEMP_AND_HUM, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(7, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestBootModeCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_BOOT_MODE;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_boot_mode_t* boot_mode = (fl_boot_mode_t*)&_msg_full->payload;
  boot_mode->boot_mode = FL_BMODE_BOOTLOADER;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)13, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_boot_mode_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_BOOT_MODE, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(1, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_boot_mode_t* payload = (fl_boot_mode_t*)&_msg_full->payload;
  EXPECT_EQ(FL_BMODE_BOOTLOADER, payload->boot_mode);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_boot_mode_t));
  _actual_crc = *((uint16_t*)&_packet_buf[10]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[12]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestBootModeOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_BOOT_MODE;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_BOOT_MODE, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(1, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestBootModeFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_BOOT_MODE;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_BOOT_MODE, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(1, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestResetCommandMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_RESET;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_RESET, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_COMMAND, _msg_full->header.flag1.message_type);
  EXPECT_EQ(2, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_TRUE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(0, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestResetOkResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_RESET;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_RESET, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(2, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestResetFailResponseMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_RESET;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)12, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(0), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_RESET, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_RESPONSE, _msg_full->header.flag1.message_type);
  EXPECT_EQ(2, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_ERROR, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t));
  _actual_crc = *((uint16_t*)&_packet_buf[9]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[11]);
}

TEST_F(Fl_Bin_Message_UnitTest, TestButtonEventMessageBuild)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = DEVICE_ID;
  _msg_full->header.message_id = FL_MSG_ID_BUTTON_EVENT;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_FALSE;

  fl_btn_status_t* btn_status = (fl_btn_status_t*)&_msg_full->payload;
  btn_status->button_num = 1;
  btn_status->button_value = FL_BUTTON_PRESSED;

  _len = fl_bin_msg_build_event(_packet_buf, sizeof(_packet_buf));

  EXPECT_EQ((uint8_t)14, _len);

  EXPECT_EQ(FL_BIN_MSG_STX, _msg_full->stx);
  EXPECT_EQ(DEVICE_ID, _msg_full->header.device_id);
  EXPECT_EQ(calculate_length_field_value(sizeof(fl_btn_status_t)), _msg_full->header.length);
  EXPECT_EQ(FL_MSG_ID_BUTTON_EVENT, _msg_full->header.message_id);
  EXPECT_EQ(FL_MSG_TYPE_EVENT, _msg_full->header.flag1.message_type);
  EXPECT_EQ(3, _msg_full->header.flag1.sequence_num);
  EXPECT_EQ(FL_FALSE, _msg_full->header.flag1.return_expected);
  EXPECT_EQ(0, _msg_full->header.flag1.reserved);
  EXPECT_EQ(FL_OK, _msg_full->header.flag2.error);
  EXPECT_EQ(0, _msg_full->header.flag2.reserved);

  fl_btn_status_t* payload = (fl_btn_status_t*)&_msg_full->payload;
  EXPECT_EQ(1, payload->button_num);
  EXPECT_EQ(FL_BUTTON_PRESSED, payload->button_value);

  _expected_crc = fl_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fl_bin_msg_header_t) + sizeof(fl_btn_status_t));
  _actual_crc = *((uint16_t*)&_packet_buf[11]);
  EXPECT_EQ(_actual_crc, _expected_crc);

  // ETX
  EXPECT_EQ((uint8_t)FL_BIN_MSG_ETX, _packet_buf[13]);
}
