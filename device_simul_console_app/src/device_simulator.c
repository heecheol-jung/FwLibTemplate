#include <stdio.h>
#include <tchar.h>
#include <process.h>
#include <time.h>
#include "device_simulator.h"
#include "fl_util.h"

#define SERIAL_INPUT_Q_SIZE     (4096)
#define SERIAL_OUTPUT_Q_SIZE    (4096)

#define APP_HW_VER              ("a.1.2.3")
#define APP_FW_VER              ("a.2.3.4")
#define BL_HW_VER               ("b.1.2.3")
#define BL_FW_VER               ("b.2.3.4")

static void serial_open(device_simulator* handle);
static void serial_close(device_simulator* handle);
static void print_error_message(DWORD error_code);
static void start_message_thread(device_simulator* handle);
static void stop_message_thread(device_simulator* handle);
static uint8_t is_thread_created(device_simulator* handle);
static unsigned __stdcall message_processing_proc(void* pparam);

static void process_app_bin_msg(device_simulator* handle, DWORD read_bytes);
static void process_app_bin_hw_ver_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_fw_ver_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_read_gpio_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_write_gpio_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_read_temperature_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_read_humidity_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_read_temp_and_hum_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_boot_mode_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);
static void process_app_bin_reset_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full);

static void on_app_bin_message_parsed(const void* parser_handle, void* context);

static void process_app_txt_msg(device_simulator* handle, DWORD read_bytes);
static void process_app_txt_hw_ver_cmd(device_simulator* handle);
static void process_app_txt_fw_ver_cmd(device_simulator* handle);
static void process_app_txt_read_gpio_cmd(device_simulator* handle);
static void process_app_txt_write_gpio_cmd(device_simulator* handle);
static void process_app_txt_read_temperature_cmd(device_simulator* handle);
static void process_app_txt_read_humidity_cmd(device_simulator* handle);
static void process_app_txt_read_temp_and_hum_cmd(device_simulator* handle);
static void process_app_txt_boot_mode_cmd(device_simulator* handle);
static void process_app_txt_reset_cmd(device_simulator* handle);
static void on_app_txt_message_parsed(const void* parser_handle, void* context);

///////////////////////////////////////////////////////////////////////////////
// Public functions
///////////////////////////////////////////////////////////////////////////////
void ds_init(device_simulator* handle)
{
  memset(handle, 0, sizeof(device_simulator));
  handle->serial_handle = INVALID_HANDLE_VALUE;
  handle->thread_handle = INVALID_HANDLE_VALUE;

  fl_txt_msg_parser_init(&handle->app_txt_parser);

  fl_bin_msg_parser_init(&handle->app_bin_parser);

  handle->device_id = 1;
}

// Start a device simulator.
int ds_start(device_simulator* handle)
{
  serial_open(handle);

  if (ds_is_serial_open(handle) != 1)
  {
    return 1;
  }

  start_message_thread(handle);

  return 0;
}

// Stop a device simulator.
int ds_stop(device_simulator* handle)
{
  stop_message_thread(handle);

  if (ds_is_serial_open(handle) == 1)
  {
    serial_close(handle);
  }

  return 0;
}

// Send a packet for a device simulator.
int ds_send_packet(device_simulator* handle, uint8_t* buf, uint16_t len)
{
  if (ds_is_serial_open(handle) != 1)
  {
    return 1;
  }

  DWORD written_bytes = 0;

  if (WriteFile(handle->serial_handle, buf, len, &written_bytes, NULL) == TRUE)
  {
    return 0;
  }
  else
  {
    return 2;
  }
}

uint8_t ds_is_serial_open(device_simulator* handle)
{
  if (handle->serial_handle == INVALID_HANDLE_VALUE)
  {
    return 0;
  }
  else
  {
    return 1;
  }
}

