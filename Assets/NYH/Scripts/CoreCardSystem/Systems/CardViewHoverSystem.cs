using NYH.CoreCardSystem;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    [SerializeField] int cardYposition = 0;
    public void Show(Card card, Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);

        Vector3 offset = new Vector3(0, cardYposition, 0);
        cardViewHover.transform.position = position + offset;
    }
    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }
}
