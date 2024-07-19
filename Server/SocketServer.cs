using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class SocketServer
    {
        protected List<Socket> clients;
        protected Socket serverSocket;

        public SocketServer(int port)
        {
            clients = new List<Socket>();
            var ep = new IPEndPoint(IPAddress.Any, port);
            serverSocket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ep);
        }

        virtual public async Task Start()
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
            while (isTerminating == false) {
                await Task.Yield();
            }

            return;
        }

        virtual public void OnAcceptCompleted(object? sender, SocketAsyncEventArgs sockEventArgs)
        {
            if (sockEventArgs.SocketError == SocketError.Success && null != sockEventArgs.AcceptSocket) {
                clients.Add(sockEventArgs.AcceptSocket);
                SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
                recvArgs.Completed += OnReceivedCompleted;

                recvArgs.SetBuffer(new byte[1024], 0, 1024);
                recvArgs.UserToken = recvArgs.AcceptSocket;


                RegistRecv(recvArgs);
            }
        }

        virtual public void RegistRecv(SocketAsyncEventArgs recvArgs)
        {
            Socket? client = recvArgs.UserToken as Socket;

            bool? pending = client?.ReceiveAsync(recvArgs);

            if (pending == false) {
                OnReceivedCompleted(null, recvArgs);
            }
        }

        abstract public void OnReceivedCompleted(object? sender, SocketAsyncEventArgs sockEventArgs);
    }
}
