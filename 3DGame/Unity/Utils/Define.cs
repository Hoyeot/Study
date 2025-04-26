using UnityEngine;

public class Define
{
    #region PlayerControl
    public const string Horizontal = "Horizontal";
    public const string Vertical = "Vertical";
    public const string MouseX = "Mouse X";
    public const string MouseY = "Mouse Y";
    #endregion

    #region Animation
    public readonly static int Speed = Animator.StringToHash("Speed");
    public readonly static int OnJump = Animator.StringToHash("Jump");
    public readonly static int IsGround = Animator.StringToHash("isGround");
    public readonly static int OnHit = Animator.StringToHash("Hit");
    #endregion

    #region Server
    public const string Host = "127.0.0.1";
    public const int Port = 8080;
    #endregion

    #region Database
    public const string LoginUrl = "http://localhost:8000/api/user/login";
    public const string RegistUrl = "http://localhost:8000/api/user/regist";
    public const string RankUrl = "http://localhost:8000/api/rank/all";
    public const string MyRankUrl = "http://localhost:8000/api/rank";
    public const string RankUpdate = "http://localhost:8000/api/rank/update";
    #endregion

    #region Message
    public const string Join = "/join";
    public const string Channel = "/channel";
    public const string Chat = "/chat";
    public const string Move = "/move";
    public const string Exit = "/exit";
    public const string Count = "/count";
    public const string Init = "/init";
    public const string Login = "/login";
    public const string Jump = "/jump";
    #endregion

    #region Prefabs
    public const string LocalPlayerPrefabPath = "Prefabs/LocalPlayer";
    public const string NetworkPlayerPrefabPath = "Prefabs/NetworkPlayer";
    #endregion

    #region Channel
    public enum ChannelType
    {
        Exit = -1,
        Lobby,
        GameChannel1,
        GameChannel2,
        GameChannel3,
    }
    #endregion
}