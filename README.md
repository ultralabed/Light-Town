# 七燈城（暫名）

2D 橫向整人闖關遊戲。從左邊的門走到右邊的門，路上要先拿到燈，門才會開。
集滿七盞燈可以許願……然後被遊戲整。

> 完全原創。不使用任何《七龍珠》元素，也沒有守護神獸。

## 現在做到哪

- ✅ 第 1 關「夜市大街」（教學關、一個黑傘幫、一塊「很穩」的地板）
- ✅ 整人結局（跳電／已讀不回／排隊／貓，隨機一種）＋「再玩一次」
- ✅ 鍵盤＋手機觸控
- ⬜ 第 2–7 關
- ⬜ 隱藏真結局（程式已經寫好，等第 4 關做出來，在那關加一行 `L.SecretReturn()` 就會啟用）
- ⬜ 網頁版（`docs/` 資料夾，要在 Unity 裡按 Build 才會有）

## 第一次打開（Unity 6）

這個 repo 裡只有「程式」和「字型」，Unity 專案的其他檔案要你電腦上的 Unity 產生一次：

1. Unity Hub → **New project** → 選 **Universal 2D** 範本，隨便取個名字建立（安裝 Unity 6 時記得勾 **Web Build Support**）。
2. 關掉 Unity。把新專案資料夾裡的 `Assets`、`Packages`、`ProjectSettings` 三個資料夾，**複製到這個 repo 資料夾裡**（`Assets` 選「合併」，不要取代掉 `Scripts` 和 `Resources`）。
3. Unity Hub → **Add** → 選這個 repo 資料夾 → 打開。
4. 按上面的 **▶**。遊戲會自己開始，不用拖任何物件。
5. 沒問題的話，把 `Assets`、`Packages`、`ProjectSettings` 都 commit 進 git，以後就不用再做一次。

## 操作

| | 鍵盤 | 手機 |
|---|---|---|
| 往左 | ← 或 A | 按螢幕最左邊（0–20%） |
| 往右 | → 或 D | 按左邊第二塊（20–40%） |
| 跳 | 空白鍵 或 W | 按螢幕右邊（60–100%） |

手機請橫著玩。

## 怎麼加新關卡

打開 `Assets/Scripts/Levels.cs`，照第 1 關的樣子寫，再加進 `All` 清單：

```csharp
static void Level2(Level L)
{
    L.Name("港口");
    L.Ground(-6, 10);          // 地板從 x=-6 到 x=10
    L.Ground(13, 40);          // 10 到 13 中間沒地板 = 海上的洞
    L.Platform(20, 23, 1.5f);  // 貨箱（頂端高度 1.5）
    L.StartDoor(0);
    L.Enemy(25, 32, 3f);       // 黑傘幫在 25–32 之間走，速度 3
    L.Lantern(21.5f, 3.5f);    // 燈
    L.Sign(5, "海很深，相信我");
    L.ExitDoor(37);
}
```

可用的積木：`Ground`、`FallingFloor`（踩了會掉）、`Platform`、`Enemy`、`Lantern`、`StartDoor`、`ExitDoor`、`Sign`、`Stall`（裝飾）、`SecretReturn`。
主角一次大約能跳 **2.4 高、4 寬**。

## 程式檔案

| 檔案 | 做什麼 |
|---|---|
| `Game.cs` | 主程式：主角移動、碰撞、重來、畫面上的字 |
| `Levels.cs` | **關卡內容都在這裡** |
| `Level.cs` | 關卡積木的做法 |
| `Ending.cs` | 許願和整人結局 |
| `GameInput.cs` | 鍵盤／滑鼠／觸控（新舊 Input System 都支援） |
| `Ui.cs` | 畫字和按鈕 |
| `Draw.cs` | 用程式畫方塊和圓（之後換美術圖改這裡） |

## 發布到 GitHub Pages

1. Unity：**File → Build Profiles → Web → Switch Platform**
2. **Player Settings → Publishing Settings → Compression Format 設成 Disabled**（或勾 Decompression Fallback），不然網頁打不開
3. **Build**，輸出資料夾選這個 repo 裡的 `docs`
4. commit、push
5. GitHub → Settings → Pages → Branch 選 `main`、資料夾選 `/docs`

## 字型

`Assets/Resources/GameFont.ttf` 是 [Noto Sans TC](https://fonts.google.com/noto/specimen/Noto+Sans+TC)，
採用 SIL Open Font License 1.1（授權全文在 `Assets/Resources/GameFont-OFL.txt`）。
網頁版沒有內建中文字，所以一定要帶這個檔案。字型裡沒有 emoji，遊戲文字請不要用 emoji。

## 還沒決定

- 主角是誰、長什麼樣子（現在是一個有眼睛的方塊）
- 美術風格
- 正式名稱（「七燈城」上架前要再查商標）
