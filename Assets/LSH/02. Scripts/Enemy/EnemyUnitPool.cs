using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitPool : MonoBehaviour
{
    public GameObject EnemyUnitPrefeb;
    public int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject human = Instantiate(EnemyUnitPrefeb);
            human.SetActive(false);

            pool.Enqueue(human);
        }
    }

    public GameObject GetEnemyUnit() //이거 쓰셈 소환할때.(카드 만드는 사람은 이걸 읽도록)
    {
        if (pool.Count == 0)
        {
            Debug.Log("전부 소환됐음요");
            return null;
        }

        GameObject enemy = pool.Dequeue();
        enemy.SetActive(true);
        EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
        if (enemyUnit != null)
        {
            enemyUnit.UnitAppear();
        }

        return enemy;
    }

    public void ReturnHuman(GameObject enemy)
    {
        enemy.SetActive(false);
        pool.Enqueue(enemy);
    }
}
