namespace NYH.CoreCardSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Collections;
    using DG.Tweening;

    public class CardSystem : Singleton<CardSystem>
    {
        [SerializeField] private HandView handView;
        [SerializeField] private Transform drawPilePoint;
        [SerializeField] private Transform discardPilePoint;

        private List<Card> drawPile = new();
        private List<Card> hand = new();
        private List<Card> discardPile = new();

        protected override void Awake()
        {
            base.Awake();
            // 액션 연결
            ActionSystem.AttachPerformer<DrawCardsGA>(action => Perform(action));
            ActionSystem.AttachPerformer<PlayCardGA>(action => Perform(action));
            ActionSystem.AttachPerformer<DiscardAllCardsGA>(action => Perform(action));
            ActionSystem.AttachPerformer<DiscardRandomGA>(action => Perform(action));
            Debug.Log("[CardSystem] 초기화 및 액션 등록 완료");
        }

        public void Setup(List<CardData> initialDeck)
        {
            drawPile.Clear();
            hand.Clear();
            discardPile.Clear();
            foreach (var data in initialDeck)
            {
                if (data != null) drawPile.Add(new Card(data));
            }
            drawPile.Shuffle();
            Debug.Log($"[CardSystem] 덱 세팅 완료: {drawPile.Count}장");
        }

        public IEnumerator Perform(GameAction action)
        {
            if (action is DrawCardsGA drawCardsGA)
            {
                Debug.Log($"[CardSystem] {drawCardsGA.Amount}장 드로우 시퀀스 시작");
                for (int i = 0; i < drawCardsGA.Amount; i++)
                {
                    if (drawPile.Count == 0) 
                    {
                        Debug.Log("[CardSystem] 덱이 비어있어 리필을 시도합니다.");
                        RefillDeck();
                    }

                    if (hand.Count >= 10) //손패 갯수 제한
                    {
                        Debug.Log("손패가 가득 찼습니다!");
                        break;
                    }

                    if (drawPile.Count > 0) 
                    {
                        yield return DrawCard();
                        yield return new WaitForSeconds(0.1f); // 카드 간 간격
                    }
                    else
                    {
                        Debug.LogWarning("[CardSystem] 더 이상 뽑을 카드가 없습니다.");
                        break;
                    }
                }
            }
            else if (action is PlayCardGA playCardGA)
            {
                yield return PlayCardPerformer(playCardGA);
            }
            else if (action is DiscardAllCardsGA discardAllCardsGA)
            {
                yield return DiscardAllCardsPerformer(discardAllCardsGA);
            }
            else if (action is DiscardRandomGA discardRandomGA)
            {
                for (int i = 0; i < discardRandomGA.Amount;i++)
                {
                    if (hand.Count == 0) break;
                    int randomIndex = Random.Range(0, hand.Count);
                    Card targetCard = hand[randomIndex];

                    yield return DiscardCardByInstance(targetCard);
                }
            }
        }

        private IEnumerator PlayCard(PlayCardGA playCardGA)
        {
            // Deprecated: Use PlayCardPerformer instead
            yield return PlayCardPerformer(playCardGA);
        }

        private IEnumerator DrawCard()
        {
            Card card = drawPile.Draw();
            hand.Add(card);
            
            Debug.Log($"[CardSystem] 카드 뽑는 중: {card.Title}");

            if (CardViewCreator.Instance == null)
            {
                Debug.LogError("[CardSystem] CardViewCreator.Instance가 null입니다! 씬에 프리팹을 가진 오브젝트가 있는지 확인하세요.");
                yield break;
            }

            if (drawPilePoint == null || handView == null)
            {
                Debug.LogError("[CardSystem] drawPilePoint나 handView가 할당되지 않았습니다!");
                yield break;
            }

            CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
            if (cardView != null)
            {
                yield return handView.AddCard(cardView);
                Debug.Log($"[CardSystem] '{card.Title}' 드로우 애니메이션 완료.");
            }
        }

        private void RefillDeck()
        {
            if (discardPile.Count > 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                drawPile.Shuffle();
            }
        }

        private IEnumerator DiscardCard(CardView cardView)
        {
            if (cardView == null) yield break;
            discardPile.Add(cardView.Card);
            
            cardView.transform.DOKill();
            cardView.transform.DOScale(Vector3.zero, 0.2f);
            Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.2f);
            if (tween != null) yield return tween.WaitForCompletion();
            
            if (cardView != null) Destroy(cardView.gameObject);
        }

        private IEnumerator DiscardCardByInstance(Card targetCard)
        {
            if (targetCard == null) yield break;

            hand.Remove(targetCard);
            CardView cardView = handView.RemoveCard(targetCard);
            
            if (cardView == null)
            {
                CardView[] allViews = FindObjectsByType<CardView>(FindObjectsSortMode.None);
                foreach (var cv in allViews)
                {
                    if (cv.Card == targetCard && !cv.IsHoverPreview) 
                    { 
                        cardView = cv; 
                        break; 
                    }
                }
            }
            yield return DiscardCard(cardView);
        }

        private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
        {
            // 리스트를 복사해서 순회 (중간에 hand에서 제거하므로)
            List<Card> cardsToDiscard = new List<Card>(hand);
            hand.Clear();

            foreach (var card in cardsToDiscard)
            {
                CardView cardView = handView.RemoveCard(card);
                // 카드가 화면에 없다면 찾아서라도 제거
                if (cardView == null)
                {
                    CardView[] allViews = FindObjectsByType<CardView>(FindObjectsSortMode.None);
                    foreach (var cv in allViews)
                    {
                        if (cv.Card == card && !cv.IsHoverPreview) 
                        { 
                            cardView = cv; 
                            break; 
                        }
                    }
                }
                
                // 순차적으로 버리지 않고 동시에 버리기 위해 코루틴 병렬 실행 고려 가능
                // 여기서는 순차적으로 실행
                StartCoroutine(DiscardCard(cardView)); 
                yield return new WaitForSeconds(0.05f); // 약간의 시차
            }
        }

        private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
        {
            hand.Remove(playCardGA.Card);
            CardView cardView = handView.RemoveCard(playCardGA.Card);
            
            if (cardView == null)
            {
                CardView[] allViews = FindObjectsByType<CardView>(FindObjectsSortMode.None);
                foreach (var cv in allViews)
                {
                    // Hover 미리보기용이 아닌 실제 카드 뷰만 찾습니다.
                    if (cv.Card == playCardGA.Card && !cv.IsHoverPreview) 
                    { 
                        cardView = cv; 
                        break; 
                    }
                }
            }

            //카드 삭제
            yield return DiscardCard(cardView);
            
            if (playCardGA.Card != null && playCardGA.Card.Effects != null)
            {
                foreach(var effect in playCardGA.Card.Effects)
                {
                    PerformEffectGA performEffectGA = new(effect);
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }


        }
    }
}
