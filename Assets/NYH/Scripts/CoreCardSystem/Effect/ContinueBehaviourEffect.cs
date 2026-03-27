using NYH.CoreCardSystem;
using UnityEngine;

public class ContinueBehaviourEffect : Effect
{
    [Header("반복할 턴 수")]
    [SerializeField] private int turnAmount;

    public override GameAction GetGameAction(int effectIndex, Card sourceCard)
    {
        return new ContinueBehaviourGA(sourceCard, effectIndex + 1, turnAmount);
    }
}
