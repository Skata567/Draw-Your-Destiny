using UnityEngine;

// 건물 배치 프리뷰 렌더링
[RequireComponent(typeof(SpriteRenderer))]
public class BuildingPreview : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BuildingData currentBuilding;
    private bool isActive = false;

    [Header("Preview Colors")]
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = 10;
        HidePreview();
    }

    // 프리뷰 표시
    public void ShowPreview(BuildingData building)
    {
        if (building == null)
            return;

        currentBuilding = building;
        spriteRenderer.sprite = building.sprite;
        isActive = true;
        spriteRenderer.enabled = true;

        transform.localScale = new Vector3(building.width, building.height, 1);
    }

    // 프리뷰 숨김
    public void HidePreview()
    {
        isActive = false;
        spriteRenderer.enabled = false;
        currentBuilding = null;
    }

    // 프리뷰 위치 및 색상 업데이트
    public void UpdatePreview(Vector3 position, bool isValid)
    {
        if (!isActive)
            return;

        transform.position = position;
        spriteRenderer.color = isValid ? validColor : invalidColor;
    }
}
