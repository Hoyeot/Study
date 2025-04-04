using UnityEngine;

public class SpawnMgr : MonoBehaviour
{
    void Start()
    {
        ObjectMgr.Instance.ResoruceLoad();
        ObjectMgr.Instance.Spawn<PlayerCtrl>(Vector3.zero);
    }
}