#include "pch.h"
#include "CppUnitTest.h"
#include "fw_lib_bin_parser.h"
#include "fw_lib_util.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace FwLibUnitTest
{
	TEST_CLASS(FwLibBinParserUnitTest)
	{
  private:
    fw_lib_bin_parser_t   _bin_parser;
    fw_lib_bin_msg_full_t _parsed_msg;
    int                   _len;
    int                   _i;
    uint8_t               _packet_buf[FW_LIB_BIN_MSG_MAX_LENGTH];
    fw_lib_status_t       _ret;
    uint16_t              _expected_crc;
    uint16_t              _actual_crc;

  public:
    TEST_METHOD_INITIALIZE(InitBeforeEveryTestMethod)
    {
      fw_lib_bin_parser_init(&_bin_parser);
      memset(&_parsed_msg, 0, sizeof(fw_lib_bin_msg_full_t));
      _expected_crc = 0;
      _actual_crc = 0;
    }

    TEST_METHOD(TestReadHardwareVersionCommandParse)
    {
      _len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_READ_HW_VERSION, 1, FW_LIB_TRUE, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_command.");
      Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 1.");
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 1.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");
    }

    TEST_METHOD(TestReadHardwareVersionOkResponseParse)
    {
      fw_bin_msg_read_hw_ver_resp_t* resp = (fw_bin_msg_read_hw_ver_resp_t*)&_packet_buf[1];

      resp->major = 1;
      resp->minor = 2;
      resp->revision = 3;
      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_HW_VERSION, 1, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 2.");
      Assert::AreEqual((uint8_t)9, _parsed_msg.header.length, L"length field should be 9.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 2.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should");

      fw_bin_msg_read_hw_ver_resp_t* parsed_resp = (fw_bin_msg_read_hw_ver_resp_t*)&_parsed_msg.header;
      Assert::AreEqual((uint8_t)1, parsed_resp->major, L"major field should be 1.");
      Assert::AreEqual((uint8_t)2, parsed_resp->minor, L"minor field should be 2.");
      Assert::AreEqual((uint8_t)3, parsed_resp->revision, L"revision field should be 3.");
    }

    TEST_METHOD(TestReadHardwareVersionErrorResponseParse)
    {
      fw_bin_msg_read_hw_ver_resp_t* resp = (fw_bin_msg_read_hw_ver_resp_t*)&_packet_buf[1];

      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_HW_VERSION, 1, FW_LIB_FALSE, FW_LIB_ERROR, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 2.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 2.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 1.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should");
    }

    TEST_METHOD(TestReadFirmwareVersionCommandParse)
    {
      _len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_READ_FW_VERSION, 2, FW_LIB_TRUE, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_FW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_command.");
      Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 1.");
      Assert::AreEqual((uint8_t)2, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 2.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");
    }

    TEST_METHOD(TestReadFirmwareVersionOkResponseParse)
    {
      fw_bin_msg_read_fw_ver_resp_t* resp = (fw_bin_msg_read_fw_ver_resp_t*)&_packet_buf[1];

      resp->major = 2;
      resp->minor = 3;
      resp->revision = 4;
      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_FW_VERSION, 2, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 2.");
      Assert::AreEqual((uint8_t)9, _parsed_msg.header.length, L"length field should be 9.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_FW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)2, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 2.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should");

      fw_bin_msg_read_hw_ver_resp_t* parsed_resp = (fw_bin_msg_read_hw_ver_resp_t*)&_parsed_msg.header;
      Assert::AreEqual((uint8_t)2, parsed_resp->major, L"major field should be 2.");
      Assert::AreEqual((uint8_t)3, parsed_resp->minor, L"minor field should be 3.");
      Assert::AreEqual((uint8_t)4, parsed_resp->revision, L"revision field should be 4.");
    }

    TEST_METHOD(TestReadFirmwareVersionErrorResponseParse)
    {
      fw_bin_msg_read_hw_ver_resp_t* resp = (fw_bin_msg_read_hw_ver_resp_t*)&_packet_buf[1];

      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_FW_VERSION, 2, FW_LIB_FALSE, FW_LIB_ERROR, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 2.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)2, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 2.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 1.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should");
    }

    TEST_METHOD(TestReadGpioCommandParse)
    {
      fw_bin_msg_read_gpio_cmd_t* cmd = (fw_bin_msg_read_gpio_cmd_t*)&_packet_buf[1];

      cmd->port_number = 1;
      _len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_READ_GPIO, 3, FW_LIB_TRUE, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)7, _parsed_msg.header.length, L"length field should be 7.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_GPIO.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_command.");
      Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 1.");
      Assert::AreEqual((uint8_t)3, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 3.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

      fw_bin_msg_read_gpio_cmd_t* parsed_resp = (fw_bin_msg_read_gpio_cmd_t*)&_parsed_msg.header;
      Assert::AreEqual((uint8_t)1, parsed_resp->port_number, L"port_number field should be 1.");
    }

    TEST_METHOD(TestReadGpioOkResponseParse)
    {
      fw_bin_msg_read_gpio_resp_t* resp = (fw_bin_msg_read_gpio_resp_t*)&_packet_buf[1];

      resp->port_value = 1;
      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_GPIO, 3, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)7, _parsed_msg.header.length, L"length field should be 7.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_GPIO.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE , (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)3, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 3.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

      fw_bin_msg_read_gpio_resp_t* parsed_resp = (fw_bin_msg_read_gpio_resp_t*)&_parsed_msg.header;
      Assert::AreEqual((uint8_t)1, parsed_resp->port_value, L"port_value field should be 1.");
    }

    TEST_METHOD(TestReadGpioErrorResponseParse)
    {
      fw_bin_msg_read_hw_ver_resp_t* resp = (fw_bin_msg_read_hw_ver_resp_t*)&_packet_buf[1];

      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_GPIO, 3, FW_LIB_FALSE, FW_LIB_ERROR, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 2.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)3, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 2.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 1.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should");
    }

    TEST_METHOD(TestWriteGpioCommandParse)
    {
      fw_bin_msg_write_gpio_cmd_t* cmd = (fw_bin_msg_write_gpio_cmd_t*)&_packet_buf[1];

      cmd->port_number = 1;
      cmd->port_value = 1;
      _len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_WRITE_GPIO, 4, FW_LIB_TRUE, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)8, _parsed_msg.header.length, L"length field should be 8.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_command.");
      Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 1.");
      Assert::AreEqual((uint8_t)4, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 4.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

      fw_bin_msg_write_gpio_cmd_t* parsed_resp = (fw_bin_msg_write_gpio_cmd_t*)&_parsed_msg.header;
      Assert::AreEqual((uint8_t)1, parsed_resp->port_number, L"port_number field should be 1.");
      Assert::AreEqual((uint8_t)1, parsed_resp->port_value, L"port_value field should be 1.");
    }

    TEST_METHOD(TestWriteGpioOkResponseParse)
    {
      fw_lib_bin_msg_header_t* resp = (fw_lib_bin_msg_header_t*)&_packet_buf[1];

      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_WRITE_GPIO, 4, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)4, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 4.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");
    }

    TEST_METHOD(TestWriteGpioErrorResponseParse)
    {
      fw_lib_bin_msg_header_t* resp = (fw_lib_bin_msg_header_t*)&_packet_buf[1];

      _len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_WRITE_GPIO, 4, FW_LIB_FALSE, FW_LIB_ERROR, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)6, _parsed_msg.header.length, L"length field should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_response.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)4, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 4.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 1.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");
    }

    TEST_METHOD(TestButtonPressedEventParse)
    {
      fw_bin_msg_button_evt_t* evt = (fw_bin_msg_button_evt_t*)&_packet_buf[1];

      evt->button_number = 1;
      evt->button_status = FW_LIB_BUTTON_PRESSED;
      _len = fw_lib_bin_msg_build_event(1, FW_LIB_MSG_ID_BUTTON_EVENT, 0, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_bin_parser_parse(&_bin_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _parsed_msg.stx, L"STX should be 0x02.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.header.device_id, L"device_id field should be 1.");
      Assert::AreEqual((uint8_t)8, _parsed_msg.header.length, L"length field should be 8.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_BUTTON_EVENT, _parsed_msg.header.message_id, L"message_id should be FW_LIB_MSG_ID_BUTTON_EVENT.");

      // Flag1
      Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_EVENT, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"message_type field should be fw_lib_msg_type_event.");
      Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_num field should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag1, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1_reserved_mask field should be 0.");

      // Flag2
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
      Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_parsed_msg.header.flag2, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

      fw_bin_msg_button_evt_t* parsed_resp = (fw_bin_msg_button_evt_t*)&_parsed_msg.header;
      Assert::AreEqual((uint8_t)1, parsed_resp->button_number, L"button_number field should be 1.");
      Assert::AreEqual((uint8_t)1, parsed_resp->button_status, L"button_status field should be 1.");
    }
	};
}