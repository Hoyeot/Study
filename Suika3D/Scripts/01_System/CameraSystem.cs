using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private float m_rotationSpeed = 80f;
    [SerializeField] private Transform m_spawnPoint;

    void Update()
    {
        Drag();
    }

    private void Drag()
    {
        float horizontalInput = 0f;

        #region PC (KeyBoard)
        if (Input.GetKey(KeyCode.A))
            horizontalInput = 1f;
        else if (Input.GetKey(KeyCode.D))
            horizontalInput = -1f;

        if (Input.GetKey(KeyCode.LeftArrow))
            horizontalInput = 1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            horizontalInput = -1f;

        if (horizontalInput != 0f)
        {
            transform.Rotate(0f, horizontalInput * m_rotationSpeed * Time.deltaTime, 0f);

            if (m_spawnPoint != null)
                m_spawnPoint.Rotate(0f, horizontalInput * m_rotationSpeed * Time.deltaTime, 0f);
        }
        #endregion

        #region Mobile
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = touch.deltaPosition.x;
                transform.Rotate(0f, deltaX * m_rotationSpeed * Time.deltaTime, 0f);
            }
        }
        #endregion
    }
}