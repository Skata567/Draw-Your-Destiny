using NYH.CoreCardSystem;
using UnityEngine;

public class ZeroCostHandEffect : Effect
{
    [Header("변경 시킬 손패의 카드의 코스트")]
    [SerializeField] private int costAmount;
    [Header("변경 시킬 손패의 카드 수")]
    [SerializeField] private int cardAmount;

    public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
    {
        ZeroCostHandGA zeroCostHandGA = new(costAmount, cardAmount);
        return zeroCostHandGA;
    }
}
