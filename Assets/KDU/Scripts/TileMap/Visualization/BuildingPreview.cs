using UnityEngine;

// ============================================================
// BuildingPreview — 건물 배치 모드 중 마우스를 따라다니는 반투명 프리뷰
//
// BuildingPlacementService가 매 프레임 UpdatePreview()를 호출해
// 위치와 색상(배치 가능=초록, 불가=빨강)을 갱신한다.
//
// 씬 계층 구조:
//   BuildingPlacementController (GameObject)
//   └─ BuildingPreview (Child GameObject, 이 컴포넌트 + SpriteRenderer)
// ============================================================
[RequireComponent(typeof(SpriteRenderer))]
public class BuildingPreview : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BuildingData currentBuilding;
    private bool isActive = false;

    [Header("Preview Colors")]
    public Color validColor   = new Color(0, 1, 0, 0.5f);  // 배치 가능 — 초록 반투명
    public Color invalidColor = new Color(1, 0, 0, 0.5f);  // 배치 불가 — 빨강 반투명

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = 10; // 모든 타일/건물 위에 표시
        HidePreview();
    }

    // 배치 모드 시작 시 호출 — 건물 스프라이트와 크기를 적용하고 표시
    public void ShowPreview(BuildingData building)
    {
        if (building == null) return;

        currentBuilding = building;
        spriteRenderer.sprite = building.sprite;
        isActive = true;
        spriteRenderer.enabled = true;

        // 크기(타일 단위)를 localScale로 반영 (1타일=1유닛 기준)
        transform.localScale = new Vector3(building.width, building.height, 1);
    }

    // 배치 모드 종료(배치 완료 또는 취소) 시 호출
    public void HidePreview()
    {
        isActive = false;
        spriteRenderer.enabled = false;
        currentBuilding = null;
    }

    // 매 프레임 BuildingPlacementService에서 호출 — 위치와 유효 색상 갱신
    public void UpdatePreview(Vector3 position, bool isValid)
    {
        if (!isActive) return;

        transform.position = position;
        spriteRenderer.color = isValid ? validColor : invalidColor;
    }
}
