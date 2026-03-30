using UnityEngine;
using UnityEngine.Tilemaps;

// ============================================================
// LordCastle — 영주성 오브젝트에 붙는 컴포넌트
//
// 씬에 미리 만들어두고, EnemyA/B/C의 Inspector에서
// [영주성] 슬롯에 이 오브젝트를 드래그해서 연결.
//
// EnemyOrigin.Start()에서:
//   1. 스프라이트 설정 (lordCastleSprite)
//   2. 적 영지 중앙 타일 위치로 이동
//   3. 체력 초기화
// ============================================================
public class LordCastle : MonoBehaviour
{
    [Header("체력")]
    public int maxHP = 100;
    public int currentHP;

    [Header("크기 (타일 단위)")]
    [Tooltip("4×4 타일 크기. 기본값 변경 불필요.")]
    public int tileSize = 4;

    // 스프라이트 렌더러 — Awake에서 자동으로 찾음
    private SpriteRenderer spriteRenderer;

    // 성이 살아있는지 여부
    public bool IsAlive => currentHP > 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // EnemyOrigin에서 게임 시작 시 호출
    // sprite: EnemyA/B/C에서 Inspector로 설정한 스프라이트
    // worldPos: 적 영지 중앙 타일의 월드 좌표
    public void Initialize(Sprite sprite, Vector3 worldPos, int hp)
    {
        maxHP = hp;
        currentHP = hp;

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;

        transform.position = worldPos;

        // 4×4 타일 크기에 맞게 스케일 설정 (1타일 = 1유닛 기준)
        transform.localScale = new Vector3(tileSize, tileSize, 1f);
    }

    // 시작 도시 영역 bounds를 받아 영주성 4x4의 정확한 중심 위치를 계산
    // tileSize만 바꾸면 20x20 + 4x4, 8x8 + 2x2 모두 같은 방식으로 처리 가능
    public void Initialize(Sprite sprite, Tilemap targetTilemap, BoundsInt territoryBounds, int hp)
    {
        Vector3 worldPos = GetCenteredWorldPosition(targetTilemap, territoryBounds);
        Initialize(sprite, worldPos, hp);
    }

    // 짝수 크기 영지 안에서도 4x4가 정확히 가운데 오도록 월드 좌표 계산
    // 예:
    //   영지가 20x20이고 tileSize가 4면 4x4 중심 배치
    //   영지가 8x8이고 tileSize가 2면 2x2 중심 배치
    private Vector3 GetCenteredWorldPosition(Tilemap targetTilemap, BoundsInt territoryBounds)
    {
        if (targetTilemap == null)
            return transform.position;

        int originX = territoryBounds.xMin + Mathf.Max(0, territoryBounds.size.x - tileSize) / 2;
        int originY = territoryBounds.yMin + Mathf.Max(0, territoryBounds.size.y - tileSize) / 2;
        Vector3Int castleOrigin = new Vector3Int(originX, originY, 0);

        // 좌하단 시작 타일에서 4x4 절반 크기만큼 이동하면 영주성 중심이 된다
        // 시작 타일에서 건물 절반 크기만큼 이동하면 건물 중심이 된다
        // tileSize가 2면 1칸, tileSize가 4면 2칸 이동
        Vector3 originWorld = targetTilemap.CellToWorld(castleOrigin);
        Vector3 centerOffset = Vector3.Scale(targetTilemap.cellSize, new Vector3(tileSize * 0.5f, tileSize * 0.5f, 0f));
        return originWorld + centerOffset;
    }

    // ── 체력 조작 ────────────────────────────────────────────

    // 피해 처리
    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        if (!IsAlive)
            Debug.Log($"[LordCastle] {gameObject.name} 함락!");
    }

    // 체력 회복
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
}
