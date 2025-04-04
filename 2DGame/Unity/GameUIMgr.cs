using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIMgr : Singleton<GameUIMgr>
{
    PlayerCtrl _player;
    public PlayerCtrl Player {  get { return _player; } }

    public TMP_Text TextDisplayChat;
    public TMP_InputField InputFieldChat;

    public Button JumpButton;
    public Button AttackButton;
    public Button SendButton;
    public Button LogOutButton;
    public Button RetryButton;
    public Button ExitButton;

    public GameObject EndGame;

    private void Start()
    {
        _player = ObjectMgr.Instance.Player;
        JumpButton.onClick.AddListener(OnClickJumpButton);
        AttackButton.onClick.AddListener(OnClickAttackButton);
        SendButton.onClick.AddListener(OnClickSendButton);
        LogOutButton.onClick.AddListener(OnClickLogOutButton);
        RetryButton.onClick.AddListener(OnClickRetryButton);
        ExitButton.onClick.AddListener(OnClickExitButton);
    }

    public void GameClear()
    {
        EndGame.SetActive(true);
    }

    public void OnClickJumpButton()
    {
        _player.Jump(_player.CharactedId);
    }

    public void OnClickAttackButton()
    {
        _player.PlayerAttack();
    }

    public void OnClickSendButton()
    {
        string message = $"{(int)Define.MessageType.Chatting}|{ObjectMgr.Instance.LocalId}|{InputFieldChat.text}";
        ClientMgr.Instance.SendMessageToServer(message);
        InputFieldChat.text = string.Empty;
    }

    public void OnClickLogOutButton()
    {
        StartCoroutine(LoginUIMgr.Instance.Logout());
        ClientMgr.Instance.OnApplicationQuit();
    }

    public void OnClickRetryButton()
    {
        string message = $"{(int)Define.MessageType.Retry}";
        ClientMgr.Instance.SendMessageToServer(message);
        InputFieldChat.text = string.Empty;
    }

    public void OnClickExitButton()
    {
        StartCoroutine(LoginUIMgr.Instance.Logout());
        ClientMgr.Instance.OnApplicationQuit();
    }

    public void DisplayChat(string userId, string message)
    {
        TextDisplayChat.text += $"[{userId}] : {message}\n";
    }
}