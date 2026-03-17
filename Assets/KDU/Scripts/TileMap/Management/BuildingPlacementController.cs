using UnityEngine;

// 건물 배치 입력 처리
public class BuildingPlacementController : MonoBehaviour
{
    [Header("References")]
    private BuildingPlacementService placementService;

    [Header("Test Building (임시 테스트용)")]
    public BuildingData testBuilding;

    [Header("Key Bindings")]
    public KeyCode testModeKey = KeyCode.T;
    public KeyCode cancelKey = KeyCode.Escape;

    private void Awake()
    {
        placementService = GetComponent<BuildingPlacementService>();
    }

    private void Update()
    {
        if (placementService == null)
            return;

        // 테스트 모드 시작
        if (Input.GetKeyDown(testModeKey) && testBuilding != null && !placementService.IsPlacing)
        {
            placementService.StartPlacing(testBuilding);
        }

        if (!placementService.IsPlacing)
            return;

        // 프리뷰 업데이트
        Vector3Int tilePos = placementService.GetMouseTilePos();
        placementService.UpdatePreview(tilePos);

        // 좌클릭 배치
        if (Input.GetMouseButtonDown(0))
        {
            placementService.TryPlaceBuilding(tilePos);
        }

        // 우클릭 또는 ESC로 취소
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(cancelKey))
        {
            placementService.CancelPlacing();
        }
    }
}
