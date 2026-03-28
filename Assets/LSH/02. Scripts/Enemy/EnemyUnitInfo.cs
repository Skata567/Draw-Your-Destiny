using UnityEngine;
public enum EnemyGender
{
    Male,
    Female
}
public enum EnemyAgeGroup
{
    Baby,
    Adult,
    Old
}
public enum EnemyJob
{
    Farmer,
    Soldier,
}
[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Enemy/EnemyInfo")]
public class EnemyUnitInfo : ScriptableObject
{
    public int adultStartAge = 3;
    public int babyStartAge = 0;
    public int maxHealth = 1;
    public int attackPower = 1;
    public float startNaturalDeathChance = 0f;
    public float naturalDeathIncreasePerTurn = 0.01f;
}

