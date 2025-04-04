using UnityEngine;

public class CameraCtrl : BaseCtrl
{
    private Transform _target;
    private Vector3 _offset = new Vector3(0, 6, -10);
    private Vector3 _movePos;
    private float _speed = 3f;

    protected override void Initialize()
    {
        _target = ObjectMgr.Instance.Player.transform;
    }

    private void FixedUpdate()
    {
        //_movePos = _target.position + offset;
        _movePos = new Vector3(_target.transform.position.x, _target.transform.position.y, 0) + _offset;
        transform.position = Vector3.Lerp(transform.position, _movePos, _speed * Time.deltaTime);
    }
}
