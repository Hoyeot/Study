using UnityEngine;

public class BossCtrl : CharacterCtrl
{
    private float _moveSpeed = 4f;
    private float _traceDistance = 10f;
    private float _attackDistance = 3f;
    private CapsuleCollider2D _capsuleCollider2D;

    private enum State
    {
        IDLE,
        TRACE,
        ATTACK
    }

    [SerializeField]
    private State _state;

    protected override void Initialize()
    {
        base.Initialize();

        _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        _rigidbody2D.mass = 1;
        _state = State.IDLE;
        MaxHp = 250;
    }

    private void FixedUpdate()
    {
        if (currentHp <= 0) return;
        MonsterBehaviour();

        if (lastPos != transform.position && _state == State.TRACE)
        {
            ClientMgr.Instance.SendMessageToServer($"{(int)Define.MessageType.MonsterMove}|{CharactedId}|{transform.position.x},{transform.position.y}");
            lastPos = transform.position;
        }
    }

    private void Update()
    {
        HpUpdate();
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        PlayerCtrl _player = ObjectMgr.Instance.Player;

        if (_player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance <= _traceDistance)
            {
                _state = State.TRACE;
                return;
            }
        }

        foreach (var player in ObjectMgr.Instance.players.Values)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= _traceDistance && _state == State.IDLE)
            {
                _state = State.TRACE;
                return;
            }
        }
    }

    private void MonsterBehaviour()
    {
        if (_state == State.IDLE)
        {
            _rigidbody2D.linearVelocity = Vector3.zero;
            return;
        }

        PlayerCtrl targetPlayer = FindNearestPlayer();

        if (targetPlayer == null)
        {
            _state = State.IDLE;
            _rigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        float currentDistance = Vector3.Distance(transform.position, targetPlayer.transform.position);

        switch (_state)
        {
            case State.TRACE:
                if (currentDistance <= _attackDistance)
                {
                    _state = State.ATTACK;
                    _animator.SetTrigger(Define.onAttack);
                    _rigidbody2D.linearVelocity = Vector2.zero;
                }
                else
                {
                    _rigidbody2D.linearVelocity = new Vector2(direction.x * _moveSpeed, direction.y);
                }
                break;

            case State.IDLE:
                _rigidbody2D.linearVelocity = Vector2.zero;
                break;
        }
        targetPlayer = null;
    }

    private PlayerCtrl FindNearestPlayer()
    {
        PlayerCtrl nearestPlayer = null;
        float minDistance = 10f;

        PlayerCtrl localPlayer = ObjectMgr.Instance.Player;
        if (localPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, localPlayer.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlayer = localPlayer;
            }
        }

        foreach (var player in ObjectMgr.Instance.players.Values)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlayer = player;
            }
        }

        return nearestPlayer;
    }

    public void MonsterDeath()
    {
        _capsuleCollider2D.enabled = false;
        StartCoroutine(DeathAction());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Transform _weapon = collision.transform.Find("Weapon");
        if (_weapon == null) return;
        Collider2D _weaponCollider2D = _weapon.GetComponent<Collider2D>();
        if (_weaponCollider2D != null && _weaponCollider2D.enabled)
        {
            ClientMgr.Instance.SendMessageToServer($"{(int)Define.MessageType.MonsterHit}|{CharactedId}|10");
        }
    }
}