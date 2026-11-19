using UnityEngine;

/// <summary>
/// 整人結局：
///   1. 七盞燈飛上天排成一圈，天空變金色
///   2. 「請說出你的願望……」（可以打字，也可以按預設按鈕）
///   3. 隨機四種結局之一（跳電／已讀不回／排隊／貓），最後都是 GAME OVER
///   4. 如果有解開祕密，多一段真結局
///   5. 「再玩一次」
/// </summary>
public class Ending
{
    enum Phase { Fly, Wish, Result }

    const int LampCount = 7;
    const float LineGap = 1.2f;   // 每隔幾秒多出一行字
    const int MaxWish = 16;

    static readonly Color Gold = new Color(0.95f, 0.72f, 0.22f);
    static readonly string[] Presets = { "變超有錢", "考試一百分", "永遠不用上班" };

    readonly Game game;
    readonly Camera cam;
    readonly Transform root;
    readonly Transform[] lamps = new Transform[LampCount];
    readonly Vector2 from;
    readonly Vector2 center;
    readonly bool borrowed; // 還沒做完七關：其他燈是「借的」

    Phase phase = Phase.Fly;
    float t;
    float spin;
    string wish = "";
    int kind;
    string[] lines = new string[0];
    string[] trueLines = new string[0];
    Transform cat;
    float catX;

    public Ending(Game game, Vector2 from)
    {
        this.game = game;
        this.from = from;
        cam = game.Cam;
        borrowed = game.Lanterns < LampCount;
        center = new Vector2(cam.transform.position.x, cam.transform.position.y + 1.9f);
        root = Draw.Group("Ending", null, 0f, 0f);

        for (int i = 0; i < LampCount; i++)
        {
            Transform l = Draw.Group("Lamp" + i, root, from.x, from.y);
            Draw.Circle(l, 0f, 0f, 1.6f, 1.6f, new Color(1f, 0.95f, 0.6f, 0.25f), 20);
            Draw.Circle(l, 0f, 0f, 0.6f, 0.7f, new Color(1f, 0.55f, 0.12f), 21);
            Draw.Circle(l, 0f, 0f, 0.3f, 0.4f, new Color(1f, 0.97f, 0.7f), 22);
            lamps[i] = l;
        }
    }

    public void Dispose()
    {
        GameInput.SetTyping(false);
        if (root != null) Object.Destroy(root.gameObject);
    }

    Vector2 LampTarget(int i)
    {
        float a = spin + i * Mathf.PI * 2f / LampCount;
        return center + new Vector2(Mathf.Cos(a) * 2.4f, Mathf.Sin(a) * 1.3f);
    }

    // ------------------------------------------------------------ 每一幀

