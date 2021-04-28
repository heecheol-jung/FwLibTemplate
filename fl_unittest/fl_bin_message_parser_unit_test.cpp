#include "pch.h"
#include "fl_bin_message_parser.h"
#include "fl_util.h"

#define HW_VER      ("1.2.3")
#define FW_VER      ("2.3.4")

class Fl_Bin_Message_Parser_UnitTest : public testing::Test
{
protected:
  fl_bin_msg_parser_t       _bin_parser;
  fl_bin_msg_full_t         _parsed_msg;
  int                       _len;
  int                       _i;
  uint8_t                   _packet_buf[FL_BIN_MSG_MAX_LENGTH];
  fl_status_t               _ret;
  uint16_t                  _expected_crc;
  uint16_t                  _actual_crc;
  fl_bin_msg_full_t*        _msg_full;

protected:
  void SetUp()
  {
    fl_bin_msg_parser_init(&_bin_parser);
    memset(&_parsed_msg, 0, sizeof(fl_bin_msg_full_t));
    memset(&_packet_buf, 0, sizeof(fl_bin_msg_full_t));
    _expected_crc = 0;
    _actual_crc = 0;
    _msg_full = NULL;
  }

  uint8_t calculate_length_field_value(uint8_t msg_size)
  {
    //                                     device id field    length field 
    //      header size                    size(4)            size(1)                       crc(2)             etx(1)
    return (sizeof(fl_bin_msg_header_t) - sizeof(uint32_t) - sizeof(uint8_t)) + msg_size + sizeof(uint16_t) + sizeof(uint8_t);
  }
};

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadHardwareVersionCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 1;
  _msg_full->header.message_id = FL_MSG_ID_READ_HW_VERSION;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_TRUE;
  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)1, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HW_VERSION, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)1, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadHardwareVersionOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 1;
  _msg_full->header.message_id = FL_MSG_ID_READ_HW_VERSION;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_hw_ver_t* hw_ver = (fl_hw_ver_t*)&_msg_full->payload;
  sprintf(hw_ver->version, HW_VER);

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)1, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value((uint8_t)strlen("1.2.3")), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HW_VERSION, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)1, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_hw_ver_t* payload = (fl_hw_ver_t*)&_parsed_msg.payload;
  EXPECT_EQ(0, strncmp(hw_ver->version, payload->version, _msg_full->header.length - 6));
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadHardwareVersionFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 1;
  _msg_full->header.message_id = FL_MSG_ID_READ_HW_VERSION;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)1, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HW_VERSION, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)1, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadFirmwareVersionCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 2;
  _msg_full->header.message_id = FL_MSG_ID_READ_FW_VERSION;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_TRUE;
  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)2, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_FW_VERSION, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadFirmwareVersionOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 2;
  _msg_full->header.message_id = FL_MSG_ID_READ_FW_VERSION;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_fw_ver_t* fw_ver = (fl_fw_ver_t*)&_msg_full->payload;
  sprintf(fw_ver->version, FW_VER);

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)2, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value((uint8_t)strlen("2.3.4")), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_FW_VERSION, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_fw_ver_t* payload = (fl_fw_ver_t*)&_parsed_msg.payload;
  EXPECT_EQ(0, strncmp(fw_ver->version, payload->version, _msg_full->header.length - 6));
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadFirmwareVersionFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 2;
  _msg_full->header.message_id = FL_MSG_ID_READ_FW_VERSION;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)2, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_FW_VERSION, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadGpioCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 3;
  _msg_full->header.message_id = FL_MSG_ID_READ_GPIO;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_gpi_port_t* gpi_port = (fl_gpi_port_t*)&_msg_full->payload;
  gpi_port->port_num = 1;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)3, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_gpi_port_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_GPIO, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)3, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_gpi_port_t* payload = (fl_gpi_port_t*)&_parsed_msg.payload;
  EXPECT_EQ(gpi_port->port_num, payload->port_num);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadGpioOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 3;
  _msg_full->header.message_id = FL_MSG_ID_READ_GPIO;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_gpi_port_value_t* gpi_port_value = (fl_gpi_port_value_t*)&_msg_full->payload;
  gpi_port_value->port_num = 1;
  gpi_port_value->port_value = 1;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)3, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_gpi_port_value_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_GPIO, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)3, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_gpi_port_value_t* payload = (fl_gpi_port_value_t*)&_parsed_msg.payload;
  EXPECT_EQ(gpi_port_value->port_num, payload->port_num);
  EXPECT_EQ(gpi_port_value->port_value, payload->port_value);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadGpioFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 3;
  _msg_full->header.message_id = FL_MSG_ID_READ_GPIO;
  _msg_full->header.flag1.sequence_num = 3;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)3, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_GPIO, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)3, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestWriteGpioCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 4;
  _msg_full->header.message_id = FL_MSG_ID_WRITE_GPIO;
  _msg_full->header.flag1.sequence_num = 4;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_gpo_port_value_t* gpo_port_value = (fl_gpo_port_value_t*)&_msg_full->payload;
  gpo_port_value->port_num = 1;
  gpo_port_value->port_value = 1;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)4, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_gpo_port_value_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_WRITE_GPIO, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)4, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_gpo_port_value_t* payload = (fl_gpo_port_value_t*)&_parsed_msg.payload;
  EXPECT_EQ(gpo_port_value->port_num, payload->port_num);
  EXPECT_EQ(gpo_port_value->port_value, payload->port_value);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestWriteGpioOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 4;
  _msg_full->header.message_id = FL_MSG_ID_WRITE_GPIO;
  _msg_full->header.flag1.sequence_num = 4;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)4, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_WRITE_GPIO, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)4, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestWriteGpioFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 4;
  _msg_full->header.message_id = FL_MSG_ID_WRITE_GPIO;
  _msg_full->header.flag1.sequence_num = 4;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)4, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_WRITE_GPIO, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)4, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadTemperatureCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 5;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMPERATURE;
  _msg_full->header.flag1.sequence_num = 5;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_sensor_t* sensor = (fl_sensor_t*)&_msg_full->payload;
  sensor->sensor_num = 1;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)5, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_sensor_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMPERATURE, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)5, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_sensor_t* payload = (fl_sensor_t*)&_parsed_msg.payload;
  EXPECT_EQ(sensor->sensor_num, payload->sensor_num);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadTemperatureOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 5;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMPERATURE;
  _msg_full->header.flag1.sequence_num = 5;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_temp_sensor_read_t* sensor_value = (fl_temp_sensor_read_t*)&_msg_full->payload;
  sensor_value->sensor_num = 1;
  sensor_value->temperature = 12.3;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)5, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_temp_sensor_read_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMPERATURE, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)5, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_temp_sensor_read_t* payload = (fl_temp_sensor_read_t*)&_parsed_msg.payload;
  EXPECT_EQ(sensor_value->sensor_num, payload->sensor_num);
  EXPECT_EQ(sensor_value->temperature, payload->temperature);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadTemperatureFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 5;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMPERATURE;
  _msg_full->header.flag1.sequence_num = 5;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)5, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMPERATURE, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)5, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadHumidityCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 6;
  _msg_full->header.message_id = FL_MSG_ID_READ_HUMIDITY;
  _msg_full->header.flag1.sequence_num = 6;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_sensor_t* sensor = (fl_sensor_t*)&_msg_full->payload;
  sensor->sensor_num = 2;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)6, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_sensor_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HUMIDITY, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)6, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_sensor_t* payload = (fl_sensor_t*)&_parsed_msg.payload;
  EXPECT_EQ(sensor->sensor_num, payload->sensor_num);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadHumidityOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 6;
  _msg_full->header.message_id = FL_MSG_ID_READ_HUMIDITY;
  _msg_full->header.flag1.sequence_num = 6;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_hum_sensor_read_t* sensor_value = (fl_hum_sensor_read_t*)&_msg_full->payload;
  sensor_value->sensor_num = 2;
  sensor_value->humidity = 23.4;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)6, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_hum_sensor_read_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HUMIDITY, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)6, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_hum_sensor_read_t* payload = (fl_hum_sensor_read_t*)&_parsed_msg.payload;
  EXPECT_EQ(sensor_value->sensor_num, payload->sensor_num);
  EXPECT_EQ(sensor_value->humidity, payload->humidity);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadHumidityFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 6;
  _msg_full->header.message_id = FL_MSG_ID_READ_HUMIDITY;
  _msg_full->header.flag1.sequence_num = 6;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)6, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_HUMIDITY, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)6, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadTemperatureHumidityCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 7;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMP_AND_HUM;
  _msg_full->header.flag1.sequence_num = 7;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_sensor_t* sensor = (fl_sensor_t*)&_msg_full->payload;
  sensor->sensor_num = 3;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)7, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_sensor_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)7, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_sensor_t* payload = (fl_sensor_t*)&_parsed_msg.payload;
  EXPECT_EQ(sensor->sensor_num, payload->sensor_num);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadTemperatureHumidityOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 7;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMP_AND_HUM;
  _msg_full->header.flag1.sequence_num = 7;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  fl_temp_hum_sensor_read_t* sensor_value = (fl_temp_hum_sensor_read_t*)&_msg_full->payload;
  sensor_value->sensor_num = 3;
  sensor_value->temperature = 12.3;
  sensor_value->humidity = 23.4;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)7, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_temp_hum_sensor_read_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)7, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_temp_hum_sensor_read_t* payload = (fl_temp_hum_sensor_read_t*)&_parsed_msg.payload;
  EXPECT_EQ(sensor_value->sensor_num, payload->sensor_num);
  EXPECT_EQ(sensor_value->temperature, payload->temperature);
  EXPECT_EQ(sensor_value->humidity, payload->humidity);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestReadTemperatureHumidityFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 7;
  _msg_full->header.message_id = FL_MSG_ID_READ_TEMP_AND_HUM;
  _msg_full->header.flag1.sequence_num = 7;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)7, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)7, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestBootModeCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 8;
  _msg_full->header.message_id = FL_MSG_ID_BOOT_MODE;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  fl_boot_mode_t* boot_mode = (fl_boot_mode_t*)&_msg_full->payload;
  boot_mode->boot_mode = FL_BMODE_BOOTLOADER;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)8, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_boot_mode_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BOOT_MODE, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)1, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestBootModeOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 8;
  _msg_full->header.message_id = FL_MSG_ID_BOOT_MODE;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)8, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BOOT_MODE, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)1, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestBootModeFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 8;
  _msg_full->header.message_id = FL_MSG_ID_BOOT_MODE;
  _msg_full->header.flag1.sequence_num = 1;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)8, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BOOT_MODE, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)1, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestResetCommandParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 9;
  _msg_full->header.message_id = FL_MSG_ID_RESET;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_TRUE;

  _len = fl_bin_msg_build_command(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)9, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_RESET, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_COMMAND, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_TRUE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestResetOkResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 9;
  _msg_full->header.message_id = FL_MSG_ID_RESET;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_OK;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)9, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_RESET, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestResetFailResponseParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 9;
  _msg_full->header.message_id = FL_MSG_ID_RESET;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;
  _msg_full->header.flag2.error = FL_ERROR;

  _len = fl_bin_msg_build_response(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)9, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(0), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_RESET, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_RESPONSE, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_ERROR, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);
}

