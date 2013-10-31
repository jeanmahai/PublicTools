using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer server = new HttpServer();
            server.StartHttpServer();
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
