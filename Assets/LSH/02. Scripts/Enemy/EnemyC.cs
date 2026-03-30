using System.Collections.Generic;
using UnityEngine;


public class EnemyC : EnemyOrigin
{
    protected override void Start()
    {
        enemyID = 3;
        enemyType = EnemyType.C;
        base.Start();
    }
    protected override void ExecuteRandomAction()
    {
        base.ExecuteRandomAction();
        Debug.Log("적 C의 행동 실행");
    }

}
