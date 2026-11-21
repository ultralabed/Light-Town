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
- ⬜ 網頁版（自動 Build 的流程已經放好，等你在 GitHub 加上 Unity 授權就會啟動，見下面）

## 第一次打開（Unity 6）

這個 repo 已經是一個可以直接打開的 Unity 專案：

1. 安裝 **Unity 6**（建議 `6000.0.58f1`，安裝時勾 **Web Build Support**）。
2. Unity Hub → **Add** → 選這個 repo 資料夾 → 打開。（第一次會跑幾分鐘，Unity 會自己補上缺的設定檔。）
3. 按上面的 **▶**。遊戲會自己開始，不用拖任何物件。
4. Unity 自己產生的 `ProjectSettings/`、`Packages/packages-lock.json`、`*.meta` 檔，請一起 commit 進 git。

> 專案用的是 Unity 內建的畫面系統（不是 Universal 2D 範本），因為遊戲只畫方塊和圓形，這樣網頁版比較小、也比較不容易出錯。

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
| `Hero.cs` | 主角的樣子和走路、跳的小動畫 |
| `Draw.cs` | 用程式畫方塊和圓（之後換美術圖改這裡） |

## 發布到 GitHub Pages（自動）

`.github/workflows/web.yml` 會在每次 push 之後，自動在 GitHub 的電腦上做出網頁版，再放上 GitHub Pages。
你只要做一次下面的設定：

1. **打開 Pages**：GitHub → repo 的 **Settings → Pages → Source** 選 **GitHub Actions**。
2. **找到 Unity 授權檔**：在你電腦上用 Unity Hub 登入並啟用免費的 Personal 授權，然後找這個檔案：
   - Windows：`C:\ProgramData\Unity\Unity_lic.ulf`
   - Mac：`/Library/Application Support/Unity/Unity_lic.ulf`
3. **加三個 secret**：GitHub → **Settings → Secrets and variables → Actions → New repository secret**
   - `UNITY_LICENSE`：用記事本打開 `Unity_lic.ulf`，把全部內容貼上
   - `UNITY_EMAIL`：你的 Unity 帳號 email
   - `UNITY_PASSWORD`：你的 Unity 密碼
4. **跑一次**：GitHub → **Actions → Build web version and deploy to GitHub Pages → Run workflow**。
   大約 10–20 分鐘後，網址會出現在 Settings → Pages。

還沒加 secret 的時候，這個流程會自己跳過，不會出現紅色失敗。

想在自己電腦上做網頁版：Unity 上方選單 **Seven Lantern City → Build Web Version**，結果在 `build/WebGL/web`。

## 語言

遊戲裡的字（告示牌、狀態列、結局）都是英文。程式註解和這份說明是中文。

## 字型

`Assets/Resources/GameFont.ttf` 是 [Noto Sans TC](https://fonts.google.com/noto/specimen/Noto+Sans+TC)，
採用 SIL Open Font License 1.1（授權全文在 `Assets/Resources/GameFont-OFL.txt`）。
網頁版沒有內建中文字，所以一定要帶這個檔案。字型裡沒有 emoji，遊戲文字請不要用 emoji。

## 還沒決定

- 主角的名字和正式造型（現在是用圓形和方塊拼的原創卡通小孩，在 `Hero.cs`）
- 美術風格
- 正式名稱（「七燈城」上架前要再查商標）
