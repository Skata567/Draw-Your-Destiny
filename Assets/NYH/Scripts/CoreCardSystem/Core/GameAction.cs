namespace NYH.CoreCardSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 게임 내에서 발생하는 모든 "동작"의 최상위 부모 클래스입니다.
    /// 예: 카드 뽑기, 카드 내기, 데미지 주기 등 모든 동작은 이 GameAction을 상속받습니다.
    /// </summary>
    public abstract class GameAction
    {
        // 이 액션이 실행되기 직전(PRE)에 연쇄적으로 발생할 액션 리스트
        // 예: 카드 뽑기 직전에 '카드 뽑기 방해' 효과가 발동할 때 사용
        public List<GameAction> PreReactions { get; private set; } = new();

        // 이 액션이 실제 실행되는 단계(PERFORM)에서 함께 발생할 액션 리스트
        public List<GameAction> PerformReactions { get; private set; } = new();

        // 이 액션이 실행된 직후(POST)에 연쇄적으로 발생할 액션 리스트
        // 예: 카드를 뽑은 직후에 '공격력이 1 증가함' 효과가 발동할 때 사용
        public List<GameAction> PostReactions { get; private set; } = new();
    }

    /// <summary>
    /// 연쇄 반응(Reaction)이 언제 일어날지를 정의하는 타이밍입니다.
    /// </summary>
    public enum ReactionTiming
    {
        PRE,  // 실행 전
        POST  // 실행 후
    }
}