void ds_set_how_to_handle_parse_result(device_simulator* handle, uint8_t result_handling_type)
{
  if (result_handling_type == APP_LIB_PARSE_RESULT_NONE)
  {
    handle->how_to_handle_parse_result = result_handling_type;
    handle->app_bin_parser.on_parsed_callback = NULL;
    handle->app_bin_parser.context = NULL;
    handle->app_txt_parser.on_parsed_callback = NULL;
    handle->app_txt_parser.context = NULL;
  }
  else if (result_handling_type == APP_LIB_PARSE_REUSLT_WITH_CALLBACK)
  {
    handle->how_to_handle_parse_result = result_handling_type;

    handle->app_bin_parser.on_parsed_callback = on_app_bin_message_parsed;
    handle->app_bin_parser.context = handle;

    handle->app_txt_parser.on_parsed_callback = on_app_txt_message_parsed;
    handle->app_txt_parser.context = handle;
  }
  else if (result_handling_type == APP_LIB_PARSE_RESULT_WITH_OUT_PARAM)
  {
    handle->how_to_handle_parse_result = result_handling_type;
    handle->app_bin_parser.on_parsed_callback = NULL;
    handle->app_bin_parser.context = NULL;
    handle->app_txt_parser.on_parsed_callback = NULL;
    handle->app_txt_parser.context = NULL;
  }
}

