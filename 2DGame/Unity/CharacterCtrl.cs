using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class CharacterCtrl : BaseCtrl
{
    [SerializeField]
    protected string characterId;
    public string CharactedId { get { return characterId; } set { characterId = value; } } // ID

    [SerializeField]
    protected float currentHp;
    public float CurrentHp { get { return currentHp; } set { currentHp = value; } } // HP

    protected Vector3 pos;
    public Vector3 Pos { get { return pos; } set { pos = value; } }

    protected float speed;
    public float Speed
    {
        get { return _animator.GetFloat(Define.Speed); }
        set { _animator.SetFloat(Define.Speed, value); }
    }

    protected bool isJump;

    public bool IsJump
    {
        get { return _animator.GetBool(Define.isJump); }
        set { _animator.SetBool(Define.isJump, value); }
    }

    protected bool isGround;

    public bool IsGround
    {
        get { return _animator.GetBool(Define.isGround); }
        set { _animator.SetBool(Define.isGround, value); }
    }

    protected float maxHp;
    public float MaxHp { get { return maxHp; } set { maxHp = value; } }

    protected Vector3 lastPos = Vector3.zero;
    public Vector3 LastPos { get { return lastPos; } set { lastPos = value; } }

    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;
    protected Rigidbody2D _rigidbody2D;
    protected Transform _hpBarTransform;
    protected Image _hpBar;

    protected override void Initialize()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _hpBarTransform = transform.Find("Canvas/HpBar - Background/HpBar");
        _hpBar = _hpBarTransform.GetComponent<Image>();

        _rigidbody2D.freezeRotation = true;
        _rigidbody2D.linearVelocity = Vector3.zero;
    }

    public virtual void Move(string userId, Vector3 position, float moveSpeed)
    {
        if (lastPos != position)
        {
            transform.Translate(Vector2.right * position * moveSpeed * Time.deltaTime);
            _spriteRenderer.flipX = position.x < 0;
            lastPos = transform.position;
            Speed = Mathf.Abs(moveSpeed);
            ClientMgr.Instance.SendMessageToServer($"{(int)Define.MessageType.PlayerMove}|{characterId}|{currentHp}|{transform.position.x},{transform.position.y},true,{moveSpeed},{_spriteRenderer.flipX}");
        }
        else
        {
            ClientMgr.Instance.SendMessageToServer($"{(int)Define.MessageType.PlayerMove}|{characterId}|{currentHp}|{transform.position.x},{transform.position.y},true,{moveSpeed},{_spriteRenderer.flipX}");
        }
    }

    public virtual void Move(string userId, Vector3 position, Vector3 velocity, float moveSpeed, bool isflip)
    {
        if (lastPos != position)
        {
            ObjectMgr.Instance.players[userId].Speed = Mathf.Abs(moveSpeed);
            ObjectMgr.Instance.players[userId]._spriteRenderer.flipX = isflip;
            ObjectMgr.Instance.players[userId].transform.position = position;
        }
    }

    public virtual void MonsterMove(string monsterId, Vector3 pos)
    {
        if (lastPos != pos)
        {
            ObjectMgr.Instance.monsters[monsterId].transform.position = pos;
        }
    }

    public virtual void Jump()
    {
        if (!IsJump && IsGround)
        {
            IsJump = true;
            IsGround = false;
            _animator.SetTrigger(Define.onJump);
            _rigidbody2D.AddForce(Vector2.up * 700, ForceMode2D.Impulse);
        }
    }

    public virtual void Jump(string userId)
    {
        if (!IsJump && IsGround)
        {
            ObjectMgr.Instance.players[userId].IsJump = true;
            ObjectMgr.Instance.players[userId].IsGround = false;
            ObjectMgr.Instance.players[userId]._animator.SetTrigger(Define.onJump);
            ObjectMgr.Instance.players[userId]._rigidbody2D.AddForce(Vector2.up * 700, ForceMode2D.Impulse);
            ClientMgr.Instance.SendMessageToServer($"{(int)Define.MessageType.PlayeJump}|{CharactedId}");
        }
    }

    public virtual void HpUpdate()
    {
        _hpBar.fillAmount = CurrentHp / maxHp;
    }

    public virtual void Attack()
    {
        _animator.SetTrigger(Define.onAttack);
        ClientMgr.Instance.SendMessageToServer($"{(int)Define.MessageType.Attack}|{CharactedId}");
    }

    public virtual void Attack(string userId)
    {
        ObjectMgr.Instance.players[userId]._animator.SetTrigger(Define.onAttack);
    }

    public IEnumerator DeathAction()
    {
        _animator.SetTrigger(Define.onMonsterDeath);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}