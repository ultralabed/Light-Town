using UnityEngine;

/// <summary>
/// 所有關卡都寫在這裡。要加新的一關：
///   1. 照 Level1 的樣子寫一個 Level2(Level L)
///   2. 把它加進下面的 All 清單
/// 走完清單裡最後一關就會進結局。
///
/// 好用的數字：主角一次能跳大約 2.4 高、4 寬。地面是 y = 0。
/// </summary>
public static class Levels
{
    /// <summary>完整遊戲總共要集幾盞燈。</summary>
    public const int TotalLanterns = 7;

    public static readonly System.Action<Level>[] All =
    {
        Level1,
        // Level2, Level3, ... 之後加在這裡
    };

    // 第 1 關：夜市大街（教學關，一個黑傘幫）
    static void Level1(Level L)
    {
        L.Name("Night Market Street");

        L.Ground(-6, 44);
        L.FallingFloor(44, 47);   // 整人：踩上去就掉
        L.Ground(47, 58);

        L.StartDoor(0);
        L.Sign(3, "← → to walk, SPACE to jump");

        L.Stall(7.5f, new Color(0.80f, 0.25f, 0.25f));
        L.Sign(11, "Black Umbrella Gang ahead. Jump over!");
        L.Enemy(14, 22, 2.5f);

        L.Sign(25, "Grab the lantern to open the door");
        L.Lantern(28.5f, 2.7f);
        L.Stall(33, new Color(0.25f, 0.55f, 0.75f));

        L.Sign(41, "This floor is very stable. Trust me.");
        L.ExitDoor(53);
    }
}
