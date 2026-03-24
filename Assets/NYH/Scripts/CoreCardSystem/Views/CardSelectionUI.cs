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
        [SerializeField] private Button closeButton;     // [추가] 닫기 버튼 레퍼런스

        private Action<Card> onCardSelectedCallback;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            if (panel != null) panel.SetActive(false);

            // [추가] 닫기 버튼 클릭 시 Close 함수 호출 연결
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        public void Show(List<Card> cards, Action<Card> onSelected = null)
        {
            if (panel == null || container == null) return;

            onCardSelectedCallback = onSelected;
            panel.SetActive(true);

            // [추가] 선택 모드(onSelected가 있음)라면 닫기 버튼을 숨기고, 
            // 단순 보기 모드(onSelected가 없음)라면 닫기 버튼을 보여줍니다.
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(onSelected == null);
            }

            // 1. 기존 카드 제거
            foreach (Transform child in container) Destroy(child.gameObject);

            // 2. 선택용 카드 생성
            foreach (var card in cards)
            {
                CardView cardView = CardViewCreator.Instance.CreateCardView(card, container.position, Quaternion.identity);
                cardView.transform.SetParent(container, false);

                // [중요] 호버 기능 설정: IsHoverPreview를 true로 설정합니다.
                cardView.IsHoverPreview = true;

                // 클릭 처리를 위한 버튼 설정 (선택 콜백이 있을 때만 활성화)
                Button button = cardView.GetComponent<Button>();
                if (button == null) button = cardView.gameObject.AddComponent<Button>();

                button.onClick.RemoveAllListeners();
                
                if (onSelected != null)
                {
                    Card capturedCard = card;
                    button.onClick.AddListener(() => OnCardClicked(capturedCard));
                    button.interactable = true;
                }
                else
                {
                    // 단순 보기 모드일 때는 버튼 상호작용은 끄되, 호버는 작동하게 합니다.
                    // (만약 버튼 자체가 Raycast를 막는다면 interatacle만 끄거나 필요시 조정)
                    button.interactable = false;
                }

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
