using ITXCM;
using System;
using System.IO;

namespace GameServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            FileInfo fi = new FileInfo("log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fi);
            Log.Init("GameServer");
            Log.Info("Game Server Init");

            GameServer server = new GameServer();
            server.Init();
            server.Start();

            Console.WriteLine("Game Server Running......");
            CommandHelper.Run();
            Log.Info("Game Server Exiting...");
            server.Stop();
            Log.Info("Game Server Exited");
        }
    }
}