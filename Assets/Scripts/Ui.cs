using UnityEngine;

/// <summary>
/// 畫面上的字和按鈕。全部用「虛擬高度 540」的座標，會自動跟著螢幕大小縮放。
/// 只在 OnGUI 的 Repaint 時呼叫。按鈕的點擊是自己算的（不靠 IMGUI 事件），
/// 所以只開新 Input System 也能按。
/// </summary>
public static class Ui
{
    public static Font Font;
    static GUIStyle style;

    public const float H = 540f;
    public static float S { get { return Screen.height / H; } }
    public static float W { get { return Screen.width / S; } }

    /// <summary>滑鼠／手指的位置，換成 UI 座標（左上角是 0,0）。</summary>
    public static Vector2 Pointer
    {
        get { return new Vector2(GameInput.PointerPos.x / S, (Screen.height - GameInput.PointerPos.y) / S); }
    }

    static GUIStyle Style(int size, TextAnchor anchor)
    {
        if (style == null)
        {
            style = new GUIStyle();
            style.font = Font;
            style.wordWrap = true;
            style.richText = false;
        }
        style.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * S));
        style.alignment = anchor;
        return style;
    }

    static Rect Px(Rect r)
    {
        return new Rect(r.x * S, r.y * S, r.width * S, r.height * S);
    }

    public static void Fill(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(Px(r), Texture2D.whiteTexture);
        GUI.color = old;
    }

    public static void Text(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, bool shadow = true)
    {
        GUIStyle st = Style(size, anchor);
        Rect p = Px(r);
        if (shadow)
        {
            float o = Mathf.Max(1f, size * S * 0.06f);
            st.normal.textColor = new Color(0f, 0f, 0f, 0.8f * color.a);
            GUI.Label(new Rect(p.x + o, p.y + o, p.width, p.height), text, st);
        }
        st.normal.textColor = color;
        GUI.Label(p, text, st);
    }

    /// <summary>一行字有多寬（UI 座標）。</summary>
    public static float TextWidth(string text, int size)
    {
        GUIStyle st = Style(size, TextAnchor.MiddleCenter);
        return st.CalcSize(new GUIContent(text)).x / S;
    }

    public static bool Button(Rect r, string text, int size = 22)
    {
        bool hover = r.Contains(Pointer);
        Fill(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), new Color(0f, 0f, 0f, 0.6f));
        Fill(r, hover ? new Color(1f, 0.85f, 0.35f, 1f) : new Color(0.95f, 0.72f, 0.2f, 1f));
        Text(r, text, size, new Color(0.15f, 0.1f, 0.05f, 1f), TextAnchor.MiddleCenter, false);
        if (hover && GameInput.PointerDown)
        {
            GameInput.PointerDown = false; // 一次點擊只算一個按鈕
            return true;
        }
        return false;
    }

    /// <summary>世界座標 → UI 座標。</summary>
    public static Vector2 FromWorld(Camera cam, Vector2 world)
    {
        Vector3 sp = cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0f));
        return new Vector2(sp.x / S, (Screen.height - sp.y) / S);
    }
}
