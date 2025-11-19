using UnityEngine;
using Global;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform m_spawnPoint;
    [SerializeField] private Transform m_targetPoint;
    [SerializeField] private float m_arcHeight = 1f;
    [SerializeField] private float m_launchForce = 2.9f;
    //[SerializeField] private GuideDrawSystem m_guideDrawSystem;
    [SerializeField] private Transform m_previewSlot;
    private GameObject m_previewInstance;

    private void Start()
    {
        PrepareNextFruit();
    }

    private void Update()
    {
        Spawn();
    }

    private Vector3 GetArcVelocity()
    {
        Vector3 direction = (m_targetPoint.position - m_spawnPoint.position);
        direction += Vector3.up * m_arcHeight;
        return direction.normalized * m_launchForce;
    }

    #region Spawn
    private void Spawn()
    {
        Vector3 velocity = GetArcVelocity();
        //m_guideDrawSystem.DrawTrajectory(m_spawnPoint.position, velocity);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnFruit(velocity);
            //m_guideDrawSystem.Clear();
            PrepareNextFruit();
        }
    }

    private void SpawnFruit(Vector3 force)
    {
        if (m_previewInstance == null) return;

        m_previewInstance.transform.SetParent(null);
        m_previewInstance.transform.position = m_spawnPoint.position;
        m_previewInstance.transform.rotation = m_spawnPoint.rotation;
        m_previewInstance.transform.localScale = Vector3.one;

        var rb = m_previewInstance.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = force;

        m_previewInstance = null;
    }
    #endregion

    private void PrepareNextFruit()
    {
        ResourceEnum fruitType = GetRandomFruit();

        m_previewInstance = GlobalUtils.LoadObject(fruitType);
        if (m_previewInstance == null) return;

        m_previewInstance.transform.SetParent(m_previewSlot);
        m_previewInstance.transform.localPosition = Vector3.zero;
        m_previewInstance.transform.localRotation = Quaternion.identity;
        m_previewInstance.transform.localScale = Vector3.one * 0.6f;

        var rb = m_previewInstance.GetComponent<Rigidbody>();
        if (rb == null) rb = m_previewInstance.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private ResourceEnum GetRandomFruit()
    {
        int start = (int)ResourceEnum.FRUIT_CHERRY;
        int end = (int)ResourceEnum.FRUIT_PEAR + 1;
        return (ResourceEnum)Random.Range(start, end);
    }
}