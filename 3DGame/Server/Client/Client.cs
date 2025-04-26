using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Numerics;
using System.Text;
using static GameServerCS.Channel;
using GameServerCS.Utils;

namespace GameServerCS
{
    public class Client : SocketAsyncEventArgs
    {
        private Channel channel;
        private Socket socket;
        private IPEndPoint ipep;
        private string _playerId;

        private readonly object testClientsLock = new object();
        private readonly Dictionary<int, ClientInfo> testClients = new Dictionary<int, ClientInfo>();
        private int testCnt = 1;

        public Client(Socket socket) : base()
        {
            try
            {
                this.socket = socket;
                this.socket.SendBufferSize = 256 * 1024;
                this.socket.ReceiveBufferSize = 256 * 1024;
                this.channel = Server.GetChannel(ChannelType.Lobby);
                this.ipep = (IPEndPoint)socket.RemoteEndPoint;
                this._playerId = "temp_" + Guid.NewGuid().ToString();

                base.SetBuffer(new byte[8192], 0, 8192);
                base.UserToken = socket;
                base.Completed += Client_Completed;

                ipep = (IPEndPoint)socket.RemoteEndPoint;
                Console.WriteLine($"[{channel.Name}] Client Connected: {ipep.Address}:{ipep.Port}");
                Logger.Log($"클라이언트 연결 : {ipep.Address}:{ipep.Port}", "CONNECTION");
                channel.AddClient(socket, _playerId);

                SendWelcomeMessage();

                if (!IsConnectionAlive())
                {
                    DisconnectClient();
                    return;
                }
                this.socket.ReceiveAsync(this);
            }
            catch (Exception ex)
            {
                Logger.LogError("클라이언트 초기화 실패", ex);
            }
        }

        private void SendWelcomeMessage()
        {
            SendToClient($"{Define.Init}|{_playerId}\n");
        }

        private void SendServerAnnouncement(string message)
        {
            channel.BroadcastMessage($"{message}");
        }

