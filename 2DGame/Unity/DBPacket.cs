public class DBPacket : Singleton<DBPacket>
{
    public int status_code;
    public string userId;
    public int useFlag;
    public string newUserId;
    public string UserID { get { return userId; } set { userId = value; } }
    public string NewUserID { get { return newUserId; } set { newUserId = value; } }
}