using System.Collections.Generic;
using UnityEngine;

public class IsometricFieldGenerator : MonoBehaviour
{
    private const float RockScale = 1.5f;
    private static readonly Vector3 RockScaleVector = new Vector3(RockScale, RockScale, 1f);
    private static readonly Vector3 RockOffsetVector = new Vector3(0f, 0.1f, 1f);

    private const int RockSheetColumns = 3;
    private const int RockSheetRows = 2;
    private const float RockDurability = 3f;
    private const int RockSortingOffset = 1000;

    [Header("Grid Size")]
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 15;

    [Header("Tile Sprite")]
    [SerializeField] private int tilePixelWidth = 64;
    [SerializeField] private int tilePixelHeight = 32;
    [SerializeField] private float pixelsPerUnit = 32f;
    [SerializeField] private Sprite tileSprite;

    [Header("Rocks")]
    [SerializeField] private int rockCount = 40;
    [SerializeField] private Sprite rockSpriteSimple;
    [SerializeField] private Sprite rockSpriteGoldDots;
    [SerializeField] private Sprite rockSpriteGold;

    [Header("Camera Fit")]
    [SerializeField] private bool fitCameraOnStart = true;
    [SerializeField] private float cameraPadding = 1.0f;

    private readonly List<TileInfo> tiles = new List<TileInfo>();
    private readonly List<Sprite> rockSimpleVariants = new List<Sprite>();
    private readonly List<Sprite> rockGoldDotsVariants = new List<Sprite>();
    private readonly List<Sprite> rockGoldVariants = new List<Sprite>();

    private readonly List<Sprite> rockSimpleCrackedVariants = new List<Sprite>();
    private readonly List<Sprite> rockGoldDotsCrackedVariants = new List<Sprite>();
    private readonly List<Sprite> rockGoldCrackedVariants = new List<Sprite>();

    private struct TileInfo
    {
        public Vector3 Position;
        public int SortingOrder;

        public TileInfo(Vector3 position, int sortingOrder)
        {
            Position = position;
            SortingOrder = sortingOrder;
        }
    }

    private void Awake()
    {
        if (tileSprite == null)
        {
            tileSprite = LoadSpriteFromResources("Sprites/grass_iso");
        }

        rockSimpleVariants.Clear();
        rockSimpleVariants.AddRange(LoadSpriteSheetFromResources("Sprites/rocks_shadow", RockSheetColumns, RockSheetRows));

        rockGoldDotsVariants.Clear();
        rockGoldDotsVariants.AddRange(LoadSpriteSheetFromResources("Sprites/rocks_shadow_gold_dots", RockSheetColumns, RockSheetRows));

        rockGoldVariants.Clear();
        rockGoldVariants.AddRange(LoadSpriteSheetFromResources("Sprites/rocks_shadow_gold", RockSheetColumns, RockSheetRows));

        rockSimpleCrackedVariants.Clear();
        rockSimpleCrackedVariants.AddRange(LoadSpriteSheetFromResources("Sprites/rocks_shadow_cracked", RockSheetColumns, RockSheetRows));

        rockGoldDotsCrackedVariants.Clear();
        rockGoldDotsCrackedVariants.AddRange(LoadSpriteSheetFromResources("Sprites/rocks_shadow_gold_dots_cracked", RockSheetColumns, RockSheetRows));

        rockGoldCrackedVariants.Clear();
        rockGoldCrackedVariants.AddRange(LoadSpriteSheetFromResources("Sprites/rocks_shadow_gold_cracked", RockSheetColumns, RockSheetRows));

        if (rockSpriteSimple == null)
        {
            rockSpriteSimple = LoadSpriteFromResources("Sprites/rock_simple");
        }

        if (rockSpriteGoldDots == null)
        {
            rockSpriteGoldDots = LoadSpriteFromResources("Sprites/rock_gold_dots");
        }

        if (rockSpriteGold == null)
        {
            rockSpriteGold = LoadSpriteFromResources("Sprites/rock_gold");
        }
    }

    private void Start()
    {
        ResetLevel();
        if (fitCameraOnStart)
        {
            FitCamera();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
            if (fitCameraOnStart)
            {
                FitCamera();
            }
        }
    }

    private void ResetLevel()
    {
        ClearChildren();
        GenerateTiles();
        SpawnRocks();

        CoinCounter counter = CoinCounter.Instance;
        if (counter != null)
        {
            counter.ResetCoins();
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void GenerateTiles()
    {
        tiles.Clear();
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

                Vector3 position = new Vector3(isoX, isoY, 0f);
                int order = sortingBase + (x + y);
                tiles.Add(new TileInfo(position, order));

                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = position;

                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = tileSprite;
                renderer.sortingOrder = order;
            }
        }
    }

