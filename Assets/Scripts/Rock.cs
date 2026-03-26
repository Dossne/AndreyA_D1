using UnityEngine;

public class Rock : MonoBehaviour
{
    public enum RockType
    {
        Plain,
        Dot,
        Gold
    }

    private const float DamagePerSecond = 4f;
    private const float CrackedThreshold = 1.5f;

    private const int CoinsPlain = 1;
    private const int CoinsDot = 2;
    private const int CoinsGold = 5;

    [SerializeField] private float durability = 3f;

    private SpriteRenderer spriteRenderer;
    private bool isCracked;
    private RockType rockType;
    private int spriteIndex;
    private Sprite[] normalVariants;
    private Sprite[] crackedVariants;

    public void Initialize(RockType type, int index, Sprite[] normal, Sprite[] cracked, float initialDurability)
    {
        rockType = type;
        spriteIndex = Mathf.Max(0, index);
        normalVariants = normal;
        crackedVariants = cracked;
        durability = Mathf.Max(0f, initialDurability);

        ApplySprite(normalVariants, spriteIndex);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        CursorFollower cursor = CursorFollower.Instance;
        if (cursor == null)
        {
            return;
        }

        if (!IsBoundsOverlappingCursor(cursor.transform.position, CursorFollower.RadiusWorld))
        {
            return;
        }

        durability -= DamagePerSecond * Time.deltaTime;
        if (!isCracked && durability <= CrackedThreshold)
        {
            ApplySprite(crackedVariants, spriteIndex);
            isCracked = true;
        }

        if (durability <= 0f)
        {
            AwardCoins();
            SpawnDust();
            Destroy(gameObject);
        }
    }

    private void AwardCoins()
    {
        CoinCounter counter = CoinCounter.Instance;
        if (counter == null)
        {
            return;
        }

        switch (rockType)
        {
            case RockType.Dot:
                counter.AddCoins(CoinsDot);
                break;
            case RockType.Gold:
                counter.AddCoins(CoinsGold);
                break;
            default:
                counter.AddCoins(CoinsPlain);
                break;
        }
    }

    private bool IsBoundsOverlappingCursor(Vector3 cursorWorld, float radiusWorld)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return false;
        }

        Vector2 cursorPos = cursorWorld;
        float radiusSqWorld = radiusWorld * radiusWorld;
        Vector2 closest = spriteRenderer.bounds.ClosestPoint(cursorPos);
        return (closest - cursorPos).sqrMagnitude <= radiusSqWorld;
    }

    private void ApplySprite(Sprite[] variants, int index)
    {
        if (spriteRenderer == null || variants == null || variants.Length == 0)
        {
            return;
        }

        int safeIndex = Mathf.Clamp(index, 0, variants.Length - 1);
        Sprite sprite = variants[safeIndex];
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void SpawnDust()
    {
        GameObject dust = new GameObject("RockDust");
        dust.transform.position = transform.position;
        dust.AddComponent<DustAnimation>();
    }
}