using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ============================================================
// FogManager — 안개 전쟁(Fog of War) 관리 싱글톤
//
// 2단계 안개 상태:
//   Explored — 기본 상태 (초기 전체 맵). 평지/강/농경지 지형은 회색으로 보임.
//              금광/도시 타일은 숨김. 건물은 wasEverSeen = true인 것만 표시.
//   Visible  — 플레이어 시야 안. 풀 컬러, 금광/도시/건물 모두 표시.
//
// 롤/스타크래프트 방식:
//   게임 시작 시 전체 맵의 기본 지형(평지, 강)은 보이지만,
//   금광과 적 도시는 직접 탐험(Visible)하기 전까지 숨겨져 있다.
//
// 렌더링 방식:
//   fogTilemap  — 검정 타일 알파값으로 시야 농도 표현 (Explored=반투명, Visible=투명)
//   기본 지형   — ground/farmland/river: Explored에서 회색 표시
//   특수 지형   — goldMine/city: Visible일 때만 표시
//   건물 visual — SpriteRenderer 알파값으로 표시/숨김
//
// [DefaultExecutionOrder(-10)] — TileMapManager 다음, 다른 시스템보다 먼저 초기화
// ============================================================
[DefaultExecutionOrder(-10)]
public class FogManager : Singleton<FogManager>
{
    // TileMapManager에서 Tilemap 참조를 받아 씬의 안개를 제어
    private TileMapManager tileMapManager;

    // 타일 좌표 → FogState. 모든 타일의 현재 안개 상태를 저장
    private Dictionary<Vector3Int, FogState> fogMap = new Dictionary<Vector3Int, FogState>();

    [Header("디버그")]
    public KeyCode debugToggleKey = KeyCode.F1;  // F1: 안개 토글 (테스트용)
    private bool debugFogDisabled = false;

    // Outpost 건설 시 등록된 영구 시야 타일 집합
    // 이 타일들은 SetExplored()를 호출해도 Visible이 유지됨
    private HashSet<Vector3Int> outpostVisionTiles = new HashSet<Vector3Int>();

    // 한 번이라도 Visible 상태였던 타일 집합 (특수 지형 스냅샷용)
    // Explored 상태에서 이 집합에 있는 타일의 특수 지형(농경지/금광/도시)은 회색으로 유지
    private HashSet<Vector3Int> revealedTiles = new HashSet<Vector3Int>();

    // fogTilemap에 깔 단색 검정 타일 (코드에서 자동 생성, 에셋 불필요)
    private Tile fogTile;

    // ── 안개 레이어 색상 ──────────────────────────────────────
    // fogTilemap의 알파값으로 시야 상태를 표현
    private static readonly Color FogExplored = new Color(0f, 0f, 0f, 0.45f); // 반투명 어두운 오버레이
    private static readonly Color FogVisible  = new Color(0f, 0f, 0f, 0f);    // 완전 투명 (시야 내)

    // ── 지형 타일맵 색상 ──────────────────────────────────────
    private static readonly Color TerrainHidden   = new Color(1f, 1f, 1f, 0f);        // 숨김 (투명)
    private static readonly Color TerrainExplored = new Color(0.6f, 0.6f, 0.6f, 1f); // 회색 (탐험됨)
    private static readonly Color TerrainVisible  = Color.white;                       // 풀 컬러 (시야 내)

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        tileMapManager = TileMapManager.Instance;
        if (tileMapManager == null)
        {
            Debug.LogError("[FogManager] TileMapManager를 찾을 수 없습니다.");
            return;
        }

