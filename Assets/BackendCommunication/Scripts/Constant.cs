namespace Backend
{
    class Constant
    {
        public enum LaneTypeEnum
        {
            grassLane = 1,
            dirtLane = 2,
            bigDirtLane = 3,
            riverLane = 4,
            riverLogLane = 5,
            riverGatorLane = 6
        }

        public const byte PackageType_Message = 0;
        public const byte PackageType_Input = 1;
        public const byte PackageType_ActionResponse = 2;
        public const byte PackageType_TimeEventTrigger = 3;
        public const byte PackageType_ClientConnect = 4;
        public const byte PackageType_ConnectResponse = 5;
        public const byte PacakgeType_ClientReady = 6;
        public const byte PackageType_GameStart = 7;
        public const byte Package_SnapshotRequest = 8;
        public const byte Package_SnapshotResponse = 9;
        public const byte Package_TickTimeRequest = 10;
        public const byte Package_TickTimeResponse = 11;
        public const byte Pacakge_OffScreenReport = 12;
        public const byte Package_GameEndResult = 50;
        public const byte Package_GameEndResultConfirm = 51;
        public const byte Package_ClientPing = 98;
        public const byte Package_ServerPong = 99;

        public const byte Input_None = 0;
        public const byte Input_Forward = 1;
        public const byte Input_Left = 2;
        public const byte Input_Right = 3;
        public const byte Input_Backward = 4;

        public const byte MoveResult_Deny = 0;
        public const byte MoveResult_OK = 1;
        public const byte MoveResult_Drown = 2;
        public const byte MoveResult_Hit = 3;
        public const byte MoveResult_OffscreenSide = 4;
        public const byte MoveResult_OffscreenBack = 5;
        public const byte MoveResult_RIDE_GATOR = 6; 
        public const byte MoveResult_RIDE_LOG = 7; 
        public const byte MoveResult_RIDE_LOTUS = 8; 
        public const byte MoveResult_HIT_GATOR = 9; 
        public const byte MoveResult_HIT_TRAIN = 10;

        public const byte TimeEvent_None = 0;
        public const byte TimeEvent_AnimalHit = 1;
        public const byte TimeEvent_RideLogOffScreen = 2;
        public const byte TimeEvent_RideGatorOffScreen = 3;

        public const int CoinSpawnPeriod = 5;
        public const double CoinSpawnPercentage = 0.1f;

        public const int LevelLeft = -9;
        public const int LevelRight = 17;
        public const int LeftMostLaneIndex = 0;
        public const int RightMostLaneIndex = 8;

        public const int PreStartZoneLaneCount = 4;
        public const int StartZoneLaneCount = 2;

        public const double LaneSpeed_Min = 0.75f;
        public const double LaneSpeed_Max = 1.0f;
        public const double TrainSpeed_Min = 1.0f;
        public const double TrainSpeed_Max = 1.5f;

        public const int MapGen_FixNoRiverUntil = 50;
        public const int MapGen_MinConnectedPoint = 2;
    }
}
