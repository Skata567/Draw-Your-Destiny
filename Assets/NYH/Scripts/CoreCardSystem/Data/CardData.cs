namespace NYH.CoreCardSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [CreateAssetMenu(menuName = "Data/Card")]
    public class CardData : ScriptableObject
    {
        [field: SerializeField][TextArea(3,5)] private string description;
        public string Description => description;
        [field: SerializeField] public int Mana { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; }
    }
}
