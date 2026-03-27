using NYH.CoreCardSystem;
using UnityEngine;

public class ResearchpointsEffect : Effect
{
    [Header("증가 시킬 연구 포인트")]
    [SerializeField] private int ResarchPointGA;

    public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
    {
        return new ResearchpointsGA(ResarchPointGA);
    }
}