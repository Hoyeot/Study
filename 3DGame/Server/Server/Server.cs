using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace GameServerCS
{
    public class Server : SocketAsyncEventArgs
    {
        private static readonly Dictionary<ChannelType, Channel> channels = new Dictionary<ChannelType, Channel>();
        private Socket socket;

        public static IReadOnlyDictionary<ChannelType, Channel> AllChannels
        {
            get
            {
                return new ReadOnlyDictionary<ChannelType, Channel>(channels);
            }
        }

        static Server()
        {
            channels[ChannelType.Lobby] = new Channel(ChannelType.Lobby, "0");
            channels[ChannelType.Game1] = new Channel(ChannelType.Game1, "1");
            channels[ChannelType.Game2] = new Channel(ChannelType.Game2, "2");
            channels[ChannelType.Game3] = new Channel(ChannelType.Game3, "3");
        }

        public Server(Socket socket)
        {
            this.socket = socket;
            base.UserToken = socket;
            base.Completed += Server_Completed;
        }

        private void Server_Completed(object sender, SocketAsyncEventArgs e)
        {
            new Client(e.AcceptSocket);
            e.AcceptSocket = null;
            this.socket.AcceptAsync(e);
        }

        public static Channel GetChannel(ChannelType type)
        {
            channels.TryGetValue(type, out Channel channel);
            return channel;
        }
    }
}