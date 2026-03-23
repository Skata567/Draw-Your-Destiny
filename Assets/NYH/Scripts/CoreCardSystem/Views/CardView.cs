namespace NYH.CoreCardSystem
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using UnityEngine.EventSystems;

    public class CardView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Text Objects")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text manaText;

        [Header("UI Image Objects")]
        [SerializeField] private Image cardArtImage;
        [SerializeField] private Image cardBackgroundImage;

        [Header("Other Settings")]
        [SerializeField] private GameObject wrapper;
        [SerializeField] private LayerMask dropLayer;
        [SerializeField] private float dragSpeed = 0.15f;
        
        [Header("Effect Settings")]
        [SerializeField] private float tiltStrength = 5f; // 기울기 강도 (상향)
        [SerializeField] private float maxTiltAngle = 20f; // 최대 기울기 각도

        public Card Card { get; private set; }

        public static bool AnyCardPickedUp = false;
        public bool IsHoverPreview { get; set; } = false; // Hover 미리보기용인지 확인용 추가

        private Vector3 dragStartPosition;
        private Vector3 currentVelocity; //속도용
        private Quaternion dragStartRotation;
        
        private bool isDragging = false;  // 꾹 누르고 있는 상태
        private bool isPickedUp = false;  // 클릭해서 붙어있는 상태
        private Vector3 pointerDownMousePos; // 처음 클릭한 위치 저장
        private float clickThreshold = 20f;  // 클릭과 드래그를 구분할 거리 임계값

        private Camera mainCamera;
        private HandView cachedHandView;

        private void Awake()
        {
            mainCamera = Camera.main;
            cachedHandView = FindFirstObjectByType<HandView>();
            
        }

        private void Update()
        {
            // 집어든 상태이거나 드래그 중일 때 마우스 따라가기
            if (isPickedUp || isDragging)
            {
                // 마우스의 현재 화면 좌표를 가져옵니다.
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = -mainCamera.transform.position.z;
                transform.position = Vector3.SmoothDamp(transform.position, mousePos, ref currentVelocity, dragSpeed);

                
                // 좌우 속도가 0.5보다 작으면 0으로 처리하여 미세 떨림 방지
                float horizontalVelocity = Mathf.Abs(currentVelocity.x) > 1500f ? currentVelocity.x : 0f;

                // 오른쪽으로 가면(-), 왼쪽으로 가면(+) 회전하도록 설정 (자연스러운 관성)
                float targetRotZ = -horizontalVelocity * tiltStrength;

                // 각도 제한 
                targetRotZ = Mathf.Clamp(targetRotZ, -maxTiltAngle, maxTiltAngle);

                // 부드럽게 회전 적용 
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetRotZ);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);

                // 우클릭 시 즉시 취소
                if (Input.GetMouseButtonDown(1))
                {
                    ReturnToHand();
                }
            }
        }

        public void Setup(Card card)
        {
            if (card == null) return;
            Card = card;
            if (titleText != null) titleText.text = card.Title;
            if (descriptionText != null) descriptionText.text = card.Description;
            if (manaText != null) manaText.text = card.Mana.ToString();
            if (cardArtImage != null && card.Image != null) cardArtImage.sprite = card.Image;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsHoverPreview || isPickedUp || isDragging || AnyCardPickedUp) return;
            transform.SetAsLastSibling();
            if (CardViewHoverSystem.Instance != null)
                CardViewHoverSystem.Instance.Show(Card, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (this == null) return;
            if (IsHoverPreview || isPickedUp || isDragging || AnyCardPickedUp) return;
            if (CardViewHoverSystem.Instance != null) CardViewHoverSystem.Instance.Hide();
            if (cachedHandView != null) StartCoroutine(cachedHandView.UpdateCardPositions(0.15f));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if(ActionSystem.Instance.IsPerforming)
            {

                return;
            }

            // 이미 집어든 상태에서 다시 클릭한 경우
            if (isPickedUp)
            {
                TryPlayCard();
                return;
            }

            // 새로 드래그/클릭 시작
            isDragging = true;
            AnyCardPickedUp = true;
            pointerDownMousePos = Input.mousePosition;
            
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            
            // 
            transform.DOKill();
            transform.rotation = Quaternion.identity; 
            transform.SetAsLastSibling();

            if (cachedHandView != null) cachedHandView.RemoveCard(Card);
            if (CardViewHoverSystem.Instance != null) CardViewHoverSystem.Instance.Hide();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!isDragging) return;

            isDragging = false;
            float dragDistance = Vector3.Distance(Input.mousePosition, pointerDownMousePos);

            //  드래그인 경우 (충분히 움직였다면)
            if (dragDistance > clickThreshold)
            {
                TryPlayCard();
            }
            // 단순 클릭인 경우 (제자리라면)
            else
            {
                isPickedUp = true;
                // isPickedUp이 true이므로 AnyCardPickedUp은 true 유지
            }
        }

        private void TryPlayCard()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z; 
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(mousePos);

            Collider2D hit = Physics2D.OverlapPoint(worldPoint, dropLayer);
            
            if (hit != null)
            {
                isPickedUp = false;
                isDragging = false;
                AnyCardPickedUp = false;
                ActionSystem.Instance.Perform(new PlayCardGA(Card));
            }
            else
            {
                ReturnToHand();
            }
        }

        private void ReturnToHand()
        {
            isPickedUp = false;
            isDragging = false;
            AnyCardPickedUp = false;
            if (cachedHandView != null) StartCoroutine(cachedHandView.AddCard(this));
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
        }
    }
}
