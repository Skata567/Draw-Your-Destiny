using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [Header("적 타입")]
    public EnemyType enemyType;

    [Header("이동 속도")]
    [SerializeField] private float moveSpeed = 3f;

    private List<Vector3Int> currentPath = new List<Vector3Int>();
    private int pathIndex = 0;
    private bool isMoving = false;

    [Header("원본 데이터")]
    public EnemyUnitInfo enemyInfo;
    public EnemyUnitPool enemyPool;

    [Header("현재 상태")]
    [SerializeField] public int age;
    [SerializeField] private AgeGroup ageGroup;
    [SerializeField] private float naturalDeathChance;
    [SerializeField] private Gender gender;

    [Header("직업")]
    [SerializeField] public Job job;

    private BuildingType curbuildingType = BuildingType.None;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 실험용
        {
            UnitNextTurn();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            UseAdultUnitCard();
        }

        HandleClickMove();
        MoveAlongPath();
    }

    public void UnitAppear() // 유닛이 나올 때 초기화
    {
        naturalDeathChance = enemyInfo.startNaturalDeathChance;
        gender = Random.value < 0.5f ? Gender.Male : Gender.Female;
        ageGroup = AgeGroup.Baby;
        age = enemyInfo.babyStartAge;

        // 이동 관련 초기화
        currentPath.Clear();
        pathIndex = 0;
        isMoving = false;

        switch (job)
        {
            case Job.Farmer:
                break;

            case Job.Soldier:
                break;

            case Job.Miner:
                break;

            default:
                Debug.LogError("뭔 직업이냐 이건.");
                break;
        }
    }

    public void UnitNextTurn()
    {
        age++;
        ChangeAgeGroup();
        ChangeDeathPer();
        CheckCardUsing();

        if (Random.value < naturalDeathChance)
        {
            Dead();
        }
        TryRandomStepMove();
    }

    //------------------유닛 한칸씩 움직이는거임-----------------------------
    private void TryRandomStepMove()
    {
        Vector3Int currentCell = TileMapManager.Instance.groundTilemap.WorldToCell(transform.position);

        Vector3Int[] directions =
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0)
    };

        List<Vector3Int> possibleMoves = new List<Vector3Int>();

        foreach (var dir in directions)
        {
            Vector3Int nextCell = currentCell + dir;

            if (!TileMapManager.Instance.IsWalkableTerritory(nextCell))
                continue;

            possibleMoves.Add(nextCell);
        }

        if (possibleMoves.Count == 0)
            return;

        Vector3Int randomTarget = possibleMoves[Random.Range(0, possibleMoves.Count)];

        currentPath = new List<Vector3Int> { randomTarget };
        pathIndex = 0;
        isMoving = true;
    }


    void CheckCardUsing()
    {
    }

    void Dead()
    {
        if (enemyPool == null)
        {
            Debug.LogWarning($"{name} 의 enemyPool이 연결되지 않았습니다.");
            gameObject.SetActive(false);
            return;
        }

        enemyPool.ReturnEnemy(gameObject);
    }

    public void ChangeAgeGroup()
    {
        switch (age)
        {
            case < 3:
                ageGroup = AgeGroup.Baby;
                break;

            case < 15:
                ageGroup = AgeGroup.Adult;
                break;

            case >= 15:
                ageGroup = AgeGroup.Old;
                break;
        }
    }

    public void ChangeDeathPer()
    {
        switch (ageGroup)
        {
            case AgeGroup.Baby:
                naturalDeathChance = enemyInfo.startNaturalDeathChance;
                break;

            case AgeGroup.Adult:
                naturalDeathChance = enemyInfo.naturalDeathIncreasePerTurn * 2;
                break;

            case AgeGroup.Old:
                naturalDeathChance = 0.7f + enemyInfo.naturalDeathIncreasePerTurn * (age - 14f);
                break;
        }
    }

    public void UseAdultUnitCard()
    {
        ageGroup = AgeGroup.Adult;
        age = enemyInfo.adultStartAge;
        Debug.Log("어른 소환 카드 사용!");
    }

    void HandleClickMove()
    {
        if (!Input.GetMouseButtonDown(1))
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3Int targetCell = TileMapManager.Instance.groundTilemap.WorldToCell(mouseWorld);
        Vector3Int startCell = TileMapManager.Instance.groundTilemap.WorldToCell(transform.position);

        if (!TileMapManager.Instance.IsWalkableTerritory(targetCell))
            return;

        List<Vector3Int> newPath = PathFindingManager.Instance.FindPath(startCell, targetCell);

        if (newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            pathIndex = 0;
            isMoving = true;
        }
    }

    void MoveAlongPath()
    {
        if (!isMoving || currentPath == null || pathIndex >= currentPath.Count)
            return;

        Vector3 targetWorld = TileMapManager.Instance.GetCellCenterWorld(currentPath[pathIndex]);
        targetWorld.z = transform.position.z;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorld,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetWorld) < 0.02f)
        {
            transform.position = targetWorld;
            pathIndex++;

            if (pathIndex >= currentPath.Count)
                isMoving = false;
        }
    }
}