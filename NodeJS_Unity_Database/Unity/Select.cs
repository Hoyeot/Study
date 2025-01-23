using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class Select : MonoBehaviour
{
    private string host = "IP";
    private int port = "Port";

    public void SelectQry()
    {
        StartCoroutine(GetTest());
    }
    public void InsertQry()
    {
        StartCoroutine(PostTest());
    }

    public IEnumerator GetTest() // Get 방식
    {
        UnityWebRequest www = UnityWebRequest.Get(host);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }

    IEnumerator PostTest() // Post 방식
    {
        string url = "IP (Port)";
        WWWForm form = new WWWForm(); // POST 통신을 진행할때 정보를 담아서 전달하는 방식 중에 하나
        string id = "hoyeon";
        string pw = "qwe123";
        form.AddField("user_id", id);
        form.AddField("user_pw", pw);
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {

            Debug.Log("error");
        }
    }
}
