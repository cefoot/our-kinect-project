using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using De.Cefoot.RestUtil;
using De.Cefoot.RestUtil.RestWorker;

namespace RestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
        }

        private static void Start()
        {
            Console.Write("Port:");
            var port = 0;
            try
            {
                var tmp = Console.ReadLine();
                port = int.Parse(tmp);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Start();
                return;
            }
            Console.Write("Path?:");
            String preString = "";
            try
            {
                var tmp = Console.ReadLine();
                var builder = new UriBuilder("http","google",port);
                if (tmp != null) builder.Path = tmp;
                var uri = builder.Uri;
                preString = tmp;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Start();
                return;
            }
            RestFactory restFactory = null;
            try
            {
                restFactory = De.Cefoot.RestUtil.RestFactory.CreateRestWorker(port, preString, new UrlMethodCallWorker());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Write("elevate?(y/n):");
                if(Console.ReadKey().KeyChar.ToString().ToLower() == "y")
                {
                    Elevate();
                }
                return;
            }
            restFactory.Start();
            while (Console.ReadLine() != "quit")
            {
                Console.WriteLine("\"quit\" to exit");
            }
        }

        private static void Elevate()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var procInfo = new ProcessStartInfo(executingAssembly.Location)
                               {
                                   UseShellExecute = true,
                                   Verb = "runas",
                                   WindowStyle = ProcessWindowStyle.Normal
                               };

            Process.Start(procInfo);
            Process.GetCurrentProcess().Close();
        }
    }
}
