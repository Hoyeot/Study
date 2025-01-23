using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class Select : MonoBehaviour
{
    private string host = "http://localhost:3000/users/getUsers";
    private int port = 3000;

    public void SelectQry()
    {
        StartCoroutine(GetTest());
    }
    public void InsertQry()
    {
        StartCoroutine(PostTest());
    }
    //void Start()
    //{
    //    StartCoroutine(GetTest()); // Get
    //    //StartCoroutine(PostTest()); // Post
    //}
    //void Update()
    //{

    //}
    public IEnumerator GetTest() // Get ���
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

    IEnumerator PostTest() // Post ���
    {
        string url = "http://localhost:3000/users/insertUser";
        WWWForm form = new WWWForm(); // POST ����� �����Ҷ� ������ ��Ƽ� �����ϴ� ��� �߿� �ϳ�
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
