using NYH.CoreCardSystem;
using UnityEngine;

public class PlayCostCardEffect : Effect
{
    [SerializeField] private int playCardCost;

    public override GameAction GetGameAction()
    {
        PlayCostGA playCostGA = new(playCardCost);
        return playCostGA;
    }
}
