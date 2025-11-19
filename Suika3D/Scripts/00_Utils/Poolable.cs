using UnityEngine;
using Global;

public class Poolable : MonoBehaviour
{
    public ResourceEnum m_rscEnumID;

    public void ReturnResource()
    {
        this.gameObject.SetActive(false);
        ObjectPoolManager.GetInstance().ReturnObject(this);
    }
}