using System.CommandLine;
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
      NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("config/nlog.config");
      var configFileOption = new Option<string>(name: "-c", description: "The path to the config file", getDefaultValue: () => "config/config.yaml");
      var rootCommand = new RootCommand("tcp redirect");
      rootCommand.AddOption(configFileOption);
      rootCommand.SetHandler(async (configFile) =>
      {
        try
        {
          var config = Config.parse(configFile);
          Server server = new(config);
          await server.Start();
        }
        catch (Exception e)
        {
          CConsole.WriteLine(e.Message, SysColor.Red);
        }

        ;
      }, configFileOption);
      return await rootCommand.InvokeAsync(args);
    }

  }
}