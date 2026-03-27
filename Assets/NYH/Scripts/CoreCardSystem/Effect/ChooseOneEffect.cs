using NYH.CoreCardSystem;
using UnityEngine;

public class ChooseOneEffect : Effect
{
    [Header("선택을 띄울 카드의 수")]
    [SerializeField] private int ChoseOneEffect;

    public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
    {
        ChooseOneGA chooseOneGA = new(ChoseOneEffect);
        return chooseOneGA;
    }
}
