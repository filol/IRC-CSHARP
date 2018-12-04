using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(8976);
            server.start();
        }
    }
}
