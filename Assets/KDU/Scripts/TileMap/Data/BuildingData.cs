using UnityEngine;

// 건물 정보 ScriptableObject
[CreateAssetMenu(fileName = "Building_", menuName = "KDU/Building")]
public class BuildingData : ScriptableObject
{
    [Header("기본 정보")]
    public string buildingName;
    public BuildingType buildingType;
    public Sprite sprite;

    [Header("비용")]
    public int goldCost;
    public int populationCost;

    [Header("크기")]
    [Range(1, 4)]
    public int width = 1;
    [Range(1, 4)]
    public int height = 1;

    [Header("배치 제약")]
    public TileType[] allowedTiles;

    [Header("생산 효과")]
    public int goldPerTurn;
    public int researchPerTurn;
    public int populationCapBonus;
}
