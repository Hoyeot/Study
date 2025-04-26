using System.Net;
using System;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Collections;

public class NetworkMgr : Singleton<NetworkMgr>
{
    private TcpClient _client;
    private NetworkStream _stream;
    public NetworkStream Stream {  get { return _stream; } }

    private byte[] _buffer = new byte[1024];
    private bool _isConnected = false;
    private bool _isReceiving = false;
    public bool IsConnected { get { return _isConnected; } }

    protected override void Initialize()
    {
        base.Initialize();
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectToServer()
    {
        if (_client != null && _client.Connected)
        {
            UIMgr.Instance.AddChatMessage("이미 서버에 연결 돼 있습니다.");
            return;
        }

        try
        {
            _client = new TcpClient();
            _client.Connect(Define.Host, Define.Port);
            _stream = _client.GetStream();

            IPEndPoint localEndpoint = (IPEndPoint)_client.Client.LocalEndPoint;

            GameMgr.Instance.CurrentChannel = (int)Define.ChannelType.Lobby;
            _isConnected = true;
            _isReceiving = true;
            StartCoroutine(ReceiveMessages());

            UIMgr.Instance.LoginUI.SetActive(false);
            UIMgr.Instance.LobbyUI.SetActive(true);
            UIMgr.Instance.ChannelText.text = $"{Define.ChannelType.Lobby}";
            UIMgr.Instance.IsCountTime = true;
            UIMgr.Instance.StartTime = Time.time;
            UIMgr.Instance.UpdateUI();
        }
        catch (Exception e)
        {
            UIMgr.Instance.AddChatMessage($"서버 연결 실패 : {e.Message}");
            if (_client != null) _client.Close();
            GameMgr.Instance.CurrentChannel = (int)Define.ChannelType.Exit;
            UIMgr.Instance.UpdateUI();
        }
    }

    public void DisconnectFromServer()
    {
        if (!IsConnected) return;

        if (_client != null)
        {
            try
            {
                if (_client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes($"{Define.Exit}|{GameMgr.Instance.PlayerId}\n");
                    _stream.Write(data, 0, data.Length);

                    foreach (var player in PlayerMgr.Instance.Players.Values)
                    {
                        if (player != null && player != PlayerMgr.Instance.LocalPlayer) Destroy(player);
                    }
                    PlayerMgr.Instance.Players.Clear();

                    if (PlayerMgr.Instance.LocalPlayer != null)
                    {
                        PlayerMgr.Instance.LocalPlayer.SetActive(false);
                        PlayerMgr.Instance.Players[GameMgr.Instance.PlayerId] = PlayerMgr.Instance.LocalPlayer;
                    }

                    _stream?.Close();
                    _client?.Close();
                    APIMgr.Instance.IsRankSend = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"연결 해제 실패 : {ex.Message}");
            }
            finally
            {
                _isConnected = false;
                _isReceiving = false;
                GameMgr.Instance.CurrentChannel = (int)Define.ChannelType.Exit;
                UIMgr.Instance.AddChatMessage("서버 연결 종료");
                UIMgr.Instance.UpdateUI();
            }
        }
    }

    private IEnumerator ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (_isReceiving && _client != null && _client.Connected)
        {
            if (_stream.DataAvailable)
            {
                try
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) yield break;

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessNetworkMessage(receivedMessage);
                }
                catch (Exception e)
                {
                    UIMgr.Instance.AddChatMessage($"전송 오류 : {e.Message}");
                    DisconnectFromServer();
                }
            }
            yield return null;
        }
    }

    private void ProcessNetworkMessage(string msg)
    {
        string[] messages = msg.Split('\n');

        foreach (string message in messages)
        {
            string[] parts = message.Split('|');

            if (string.IsNullOrWhiteSpace(message)) continue;

            try
            {
                switch (parts[0])
                {
                    case Define.Init:
                        SendToServer($"{Define.Login}|{GameMgr.Instance.PlayerId}");
                        break;

                    case Define.Join:
                        if (parts.Length == 9 && parts[2] != GameMgr.Instance.PlayerId && GameMgr.Instance.CurrentChannel > (int)Define.ChannelType.Lobby)
                        {
                            if (int.Parse(parts[1]) == GameMgr.Instance.CurrentChannel)
                            {
                                Vector3 pos = new Vector3(float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]));
                                Vector3 rot = new Vector3(float.Parse(parts[6]), float.Parse(parts[7]), float.Parse(parts[8]));

                                PlayerMgr.Instance.SpawnRemotePlayer(parts[2], pos, rot);
                            }
                        }
                        break;

                    case Define.Channel:
                        int _tempChannel;
                        int.TryParse(parts[1], out _tempChannel);
                        GameMgr.Instance.CurrentChannel = _tempChannel;

                        if (GameMgr.Instance.CurrentChannel == (int)Define.ChannelType.Lobby)
                        {
                            UIMgr.Instance.ChannelText.text = $"{Define.ChannelType.Lobby}";
                            PlayerMgr.Instance.ClearCurrentChannelPlayers();
                        }
                        else
                        {
                            UIMgr.Instance.ChannelText.text = $"Channel {GameMgr.Instance.CurrentChannel}";
                        }
                        break;

                    case Define.Jump:
                        if (parts.Length >= 2)
                        {
                            string playerId = parts[1];
                            if (PlayerMgr.Instance.Players.TryGetValue(playerId, out GameObject player))
                            {
                                NetworkPlayer np = player.GetComponent<NetworkPlayer>();
                                if (np != null)
                                {
                                    np.UpdateTransform(
                                        np.transform.position,
                                        np.transform.rotation.eulerAngles,
                                        np.GetCurrentSpeed(),
                                        np.GetIsGround(),
                                        true
                                    );
                                }
                            }
                        }
                        break;

                    case Define.Move:
                        if (parts.Length == 10)
                        {
                            string playerId = parts[1];
                            Vector3 pos = new Vector3(float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4]));
                            Vector3 rot = new Vector3(float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]));
                            float speed = float.Parse(parts[8]);
                            bool isGround = bool.Parse(parts[9]);

                            PlayerMgr.Instance.UpdatePlayerPosition(playerId, pos, rot, speed, isGround);
                        }
                        break;

                    case Define.Chat:
                        UIMgr.Instance.AddChatMessage(message.Substring(Define.Chat.Length + 1));
                        break;

                    case Define.Count:
                        UIMgr.Instance.UpdateChannelCounts(message);
                        break;

                    case Define.Exit:
                        if (parts.Length >= 2)
                        {
                            int leftChannel = int.Parse(parts[2]);
                            if (leftChannel == GameMgr.Instance.CurrentChannel)
                                PlayerMgr.Instance.DestroyPlayer(parts[1]);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"메시지 오류 : {ex.Message}");
            }
        }
    }

    public void SendToServer(string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        _stream.Write(data, 0, data.Length);
    }

    public void SendMessageToServer()
    {
        if (GameMgr.Instance.CurrentChannel < (int)Define.ChannelType.Lobby || !_client.Connected)
        {
            UIMgr.Instance.AddChatMessage("서버와 연결이 안돼있습니다.");
            return;
        }

        string message = UIMgr.Instance.MessageText.text;
        if (string.IsNullOrEmpty(message)) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
            UIMgr.Instance.MessageText.text = string.Empty;
        }
        catch (Exception e)
        {
            UIMgr.Instance.AddChatMessage($"전송 실패 : {e.Message}");
            DisconnectFromServer();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}