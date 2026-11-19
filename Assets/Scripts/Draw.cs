using UnityEngine;

/// <summary>
/// 用程式畫方塊和圓形（第一版不用美術圖，之後再把這裡換成真的圖）。
/// </summary>
public static class Draw
{
    static Sprite square;
    static Sprite circle;
    static Material material;

    static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var px = new Color[16];
                for (int i = 0; i < px.Length; i++) px[i] = Color.white;
                tex.SetPixels(px);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return square;
        }
    }

    static Sprite CircleSprite
    {
        get
        {
            if (circle == null)
            {
                const int n = 64;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
                var px = new Color[n * n];
                float r = n * 0.5f;
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        float dx = x + 0.5f - r;
                        float dy = y + 0.5f - r;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = Mathf.Clamp01(r - d); // 邊緣柔一點
                        px[y * n + x] = new Color(1f, 1f, 1f, a);
                    }
                }
                tex.SetPixels(px);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            }
            return circle;
        }
    }

    static SpriteRenderer Make(string name, Sprite sprite, Transform parent, float x, float y, float w, float h, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        // 用不受光的材質，這樣場景裡沒有 2D 燈光也看得到
        if (material == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null) material = new Material(shader);
        }
        if (material != null) sr.sharedMaterial = material;
        return sr;
    }

    /// <summary>畫一個方塊。(x, y) 是中心點，w/h 是寬高。</summary>
    public static SpriteRenderer Box(Transform parent, float x, float y, float w, float h, Color color, int order)
    {
        return Make("Box", Square, parent, x, y, w, h, color, order);
    }

    /// <summary>畫一個圓（或橢圓）。(x, y) 是中心點，w/h 是寬高。</summary>
    public static SpriteRenderer Circle(Transform parent, float x, float y, float w, float h, Color color, int order)
    {
        return Make("Circle", CircleSprite, parent, x, y, w, h, color, order);
    }

    /// <summary>空的群組物件，用來把幾個形狀綁在一起移動。</summary>
    public static Transform Group(string name, Transform parent, float x, float y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(x, y, 0f);
        return go.transform;
    }
}
