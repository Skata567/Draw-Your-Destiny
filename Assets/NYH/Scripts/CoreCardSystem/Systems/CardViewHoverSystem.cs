using NYH.CoreCardSystem;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

    protected override void Awake()
    {
        base.Awake();
        if (cardViewHover != null)
        {
            cardViewHover.IsHoverPreview = true;
        }
    }

    public void Show(Card card, Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);

        // 1. ī���� RectTransform���� ũ�� ������ �����ɴϴ�.
        RectTransform rect = cardViewHover.GetComponent<RectTransform>();

        // ī���� ����/���� ���� ���̸� ���մϴ� (Scale�� 1�� �� ����)
        float halfWidth = (rect.rect.width * rect.lossyScale.x) / 2f;
        float halfHeight = (rect.rect.height * rect.lossyScale.y) / 2f;

        Vector3 targetPos = position;

        // 3. ȭ�� ��谪 ��� (0 + ����ũ�� ~ ȭ��ʺ� - ����ũ��)
        float minX = halfWidth;
        float maxX = Screen.width - halfWidth;
        float minY = halfHeight;
        float maxY = Screen.height - halfHeight;

        // 4. ���� ��ġ ���� (Clamp)
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        cardViewHover.transform.position = targetPos;
    }
    public void Hide()
    {
        if (cardViewHover != null)
        {
            cardViewHover.gameObject.SetActive(false);
        }
    }
}
