using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.U2D.ScriptablePacker;

public class Insert : MonoBehaviour
{
    private string host = "IP";
    private int port = Port;

    public TMP_InputField idInput;
    public TMP_InputField pwInput;
    public TMP_InputField nameInput;
    public TMP_InputField nicknameInput;

    public class PacketData
    {
        public string url;
        public int userid;
    }

    public class UserData
    {
        public string userid;
        public string userpw;
        public string username;
        public string usernickname;
    }

    public void InsertQry()
    {
        StartCoroutine(ServerMakerUser());
    }

    IEnumerator ServerMakerUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", idInput.text);
        form.AddField("user_pw", pwInput.text);
        form.AddField("user_name", nameInput.text);
        form.AddField("user_nickname", nicknameInput.text);
        Debug.Log($"ID: {idInput.text}, PW: {pwInput.text}, Name: {nameInput.text}, Nickname: {nicknameInput.text}");
        UnityWebRequest www = UnityWebRequest.Post("IP", form);

        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            PacketData data = JsonUtility.FromJson<PacketData>(www.downloadHandler.text);
        }
    }
}
