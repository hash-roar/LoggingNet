using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConsoleApp2
{
  public struct Config
  {

    public Dictionary<string, Dictionary<string, int>> forward;
    public int LogInterval = 1000;

    public Config() {
      this.forward = [];
    }


    public static Config parse(string filePath)
    {
      // read file
      StreamReader sr = new StreamReader(filePath);
      string yaml = sr.ReadToEnd();

      var deserializer = new DeserializerBuilder().WithEnumNamingConvention(
        UnderscoredNamingConvention.Instance).Build();

      var res   = deserializer.Deserialize<Config>(yaml);

      return res;
    }

    
  }
}
