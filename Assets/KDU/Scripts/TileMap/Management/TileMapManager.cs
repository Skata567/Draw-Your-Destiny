using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 타일맵 관리 싱글톤
public class TileMapManager : Singleton<TileMapManager>
{
    [Header("Tilemap Layers")]
    public Tilemap groundTilemap;      // 평지 (배치 가능)
    public Tilemap terrainTilemap;     // 지형 (산, 강 등 배치 불가)
    public Tilemap buildingTilemap;    // 건물 배치용

    [Header("Building Management")]
    private Dictionary<Vector3Int, BuildingType> buildingMap = new Dictionary<Vector3Int, BuildingType>();
    private Dictionary<Vector3Int, GameObject> buildingObjects = new Dictionary<Vector3Int, GameObject>();
    private Transform buildingContainer;

    // 하위 호환성 (기존 코드용)
    public Tilemap tilemap => groundTilemap;

    protected override void Awake()
    {
        base.Awake();

        GameObject container = new GameObject("Buildings");
        container.transform.SetParent(transform);
        buildingContainer = container.transform;
    }

    // 타일 타입 확인 (Tilemap 기반)
    public TileType GetTileType(Vector3Int position)
    {
        // 지형 타일맵 확인 (우선순위 높음)
        if (terrainTilemap != null && terrainTilemap.HasTile(position))
        {
            // 타일 이름이나 태그로 타입 구분 가능
            // 예: "Mountain", "River" 등
            return TileType.Mountain; // 임시: 모든 지형을 Mountain으로
        }

        // 평지 타일맵 확인
        if (groundTilemap != null && groundTilemap.HasTile(position))
        {
            return TileType.Plain;
        }

        // 타일이 없으면 배치 불가
        return TileType.Mountain; // 또는 별도의 None 타입
    }

    // 위치가 유효한지 확인 (타일이 존재하는지)
    public bool IsValidPosition(Vector3Int position)
    {
        // groundTilemap에 타일이 있으면 유효
        return groundTilemap != null && groundTilemap.HasTile(position);
    }

    // 건물 배치 가능 확인
    public bool CanPlace(Vector3Int clickPos, BuildingData building)
    {
        Vector3Int origin = GetOrigin(clickPos, building);

        for (int x = 0; x < building.width; x++)
        {
            for (int y = 0; y < building.height; y++)
            {
                Vector3Int checkPos = origin + new Vector3Int(x, y, 0);

                // 유효한 위치인지 확인
                if (!IsValidPosition(checkPos))
                    return false;

                // 지형 타일이 있으면 배치 불가
                if (terrainTilemap != null && terrainTilemap.HasTile(checkPos))
                    return false;

                // 이미 건물이 있으면 배치 불가
                if (buildingMap.ContainsKey(checkPos))
                    return false;

                // 타일 타입 확인
                TileType tileType = GetTileType(checkPos);
                bool allowed = false;
                foreach (TileType t in building.allowedTiles)
                {
                    if (tileType == t)
                    {
                        allowed = true;
                        break;
                    }
                }
                if (!allowed)
                    return false;
            }
        }

        return true;
    }

    // 건물 배치
    public void PlaceBuilding(Vector3Int clickPos, BuildingData building)
    {
        if (!CanPlace(clickPos, building))
            return;

        Vector3Int origin = GetOrigin(clickPos, building);

        for (int x = 0; x < building.width; x++)
        {
            for (int y = 0; y < building.height; y++)
            {
                Vector3Int pos = origin + new Vector3Int(x, y, 0);
                buildingMap[pos] = building.buildingType;
            }
        }

        CreateBuildingVisual(origin, building);

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.SpendGold(building.goldCost);
            gameManager.IncreasePopulationCap(building.populationCapBonus);
        }
    }

    // 건물 시각화 생성
    private void CreateBuildingVisual(Vector3Int origin, BuildingData building)
    {
        GameObject buildingObj = new GameObject($"{building.buildingName}_{origin.x}_{origin.y}");
        buildingObj.transform.SetParent(buildingContainer);

        SpriteRenderer spriteRenderer = buildingObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = building.sprite;
        spriteRenderer.sortingOrder = 1;

        float centerX = origin.x + (building.width - 1) * 0.5f;
        float centerY = origin.y + (building.height - 1) * 0.5f;
        Vector3 worldPos = groundTilemap.CellToWorld(new Vector3Int(Mathf.RoundToInt(centerX), Mathf.RoundToInt(centerY), 0));

        buildingObj.transform.position = worldPos + groundTilemap.cellSize * 0.5f;
        buildingObj.transform.localScale = new Vector3(building.width, building.height, 1);

        buildingObjects[origin] = buildingObj;
    }

    // 건물 기준점 계산 (StarCraft 방식)
    public Vector3Int GetOrigin(Vector3Int clickPos, BuildingData building)
    {
        int offsetX = building.width % 2 == 0
            ? building.width / 2 - 1
            : building.width / 2;

        int offsetY = building.height % 2 == 0
            ? building.height / 2 - 1
            : building.height / 2;

        return clickPos - new Vector3Int(offsetX, offsetY, 0);
    }

    // 건물이 차지하는 타일 목록
    public List<Vector3Int> GetBuildingFootprint(Vector3Int clickPos, BuildingData building)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        Vector3Int origin = GetOrigin(clickPos, building);

        for (int x = 0; x < building.width; x++)
        {
            for (int y = 0; y < building.height; y++)
            {
                positions.Add(origin + new Vector3Int(x, y, 0));
            }
        }

        return positions;
    }

    // 건물 제거
    public void RemoveBuilding(Vector3Int position)
    {
        if (!buildingMap.ContainsKey(position))
            return;

        if (buildingObjects.ContainsKey(position))
        {
            Destroy(buildingObjects[position]);
            buildingObjects.Remove(position);
        }

        buildingMap.Remove(position);
    }
}
