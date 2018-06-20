using NBitcoin;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using DtpCore.Extensions;

namespace DtpCore.Configuration
{
    public abstract class App
    {
        public static JObject Config = new JObject();

        public static Network BitcoinNetwork = Network.TestNet;

        static App()
        {
        }

        public static void InitializeLogging()
        {
            //var consoleListener = new ColoredConsoleTraceListener();
            //Trace.Listeners.Add(consoleListener);
            //Trace.AutoFlush = true;
            
            //if (App.Config["filelog"].ToBoolean(true) == true)
            //{
            //    var fileListener = new RollingFileTraceListener();
                
            //    //fileListener.Attributes[""] = "";
            //    Trace.Listeners.Add(fileListener);
            //}

        }

        public static void LoadConfigFile(string filename)
        {
            if (File.Exists(filename))
            {
                var text = File.ReadAllText(filename);
                Config.Merge(JObject.Parse(text));
            }
        }

        public static void SaveConfigFile(string filename)
        {
            var json = Config.ToString();
            File.WriteAllText(filename, json);
        }
    }
}
