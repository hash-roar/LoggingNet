using NLog;

namespace ConsoleApp2
{
  public class ConsoleFormatter
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
    public static void  WriteToConsole(string message)
    {
      Console.SetCursorPosition(0, 2);
      Console.Write(message);
    }

    public static void WriteToLog(string message)
    {
      logger.Debug(message);
    }
  }
}
