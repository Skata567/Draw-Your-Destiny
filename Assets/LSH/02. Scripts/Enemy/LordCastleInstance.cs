using System.Collections.Generic;
using UnityEngine;

// ============================================================
// LordCastleInstance — AI 문명 영주성의 런타임 데이터
//
// 영주성은 각 AI 문명(A/B/C)의 핵심 건물.
// EnemyOrigin.SpawnLordCastle()에서 생성되며
// TileMapManager에 BuildingInstance로 등록되어 안개 시스템과 연동된다.
//
// 파괴 조건: currentHP <= 0
// 함락 처리는 EnemyOrigin 또는 전투 시스템에서 IsAlive를 체크해 처리.
// ============================================================
public class LordCastleInstance
{
    // 이 성을 소유한 문명 ID (1=AI-A, 2=AI-B, 3=AI-C)
    public int ownerCivID;

    // 성의 최대/현재 체력
    public int maxHP;
    public int currentHP;

    // 성 중앙 타일 좌표 (CitySpawnManager가 결정한 도시 중심)
    public Vector3Int centerTilePos;

    // 성이 차지하는 4×4 타일 좌표 목록
    // 충돌 검사, 안개 렌더링에 사용
    public List<Vector3Int> footprint;

    // 씬에 표시되는 스프라이트 오브젝트 (SpriteRenderer 포함)
    // FogManager가 이 오브젝트의 색상을 제어해 보이거나 숨김
    public GameObject visual;

    // 안개 스냅샷 플래그 — Visible 상태에서 한 번이라도 봤으면 true
    // Explored 상태에서도 마지막으로 본 모습 유지 (스타크래프트 방식)
    public bool wasEverSeen = false;

    // 성이 살아있는지 여부
    public bool IsAlive => currentHP > 0;

    // ── 체력 조작 ────────────────────────────────────────────

    // 피해 처리 — 0 미만으로 내려가지 않음
    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        if (!IsAlive)
            Debug.Log($"[LordCastle] 문명 {ownerCivID}의 영주성이 함락됐습니다!");
    }

    // 체력 회복 — maxHP 초과 불가
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
}
