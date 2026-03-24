using NYH.CoreCardSystem;
using System.Collections;
using UnityEngine;

/// <summary>
/// 카드를 냈을 때 발생하는 '효과(Effect)'들을 실질적으로 처리하는 시스템입니다.
/// 추상적인 효과 데이터를 실제 실행 가능한 GameAction으로 변환해 줍니다.
/// </summary>
public class EffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // [사용법] 시스템이 활성화될 때 ActionSystem에 "효과 실행 명령"을 처리하겠다고 등록합니다.
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }

    private void OnDisable()
    {
        // 시스템이 비활성화될 때 등록을 해제합니다.
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }

    /// <summary>
    /// [핵심 로직] 예약된 PerformEffectGA를 받아 실제로 어떤 일이 일어날지 결정합니다.
    /// </summary>
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        if (performEffectGA == null || performEffectGA.Effect == null) 
        {
            Debug.LogWarning("[EffectSystem] 실행할 효과(Effect)가 없습니다.");
            yield break;
        }

        // 1. 효과 데이터(ScriptableObject)로부터 실제 실행할 액션을 가져옵니다.
        // 예: DrawCardEffect라면 여기서 DrawCardsGA(2) 같은 액션을 리턴합니다.
        GameAction effectAction = performEffectGA.Effect.GetGameAction();

        // 2. 가져온 진짜 액션을 다시 ActionSystem의 연쇄 반응 리스트에 넣습니다.
        // 이렇게 하면 '카드 내기' 동작 중에 '카드 뽑기' 동작이 순차적으로 실행됩니다.
        if (effectAction != null)
        {
            ActionSystem.Instance.AddReaction(effectAction);
        }

        yield return null; // 한 프레임 대기
    }
}

/*
 * [새로운 효과를 추가하는 방법]
 * 1. Models/Effect.cs를 상속받는 새로운 클래스를 만듭니다. (예: DamageEffect)
 * 2. GetGameAction() 함수를 오버라이드하여 실제 데미지를 주는 GA를 리턴하게 만듭니다.
 * 3. CardData 에셋의 Effects 리스트에 새로 만든 효과를 드래그해서 넣습니다.
 * 4. 이제 카드를 내면 자동으로 이 EffectSystem을 통해 효과가 발동됩니다!
 */
