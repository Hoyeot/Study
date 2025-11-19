using UnityEngine;
using Global;

public abstract class FruitBase : MonoBehaviour
{
    [SerializeField] protected ResourceEnum m_fruitType;
    [SerializeField] protected Rigidbody m_rigidbody;
    [SerializeField] protected bool m_hasMerged = false;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Initialize(ResourceEnum _rscEnum)
    {
        m_fruitType = _rscEnum;
    }

    private void Merge(FruitBase other)
    {
        ResourceEnum next = GetNextFruitType();
        if (next == ResourceEnum.FRUIT_WATERMELLON) return;

        GameObject evolved = GlobalUtils.LoadObject(next);
        evolved.transform.position = (this.transform.position + other.transform.position) * 0.5f;
        evolved.transform.rotation = Quaternion.identity;
        evolved.gameObject.SetActive(true);

        var rb = evolved.GetComponent<Rigidbody>();
        //rb.isKinematic = false;

        Destroy(this.gameObject);
        Destroy(other.gameObject);
    }

    protected ResourceEnum GetNextFruitType()
    {
        int next = (int)m_fruitType + 1;
        return (ResourceEnum)next;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(GStrings.G_Ground))
        {
            InGameManager.Instance?.GameOver();
            return;
        }

        FruitBase other = collision.gameObject.GetComponent<FruitBase>();
        if (other == null) return;

        if (other.m_fruitType == this.m_fruitType && !m_hasMerged && !other.m_hasMerged)
        {
            if (this.transform.position.y < other.transform.position.y)
                return;

            m_hasMerged = true;
            other.m_hasMerged = true;

            Merge(other);
        }
    }
}