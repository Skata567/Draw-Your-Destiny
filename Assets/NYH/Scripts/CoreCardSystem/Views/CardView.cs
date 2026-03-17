namespace NYH.CoreCardSystem
{
    using TMPro;
    using UnityEngine;

    public class CardView: MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text mana;
        [SerializeField] private SpriteRenderer imageSr;
        [SerializeField] private GameObject wrapper;
        [SerializeField] private LayerMask dropLayer;

        public Card Card { get; private set; }
        private Vector3 dragStartPosition;
        private Quaternion dragStartRotation;
        private bool isDragging = false;

        public void Setup(Card card)
        {
            Card = card;
            title.text = card.Title;
            description.text = card.Description;
            mana.text = card.Mana.ToString();
            imageSr.sprite = card.Image;
        }

        private void OnMouseDown()
        {
            isDragging = true;
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        void OnMouseDrag()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = -1;
            transform.position = mousePos;
        }

        private void OnMouseUp()
        {
            if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
            {
                PlayCardGA playCardGA = new(Card);
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
            isDragging = false;
        }
    }
}
