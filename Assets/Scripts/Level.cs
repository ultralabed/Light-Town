using System.Collections.Generic;
using UnityEngine;

/// <summary>可以站的東西（地板、平台）。trap = true 表示踩上去會掉下去。</summary>
public class Solid
{
    public Rect rect;
    public Transform tf;
    public bool on = true;     // false = 已經掉了，不能站
    public bool trap;
    public bool triggered;
    public float timer;
    public float vy;
}

/// <summary>黑傘幫：在 x0 和 x1 之間來回走。</summary>
public class Patrol
{
    public float x0, x1, speed, x;
    public int dir = 1;
    public Transform tf;
    public Rect Rect { get { return new Rect(x - 0.35f, 0f, 0.7f, 1.2f); } }
}

public class SignPost
{
    public Vector2 pos;
    public string text;
}

/// <summary>
/// 一個關卡。Levels.cs 裡用下面這些「積木」把關卡排出來：
/// Ground / FallingFloor / Platform / Enemy / Lantern / StartDoor / ExitDoor / Sign / Stall。
/// 座標單位：1 大約是主角的寬度。地面的高度是 y = 0。
/// </summary>
public class Level
{
    public Transform root;
    public string name = "";

    public readonly List<Solid> solids = new List<Solid>();
    public readonly List<Patrol> enemies = new List<Patrol>();
    public readonly List<SignPost> signs = new List<SignPost>();

    public float minX = float.MaxValue;
    public float maxX = float.MinValue;

    public float startX;
    public float exitX;
    public SpriteRenderer exitDoor;

    public bool hasLantern;
    public bool lanternTaken;
    public Vector2 lanternPos;
    public Transform lanternTf;

    public bool secretReturn; // 拿到燈後走回起點的門 → 解開真結局

    static readonly Color GroundColor = new Color(0.20f, 0.18f, 0.26f);
    static readonly Color GroundTop = new Color(0.42f, 0.38f, 0.50f);
    public static readonly Color DoorLocked = new Color(0.45f, 0.45f, 0.48f);
    public static readonly Color DoorOpen = new Color(1.00f, 0.80f, 0.20f);

    public Level(Transform root)
    {
        this.root = root;
    }

    // ---------------------------------------------------------------- 積木

    public void Name(string levelName)
    {
        name = levelName;
    }

    /// <summary>地板，從 x0 到 x1。兩段地板中間留空 = 洞。</summary>
    public void Ground(float x0, float x1)
    {
        AddFloor(x0, x1, false);
    }

    /// <summary>整人地板：看起來一樣，踩上去就掉。</summary>
    public void FallingFloor(float x0, float x1)
    {
        AddFloor(x0, x1, true);
    }

    void AddFloor(float x0, float x1, bool trap)
    {
        var s = new Solid();
        s.rect = new Rect(x0, -4f, x1 - x0, 4f);
        s.trap = trap;
        s.tf = Draw.Group(trap ? "FallingFloor" : "Ground", root, (x0 + x1) * 0.5f, 0f);
        Draw.Box(s.tf, 0f, -2f, x1 - x0, 4f, GroundColor, 0);
        Draw.Box(s.tf, 0f, -0.1f, x1 - x0, 0.2f, GroundTop, 1);
        solids.Add(s);
        minX = Mathf.Min(minX, x0);
        maxX = Mathf.Max(maxX, x1);
    }

    /// <summary>浮在空中的平台（貨箱、屋頂都可以用這個）。y 是平台頂端的高度。</summary>
    public void Platform(float x0, float x1, float y, float thickness = 0.5f)
    {
        var s = new Solid();
        s.rect = new Rect(x0, y - thickness, x1 - x0, thickness);
        s.tf = Draw.Group("Platform", root, (x0 + x1) * 0.5f, y);
        Draw.Box(s.tf, 0f, -thickness * 0.5f, x1 - x0, thickness, GroundColor, 0);
        Draw.Box(s.tf, 0f, -0.08f, x1 - x0, 0.16f, GroundTop, 1);
        solids.Add(s);
    }

    /// <summary>黑傘幫，在 x0 和 x1 之間巡邏。</summary>
    public void Enemy(float x0, float x1, float speed)
    {
        var e = new Patrol();
        e.x0 = Mathf.Min(x0, x1);
        e.x1 = Mathf.Max(x0, x1);
        e.speed = speed;
        e.x = e.x0;
        e.tf = Draw.Group("BlackUmbrella", root, e.x, 0f);
        Color black = new Color(0.05f, 0.05f, 0.07f);
        Draw.Box(e.tf, 0f, 0.45f, 0.6f, 0.9f, black, 6);                        // 身體
        Draw.Box(e.tf, 0.42f, 0.85f, 0.06f, 0.9f, new Color(0.4f, 0.4f, 0.4f), 5); // 傘柄
        Draw.Circle(e.tf, 0.1f, 1.35f, 1.5f, 0.7f, black, 7);                   // 傘
        Draw.Box(e.tf, -0.12f, 0.65f, 0.1f, 0.1f, Color.white, 8);              // 眼睛
        Draw.Box(e.tf, 0.12f, 0.65f, 0.1f, 0.1f, Color.white, 8);
        enemies.Add(e);
    }