    private void SpawnRocks()
    {
        if (tiles.Count == 0)
        {
            return;
        }

        if (rockSimpleVariants.Count == 0 && rockGoldDotsVariants.Count == 0 && rockGoldVariants.Count == 0 &&
            rockSpriteSimple == null && rockSpriteGoldDots == null && rockSpriteGold == null)
        {
            return;
        }

        ShuffleTiles();
        int spawnCount = Mathf.Min(rockCount, tiles.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            TileInfo tile = tiles[i];
            RockSpawnInfo spawnInfo = PickWeightedRockSpawn();
            if (spawnInfo.Sprite == null)
            {
                continue;
            }

            GameObject rock = new GameObject($"Rock_{i}");
            rock.transform.SetParent(transform, false);
            rock.transform.localPosition = tile.Position + RockOffsetVector;
            rock.transform.localScale = RockScaleVector;

            SpriteRenderer renderer = rock.AddComponent<SpriteRenderer>();
            renderer.sprite = spawnInfo.Sprite;
            renderer.sortingOrder = RockSortingOffset + tile.SortingOrder;

            Rock rockComponent = rock.AddComponent<Rock>();
            rockComponent.Initialize(
                spawnInfo.Type,
                spawnInfo.SpriteIndex,
                spawnInfo.NormalVariants,
                spawnInfo.CrackedVariants,
                RockDurability);
        }
    }

    private RockSpawnInfo PickWeightedRockSpawn()
    {
        int roll = Random.Range(0, 100);
        if (roll < 70)
        {
            return PickSimpleRockSpawn();
        }
        if (roll < 90)
        {
            return PickGoldDotsRockSpawn();
        }
        return PickGoldRockSpawn();
    }

    private RockSpawnInfo PickSimpleRockSpawn()
    {
        int index = PickVariantIndex(rockSimpleVariants.Count);
        Sprite sprite = PickSpriteFromVariants(rockSimpleVariants, rockSpriteSimple, index);
        Sprite[] normal = rockSimpleVariants.ToArray();
        Sprite[] cracked = rockSimpleCrackedVariants.ToArray();
        return new RockSpawnInfo(Rock.RockType.Plain, index, sprite, normal, cracked);
    }

    private RockSpawnInfo PickGoldDotsRockSpawn()
    {
        int index = PickVariantIndex(rockGoldDotsVariants.Count);
        Sprite sprite = PickSpriteFromVariants(rockGoldDotsVariants, rockSpriteGoldDots, index);
        Sprite[] normal = rockGoldDotsVariants.ToArray();
        Sprite[] cracked = rockGoldDotsCrackedVariants.ToArray();
        return new RockSpawnInfo(Rock.RockType.Dot, index, sprite, normal, cracked);
    }

    private RockSpawnInfo PickGoldRockSpawn()
    {
        int index = PickVariantIndex(rockGoldVariants.Count);
        Sprite sprite = PickSpriteFromVariants(rockGoldVariants, rockSpriteGold, index);
        Sprite[] normal = rockGoldVariants.ToArray();
        Sprite[] cracked = rockGoldCrackedVariants.ToArray();
        return new RockSpawnInfo(Rock.RockType.Gold, index, sprite, normal, cracked);
    }

    private int PickVariantIndex(int count)
    {
        if (count <= 0)
        {
            return 0;
        }
        return Random.Range(0, count);
    }

    private Sprite PickSpriteFromVariants(List<Sprite> variants, Sprite fallback, int index)
    {
        if (variants.Count > 0)
        {
            int safeIndex = Mathf.Clamp(index, 0, variants.Count - 1);
            return variants[safeIndex];
        }
        return fallback;
    }

    private void ShuffleTiles()
    {
        for (int i = tiles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            TileInfo temp = tiles[i];
            tiles[i] = tiles[j];
            tiles[j] = temp;
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

    private Sprite LoadSpriteFromResources(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
    }

    private List<Sprite> LoadSpriteSheetFromResources(string resourcePath, int columns, int rows)
    {
        List<Sprite> sprites = new List<Sprite>();
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null || columns <= 0 || rows <= 0)
        {
            return sprites;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float cellWidth = texture.width / (float)columns;
        float cellHeight = texture.height / (float)rows;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Rect rect = new Rect(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
                sprites.Add(sprite);
            }
        }

        return sprites;
    }

    private readonly struct RockSpawnInfo
    {
        public readonly Rock.RockType Type;
        public readonly int SpriteIndex;
        public readonly Sprite Sprite;
        public readonly Sprite[] NormalVariants;
        public readonly Sprite[] CrackedVariants;

        public RockSpawnInfo(Rock.RockType type, int spriteIndex, Sprite sprite, Sprite[] normalVariants, Sprite[] crackedVariants)
        {
            Type = type;
            SpriteIndex = spriteIndex;
            Sprite = sprite;
            NormalVariants = normalVariants;
            CrackedVariants = crackedVariants;
        }
    }
}