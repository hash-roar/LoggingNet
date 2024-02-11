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
  public class Channel


  {
    public string name;
    public int from;
    public int to;

    public string host = "0.0.0.0";

    private TcpListener tcpListener;
    public ConcurrentDictionary<string, Connection> clients = [];

    private object lockObj = new object(); // lock
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    public Channel(string name, int from, int to)
    {
      this.from = from;
      this.to = to;
      logger.Info($"create channel {name} from {from} to {to}");
      this.tcpListener = new TcpListener(IPAddress.Parse(host), from);
      this.name = name;
    }

    public async Task Start()
    {
      tcpListener.Start();
      try
      {
        tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        while (true)
        {
          var client = await tcpListener.AcceptSocketAsync();
          var remoteIp = client?.RemoteEndPoint?.ToString();
          logger.Info($"receive from {remoteIp}");
          if (remoteIp != null && client != null)
          {
              clients[remoteIp] = new Connection(client, this);
            _ = Task.Run(() =>
            {
              clients[remoteIp].handleConnect(to);
            });

          }
        }
      }
      catch (Exception ex)
      {
        logger.Error(ex);
      }

    }

    public void removeConnection(string remoteIp)
    {
      // lock  use lockObj

      lock (lockObj)
      {
        _ = clients.Remove(remoteIp, out var client);
        logger.Info($"{remoteIp} remove from server,total {client?.readNumTotal} read, {client?.writeNumTotal} write");
      }


    }

  }
}
