using System.Collections.Generic;
using UnityEngine;

public enum EnemyActionType
{
    Attack,
    Building,
    GetGold,
    GetHuman
}

[System.Serializable]
public class ActionCase
{
    public EnemyActionType actionType;
    [Range(0, 100)] public int weight;
}

public class EnemyOrigin : MonoBehaviour
{
    [Header("소환 위치")]
    [SerializeField] protected Transform spawnPoint;

    [Header("적 타입")]
    [SerializeField] protected EnemyType enemyType;

    [Header("적 골드 개념")]
    [SerializeField] protected int gold = 0;

    [Header("행동 확률")]
    [SerializeField] private List<ActionCase> actionCases = new List<ActionCase>();

    [SerializeField] protected int enemyID;
    [SerializeField] protected EnemyUnitPool enemyUnitPool;

    protected virtual void Start()
    {
        enemyUnitPool = transform.parent.GetComponentInChildren<EnemyUnitPool>();

        AddTurnList();
        GameStartEnemyInfo();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // 실험용
        {
            StartEnemyTurn();
        }
    }

    protected virtual void GameStartEnemyInfo()
    {
    }

    protected virtual void AddTurnList()
    {
        actionCases.Clear();

        actionCases.Add(new ActionCase { actionType = EnemyActionType.Attack, weight = 5 });
        actionCases.Add(new ActionCase { actionType = EnemyActionType.Building, weight = 20 });
        actionCases.Add(new ActionCase { actionType = EnemyActionType.GetGold, weight = 40 });
        actionCases.Add(new ActionCase { actionType = EnemyActionType.GetHuman, weight = 35 });
    }

    //--------------------턴 시작과 종료----------------------
    public virtual void StartEnemyTurn()
    {
        Debug.Log("적 턴 시작");
        ExecuteRandomAction();
    }

    protected virtual void EndEnemyTurn()
    {
        Debug.Log("적 턴 종료");
    }

    //---------------------행동 실행----------------------
    protected virtual void ExecuteRandomAction()
    {
        EnemyActionType enemyAction = GetWeightedRandomAction();
        bool actionCheck = CheckAction(enemyAction);

        if (!actionCheck)
        {
            enemyAction = EnemyActionType.GetGold;
            Debug.Log($"<color=yellow>[적은 지금 거지에요]</color> 골드 부족으로 인해 강제적으로 골드 턴.");
        }

        Debug.Log($"<color=yellow>[Enemy Turn]</color> 행동: {enemyAction}");

        switch (enemyAction)
        {
            case EnemyActionType.Attack:
                DoAttack();
                break;

            case EnemyActionType.Building:
                DoBuilding();
                break;

            case EnemyActionType.GetGold:
                DoGetGold();
                break;

            case EnemyActionType.GetHuman:
                DoGetEnemyHuman();
                break;
        }

        EndEnemyTurn();
    }

    protected virtual EnemyActionType GetWeightedRandomAction()
    {
        int totalWeight = 0;

        foreach (var actionCase in actionCases)
        {
            totalWeight += actionCase.weight;
        }

        if (totalWeight <= 0)
            return EnemyActionType.GetGold;

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var actionCase in actionCases)
        {
            currentWeight += actionCase.weight;

            if (randomValue < currentWeight)
                return actionCase.actionType;
        }

        return EnemyActionType.GetGold;
    }

    //--------------------각 기본 행동들-------------------
    protected virtual void DoAttack()
    {
        Debug.Log("적이 공격을 선택했다!");
    }

    protected virtual void DoBuilding()
    {
        Debug.Log("적이 건물을 짓는다람쥐");
    }

    protected virtual void DoGetGold()
    {
        gold += 10;
        Debug.Log($"<color=blue>[골드 얻음]</color> 현재 골드: {gold}");
    }

    protected virtual void DoWait()
    {
        Debug.Log("한조 대기중.");
    }

    protected virtual void DoGetEnemyHuman()
    {
        if (enemyUnitPool == null)
        {
            Debug.LogWarning("EnemyUnitPool이 연결되지 않았습니다.");
            return;
        }

        if (gold < 10)
        {
            Debug.LogWarning("골드 부족으로 유닛을 소환할 수 없습니다.");
            return;
        }

        GameObject spawnedEnemy = enemyUnitPool.GetEnemyUnit(enemyType);

        if (spawnedEnemy == null)
        {
            Debug.LogWarning($"{enemyType} 타입 유닛 소환 실패");
            return;
        }

        gold -= 10;

        if(spawnPoint == null)
        {
            spawnedEnemy.transform.position = transform.position;
        }
        else
            spawnedEnemy.transform.position = spawnPoint.position;

        EnemyUnit enemyUnit = spawnedEnemy.GetComponent<EnemyUnit>();
        if (enemyUnit != null)
        {
            enemyUnit.enemyType = enemyType;
            enemyUnit.enemyPool = enemyUnitPool;

            // 직업은 여기서 따로 랜덤/설정 가능
            // enemyUnit.job = Job.Farmer;
            // enemyUnit.UnitAppear(); 
            // ※ 이미 GetEnemyUnit에서 UnitAppear() 호출 중이면 여기선 다시 호출 안 해도 됨
        }

        Debug.Log($"<color=red>[유닛 소환]</color> {enemyType} 적 소환 완료 / 현재 골드: {gold}");
    }

    //-------------------특수 행동--------------------------------
    protected virtual void BuildSmallTown()
    {
    }

    //----------------행동검사------------------
    protected virtual bool CheckAction(EnemyActionType action)
    {
        switch (action)
        {
            case EnemyActionType.Attack:
                return true;

            case EnemyActionType.Building:
                return gold >= 50;

            case EnemyActionType.GetGold:
                return true;

            case EnemyActionType.GetHuman:
                return gold >= 10;
        }

        return false;
    }
}