# BiliUwp 维护说明（2026-10-19 整理）

> ⚠️ 本文档记录的是 **2026-10-19** 的接口实测情况。B 站接口变化很快，如果照着做时接口已经变了，
> 优先看日志里的错误码（`-352` 风控 / `-400` 参数 / `-404` 不存在 / `62002` 稿件不可见），
> 再去验证对应接口是否还活着 —— 本文件的价值是"迁移思路"，不是"永久可用的接口清单"。

这是从 `xiaoyaocz/biliuwp`（哔哩哔哩第三方 UWP 客户端，2023 年前后停更）复活出来的工作副本，
用来在 HoloLens / Windows 桌面 UWP 上继续做 B 站客户端。

原来的总目录 `D:\MyBiliApp` 里堆了 8 个互相复制的副本、构建产物和日志，已清理成：

```
D:\MyBiliApp
├─ BiliUwp\          ← 唯一的当前工程（本目录）
├─ _archive\         ← 旧版本源码 zip（仅在回溯时用）
└─ README.md         ← 当前总说明
```

## 环境与编译

- Visual Studio 2019（含 UWP / Windows 10 SDK 10.0.19041）
- 解决方案：`BiliBili.sln`
- 命令行编译：

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe' `
  BiliBili.sln /p:Configuration=Debug /p:Platform=x86 /restore /m
```

已确认 x86 Debug 可以编译通过（会输出 `AppPackages\BiliBili.UWP_3.10.2.0_x86_Debug_Test\*.appx`）。

### 一键部署 / 运行（推荐）

```powershell
powershell -ExecutionPolicy Bypass -File .\deploy.ps1
```

`deploy.ps1` 会：编译 → 把生成的 `.appx` 解包成松散布局（和 VS F5 部署方式一致，
不需要签名证书）→ 注册 → 启动。
参数：`-NoBuild` 跳过编译，`-NoLaunch` 只部署不启动。

> 注意：不要直接拿 `bin\x86\Debug` 目录去注册（那里缺少 `Assets`、`entrypoint`、
> `WinMetadata` 等资源，应用会在启动时闪退）。要用 `deploy.ps1` 或者 VS 的 F5。
>
> 如果编译过程中途被中断过、之后应用启动闪退，删掉 `bin` / `obj` 重新完整编译一次即可。


## 本版本做了什么

### 1. 修复 BV 号视频打不开详情 / 评论

原来视频详情走的是**客户端私有接口**：

```
https://app.bilibili.com/x/v2/view?access_key=...&appkey=...&sign=...
```

这个接口依赖早就停用的 Android appkey，也会被风控拦截，BV 号尤其容易失败；
而网页端接口不需要 appkey / sign，仍然可用。

改动：

- 新增 `BiliBili.UWP/Api/BiliWebApi.cs`：统一用网页端接口取视频信息
  - 主接口 `https://api.bilibili.com/x/web-interface/view/detail?bvid=...|aid=...`
  - 失败自动回退到 `https://api.bilibili.com/x/web-interface/view`
  - 一次拿到 **视频信息 + 相关视频 + 标签 + 充电信息**，av 号、BV 号、完整链接都认
  - 自动补齐 `short_link`、`req_user` 等网页接口不返回的字段
- `Pages/VideoViewPage.xaml.cs`
  - 详情统一改走 `BiliWebApi.GetVideoInfo`
  - 参数解析兼容 `string` / `object[]` / 数字，BV 号不再被误判
  - 评论**提前加载**：之前读取清晰度等步骤一旦抛异常，评论区就永远空着
  - 单个步骤失败不再影响整页（清晰度、合集、收藏夹各自兜底）
- `Controls/CommentV2Control.xaml.cs`
  - 评论接口先试带签名的地址，失败自动回退到不需要签名的网页端地址
  - `data.upper`、`data.replies` 等空引用做了保护
- `Helper/MessageCenter.cs`
  - BV 号优先识别，避免被旧的 av 号正则误伤
  - 支持手机端分享的 `b23.tv` / `bili2233.cn` 短链（自动跟随跳转再解析）
- `Helper/WebClientClass.cs`
  - 新增 `GetRedirectUrl`，给短链解析用

### 2. 播放地址 / 番剧播放迁移到网页端接口

`Helper/PlayurlHelper.cs` 里新增了一组网页端接口辅助方法（不需要 appkey/sign）：

| 接口 | 用途 |
| --- | --- |
| `x/player/playurl?avid=&cid=&fnval=16` | 普通视频 DASH（登录后 1080P+） |
| `x/player/playurl?avid=&cid=&fnval=1` | 普通视频 MP4（未登录也能到 720P） |
| `pgc/player/web/playurl?avid=&cid=&ep_id=&module=bangumi` | 番剧 DASH / MP4 |

- 播放策略：**登录用户优先 DASH**（可以上 1080P/60fps），**未登录优先 durl(MP4)**（匿名能到 720P，比匿名 DASH 的 480P 更高）
- 原来 `platform=html5` 参数会让服务器**只返回 MP4、不返回 DASH**，导致 DASH 播放永远失败，已去掉
- 清晰度列表改用 `support_formats` 解析，未登录/非大会员会标注「需登录 / 需大会员」，不再直接砍到 360P
- 默认清晰度改为「当前账号能拿到的最高」：未登录 720P、普通登录 1080P、大会员最高
- 番剧播放地址同样改成网页端接口，原来的 `bangumi.bilibili.com` + 旧 appkey 已失效

### 2.1 登录后 1080P 和 480P 一样（已修，关键）

