

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Google.Protobuf;


namespace Client
{
    public class ServerSocket
    {
        Socket socket;
        bool isConnect;

        SocketAsyncEventArgs? sendArgs;

        public ServerSocket(EndPoint ep, int port)
        {
            socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = ep;

            // 연결 요청용 SocketAsyncEventArgs 객체를 인수로 보내 비동기로 연결 요청을 합니다.
            socket.ConnectAsync(args);
            isConnect = true;

        }
        public void OnConnectCompleted(object? obj, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success) {

                // 데이터 송신용 SocketAsyncEventArgs 객체
                sendArgs = new SocketAsyncEventArgs();
                sendArgs.Completed += OnSendCompleted;

                // 데이터 수신용 SocketAsyncEventArgs 객체
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(new byte[1024], 0, 1024);
                receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

                // 데이터 수신 준비를 합니다.
                bool pending = socket.ReceiveAsync(receiveArgs);
                if (pending == false)
                    OnRecvCompleted(null, receiveArgs);
            }
        }

        public void OnRecvCompleted(object? obj, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                //string recvData = System.Text.Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);

                Packet.packet.Parser.ParseFrom(args.Buffer, 0, args.BytesTransferred);


                // .. 받은 데이터를 채팅 UI에 표현합니다 ..

                // 새로운 데이터 수신을 준비합니다.
                bool pending = socket.ReceiveAsync(args);
                if (pending == false)
                    OnRecvCompleted(null, args);
            }
        }
        public void SendMsg(IMessage message)
        {
            if(isConnect == true) 
            {
                socket.Send(message.ToByteArray());
            }
            //if (message.Equals(string.Empty) == false) {
            //    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            //    sendArgs.SetBuffer(buffer, 0, buffer.Length);
            //    socket.SendAsync(sendArgs);
            //}
        }
        public void OnSendCompleted(object? obj, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                //sendArgs?.BufferList = null;
            }
        }
    }

    internal class Client
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
