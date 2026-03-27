/*using NYH.CoreCardSystem;

namespace NYH
{RegisterDurationGA
    public class RegisterDurationGA : GameAction
    {
        public int Amount { get; private set; }

        public RegisterDurationGA(int amount)
        {
            Amount = amount;
        }
    }
}
*/

/*
 *  1 [System.Serializable]
    2 public class DurationEffect : Effect
    3 {
    4     public int duration; // 몇 턴 동안 지속될지 설정 (예: 3)
    5
    6     public override GameAction GetGameAction()
    7     {
    8         // 카드를 낼 때 이 액션이 실행되면 CardSystem이 감지합니다.
    9         return new RegisterDurationGA(duration);
   10     }
   11 }

  2. 활성화된 효과를 관리하는 데이터 (ActiveDuration.cs)
  현재 게임 필드 위에 떠 있는 "지속 효과" 그 자체를 정의합니다.

    1 public class ActiveDuration
    2 {
    3     public Card SourceCard;      // 원래 어떤 카드였는지 (효과들을 다시 쓰기 위해)
    4     public int RemainingTurns;   // 남은 턴 수
    5
    6     public ActiveDuration(Card card, int turns)
    7     {
    8         SourceCard = card;
    9         RemainingTurns = turns;
   10     }
   11 }

  3. CardSystem.cs에서의 처리 (핵심 로직)
  여기서 카드를 낼 때 등록하고, 턴이 끝날 때마다 효과를 "재발동" 시킵니다.

    1 public class CardSystem : Singleton<CardSystem>
    2 {
    3     // 현재 활성화된 지속 효과 리스트
    4     private List<ActiveDuration> activeDurations = new();
    5
    6     // [A] 카드를 낼 때: 지속 효과가 있다면 리스트에 등록
    7     private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    8     {
    9         // ... 기존 카드 내기 로직 (무덤 이동 등) ...
   10
   11         // 카드 효과 중에 DurationEffect가 있는지 체크
   12         foreach (var effect in playCardGA.Card.Effects)
   13         {
   14             if (effect is DurationEffect de)
   15             {
   16                 // 지속 효과 리스트에 추가 (카드는 무덤에 가도 데이터는 여기 남음)
   17                 activeDurations.Add(new ActiveDuration(playCardGA.Card, de.duration));
   18             }
   19         }
   20     }
   21
   22     // [B] 턴 종료 시: 리스트를 돌며 효과 재발동 (여기가 포인트!)
   23     public IEnumerator ProcessTurnEndEffects()
   24     {
   25         List<ActiveDuration> expired = new();
   26
   27         foreach (var active in activeDurations)
   28         {
   29             // 1. 해당 카드가 가진 '모든 효과'를 다시 실행 목록에 넣음
   30             // (이미 만든 GoldCardEffect 등이 여기서 다시 불려감!)
   31             foreach (var effect in active.SourceCard.Effects)
   32             {
   33                 // 지속 시간 효과 자체는 빼고 나머지 실제 효과들만 실행
   34                 if (effect is not DurationEffect)
   35                 {
   36                     ActionSystem.Instance.AddReaction(new PerformEffectGA(effect));
   37                 }
   38             }
   39
   40             // 2. 턴 감소 및 만료 체크
   41             active.RemainingTurns--;
   42             if (active.RemainingTurns <= 0) expired.Add(active);
   43         }
   44
   45         // 3. 끝난 효과는 삭제
   46         foreach (var ex in expired) activeDurations.Remove(ex);
   47
   48         yield return null;
   49     }
   50 }
   1 public void EndTurn()
   2 {
   3     endTurn = true;
   4
   5     // 턴이 끝날 때 CardSystem에 "자, 이제 지속 효과들 다 발동시켜!"라고 명령
   6     StartCoroutine(CardSystem.Instance.ProcessTurnEndEffects());
   7 }
 * 
 */