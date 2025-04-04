using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

public class ClientMgr : Singleton<ClientMgr>
{
    private TcpClient _client;
    private NetworkStream _stream;
    private byte[] _buffer = new byte[1024];

    private Queue<string> _messageQueue = new Queue<string>();
    private object _lock = new object();

    private string _playerID;

    void Start()
    {
        _playerID = ObjectMgr.Instance.Player.CharactedId;
        ConnectServer();
    }

    void Update()
    {
        lock (_lock)
        {
            while (_messageQueue.Count > 0)
            {
                string message = _messageQueue.Dequeue();
                ProcessMessage(message);
            }
        }
    }

    private void ConnectServer()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(Define.LocalHost, Define.Port);
            _stream = _client.GetStream();
            _stream.BeginRead(_buffer, 0, _buffer.Length, OnDataReceived, null);

            SendMessageToServer($"{(int)Define.MessageType.Join}|{ObjectMgr.Instance.Player.CharactedId}"); // 서버에 접속메시지 보냄
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void OnDataReceived(IAsyncResult result)
    {
        if (_stream == null || _client == null) return;

        try
        {
            int byteRead = _stream.EndRead(result);
            if (byteRead > 0)
            {
                string receivedData = Encoding.UTF8.GetString(_buffer, 0, byteRead);

                string[] messages = receivedData.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

                lock (_lock)
                {
                    foreach (string message in messages)
                    {
                        _messageQueue.Enqueue(message);
                    }
                }

                _stream.BeginRead(_buffer, 0, _buffer.Length, OnDataReceived, null);
            }
            else
            {
                Debug.Log($"연결 종료");
                CloseConnection();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void ProcessMessage(string message)
    {
        bool checkMessage = message.Contains("|");
        if (!checkMessage) return;

        message = message.TrimEnd('\n');
        string[] parts = message.Split('|');
        Define.MessageType messageType = (Define.MessageType)Enum.Parse(typeof(Define.MessageType), parts[0]);

        string userID;
        float hp;
        string[] position;
        float x, y;

        switch (messageType)
        {
            case Define.MessageType.Join:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                hp = float.Parse(parts[2]);
                position = parts[3].Split(',');
                x = float.Parse(position[0]);
                y = float.Parse(position[1]);
                if (userID == ObjectMgr.Instance.LocalId)
                {
                    ObjectMgr.Instance.Setting<PlayerCtrl>(userID, hp, new Vector3(x, y, 0));
                }
                else
                {
                    ObjectMgr.Instance.AddPlayer(userID, hp, new Vector3(x, y, 0));
                }
                break;

            case Define.MessageType.Leave:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;

                ObjectMgr.Instance.RemovePlayer(userID);
                break;

            case Define.MessageType.PlayerMove:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                if (userID == ObjectMgr.Instance.LocalId) break;
                hp = float.Parse(parts[2]);

                string[] movedPosition = parts[3].Split(",");
                float movedX = float.Parse(movedPosition[0]);
                float movedY = float.Parse(movedPosition[1]);

                string[] velocityData = parts[4].Split(","); // 속도
                float velocityX = float.Parse(velocityData[0]);
                float velocityY = float.Parse(velocityData[1]);

                float speed = float.Parse(parts[6]);

                string flip = parts[7];
                bool isflip = Convert.ToBoolean(flip);

                Vector3 pos = new Vector3(movedX, movedY, 0);
                Vector3 velocity = new Vector3(velocityX, velocityY, 0);

                ObjectMgr.Instance.MovePlayer(userID, hp, pos, velocity, Time.time, speed, isflip);
                break;

            case Define.MessageType.PlayerCheck:
                for (int i = 1; i < parts.Length; i += 3)
                {
                    userID = parts[i];
                    if (string.IsNullOrEmpty(userID)) break;

                    hp = float.Parse(parts[i + 1]);
                    string[] playerData = parts[i + 2].Split(",");

                    if (playerData.Length >= 2 && float.TryParse(playerData[0], out float px) && float.TryParse(playerData[1], out float py))
                    {
                        ObjectMgr.Instance.AddPlayer(userID, hp, new Vector3(px, py, 0));
                    }
                    else
                    {
                        Debug.LogError($"Invalid player data format: {parts[i + 2]}");
                    }
                }
                break;

            case Define.MessageType.PlayeJump:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                if (!ObjectMgr.Instance.players.ContainsKey(userID)) break;
                ObjectMgr.Instance.players[userID].Jump(userID);
                break;

            case Define.MessageType.Chatting:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                string chatMessage = parts[2];
                GameUIMgr.Instance.DisplayChat(userID, chatMessage);
                break;

            case Define.MessageType.Attack:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                if (!ObjectMgr.Instance.players.ContainsKey(userID)) break;
                if (userID == ObjectMgr.Instance.LocalId) break;
                ObjectMgr.Instance.players[userID].Attack(userID);
                break;

            case Define.MessageType.MonsterSpawn:
                for (int i = 1; i < parts.Length; i++)
                {
                    string[] monsterData = parts[i].Split(',');
                    if (monsterData.Length < 5) continue;

                    userID = monsterData[0];
                    hp = float.Parse(monsterData[1]);
                    string state = monsterData[2];
                    float monsterX = ObjectMgr.Instance.SpawnPos[i - 1].transform.position.x;
                    float monsterY = ObjectMgr.Instance.SpawnPos[i - 1].transform.position.y;

                    if (!ObjectMgr.Instance.monsters.ContainsKey(userID))
                    {
                        ObjectMgr.Instance.SpawnMonster(userID, new Vector3(monsterX, monsterY, 0), hp, state);
                    }
                }
                if (ObjectMgr.Instance.monsters.Count != 0 && ObjectMgr.Instance.monsters != null)
                {
                    ObjectMgr.Instance.MonsterInfoSend();
                }
                break;

            case Define.MessageType.MonsterDeath:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                ObjectMgr.Instance.RemoveMonster(userID);
                break;

            case Define.MessageType.MonsterMove:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                if (!ObjectMgr.Instance.monsters.ContainsKey(userID)) break;

                string[] monsterMovePosition = parts[2].Split(",");
                float moveMonsterX = float.Parse(monsterMovePosition[0]);
                float moveMonsterY = float.Parse(monsterMovePosition[1]);
                ObjectMgr.Instance.monsters[userID].MonsterMove(userID, new Vector3(moveMonsterX, moveMonsterY, 0));
                break;

            case Define.MessageType.MonsterPos:
                for (int i = 1; i < parts.Length; i++)
                {
                    string[] monsterData = parts[i].Split(',');
                    if (monsterData.Length < 5) continue;

                    userID = monsterData[0];
                    hp = float.Parse(monsterData[1]);
                    string state = monsterData[2];
                    float monsterX = float.Parse(monsterData[3]);
                    float monsterY = float.Parse(monsterData[4]);

                    if (!ObjectMgr.Instance.monsters.ContainsKey(userID))
                    {
                        if (userID != "999")
                        {
                            ObjectMgr.Instance.SpawnMonster(userID, new Vector3(monsterX, monsterY, 0), hp, state);
                        }
                        else
                        {
                            ObjectMgr.Instance.SpawnBoss(userID, hp);
                        }
                    }
                }
                break;

            case Define.MessageType.MonsterHit:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                float damage = float.Parse(parts[2]);
                if (userID == "999")
                {
                    ObjectMgr.Instance.bosses[userID].CurrentHp -= 10;
                }
                else
                {
                    ObjectMgr.Instance.monsters[userID].CurrentHp -= 10;
                }
                break;

            case Define.MessageType.BossSpawn:
                userID = parts[1];
                hp = float.Parse(parts[2]);

                ObjectMgr.Instance.SpawnBoss(userID, hp);

                break;

            case Define.MessageType.BossDeath:
                userID = parts[1];
                if (string.IsNullOrEmpty(userID)) break;
                ObjectMgr.Instance.RemoveMonster(userID);
                GameUIMgr.Instance.GameClear();
                break;
        }
    }

    private void CloseConnection()
    {
        if (_client != null)
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
            _client.Close();
            _client = null;
        }
    }

    public void SendMessageToServer(string message)
    {
        try
        {
            if (_client != null && _client.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                _stream.Write(data, 0, data.Length);
            }
            else
            {
                Debug.LogWarning($"전송 실패 (서버와의 연결 확인)");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(_playerID))
        {
            ObjectMgr.Instance.RemovePlayer(_playerID);
            ObjectMgr.Instance.ClearMonster();
            SendMessageToServer($"{(int)Define.MessageType.Leave}|{_playerID}");

            ObjectMgr.Instance.ClearPlayer();
        }
    }
}