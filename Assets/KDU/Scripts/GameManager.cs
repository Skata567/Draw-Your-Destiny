using UnityEngine;

// 전체 게임 관리 싱글톤
public class GameManager : PersistentSingleton<GameManager>
{
    [Header("Managers")]
    private TileMapManager tileMapManager;
    private BuildingPlacementService placementService;

    [Header("Game State")]
    public int currentTurn = 0;
    public int playerGold = 500;
    public int playerResearch = 0;
    public int playerPopulation = 0;
    public int playerPopulationCap = 10;

    protected override void Awake()
    {
        base.Awake();
        InitializeManagers();
    }

    // 매니저 초기화
    private void InitializeManagers()
    {
        tileMapManager = TileMapManager.Instance;
        placementService = FindFirstObjectByType<BuildingPlacementService>();
    }

    // 건물 배치 모드 시작
    public void StartBuildingPlacement(BuildingData buildingData)
    {
        if (placementService == null)
            return;

        if (playerGold < buildingData.goldCost)
            return;

        placementService.StartPlacing(buildingData);
    }

    // 턴 종료
    public void EndTurn()
    {
        currentTurn++;
    }

    // 금 추가
    public void AddGold(int amount)
    {
        playerGold += amount;
    }

    // 금 소모
    public bool SpendGold(int amount)
    {
        if (playerGold < amount)
            return false;

        playerGold -= amount;
        return true;
    }

    // 연구 포인트 추가
    public void AddResearch(int amount)
    {
        playerResearch += amount;
    }

    // 인구 한도 증가
    public void IncreasePopulationCap(int amount)
    {
        playerPopulationCap += amount;
    }
}
