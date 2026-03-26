using System.Collections.Generic;
using UnityEngine;

public class DustAnimation : MonoBehaviour
{
    private const int SheetColumns = 6;
    private const int SheetRows = 6;
    private const float FrameRate = 72f;
    private const float PixelsPerUnit = 32f;
    private const int SortingOrder = 2000;

    private static readonly string ResourcePath = "Sprites/dustparticle";

    private SpriteRenderer spriteRenderer;
    private readonly List<Sprite> frames = new List<Sprite>();
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingOrder = SortingOrder;


        LoadFrames();
        if (frames.Count > 0)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (frames.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        int frameIndex = Mathf.FloorToInt(timer * FrameRate);
        if (frameIndex >= frames.Count)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = frames[frameIndex];
    }

    private void LoadFrames()
    {
        Texture2D texture = Resources.Load<Texture2D>(ResourcePath);
        if (texture == null)
        {
            return;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float cellWidth = texture.width / (float)SheetColumns;
        float cellHeight = texture.height / (float)SheetRows;

        for (int row = 0; row < SheetRows; row++)
        {
            for (int col = 0; col < SheetColumns; col++)
            {
                Rect rect = new Rect(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit);
                frames.Add(sprite);
            }
        }
    }
}