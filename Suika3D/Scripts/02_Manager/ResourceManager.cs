using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager m_Instance;
    private const string BasePath = "Prefabs/";

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static ResourceManager GetInstance()
    {
        return m_Instance;
    }

    //TODO : 접근하는 UniqueId는 Enum으로 수정 예정
    public async UniTask<GameObject> GetResources(string _uppath, string path)
    {
        try
        {
            string fullPath = $"{_uppath}{path}";
            return await Resources.LoadAsync<GameObject>(fullPath) as GameObject;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ResourceManager] Failed to load: {_uppath}{path}, Error: {ex.Message}");
            return null;
        }
    }
}