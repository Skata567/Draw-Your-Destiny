namespace NYH.CoreCardSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 카드 선택(발견/Discover) UI를 관리하는 클래스입니다.
    /// </summary>
    public class CardSelectionUI : MonoBehaviour
    {
        public static CardSelectionUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject panel;       
        [SerializeField] private Transform container;    

        private Action<Card> onCardSelectedCallback;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        public void Show(List<Card> cards, Action<Card> onSelected)
        {
            if (panel == null || container == null) return;

            onCardSelectedCallback = onSelected;
            panel.SetActive(true);

            // 1. 기존 카드 제거
            foreach (Transform child in container) Destroy(child.gameObject);

            // 2. 선택용 카드 생성
            foreach (var card in cards)
            {
                CardView cardView = CardViewCreator.Instance.CreateCardView(card, container.position, Quaternion.identity);
                cardView.transform.SetParent(container, false);

                // [중요] 호버 기능 복구: 이제 IsHoverPreview를 true로 하지 않습니다!
                // cardView.IsHoverPreview = false; (기본값이 false임)

                // 클릭 처리를 위한 버튼 설정
                Button button = cardView.GetComponent<Button>();
                if (button == null) button = cardView.gameObject.AddComponent<Button>();

                button.onClick.RemoveAllListeners();
                Card capturedCard = card;
                button.onClick.AddListener(() => OnCardClicked(capturedCard));

                cardView.transform.localScale = Vector3.one;
            }
        }

        private void OnCardClicked(Card card)
        {
            Debug.Log($"[CardSelectionUI] 카드 선택 완료: {card.Title}");
            onCardSelectedCallback?.Invoke(card);
            
            // 호버가 뜬 채로 선택될 경우 미리보기를 숨겨줍니다.
            if (CardViewHoverSystem.Instance != null) CardViewHoverSystem.Instance.Hide();
            
            Close();
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
