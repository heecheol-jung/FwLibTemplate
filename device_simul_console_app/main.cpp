#include <iostream>
#include <string>
#include <map>
#include "app_common_util.h"
#include "app_constant.h"
#include "device_simulators.h"

int main()
{
  bool loop = true;
  std::map<std::string, app_func_ptr_t> map_menus;

  map_menus.insert(std::make_pair(APP_LIB_STR_QUIT, (app_func_ptr_t)NULL));
  map_menus[STR_DEVICE_SIMULATOR] = device_simulator_example;

  while (loop)
  {
    std::string str_line;

    print_menu(&map_menus);

    std::getline(std::cin, str_line);
    if (str_line == APP_LIB_STR_QUIT)
    {
      loop = false;
    }
    else
    {
      if (map_menus.count(str_line) > 0)
      {
        map_menus[str_line]();
      }
      else
      {
        std::cout << APP_LIB_STR_UNKNOWN_COMMANDD << std::endl;
      }
    }
  }

  std::cout << "main done." << std::endl;
}
