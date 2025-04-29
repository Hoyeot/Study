using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMgr : Singleton<PlayerMgr>
{
    private Transform _playerContainer;
    private Dictionary<string, GameObject> _players = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> Players { get { return _players; } }
    private GameObject _localPlayer;
    public GameObject LocalPlayer {  get { return _localPlayer; } }
    private bool _isLocalPlayerSpawned = false;

    private GameObject _localPlayerPrefab;
    private GameObject _remotePlayerPrefab;

    private float _positionUpdateInterval = 0.25f;

    protected override void Initialize()
    {
        base.Initialize();

        _localPlayerPrefab = Resources.Load<GameObject>(Define.LocalPlayerPrefabPath);
        _remotePlayerPrefab = Resources.Load<GameObject>(Define.NetworkPlayerPrefabPath);
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnLocalPlayer()
    {
        if (_isLocalPlayerSpawned && _localPlayer != null)
        {
            _localPlayer.SetActive(true);
            return;
        }

        if (_localPlayerPrefab == null) return;

        _localPlayer = Instantiate(_localPlayerPrefab, Vector3.zero, Quaternion.Euler(0, 90, 0), _playerContainer);
        _localPlayer.name = GameMgr.Instance.PlayerId;
        _localPlayer.AddComponent<PlayerCtrl>();

        _players.Add(GameMgr.Instance.PlayerId, _localPlayer);
        _isLocalPlayerSpawned = true;
    }

    public void SpawnRemotePlayer(string playerId, Vector3 pos, Vector3 rot)
    {
        if (!_players.ContainsKey(playerId))
        {
            if (_remotePlayerPrefab == null) return;
            GameObject _remotePlayer = Instantiate(_remotePlayerPrefab, pos, Quaternion.Euler(rot), _playerContainer);
            _remotePlayer.name = playerId;

            NetworkPlayer np = _remotePlayer.AddComponent<NetworkPlayer>();
            np.Initialize(playerId, $"Player_{playerId}");
            np.UpdateTransform(pos, rot);
            _players.Add(playerId, _remotePlayer);
        }
    }

    public void DestroyPlayer(string playerId)
    {
        if (_players.TryGetValue(playerId, out GameObject player))
        {
            if (player == _localPlayer)
            {
                player.SetActive(false);
            }
            else
            {
                Destroy(player);
            }
            _players.Remove(playerId);
        }
    }

    public IEnumerator SendPositionUpdate()
    {
        WaitForSeconds delay = new WaitForSeconds(_positionUpdateInterval);
        Vector3 _lastPos = Vector3.zero;
        Vector3 _lastRot = Vector3.zero;
        float _lastSpeed = 0f;
        bool _lastIsGround = true;

        while (NetworkMgr.Instance.IsConnected)
        {
            // 이동, 회전 서버패킷 전송 최소화
            if (_localPlayer != null && !UIMgr.Instance.IsMenuOpen && (Vector3.Distance(_lastPos, _localPlayer.transform.position) > 0.01f || Vector3.Distance(_lastRot, _localPlayer.transform.rotation.eulerAngles) > 0.5f))
            {
                PlayerCtrl playerCtrl = _localPlayer.GetComponent<PlayerCtrl>();
                Animator animator = _localPlayer.GetComponent<Animator>();

                float currentSpeed = animator.GetFloat(Define.Speed);
                bool currentIsGround = animator.GetBool(Define.IsGround);

                bool positionChanged = Vector3.Distance(_lastPos, _localPlayer.transform.position) > 0.01f;
                bool rotationChanged = Vector3.Distance(_lastRot, _localPlayer.transform.rotation.eulerAngles) > 0.5f;
                bool animationChanged = Mathf.Abs(_lastSpeed - currentSpeed) > 0.1f || _lastIsGround != currentIsGround;

                if (positionChanged || rotationChanged || animationChanged)
                {
                    string msg = $"{Define.Move}|" +
                        $"{GameMgr.Instance.PlayerId}|" +
                        $"{_localPlayer.transform.position.x.ToString("F2")}|" +
                        $"{_localPlayer.transform.position.y.ToString("F2")}|" +
                        $"{_localPlayer.transform.position.z.ToString("F2")}|" +
                        $"{_localPlayer.transform.rotation.eulerAngles.x.ToString("F2")}|" +
                        $"{_localPlayer.transform.rotation.eulerAngles.y.ToString("F2")}|" +
                        $"{_localPlayer.transform.rotation.eulerAngles.z.ToString("F2")}|" +
                        $"{currentSpeed.ToString("F2")}|" +
                        $"{currentIsGround.ToString()}\n";

                    NetworkMgr.Instance.SendToServer(msg);

                    _lastPos = _localPlayer.transform.position;
                    _lastRot = _localPlayer.transform.rotation.eulerAngles;
                    _lastSpeed = currentSpeed;
                    _lastIsGround = currentIsGround;
                }
            }
            yield return delay;
        }
    }

    public void UpdatePlayerPosition(string playerId, Vector3 pos, Vector3 rot, float speed, bool isGround)
    {
        if (_players.TryGetValue(playerId, out GameObject player))
        {
            NetworkPlayer np = player.GetComponent<NetworkPlayer>();
            if (np != null)
            {
                // 점프 트리거는 별도로 처리해야 함 (서버에서 특정 패킷으로 보내야 함)
                np.UpdateTransform(pos, rot, speed, isGround, false);
            }
        }
    }

    public void ClearCurrentChannelPlayers()
    {
        if (_localPlayer != null)
        {
            _localPlayer.SetActive(false);
        }

        foreach (var player in _players.Values)
        {
            if (player != null && player != _localPlayer) Destroy(player);
        }
        _players.Clear();

        if (_localPlayer != null)
        {
            _players.Add(GameMgr.Instance.PlayerId, _localPlayer);
        }
    }

    public void ResetLocalPlayerPosition()
    {
        if (_localPlayer != null)
        {
            _localPlayer.transform.position = Vector3.zero;
            _localPlayer.transform.rotation = Quaternion.Euler(0, 90, 0);
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