void ds_button_press(device_simulator* handle)
{
  if (ds_is_serial_open(handle) == 0)
  {
    APP_LIB_DEBUG_PRINT(("Serial port is not opened."));
    return;
  }

  if (handle->msg_type == APP_LIB_MSG_BIN)
  {
    fl_bin_msg_full_t* tx_msg_full = (fl_bin_msg_full_t*)handle->tx_buf;

    tx_msg_full->header.device_id = 1;
    tx_msg_full->header.message_id = FL_MSG_ID_BUTTON_EVENT;
    tx_msg_full->header.flag1.sequence_num = 1;
    tx_msg_full->header.flag1.return_expected = FL_FALSE;
    tx_msg_full->header.flag2.error = FL_OK;

    fl_btn_status_t* btn_status = (fl_btn_status_t*)&tx_msg_full->payload;
    btn_status->button_num = 1;
    btn_status->button_value = FL_BUTTON_PRESSED;

    handle->tx_len = fl_bin_msg_build_event((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
  }
  else if (handle->msg_type == APP_LIB_MSG_TXT)
  {
    fl_btn_status_t button;
    button.button_num = 2;
    button.button_value = FL_BUTTON_PRESSED;

    handle->tx_len = fl_txt_msg_build_event(1, FL_MSG_ID_BUTTON_EVENT,
      &button, sizeof(fl_btn_status_t),
      (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
  }
  

  ds_send_packet(handle, handle->tx_buf, handle->tx_len);
  handle->tx_len = 0;
}

void ds_button_release(device_simulator* handle)
{
  if (ds_is_serial_open(handle) == 0)
  {
    APP_LIB_DEBUG_PRINT(("Serial port is not opened."));
    return;
  }

  if (handle->msg_type == APP_LIB_MSG_BIN)
  {
    fl_bin_msg_full_t* tx_msg_full = (fl_bin_msg_full_t*)handle->tx_buf;

    tx_msg_full->header.device_id = 1;
    tx_msg_full->header.message_id = FL_MSG_ID_BUTTON_EVENT;
    tx_msg_full->header.flag1.sequence_num = 1;
    tx_msg_full->header.flag1.return_expected = FL_FALSE;
    tx_msg_full->header.flag2.error = FL_OK;

    fl_btn_status_t* btn_status = (fl_btn_status_t*)&tx_msg_full->payload;
    btn_status->button_num = 1;
    btn_status->button_value = FL_BUTTON_RELEASED;

    handle->tx_len = fl_bin_msg_build_event((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
  }
  else if (handle->msg_type == APP_LIB_MSG_TXT)
  {
    fl_btn_status_t button;
    button.button_num = 2;
    button.button_value = FL_BUTTON_RELEASED;

    handle->tx_len = fl_txt_msg_build_event(1, FL_MSG_ID_BUTTON_EVENT,
      &button, sizeof(fl_btn_status_t),
      (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
  }

  ds_send_packet(handle, handle->tx_buf, handle->tx_len);
  handle->tx_len = 0;
}

///////////////////////////////////////////////////////////////////////////////
// Private functions
///////////////////////////////////////////////////////////////////////////////
static void serial_open(device_simulator* handle)
{
  HANDLE serial_handle = INVALID_HANDLE_VALUE;
  TCHAR path[MAX_PATH] = { 0 };
  const int buf_size = sizeof(path) / sizeof(path[0]) - 1;
  int len = 0;
  DCB             dcb;
  COMMTIMEOUTS    cto;

  if (strlen(handle->com_port_name) == 0)
  {
    APP_LIB_DEBUG_PRINT(("[%s] Invalid COM port name\n", __FUNCTION__));
  }

  len = _sntprintf_s(path, buf_size, MAX_PATH - 1, _T("\\\\.\\%s"), handle->com_port_name);
  serial_handle = CreateFile(
    path,     // address of name of the communications device
    GENERIC_READ | GENERIC_WRITE,          // access (read-write) mode
    0,                  // share mode
    NULL,               // address of security descriptor
    OPEN_EXISTING,      // how to create
    0,                  // file attributes
    NULL                // handle of file with attributes to copy
  );

  if (serial_handle == INVALID_HANDLE_VALUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] CreateFile failed\n", __FUNCTION__));
    return;
  }

  if (SetCommMask(serial_handle, EV_RXCHAR) != TRUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] SetCommMask failed\n", __FUNCTION__));

    serial_close(handle);
    return;
  }

  if (SetupComm(serial_handle, SERIAL_INPUT_Q_SIZE, SERIAL_OUTPUT_Q_SIZE) != TRUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] SetupComm failed\n", __FUNCTION__));

    serial_close(handle);
    return;
  }

  if (PurgeComm(serial_handle, PURGE_TXABORT | PURGE_TXCLEAR | PURGE_RXABORT | PURGE_RXCLEAR) != TRUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] PurgeComm failed\n", __FUNCTION__));

    serial_close(handle);
    return;
  }

  cto.ReadIntervalTimeout = MAXDWORD;
  cto.ReadTotalTimeoutMultiplier = 0;
  cto.ReadTotalTimeoutConstant = 0;
  cto.WriteTotalTimeoutConstant = 0;
  if (SetCommTimeouts(serial_handle, &cto) != TRUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] SetCommTimeouts failed\n", __FUNCTION__));

    serial_close(handle);
    return;
  }

  if (GetCommState(serial_handle, &dcb) != TRUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] GetCommState failed\n", __FUNCTION__));

    serial_close(handle);
    return;
  }

  dcb.BaudRate = 115200;
  dcb.ByteSize = 8;
  dcb.fParity = FALSE;
  dcb.Parity = NOPARITY;
  dcb.StopBits = ONESTOPBIT;
  if (SetCommState(serial_handle, &dcb) != TRUE)
  {
    print_error_message(GetLastError());
    APP_LIB_DEBUG_PRINT(("[%s] SetCommState failed\n", __FUNCTION__));

    serial_close(handle);
    return;
  }
  handle->serial_handle = serial_handle;
  APP_LIB_DEBUG_PRINT(("[%s] Serial port opened.\n\n", __FUNCTION__));
}

static void serial_close(device_simulator* handle)
{
  if (handle->serial_handle != INVALID_HANDLE_VALUE)
  {
    CloseHandle(handle->serial_handle);
    handle->serial_handle = INVALID_HANDLE_VALUE;

    APP_LIB_DEBUG_PRINT(("[%s] Serial port closed\n", __FUNCTION__));
  }
}

static void print_error_message(DWORD error_code)
{
  LPVOID lpMsgBuf = NULL;
  LPVOID lpDisplayBuf = NULL;

  FormatMessage(
    FORMAT_MESSAGE_ALLOCATE_BUFFER |
    FORMAT_MESSAGE_FROM_SYSTEM |
    FORMAT_MESSAGE_IGNORE_INSERTS,
    NULL,
    error_code,
    MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
    (LPTSTR)&lpMsgBuf,
    0, NULL);

  if (lpMsgBuf != NULL)
  {
    _tprintf(_T("%s"), (TCHAR*)lpMsgBuf);
    LocalFree(lpMsgBuf);
  }
}

static void start_message_thread(device_simulator* handle)
{
  handle->loop = 1;

  // https://docs.microsoft.com/en-us/cpp/c-runtime-library/reference/beginthread-beginthreadex?redirectedfrom=MSDN&view=vs-2019
  // _beginthreadex returns 0 on failure, rather than -1L.
  handle->thread_handle = (HANDLE)_beginthreadex(NULL, 0, message_processing_proc, handle, 0, &handle->thread_id);
  if (handle->thread_handle == 0)
  {
    handle->loop = 0;
    handle->thread_id = 0;
    APP_LIB_DEBUG_PRINT(("[%s] _beginthreadex failed\n", __FUNCTION__));
    return;
  }

  APP_LIB_DEBUG_PRINT(("message thread started\n"));
}

static void stop_message_thread(device_simulator* handle)
{
  if (handle->thread_handle != INVALID_HANDLE_VALUE)
  {
    handle->loop = 0;
    WaitForSingleObject(handle->thread_handle, INFINITE);
    CloseHandle(handle->thread_handle);
    handle->thread_handle = INVALID_HANDLE_VALUE;
    handle->thread_id = 0;
    APP_LIB_DEBUG_PRINT(("message thread stopped\n"));
  }
}

static uint8_t is_thread_created(device_simulator* handle)
{
  if (handle->thread_handle != INVALID_HANDLE_VALUE)
  {
    return 1;
  }
  else
  {
    return 0;
  }
}

static unsigned __stdcall message_processing_proc(void* pparam)
{
  device_simulator* handle = (device_simulator*)pparam;
  DWORD read_bytes = 0;

  APP_LIB_DEBUG_PRINT(("%s enter.\n", __FUNCTION__));

  while (handle->loop == 1)
  {
    if ((ReadFile(handle->serial_handle, handle->rx_buf, sizeof(handle->rx_buf), &read_bytes, NULL) == TRUE) &&
      (read_bytes > 0))
    {
      APP_LIB_DEBUG_PRINT(("Read bytes : %d\n", read_bytes));

      if (handle->msg_type == APP_LIB_MSG_BIN)
      {
        process_app_bin_msg(handle, read_bytes);
      }
      else if (handle->msg_type == APP_LIB_MSG_TXT)
      {
        process_app_txt_msg(handle, read_bytes);
      }
    }

    Sleep(1);
  }
  //_endthreadex(0);

  APP_LIB_DEBUG_PRINT(("%s leave.\n", __FUNCTION__));
  return 0;
}

///////////////////////////////////////////////////////////////////////////////
// Application firmware binary message processing
///////////////////////////////////////////////////////////////////////////////
static void process_app_bin_msg(device_simulator* handle, DWORD read_bytes)
{
  fl_status_t ret = FL_ERROR;

  for (DWORD i = 0; i < read_bytes; i++)
  {
    ret = fl_bin_msg_parser_parse(&handle->app_bin_parser, handle->rx_buf[i], NULL);
    if (ret == FL_OK)
    {
      fl_bin_msg_header_t* header = (fl_bin_msg_header_t*)&(handle->app_bin_parser.buf[1]);
      fl_bin_msg_full_t* tx_msg_full = (fl_bin_msg_full_t*)handle->tx_buf;

      tx_msg_full->header.device_id = header->device_id;
      tx_msg_full->header.message_id = header->message_id;
      tx_msg_full->header.flag1.sequence_num = header->flag1.sequence_num;
      tx_msg_full->header.flag1.return_expected = FL_FALSE;
      tx_msg_full->header.flag2.error = FL_OK;

      APP_LIB_DEBUG_PRINT(("process_app_bin_msg\n"));

      switch (header->message_id)
      {
      case FL_MSG_ID_READ_HW_VERSION:
        process_app_bin_hw_ver_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_READ_FW_VERSION:
        process_app_bin_fw_ver_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_READ_GPIO:
        process_app_bin_read_gpio_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_WRITE_GPIO:
        process_app_bin_write_gpio_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_READ_TEMPERATURE:
        process_app_bin_read_temperature_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_READ_HUMIDITY:
        process_app_bin_read_humidity_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_READ_TEMP_AND_HUM:
        process_app_bin_read_temp_and_hum_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_BOOT_MODE:
        process_app_bin_boot_mode_cmd(handle, tx_msg_full);
        break;

      case FL_MSG_ID_RESET:
        process_app_bin_reset_cmd(handle, tx_msg_full);
        break;
      }

      if (handle->tx_len > 0)
      {
        ds_send_packet(handle, (uint8_t*)handle->tx_buf, handle->tx_len);
        handle->tx_len = 0;
      }
    }

    if (ret != FL_BIN_MSG_PARSER_PARSING)
    {
      fl_bin_msg_parser_clear(&handle->app_bin_parser);
    }
  }
}

static void on_app_bin_message_parsed(const void* parser_handle, void* context)
{
  fl_bin_msg_parser_t* parser = (fl_bin_msg_parser_t*)parser_handle;
  device_simulator* ds = (device_simulator*)parser->context;
  fl_bin_msg_header_t* header = (fl_bin_msg_header_t*)&(parser->buf[1]);
  fl_bin_msg_full_t* tx_msg_full = (fl_bin_msg_full_t*)ds->tx_buf;

  tx_msg_full->header.device_id = header->device_id;
  tx_msg_full->header.message_id = header->message_id;
  tx_msg_full->header.flag1.sequence_num = header->flag1.sequence_num;
  tx_msg_full->header.flag1.return_expected = FL_FALSE;
  tx_msg_full->header.flag2.error = FL_OK;

  APP_LIB_DEBUG_PRINT(("on_app_bin_message_parsed callback\n"));

  switch (header->message_id)
  {
  case FL_MSG_ID_READ_HW_VERSION:
    process_app_bin_hw_ver_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_READ_FW_VERSION:
    process_app_bin_fw_ver_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_READ_GPIO:
    process_app_bin_read_gpio_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_WRITE_GPIO:
    process_app_bin_write_gpio_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_READ_TEMPERATURE:
    process_app_bin_read_temperature_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_READ_HUMIDITY:
    process_app_bin_read_humidity_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_READ_TEMP_AND_HUM:
    process_app_bin_read_temp_and_hum_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_BOOT_MODE:
    process_app_bin_boot_mode_cmd(ds, tx_msg_full);
    break;

  case FL_MSG_ID_RESET:
    process_app_bin_reset_cmd(ds, tx_msg_full);
    break;
  }

  if (ds->tx_len > 0)
  {
    ds_send_packet(ds, (uint8_t*)ds->tx_buf, ds->tx_len);
    ds->tx_len = 0;
  }
}

static void process_app_bin_hw_ver_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  
  fl_hw_ver_t* hw_ver = (fl_hw_ver_t*)&(tx_msg_full->payload);
  sprintf(hw_ver->version, APP_HW_VER);

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_fw_ver_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  
  fl_fw_ver_t* fw_ver = (fl_fw_ver_t*)&(tx_msg_full->payload);
  sprintf(fw_ver->version, APP_FW_VER);

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_read_gpio_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  fl_gpi_port_t* gpi_port = (fl_gpi_port_t*)&(rx_msg_full->payload);
  
  APP_LIB_DEBUG_PRINT(("GPI port number : %d\n", gpi_port->port_num));

  time_t t;
  fl_gpi_port_value_t* gpi_port_val = (fl_gpi_port_value_t*)&(tx_msg_full->payload);

  // GPIO number.
  gpi_port_val->port_num = gpi_port->port_num;

  srand((unsigned)time(&t));
  // GPIO value.
  gpi_port_val->port_value = rand() % 2;

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_write_gpio_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  fl_gpo_port_value_t* gpo_port_value = (fl_gpo_port_value_t*)&(rx_msg_full->payload);
  
  APP_LIB_DEBUG_PRINT(("GPO port number : %d, Port value : %d\n", gpo_port_value->port_num, gpo_port_value->port_value));

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_read_temperature_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  fl_sensor_t* sensor = (fl_sensor_t*)&(rx_msg_full->payload);
  
  fl_temp_sensor_read_t* sensor_read = (fl_temp_sensor_read_t*)&(tx_msg_full->payload);
  time_t t;

  APP_LIB_DEBUG_PRINT(("Temperature sensor number : %d\n", sensor->sensor_num));

  srand((unsigned)time(&t));

  // Sensor number.
  sensor_read->sensor_num = sensor->sensor_num;
  // Sensor value.
  sensor_read->temperature = ((double)rand() / (double)RAND_MAX) * 30.0;

  APP_LIB_DEBUG_PRINT(("Temperature : %f\n", sensor_read->temperature));

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_read_humidity_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  fl_sensor_t* sensor = (fl_sensor_t*)&(rx_msg_full->payload);
  fl_hum_sensor_read_t* sensor_read = (fl_hum_sensor_read_t*)&(tx_msg_full->payload);
  time_t t;

  APP_LIB_DEBUG_PRINT(("Humidity sensor number : %d\n", sensor->sensor_num));

  srand((unsigned)time(&t));

  // Sensor number.
  sensor_read->sensor_num = sensor->sensor_num;
  // Sensor value.
  sensor_read->humidity = ((double)rand() / (double)RAND_MAX) * 100.0;

  APP_LIB_DEBUG_PRINT(("Humidity : %f\n", sensor_read->humidity));

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_read_temp_and_hum_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  fl_sensor_t* sensor = (fl_sensor_t*)&(rx_msg_full->payload);
  
  APP_LIB_DEBUG_PRINT(("Temperature/Humidity sensor number : %d\n", sensor->sensor_num));

  fl_temp_hum_sensor_read_t* sensor_read = (fl_temp_hum_sensor_read_t*)&(tx_msg_full->payload);
  time_t t;

  srand((unsigned)time(&t));

  // Sensor number.
  sensor_read->sensor_num = sensor->sensor_num;
  // Sensor value.
  sensor_read->temperature = ((double)rand() / (double)RAND_MAX) * 30.0;
  sensor_read->humidity = ((double)rand() / (double)RAND_MAX) * 100.0;

  APP_LIB_DEBUG_PRINT(("Temperature : %f, Humidity : %f\n", sensor_read->temperature, sensor_read->humidity));

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_boot_mode_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  fl_boot_mode_t* boot_mode = (fl_boot_mode_t*)&(rx_msg_full->payload);
  
  APP_LIB_DEBUG_PRINT(("Boot mode : %d\n", boot_mode->boot_mode));

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_bin_reset_cmd(device_simulator* handle, fl_bin_msg_full_t* tx_msg_full)
{
  fl_bin_msg_full_t* rx_msg_full = (fl_bin_msg_full_t*)handle->app_bin_parser.buf;
  
  APP_LIB_DEBUG_PRINT(("Reset\n"));

  handle->tx_len = fl_bin_msg_build_response((uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

///////////////////////////////////////////////////////////////////////////////
// Application firmware text message processing
///////////////////////////////////////////////////////////////////////////////
static void process_app_txt_msg(device_simulator* handle, DWORD read_bytes)
{
  for (DWORD i = 0; i < read_bytes; i++)
  {
    if (fl_txt_msg_parser_parse_command(&handle->app_txt_parser, handle->rx_buf[i], NULL) != FL_TXT_MSG_PARSER_PARSING)
    {
      switch (handle->app_txt_parser.msg_id)
      {
      case FL_MSG_ID_READ_HW_VERSION:
        process_app_txt_hw_ver_cmd(handle);
        break;

      case FL_MSG_ID_READ_FW_VERSION:
        process_app_txt_fw_ver_cmd(handle);
        break;

      case FL_MSG_ID_READ_GPIO:
        process_app_txt_read_gpio_cmd(handle);
        break;

      case FL_MSG_ID_WRITE_GPIO:
        process_app_txt_write_gpio_cmd(handle);
        break;

      case FL_MSG_ID_READ_TEMPERATURE:
        process_app_txt_read_temperature_cmd(handle);
        break;

      case FL_MSG_ID_READ_HUMIDITY:
        process_app_txt_read_humidity_cmd(handle);
        break;

      case FL_MSG_ID_READ_TEMP_AND_HUM:
        process_app_txt_read_temp_and_hum_cmd(handle);
        break;

      case FL_MSG_ID_BOOT_MODE:
        process_app_txt_boot_mode_cmd(handle);
        break;

      case FL_MSG_ID_RESET:
        process_app_txt_reset_cmd(handle);
        break;
      }

      if (handle->tx_len > 0)
      {
        ds_send_packet(handle, (uint8_t*)handle->tx_buf, handle->tx_len);
        handle->tx_len = 0;
      }

      fl_txt_msg_parser_clear(&handle->app_txt_parser);
    }
  }
}

static void on_app_txt_message_parsed(const void* parser_handle, void* context)
{
  fl_txt_msg_parser_t* parser = (fl_txt_msg_parser_t*)parser_handle;
  device_simulator* ds = (device_simulator*)parser->context;

  APP_LIB_DEBUG_PRINT(("on_app_txt_message_parsed callback\n"));

  switch (parser->msg_id)
  {
  case FL_MSG_ID_READ_HW_VERSION:
    process_app_txt_hw_ver_cmd(ds);
    break;

  case FL_MSG_ID_READ_FW_VERSION:
    process_app_txt_fw_ver_cmd(ds);
    break;

  case FL_MSG_ID_READ_GPIO:
    process_app_txt_read_gpio_cmd(ds);
    break;

  case FL_MSG_ID_WRITE_GPIO:
    process_app_txt_write_gpio_cmd(ds);
    break;

  case FL_MSG_ID_READ_TEMPERATURE:
    process_app_txt_read_temperature_cmd(ds);
    break;

  case FL_MSG_ID_READ_HUMIDITY:
    process_app_txt_read_humidity_cmd(ds);
    break;

  case FL_MSG_ID_READ_TEMP_AND_HUM:
    process_app_txt_read_temp_and_hum_cmd(ds);
    break;

  case FL_MSG_ID_BOOT_MODE:
    process_app_txt_boot_mode_cmd(ds);
    break;

  case FL_MSG_ID_RESET:
    process_app_txt_reset_cmd(ds);
    break;
  }

  if (ds->tx_len > 0)
  {
    ds_send_packet(ds, (uint8_t*)ds->tx_buf, ds->tx_len);
    ds->tx_len = 0;
  }
}

static void process_app_txt_hw_ver_cmd(device_simulator* handle)
{
  fl_hw_ver_t hw_ver;

  sprintf(hw_ver.version, APP_HW_VER);
  APP_LIB_DEBUG_PRINT(("Hardware version : %s\n", hw_ver.version));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    &hw_ver, sizeof(fl_hw_ver_t),
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_fw_ver_cmd(device_simulator* handle)
{
  fl_fw_ver_t fw_ver;

  sprintf(fw_ver.version, APP_FW_VER);
  APP_LIB_DEBUG_PRINT(("Firmware version : %s\n", fw_ver.version));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    &fw_ver, sizeof(fl_fw_ver_t),
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_read_gpio_cmd(device_simulator* handle)
{
  fl_gpi_port_t* gpi_port = (fl_gpi_port_t*)&(handle->app_txt_parser.payload);
  fl_gpi_port_value_t gpi_port_val;
  time_t t;

  APP_LIB_DEBUG_PRINT(("GPI port number : %d\n", gpi_port->port_num));

  srand((unsigned)time(&t));

  // GPIO number.
  gpi_port_val.port_num = gpi_port->port_num;
  // GPIO value.
  gpi_port_val.port_value = rand() % 2;

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    &gpi_port_val, sizeof(fl_gpi_port_value_t),
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_write_gpio_cmd(device_simulator* handle)
{
  fl_gpo_port_value_t* gpo_port_value = (fl_gpo_port_value_t*)&(handle->app_txt_parser.payload);

  APP_LIB_DEBUG_PRINT(("GPO port number : %d, Port value : %d\n", gpo_port_value->port_num, gpo_port_value->port_value));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    NULL, 0,
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_read_temperature_cmd(device_simulator* handle)
{
  fl_sensor_t* sensor = (fl_sensor_t*)&(handle->app_txt_parser.payload);
  fl_temp_sensor_read_t sensor_read;
  time_t t;

  APP_LIB_DEBUG_PRINT(("Temperature sensor number : %d\n", sensor->sensor_num));

  srand((unsigned)time(&t));

  // Sensor number.
  sensor_read.sensor_num = sensor->sensor_num;
  // Sensor value.
  sensor_read.temperature = ((double)rand() / (double)RAND_MAX) * 30.0;

  APP_LIB_DEBUG_PRINT(("Temperature : %f\n", sensor_read.temperature));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    &sensor_read, sizeof(fl_temp_sensor_read_t),
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_read_humidity_cmd(device_simulator* handle)
{
  fl_sensor_t* sensor = (fl_sensor_t*)&(handle->app_txt_parser.payload);
  fl_hum_sensor_read_t sensor_read;
  time_t t;

  APP_LIB_DEBUG_PRINT(("Humidity sensor number : %d\n", sensor->sensor_num));

  srand((unsigned)time(&t));

  // Sensor number.
  sensor_read.sensor_num = sensor->sensor_num;
  // Sensor value.
  sensor_read.humidity = ((double)rand() / (double)RAND_MAX) * 100.0;

  APP_LIB_DEBUG_PRINT(("Humidity : %f\n", sensor_read.humidity));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    &sensor_read, sizeof(fl_hum_sensor_read_t),
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_read_temp_and_hum_cmd(device_simulator* handle)
{
  fl_sensor_t* sensor = (fl_sensor_t*)&(handle->app_txt_parser.payload);
  fl_temp_hum_sensor_read_t sensor_read;
  time_t t;

  APP_LIB_DEBUG_PRINT(("Temperature/Humidity sensor number : %d\n", sensor->sensor_num));

  srand((unsigned)time(&t));

  // Sensor number.
  sensor_read.sensor_num = sensor->sensor_num;
  // Sensor value.
  sensor_read.temperature = ((double)rand() / (double)RAND_MAX) * 30.0;
  sensor_read.humidity = ((double)rand() / (double)RAND_MAX) * 100.0;

  APP_LIB_DEBUG_PRINT(("Temperature : %f, Humidity : %f\n", sensor_read.temperature, sensor_read.humidity));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    &sensor_read, sizeof(fl_temp_hum_sensor_read_t),
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_boot_mode_cmd(device_simulator* handle)
{
  fl_boot_mode_t* boot_mode = (fl_boot_mode_t*)&(handle->app_txt_parser.payload);

  APP_LIB_DEBUG_PRINT(("Boot mode : %d\n", boot_mode->boot_mode));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    NULL, 0,
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}

static void process_app_txt_reset_cmd(device_simulator* handle)
{
  APP_LIB_DEBUG_PRINT(("Reset\n"));

  handle->tx_len = fl_txt_msg_build_response(handle->device_id, handle->app_txt_parser.msg_id, FL_OK,
    NULL, 0,
    (uint8_t*)handle->tx_buf, sizeof(handle->tx_buf));
}
