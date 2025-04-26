using System;
using System.Text;
using UnityEngine;

public class GameMgr : Singleton<GameMgr>
{
    private int _currentChannel = (int)Define.ChannelType.Exit;
    public int CurrentChannel { get { return _currentChannel; } set { _currentChannel = value; } }
    private string _playerId;
    public string PlayerId {  get { return _playerId; } set { _playerId = value; } }
    private float _startTime;
    private string _currentTime;
    public string CurrentTime { get { return _currentTime; } set { _currentTime = value; } }
    private bool _isCountTime = false;
    public bool IsCountTime { get { return _isCountTime; } set { _isCountTime = value; } }

    protected override void Initialize()
    {
        base.Initialize();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && _currentChannel >= (int)Define.ChannelType.Lobby)
        {
            NetworkMgr.Instance.SendMessageToServer();
        }

        if (_isCountTime) UIMgr.Instance.TimeCount();
    }

    public void SwitchChannel(int channel)
    {
        if (_currentChannel < (int)Define.ChannelType.Lobby)
        {
            UIMgr.Instance.AddChatMessage($"이미 채널 {_currentChannel}에 연결되어 있습니다.\n먼저 연결을 종료해주세요.");
            return;
        }

        if (channel == _currentChannel) return;

        try
        {
            PlayerMgr.Instance.ClearCurrentChannelPlayers();

            if (channel > (int)Define.ChannelType.Lobby)
            {
                _currentChannel = channel;
                PlayerMgr.Instance.SpawnLocalPlayer();
                PlayerMgr.Instance.ResetLocalPlayerPosition();
                StartCoroutine(PlayerMgr.Instance.SendPositionUpdate());
            }

            string message = $"{Define.Join}|{channel}|{_playerId}|" +
                $"{PlayerMgr.Instance.LocalPlayer.transform.position.x}|{PlayerMgr.Instance.LocalPlayer.transform.position.y}|{PlayerMgr.Instance.LocalPlayer.transform.position.z}|" +
                $"{PlayerMgr.Instance.LocalPlayer.transform.rotation.eulerAngles.x}|{PlayerMgr.Instance.LocalPlayer.transform.rotation.eulerAngles.y}|{PlayerMgr.Instance.LocalPlayer.transform.rotation.eulerAngles.z}\n";
            byte[] data = Encoding.UTF8.GetBytes(message);
            NetworkMgr.Instance.Stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            UIMgr.Instance.AddChatMessage($"채널변경 실패 : {e.Message}");
            NetworkMgr.Instance.DisconnectFromServer();
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

    protected override void OnDestroy()
    {
        if (NetworkMgr.Instance != null)
        {
            NetworkMgr.Instance.DisconnectFromServer();
        }
        base.OnDestroy();
    }
}