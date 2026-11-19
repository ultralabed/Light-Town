using UnityEngine;

/// <summary>
/// 《七燈城》主程式。按 ▶ 就會自己啟動，不用在場景裡拖任何東西。
/// 負責：主角移動、黑傘幫、燈、門、重來、畫面上的字、進結局。
/// </summary>
public class Game : MonoBehaviour
{
    // ---- 手感數字（想調手感改這裡）----
    const float MoveSpeed = 6f;
    const float Gravity = 40f;
    const float JumpSpeed = 14f;
    const float MaxFall = 25f;
    const float HalfW = 0.4f;   // 主角半寬
    const float BodyH = 1.2f;   // 主角身高
    const float CamSize = 5f;
    const float CamY = 2.6f;

    public static readonly Color Night = new Color(0.07f, 0.08f, 0.18f);

    enum State { Play, Dead, Ending }

    static bool booted;

    State state;
    Camera cam;
    Level level;
    int levelIndex;
    Ending ending;

    public int Lanterns { get; private set; }
    public int Restarts { get; private set; }
    public bool SecretFound { get; private set; }
    public Camera Cam { get { return cam; } }
    public Vector2 PlayerPos { get { return new Vector2(px, py + BodyH * 0.5f); } }

    // 主角
    Transform player;
    SpriteRenderer playerBody;
    Transform playerEyes;
    float px, py, vx, vy;
    bool grounded;
    Solid standing;
    float coyote, jumpBuffer;
    float deadTimer;

    string toast = "";
    float toastTimer;

    static readonly Color PlayerColor = new Color(0.35f, 0.85f, 0.80f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (booted) return;
        booted = true;
        var go = new GameObject("SevenLanternCity");
        DontDestroyOnLoad(go);
        go.AddComponent<Game>();
    }

    void Awake()
    {
        Application.targetFrameRate = 60;

        cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = CamSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Night;
        cam.transform.position = new Vector3(0f, CamY, -10f);

        // 中文字型：Assets/Resources/GameFont.ttf
        Ui.Font = Resources.Load<Font>("GameFont");
        if (Ui.Font == null) Debug.LogWarning("找不到 Assets/Resources/GameFont.ttf，中文字可能顯示不出來。");

        StartLevel(0);
    }

    void OnDestroy()
    {
        booted = false;
    }

    // ------------------------------------------------------------ 關卡

    void StartLevel(int index)
    {
        if (level != null && level.root != null) Destroy(level.root.gameObject);
        if (player != null) Destroy(player.gameObject);

        levelIndex = index;
        var root = new GameObject("Level " + (index + 1)).transform;
        level = new Level(root);
        Levels.All[index](level);

        // 主角（暫時是一個有眼睛的方塊，之後換圖）
        player = Draw.Group("Player", null, 0f, 0f);
        playerBody = Draw.Box(player, 0f, BodyH * 0.5f, HalfW * 2f, BodyH, PlayerColor, 10);
        playerEyes = Draw.Group("Eyes", player, 0f, 0f);
        Draw.Box(playerEyes, 0.02f, 0.85f, 0.12f, 0.2f, new Color(0.05f, 0.1f, 0.15f), 11);
        Draw.Box(playerEyes, 0.26f, 0.85f, 0.12f, 0.2f, new Color(0.05f, 0.1f, 0.15f), 11);

        px = level.startX;
        py = 0f;
        vx = 0f;
        vy = 0f;
        grounded = false;
        standing = null;
        coyote = 0f;
        jumpBuffer = 0f;
        state = State.Play;
        cam.backgroundColor = Night;
        SyncPlayer();
        FollowCamera(true);
    }

    public void RestartGame()
    {
        if (ending != null) ending.Dispose();
        ending = null;
        Lanterns = 0;
        Restarts = 0;
        SecretFound = false;
        toastTimer = 0f;
        StartLevel(0);
    }

    // ------------------------------------------------------------ 每一幀

    void Update()
    {
        GameInput.Poll();
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);

        if (state == State.Ending)
        {
            ending.Tick(dt);
            return;
        }

        if (toastTimer > 0f) toastTimer -= dt;
        UpdateTraps(dt);
        UpdateEnemies(dt);
        UpdateLantern();

        if (state == State.Dead)
        {
            deadTimer -= dt;
            if (deadTimer <= 0f)
            {
                Restarts++;
                StartLevel(levelIndex);
            }
            return;
        }

