using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Packets
{
    public class user
    {
        public string user_id;
        public string user_pw;
        public string user_name;
        public string user_nickname;
        public string highpoint;
    }

    public class res_get_users
    {
        public int status_code;
        public List<user> users;
    }
}
