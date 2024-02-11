﻿using System.CommandLine;
using System.Drawing;
using ConsoleApp2;
using CConsole = Colorful.Console;
using Spectre;
using Spectre.Console;
using SysColor = System.Drawing.Color;
using Color = Spectre.Console.Color;

namespace ConsoleApp2
{
  class Program
  {
    static async Task<int> Main(string[] args)
    {
      var configFileOption = new Option<string>(name: "-c", description: "The path to the config file", getDefaultValue: () => "config.yaml");
      var rootCommand = new RootCommand("tcp redirect");
      rootCommand.AddOption(configFileOption);

      rootCommand.SetHandler(async (configFile) =>
      {
        var config = Config.parse(configFile);
        Server server = new(config);
        await server.Start();
        ;
      }, configFileOption);
      return await rootCommand.InvokeAsync(args);
    }

  }
}