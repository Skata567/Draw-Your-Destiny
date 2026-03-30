﻿using NYH.CoreCardSystem;
using UnityEngine;

public class PlayCostCardEffect : Effect
{
    [Header("카드 사용시 없앨 골드")]
    [SerializeField] private int playCardCost;

    public override GameAction GetGameAction(int effectIndex = 0, Card sourceCard = null)
    {
        // 만약 sourceCard가 전달되었다면 카드의 실시간 Cost를 사용합니다.
        // sourceCard가 없는 특수한 상황을 대비해 기본값(playCardCost)을 백업으로 둡니다.
        int actualCostToPay = (sourceCard != null) ? sourceCard.Cost : playCardCost;
        
        PlayCostGA playCostGA = new(actualCostToPay);
        return playCostGA;
    }
}