        InitializeFog();
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugToggleKey))
            ToggleFogDebug();
    }

    // F1: 안개를 완전히 끄거나 원래 상태로 복원 (테스트용)
    public void ToggleFogDebug()
    {
        debugFogDisabled = !debugFogDisabled;

        Tilemap fogTilemap = tileMapManager.fogTilemap;

        foreach (Vector3Int pos in fogMap.Keys)
        {
            if (debugFogDisabled)
            {
                // 안개 완전 투명 + 모든 지형 풀 컬러
                if (fogTilemap != null) fogTilemap.SetColor(pos, FogVisible);
                SetBaseTerrainColor(pos, TerrainVisible);
                SetSpecialTerrainColor(pos, TerrainVisible);
            }
            else
            {
                // 실제 안개 상태로 복원
                UpdateTileVisual(pos);
            }
        }

        Debug.Log($"[FogManager] 디버그 안개 {(debugFogDisabled ? "OFF" : "ON")}");
    }

    // 모든 타일을 Explored로 초기화 (기본 지형 보임, 금광/도시 숨김)
    private void InitializeFog()
    {
        Tilemap fogTilemap = tileMapManager.fogTilemap;
        if (fogTilemap == null)
        {
            Debug.LogError("[FogManager] fogTilemap이 연결되지 않았습니다.");
            return;
        }

        fogTile = ScriptableObject.CreateInstance<Tile>();
        fogTile.sprite = CreateSolidSprite();
        fogTile.color = Color.white;

        foreach (Vector3Int pos in tileMapManager.GetAllTilePositions())
        {
            fogMap[pos] = FogState.Explored;

            // 안개 오버레이 타일 배치 (반투명)
            fogTilemap.SetTile(pos, fogTile);
            fogTilemap.SetTileFlags(pos, TileFlags.None);
            fogTilemap.SetColor(pos, FogExplored);

            // 기본 지형(평지/강/농경지): 회색으로 표시
            SetBaseTerrainColor(pos, TerrainExplored);

            // 금광/도시: 직접 탐험 전까지 숨김
            SetSpecialTerrainColor(pos, TerrainHidden);
        }
    }

    // ── 공개 API ──────────────────────────────────────────────────

    // center 기준 원형 반경 내 타일을 Visible로 전환
    // 사용처: 정찰 카드 사용 시, Outpost 건설 직후, 도시 주변 초기 시야
    public void SetVisible(Vector3Int center, int radius)
    {
        ForEachInRadius(center, radius, pos =>
        {
            fogMap[pos] = FogState.Visible;
            revealedTiles.Add(pos); // 한 번 본 타일로 기록 → 이후 Explored에서도 특수 지형 유지

            // 시야에 들어온 건물 스냅샷 갱신
            BuildingInstance building = tileMapManager.GetBuildingAt(pos);
            if (building != null)
                building.wasEverSeen = true;

            UpdateTileVisual(pos);
        });
    }

    // center 기준 반경 내 Visible 타일을 Explored로 되돌림
    // 사용처: 정찰 유닛이 해당 지역을 벗어날 때
    // Outpost 영구 시야 타일(outpostVisionTiles)은 유지됨
    public void SetExplored(Vector3Int center, int radius)
    {
        ForEachInRadius(center, radius, pos =>
        {
            if (outpostVisionTiles.Contains(pos)) return;

            fogMap[pos] = FogState.Explored;
            UpdateTileVisual(pos);
        });
    }

    // Outpost 건설 시 TileMapManager에서 호출
    // origin(8×8 영역 좌하단) + buildingSize(8) → buildingSize+margin 범위를 영구 Visible 등록
    public void OnOutpostBuilt(Vector3Int origin, int buildingSize)
    {
        int margin = 2;
        for (int dx = -margin; dx < buildingSize + margin; dx++)
        for (int dy = -margin; dy < buildingSize + margin; dy++)
        {
            Vector3Int pos = origin + new Vector3Int(dx, dy, 0);
            if (!fogMap.ContainsKey(pos)) continue;

            outpostVisionTiles.Add(pos);
            fogMap[pos] = FogState.Visible;

            BuildingInstance b = tileMapManager.GetBuildingAt(pos);
            if (b != null) b.wasEverSeen = true;

            UpdateTileVisual(pos);
        }
    }

    // Outpost 철거 시 TileMapManager에서 호출 — 영구 시야 해제
    public void OnOutpostDestroyed(Vector3Int origin, int buildingSize)
    {
        int margin = 2;
        for (int dx = -margin; dx < buildingSize + margin; dx++)
        for (int dy = -margin; dy < buildingSize + margin; dy++)
        {
            Vector3Int pos = origin + new Vector3Int(dx, dy, 0);
            outpostVisionTiles.Remove(pos);

            if (fogMap.ContainsKey(pos))
                fogMap[pos] = FogState.Explored;

            UpdateTileVisual(pos);
        }
    }

    // 건물이 배치된 직후 TileMapManager.PlaceBuilding()에서 호출
    // footprint가 Visible 상태면 wasEverSeen = true → 이후 Explored에서도 보임
    public void OnBuildingPlaced(BuildingInstance building)
    {
        foreach (Vector3Int pos in building.footprint)
        {
            if (fogMap.TryGetValue(pos, out FogState state) && state == FogState.Visible)
                building.wasEverSeen = true;
        }

        RenderBuilding(building);
    }

    // 건물 철거 시 TileMapManager.RemoveBuilding()에서 호출
    // visual 제거는 TileMapManager가 담당. UpdateTileVisual이 재탐험 시 자동 갱신
    public void OnBuildingDestroyed(BuildingInstance building)
    {
        // 현재 별도 처리 없음
    }

    // 특정 타일의 현재 안개 상태 반환 (fogMap에 없으면 Explored)
    public FogState GetFogState(Vector3Int pos)
    {
        return fogMap.TryGetValue(pos, out FogState state) ? state : FogState.Explored;
    }

    // ── 렌더링 ───────────────────────────────────────────────────

    // 단일 타일의 안개 상태에 맞게 모든 시각 요소(fog/지형/건물) 갱신
    // fogMap 변경 후 항상 이 메서드를 호출해야 화면이 동기화됨
    private void UpdateTileVisual(Vector3Int pos)
    {
        FogState state = fogMap.TryGetValue(pos, out FogState s) ? s : FogState.Explored;

        Tilemap fogTilemap = tileMapManager.fogTilemap;

        // 안개 오버레이 (Explored = 반투명, Visible = 투명)
        if (fogTilemap != null)
            fogTilemap.SetColor(pos, state == FogState.Visible ? FogVisible : FogExplored);

        // 기본 지형(평지/강/농경지): Explored에서도 회색으로 표시
        SetBaseTerrainColor(pos, state == FogState.Visible ? TerrainVisible : TerrainExplored);

        // 특수 지형(농경지/금광/도시):
        //   Visible        → 풀 컬러
        //   Explored + 한 번이라도 본 타일 → 회색 (건물 스냅샷처럼 마지막 본 모습 유지)
        //   Explored + 한 번도 못 본 타일  → 숨김 (적 위치 비공개)
        Color specialColor;
        if (state == FogState.Visible)
            specialColor = TerrainVisible;
        else if (revealedTiles.Contains(pos))
            specialColor = TerrainExplored;
        else
            specialColor = TerrainHidden;
        SetSpecialTerrainColor(pos, specialColor);

        // 건물 렌더링 (스냅샷 방식)
        BuildingInstance building = tileMapManager.GetBuildingAt(pos);
        if (building != null)
            RenderBuilding(building);
    }

    // 건물 시각화 (스냅샷 방식)
    // Visible: 항상 표시
    // Explored + wasEverSeen = true: 마지막으로 본 모습 유지
    // Explored + wasEverSeen = false: 숨김 (한 번도 못 본 적 건물, 초기 금광 등)
    private void RenderBuilding(BuildingInstance building)
    {
        if (building?.visual == null) return;

        SpriteRenderer sr = building.visual.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 건물 footprint 중 가장 높은 FogState 기준으로 표시 여부 결정
        FogState highestState = FogState.Explored;
        foreach (Vector3Int pos in building.footprint)
        {
            FogState tileState = fogMap.TryGetValue(pos, out FogState s) ? s : FogState.Explored;
            if (tileState > highestState) highestState = tileState;
        }

        bool show = highestState == FogState.Visible
                 || (highestState == FogState.Explored && building.wasEverSeen);

        sr.color = show ? Color.white : new Color(1f, 1f, 1f, 0f);
    }

    // ── 지형 색상 설정 ────────────────────────────────────────────

    // 기본 지형 (ground/river): Explored에서도 보이는 레이어
    private void SetBaseTerrainColor(Vector3Int pos, Color color)
    {
        SetTilemapColor(tileMapManager.groundTilemap, pos, color);
        SetTilemapColor(tileMapManager.riverTilemap,  pos, color);
    }

    // 특수 지형 (farmland/goldMine/city): Visible일 때만 보이는 레이어
    // 농경지는 도시 주변에만 생성되므로 적 위치가 특정될 수 있어 숨김 처리
    private void SetSpecialTerrainColor(Vector3Int pos, Color color)
    {
        SetTilemapColor(tileMapManager.farmlandTilemap, pos, color);
        SetTilemapColor(tileMapManager.goldMineTilemap, pos, color);
        SetTilemapColor(tileMapManager.cityTilemap,     pos, color);
    }

    // 특정 Tilemap의 해당 좌표 타일 색상 설정
    // TileFlags.None 설정 필수 — 없으면 Unity가 색상 변경을 무시함
    private void SetTilemapColor(Tilemap target, Vector3Int pos, Color color)
    {
        if (target == null || !target.HasTile(pos)) return;
        target.SetTileFlags(pos, TileFlags.None);
        target.SetColor(pos, color);
    }

    // ── 유틸 ─────────────────────────────────────────────────────

    // center 기준 원형 반경 내 유효 타일에 action 실행 (유클리드 거리 판별)
    private void ForEachInRadius(Vector3Int center, int radius, System.Action<Vector3Int> action)
    {
        for (int dx = -radius; dx <= radius; dx++)
        for (int dy = -radius; dy <= radius; dy++)
        {
            if (dx * dx + dy * dy > radius * radius) continue;

            Vector3Int pos = center + new Vector3Int(dx, dy, 0);
            if (!fogMap.ContainsKey(pos)) continue;

            action(pos);
        }
    }

    // 1×1 흰색 단색 스프라이트를 코드로 생성
    // 안개 타일, 영토 오버레이 타일 등 에셋 없이 단색 타일이 필요할 때 사용
    // TileMapManager도 영토 타일에서 이 메서드를 호출함
    public static Sprite CreateSolidSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
