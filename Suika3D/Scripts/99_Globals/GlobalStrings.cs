using System.Collections.Generic;
using UnityEngine;

namespace Global
{
    public class GStrings
    {
        #region Object Path
        public const string G_Prefab = "Prefabs/";
        #endregion

        #region Layer
        public const string G_Ground = "Ground";
        #endregion

        // Fruit
        /*
        체리 -> 딸기 -> 망고 -> 오렌지 -> 배 -> 복숭아 -> 사과 -> 용과 -> 포도 -> 파인애플 -> 수박
        */
        public static readonly Dictionary<ResourceEnum, string> G_ResourceKey_Fruit = new()
        {
            {ResourceEnum.FRUIT_CHERRY, "01_Cherry" },
            {ResourceEnum.FRUIT_STRAWBERRY, "02_Strawberry" },
            {ResourceEnum.FRUIT_MANGO, "03_Mango" },
            {ResourceEnum.FRUIT_ORANGE, "04_Orange" },
            {ResourceEnum.FRUIT_PEAR, "05_Pear" },
            {ResourceEnum.FRUIT_PEACH, "06_Peach" },
            {ResourceEnum.FRUIT_APPLE, "07_Apple" },
            {ResourceEnum.FRUIT_DRAGONFRUIT, "08_Dragonfruit" },
            {ResourceEnum.FRUIT_GRAPE , "09_Grape" },
            {ResourceEnum.FRUIT_PINEAPPLE, "10_Pineapple" },
            {ResourceEnum.FRUIT_WATERMELLON, "11_Watermelon" },
        };
    }
}