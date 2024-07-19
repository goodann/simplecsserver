using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(24, 24);
            ClientServer server = new ClientServer(2304);
            //ThreadPool.QueueUserWorkItem(myFunc.Func1);
            var serverTask = server.Start();

            serverTask.Wait();
        }
    }
}