TEST_F(Fl_Bin_Message_Parser_UnitTest, TestButtonEventParse)
{
  _msg_full = (fl_bin_msg_full_t*)_packet_buf;

  _msg_full->header.device_id = 1;
  _msg_full->header.message_id = FL_MSG_ID_BUTTON_EVENT;
  _msg_full->header.flag1.sequence_num = 2;
  _msg_full->header.flag1.return_expected = FL_FALSE;

  fl_btn_status_t* btn_status = (fl_btn_status_t*)&_msg_full->payload;
  btn_status->button_num = 1;
  btn_status->button_value = FL_BUTTON_PRESSED;

  _len = fl_bin_msg_build_event(_packet_buf, sizeof(_packet_buf));

  for (_i = 0; _i < _len; _i++)
  {
    _ret = fl_bin_msg_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
  }

  EXPECT_EQ((fl_status_t)FL_OK, _ret);
  EXPECT_EQ((uint8_t)FL_BIN_MSG_STX, _parsed_msg.stx);
  EXPECT_EQ((uint32_t)1, _parsed_msg.header.device_id);
  EXPECT_EQ((uint8_t)calculate_length_field_value(sizeof(fl_btn_status_t)), _parsed_msg.header.length);
  EXPECT_EQ((uint8_t)FL_MSG_ID_BUTTON_EVENT, _parsed_msg.header.message_id);

  // Flag1
  EXPECT_EQ((uint8_t)FL_MSG_TYPE_EVENT, _parsed_msg.header.flag1.message_type);
  EXPECT_EQ((uint8_t)FL_FALSE, _parsed_msg.header.flag1.return_expected);
  EXPECT_EQ((uint8_t)2, _parsed_msg.header.flag1.sequence_num);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag1.reserved);

  // Flag2
  EXPECT_EQ((uint8_t)FL_OK, _parsed_msg.header.flag2.error);
  EXPECT_EQ((uint8_t)0, _parsed_msg.header.flag2.reserved);

  fl_btn_status_t* payload = (fl_btn_status_t*)&_parsed_msg.payload;
  EXPECT_EQ(1, payload->button_num);
  EXPECT_EQ(1, payload->button_value);
}