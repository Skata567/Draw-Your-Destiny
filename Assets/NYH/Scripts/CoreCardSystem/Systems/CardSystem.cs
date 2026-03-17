namespace NYH.CoreCardSystem
{
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using DG.Tweening;

    public class CardSystem : Singleton<CardSystem>
    {
        [SerializeField] private HandView handView;
        [SerializeField] private Transform drawPilePoint;
        [SerializeField] private Transform discardPilePoint;
        private readonly List<Card> drawPile = new();
        private readonly List<Card> discardPile = new();
        private readonly List<Card> hand = new();

        private void OnEnable()
        {
            ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
            ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
            ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        }
        
        public void Setup(List<CardData> deckData)
        {
            drawPile.Clear();
            foreach (var data in deckData)
            {
                Card card = new Card(data);
                drawPile.Add(card);
            }
        }

        private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
        {
            int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
            int notDrawnAmount = drawCardsGA.Amount - actualAmount;
            for (int i = 0; i < actualAmount; i++)
            {
                yield return DrawCard();
            }
            if(notDrawnAmount > 0)
            {
                RefillDeck();
            }
        }

        private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
        {
            foreach(var card in hand)
            {
                CardView cardView = handView.RemoveCard(card);
                yield return DiscardCard(cardView);
            }
            hand.Clear();
        }

        private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
        {
            hand.Remove(playCardGA.Card);
            CardView cardView = handView.RemoveCard(playCardGA.Card);
            yield return DiscardCard(cardView);
            
            // 카드 효과 실행 부분 (기존 PerformEffectGA)은 여기에 직접 구현하거나
            // 이 액션을 상속받아 사용하세요.
        }

        private IEnumerator DrawCard()
        {
            Card card = drawPile.Draw();
            hand.Add(card);
            CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
            yield return handView.AddCard(cardView);
        }

        private void RefillDeck()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
        }

        private IEnumerator DiscardCard(CardView cardView)
        {
            discardPile.Add(cardView.Card);
            cardView.transform.DOScale(Vector3.zero, 0.15f);
            Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
            yield return tween.WaitForCompletion();
            Destroy(cardView.gameObject);
        }
    }
}
