using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace ConsoleApp1
{

    public class Server
    {
        protected List<Socket> clients = new List<Socket>();
        protected Socket?    serverSocket;
    }

    public class ClientServer : Server
    {
        public ClientServer(int port)
        {
            var ep = new IPEndPoint(IPAddress.Any, port);
            serverSocket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ep);
        }

        public async Task Start()
        {

            serverSocket?.Listen(1000);

            SocketAsyncEventArgs sockEventArgs = new SocketAsyncEventArgs();

            sockEventArgs.Completed += OnAcceptCompleted;


            sockEventArgs.AcceptSocket = null;

            bool isTerminating = false;
            bool? pending = serverSocket?.AcceptAsync(sockEventArgs);
            if (pending == false) {
                OnAcceptCompleted(null, sockEventArgs);
            }
            while (isTerminating == false) 
            {
                await Task.Yield();
            }

            return;
        }

        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs sockEventArgs)
        {
            if(sockEventArgs.SocketError == SocketError.Success && null != sockEventArgs.AcceptSocket) {
                //Socket? Client = sockEventArgs.UserToken as Socket;
                //Client?.ReceiveAsync(sockEventArgs);

                clients.Add(sockEventArgs.AcceptSocket);


                SocketAsyncEventArgs recvArgs  = new SocketAsyncEventArgs();
                recvArgs.Completed += OnReceivedCompleted;

                recvArgs.SetBuffer(new byte[1024], 0, 1024);
                recvArgs.UserToken = recvArgs.AcceptSocket;


                RegistRecv(recvArgs);
            }
        }

        void RegistRecv(SocketAsyncEventArgs recvArgs)
        {
            Socket? client = recvArgs.UserToken as Socket;

            bool? pending = client?.ReceiveAsync(recvArgs);

            if (pending == false) 
            {
                OnReceivedCompleted(null, recvArgs);
            }
        }

        void OnReceivedCompleted(object? sender, SocketAsyncEventArgs sockEventArgs)
        {
            if(sockEventArgs.BytesTransferred > 0 && sockEventArgs.SocketError != SocketError.Success) 
            {
                var buffer = sockEventArgs.Buffer;
                if(buffer == null) 
                { 
                    return; 
                }

                int offset = 0;
                int packetId = BitConverter.ToInt32(buffer, offset);
                offset += sizeof(int);
                Packet.packet received= Packet.packet.Parser.ParseFrom(buffer, offset, sockEventArgs.BytesTransferred - offset);
                Console.WriteLine($"{received.I}");

            }

        }
    }


    internal class Program
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