using NYH.CoreCardSystem;
using System.Collections;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
        
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        if (performEffectGA == null || performEffectGA.Effect == null) 
        {
            Debug.LogWarning("[EffectSystem] 실행할 효과(Effect)가 null입니다.");
            yield break;
        }

        GameAction effectAction = performEffectGA.Effect.GetGameAction();
        ActionSystem.Instance.AddReaction(effectAction);
        yield return null;
    }
}
