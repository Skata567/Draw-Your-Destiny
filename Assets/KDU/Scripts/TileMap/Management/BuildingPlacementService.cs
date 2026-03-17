using UnityEngine;

// 건물 배치
public class BuildingPlacementService : MonoBehaviour
{
    private TileMapManager tileMapManager;
    private BuildingPreview buildingPreview;
    private Camera mainCamera;

    private BuildingData currentBuilding;
    private bool isPlacing = false;

    private Vector3Int lastTilePos = Vector3Int.zero;
    private bool lastCanPlace = false;

    public bool IsPlacing => isPlacing;

    private void Awake()
    {
        buildingPreview = GetComponentInChildren<BuildingPreview>();
    }

    private void Start()
    {
        tileMapManager = TileMapManager.Instance;
        mainCamera = Camera.main;

        if (buildingPreview != null)
            buildingPreview.HidePreview();
    }

    // 배치 모드 시작
    public void StartPlacing(BuildingData data)
    {
        if (data == null || tileMapManager == null)
            return;

        currentBuilding = data;
        isPlacing = true;

        // 캐시 초기화
        lastTilePos = Vector3Int.zero;
        lastCanPlace = false;

        if (buildingPreview != null)
            buildingPreview.ShowPreview(currentBuilding);
    }

    // 배치 모드 취소
    public void CancelPlacing()
    {
        isPlacing = false;
        currentBuilding = null;

        lastTilePos = Vector3Int.zero;
        lastCanPlace = false;

        if (buildingPreview != null)
            buildingPreview.HidePreview();
    }

    // 프리뷰 업데이트
    public void UpdatePreview(Vector3Int tilePos)
    {
        if (!isPlacing || buildingPreview == null || currentBuilding == null || tileMapManager == null || tileMapManager.groundTilemap == null)
            return;

        Vector3Int origin = tileMapManager.GetOrigin(tilePos, currentBuilding);

        float centerX = origin.x + (currentBuilding.width - 1) * 0.5f;
        float centerY = origin.y + (currentBuilding.height - 1) * 0.5f;
        Vector3 worldPos = tileMapManager.groundTilemap.CellToWorld(
            new Vector3Int(Mathf.RoundToInt(centerX), Mathf.RoundToInt(centerY), 0));

        // 타일 중심으로 이동
        worldPos += tileMapManager.groundTilemap.cellSize * 0.5f;

        // 타일 좌표가 바뀔 때만 CanPlace 재검사
        bool canPlace;
        if (tilePos != lastTilePos)
        {
            canPlace = tileMapManager.CanPlace(tilePos, currentBuilding);
            lastTilePos = tilePos;
            lastCanPlace = canPlace;
        }
        else
        {
            canPlace = lastCanPlace;
        }

        buildingPreview.UpdatePreview(worldPos, canPlace);
    }

    // 건물 배치 시도
    public bool TryPlaceBuilding(Vector3Int tilePos)
    {
        if (!isPlacing || currentBuilding == null || tileMapManager == null)
            return false;

        bool canPlace = (tilePos == lastTilePos) ? lastCanPlace : tileMapManager.CanPlace(tilePos, currentBuilding);

        if (canPlace)
        {
            tileMapManager.PlaceBuilding(tilePos, currentBuilding);
            CancelPlacing();
            return true;
        }

        return false;
    }

    // 마우스 위치를 타일 좌표로 변환
    public Vector3Int GetMouseTilePos()
    {
        if (mainCamera == null || tileMapManager == null || tileMapManager.groundTilemap == null)
            return Vector3Int.zero;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        return tileMapManager.groundTilemap.WorldToCell(mouseWorld);
    }
}
