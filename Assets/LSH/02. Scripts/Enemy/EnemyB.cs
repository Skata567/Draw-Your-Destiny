using System.Collections.Generic;
using UnityEngine;


public class EnemyB : EnemyOrigin
{
    protected override void Start()
    {
        enemyID = 2;
        enemyType = EnemyType.B;
        base.Start();
    }
    protected override void ExecuteRandomAction()
    {
        base.ExecuteRandomAction();
        Debug.Log("적 B의 행동 실행");
    }

}
