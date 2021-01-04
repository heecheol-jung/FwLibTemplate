#include "pch.h"
#include "CppUnitTest.h"
#include "fw_lib_txt_parser.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace FwLibUnitTest
{
	TEST_CLASS(FwLibTxtParserUnitTest)
	{
  private:
    fw_lib_txt_parser_t   _txt_parser;
    fw_lib_txt_msg_t      _parsed_msg;
    int                   _len;
    int                   _i;
    uint8_t               _packet_buf[FW_LIB_TXT_MSG_MAX_LENGTH];
    fw_lib_status_t       _ret;
    fw_lib_msg_arg_t	    _args[FW_LIB_TXT_MSG_MAX_ARG_COUNT];

  public:
    TEST_METHOD_INITIALIZE(InitBeforeEveryTestMethod)
    {
      fw_lib_txt_parser_init(&_txt_parser);
      memset(&_parsed_msg, 0, sizeof(fw_lib_txt_msg_t));
    }

    TEST_METHOD(TestReadHardwareVersionCommandParse)
    {
      _len = fw_lib_txt_msg_build_command(1, FW_LIB_MSG_ID_READ_HW_VERSION, NULL, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.device_id, L"Device ID should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");
      Assert::AreEqual((uint8_t)0, _parsed_msg.arg_count, L"The number of arguments should be 0.");
    }

    TEST_METHOD(TestReadHardwareVersionResponseParse)
    {
#define _HW_VER_STRING  ("1.2.3")
      
      // Version string.
      _args[0].type = FW_LIB_ARG_TYPE_STRING;
      sprintf(_args[0].value.string_value, _HW_VER_STRING);
      _len = fw_lib_txt_msg_build_response(1, FW_LIB_MSG_ID_READ_HW_VERSION, _args, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)1, _parsed_msg.device_id, L"Device ID should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");
      Assert::AreEqual((uint8_t)2, _parsed_msg.arg_count, L"The number of arguments should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_STRING, (uint8_t)_parsed_msg.args[1].type, L"Args[0] type should be fw_lib_arg_type_string.");
      Assert::AreEqual((uint8_t)0, (uint8_t)memcmp((const void*)_HW_VER_STRING, (const void*)_parsed_msg.args[1].value.string_value, strlen(_HW_VER_STRING)), L"Args[1] value should be 1.2.3.");
    }

    TEST_METHOD(TestReadFirmwareVersionCommandParse)
    {
      _len = fw_lib_txt_msg_build_command(2, FW_LIB_MSG_ID_READ_FW_VERSION, NULL, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)2, _parsed_msg.device_id, L"Device ID should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_FW_VERSION.");
      Assert::AreEqual((uint8_t)0, _parsed_msg.arg_count, L"The number of arguments should be 0.");
    }

    TEST_METHOD(TestReadFirmwareVersionResponseParse)
    {
#define _FW_VER_STRING  ("2.3.4")

      // Version string.
      _args[0].type = FW_LIB_ARG_TYPE_STRING;
      sprintf(_args[0].value.string_value, _FW_VER_STRING);
      _len = fw_lib_txt_msg_build_response(2, FW_LIB_MSG_ID_READ_FW_VERSION, _args, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)2, _parsed_msg.device_id, L"Device ID should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_FW_VERSION.");
      Assert::AreEqual((uint8_t)2, _parsed_msg.arg_count, L"The number of arguments should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_STRING, (uint8_t)_parsed_msg.args[1].type, L"Args[0] type should be fw_lib_arg_type_string.");
      Assert::AreEqual((uint8_t)0, (uint8_t)memcmp((const void*)_FW_VER_STRING, (const void*)_parsed_msg.args[1].value.string_value, strlen(_FW_VER_STRING)), L"Args[1] value should be 2.3.4");
    }

    TEST_METHOD(TestReadGpioCommandParse)
    {
      // GPIO number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 2;
      _len = fw_lib_txt_msg_build_command(3, FW_LIB_MSG_ID_READ_GPIO, _args, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)3, _parsed_msg.device_id, L"Device ID should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_GPIO.");
      Assert::AreEqual((uint8_t)1, _parsed_msg.arg_count, L"The number of arguments should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)2, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 2.");
    }

    TEST_METHOD(TestReadGpioResponseParse)
    {
      // GPIO number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 2;

      // GPIO value.
      _args[1].type = FW_LIB_ARG_TYPE_UINT8;
      _args[1].value.uint8_value = 0;

      _len = fw_lib_txt_msg_build_response(3, FW_LIB_MSG_ID_READ_GPIO, _args, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)3, _parsed_msg.device_id, L"Device ID should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_GPIO.");
      Assert::AreEqual((uint8_t)3, _parsed_msg.arg_count, L"The number of arguments should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[1].type, L"Args[1] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)2, (uint8_t)_parsed_msg.args[1].value.uint8_value, L"Args[1] value should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[2].type, L"Args[2] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[2].value.uint8_value, L"Args[2] value should be 0.");
    }

    TEST_METHOD(TestWriteGpioCommandParse)
    {
      // GPIO number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 3;

      // GPIO value.
      _args[1].type = FW_LIB_ARG_TYPE_UINT8;
      _args[1].value.uint8_value = 1;

      _len = fw_lib_txt_msg_build_command(4, FW_LIB_MSG_ID_WRITE_GPIO, _args, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)4, _parsed_msg.device_id, L"Device ID should be 4.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");
      Assert::AreEqual((uint8_t)2, _parsed_msg.arg_count, L"The number of arguments should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)3, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[1].type, L"Args[1] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)1, (uint8_t)_parsed_msg.args[1].value.uint8_value, L"Args[1] value should be 1.");
    }

    TEST_METHOD(TestWriteGpioResponseParse)
    {
      _len = fw_lib_txt_msg_build_response(4, FW_LIB_MSG_ID_WRITE_GPIO, NULL, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)4, _parsed_msg.device_id, L"Device ID should be 4.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");
      Assert::AreEqual((uint8_t)1, _parsed_msg.arg_count, L"The number of arguments should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
    }

    TEST_METHOD(TestButtonPressedEventParse)
    {
      // Button number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 4;

      // BUtton value.
      _args[1].type = FW_LIB_ARG_TYPE_UINT8;
      _args[1].value.uint8_value = 1;

      _len = fw_lib_txt_msg_build_event(5, FW_LIB_MSG_ID_BUTTON_EVENT, _args, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)5, _parsed_msg.device_id, L"Device ID should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_BUTTON_EVENT, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_BUTTON_EVENT.");
      Assert::AreEqual((uint8_t)2, _parsed_msg.arg_count, L"The number of arguments should be 2.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)4, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 4.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[1].type, L"Args[1] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)1, (uint8_t)_parsed_msg.args[1].value.uint8_value, L"Args[1] value should be 1.");
    }

    TEST_METHOD(TestReadTemperatureCommandParse)
    {
      // Sensor number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 5;
      _len = fw_lib_txt_msg_build_command(6, FW_LIB_MSG_ID_READ_TEMPERATURE, _args, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)6, _parsed_msg.device_id, L"Device ID should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_TEMPERATURE, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_TEMPERATURE.");
      Assert::AreEqual((uint8_t)1, _parsed_msg.arg_count, L"The number of arguments should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)5, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 5.");
    }

    TEST_METHOD(TestReadTemperatureResponseParse)
    {
      // Sensor number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 5;

      // Temperature value.
      _args[1].type = FW_LIB_ARG_TYPE_UINT16;
      _args[1].value.uint16_value = 123;

      _len = fw_lib_txt_msg_build_response(6, FW_LIB_MSG_ID_READ_TEMPERATURE, _args, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)6, _parsed_msg.device_id, L"Device ID should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_TEMPERATURE, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_TEMPERATURE.");
      Assert::AreEqual((uint8_t)3, _parsed_msg.arg_count, L"The number of arguments should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[1].type, L"Args[1] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)5, (uint8_t)_parsed_msg.args[1].value.uint8_value, L"Args[1] value should be 5.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_DOUBLE, (uint8_t)_parsed_msg.args[2].type, L"Args[2] type should be FW_LIB_ARG_TYPE_DOUBLE.");
      Assert::AreEqual((double)12.3, (double)_parsed_msg.args[2].value.double_value, L"Args[2] value should be 12.3.");
    }

    TEST_METHOD(TestReadHumidityCommandParse)
    {
      // Sensor number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 5;
      _len = fw_lib_txt_msg_build_command(6, FW_LIB_MSG_ID_READ_HUMIDITY, _args, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)6, _parsed_msg.device_id, L"Device ID should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HUMIDITY, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_HUMIDITY.");
      Assert::AreEqual((uint8_t)1, _parsed_msg.arg_count, L"The number of arguments should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)5, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 5.");
    }

    TEST_METHOD(TestReadHumidityResponseParse)
    {
      // Sensor number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 5;

      // Temperature value.
      _args[1].type = FW_LIB_ARG_TYPE_UINT16;
      _args[1].value.uint16_value = 456;

      _len = fw_lib_txt_msg_build_response(6, FW_LIB_MSG_ID_READ_HUMIDITY, _args, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)6, _parsed_msg.device_id, L"Device ID should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HUMIDITY, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_HUMIDITY.");
      Assert::AreEqual((uint8_t)3, _parsed_msg.arg_count, L"The number of arguments should be 3.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[1].type, L"Args[1] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)5, (uint8_t)_parsed_msg.args[1].value.uint8_value, L"Args[1] value should be 5.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_DOUBLE, (uint8_t)_parsed_msg.args[2].type, L"Args[2] type should be FW_LIB_ARG_TYPE_DOUBLE.");
      Assert::AreEqual((double)45.6, (double)_parsed_msg.args[2].value.double_value, L"Args[2] value should be 45.6.");
    }

    TEST_METHOD(TestReadTemperatureAndHumidityCommandParse)
    {
      // Sensor number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 5;
      _len = fw_lib_txt_msg_build_command(6, FW_LIB_MSG_ID_READ_TEMP_AND_HUM, _args, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_command(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)6, _parsed_msg.device_id, L"Device ID should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_TEMP_AND_HUM.");
      Assert::AreEqual((uint8_t)1, _parsed_msg.arg_count, L"The number of arguments should be 1.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)5, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 5.");
    }

    TEST_METHOD(TestReadTemperatureAndHumidityResponseParse)
    {
      // Sensor number.
      _args[0].type = FW_LIB_ARG_TYPE_UINT8;
      _args[0].value.uint8_value = 5;

      // Temperature value.
      _args[1].type = FW_LIB_ARG_TYPE_UINT16;
      _args[1].value.uint16_value = 123;

      // Humidity value.
      _args[2].type = FW_LIB_ARG_TYPE_UINT16;
      _args[2].value.uint16_value = 456;

      _len = fw_lib_txt_msg_build_response(6, FW_LIB_MSG_ID_READ_TEMP_AND_HUM, _args, FW_LIB_OK, _packet_buf);

      for (_i = 0; _i < _len; _i++)
      {
        _ret = fw_lib_txt_parser_parse_response_event(&_txt_parser, _packet_buf[_i], &_parsed_msg);
      }

      Assert::AreEqual((fw_lib_status_t)FW_LIB_OK, _ret, L"Parse result should be FW_LIB_OK.");
      Assert::AreEqual((uint32_t)6, _parsed_msg.device_id, L"Device ID should be 6.");
      Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_TEMP_AND_HUM, _parsed_msg.msg_id, L"message_id should be FW_LIB_MSG_ID_READ_TEMP_AND_HUM.");
      Assert::AreEqual((uint8_t)4, _parsed_msg.arg_count, L"The number of arguments should be 4.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[0].type, L"Args[0] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)0, (uint8_t)_parsed_msg.args[0].value.uint8_value, L"Args[0] value should be 0.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_UINT8, (uint8_t)_parsed_msg.args[1].type, L"Args[1] type should be fw_lib_arg_type_uint8.");
      Assert::AreEqual((uint8_t)5, (uint8_t)_parsed_msg.args[1].value.uint8_value, L"Args[1] value should be 5.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_DOUBLE, (uint8_t)_parsed_msg.args[2].type, L"Args[2] type should be FW_LIB_ARG_TYPE_DOUBLE.");
      Assert::AreEqual((double)12.3, (double)_parsed_msg.args[2].value.double_value, L"Args[2] value should be 12.3.");
      Assert::AreEqual((uint8_t)FW_LIB_ARG_TYPE_DOUBLE, (uint8_t)_parsed_msg.args[3].type, L"Args[2] type should be FW_LIB_ARG_TYPE_DOUBLE.");
      Assert::AreEqual((double)45.6, (double)_parsed_msg.args[3].value.double_value, L"Args[3] value should be 45.6.");
    }
	};
}
