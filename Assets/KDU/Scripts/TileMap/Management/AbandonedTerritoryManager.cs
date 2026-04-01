using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// AbandonedTerritoryManager — 버려진 영지 (맵 정중앙 중립 거점) 관리
//
// 스펙:
//   - 맵 정중앙에 고정 배치, 8×8 크기 (소규모 영지와 동일)
//   - 초기 상태: 중립 (미점령). 지형만 보이고 소유자 없음
//   - 점령: 유닛 진입 시 즉시 점령 (무혈). 외벽 없음
//   - 완전 점령: 점령 후 소규모 영지 카드를 잔해 위치에 정확히 배치해야 함
//   - 완전 점령 시: 외벽 자동 설치 + 전 영지에서 거리 무관 직접 이동 가능
//
// AI 연동:
//   OnCaptured / OnOutpostBuilt / OnRecaptured / OnLost 이벤트 구독
//   TryCapture(unitPos, civID) 호출 (LSH 유닛 시스템에서)
// ============================================================
[DefaultExecutionOrder(5)] // CitySpawnManager(10)보다 먼저 실행 → IsInRuins() 사용 가능
public class AbandonedTerritoryManager : Singleton<AbandonedTerritoryManager>
{
    [Header("잔해 위치 설정")]
    [Tooltip("맵 정중앙 타일 좌표. Inspector에서 직접 입력.")]
    public Vector3Int centerTilePos;

    [Tooltip("잔해 크기 (소규모 영지와 동일, 기본 8)")]
    public int outpostSize = 8;

    [Header("시각화")]
    [Tooltip("잔해 장식 프리팹 (게임 시작 시 중앙에 자동 생성)")]
    public GameObject ruinsVisualPrefab;

    // ── 런타임 상태 (읽기 전용 프로퍼티로 외부 공개) ──────────

    // 현재 소유 문명 (-1 = 중립)
    public int OwnerCivID { get; private set; } = -1;

    // 점령은 됐지만 소규모 영지 카드 배치를 아직 안 한 문명 (-1 = 해당 없음)
    // TileMapManager.CanPlace에서 이 값으로 배치 허용 여부 판단
    public int PendingOutpostCivID { get; private set; } = -1;

    // 소규모 영지 카드 배치 완료 여부
    public bool IsOutpostBuilt { get; private set; } = false;

    // 외벽 설치 완료 여부
    public bool IsWallBuilt { get; private set; } = false;

    // 점령 여부 (중립이 아닌 경우)
    public bool IsCaptured => OwnerCivID != -1;

    // 버려진 영지 전체 타일 좌표 목록 (유닛 진입 감지에 사용)
    private List<Vector3Int> ruinsFootprint = new List<Vector3Int>();

    // ── AI 연동용 이벤트 ───────────────────────────────────────
    // 나중에 AI 시스템에서 구독해서 사용
    public event Action<int> OnCaptured;       // 최초 점령 시 (civID)
    public event Action<int> OnOutpostBuilt;   // 소규모 영지 배치 완료 시 (civID)
    public event Action<int> OnRecaptured;     // 재점령 시 (새 ownerCivID)
    public event Action<int> OnLost;           // 점령지를 잃을 때 (이전 ownerCivID)

    private GameObject ruinsVisualInstance;

    protected override void Awake()
    {
        base.Awake();
        BuildFootprint();
    }

    private void Start()
    {
        SpawnRuinsVisual();
    }

    // ── 초기화 ────────────────────────────────────────────────

    // 잔해 영역 footprint 계산 (좌하단 origin 기준 8×8)
    private void BuildFootprint()
    {
        ruinsFootprint.Clear();
        Vector3Int origin = GetRuinsOrigin();
        for (int x = 0; x < outpostSize; x++)
        for (int y = 0; y < outpostSize; y++)
            ruinsFootprint.Add(origin + new Vector3Int(x, y, 0));
    }

    // 잔해 좌하단 기준점 반환
    // TileMapManager.GetOrigin과 동일한 계산 (짝수 크기: clickPos - (size/2 - 1))
    public Vector3Int GetRuinsOrigin()
    {
        int offset = outpostSize / 2 - 1; // 8×8 → offset = 3
        return centerTilePos - new Vector3Int(offset, offset, 0);
    }

