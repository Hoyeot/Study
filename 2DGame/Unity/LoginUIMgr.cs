using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;

public class LoginUIMgr : Singleton<LoginUIMgr>
{
    public TMP_InputField InputFieldLoginUserId;
    public TMP_InputField InputFieldLoginUserPW;
    public TMP_InputField InputFieldJoinUserID;
    public TMP_InputField InputFieldJoinUserPW;

    public Button ButtonLogin;
    public Button ButtonJoin;
    public Button ButtonCreate;
    public Button ButtonClose;

    public GameObject PanelLogin;
    public GameObject PanelJoin;
    public GameObject PanelPopup;

    public TMP_Text TextPopupMessage;

    private void Start()
    {
        PanelLogin.SetActive(true);
        ButtonLogin.onClick.AddListener(OnClickButtonLogin);
        ButtonJoin.onClick.AddListener(OnClickButtonJoin);
        ButtonCreate.onClick.AddListener(OnClickButtonCreate);
        ButtonClose.onClick.AddListener(OnClickButtonClose);
        PanelJoin.SetActive(false);
        PanelPopup.SetActive(false);
    }

    private void Popup(int state)
    {
        switch (state)
        {
            case (int)Define.DBReceived.Empty:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"ID/PW�� �Էµ��� �ʾҽ��ϴ�.";
                break;

            case (int)Define.DBReceived.LoginFailed:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"IP/PW�� �ùٸ��� �ʽ��ϴ�.";
                break;

            case (int)Define.DBReceived.JoinSuccess:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"ȸ������ ����.";
                break;

            case (int)Define.DBReceived.JoinFailed:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"�̹� ���ԵǾ��ִ� �����Դϴ�.";
                break;

            case (int)Define.DBReceived.JoinAlready:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"�̹� ���� �� �Դϴ�.";
                break;

            case (int)Define.DBReceived.UpdateFailed:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"������Ʈ ����";
                break;

            case (int)Define.DBReceived.ConnectFailed:
                PanelPopup.SetActive(true);
                TextPopupMessage.text = $"DB ���� ����";
                break;
        }
    }

    private void OnClickButtonLogin() // �α���
    {
        if (string.IsNullOrEmpty(InputFieldLoginUserId.text) && string.IsNullOrEmpty(InputFieldLoginUserPW.text))
        {
            Popup((int)Define.DBReceived.Empty);
            return;
        }
        StartCoroutine(LoginCheckUserInfo());
    }

    private void OnClickButtonJoin() // ȸ������
    {
        InputFieldLoginUserId.text = null;
        InputFieldLoginUserPW.text = null;
        PanelLogin.SetActive(false);
        PanelJoin.SetActive(true);
    }

    private void OnClickButtonCreate() // ���� (Insert)
    {
        if (string.IsNullOrEmpty(InputFieldJoinUserID.text) && string.IsNullOrEmpty(InputFieldJoinUserPW.text))
        {
            Popup((int)Define.DBReceived.Empty);
            return;
        }
        StartCoroutine(JoinUser());
    }

    private void OnClickButtonClose()
    {
        PanelPopup.SetActive(false);
    }

    IEnumerator LoginCheckUserInfo() // ��ȸ (Select)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", InputFieldLoginUserId.text);
        form.AddField("user_pw", InputFieldLoginUserPW.text);

        using (UnityWebRequest www = UnityWebRequest.Post(Define.GetHost, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    DBPacket response = JsonConvert.DeserializeObject<DBPacket>(www.downloadHandler.text);

                    switch (response.status_code)
                    {
                        case 200:
                            if (response.useFlag == 1)
                            {
                                Popup((int)Define.DBReceived.JoinAlready);
                                break;
                            }
                            ObjectMgr.Instance.LocalId = InputFieldLoginUserId.text;
                            StartCoroutine(UpdateFlag());
                            break;

                        case 404: // not found
                            break;
                    }
                }
                catch
                {
                    Popup((int)Define.DBReceived.LoginFailed);
                }
            }
            else
            {
                Popup((int)Define.DBReceived.ConnectFailed);
            }
        }
    }
    IEnumerator JoinUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", InputFieldJoinUserID.text);
        form.AddField("user_pw", InputFieldJoinUserPW.text);

        using (UnityWebRequest www = UnityWebRequest.Post(Define.InsertHost, form))
        {
            yield return www.SendWebRequest();

            if (www.error != null)
            {
                Popup((int)Define.DBReceived.ConnectFailed);
            }
            else
            {
                try
                {
                    DBPacket response = JsonConvert.DeserializeObject<DBPacket>(www.downloadHandler.text);

                    if (response.status_code == Define.Success)
                    {
                        Popup((int)Define.DBReceived.JoinSuccess);

                        InputFieldJoinUserID.text = null;
                        InputFieldJoinUserPW.text = null;
                        PanelJoin.SetActive(false);
                        PanelLogin.SetActive(true);
                    }
                    else if (response.status_code == Define.DuplicateError)
                    {
                        Popup((int)Define.DBReceived.JoinFailed);
                    }
                }
                catch
                {
                    Popup((int)Define.DBReceived.UpdateFailed);
                }
            }
        }
    }

    IEnumerator UpdateFlag()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", InputFieldLoginUserId.text);
        form.AddField("useFlag", 1);

        using (UnityWebRequest www = UnityWebRequest.Post(Define.UpdateHost, form))
        {
            yield return www.SendWebRequest();

            if (www.error != null)
            {
                Popup((int)Define.DBReceived.ConnectFailed);
            }
            else
            {
                try
                {
                    DBPacket response = JsonConvert.DeserializeObject<DBPacket>(www.downloadHandler.text);

                    if (response.status_code != Define.Success)
                    {
                        Popup((int)Define.DBReceived.UpdateFailed);
                    }
                    else
                    {
                        SceneManager.LoadScene("Game");
                    }
                }
                catch
                {
                    Popup((int)Define.DBReceived.UpdateFailed);
                }
            }
        }
    }

    public IEnumerator Logout()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", ObjectMgr.Instance.LocalId);
        form.AddField("useFlag", 0);

        using (UnityWebRequest www = UnityWebRequest.Post(Define.UpdateHost, form))
        {
            yield return www.SendWebRequest();

            if (www.error != null)
            {
                Popup((int)Define.DBReceived.ConnectFailed);
            }
            else
            {
                DBPacket response = JsonConvert.DeserializeObject<DBPacket>(www.downloadHandler.text);

                if (response.status_code != Define.Success)
                {
                    Popup((int)Define.DBReceived.UpdateFailed);
                }
                else
                {
                    SceneManager.LoadScene("Login");
                }
            }
        }
    }
}