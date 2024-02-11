using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApp2
{
  using SampleInfoType = Dictionary<Tuple<string, string>, Dictionary<string, string>>;
  public class Server
  {
    public string name;
    public Config config;
    public List<Channel> chans = new List<Channel>();

    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    // lock

    public Server(Config config, string name = "server")
    {
      this.config = config;
      this.name = name;
    }

    public async Task Start()
    {
      var tasks = new List<Task>();
      foreach (var (name, dict) in config.forward)
      {
        var channel = new Channel(name, dict["from"], dict["to"]);
        chans.Add(channel);
        tasks.Add(channel.Start());
      }
      tasks.Add(LogInterval());

      await Task.WhenAll(tasks);
    }

    public async Task LogInterval()
    {
      while (true)
      {
        var info = SampleStatus();
        WriteSampleStatus(info);
        await Task.Delay(config.LogInterval);
      }
    }
    public SampleInfoType SampleStatus()
    {
      var ret = new SampleInfoType();
      foreach (var channel in chans)
      {
        foreach (var (_, conn) in channel.clients)
        {
          var key = new Tuple<string, string>(channel.name, conn.remoteIp);
          var value = new Dictionary<string, string>
          {
            {"from", conn.remoteIp},
            {"to", channel.to.ToString()},
            {"readNumTotal", conn.readNumTotal.ToString()},
            {"writeNumTotal", conn.writeNumTotal.ToString()},
          };
          ret[key] = value;
        }
      }

      return ret;
    }

    public void WriteSampleStatus(SampleInfoType info)
    {
      var messageBuilder = new StringBuilder();
      messageBuilder.AppendLine($"server: {name}--status:");
      foreach (var (key, value) in info)
      {
        var message = $"channel {key.Item1} remoteIp {key.Item2} from {value["from"]} to {value["to"]} readNumTotal {value["readNumTotal"]} writeNumTotal {value["writeNumTotal"]}";
        messageBuilder.AppendLine(message);
      }
      ConsoleFormatter.WriteToLog(messageBuilder.ToString());
    }

    


  }
}
