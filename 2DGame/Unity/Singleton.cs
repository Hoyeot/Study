using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance = null;
    public static bool IsInstance => _instance != null;
    public static T TryGetInstance() => IsInstance ? _instance : null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject manager = GameObject.Find("@Managers");
                if (manager == null)
                {
                    manager = new GameObject("@Managers");
                    DontDestroyOnLoad(manager);
                }
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    T component = obj.AddComponent<T>();
                    obj.transform.parent = manager.transform;
                    _instance = component;
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    protected void OnSceneChanged(Scene scene, LoadSceneMode mode)
    {
        Clear();
    }

    protected virtual void Clear()
    {

    }
}