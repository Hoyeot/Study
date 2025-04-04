using UnityEngine;
using TMPro;

public class PlayerCtrl : CharacterCtrl
{
    private bl_Joystick _js;
    private Transform _transform;
    private Collider2D _weaponCollder;
    private TMP_Text _playerName;

    private bool _isStop;
    private bool _isAttack;
    private readonly bool _isJump;
    private readonly bool _isGround;

    private float _moveSpeed = 8f;

    protected override void Initialize()
    {
        base.Initialize();

        _js = GameObject.Find("UI/Joystick").GetComponent<bl_Joystick>();
        _transform = transform.Find("Weapon");
        _weaponCollder = _transform.GetComponent<Collider2D>();
        _playerName = GetComponentInChildren<TMP_Text>();

        _rigidbody2D.mass = 100;
        MaxHp = 100;
    }

    private void FixedUpdate()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName(Define.Idle))
        {
            _isAttack = false;
            _weaponCollder.enabled = false;
        }

        if (characterId == ObjectMgr.Instance.LocalId)
        {
            PlayerBehaviour();
        }
    }

    private void Update()
    {
        HpUpdate();
    }

    private void PlayerBehaviour()
    {
        if (_isAttack) return;

        Vector3 dir = new Vector3(_js.Horizontal, 0, 0);
        dir.Normalize();

        if (dir != Vector3.zero)
        {
            Move(CharactedId, dir, _moveSpeed);
            _isStop = false;
        }
        else
        {
            Speed = 0;
            if (!_isStop)
            {
                Move(CharactedId, transform.position, 0);
                _isStop = true;
            }
        }

        if (_spriteRenderer.flipX)
        {
            _transform.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            _transform.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void DisplayId(string userId)
    {
        _playerName.text = userId;
    }

    public void PlayerAttack()
    {
        Attack();
        _weaponCollder.enabled = true;
        _isAttack = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag(Define.Ground))
        {
            IsGround = true;
            IsJump = false;
        }

        if (collision.transform.CompareTag(Define.Wall))
        {
            _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocity.y);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag(Define.Wall))
        {
            _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocity.y);
        }
    }
}