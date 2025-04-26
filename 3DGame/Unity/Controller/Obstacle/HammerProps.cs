using UnityEngine;

public class HammerProps : MonoBehaviour
{
    private float _speed = 25f;
    private float _max = 60f;
    private float _min = -60f;

    Rigidbody _rigidbody;

    float _direction = 1f;
    private float _randomOffset;
    private float _currentAngle;
    private Vector3 _offSet;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.mass = 100f;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezePositionZ;

        _randomOffset = Random.Range(0f, 2f * Mathf.PI);
        _offSet = transform.position;
    }

    private void FixedUpdate()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        float t = Mathf.Sin((Time.time * _speed * 0.1f) + _randomOffset);
        //_currentAngle = Mathf.PingPong(Time.time * _speed, _max - _min) + _min;
        _currentAngle = Mathf.Lerp(_min, _max, (t + 1) * 0.5f);
        transform.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
        transform.position = _offSet;
    }
}