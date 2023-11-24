using System.Collections.Generic;

[System.Serializable]
public class Database
{
    [System.Serializable]
    public class Terrain
    {
        public const int preSpawn = 10;
        public const int maxPrespawn = 50;
        public const float startXPos = 0;
        public const float playZone = 9;

    }

    [System.Serializable]
    public class PlayerStat
    {
        public const float startPosX = 0, startPosY = 0;
        public const float movePosX = 0.5f, movePosY = 0.25f;
    }
}
