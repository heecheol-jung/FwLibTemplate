#include "pch.h"
#include "CppUnitTest.h"
#include "fw_lib_txt_message.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace FwLibUnitTest
{
	TEST_CLASS(FwLibTxtMessageUnitTest)
	{
	private:
		uint8_t						_packet_buf[FW_LIB_TXT_MSG_MAX_LENGTH * 2];
		uint8_t						_len;
		fw_lib_msg_arg_t	_args[FW_LIB_TXT_MSG_MAX_ARG_COUNT];

	public:
		TEST_METHOD_INITIALIZE(InitializeBeforeEveryTestMethod)
		{
			memset(_packet_buf, 0, sizeof(_packet_buf));
			memset(_args, 0, sizeof(_args));
		}

		TEST_METHOD(TestReadHardwareVersionCommandMessageBuild)
		{
			_len = fw_lib_txt_msg_build_command(1, FW_LIB_MSG_ID_READ_HW_VERSION, NULL, _packet_buf);

			Assert::AreEqual((uint8_t)8, _len, L"Packet length should be 8 bytes.");

			Assert::AreEqual(0, strcmp("RHVER 1\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_READ_HW_VERSION command string");
		}

		TEST_METHOD(TestReadHardwareVersionResponseMessageBuild)
		{
			// Version string.
			_args[0].type = FW_LIB_ARG_TYPE_STRING;
			sprintf(_args[0].value.string_value, "1.2.3");
			_len = fw_lib_txt_msg_build_response(1, FW_LIB_MSG_ID_READ_HW_VERSION, _args, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)16, _len, L"Packet length should be 16 bytes.");

			Assert::AreEqual(0, strcmp("RHVER 1,0,1.2.3\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_READ_HW_VERSION response string");
		}

		TEST_METHOD(TestReadFirmwareVersionCommandMessageBuild)
		{
			_len = fw_lib_txt_msg_build_command(2, FW_LIB_MSG_ID_READ_FW_VERSION, NULL, _packet_buf);

			Assert::AreEqual((uint8_t)8, _len, L"Packet length should be 8 bytes.");

			Assert::AreEqual(0, strcmp("RFVER 2\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_READ_FW_VERSION command string");
		}

		TEST_METHOD(TestReadFirmwareVersionResponseMessageBuild)
		{
			// Version string.
			_args[0].type = FW_LIB_ARG_TYPE_STRING;
			sprintf(_args[0].value.string_value, "2.3.4");
			_len = fw_lib_txt_msg_build_response(2, FW_LIB_MSG_ID_READ_FW_VERSION, _args, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)16, _len, L"Packet length should be 16 bytes.");

			Assert::AreEqual(0, strcmp("RFVER 2,0,2.3.4\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_READ_FW_VERSION response string");
		}

		TEST_METHOD(TestReadGpioCommandMessageBuild)
		{
			// GPIO number.
			_args[0].type = FW_LIB_ARG_TYPE_UINT8;
			_args[0].value.uint8_value = 2;
			_len = fw_lib_txt_msg_build_command(3, FW_LIB_MSG_ID_READ_GPIO, _args, _packet_buf);

			Assert::AreEqual((uint8_t)10, _len, L"Packet length should be 10 bytes.");

			Assert::AreEqual(0, strcmp("RGPIO 3,2\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_READ_GPIO command string");
		}

		TEST_METHOD(TestReadGpioResponseMessageBuild)
		{
			// GPIO number.
			_args[0].type = FW_LIB_ARG_TYPE_UINT8;
			_args[0].value.uint8_value = 2;

			// GPIO value.
			_args[1].type = FW_LIB_ARG_TYPE_UINT8;
			_args[1].value.uint8_value = 0;

			_len = fw_lib_txt_msg_build_response(3, FW_LIB_MSG_ID_READ_GPIO, _args, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)14, _len, L"Packet length should be 14 bytes.");

			Assert::AreEqual(0, strcmp("RGPIO 3,0,2,0\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_READ_GPIO response string");
		}

		TEST_METHOD(TestWriteGpioCommandMessageBuild)
		{
			// GPIO number.
			_args[0].type = FW_LIB_ARG_TYPE_UINT8;
			_args[0].value.uint8_value = 3;

			// GPIO value.
			_args[1].type = FW_LIB_ARG_TYPE_UINT8;
			_args[1].value.uint8_value = 1;

			_len = fw_lib_txt_msg_build_command(4, FW_LIB_MSG_ID_WRITE_GPIO, _args, _packet_buf);

			Assert::AreEqual((uint8_t)12, _len, L"Packet length should be 12 bytes.");

			Assert::AreEqual(0, strcmp("WGPIO 4,3,1\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_WRITE_GPIO command string");
		}

		TEST_METHOD(TestWriteGpioResponseMessageBuild)
		{
			_len = fw_lib_txt_msg_build_response(4, FW_LIB_MSG_ID_WRITE_GPIO, NULL, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)10, _len, L"Packet length should be 10 bytes.");

			Assert::AreEqual(0, strcmp("WGPIO 4,0\n", (const char*)_packet_buf), L"Check FW_LIB_MSG_ID_WRITE_GPIO response string");
		}
	};
}