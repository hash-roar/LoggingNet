using log4net.Repository.Hierarchy;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
  public class Connection
  {
    Socket socket;

    TcpClient? target;
    public int readNumTotal;
    public int writeNumTotal;

    public string remoteIp;

    public DateTime beginTime = DateTime.Now;

    // bool isClosed = false;
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private Channel channel;
    public Connection(Socket socket, Channel channel)
    {
      this.socket = socket;
      remoteIp = socket.RemoteEndPoint?.ToString() ?? "";
      this.channel = channel;
    }

    public async void handleConnect(int toPort)
    {
      target = new TcpClient();
      try
      {
        // dial to target
        logger.Info($"connect from {remoteIp} to {toPort}");
        await target.ConnectAsync(IPAddress.Parse("127.0.0.1"), toPort);
        logger.Info($"connect from {remoteIp} to {toPort} success");
        // wait 2 direction forward
        var tasks = new List<Task>();
        tasks.Add(forward_from(toPort, remoteIp));
        tasks.Add(forward_to(toPort, remoteIp));
        await Task.WhenAll(tasks);
      }
      catch (Exception e)
      {
        logger.Info(e, "error when connect");
      }
      finally
      {
        if (remoteIp != null)
        {
          channel.removeConnection(remoteIp);
        }
      }

      async Task forward_from(int toPort, string? remoteIp)
      {
        try
        {
          const int bufferSize = 1024 * 1024 * 10;
          var buffer = new byte[bufferSize];
          socket.ReceiveBufferSize = bufferSize;
          while (true)
          {
            var readNum = await socket.ReceiveAsync(buffer);
            if (readNum > 0)
            {
              logger.Debug($"receive {readNum} bytes from {remoteIp} to {toPort}");
              ArraySegment<byte> bytes = new ArraySegment<byte>(buffer, 0, readNum);
              await target.GetStream().WriteAsync(bytes);
              readNumTotal += readNum;
            }
            else
            {
              logger.Info($"receive 0 bytes from {remoteIp} to {toPort}");
              break;
            }
          }

        }
        catch (Exception e)
        {
          logger.Debug(e, "error when forward");
        }
        finally
        {
          closeConnection();
        }

      }

      async Task forward_to(int toPort, string? remoteIp)
      {
        try
        {
          const int bufferSize = 1024 * 1024 * 10;

          var buffer = new byte[bufferSize];
          while (true)
          {
            var readNum = await target.GetStream().ReadAsync(buffer,0,bufferSize);
            if (readNum > 0)
            {
              logger.Debug($"receive {readNum} bytes from {toPort} to {remoteIp}");
              ArraySegment<byte> bytes = new ArraySegment<byte>(buffer, 0, readNum);
              await socket.SendAsync(bytes);
              writeNumTotal += readNum;
            }
            else
            {
              logger.Info($"receive 0 bytes from {toPort} to {remoteIp}");
              break;
            }
          }
        }
        catch (Exception e)
        {
          closeConnection();
          logger.Debug(e, "error when forward");
        }
      }

      void closeConnection()
      {
        target?.Close();
        socket.Close();
      }



    }
  }
}
