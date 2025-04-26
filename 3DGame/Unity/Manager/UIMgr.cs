using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIMgr : Singleton<UIMgr>
{
    [Header("Login UI")]
    public GameObject LoginUI;
    public Button LoginButton;
    public Button GoRegisterButton;
    public Button RankButton;
    public TMP_InputField Login_IdInputField;
    public TMP_InputField Login_PwInputField;

    [Header("Register UI")]
    public GameObject RegisterUI;
    public Button RegisterButton;
    public Button PreviousButton;
    public TMP_InputField Register_IdInputField;
    public TMP_InputField Register_PwInputField;

    [Header("Popup UI")]
    public GameObject PopupUI;
    public Button Popup_CloseButton;
    public TMP_Text Popup_Message;

    [Header("RankBoard UI")]
    public GameObject RankBoardUI;
    public Button Rank_CloseButton;
    public Button Rank_SerachButton;
    public TMP_Text Rank_Message;
    public TMP_InputField Rank_IdInputField;
    private bool _isRankSend = false;

    [Header("Lobby UI")]
    public GameObject LobbyUI;
    public Button[] ChannelButton; // 0 : Lobby, 1~3 : Channel
    public Button SendButton;
    public Button LogoutButton;
    public Button CloseButton;
    public TMP_Text ChannelText;
    public TMP_Text MessageText;
    public TMP_Text ChannelCount0;
    public TMP_Text ChannelCount1;
    public TMP_Text ChannelCount2;
    public TMP_Text ChannelCount3;
    public TMP_InputField MessageInput;

    [Header("Menu UI")]
    public GameObject MenuUI;
    public Button MenuButton;
    public TMP_Text TimeText;
    private bool _isMenuOpen = true;
    public bool IsMenuOpen { get { return _isMenuOpen; } set { _isMenuOpen = value; } }
    private float _startTime;
    public float StartTime { get { return _startTime; } set { _startTime = value; } }

    private bool _isCountTime = false;
    public bool IsCountTime { get { return _isCountTime; } set { _isCountTime = value; } }

    protected override void Initialize()
    {
        base.Initialize();

        LoginButton.onClick.AddListener(() => LoginButtonClick());
        GoRegisterButton.onClick.AddListener(() => GoRegisterButtonClick());
        RegisterButton.onClick.AddListener(() => RegisterButtonClick());
        PreviousButton.onClick.AddListener(() => PreviousButtonClick());
        Popup_CloseButton.onClick.AddListener(() => PopupCloseButtonClick());
        RankButton.onClick.AddListener(() => RankButtonClick());
        Rank_SerachButton.onClick.AddListener(() => RankSerachButtonClick());
        Rank_CloseButton.onClick.AddListener(() => RankCloseButtonClick());
        LogoutButton.onClick.AddListener(() => Logout());
        SendButton.onClick.AddListener(() => SendButtonClick());
        CloseButton.onClick.AddListener(() => CloseButtonClick());
        MenuButton.onClick.AddListener(() => MenuButtonClick());
        MessageInput.onSubmit.AddListener(OnSubmitInputFieldChat);

        for (int i = 0; i < ChannelButton.Length; i++)
        {
            int channelIndex = i;
            ChannelButton[i].onClick.AddListener(() => GameMgr.Instance.SwitchChannel(channelIndex));
        }
        UpdateUI();
        DontDestroyOnLoad(gameObject);
    }

    private void GoRegisterButtonClick()
    {
        Login_IdInputField.text = string.Empty;
        Login_PwInputField.text = string.Empty;
        LoginUI.SetActive(false);
        RegisterUI.SetActive(true);
    }

    private void RegisterButtonClick()
    {
        if (Register_IdInputField.text == null || Register_PwInputField.text == null) return;
        Register(Register_IdInputField.text, Register_PwInputField.text);
    }

    private void LoginButtonClick()
    {
        if (Login_IdInputField.text == null || Login_PwInputField.text == null) return;
        Login(Login_IdInputField.text, Login_PwInputField.text);
    }

    private void PreviousButtonClick()
    {
        RegisterUI.SetActive(false);
        LoginUI.SetActive(true);
    }

    private void PopupCloseButtonClick()
    {
        Popup_Message.text = string.Empty;
        PopupUI.SetActive(false);
    }

    private void RankButtonClick()
    {
        RankBoardUI.SetActive(true);
        StartCoroutine(APIMgr.Instance.RankCoroutine());
    }

    private void RankSerachButtonClick()
    {

        if (string.IsNullOrEmpty(Rank_IdInputField.text)) return;
        StartCoroutine(APIMgr.Instance.RankSerachCoroutine(Rank_IdInputField.text));
    }

    private void RankCloseButtonClick()
    {
        Rank_IdInputField.text = string.Empty;
        RankBoardUI.SetActive(false);
    }

    private void Register(string userId, string password)
    {
        StartCoroutine(APIMgr.Instance.RegisterCoroutine(userId, password));
    }

    private void Login(string userId, string password)
    {
        StartCoroutine(APIMgr.Instance.LoginCoroutine(userId, password));
    }

    private void SendButtonClick()
    {
        if (!string.IsNullOrEmpty(MessageInput.text))
        {
            string message = $"{Define.Chat}|{GameMgr.Instance.PlayerId}|{MessageInput.text}\n";
            NetworkMgr.Instance.SendToServer(message);
            MessageInput.text = string.Empty;
        }
    }

    private void CloseButtonClick()
    {
        LoginUI.SetActive(false);
        LobbyUI.SetActive(false);
        MenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        GameMgr.Instance.IsCountTime = true;
        _isMenuOpen = false;
    }

    private void MenuButtonClick()
    {
        MenuUI.SetActive(false);
        LobbyUI.SetActive(true);
        _isMenuOpen = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Logout()
    {
        NetworkMgr.Instance.DisconnectFromServer();
        MessageText.text = string.Empty; ;
        LoginUI.SetActive(true);
        LobbyUI.SetActive(false);
    }

    private void OnSubmitInputFieldChat(string message)
    {
        SendButtonClick();
        MessageInput.text = string.Empty;
    }

    public void TimeCount()
    {
        if (NetworkMgr.Instance.IsConnected)
        {
            float elapsedTime = Time.time - _startTime;
            GameMgr.Instance.CurrentTime = TimeSpan.FromSeconds(elapsedTime).ToString("hh':'mm':'ss");
            TimeText.text = GameMgr.Instance.CurrentTime;
        }
        else
        {
            _startTime = 0f;
        }
    }

    public void AddChatMessage(string message)
    {
        MessageText.text += $"{message}\n";
    }

    public void UpdateUI()
    {
        SendButton.interactable = (GameMgr.Instance.CurrentChannel >= (int)Define.ChannelType.Lobby);
        ChannelButton[0].interactable = (GameMgr.Instance.CurrentChannel >= (int)Define.ChannelType.Lobby);

        foreach (var btn in ChannelButton)
        {
            btn.interactable = (GameMgr.Instance.CurrentChannel >= (int)Define.ChannelType.Lobby);
        }
    }

    public void UpdateChannelCounts(string countMessage)
    {
        try
        {
            string[] counts = countMessage.Split('|');

            // UI 업데이트
            ChannelCount0.text = $"{counts[1]}";  // 로비 인원
            ChannelCount1.text = $"{counts[2]}";  // 채널 1 인원
            ChannelCount2.text = $"{counts[3]}";  // 채널 2 인원
            ChannelCount3.text = $"{counts[4]}";  // 채널 3 인원
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}");
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