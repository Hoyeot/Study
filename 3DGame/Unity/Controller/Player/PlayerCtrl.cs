using UnityEngine;
using Unity.Cinemachine;

public class PlayerCtrl : MonoBehaviour
{
    Rigidbody _rigidbody;
    CinemachineCamera _cinemachineCamera;
    CinemachineFollow _cinemachineFollow;
    Animator _animator;

    Vector3 _spawnPoint = Vector3.zero;
    Quaternion _spawnRot = Quaternion.Euler(0, 90, 0);

    float _moveSpeed = 5f;
    float _mouseSpeed = 1f;
    float _jumpForce = 8f;
    float _zoomSpeed = 5f;
    float _knockBackForce = 10f;

    bool _isKnockBack = false;

    float _minZoom = 2f;
    float _maxZoom = 8f;

    [SerializeField]
    bool _isGround = true;
    float _groundCheckDistance = 0.1f;
    float _airControlFactor = 0.1f;
    LayerMask _groundLayer;

    private GameObject _groundCheck;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate; // 보간
        _rigidbody.freezeRotation = true;
        _groundCheck = transform.GetChild(0).gameObject;
        _groundLayer = LayerMask.GetMask("Ground");

        InitializeCamera();
    }

    void Update()
    {
        if (!UIMgr.Instance.IsMenuOpen)
        {
            CheckGround();
            MouseZoom();
            MouseMove();
            HandleJump();
        }
    }

    private void FixedUpdate()
    {
        if (!UIMgr.Instance.IsMenuOpen && !_isKnockBack) Move();
    }

    private void Respawn()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        transform.transform.position = _spawnPoint;
        transform.rotation = _spawnRot;
    }

    private void Knockback()
    {
        //_rigidbody.AddForce(Vector3.back, ForceMode.Impulse);
        _rigidbody.AddForce(-transform.forward * _knockBackForce, ForceMode.Impulse);
        _animator.SetTrigger(Define.OnHit);
        Invoke("EnableMove", 1.5f);
    }

    private void EnableMove()
    {
        _isKnockBack = false;
    }

    private void InitializeCamera()
    {
        GameObject camObj = GameObject.Find("CinemachineCamera");
        _cinemachineCamera = camObj.GetComponent<CinemachineCamera>();
        _cinemachineFollow = camObj.GetComponent<CinemachineFollow>();

        _cinemachineCamera.Target.TrackingTarget = transform;
    }

    private void CheckGround()
    {
        Vector3 boxSize = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);
        _isGround = Physics.CheckBox(_groundCheck.transform.position, boxSize, Quaternion.identity, _groundLayer);
        _animator.SetBool(Define.IsGround, _isGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 boxSize = new Vector3(transform.lossyScale.x, 0.1f, transform.lossyScale.z);
        Gizmos.DrawWireCube(_groundCheck.transform.position, boxSize);
    }

    private void Move()
    {
        float h = Input.GetAxis(Define.Horizontal);
        float v = Input.GetAxis(Define.Vertical);

        Vector3 moveDirection = new Vector3(h, 0f, v).normalized;

        if (moveDirection != Vector3.zero)
        {
            _animator.SetFloat(Define.Speed, 1f);
        }
        else
        {
            _animator.SetFloat(Define.Speed, 0f);
        }

        // 카메라 방향 기준으로 이동 벡터 계산
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 desiredMoveDirection = cameraForward * v + cameraRight * h;

        float controlFactor = _isGround ? 1f : _airControlFactor;

        Vector3 currentVelocity = _rigidbody.linearVelocity;
        Vector3 targetVelocity = desiredMoveDirection * _moveSpeed;
        targetVelocity.y = currentVelocity.y;

        Vector3 velocityChange = (targetVelocity - currentVelocity) * controlFactor;
        velocityChange.y = 0f;

        _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
    }

    private void TryJump()
    {
        if (_isGround)
        {
            _animator.SetTrigger(Define.OnJump);

            // 점프 이벤트 서버에 전송
            string msg = $"{Define.Jump}|{GameMgr.Instance.PlayerId}\n";
            NetworkMgr.Instance.SendToServer(msg);

            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isGround = false;
        }
    }

    private void MouseMove()
    {
        float mouseX = Input.GetAxis(Define.MouseX) * _mouseSpeed;
        _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.Euler(0f, mouseX, 0f));

        float mouseY = -Input.GetAxis(Define.MouseY) * _mouseSpeed;
    }

    private void MouseZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;

        Vector3 newOffset = _cinemachineFollow.FollowOffset;

        newOffset.y = Mathf.Clamp(newOffset.y - scroll, _minZoom, _maxZoom);

        _cinemachineFollow.FollowOffset = newOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Obstacle") && !_isKnockBack)
        {
            _isKnockBack = true;
            Knockback();
        }

        if (other.transform.CompareTag("Respawn"))
        {
            Respawn();
        }

        if (other.transform.CompareTag("Jump"))
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(Vector3.up * _jumpForce * 1.5f, ForceMode.Impulse);
            _animator.SetTrigger(Define.OnJump);
            _isGround = false;
        }

        if (other.transform.CompareTag("Goal"))
        {
            GameMgr.Instance.IsCountTime = false;
            APIMgr.Instance.RankUpdate();
        }
    }
}