    public void Tick(float dt)
    {
        t += dt;
        spin += dt * 0.6f;

        if (phase == Phase.Fly)
        {
            for (int i = 0; i < LampCount; i++)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - i * 0.12f) / 2f));
                Vector2 p = Vector2.Lerp(from, LampTarget(i), k);
                lamps[i].position = new Vector3(p.x, p.y, 0f);
            }
            cam.backgroundColor = Color.Lerp(Game.Night, Gold, Mathf.Clamp01(t / 3f));
            if (t > 3.4f)
            {
                phase = Phase.Wish;
                t = 0f;
                GameInput.SetTyping(true);
            }
            return;
        }

        for (int i = 0; i < LampCount; i++)
        {
            Vector2 p = LampTarget(i);
            lamps[i].position = new Vector3(p.x, p.y, 0f);
        }

        if (phase == Phase.Wish)
        {
            string typed = GameInput.Typed;
            for (int i = 0; i < typed.Length; i++)
            {
                char c = typed[i];
                if (c >= 32 && c != 127 && wish.Length < MaxWish) wish += c;
            }
            if (GameInput.Backspace && wish.Length > 0) wish = wish.Substring(0, wish.Length - 1);
            if (GameInput.Submit && wish.Trim().Length > 0) Decide();
            return;
        }

        // ---- Result：每種結局的小動畫 ----
        if (t > LinesEnd) HideLamps(float.MaxValue); // GAME OVER：燈全部熄掉
        if (trueLines.Length > 0 && t > TrueStart)
        {
            // 真結局：只剩家裡那一盞燈亮著
            lamps[0].gameObject.SetActive(true);
            lamps[0].position = new Vector3(center.x, center.y + 0.2f, 0f);
            cam.backgroundColor = Game.Night;
            return;
        }
        if (kind == 0)
        {
            // 跳電：「啪。」那一行出現時全黑
            if (t > LineGap * 2f)
            {
                cam.backgroundColor = Color.black;
                HideLamps(float.MaxValue);
            }
        }
        else if (kind == 3)
        {
            // 貓：從左走到右，走過的燈都被叼走
            float k = Mathf.Clamp01((t - LineGap) / 3.5f);
            catX = Mathf.Lerp(center.x - 11f, center.x + 11f, k);
            if (cat != null) cat.position = new Vector3(catX, center.y - 0.3f + Mathf.Abs(Mathf.Sin(t * 10f)) * 0.08f, 0f);
            HideLamps(catX);
            cam.backgroundColor = Color.Lerp(Gold, Game.Night, Mathf.Clamp01((t - LineGap * 2f) / 3f));
        }
        else
        {
            cam.backgroundColor = Color.Lerp(Gold, Game.Night, Mathf.Clamp01((t - LineGap * 2f) / 5f));
        }
    }

    float LinesEnd { get { return lines.Length * LineGap + 0.6f; } }
    float TrueStart { get { return LinesEnd + 2.5f; } }

    void HideLamps(float leftOfX)
    {
        for (int i = 0; i < LampCount; i++)
        {
            if (lamps[i].gameObject.activeSelf && lamps[i].position.x < leftOfX) lamps[i].gameObject.SetActive(false);
        }
    }

    void Decide()
    {
        wish = wish.Trim();
        GameInput.SetTyping(false);
        phase = Phase.Result;
        t = 0f;
        kind = Random.Range(0, 4);
        string w = "「" + wish + "」";

        if (kind == 0)
        {
            lines = new[] { "七盞燈一起亮了起來——", "太亮了。", "啪。", "整座城跳電了。", "願望" + w + "因為停電，無法受理。" };
        }
        else if (kind == 1)
        {
            lines = new[] { "你的願望已送出：" + w, "已讀。", "……", "…………", "（對方已讀不回）" };
        }
        else if (kind == 2)
        {
            lines = new[] { "願望" + w + "已受理。", "您的號碼牌：第 9,999,999 號", "目前叫號：第 1 號", "預計等候時間：87 年", "請耐心等候，過號不補。" };
        }
        else
        {
            lines = new[] { "七盞燈在天上轉啊轉——", "一隻貓走了過來。", "牠把燈全部叼走了。", "願望" + w + "的回覆：喵。" };
            cat = Draw.Group("Cat", root, center.x - 11f, center.y - 0.3f);
            Color fur = new Color(0.95f, 0.6f, 0.25f);
            Draw.Box(cat, 0f, 0f, 1.5f, 0.6f, fur, 30);          // 身體
            Draw.Box(cat, 0.85f, 0.3f, 0.65f, 0.6f, fur, 31);    // 頭
            Draw.Box(cat, 0.65f, 0.68f, 0.18f, 0.22f, fur, 31);  // 耳朵
            Draw.Box(cat, 1.05f, 0.68f, 0.18f, 0.22f, fur, 31);
            Draw.Box(cat, -0.8f, 0.4f, 0.14f, 0.8f, fur, 30);    // 尾巴
            Draw.Box(cat, -0.5f, -0.45f, 0.16f, 0.35f, fur, 30); // 腳
            Draw.Box(cat, 0.5f, -0.45f, 0.16f, 0.35f, fur, 30);
            Draw.Box(cat, 0.98f, 0.36f, 0.09f, 0.09f, Color.black, 32); // 眼睛
        }

        if (game.SecretFound)
        {
            trueLines = new[] { "……等一下。", "你想起那塊牌子：「回家的路，一直都在。」", "你沒有再許願，走回了家。", "家裡的燈，本來就亮著。" };
        }
    }

    // ------------------------------------------------------------ 畫面上的字

    public void Gui()
    {
        float w = Ui.W;

        if (phase == Phase.Fly)
        {
            if (borrowed && t > 1.5f)
            {
                Ui.Text(new Rect(0f, 440f, w, 40f), "（試玩版只有 " + game.Lanterns + " 關，另外 " + (LampCount - game.Lanterns) + " 盞燈先借你）", 18, Color.white);
            }
            return;
        }

        if (phase == Phase.Wish)
        {
            Ui.Fill(new Rect(0f, 280f, w, 220f), new Color(0f, 0f, 0f, 0.35f));
            Ui.Text(new Rect(0f, 285f, w, 50f), "請說出你的願望……", 32, Color.white);

            Rect field = new Rect(w * 0.5f - 210f, 345f, 420f, 46f);
            Ui.Fill(new Rect(field.x - 2f, field.y - 2f, field.width + 4f, field.height + 4f), new Color(0f, 0f, 0f, 0.7f));
            Ui.Fill(field, new Color(1f, 1f, 1f, 0.92f));
            string cursor = Mathf.Repeat(t, 1f) < 0.5f ? "|" : " ";
            bool has = wish.Trim().Length > 0;
            Ui.Text(new Rect(field.x + 10f, field.y, field.width - 110f, field.height), wish + cursor, 22, new Color(0.1f, 0.1f, 0.1f), TextAnchor.MiddleLeft, false);
            if (has && Ui.Button(new Rect(field.xMax - 90f, field.y + 4f, 86f, field.height - 8f), "許願", 20))
            {
                Decide();
                return;
            }

            Ui.Text(new Rect(0f, 396f, w, 26f), "打字後按 Enter，或直接選一個：", 16, Color.white);
            float bw = 190f, gap = 14f;
            float x0 = w * 0.5f - (bw * 3f + gap * 2f) * 0.5f;
            for (int i = 0; i < Presets.Length; i++)
            {
                if (Ui.Button(new Rect(x0 + i * (bw + gap), 430f, bw, 52f), Presets[i], 22))
                {
                    wish = Presets[i];
                    Decide();
                    return;
                }
            }
            return;
        }

        // ---- Result ----
        float linesEnd = LinesEnd;
        bool over = t > linesEnd;
        float trueStart = TrueStart;
        Ui.Fill(new Rect(0f, 285f, w, 180f), new Color(0f, 0f, 0f, 0.35f));
        bool showTrue = trueLines.Length > 0 && t > trueStart;
        float allEnd = trueLines.Length > 0 ? trueStart + trueLines.Length * 1.5f + 0.5f : linesEnd + 1.2f;

        if (showTrue)
        {
            Ui.Text(new Rect(0f, 205f, w, 70f), "真結局", 46, new Color(1f, 0.9f, 0.5f));
            int n = Mathf.Min(trueLines.Length, Mathf.FloorToInt((t - trueStart) / 1.5f) + 1);
            for (int i = 0; i < n; i++)
            {
                Ui.Text(new Rect(0f, 292f + i * 33f, w, 33f), trueLines[i], 22, Color.white);
            }
        }
        else
        {
            if (over) Ui.Text(new Rect(0f, 110f, w, 110f), "GAME OVER", 68, new Color(1f, 0.3f, 0.3f));
            int n = Mathf.Min(lines.Length, Mathf.FloorToInt(t / LineGap) + 1);
            for (int i = 0; i < n; i++)
            {
                Ui.Text(new Rect(0f, 292f + i * 33f, w, 33f), lines[i], 22, Color.white);
            }
        }

        if (t > allEnd)
        {
            if (Ui.Button(new Rect(w * 0.5f - 100f, 468f, 200f, 54f), "再玩一次", 24))
            {
                game.RestartGame();
            }
        }
    }
}