原因：网页端播放接口是**认 cookie（SESSDATA）**的，而 App 里保存的登录态只有 `access_key`。
之前所有播放请求都是匿名请求，服务器只给 480P DASH / 720P MP4，所以选 1080P 也只是把 480P 的流换个名字。

修复（新增 `Helper/BiliWebSession.cs`）：

- 所有网页请求改为复用**同一个** `HttpBaseProtocolFilter` + `HttpClient`（原来每次 `new HttpClient()` 都建新的 cookie 容器，登录态永远带不上）
- 登录后自动用 `access_key` 走 passport 的 sso 接口换取 `SESSDATA` 等 cookie，并持久化到设置里；下次启动直接复用
- 播放/清晰度请求自动带上登录 cookie；退出登录时清空
- 日志里会写诊断信息：`播放诊断: 网页登录cookie=..., 请求清晰度=..., 服务器实际给出=...`，方便确认是否真的生效

### 3. 首页推荐卡片误判成 BV 号（已修）

推荐流卡片的 `uri` 里带 `player_preload` 加密串，里面恰好含有 `BvEuENvNC8aN` 这种片段，
被老的 BV 正则误判成视频号，点开就是「视频不存在」。修复：

- `Helper/MessageCenter.cs`：BV 号严格限定为 **`BV1` 开头共 12 位**，并且只在 URL 的路径部分里匹配（不再扫 query 参数）；同时优先处理 `bilibili://video/`、`?aid=`、`/av123` 等明确写法
- `Pages/Home/RecommendPage.xaml.cs`：视频卡片直接用卡片自己的 `param`(av号) 打开，不再解析带加密参数的 `uri`
- `Api/BiliWebApi.cs`：入参解析同样改成严格 BV 规则

### 4. 排行榜 / 分区 / 番剧简介（第三轮修复）

**排行榜**：旧的 `app.bilibili.com/x/v2/rank` 现在返回 `data` 是**空数组**，导致 `ApiDataModel<JObject>` 反序列化直接抛异常，
整个排行榜页面一直空着。已改为网页端 `x/web-interface/ranking/v2?rid=&type=all|origin`，并把手动映射到
`RankItemModel`（owner→author、stat.view→play、stat.danmaku→video_review、score→pts、duration 秒转 mm:ss）。
注意：这个接口需要 `buvid3/buvid4` cookie，否则会被风控返回 `-352`。

**分区页（原「频道」tab）**：B 站早已下线「频道订阅」功能，`app.bilibili.com/x/channel/*` 全部 404。
现在该 tab 改名「分区」，只保留分区导航网格（`ApiHelper.SetRegions()` 的电视端接口仍可用，另加了兜底分区列表），
下线掉的「发现频道/我的频道」区块已隐藏，也不再请求那个 404 接口。

**番剧简介**：`bangumi.bilibili.com/media/api/detail` 已失效，原来会用它覆盖 `evaluate` 导致简介空白。
现在改为优先使用 `pgc/view/app/season` 自带的 `evaluate / areas / actor / staff`，detail 接口有内容才覆盖。

**分区视频列表页（第四轮）**：点开分区后一直是空的，因为 `PartsPage` 用的是
`app.bilibili.com/x/v2/region/show/child/list`，该接口现在只返回 `{"code":0,"data":[]}`（空数组）。
改用网页端 `x/web-interface/newlist?rid=&pn=&ps=20&order=`：

- 该接口**只对顶级分区可用**（实测 1/3/4/5/13/36/181/188 等都有数据，且结果会聚合子分区内容）；
  对子分区 tid（如动画下的 24/25/27/47…）返回空，`ranking/v2` 对子分区还会 `-352`。
- 因此分区页改为**每个分区一个列表**（不再分子分区 tab），排序菜单仍然可用（view/danmaku/reply/senddate）。
- 旧的 Banner(`app.bilibili.com/api/region2/*.json`)、推荐(`bilibili.com/index/ding/*.json`)、
  标签(`x/tag/hots`) 三个接口都已失效，改为直接返回空/占位，不再发无效请求。
- 播放量/弹幕数改成「1.2万」格式显示。

**通用**：`BiliWebSession` 增加了 `buvid3/buvid4` 获取与 `GetStringAsync`，之后所有走网页端的接口
（排行榜、分区、播放等）都不会再因为缺 buvid 被 `-352` 风控。

### 5. 顺手修掉的报错（来自 App 日志）

- `Modules/Home/RecommendVM.cs`：推荐流里混入的动态 id 超过 `Int32`，导致首页推荐整块解析失败，`args.aid` 改为 `long`
- `Api/Home/RecommendAPI.cs`：`rid` / `tag_id` 改为 `long` 以配合上面
- `VideoViewPage.btn_Season_Click`：没有合集信息时点击空引用
- 分享 / 系统分享在 `short_link` 为空时崩溃
- `Modules/RankVM.cs`：`ApiHelper.regions` 为空时构造排行榜区域列表会空引用，已加保护

### 6. 目录清理

- 删除了 3 个重复的整项目备份、2 个 HoloLens 项目、1 个参考项目、若干构建日志
- 旧源码压缩保存在 `_archive`（只有源码，几 MB）

## 已知问题（下一步可以继续处理）

- 首页「推荐」内容用的还是旧的 app 接口，内容源本身可能变化
- 评论点赞 / 回复 / 删除等写操作仍走旧签名接口（点赞实测可用，写评论未验证）
- 下载功能基本可用（走网页端 playurl），DASH 下载会分成音视频两条任务再合并
- 排行榜的「日/周/月」筛选在网页端 v2 接口里已不区分，目前只切换全站/原创


