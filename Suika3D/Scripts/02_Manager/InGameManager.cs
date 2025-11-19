using Cysharp.Threading.Tasks;
using System.Resources;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private ResourceManager m_resourceManager;
    [SerializeField] private ObjectPoolManager m_objectPoolManager;
    [SerializeField] private SpawnManager m_spawnManager;
    [SerializeField] private bool m_isGameOver = false;

    public static InGameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        m_resourceManager.gameObject.SetActive(true);
        m_objectPoolManager.gameObject.SetActive(true);
        m_spawnManager.gameObject.SetActive(false);

        await UniTask.DelayFrame(1);
        await InitMangers();

        m_spawnManager.gameObject.SetActive(true);
    }

    private async UniTask InitMangers()
    {
        await ObjectPoolManager.GetInstance().Initialize();
        Debug.Log($"ObjectPool Initialized");
    }

    public void GameOver()
    {
        if (m_isGameOver) return;
        m_isGameOver = true;

        Debug.Log($"Game OVer");
    }
}