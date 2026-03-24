using NYH.CoreCardSystem;
using UnityEngine;

public class PlayCostCardEffect : Effect
{
    [Header("카드 사용시 없앨 골드")]
    [SerializeField] private int playCardCost;

    public override GameAction GetGameAction()
    {
        PlayCostGA playCostGA = new(playCardCost);
        return playCostGA;
    }
}
