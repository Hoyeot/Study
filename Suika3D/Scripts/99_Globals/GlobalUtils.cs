using UnityEngine;

namespace Global
{
    public class GlobalUtils
    {
        #region Load Resources
        public static T LoadObject<T>(ResourceEnum _rscenum) where T : MonoBehaviour
        {
            return ObjectPoolManager.GetInstance().GetObject<T>(_rscenum);
        }

        public static GameObject LoadObject(ResourceEnum _rscenum)
        {
            return ObjectPoolManager.GetInstance().GetObject(_rscenum);
        }
        #endregion
    }
}