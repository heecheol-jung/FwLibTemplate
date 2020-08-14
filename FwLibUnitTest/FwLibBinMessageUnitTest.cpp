#include "pch.h"
#include "CppUnitTest.h"
#include "fw_lib_bin_message.h"
#include "fw_lib_util.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace FwLibUnitTest
{
	TEST_CLASS(FwLibBinMessageUnitTest)
	{
	private:
		uint8_t _packet_buf[FW_LIB_BIN_MSG_MAX_LENGTH];
		uint16_t _expected_crc;
		uint16_t _actual_crc;
		uint8_t _len;

	public:
		TEST_METHOD_INITIALIZE(InitializeBeforeEveryTestMethod)
		{
			memset(_packet_buf, 0, sizeof(_packet_buf));
			_expected_crc = 0;
			_actual_crc = 0;
			_len = 0;
		}

		TEST_METHOD(TestHeaderFlag1SequenceNumberSet)
		{
			wchar_t test_msg[128];

			for (uint8_t i = FW_LIB_BIN_MSG_MIN_SEQUENCE; i < FW_LIB_BIN_MSG_MAX_SEQUENCE; i++)
			{
				_packet_buf[0] = 0;
				_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], i, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS);

				swprintf(test_msg, L"Header.Flag1 sequence number should be %d.", i);
				Assert::AreEqual((uint8_t)i, _packet_buf[0], test_msg);
			}
		}

		TEST_METHOD(TestHeaderFlag1ReturnExpectedSet)
		{
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], FW_LIB_TRUE, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
			Assert::AreEqual((uint8_t)0x10, _packet_buf[0], L"Header.Flag1 return expected should be 1.");

			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], FW_LIB_FALSE, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
			Assert::AreEqual((uint8_t)0x00, _packet_buf[0], L"Header.Flag1 return expected should be 0.");
		}

		TEST_METHOD(TestHeaderFlag1MessageTypeSet)
		{
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], (uint8_t)3, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
			Assert::AreEqual((uint8_t)0x60, _packet_buf[0], L"Header.Flag1 message type should be reserved.");

			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], (uint8_t)FW_LIB_MSG_TYPE_EVENT, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
			Assert::AreEqual((uint8_t)0x40, _packet_buf[0], L"Header.Flag1 message type should be event.");

			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], (uint8_t)FW_LIB_MSG_TYPE_RESPONSE, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
			Assert::AreEqual((uint8_t)0x20, _packet_buf[0], L"Header.Flag1 message type should be response.");

			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], (uint8_t)FW_LIB_MSG_TYPE_COMMAND, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);
			Assert::AreEqual((uint8_t)0x00, _packet_buf[0], L"Header.Flag1 message type should be command.");
		}

		TEST_METHOD(TestHeaderFlag1Set)
		{
			// Sequence number : 5
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], 0x05, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS);

			// Return expected : 1
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], FW_LIB_TRUE, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);

			// Command message
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], (uint8_t)FW_LIB_MSG_TYPE_COMMAND, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);

			Assert::AreEqual((uint8_t)0x15, _packet_buf[0], L"Header.Flag1 should be 0x15.");
		}

		TEST_METHOD(TestHeaderFlag1Get)
		{
			uint8_t sequence_number = 0;
			uint8_t return_expected = 0;
			uint8_t message_type = 0;

			// Sequence number : 5
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], 0x05, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
			// Return expected : 1
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], FW_LIB_TRUE, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
			// Command message
			_packet_buf[0] = FW_LIB_BIT_FIELD_SET(_packet_buf[0], (uint8_t)FW_LIB_MSG_TYPE_COMMAND, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);

			sequence_number = FW_LIB_BIT_FIELD_GET(_packet_buf[0], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS);
			return_expected = FW_LIB_BIT_FIELD_GET(_packet_buf[0], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS);
			message_type = FW_LIB_BIT_FIELD_GET(_packet_buf[0], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS);

			Assert::AreEqual((uint8_t)0x05, sequence_number, L"Header.Flag1 sequence nubmer should be 5.");
			Assert::AreEqual((uint8_t)0x01, return_expected, L"Header.Flag1 return expected should be 1.");
			Assert::AreEqual((uint8_t)0x00, message_type, L"Header.Flag1 message type should be 0.");
		}

		TEST_METHOD(TestReadHardwareVersionCommandMessageBuild)
		{
			fw_lib_bin_msg_header_t* header = (fw_lib_bin_msg_header_t*)&_packet_buf[1];

			_len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_READ_HW_VERSION, 1, FW_LIB_TRUE, _packet_buf);

			Assert::AreEqual((uint8_t)12, _len, L"Packet length should be 12 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)6, _packet_buf[5], L"length field should 6.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

			// Flag1
			Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"sequence_number field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"return_expected field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"Message type should be fw_lib_msg_type_command.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)header, sizeof(fw_lib_bin_msg_header_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[9]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0x06, _packet_buf[9], L"CRC16 byte1 should be 0x96.");
			Assert::AreEqual((uint8_t)0x03, _packet_buf[10], L"CRC16 byte1 should be 0x4b.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[11], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestReadHardwareVersionResponseMessageBuild)
		{
			fw_bin_msg_read_hw_ver_resp_t* resp = (fw_bin_msg_read_hw_ver_resp_t*)&_packet_buf[1];

			resp->major = 1;
			resp->minor = 2;
			resp->revision = 3;
			_len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_HW_VERSION, 1, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)15, _len, L"Packet length should be 15 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)9, _packet_buf[5], L"length field should 9.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_HW_VERSION, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_READ_HW_VERSION.");

			// Flag1
			Assert::AreEqual((uint8_t)1, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 0.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.message_type should be fw_lib_msg_type_response.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			Assert::AreEqual((uint8_t)1, _packet_buf[9], L"Major should be 1.");
			Assert::AreEqual((uint8_t)2, _packet_buf[10], L"Minor should be 2.");
			Assert::AreEqual((uint8_t)3, _packet_buf[11], L"Revision should be 3.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fw_bin_msg_read_hw_ver_resp_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[12]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0xf3, _packet_buf[12], L"CRC16 byte1 should be 0xf3.");
			Assert::AreEqual((uint8_t)0x78, _packet_buf[13], L"CRC16 byte1 should be 0x78.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[14], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestReadFirmwareVersionCommandMessageBuild)
		{
			fw_lib_bin_msg_header_t* header = (fw_lib_bin_msg_header_t*)&_packet_buf[1];

			_len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_READ_FW_VERSION, 2, FW_LIB_TRUE, _packet_buf);

			Assert::AreEqual((uint8_t)12, _len, L"Packet length should be 12 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)6, _packet_buf[5], L"length field should 6.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_READ_FW_VERSION.");

			// Flag1
			Assert::AreEqual((uint8_t)2, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 2.");
			Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.messate_type should be fw_lib_msg_type_command.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1.reserved should be 0.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)header, sizeof(fw_lib_bin_msg_header_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[9]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0x0c, _packet_buf[9], L"CRC16 byte1 should be 0x0c.");
			Assert::AreEqual((uint8_t)0x06, _packet_buf[10], L"CRC16 byte1 should be 0x06.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[11], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestReadFirmwareVersionResponseMessageBuild)
		{
			fw_bin_msg_read_fw_ver_resp_t* resp = (fw_bin_msg_read_fw_ver_resp_t*)&_packet_buf[1];

			resp->major = 2;
			resp->minor = 3;
			resp->revision = 4;
			_len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_FW_VERSION, 2, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)15, _len, L"Packet length should be 15 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)9, _packet_buf[5], L"length field should 9.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_FW_VERSION, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_READ_FW_VERSION.");

			// Flag1
			Assert::AreEqual((uint8_t)2, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 0.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.message_type should be fw_lib_msg_type_response.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1.reserved should be 0.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			Assert::AreEqual((uint8_t)2, _packet_buf[9], L"Major should be 1.");
			Assert::AreEqual((uint8_t)3, _packet_buf[10], L"Minor should be 2.");
			Assert::AreEqual((uint8_t)4, _packet_buf[11], L"Revision should be 3.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fw_bin_msg_read_fw_ver_resp_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[12]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0xaa, _packet_buf[12], L"CRC16 byte1 should be 0xaa.");
			Assert::AreEqual((uint8_t)0x57, _packet_buf[13], L"CRC16 byte1 should be 0x57.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[14], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestReadGpioCommandMessageBuild)
		{
			fw_bin_msg_read_gpio_cmd_t* cmd = (fw_bin_msg_read_gpio_cmd_t*)&_packet_buf[1];

			cmd->port_number = 1;
			_len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_READ_GPIO, 3, FW_LIB_TRUE, _packet_buf);

			Assert::AreEqual((uint8_t)13, _len, L"Packet length should be 13 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)7, _packet_buf[5], L"length field should 7.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_READ_GPIO.");

			// Flag1
			Assert::AreEqual((uint8_t)3, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 3.");
			Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.messate_type should be fw_lib_msg_type_command.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1.reserved should be 0.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// Port number
			Assert::AreEqual((uint8_t)1, _packet_buf[9], L"GPIO number should be 1.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)cmd, sizeof(fw_bin_msg_read_gpio_cmd_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[10]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0x05, _packet_buf[10], L"CRC16 byte1 should be 0x05.");
			Assert::AreEqual((uint8_t)0x02, _packet_buf[11], L"CRC16 byte1 should be 0x02.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[12], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestReadGpioResponseMessageBuild)
		{
			fw_bin_msg_read_gpio_resp_t* resp = (fw_bin_msg_read_gpio_resp_t*)&_packet_buf[1];

			resp->port_value = 1;
			_len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_READ_GPIO, 3, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)13, _len, L"Packet length should be 13 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)7, _packet_buf[5], L"length field should 7.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_READ_GPIO, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_READ_GPIO.");

			// Flag1
			Assert::AreEqual((uint8_t)3, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 3.");
			Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 0.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.message_type should be fw_lib_msg_type_response.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// GPIO input value
			Assert::AreEqual((uint8_t)1, _packet_buf[9], L"GPIO value should be 1.");
			
			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fw_bin_msg_read_gpio_resp_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[10]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0xc5, _packet_buf[10], L"CRC16 byte1 should be 0xc6.");
			Assert::AreEqual((uint8_t)0x62, _packet_buf[11], L"CRC16 byte1 should be 0x62.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[12], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestWriteGpioCommandMessageBuild)
		{
			fw_bin_msg_write_gpio_cmd_t* cmd = (fw_bin_msg_write_gpio_cmd_t*)&_packet_buf[1];

			cmd->port_number = 1;
			cmd->port_value = 1;
			_len = fw_lib_bin_msg_build_command(1, FW_LIB_MSG_ID_WRITE_GPIO, 4, FW_LIB_TRUE, _packet_buf);

			Assert::AreEqual((uint8_t)14, _len, L"Packet length should be 14 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)8, _packet_buf[5], L"length field should 8.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");

			// Flag1
			Assert::AreEqual((uint8_t)4, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 4.");
			Assert::AreEqual((uint8_t)FW_LIB_TRUE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_COMMAND, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.messate_type should be fw_lib_msg_type_command.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RESERVED_POS), L"flag1.reserved should be 0.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// Port number
			Assert::AreEqual((uint8_t)1, _packet_buf[9], L"GPIO number should be 1.");
			// Port value
			Assert::AreEqual((uint8_t)1, _packet_buf[10], L"GPIO value should be 1.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)cmd, sizeof(fw_bin_msg_write_gpio_cmd_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[11]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0xa3, _packet_buf[11], L"CRC16 byte1 should be 0xa3.");
			Assert::AreEqual((uint8_t)0xd0, _packet_buf[12], L"CRC16 byte1 should be 0xd0.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[13], L"ETX should be 0x03.");
		}

		TEST_METHOD(TestWriteGpioResponseMessageBuild)
		{
			fw_lib_bin_msg_header_t* resp = (fw_lib_bin_msg_header_t*)&_packet_buf[1];

			_len = fw_lib_bin_msg_build_response(1, FW_LIB_MSG_ID_WRITE_GPIO, 4, FW_LIB_FALSE, FW_LIB_OK, _packet_buf);

			Assert::AreEqual((uint8_t)12, _len, L"Packet length should be 12 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)6, _packet_buf[5], L"length field should 6.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_WRITE_GPIO, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_WRITE_GPIO.");

			// Flag1
			Assert::AreEqual((uint8_t)4, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 4.");
			Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 0.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_RESPONSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.message_type should be fw_lib_msg_type_response.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fw_lib_bin_msg_header_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[9]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0x78, _packet_buf[9], L"CRC16 byte1 should be 0x78.");
			Assert::AreEqual((uint8_t)0x3c, _packet_buf[10], L"CRC16 byte1 should be 0x3c.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[11], L"ETX should be 0x03.");
		}
		
		TEST_METHOD(TestButtonPressedEventMessageBuild)
		{
			fw_bin_msg_button_evt_t* evt = (fw_bin_msg_button_evt_t*)&_packet_buf[1];

			evt->button_number = 1;
			evt->button_status = FW_LIB_BUTTON_PRESSED;
			_len = fw_lib_bin_msg_build_event(1, FW_LIB_MSG_ID_BUTTON_EVENT, 0, _packet_buf);

			Assert::AreEqual((uint8_t)14, _len, L"Packet length should be 14 bytes.");
			// STX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_STX, _packet_buf[0], L"STX should be 0x02.");

			// Device ID
			Assert::AreEqual((uint8_t)0, _packet_buf[1], L"device_id field(byte1) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[2], L"device_id field(byte2) should be 0.");
			Assert::AreEqual((uint8_t)0, _packet_buf[3], L"device_id field(byte3) should be 0.");
			Assert::AreEqual((uint8_t)1, _packet_buf[4], L"device_id field(byte4) should be 1.");

			// Length
			Assert::AreEqual((uint8_t)8, _packet_buf[5], L"length field should 8.");

			// Message ID
			Assert::AreEqual((uint8_t)FW_LIB_MSG_ID_BUTTON_EVENT, _packet_buf[6], L"message_id should be FW_LIB_MSG_ID_BUTTON_EVENT.");

			// Flag1
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_MASK, FW_LIB_BIN_MSG_HDR_FLG1_SEQ_NUM_POS), L"flag1.sequence_number field should be 0.");
			Assert::AreEqual((uint8_t)FW_LIB_FALSE, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_MASK, FW_LIB_BIN_MSG_HDR_FLG1_RET_EXPECTED_POS), L"flag1.return_expected field should be 0.");
			Assert::AreEqual((uint8_t)FW_LIB_MSG_TYPE_EVENT, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[7], FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_MASK, FW_LIB_BIN_MSG_HDR_FLG1_MSG_TYPE_POS), L"flag1.message_type should be fw_lib_msg_type_event.");

			// Flag2
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_ERROR_MASK, FW_LIB_BIN_MSG_HDR_FLG2_ERROR_POS), L"flag2.error should be 0.");
			Assert::AreEqual((uint8_t)0, (uint8_t)FW_LIB_BIT_FIELD_GET(_packet_buf[8], FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_MASK, FW_LIB_BIN_MSG_HDR_FLG2_RESERVED_POS), L"flag2.reserved should be 0.");

			// Button number
			Assert::AreEqual((uint8_t)1, _packet_buf[9], L"button number should be 1.");
			Assert::AreEqual((uint8_t)FW_LIB_BUTTON_PRESSED, _packet_buf[10], L"button status should be 1.");

			// CRC
			_expected_crc = fw_lib_crc_16((const unsigned char*)&_packet_buf[1], sizeof(fw_bin_msg_button_evt_t));
			_expected_crc = FW_LIB_SWAP_2BYTES(_expected_crc);
			_actual_crc = *((uint16_t*)&_packet_buf[11]);
			Assert::AreEqual((uint32_t)_expected_crc, (uint32_t)_actual_crc, L"CRC16 should be matched.");
			Assert::AreEqual((uint8_t)0x16, _packet_buf[11], L"CRC16 byte1 should be 0x16.");
			Assert::AreEqual((uint8_t)0x0a, _packet_buf[12], L"CRC16 byte1 should be 0x0a.");

			// ETX
			Assert::AreEqual((uint8_t)FW_LIB_BIN_MSG_ETX, _packet_buf[13], L"ETX should be 0x03.");
		}
	};
}
