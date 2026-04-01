using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxCollider2DByCameraZoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private BoxCollider2D targetCollider;

    [Header("Scaling")]
    [SerializeField] private float baseOrthographicSize = 5f;
    [SerializeField] private Vector2 baseColliderSize = Vector2.one;
    [SerializeField] private bool clampScale = true;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCollider == null) targetCollider = GetComponent<BoxCollider2D>();

        if (targetCollider != null && baseColliderSize == Vector2.one)
        {
            baseColliderSize = targetCollider.size;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || targetCollider == null) return;
        if (!targetCamera.orthographic) return;
        if (baseOrthographicSize <= 0f) return;

        float scale = targetCamera.orthographicSize / baseOrthographicSize;
        if (clampScale)
        {
            scale = Mathf.Clamp(scale, minScale, maxScale);
        }

        targetCollider.size = baseColliderSize * scale;
    }

    [ContextMenu("Use Current Values As Base")]
    private void UseCurrentValuesAsBase()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCollider == null) targetCollider = GetComponent<BoxCollider2D>();
        if (targetCamera == null || targetCollider == null) return;

        baseOrthographicSize = targetCamera.orthographicSize;
        baseColliderSize = targetCollider.size;
    }
}
