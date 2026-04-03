using UnityEngine;
using UnityEngine.Tilemaps;

// ============================================================
// PlayerLordCastle — 플레이어 영주성 오브젝트에 붙는 컴포넌트
//
// 씬에 빈 GameObject(PlayerCastle)를 만들고 이 컴포넌트와
// SpriteRenderer를 추가한 뒤, GameManager의 playerLordCastle
// 슬롯에 드래그해서 연결.
//
// GameManager.Start()에서:
//   1. 스프라이트 설정 (lordCastleSprite)
//   2. 플레이어 영지 중앙 타일 위치로 이동
// ============================================================
public class PlayerLordCastle : MonoBehaviour
{
    [Header("스프라이트")]
    [Tooltip("플레이어 영주성 스프라이트 — Inspector에서 설정")]
    public Sprite lordCastleSprite;

    [Header("크기 (타일 단위)")]
    [Tooltip("4×4 타일 크기. 기본값 변경 불필요.")]
    public int tileSize = 4;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // GameManager에서 게임 시작 시 호출
    public void Initialize(Tilemap targetTilemap, BoundsInt territoryBounds)
    {
        Vector3 worldPos = GetCenteredWorldPosition(targetTilemap, territoryBounds);

        if (spriteRenderer != null && lordCastleSprite != null)
            spriteRenderer.sprite = lordCastleSprite;

        transform.position = worldPos;
        transform.localScale = new Vector3(tileSize, tileSize, 1f);
    }

    // 영지 bounds 안에서 4x4가 정확히 가운데 오도록 월드 좌표 계산
    private Vector3 GetCenteredWorldPosition(Tilemap targetTilemap, BoundsInt territoryBounds)
    {
        if (targetTilemap == null)
            return transform.position;

        int originX = territoryBounds.xMin + Mathf.Max(0, territoryBounds.size.x - tileSize) / 2;
        int originY = territoryBounds.yMin + Mathf.Max(0, territoryBounds.size.y - tileSize) / 2;
        Vector3Int castleOrigin = new Vector3Int(originX, originY, 0);

        Vector3 originWorld = targetTilemap.CellToWorld(castleOrigin);
        Vector3 centerOffset = Vector3.Scale(targetTilemap.cellSize, new Vector3(tileSize * 0.5f, tileSize * 0.5f, 0f));
        return originWorld + centerOffset;
    }
}
