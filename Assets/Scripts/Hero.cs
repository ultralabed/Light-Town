using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主角的樣子：一個原創的卡通小孩（圓頭、大眼睛、紅圍巾、藍綠色外套）。
/// 全部用圓形和方塊拼出來，之後要換成真的美術圖，改這個檔案就好。
/// 碰撞大小沒有變（寬 0.8、高 1.2），這裡只管「看起來的樣子」和小動畫。
/// </summary>
public class Hero
{
    public readonly Transform root;

    readonly List<SpriteRenderer> parts = new List<SpriteRenderer>();
    readonly List<Color> colors = new List<Color>();
    readonly Transform legL, legR, armL, armR, head, scarfTail;
    float walk;
    int facing = 1;

    static readonly Color Skin = new Color(1.00f, 0.87f, 0.74f);
    static readonly Color Hair = new Color(0.20f, 0.14f, 0.22f);
    static readonly Color Coat = new Color(0.30f, 0.82f, 0.78f);
    static readonly Color CoatDark = new Color(0.20f, 0.62f, 0.62f);
    static readonly Color Scarf = new Color(0.93f, 0.27f, 0.27f);
    static readonly Color Shoe = new Color(0.16f, 0.18f, 0.34f);
    static readonly Color Ink = new Color(0.10f, 0.08f, 0.14f);

    public Hero()
    {
        root = Draw.Group("Player", null, 0f, 0f);

        // 腳
        legL = Draw.Group("LegL", root, -0.15f, 0f);
        Add(Draw.Box(legL, 0f, 0.14f, 0.16f, 0.28f, Shoe, 10));
        Add(Draw.Box(legL, 0.04f, 0.04f, 0.24f, 0.09f, Shoe, 10));
        legR = Draw.Group("LegR", root, 0.15f, 0f);
        Add(Draw.Box(legR, 0f, 0.14f, 0.16f, 0.28f, Shoe, 10));
        Add(Draw.Box(legR, 0.04f, 0.04f, 0.24f, 0.09f, Shoe, 10));

        // 後面那隻手、圍巾尾巴
        armL = Draw.Group("ArmL", root, -0.30f, 0.50f);
        Add(Draw.Circle(armL, 0f, 0f, 0.18f, 0.34f, CoatDark, 10));
        scarfTail = Draw.Group("ScarfTail", root, -0.30f, 0.66f);
        Add(Draw.Box(scarfTail, 0f, 0f, 0.26f, 0.12f, Scarf, 10));

        // 身體（外套）
        Add(Draw.Circle(root, 0f, 0.48f, 0.66f, 0.62f, Coat, 11));
        Add(Draw.Box(root, 0f, 0.36f, 0.56f, 0.24f, Coat, 11));
        Add(Draw.Circle(root, 0.10f, 0.44f, 0.08f, 0.08f, Ink, 12)); // 釦子

        // 前面那隻手
        armR = Draw.Group("ArmR", root, 0.30f, 0.50f);
        Add(Draw.Circle(armR, 0f, 0f, 0.18f, 0.34f, CoatDark, 14));
        Add(Draw.Circle(armR, 0f, -0.15f, 0.14f, 0.14f, Skin, 14));

        // 圍巾
        Add(Draw.Box(root, 0f, 0.74f, 0.58f, 0.14f, Scarf, 13));

        // 頭
        head = Draw.Group("Head", root, 0f, 1.04f);
        Add(Draw.Circle(head, -0.03f, 0.06f, 0.88f, 0.80f, Hair, 15));        // 頭髮
        Add(Draw.Circle(head, 0.02f, 0.46f, 0.16f, 0.22f, Hair, 15));         // 呆毛
        Add(Draw.Circle(head, 0.05f, -0.03f, 0.74f, 0.66f, Skin, 16));        // 臉
        Add(Draw.Circle(head, 0.02f, 0.01f, 0.21f, 0.26f, Color.white, 17));  // 眼白
        Add(Draw.Circle(head, 0.28f, 0.01f, 0.21f, 0.26f, Color.white, 17));
        Add(Draw.Circle(head, 0.05f, 0.00f, 0.11f, 0.15f, Ink, 18));          // 眼珠
        Add(Draw.Circle(head, 0.31f, 0.00f, 0.11f, 0.15f, Ink, 18));
        Add(Draw.Circle(head, -0.12f, -0.16f, 0.14f, 0.09f, new Color(1f, 0.55f, 0.55f, 0.7f), 17)); // 腮紅
        Add(Draw.Box(head, 0.17f, -0.20f, 0.10f, 0.035f, Ink, 17));           // 嘴巴
    }

    void Add(SpriteRenderer sr)
    {
        parts.Add(sr);
        colors.Add(sr.color);
    }

    /// <summary>每一幀更新位置和小動畫。</summary>
    public void Animate(float x, float y, float vx, float vy, bool grounded, float dt)
    {
        if (vx > 0.01f) facing = 1;
        else if (vx < -0.01f) facing = -1;

        root.position = new Vector3(x, y, 0f);
        root.localScale = new Vector3(facing, 1f, 1f);

        float legA = 0f, legB = 0f, arm = 0f, bob = 0f, tail = 0f;
        if (!grounded)
        {
            // 在空中：腳縮起來、手舉高、圍巾飄起來
            legA = 0.10f;
            legB = 0.04f;
            arm = vy > 0f ? 0.16f : 0.08f;
            tail = 0.08f;
        }
        else if (Mathf.Abs(vx) > 0.01f)
        {
            // 走路：兩隻腳輪流抬
            walk += dt * 16f;
            float s = Mathf.Sin(walk);
            legA = Mathf.Max(0f, s) * 0.12f;
            legB = Mathf.Max(0f, -s) * 0.12f;
            arm = s * 0.05f;
            bob = Mathf.Abs(s) * 0.03f;
            tail = 0.04f + s * 0.02f;
        }
        else
        {
            // 站著：輕輕呼吸
            bob = Mathf.Sin(Time.time * 3f) * 0.012f;
        }

        legL.localPosition = new Vector3(-0.15f, legA, 0f);
        legR.localPosition = new Vector3(0.15f, legB, 0f);
        armL.localPosition = new Vector3(-0.30f, 0.50f - arm + (grounded ? 0f : arm * 2f), 0f);
        armR.localPosition = new Vector3(0.30f, 0.50f + arm, 0f);
        head.localPosition = new Vector3(0f, 1.04f + bob, 0f);
        scarfTail.localPosition = new Vector3(-0.36f - tail, 0.66f + tail, 0f);
    }

    /// <summary>被抓到：整個人變紅。</summary>
    public void Hurt()
    {
        for (int i = 0; i < parts.Count; i++)
        {
            Color c = colors[i];
            parts[i].color = new Color(1f, c.g * 0.35f, c.b * 0.35f, c.a);
        }
    }
}