    // 잔해 장식 오브젝트 씬에 생성
    private void SpawnRuinsVisual()
    {
        if (ruinsVisualPrefab == null) return;
        if (TileMapManager.Instance == null) return;

        Vector3 worldPos = TileMapManager.Instance.groundTilemap.GetCellCenterWorld(centerTilePos);
        ruinsVisualInstance = Instantiate(ruinsVisualPrefab, worldPos, Quaternion.identity);
        ruinsVisualInstance.name = "AbandonedTerritory_Visual";
    }

    // ── 외부 조회 API ─────────────────────────────────────────

    // 이 타일 좌표가 잔해 영역 안인지 확인 (LSH 유닛 이동에서 사용)
    public bool IsInRuins(Vector3Int pos) => ruinsFootprint.Contains(pos);

    // 버려진 영지의 월드 중심 좌표 (AI 이동 목표 계산에 사용)
    public Vector3 GetWorldCenter()
    {
        if (TileMapManager.Instance == null) return Vector3.zero;
        return TileMapManager.Instance.groundTilemap.GetCellCenterWorld(centerTilePos);
    }

    // TileMapManager.CanPlace에서 호출 — 잔해 위 소규모 영지 배치 허용 여부
    // origin: TileMapManager.GetOrigin이 계산한 기준점
    // civID: 배치 시도하는 문명 ID
    public bool CanPlaceOutpostHere(Vector3Int origin, int civID)
    {
        // 이 문명이 점령했고, 아직 outpost 미배치 상태여야 함
        if (PendingOutpostCivID != civID) return false;
        // 잔해 기준점과 정확히 일치해야 함
        return origin == GetRuinsOrigin();
    }

    // ── 점령 API (LSH 유닛 시스템에서 호출) ──────────────────

    // 유닛이 타일에 진입할 때마다 호출
    // 반환: 이 타일이 잔해 영역이고 점령 처리가 발생했으면 true
    public bool TryCapture(Vector3Int unitPos, int civID)
    {
        if (!ruinsFootprint.Contains(unitPos)) return false;
        if (OwnerCivID == civID) return false; // 이미 내 영지

        int previousOwner = OwnerCivID;

        if (previousOwner != -1)
            OnLost?.Invoke(previousOwner);

        OwnerCivID = civID;
        PendingOutpostCivID = civID;
        IsOutpostBuilt = false;
        IsWallBuilt = false;

        // 영토 소유권 등록
        TileMapManager.Instance?.ClaimTerritory(centerTilePos, civID, outpostSize / 2);

        if (previousOwner == -1)
        {
            OnCaptured?.Invoke(civID);
            Debug.Log($"[AbandonedTerritory] 문명 {civID}: 버려진 영지 점령 완료. 소규모 영지 카드를 잔해 위에 배치하세요.");
        }
        else
        {
            OnRecaptured?.Invoke(civID);
            Debug.Log($"[AbandonedTerritory] 문명 {civID}: 버려진 영지 재점령. 소규모 영지 카드를 잔해 위에 배치하세요.");
        }

        return true;
    }

    // ── 소규모 영지 배치 완료 콜백 (TileMapManager.PlaceBuilding에서 호출) ──

    // Outpost 카드가 잔해 위에 정확히 배치됐을 때 TileMapManager가 호출
    public void OnRuinsOutpostBuilt(int civID)
    {
        if (PendingOutpostCivID != civID) return;

        IsOutpostBuilt = true;
        PendingOutpostCivID = -1;

        // 외벽 자동 설치
        InstallWall(civID);

        // 잔해 장식 제거 (소규모 영지 비주얼로 교체됨)
        if (ruinsVisualInstance != null)
        {
            Destroy(ruinsVisualInstance);
            ruinsVisualInstance = null;
        }

        OnOutpostBuilt?.Invoke(civID);
        Debug.Log($"[AbandonedTerritory] 문명 {civID}: 소규모 영지 배치 완료. 외벽 자동 설치됨. 전 영지에서 직접 이동 가능.");
    }

    // 외벽 자동 설치 (Wall BuildingData 연결 후 구현 예정)
    private void InstallWall(int civID)
    {
        IsWallBuilt = true;
        // TODO: Wall BuildingData를 Inspector에서 연결 후
        //       TileMapManager.PlaceBuilding(wallPos, wallData, civID) 호출
        Debug.Log($"[AbandonedTerritory] 외벽 자동 설치 (civID={civID}) — Wall BuildingData 연결 후 구현 예정");
    }
}
