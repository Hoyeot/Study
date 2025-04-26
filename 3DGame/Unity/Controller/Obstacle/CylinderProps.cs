using UnityEngine;

public class CylinderProps : MonoBehaviour
{
    private float _speed = 10f;
    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 0, 10f) * _speed * Time.deltaTime);
    }
}