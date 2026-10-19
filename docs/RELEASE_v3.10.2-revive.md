## BiliUwp 复活版 `v3.10.2-revive`

基于 [xiaoyaocz/biliuwp](https://github.com/xiaoyaocz/biliuwp)（MIT License，2023 年前后停更）的复活分叉。
原项目依赖的客户端私有接口大多已失效，本项目把视频详情、播放、搜索、排行榜、分区、番剧播放等
迁移到了目前仍可用的网页端接口，并修复了一批运行时报错。

### 本版主要修复

- **视频详情 + 评论**：改用 `x/web-interface/view/detail`，av 号 / BV 号 / 完整链接 / b23.tv 短链都能打开
- **播放**：网页端 `playurl`；未登录最高 720P，登录后 1080P（DASH 与 MP4 自动选择）
- **登录态**：新增全局网页会话，access_key 自动换取 `SESSDATA` cookie 并持久化（高清播放的关键）
- **搜索**：修复参数拼接导致的双 `?`（-400），改用 `x/web-interface/search/type`，视频/番剧/影视/用户/直播/专栏可用
- **排行榜**：改用 `x/web-interface/ranking/v2`
- **分区**：改用 `newlist` + 本地分页；修复点开分区空白、视频点不进去、下滑重复加载
- **番剧**：播放地址、清晰度、简介改用网页端 PGC 接口
- **其它**：首页推荐 Int32 溢出、BV 号误判、启动公告、分享空引用等

### 安装

- **PC（Win10/11）**：推荐用仓库里的 `deploy.ps1`（松散注册，**不需要证书**）
- **HoloLens / ARM 设备**：ARM `appxbundle` 是**未签名**的，需用自己的侧载证书签名后再安装，
  或直接用 VS2019 远程部署。签名可用仓库的 `tools/sign-package.ps1`
  ```powershell
  .\tools\sign-package.ps1 -Package "...\BiliBili.UWP_3.10.2.0_ARM.appxbundle" -Thumbprint "<你的证书指纹>"
  ```
- 依赖包见 `Dependencies_ARM_x86.zip`（.NET Native 2.2 / UI.Xaml 2.3 / VCLibs）

### 已知问题

- 部分老视频 / 番剧版权内容不可用（源站限制）
- 评论写操作（发评论、回复）未验证，点赞可用
- 首页推荐内容源本身可能变化
- 分区页目前展示的是分区排行榜内容（网页端没有可翻页的"最新"接口）
