using Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientServer : SocketServer
    {
        delegate void Handler(byte[] buffer, ref int offset, int size);

        Dictionary<Common.PacketId, Handler> HandlerList;

        public ClientServer(int port) : base(port)
        {
            HandlerList = new Dictionary<Common.PacketId, Handler>()
            { 
                { Common.PacketId.Packet, OnPacket },
            };
        }

        override public void OnReceivedCompleted(object? sender, SocketAsyncEventArgs sockEventArgs)
        {
            if (sockEventArgs.BytesTransferred > 0 && sockEventArgs.SocketError != SocketError.Success) {
                var buffer = sockEventArgs.Buffer;
                if (buffer == null) {
                    return;
                }

                int offset = 0;
                int packetId = BitConverter.ToInt32(buffer, offset);
                offset += sizeof(int);
                int size = sockEventArgs.BytesTransferred - offset;
                HandlerList[(Common.PacketId)packetId](buffer, ref offset, size);

            }

        }

        public void OnPacket(byte[] buffer, ref int offset, int size)
        {
            Packet.packet received = Packet.packet.Parser.ParseFrom(buffer, offset, size);
            Console.WriteLine($"{received.I}");
        }
    }
}
