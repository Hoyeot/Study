using Cysharp.Threading.Tasks;
using Global;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolingObject
{
    public ResourceEnum m_resourceId;
    public GameObject m_poolObject;
    public int m_amountPoolCount;
}

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager m_Instance;
    private Dictionary<ResourceEnum, PoolingObject> m_poolableOrigin;
    private Dictionary<ResourceEnum, Queue<Poolable>> m_dicUsablePool;
    private GameObject m_inactiveContainer;

    public static ObjectPoolManager GetInstance()
    {
        return m_Instance;
    }

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async UniTask Initialize()
    {
        m_poolableOrigin = new();
        m_dicUsablePool = new();

        m_poolableOrigin.Clear();
        m_dicUsablePool.Clear();

        if (m_inactiveContainer == null)
        {
            m_inactiveContainer = new GameObject("Pooled_Inactive_Container");
            m_inactiveContainer.transform.SetParent(transform);
        }

        await LoadResource(RSC_TYPE.FRUIT, GStrings.G_Prefab + RSC_UNIT_TYPE.Fruit.ToString(), GStrings.G_ResourceKey_Fruit);
    }

    private async UniTask LoadResource(RSC_TYPE _upPath, string _subPath, Dictionary<ResourceEnum, string> _dicKey)
    {
        foreach (ResourceEnum _enumid in Enum.GetValues(typeof(ResourceEnum)))
        {
            if (_dicKey.ContainsKey(_enumid))
            {
                string filename = _dicKey[_enumid];
                string path = _subPath + "/";

                GameObject _object = await ResourceManager.GetInstance().GetResources(path, filename);

                if (_object == null)
                {
                    Debug.Log($"Prefab {path} is not exist");
                    continue;
                }

                PoolingObject _poolingObject = new()
                {
                    m_resourceId = _enumid,
                    m_poolObject = _object,
                    m_amountPoolCount = 5
                };

                m_poolableOrigin.Add(_enumid, _poolingObject);

                Queue<Poolable> queue = new();
                m_dicUsablePool.Add(_enumid, queue);
            }
            else
                continue;
        }
    }

    public GameObject CreateObject(ResourceEnum _enumid)
    {
        if (m_poolableOrigin.ContainsKey(_enumid))
        {
            GameObject _poolable = Instantiate(m_poolableOrigin[_enumid].m_poolObject);
            _poolable.transform.SetParent(transform);
            return _poolable;
        }
        else
        {
            return null;
        }
    }

    public T GetObject<T>(ResourceEnum _enumId) where T : MonoBehaviour
    {
        if (m_dicUsablePool.ContainsKey(_enumId))
        {
            if (m_dicUsablePool[_enumId].Count > 0)
            {
                Poolable _poolableObject = m_dicUsablePool[_enumId].Dequeue();
                T _object = _poolableObject.GetComponent<T>();
                return _object;
            }
            else
            {
                GameObject _poolableObject = CreateObject(_enumId);
                T _object = _poolableObject.GetComponent<T>();
                return _object;
            }
        }
        else
        {
            Debug.Log("GetObject return null");
            return null;
        }
    }

    public GameObject GetObject(ResourceEnum _enumId)
    {
        if (m_dicUsablePool.ContainsKey(_enumId))
        {
            if (m_dicUsablePool[_enumId].Count > 0)
            {
                Poolable _poolableObject = m_dicUsablePool[_enumId].Dequeue();
                return _poolableObject.gameObject;
            }
            else
            {
                GameObject _poolableObject = CreateObject(_enumId);
                return _poolableObject;
            }
        }
        else
        {
            Debug.Log($"GetObject ruturn null {_enumId}");
            return null;
        }
    }

    public void ReturnObject(Poolable _poolable)
    {
        ResourceEnum _enumId = _poolable.m_rscEnumID;
        if (m_dicUsablePool.ContainsKey(_enumId))
        {
            _poolable.gameObject.SetActive(false);
            _poolable.gameObject.transform.parent = m_inactiveContainer.transform;

            m_dicUsablePool[_enumId].Enqueue(_poolable);
        }
        else
        {
            Destroy(_poolable.gameObject);
        }
    }

    public void ClearObjectPool()
    {
        m_dicUsablePool?.Clear();
        m_poolableOrigin?.Clear();
    }
}