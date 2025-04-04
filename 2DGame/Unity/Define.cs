using UnityEngine;

public class Define
{
    public readonly static int onJump = Animator.StringToHash("onJump");
    public readonly static int onAttack = Animator.StringToHash("onAttack");
    public readonly static int Speed = Animator.StringToHash("Speed");
    public readonly static int isJump = Animator.StringToHash("isJump");
    public readonly static int isGround = Animator.StringToHash("isGround");
    public readonly static int onMonsterHit = Animator.StringToHash("onMonsterHit");
    public readonly static int onMonsterDeath = Animator.StringToHash("onMonsterDeath");

    public const string Idle = "Player_Idle";

    public const string Ground = "Ground";
    public const string Wall = "Wall";
    public const string SpawnPos = "SpawnPos";
    public const string Player = "Player";
    public const string Monster = "Monster";

    public const string PlayerPath = "Prefabs/Player";
    public const string MonsterPath = "Prefabs/Slime(Green)";
    public const string BossPath = "Prefabs/Boss";

    public const string LocalHost = "127.0.0.1";
    public const int Port = 8080;
    public enum MessageType
    { 
        Join,
        Leave,
        PlayerMove,
        PlayerCheck,
        PlayeJump,
        Chatting,
        Attack,
        Retry,
        MonsterSpawn = 10,
        MonsterDeath,
        MonsterMove,
        MonsterPos,
        MonsterHit = 17,
        BossSpawn = 20,
        BossDeath
    }

    public const string GetHost = "http://localhost:3000/users/getUsers";
    public const string CheckHost = "http://localhost:3000/users/chkUsers";
    public const string InsertHost = "http://localhost:3000/users/insertUser";
    public const string UpdateHost = "http://localhost:3000/users/updateUser";
    public const int Success = 200;
    public const int Fail = 999;
    public const int DuplicateError = 888;

    public enum DBReceived
    {
        Empty,
        LoginFailed,
        JoinSuccess,
        JoinFailed,
        JoinAlready,
        UpdateFailed,
        ConnectFailed = 9
    }
}
