using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting) return null;

            if (_instance == null)
            {
                GameObject manager = GameObject.Find("@Managers");
                if (manager == null)
                {
                    manager = new GameObject("@Managers");
                    DontDestroyOnLoad(manager);
                }
                // 기존 인스턴스 찾기
                _instance = FindAnyObjectByType<T>();

                // 없으면 새로 생성
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
        
    }

    protected virtual void OnEnable()
    {
        // 중복 생성 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Application.quitting += OnApplicationQuitEvent;
    }

    protected virtual void OnDisable()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        Application.quitting -= OnApplicationQuitEvent;
    }

    protected virtual void OnApplicationQuitEvent()
    {
        _applicationIsQuitting = true;
        if (_instance == this)
        {
            Destroy(gameObject);
            _instance = null;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}