using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Numerics;
using System.Text;
using GameServerCS.Utils;

namespace GameServerCS
{
    public class Channel
    {
        public ChannelType Type { get; }
        public string Name { get; }
        private readonly List<Socket> clientList = new List<Socket>();
        private readonly object clientListLock = new object();
        private readonly Dictionary<int, ClientInfo> portToClientDict = new Dictionary<int, ClientInfo>();
        private readonly Dictionary<string, ClientInfo> idToClientDict = new Dictionary<string, ClientInfo>();
        private readonly object clientLock = new object();

        public Channel(ChannelType type, string name)
        {
            Type = type;
            Name = name;
        }

        public void AddClient(Socket socket, string playerId = null)
        {
            lock (clientLock)
            {
                if (!clientList.Contains(socket))
                {
                    var ipEp = (IPEndPoint)socket.RemoteEndPoint;
                    var clientInfo = new ClientInfo()
                    {
                        Port = ipEp.Port,
                        PlayerId = playerId ?? ipEp.Port.ToString(),
                        Socket = socket
                    };
                    clientList.Add(socket);
                    portToClientDict[ipEp.Port] = clientInfo;
                    idToClientDict[clientInfo.PlayerId] = clientInfo;
                }
            }
        }

        public void RemoveClient(Socket socket)
        {
            lock (clientLock)
            {
                var ipEp = (IPEndPoint)socket.RemoteEndPoint;
                if (portToClientDict.TryGetValue(ipEp.Port, out var info))
                {
                    portToClientDict.Remove(ipEp.Port);
                    idToClientDict.Remove(info.PlayerId);

                    string leaveMessage = $"{Define.Exit}|{info.PlayerId}|{(int)this.Type}\n";
                    BroadcastMessage(leaveMessage);
                    clientList.Remove(socket);
                }
            }
        }

        public ClientInfo GetClientInfo(string playerId)
        {
            lock (clientLock)
            {
                return idToClientDict.TryGetValue(playerId, out var info) ? info : null;
            }
        }

        public List<ClientInfo> GetAllClientsInfo()
        {
            lock (clientLock)
            {
                return portToClientDict.Values.ToList();
            }
        }

        public void BroadcastMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                lock (clientListLock)
                {
                    Logger.Log($"[{Name}] 브로드캐스트 시작 - 수신자 : {portToClientDict.Count}명", "BROADCAST");

                    foreach (var client in portToClientDict.Values.ToList())
                    {
                        try
                        {
                            if (client.Socket.Connected)
                            {
                                client.Socket.Send(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"[{Name}] 클라이언트 {client.Port}에게 브로드캐스트 실페", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{Name}] 브로드캐스트 중 오류 발생", ex);
            }
        }

        public void BroadcastAllChannels(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var channel in Server.AllChannels.Values)
            {
                lock (channel.clientLock)
                {
                    foreach (var client in channel.portToClientDict.Values.ToList())
                    {
                        if (client.Socket.Connected)
                            client.Socket.Send(data);
                    }
                }
            }
        }

        public int ClientCount()
        {
            lock (clientListLock)
            {
                return portToClientDict.Count;
            }
        }

        public void UpdateClientId(Socket socket, string userId)
        {
            lock (clientLock)
            {
                var ipep = (IPEndPoint)socket.RemoteEndPoint;
                if (portToClientDict.TryGetValue(ipep.Port, out var info))
                {
                    idToClientDict.Remove(info.PlayerId);
                    info.PlayerId = userId;
                    idToClientDict[userId] = info;
                }
            }
        }

        public void UpdateClientPosition(int port, Vector3 pos, Vector3 rot)
        {
            lock (clientLock)
            {
                if (portToClientDict.TryGetValue(port, out var info))
                {
                    info.Position = pos;
                    info.Rotation = rot;
                }
            }
        }

        public class ClientInfo
        {
            public int Port { get; set; }
            public string PlayerId { get; set; }
            public Socket Socket { get; set; }
            public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
            public Vector3 Rotation { get; set; } = Vector3.Zero;
        }
    }
}