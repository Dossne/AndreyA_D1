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
    [SerializeField] private Sprite tileSprite;

    [Header("Camera Fit")]
    [SerializeField] private bool fitCameraOnStart = true;
    [SerializeField] private float cameraPadding = 1.0f;

    private void Awake()
    {
        if (tileSprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Sprites/grass_iso");
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                tileSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    pixelsPerUnit);
            }
        }
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
        if (tileSprite == null)
        {
            return;
        }

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
}