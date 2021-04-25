#ifndef DEVICE_SIMULATOR_H
#define DEVICE_SIMULATOR_H

#include <Windows.h>
#include "fl_txt_message_parser.h"
#include "fl_bin_message_parser.h"

#include "app_common.h"

typedef struct
{
  uint32_t                  device_id;
  char                      tx_buf[APP_LIB_MAX_BUF_LEN];
  uint32_t                  tx_len;
  char                      rx_buf[APP_LIB_MAX_BUF_LEN];
  uint32_t                  rx_len;
  HANDLE                    serial_handle;
  HANDLE                    thread_handle;
  unsigned int              thread_id;
  uint8_t                   loop;
  uint8_t                   msg_type; // APP_LIB_MSG_BIN, APP_LIB_MSG_TXT
  uint8_t                   fw_buf[128 * 1024]; // binary firmware receive buffer
  uint16_t                  fw_len; // Total size of the received binary firwmware.
  char                      com_port_name[APP_LIB_COM_PORT_NAME_BUF_LEN];
  uint8_t                   how_to_handle_parse_result; // APP_LIB_PARSE_RESULT_NONE, APP_LIB_PARSE_REUSLT_WITH_CALLBACK, APP_LIB_PARSE_RESULT_WITH_OUT_PARAM

  // Application firmware text message parser.
  fl_txt_msg_parser_t       app_txt_parser;
  // Application firmware binary message parser.
  fl_bin_msg_parser_t       app_bin_parser;
} device_simulator;

APP_LIB_BEGIN_DECLS

// Initialize a device simulator.
void ds_init(device_simulator* handle);

// Start a device simulator.
int ds_start(device_simulator* handle);

// Stop a device simulator.
int ds_stop(device_simulator* handle);

// Send a packet for a device simulator.
int ds_send_packet(device_simulator* handle, uint8_t* buf, uint16_t len);

uint8_t ds_is_serial_open(device_simulator* handle);

void ds_set_how_to_handle_parse_result(device_simulator* handle, uint8_t result_handling_type);

void ds_button_press(device_simulator* handle);

void ds_button_release(device_simulator* handle);

APP_LIB_END_DECLS

#endif
