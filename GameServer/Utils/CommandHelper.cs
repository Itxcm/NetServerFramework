using System;

namespace GameServer
{
    internal class CommandHelper
    {
        public static void Run()
        {
            bool run = true;
            while (run)
            {
                Console.Write(">");
                string line = Console.ReadLine();
                switch (line.ToLower().Trim())
                {
                    case "exit":
                        run = false;
                        break;

                    default:
                        Help();
                        break;
                }
            }
        }

        public static void Help()
        {
            Console.Write(@"
Help:
    exit    Exit Game Server
    help    Show Help
");
        }
    }
}