        private bool IsConnectionAlive()
        {
            try
            {
                bool isReadable = socket.Poll(1000, SelectMode.SelectRead);
                bool hasData = socket.Available > 0;

                if (isReadable && !hasData)
                {
                    Console.WriteLine($"[Disconnected] {ipep.Port}");
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ChannelCount()
        {
            Console.WriteLine($"Lobby : {Server.GetChannel(ChannelType.Lobby).ClientCount()}");
            Console.WriteLine($"Game1 : {Server.GetChannel(ChannelType.Game1).ClientCount()}");
            Console.WriteLine($"Game2 : {Server.GetChannel(ChannelType.Game2).ClientCount()}");
            Console.WriteLine($"Game3 : {Server.GetChannel(ChannelType.Game3).ClientCount()}");

            string message = $"{Define.Count}|{Server.GetChannel(ChannelType.Lobby).ClientCount()}|" +
                            $"{Server.GetChannel(ChannelType.Game1).ClientCount()}|" +
                            $"{Server.GetChannel(ChannelType.Game2).ClientCount()}|" +
                            $"{Server.GetChannel(ChannelType.Game3).ClientCount()}\n";

            channel.BroadcastAllChannels(message);
        }

        private void SendToClient(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                socket.Send(data);

                Logger.LogMessage(ipep, "송신", message);
            }
            catch (Exception ex)
            {
                Logger.LogError($"메시지 전송 실패", ex);
            }
        }

        private void Client_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (socket.Connected && base.BytesTransferred > 0)
            {
                byte[] data = new byte[base.BytesTransferred];
                Array.Copy(e.Buffer, data, e.BytesTransferred);
                string msg = Encoding.UTF8.GetString(data);

                try
                {
                    ProcessClientMessage(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    SendToClient("[SERVER] Error processing your message");
                }

                if (socket.Connected)
                {
                    e.SetBuffer(new byte[8192], 0, 8192);
                    if (!IsConnectionAlive())
                    {
                        DisconnectClient();
                        return;
                    }
                    socket.ReceiveAsync(e);
                }
            }
            else
            {
                if (e.SocketError != SocketError.Success)
                {
                    DisconnectClient();
                }
            }
        }

        private void ProcessClientMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            Logger.LogMessage(ipep, "수신", msg);

            string[] messages = msg.Split('\n');

            foreach (string message in messages)
            {
                string[] parts = message.Split('|');
                if (string.IsNullOrWhiteSpace(message)) continue;

                try
                {
                    switch (parts[0])
                    {
                        case Define.Login:
                            if (parts.Length >= 2)
                            {
                                Logger.Log($"로그인 : {parts[1]}", "AUTH");
                                HandleLogin(parts[1]);
                            }
                            break;

                        case Define.Join:
                            if (parts.Length >= 3)
                            {
                                if (parts[1] == "999") // 부하 테스트용 특수 채널
                                {
                                    // 테스트용 간소화된 처리
                                    var clientInfo = new Channel.ClientInfo
                                    {
                                        PlayerId = parts[2],
                                        Socket = socket,
                                        Port = ((IPEndPoint)socket.RemoteEndPoint).Port
                                    };

                                    // 최소한의 브로드캐스트만 수행
                                    string response = $"{Define.Join}|1|{parts[2]}|0|0|0|0|0|0\n";
                                    socket.Send(Encoding.UTF8.GetBytes(response));

                                    // 테스트 클라이언트 목록에 추가
                                    lock (testClientsLock)
                                    {
                                        testClients.Add(testCnt, clientInfo);
                                    }
                                    testCnt++;
                                }
                                else
                                {
                                    Console.WriteLine($"[{channel.Name}][{parts[1]}] : {msg}");
                                    Logger.Log($"접속 : {parts[1]} -> {channel.Name}채널", "JOIN");
                                    HandleJoin(parts[1], parts[2]);
                                }
                            }
                            break;

                        case Define.Chat:
                            if (parts.Length >= 2)
                            {
                                Console.WriteLine($"[{channel.Name}][{parts[1]}] : {msg}");
                                Logger.Log($"채팅 : [{channel.Name}]{parts[1]} : {msg}", "CHAT");
                                HandleGameChannelMessage(parts[1], parts[2]);
                            }
                            break;

                        case Define.Jump:
                            if (parts.Length >= 2)
                            {
                                channel.BroadcastMessage(msg);
                            }
                            break;

                        case Define.Move:
                            HandlePlayerMove(message + "\n");
                            break;

                        case Define.Exit:
                            Console.WriteLine($"[{channel.Name}][{parts[1]}] : {msg}");
                            DisconnectClient(parts[1]);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error : {ex.Message}");
                }
            }
        }

        private void HandleLogin(string userId)
        {
            _playerId = userId;
            channel.UpdateClientId(socket, userId);
            ChannelCount();
        }

        private void HandleJoin(string channelNumberStr, string playerId)
        {
            _playerId = playerId;

            if (int.TryParse(channelNumberStr, out int newChannelId))
            {
                var newChannelType = (ChannelType)newChannelId;
                var newChannel = Server.GetChannel(newChannelType);

                if (newChannel != null && newChannel != channel)
                {
                    channel.RemoveClient(socket);
                    channel = newChannel;
                    channel.AddClient(socket, playerId);

                    SendExistingClientsInfo();

                    var clientInfo = channel.GetClientInfo(playerId);
                    string joinMessage = $"{Define.Join}|{newChannelId}|{playerId}|" +
                        $"{clientInfo.Position.X}|{clientInfo.Position.Y}|{clientInfo.Position.Z}|" +
                        $"{clientInfo.Rotation.X}|{clientInfo.Rotation.Y}|{clientInfo.Rotation.Z}\n";
                    channel.BroadcastMessage(joinMessage);
                    channel.BroadcastMessage(joinMessage);

                    SendToClient($"{Define.Channel}|{channel.Name}\n");
                    ChannelCount();
                }
            }
        }

        private void SendExistingClientsInfo()
        {
            var clients = channel.GetAllClientsInfo();

            foreach (var clientInfo in clients.Where(c => c.Port != ipep.Port))
            {
                string infoMessage = $"{Define.Join}|{(int)channel.Type}|{clientInfo.PlayerId}|" +
                    $"{clientInfo.Position.X}|{clientInfo.Position.Y}|{clientInfo.Position.Z}|" +
                    $"{clientInfo.Rotation.X}|{clientInfo.Rotation.Y}|{clientInfo.Rotation.Z}\n";
                SendToClient(infoMessage);
            }
        }

        private void HandleGameChannelMessage(string playerid, string msg)
        {
            channel.BroadcastMessage($"{Define.Chat}|[{playerid}]: {msg}");
        }

        private void HandlePlayerMove(string msg)
        {
            if (msg.StartsWith(Define.Move) && msg.Split('|').Length >= 8)
            {
                var parts = msg.Split('|');
                string playerId = parts[1];

                if (float.TryParse(parts[2], out float x) &&
                    float.TryParse(parts[3], out float y) &&
                    float.TryParse(parts[4], out float z) &&
                    float.TryParse(parts[5], out float rotX) &&
                    float.TryParse(parts[6], out float rotY) &&
                    float.TryParse(parts[7], out float rotZ))
                {
                    Vector3 pos = new Vector3(x, y, z);
                    Vector3 rot = new Vector3(rotX, rotY, rotZ);

                    channel.UpdateClientPosition(ipep.Port, pos, rot);
                }

                channel.BroadcastMessage(msg);
            }
        }

        private void DisconnectClient()
        {
            if (socket.Connected)
            {
                try
                {
                    if (socket.Connected)
                    {
                        channel.RemoveClient(socket);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }

                    Console.WriteLine($"[{channel.Name}] Client Disconnected: {ipep.Port}");

                    ChannelCount();
                }
                catch { }
            }
        }

        private void DisconnectClient(string playerId)
        {
            if (socket.Connected)
            {
                try
                {
                    if (socket.Connected)
                    {
                        channel.RemoveClient(socket);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }

                    Console.WriteLine($"[{channel.Name}] Client Disconnected: {ipep.Port}");

                    ChannelCount();
                }
                catch { }
            }
        }
    }
}