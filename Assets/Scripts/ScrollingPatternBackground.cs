using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scrolls a repeating pattern across a UI Image using UV offsets (see UI/ScrollingTiledPattern).
/// The pattern fills the whole canvas at any resolution without stretching the source image:
/// the tile keeps its square aspect and scales with the screen height, and the scroll wraps
/// seamlessly forever instead of moving a fixed-size RectTransform.
/// </summary>
[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class ScrollingPatternBackground : MonoBehaviour
{
    [Tooltip("Image that renders the pattern. Defaults to the Image on this GameObject.")]
    public Image targetImage;

    [Tooltip("Material using the UI/ScrollingTiledPattern shader. A runtime copy is created from it.")]
    public Material patternMaterial;

    [Header("Tiling")]
    [Tooltip("Tile size in screen pixels at the reference resolution height.")]
    public float tileSizeInPixels = 150f;

    [Tooltip("Screen height the tile size and scroll speed are authored for.")]
    public float referenceHeight = 720f;

    [Header("Scroll")]
    [Tooltip("Scroll speed in screen pixels per second at the reference resolution height. The original background scrolled 100 px/s.")]
    public Vector2 scrollSpeed = new Vector2(100f, 100f);

    private static readonly int TilesId = Shader.PropertyToID("_Tiles");
    private static readonly int ScrollId = Shader.PropertyToID("_Scroll");

    private Material runtimeMaterial;
    private Canvas canvas;
    private Vector2 scrollOffset;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        canvas = GetComponentInParent<Canvas>();

        Material source = patternMaterial != null ? patternMaterial : targetImage.material;
        if (source != null && source.HasProperty(TilesId))
        {
            runtimeMaterial = new Material(source) { name = source.name + " (Runtime)" };
            runtimeMaterial.hideFlags = HideFlags.DontSave;
            targetImage.material = runtimeMaterial;
        }
        else
        {
            Debug.LogWarning("ScrollingPatternBackground: assign a material using the UI/ScrollingTiledPattern shader.", this);
        }
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }

    private void Update()
    {
        if (runtimeMaterial == null || targetImage == null)
        {
            return;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }
        }

        Vector2 screenSize = canvas.pixelRect.size;
        if (screenSize.x <= 0f || screenSize.y <= 0f)
        {
            return;
        }

        // Scale the tile with the screen height so the pattern looks the same at every resolution.
        float resolutionScale = screenSize.y / Mathf.Max(1f, referenceHeight);
        float tilePixels = Mathf.Max(1f, tileSizeInPixels * resolutionScale);

        Vector2 tiles = screenSize / tilePixels;

        // Scroll in tile units, wrapped to keep precision over long sessions.
        scrollOffset += scrollSpeed * (resolutionScale * Time.deltaTime) / tilePixels;
        scrollOffset.x = Mathf.Repeat(scrollOffset.x, 1f);
        scrollOffset.y = Mathf.Repeat(scrollOffset.y, 1f);

        runtimeMaterial.SetVector(TilesId, new Vector4(tiles.x, tiles.y, 0f, 0f));
        runtimeMaterial.SetVector(ScrollId, new Vector4(scrollOffset.x, scrollOffset.y, 0f, 0f));
    }
}
