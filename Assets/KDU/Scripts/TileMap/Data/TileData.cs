using UnityEngine;

// ============================================================
// TileData — 타일 1칸의 런타임 상태를 저장하는 데이터 클래스
//
// TileMapManager.tileDataMap(Dictionary<Vector3Int, TileData>)에
// 타일 좌표를 키로 저장되어 있다.
// 씬 시작 시 모든 지형 Tilemap을 순회해 자동 생성됨.
// ============================================================
[System.Serializable]
public class TileData
{
    // 타일 지형 종류 (Plain, River, Farmland, Resource, City)
    // Outpost 건설 시 Plain → City 로 런타임에 변경될 수 있음
    public TileType type;

    // 현재 안개 상태 — FogManager가 직접 관리하므로 여기선 참조용
    public FogState fogState;

    // 이 타일을 점령한 문명 ID
    // -1 = 미점령 / 0 = 플레이어 / 1~3 = AI
    public int ownerCivID;

    // 이 타일 위에 있는 건물 타입 (None = 건물 없음)
    // BuildingInstance와 중복 추적되지만 빠른 타입 조회용으로 유지
    public BuildingType building;

    // 기본값: Plain, Explored(지형 보임), 미점령, 건물 없음
    public TileData(TileType tileType = TileType.Plain, FogState fog = FogState.Explored, int owner = -1)
    {
        type       = tileType;
        fogState   = fog;
        ownerCivID = owner;
        building   = BuildingType.None;
    }
}