    /// <summary>這一關的燈。</summary>
    public void Lantern(float x, float y)
    {
        hasLantern = true;
        lanternTaken = false;
        lanternPos = new Vector2(x, y);
        lanternTf = Draw.Group("Lantern", root, x, y);
        Draw.Circle(lanternTf, 0f, 0f, 1.8f, 1.8f, new Color(1f, 0.85f, 0.3f, 0.22f), 3);
        Draw.Circle(lanternTf, 0f, 0f, 0.7f, 0.8f, new Color(1f, 0.62f, 0.15f), 4);
        Draw.Circle(lanternTf, 0f, 0f, 0.35f, 0.45f, new Color(1f, 0.95f, 0.6f), 5);
        Draw.Box(lanternTf, 0f, 0.42f, 0.3f, 0.1f, new Color(0.55f, 0.12f, 0.1f), 5);
        Draw.Box(lanternTf, 0f, -0.42f, 0.3f, 0.1f, new Color(0.55f, 0.12f, 0.1f), 5);
    }

    /// <summary>起點的門（主角從這裡出來）。</summary>
    public void StartDoor(float x)
    {
        startX = x;
        MakeDoor("StartDoor", x, new Color(0.30f, 0.26f, 0.36f));
    }

    /// <summary>出口的門。拿到燈之前是灰色（鎖著），拿到燈變金色。</summary>
    public void ExitDoor(float x)
    {
        exitX = x;
        exitDoor = MakeDoor("ExitDoor", x, DoorLocked);
    }

    SpriteRenderer MakeDoor(string doorName, float x, Color color)
    {
        Transform t = Draw.Group(doorName, root, x, 0f);
        Draw.Box(t, 0f, 1.1f, 1.5f, 2.2f, new Color(0.10f, 0.09f, 0.14f), 1);
        SpriteRenderer door = Draw.Box(t, 0f, 1.0f, 1.2f, 2.0f, color, 2);
        Draw.Circle(t, 0.4f, 1.0f, 0.14f, 0.14f, new Color(0.1f, 0.1f, 0.1f), 3);
        return door;
    }

    /// <summary>告示牌。</summary>
    public void Sign(float x, string text)
    {
        Transform t = Draw.Group("Sign", root, x, 0f);
        Draw.Box(t, 0f, 0.5f, 0.1f, 1.0f, new Color(0.45f, 0.32f, 0.2f), 1);
        Draw.Box(t, 0f, 1.05f, 0.9f, 0.5f, new Color(0.75f, 0.6f, 0.38f), 2);
        var s = new SignPost();
        s.pos = new Vector2(x, 1.5f);
        s.text = text;
        signs.Add(s);
    }

    /// <summary>夜市攤位（只是裝飾，不會擋路）。</summary>
    public void Stall(float x, Color awning)
    {
        Transform t = Draw.Group("Stall", root, x, 0f);
        Draw.Box(t, 0f, 0.6f, 2.4f, 1.2f, new Color(0.16f, 0.14f, 0.22f), -5);
        Draw.Box(t, -1.1f, 1.5f, 0.1f, 0.8f, new Color(0.16f, 0.14f, 0.22f), -5);
        Draw.Box(t, 1.1f, 1.5f, 0.1f, 0.8f, new Color(0.16f, 0.14f, 0.22f), -5);
        Draw.Box(t, 0f, 2.1f, 2.8f, 0.5f, awning, -4);
        Draw.Circle(t, -0.7f, 1.65f, 0.25f, 0.25f, new Color(1f, 0.9f, 0.5f, 0.9f), -4);
        Draw.Circle(t, 0f, 1.65f, 0.25f, 0.25f, new Color(1f, 0.9f, 0.5f, 0.9f), -4);
        Draw.Circle(t, 0.7f, 1.65f, 0.25f, 0.25f, new Color(1f, 0.9f, 0.5f, 0.9f), -4);
    }

    /// <summary>
    /// 這一關藏著真結局：拿到燈之後走回起點的門就會解開（預計用在第 4 關）。
    /// </summary>
    public void SecretReturn()
    {
        secretReturn = true;
    }
}
