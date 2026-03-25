using UnityEngine;

public class IsometricFieldGenerator : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 15;

    [Header("Tile Sprite")]
    [SerializeField] private int tilePixelWidth = 64;
    [SerializeField] private int tilePixelHeight = 32;
    [SerializeField] private float pixelsPerUnit = 32f;
    [SerializeField] private Color grassColor = new Color(0.25f, 0.7f, 0.25f, 1f);

    [Header("Camera Fit")]
    [SerializeField] private bool fitCameraOnStart = true;
    [SerializeField] private float cameraPadding = 0.5f;

    private Sprite tileSprite;

    private void Awake()
    {
        tileSprite = CreateDiamondSprite(tilePixelWidth, tilePixelHeight, pixelsPerUnit, grassColor);
    }

    private void Start()
    {
        Generate();
        if (fitCameraOnStart)
        {
            FitCamera();
        }
    }

    private void Generate()
    {
        float tileWorldWidth = tilePixelWidth / pixelsPerUnit;
        float tileWorldHeight = tilePixelHeight / pixelsPerUnit;

        float centerX = ((width - 1) - (height - 1)) * tileWorldWidth * 0.25f;
        float centerY = (width + height - 2) * tileWorldHeight * 0.25f;

        int sortingBase = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float isoX = (x - y) * tileWorldWidth * 0.5f - centerX;
                float isoY = (x + y) * tileWorldHeight * 0.5f - centerY;

                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(isoX, isoY, 0f);

                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = tileSprite;
                renderer.sortingOrder = sortingBase + (x + y);
            }
        }
    }

    private void FitCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        float tileWorldWidth = tilePixelWidth / pixelsPerUnit;
        float tileWorldHeight = tilePixelHeight / pixelsPerUnit;

        float worldWidth = (width + height) * tileWorldWidth * 0.5f;
        float worldHeight = (width + height) * tileWorldHeight * 0.5f;

        cam.orthographicSize = worldHeight * 0.5f + cameraPadding;
        cam.transform.position = new Vector3(0f, 0f, cam.transform.position.z);
    }

    private static Sprite CreateDiamondSprite(int pixelWidth, int pixelHeight, float ppu, Color fillColor)
    {
        if (pixelWidth <= 0 || pixelHeight <= 0 || ppu <= 0f)
        {
            return null;
        }

        Texture2D texture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        int cx = pixelWidth / 2;
        int cy = pixelHeight / 2;
        float halfWidth = pixelWidth / 2f;
        float halfHeight = pixelHeight / 2f;

        for (int y = 0; y < pixelHeight; y++)
        {
            for (int x = 0; x < pixelWidth; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - cx) / halfWidth;
                float dy = Mathf.Abs(y + 0.5f - cy) / halfHeight;
                bool inside = dx + dy <= 1f;
                texture.SetPixel(x, y, inside ? fillColor : Color.clear);
            }
        }

        texture.Apply();

        Rect rect = new Rect(0, 0, pixelWidth, pixelHeight);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, rect, pivot, ppu);
    }
}