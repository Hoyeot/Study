using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using System.Text;

public class ObjectMgr : Singleton<ObjectMgr>
{
    private PlayerCtrl _player;
    public PlayerCtrl Player { get { return _player; } }

    private string localId;
    public string LocalId { get { return localId; } set { localId = value; } }

    private MonsterCtrl _monster;
    public MonsterCtrl Monster { get { return _monster; } }

    public Dictionary<string, PlayerCtrl> players = new Dictionary<string, PlayerCtrl>();
    public Dictionary<string, MonsterCtrl> monsters = new Dictionary<string, MonsterCtrl>();
    public Dictionary<string, BossCtrl> bosses = new Dictionary<string, BossCtrl>();

    private GameObject _playerResource;
    private GameObject _monsterResource;
    private GameObject _bossResource;
    public GameObject[] SpawnPos;

    private CinemachineCamera _cinemachineCamera;
    protected override void Initialize()
    {
        base.Initialize();
        ResoruceLoad();
    }

    public void ResoruceLoad()
    {
        _playerResource = Resources.Load<GameObject>(Define.PlayerPath);
        _monsterResource = Resources.Load<GameObject>(Define.MonsterPath);
        _bossResource = Resources.Load<GameObject>(Define.BossPath);
        _cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        SpawnPos = GameObject.FindGameObjectsWithTag(Define.SpawnPos);
    }

    public T Spawn<T>(Vector3 spawnPos) where T : CharacterCtrl
    {
        Type type = typeof(T);

        if (type == typeof(PlayerCtrl))
        {
            GameObject _obj = Instantiate(_playerResource, spawnPos, Quaternion.identity);
            PlayerCtrl _playerCtrl = _obj.GetOrAddComponent<PlayerCtrl>();
            _playerCtrl.CharactedId = localId;
            _playerCtrl.DisplayId(_playerCtrl.CharactedId);
            players.Add(_playerCtrl.CharactedId, _playerCtrl);
            _player = _playerCtrl;
            SetupCinemachineCamera(_obj.transform);

            return _playerCtrl as T;
        }
        return null;
    }

    public T Setting<T>(string userId, float hp, Vector3 pos) where T : CharacterCtrl
    {
        if (!players.ContainsKey(userId)) return null;

        Type type = typeof(T);

        if (type == typeof(PlayerCtrl))
        {
            _player.CharactedId = userId;
            _player.CurrentHp = hp;
            _player.Pos = pos;
        }

        return null;
    }

    private void SetupCinemachineCamera(Transform target)
    {
        _cinemachineCamera.Follow = target;
    }

    public void AddPlayer(string userId, float hp, Vector3 pos)
    {
        if (!players.ContainsKey(userId))
        {
            GameObject _obj = Instantiate(_playerResource, pos, Quaternion.identity);
            PlayerCtrl _playerCtrl = _obj.GetOrAddComponent<PlayerCtrl>();
            _playerCtrl.CharactedId = userId;
            _playerCtrl.CurrentHp = hp;
            _playerCtrl.DisplayId(userId);
            players.Add(userId, _playerCtrl);
        }
    }

    public void MovePlayer(string userId, float hp, Vector3 pos, Vector3 velocity, float timeStamp, float speed, bool isflip)
    {
        if (players.ContainsKey(userId))
        {
            Vector3 predictedPos = (pos + velocity);
            _player.Move(userId, predictedPos, velocity, speed, isflip);
        }
    }

    public void RemovePlayer(string userId)
    {
        if (players.ContainsKey(userId))
        {
            Destroy(players[userId].gameObject);
            players.Remove(userId);
        }
    }

    public void ClearPlayer()
    {
        foreach (var player in players.Values)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
                Resources.UnloadUnusedAssets();
            }
        }
        players.Clear();
    }

    public void SpawnMonster(string monsterId, Vector3 pos, float hp, string state)
    {
        if (!monsters.ContainsKey(monsterId))
        {
            GameObject monsterObj = Instantiate(_monsterResource, pos, Quaternion.identity);
            MonsterCtrl monsterCtrl = monsterObj.GetComponent<MonsterCtrl>();
            monsterCtrl.CharactedId = monsterId;
            monsterCtrl.CurrentHp = hp;
            monsters[monsterId] = monsterCtrl;
        }
    }

    public void ClearMonster()
    {
        foreach (var monster in monsters.Values)
        {
            if (monster != null)
            {
                Destroy(monster.gameObject);
                Resources.UnloadUnusedAssets();
            }
        }
        monsters.Clear();
    }

    public void MonsterInfoSend()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{(int)Define.MessageType.MonsterPos}|");

        foreach (var monster in monsters.Values)
        {
            if (monster != null)
            {
                sb.Append($"{monster.CharactedId},{monster.transform.position.x:F2},{monster.transform.position.y:F2};");
            }
        }
        if (sb.Length > 0) sb.Length--;
        ClientMgr.Instance.SendMessageToServer(sb.ToString());
    }

    public void RemoveMonster(string monsterId)
    {
        if (monsterId == "999")
        {
            if (bosses.ContainsKey(monsterId))
            {
                bosses[monsterId].MonsterDeath();
                bosses.Remove(monsterId);
            }
        }
        else
        {
            if (monsters.ContainsKey(monsterId))
            {
                monsters[monsterId].MonsterDeath();
                monsters.Remove(monsterId);
            }
        }
    }

    public void SpawnBoss(string bossId, float hp)
    {
        if (!monsters.ContainsKey(bossId))
        {
            GameObject bossObj = Instantiate(_bossResource, new Vector3(132, 1, 0), Quaternion.identity);
            BossCtrl bossCtrl = bossObj.GetComponent<BossCtrl>();
            bossCtrl.CharactedId = bossId;
            bossCtrl.CurrentHp = hp;
            bosses.Add(bossId, bossCtrl);
        }
    }
}