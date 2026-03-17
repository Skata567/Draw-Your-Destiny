using UnityEditor;
using UnityEngine;

public class HumanUnit : MonoBehaviour
{
    [Header("원본 데이터")]
    public UnitInfo unitInfo;
    public BuildingData buildingData;
    public HumanPool humanPool;

    [Header("현재 상태")]
    [SerializeField] private int age;
    [SerializeField] private Gender gender;
    [SerializeField] private AgeGroup ageGroup;
    [SerializeField] private bool koreanArmy;
    [SerializeField] private float naturalDeathChance;

    private bool isDead = false;
    void Start()
    {
        UnitAppear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //실험용
        {
            UnitNextTurn();
        }
    }
    void UnitAppear() //유닛이 나올 때 초기화하는거
    {
        humanPool = FindAnyObjectByType<HumanPool>();
        naturalDeathChance = unitInfo.startNaturalDeathChance;
        gender = Random.value < 0.5f ? Gender.Male : Gender.Female;
        ageGroup = Random.value < 0.5f ? AgeGroup.Adult : AgeGroup.Baby;
        if(ageGroup == AgeGroup.Baby)
            age = unitInfo.babyStartAge;
        else
            age = unitInfo.adultStartAge;
        koreanArmy = false;
    }
    public void UnitNextTurn() //턴이 지날때마다 나오는거니까 여따가 다 때려박을까
    {
        age++;
        ChangeAgeGroup();
        ChangeDeathPer();
        if (Random.value < naturalDeathChance)
        {
            Dead();
        }
    }
    void Dead() //죽는거는 이거
    {
        humanPool.ReturnHuman(this.gameObject);
    }
    public void ChangeAgeGroup()//나이 그룹 바뀌는거는 이거
    {
        switch(age)
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
                naturalDeathChance = unitInfo.startNaturalDeathChance;
                break;
            case AgeGroup.Adult:
                naturalDeathChance += unitInfo.naturalDeathIncreasePerTurn * (age - unitInfo.adultStartAge);
                break;
            case AgeGroup.Old:
                naturalDeathChance += unitInfo.naturalDeathIncreasePerTurn * 10; 
                break;
        }
    }

    public void GoToFarm() //농사 지으러 가자
    {

    }
}
