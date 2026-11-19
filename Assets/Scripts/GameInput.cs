using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 所有輸入都從這裡讀：鍵盤、滑鼠、手機觸控。
/// 新的 Input System 和舊的 Input Manager 都支援。
/// 每一幀由 Game 呼叫一次 Poll()。
/// </summary>
public static class GameInput
{
    public static float MoveX;        // -1 往左、+1 往右、0 不動
    public static bool JumpHeld;      // 跳的按鍵正按著
    public static bool JumpDown;      // 這一幀剛按下跳
    public static bool PointerDown;   // 這一幀剛點了一下（滑鼠或手指）
    public static Vector2 PointerPos; // 點的位置（螢幕像素，左下角是 0,0）
    public static bool TouchSeen;     // 有沒有用過觸控（有才顯示畫面按鈕）
    public static string Typed = "";  // 這一幀打的字
    public static bool Backspace;
    public static bool Submit;        // Enter

    static bool prevJump;

#if ENABLE_INPUT_SYSTEM
    static Keyboard hooked;
    static readonly System.Text.StringBuilder buffer = new System.Text.StringBuilder();

    static void OnText(char c)
    {
        buffer.Append(c);
    }
#endif

    public static void Poll()
    {
        bool left = false, right = false, jump = false;
        PointerDown = false;
        Backspace = false;
        Submit = false;
        Typed = "";

        if (Application.isMobilePlatform) TouchSeen = true;

#if ENABLE_INPUT_SYSTEM
        // ---------- 新 Input System ----------
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (hooked != kb)
            {
                if (hooked != null) hooked.onTextInput -= OnText;
                kb.onTextInput += OnText;
                hooked = kb;
            }
            left = kb.leftArrowKey.isPressed || kb.aKey.isPressed;
            right = kb.rightArrowKey.isPressed || kb.dKey.isPressed;
            jump = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
            Backspace = kb.backspaceKey.wasPressedThisFrame;
            Submit = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
        }
        Typed = buffer.ToString();
        buffer.Length = 0;

        var mouse = Mouse.current;
        if (mouse != null)
        {
            PointerPos = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame) PointerDown = true;
        }

        var ts = Touchscreen.current;
        if (ts != null)
        {
            foreach (var t in ts.touches)
            {
                if (!t.press.isPressed) continue;
                TouchSeen = true;
                Vector2 p = t.position.ReadValue();
                Zone(p.x, ref left, ref right, ref jump);
                if (t.press.wasPressedThisFrame)
                {
                    PointerDown = true;
                    PointerPos = p;
                }
            }
        }
#else
        // ---------- 舊 Input Manager ----------
        left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        jump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        Backspace = Input.GetKeyDown(KeyCode.Backspace);
        Submit = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        Typed = Input.inputString;

        PointerPos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0)) PointerDown = true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
            TouchSeen = true;
            Zone(t.position.x, ref left, ref right, ref jump);
            if (t.phase == TouchPhase.Began)
            {
                PointerDown = true;
                PointerPos = t.position;
            }
        }
#endif

        MoveX = (right ? 1f : 0f) - (left ? 1f : 0f);
        JumpHeld = jump;
        JumpDown = jump && !prevJump;
        prevJump = jump;
    }

    /// <summary>
    /// 手機觸控分區：螢幕左邊 0–20% 往左、20–40% 往右、右邊 60–100% 跳。
    /// </summary>
    static void Zone(float x, ref bool left, ref bool right, ref bool jump)
    {
        float f = x / Mathf.Max(1, Screen.width);
        if (f < 0.2f) left = true;
        else if (f < 0.4f) right = true;
        else if (f >= 0.6f) jump = true;
    }

    /// <summary>打字輸入時打開輸入法（讓電腦可以打中文）。</summary>
    public static void SetTyping(bool on)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) kb.SetIMEEnabled(on);
#else
        Input.imeCompositionMode = on ? IMECompositionMode.On : IMECompositionMode.Auto;
#endif
    }
}
