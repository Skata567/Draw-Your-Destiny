using NYH.CoreCardSystem;
using UnityEngine;

/// <summary>
/// 마우스를 카드 위에 올렸을 때(Hover), 해당 카드를 크게 보여주는 미리보기 시스템입니다.
/// 씬에 미리 배치된 'CardViewHover' 오브젝트를 활성화하고 위치를 조절합니다.
/// </summary>
public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [Header("Preview UI")]
    [SerializeField] private CardView cardViewHover; // 크게 보여줄 미리보기용 카드 프리팹

    protected override void Awake()
    {
        base.Awake();
        if (cardViewHover != null)
        {
            // 이 카드 뷰는 '미리보기 전용'임을 설정하여 드래그나 클릭이 안 되게 합니다.
            cardViewHover.IsHoverPreview = true;
            cardViewHover.gameObject.SetActive(false); // 처음엔 숨김
        }
    }

    /// <summary>
    /// [사용법] CardView.OnPointerEnter에서 호출하여 미리보기를 띄웁니다.
    /// </summary>
    /// <param name="card">보여줄 카드 데이터</param>
    /// <param name="position">현재 마우스 혹은 원래 카드의 위치</param>
    public void Show(Card card, Vector3 position)
    {
        if (cardViewHover == null) return;

        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card); 

        // [중요] 미리보기 카드가 마우스 커서를 가려서 호버가 풀리는(깜빡이는) 현상을 방지합니다.
        // 이 카드의 모든 이미지 컴포넌트에서 Raycast Target을 꺼버립니다.
        var images = cardViewHover.GetComponentsInChildren<UnityEngine.UI.Graphic>();
        foreach (var img in images) img.raycastTarget = false;

        // --- 화면 밖으로 나가지 않게 위치 계산 ---
        RectTransform rect = cardViewHover.GetComponent<RectTransform>();

        // 미리보기 카드의 실제 가로/세로 절반 크기를 구합니다.
        float halfWidth = (rect.rect.width * rect.lossyScale.x) / 2f;
        float halfHeight = (rect.rect.height * rect.lossyScale.y) / 2f;

        Vector3 targetPos = position;

        // 화면 경계값 계산 (카드가 화면 끝에 걸리지 않도록)
        float minX = halfWidth;
        float maxX = Screen.width - halfWidth;
        float minY = halfHeight;
        float maxY = Screen.height - halfHeight;

        // 최종 위치를 화면 안쪽으로 제한(Clamp)합니다.
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        cardViewHover.transform.position = targetPos;
    }

    /// <summary>
    /// [사용법] CardView.OnPointerExit에서 호출하여 미리보기를 숨깁니다.
    /// </summary>
    public void Hide()
    {
        if (cardViewHover != null)
        {
            cardViewHover.gameObject.SetActive(false);
        }
    }
}
