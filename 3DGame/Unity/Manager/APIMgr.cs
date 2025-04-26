using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class APIMgr : Singleton<APIMgr>
{
    private Action<bool, string> _onComplete;
    private class RankData
    {
        public string userId;
        public string time;
    }
    private bool _isRankSend = false;
    public bool IsRankSend { get { return _isRankSend; } set { _isRankSend = value; } }

    protected override void Initialize()
    {
        base.Initialize();
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator RegisterCoroutine(string userId, string password)
    {
        string jsonBody = $"{{\"userId\":\"{userId}\",\"password\":\"{password}\"}}";
        using (UnityWebRequest request = CreateRequest(Define.RegistUrl, jsonBody))
        {
            yield return request.SendWebRequest();
            HandleResponse(request, isLogin: false);
        }
    }

    public IEnumerator LoginCoroutine(string userId, string password)
    {
        string jsonBody = $"{{\"userId\":\"{userId}\",\"password\":\"{password}\"}}";
        using (UnityWebRequest request = CreateRequest(Define.LoginUrl, jsonBody))
        {
            yield return request.SendWebRequest();
            HandleResponse(request, isLogin: true);
        }
    }

    public IEnumerator RankCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(Define.RankUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleRankResponse(request.downloadHandler.text);
            }
            else
            {
                UIMgr.Instance.PopupUI.SetActive(true);
                UIMgr.Instance.Popup_Message.text = "<#FF0000> 랭킹 로드 에러</color>\n";
            }
        }
    }

    public IEnumerator RankSerachCoroutine(string userId)
    {
        string url = $"{Define.MyRankUrl}/{UnityWebRequest.EscapeURL(userId)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                RankData rank = JsonConvert.DeserializeObject<RankData>(request.downloadHandler.text);

                HandleSingleRankResponse(rank);
            }
            else
            {
                UIMgr.Instance.PopupUI.SetActive(true);
                UIMgr.Instance.Popup_Message.text = "<#FF0000> 랭킹 로드 에러</color>\n";
            }
        }
    }

    private UnityWebRequest CreateRequest(string url, string jsonBody)
    {
        byte[] body = Encoding.UTF8.GetBytes(jsonBody);
        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private void HandleSingleRankResponse(RankData rank)
    {
        // 결과 표시 예시
        UIMgr.Instance.Rank_Message.text =
            $"{rank.userId} - <#0009FF>{rank.time}</color>";
    }

    private void HandleResponse(UnityWebRequest request, bool isLogin)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            if (isLogin)
            {
                GameMgr.Instance.PlayerId = UIMgr.Instance.Login_IdInputField.text;
                UIMgr.Instance.LoginUI.SetActive(false);
                UIMgr.Instance.LobbyUI.SetActive(true);
                NetworkMgr.Instance.ConnectToServer();
            }
            else
            {
                _onComplete?.Invoke(true, "회원가입 성공!");
                UIMgr.Instance.PopupUI.SetActive(true);
                UIMgr.Instance.Popup_Message.text = "<#00FF80>회원가입 성공</color>";
                UIMgr.Instance.Register_IdInputField.text = string.Empty;
                UIMgr.Instance.Register_PwInputField.text = string.Empty;
                UIMgr.Instance.RegisterUI.SetActive(false);
                UIMgr.Instance.LoginUI.SetActive(true);
            }
        }
        else
        {
            if (isLogin)
            {
                UIMgr.Instance.PopupUI.SetActive(true);
                UIMgr.Instance.Popup_Message.text = "<#FF0000>로그인 실패</color>\n아이디, 패스워드를 확인하세요";
            }
            else
            {
                UIMgr.Instance.PopupUI.SetActive(true);
                UIMgr.Instance.Popup_Message.text = "<#FF0000>회원가입 실패</color>\n";
            }
            string errorMsg = $"실패: {request.error}";
            _onComplete?.Invoke(false, errorMsg);
        }
    }

    private void HandleRankResponse(string json)
    {
        try
        {
            List<RankData> ranks = JsonConvert.DeserializeObject<List<RankData>>(json);

            if (ranks == null || ranks.Count == 0)
            {
                UIMgr.Instance.Rank_Message.text = $"랭킹 데이터가 없습니다.";
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < ranks.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {ranks[i].userId} <#0009FF>{ranks[i].time}</color>");

                    UIMgr.Instance.Rank_Message.text = sb.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex.Message}");
        }
    }

    public void RankUpdate()
    {
        if (!_isRankSend)
        {
            StartCoroutine(SubRankCoroutine(GameMgr.Instance.PlayerId, GameMgr.Instance.CurrentTime));
        }
    }

    private IEnumerator SubRankCoroutine(string userId, string time)
    {
        string url = $"{Define.RankUpdate}";
        RankData data = new RankData()
        {
            userId = userId,
            time = time
        };

        string json = JsonConvert.SerializeObject(data);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) { _isRankSend = true; }
            else
            {
                UIMgr.Instance.PopupUI.SetActive(true);
                UIMgr.Instance.Popup_Message.text = $"저장 실패";
            }
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