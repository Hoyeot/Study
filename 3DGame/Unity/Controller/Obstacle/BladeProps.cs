using UnityEngine;

public class BladeProps : MonoBehaviour
{
    float _speed = 15f;

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0f, 10f, 0f) * _speed  * Time.deltaTime, Space.World);
    }
}