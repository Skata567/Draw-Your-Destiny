using NYH.CoreCardSystem;
using UnityEngine;

public class ColdCardEffect : Effect
{
    [Header("획득할 골드 수")]
    [SerializeField] private int costAmount;

    public override GameAction GetGameAction()
    {
        return new GoldCardGA(costAmount);
    }

}

