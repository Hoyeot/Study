namespace Global
{
    #region Scene
    public enum SceneIndex
    {
        NONE = -1,
        START = 0,
        START_LOADING = 1,
        LOBBY = 2,
        GAME_LOADING = 3,
        GAME = 4,
    }
    #endregion

    #region File & Resource
    // 리소스 대분류
    public enum RSC_TYPE
    { 
        NONE,

        FRUIT,

        MAX,
    }

    public enum RSC_UNIT_TYPE
    {
        NONE,

        Fruit,

        MAX,
    }
    #endregion
}