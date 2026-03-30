using UnityEngine;

public class EnemyA : EnemyOrigin
{
    protected override void Start()
    {
        enemyID = 1;
        enemyType = EnemyType.A;
        base.Start();
    }

    protected override void ExecuteRandomAction()
    {
        base.ExecuteRandomAction();
        Debug.Log("적 A의 행동 실행");
    }


    // 적 A는 더 많은 인간을 뽑는다고 가정
    //protected override void DoGetEnemyHuman()
    //{
    //    Debug.Log("A는 더 많이 뽑는다!");

    //    GameObject enemy1 = enemyUnitPool.GetEnemyUnit(enemyType);
    //    GameObject enemy2 = enemyUnitPool.GetEnemyUnit(enemyType);

    //    gold -= 20;
    //}

    // 적 A는 건물 안 짓는다고 가정
    //protected override void DoBuilding()
    //{
    //    Debug.Log("A는 건물 안 지음");
    //}

    // 적 A는 인간 소환이 더 싸다고 가정
    //protected override bool CheckAction(EnemyActionType action)
    //{
    //    if (action == EnemyActionType.GetHuman)
    //        return gold >= 5; // A는 더 싸게 소환

    //    return base.CheckAction(action);
    //}
}