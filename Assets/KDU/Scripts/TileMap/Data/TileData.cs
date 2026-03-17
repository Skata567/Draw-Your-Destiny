using UnityEngine;

// 타일 정보 데이터
[System.Serializable]
public class TileData
{
    public TileType type;
    public FogState fogState;
    public int ownerCivID;  // -1 = 미점령
    public BuildingType building;

    public TileData(TileType tileType = TileType.Plain, FogState fog = FogState.Hidden, int owner = -1)
    {
        type = tileType;
        fogState = fog;
        ownerCivID = owner;
        building = BuildingType.None;
    }
}
