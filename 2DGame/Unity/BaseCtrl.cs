using UnityEngine;

public abstract class BaseCtrl : MonoBehaviour
{
    private void Awake()
    {
        Initialize();
    }

    protected abstract void Initialize();
}