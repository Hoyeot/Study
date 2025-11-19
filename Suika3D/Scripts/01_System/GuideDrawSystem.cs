using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GuideDrawSystem : MonoBehaviour
{
    [SerializeField] private int m_resolution = 30;
    [SerializeField] private float m_timeStep = 0.1f;
    [SerializeField] private LineRenderer m_lineRenderer;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_lineRenderer.startColor = Color.green;
        m_lineRenderer.endColor = Color.green;
    }

    public void DrawTrajectory(Vector3 startPos, Vector3 velocity)
    {
        m_lineRenderer.positionCount = m_resolution;

        for (int i = 0; i < m_resolution; i++)
        {
            float t = i * m_timeStep;
            Vector3 point = startPos + velocity * t + 0.5f * Physics.gravity * t * t;
            m_lineRenderer.SetPosition(i, point);

            if (i > 1 && point.y < 0)
            {
                m_lineRenderer.positionCount = i + 1;
                break;
            }
        }
    }

    public void Clear()
    {
        m_lineRenderer.positionCount = 0;
    }
}