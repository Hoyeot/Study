using System.ComponentModel;

namespace Global
{
    public enum ResourceEnum
    {
        RSC_NONE = 0,

        #region Fruit
        /*
        체리 -> 딸기 -> 망고 -> 오렌지 -> 배 -> 복숭아 -> 사과 -> 용과 -> 포도 -> 파인애플 -> 수박
        */
        FRUIT_NONE = 100,

        [Description("체리")] FRUIT_CHERRY,
        [Description("딸기")] FRUIT_STRAWBERRY,
        [Description("망고")] FRUIT_MANGO,
        [Description("오렌지")] FRUIT_ORANGE,
        [Description("배")] FRUIT_PEAR,
        [Description("복숭아")] FRUIT_PEACH,
        [Description("사과")] FRUIT_APPLE,
        [Description("용과")] FRUIT_DRAGONFRUIT,
        [Description("포도")] FRUIT_GRAPE,
        [Description("파인애플")] FRUIT_PINEAPPLE,
        [Description("수박")] FRUIT_WATERMELLON,

        FRUIT_MAX = 199,
        #endregion

        RSC_MAX,
    }
}