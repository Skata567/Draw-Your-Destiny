using NYH.CoreCardSystem;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; private set; }
    public PerformEffectGA(Effect effect)
    {
        Effect = effect;
    }
}
