using UnityEngine;
public class NetworkPlayer : MonoBehaviour
{
    //private TMP_Text _nameText;
    private string _playerId;
    private Animator _animator;

    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private float _posLerpSpeed = 6f;
    private float _rotLerpSpeed = 10f;

    private float _targetSpeed;
    private bool _targetIsGround;
    private bool _triggerJump;
    private float _speedLerpSpeed = 5f;

    public void Initialize(string id, string playerName)
    {
        _playerId = id;
        _animator = GetComponent<Animator>();
        //_nameText.text = playerName;
        _targetPos = transform.position;
        _targetRot = transform.rotation;

        _targetSpeed = 0f;
        _targetIsGround = true;
        _triggerJump = false;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPos, _posLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRot, _rotLerpSpeed * Time.deltaTime);

        _animator.SetFloat(Define.Speed, _targetSpeed);
        _animator.SetBool(Define.IsGround, _targetIsGround);

        if (_triggerJump)
        {
            _animator.SetTrigger(Define.OnJump);
            _triggerJump = false;
        }
    }

    public float GetCurrentSpeed()
    {
        return _targetSpeed;
    }

    public bool GetIsGround()
    {
        return _targetIsGround;
    }

    public void UpdateTransform(Vector3 pos, Vector3 rot)
    {
        _targetPos = pos;
        _targetRot = Quaternion.Euler(rot);
    }

    public void UpdateTransform(Vector3 pos, Vector3 rot, float speed, bool isGround, bool jump)
    {
        _targetPos = pos;
        _targetRot = Quaternion.Euler(rot);
        _targetSpeed = speed;
        _targetIsGround = isGround;

        if (jump)
        {
            _triggerJump = true;
        }
    }
}