        MovePlayer(dt);
        SyncPlayer();
        FollowCamera(false);
        CheckThings();
    }

    Rect PlayerRect()
    {
        return new Rect(px - HalfW, py, HalfW * 2f, BodyH);
    }

    void MovePlayer(float dt)
    {
        vx = GameInput.MoveX * MoveSpeed;
        vy = Mathf.Max(vy - Gravity * dt, -MaxFall);

        // 跳：按早一點點、或剛離開地板一點點，都還算數（手感比較好）
        if (GameInput.JumpDown) jumpBuffer = 0.12f; else jumpBuffer -= dt;
        if (grounded) coyote = 0.1f; else coyote -= dt;
        if (jumpBuffer > 0f && coyote > 0f)
        {
            vy = JumpSpeed;
            jumpBuffer = 0f;
            coyote = 0f;
        }
        if (!GameInput.JumpHeld && vy > 6f) vy = 6f; // 放開按鍵就跳矮一點

        // 先左右
        px += vx * dt;
        px = Mathf.Clamp(px, level.minX + HalfW, level.maxX - HalfW);
        for (int i = 0; i < level.solids.Count; i++)
        {
            Solid s = level.solids[i];
            if (!s.on || !PlayerRect().Overlaps(s.rect)) continue;
            if (vx > 0f) px = s.rect.xMin - HalfW;
            else if (vx < 0f) px = s.rect.xMax + HalfW;
        }

        // 再上下
        py += vy * dt;
        grounded = false;
        standing = null;
        for (int i = 0; i < level.solids.Count; i++)
        {
            Solid s = level.solids[i];
            if (!s.on || !PlayerRect().Overlaps(s.rect)) continue;
            if (vy <= 0f)
            {
                py = s.rect.yMax;
                grounded = true;
                standing = s;
            }
            else
            {
                py = s.rect.yMin - BodyH;
            }
            vy = 0f;
        }

        // 踩到整人地板
        if (standing != null && standing.trap && !standing.triggered && px > standing.rect.xMin + 0.3f)
        {
            standing.triggered = true;
            standing.timer = 0.12f;
        }
    }

    void SyncPlayer()
    {
        player.position = new Vector3(px, py, 0f);
        if (vx > 0.01f) playerEyes.localPosition = new Vector3(0f, 0f, 0f);
        else if (vx < -0.01f) playerEyes.localPosition = new Vector3(-0.28f, 0f, 0f);
    }

    void FollowCamera(bool snap)
    {
        float halfW = CamSize * cam.aspect;
        float lo = level.minX + halfW;
        float hi = level.maxX - halfW;
        float target = lo > hi ? (level.minX + level.maxX) * 0.5f : Mathf.Clamp(px, lo, hi);
        float x = snap ? target : Mathf.Lerp(cam.transform.position.x, target, 0.15f);
        cam.transform.position = new Vector3(x, CamY, -10f);
    }

    void UpdateTraps(float dt)
    {
        for (int i = 0; i < level.solids.Count; i++)
        {
            Solid s = level.solids[i];
            if (!s.trap || !s.triggered) continue;
            if (s.on)
            {
                s.timer -= dt;
                if (s.timer <= 0f) s.on = false;
            }
            else if (s.tf.position.y > -20f)
            {
                s.vy -= Gravity * dt;
                s.tf.position += new Vector3(0f, s.vy * dt, 0f);
            }
        }
    }

    void UpdateEnemies(float dt)
    {
        for (int i = 0; i < level.enemies.Count; i++)
        {
            Patrol e = level.enemies[i];
            e.x += e.dir * e.speed * dt;
            if (e.x >= e.x1) { e.x = e.x1; e.dir = -1; }
            else if (e.x <= e.x0) { e.x = e.x0; e.dir = 1; }
            e.tf.position = new Vector3(e.x, 0f, 0f);
            e.tf.localScale = new Vector3(e.dir, 1f, 1f);
        }
    }

    void UpdateLantern()
    {
        if (!level.hasLantern || level.lanternTf == null) return;
        if (level.lanternTaken)
        {
            // 拿到的燈跟在主角頭上
            Vector3 want = new Vector3(px, py + BodyH + 0.7f, 0f);
            level.lanternTf.position = Vector3.Lerp(level.lanternTf.position, want, 0.2f);
        }
        else
        {
            float bob = Mathf.Sin(Time.time * 3f) * 0.12f;
            level.lanternTf.position = new Vector3(level.lanternPos.x, level.lanternPos.y + bob, 0f);
        }
    }

    void CheckThings()
    {
        Rect me = PlayerRect();

        // 掉進洞裡
        if (py < -5f) { Die(); return; }

        // 碰到黑傘幫
        Rect hurt = new Rect(me.x + 0.08f, me.y + 0.05f, me.width - 0.16f, me.height - 0.1f);
        for (int i = 0; i < level.enemies.Count; i++)
        {
            if (hurt.Overlaps(level.enemies[i].Rect)) { Die(); return; }
        }

        // 拿燈
        if (level.hasLantern && !level.lanternTaken)
        {
            Rect lamp = new Rect(level.lanternPos.x - 0.45f, level.lanternPos.y - 0.45f, 0.9f, 0.9f);
            if (me.Overlaps(lamp))
            {
                level.lanternTaken = true;
                if (level.exitDoor != null) level.exitDoor.color = Level.DoorOpen;
                Say("拿到燈了！門開了。", 2f);
            }
        }

        // 隱藏真結局：拿到燈之後走回起點的門
        if (level.secretReturn && level.lanternTaken && !SecretFound && Mathf.Abs(px - level.startX) < 0.6f)
        {
            SecretFound = true;
            Say("……你好像聽見家裡有人在等你。", 3f);
        }

        // 出口
        Rect door = new Rect(level.exitX - 0.6f, 0f, 1.2f, 2f);
        if (me.Overlaps(door))
        {
            if (level.lanternTaken) FinishLevel();
            else Say("門鎖著。先去拿燈！", 0.3f);
        }
    }

    void Die()
    {
        state = State.Dead;
        deadTimer = 0.5f;
        playerBody.color = new Color(1f, 0.3f, 0.3f);
    }

    void FinishLevel()
    {
        Lanterns++;
        if (levelIndex + 1 < Levels.All.Length)
        {
            StartLevel(levelIndex + 1);
        }
        else
        {
            state = State.Ending;
            Vector2 from = PlayerPos;
            if (level.root != null) Destroy(level.root.gameObject);
            if (player != null) Destroy(player.gameObject);
            level = null;
            ending = new Ending(this, from);
        }
    }

    void Say(string text, float seconds)
    {
        toast = text;
        toastTimer = seconds;
    }

    // ------------------------------------------------------------ 畫面上的字

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        float w = Ui.W;

        if (state == State.Ending)
        {
            ending.Gui();
        }
        else
        {
            // 告示牌
            for (int i = 0; i < level.signs.Count; i++)
            {
                SignPost s = level.signs[i];
                Vector2 p = Ui.FromWorld(cam, s.pos);
                float tw = Ui.TextWidth(s.text, 17) + 20f;
                if (p.x < -tw || p.x > w + tw) continue;
                Rect r = new Rect(p.x - tw * 0.5f, p.y - 34f, tw, 30f);
                Ui.Fill(r, new Color(0f, 0f, 0f, 0.55f));
                Ui.Text(r, s.text, 17, new Color(1f, 0.95f, 0.8f), TextAnchor.MiddleCenter, false);
            }

            // 上面那一排：第幾關、燈、重來次數
            Ui.Fill(new Rect(0f, 0f, w, 40f), new Color(0f, 0f, 0f, 0.45f));
            Ui.Text(new Rect(14f, 0f, w, 40f), "第 " + (levelIndex + 1) + " 關　" + level.name, 20, Color.white, TextAnchor.MiddleLeft);
            Ui.Text(new Rect(0f, 0f, w, 40f), "燈 " + Lanterns + "/" + Levels.TotalLanterns, 20, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);
            Ui.Text(new Rect(0f, 0f, w - 14f, 40f), "重來 " + Restarts + " 次", 20, Color.white, TextAnchor.MiddleRight);

            if (toastTimer > 0f)
            {
                Ui.Text(new Rect(0f, 60f, w, 40f), toast, 24, Color.white);
            }

            // 手機的畫面按鈕（整條直的區域都能按，這裡只是畫出提示）
            if (GameInput.TouchSeen)
            {
                Color c = new Color(1f, 1f, 1f, 0.16f);
                Color t = new Color(1f, 1f, 1f, 0.75f);
                float bh = 110f, by = Ui.H - bh - 10f;
                Rect l = new Rect(w * 0.0f + 6f, by, w * 0.2f - 12f, bh);
                Rect r = new Rect(w * 0.2f + 6f, by, w * 0.2f - 12f, bh);
                Rect j = new Rect(w * 0.6f + 6f, by, w * 0.4f - 12f, bh);
                Ui.Fill(l, c); Ui.Text(l, "←", 44, t);
                Ui.Fill(r, c); Ui.Text(r, "→", 44, t);
                Ui.Fill(j, c); Ui.Text(j, "跳", 40, t);
            }
        }

        // 手機直拿的時候提醒橫過來
        if (Screen.height > Screen.width)
        {
            Ui.Fill(new Rect(0f, Ui.H * 0.4f, w, Ui.H * 0.2f), new Color(0f, 0f, 0f, 0.8f));
            Ui.Text(new Rect(0f, Ui.H * 0.4f, w, Ui.H * 0.2f), "請把手機橫過來玩", 16, Color.white);
        }
    }
}
