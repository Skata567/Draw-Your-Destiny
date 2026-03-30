using NYH.CoreCardSystem;
using UnityEngine;

public class GoldCardEffect : Effect
{
    [Header("획득할 골드 수")]
    [SerializeField] private int costAmount;

    public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
    {
        return new GoldCardGA(costAmount);
    }

}

