using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    private const float CursorRadius = 0.4f;
    private const int CursorPixelsPerUnit = 64;
    private const int CursorSortingOrder = 1100;

    private const float CursorPressedScale = 1.2f;
    private const float CursorScaleSpeed = 10f;

    private static readonly Color CursorColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);

    public static CursorFollower Instance { get; private set; }
    public static float RadiusWorld => CursorRadius;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        Instance = this;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite(CursorRadius, CursorPixelsPerUnit, CursorColor);
        spriteRenderer.sortingOrder = CursorSortingOrder;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 mouse = Input.mousePosition;
        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = 0f;
        transform.position = world;

        float targetScale = Input.GetMouseButton(0) ? CursorPressedScale : 1f;
        Vector3 desired = new Vector3(targetScale, targetScale, 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, desired, CursorScaleSpeed * Time.deltaTime);
    }

    private static Sprite CreateCircleSprite(float radiusWorld, int ppu, Color color)
    {
        if (radiusWorld <= 0f || ppu <= 0)
        {
            return null;
        }

        int pixelRadius = Mathf.Max(1, Mathf.RoundToInt(radiusWorld * ppu));
        int size = pixelRadius * 2 + 1;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(pixelRadius + 0.5f, pixelRadius + 0.5f);
        float radiusSq = (pixelRadius + 0.5f) * (pixelRadius + 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center.x;
                float dy = y + 0.5f - center.y;
                float distSq = dx * dx + dy * dy;
                texture.SetPixel(x, y, distSq <= radiusSq ? color : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
    }
}