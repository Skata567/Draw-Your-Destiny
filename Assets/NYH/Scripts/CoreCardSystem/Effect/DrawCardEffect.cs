using NYH.CoreCardSystem;
using UnityEngine;

public class DrawCardEffect : Effect
{
    [Header("드로우할 카드의 수")]
    [SerializeField] private int drawAmount;

    public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
    {
        DrawCardsGA drawCardsGA = new(drawAmount);
        return drawCardsGA;
    }
}
