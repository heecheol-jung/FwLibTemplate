using AppCommon.Net;
using Serilog;
using System;
using System.Collections.Generic;

namespace HostSimulConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://github.com/serilog/serilog/wiki/AppSettings
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();

            // serilog seq app.config
            // https://github.com/serilog/serilog-sinks-seq

            Dictionary<string, IExample> exampleTable = new Dictionary<string, IExample>()
            {
                { AppConstant.STR_HOST_SIMULATOR, new HostSimulatorExample1() },
                { AppConstant.STR_MESSAGE_MANAGER, new MessageManagerExample() },
                { AppCommonConstant.STR_QUIT, null }
            };

            while (true)
            {
                AppCommonUtil.PrintExampleMenu(exampleTable);
                string command = Console.ReadLine().ToLower();

                if (command == AppCommonConstant.STR_QUIT)
                {
                    break;
                }
                if (exampleTable.ContainsKey(command) == true)
                {
                    if (exampleTable[command] != null)
                    {
                        exampleTable[command].Do();
                    }
                    else
                    {
                        Log.Warning($"Invalid example for {command}");
                        Console.WriteLine($"Invalid example for {command}");
                    }
                }
                else
                {
                    Log.Warning("Unknown command!!!");
                    Console.WriteLine("Unknown command!!!");
                }
            }

            Log.Information("Main done");
            Log.CloseAndFlush();

            Console.WriteLine("Main done");
        }
    }
}
