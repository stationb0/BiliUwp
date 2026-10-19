# 哔哩哔哩第三方 UWP 客户端（复活版）

> 基于 [xiaoyaocz/biliuwp](https://github.com/xiaoyaocz/biliuwp)（MIT License，2023 年前后停更）的**复活分叉**：
> 原项目的大部分客户端私有接口已失效，这里把视频详情、播放地址、搜索、排行榜、分区、番剧播放等
> 迁移到了目前仍可用的网页端接口，并修复了一批运行时报错。

## 目前可用的功能

- 首页推荐 / 热门 / 排行榜 / 分区浏览
- 视频详情（av 号、BV 号、完整链接、b23.tv 短链都能打开）+ 评论
- 播放：未登录最高 720P，登录后 1080P（DASH / MP4 自动选择）
- 番剧：信息、剧集列表、播放（网页端 PGC 接口）
- 搜索：视频 / 番剧 / 影视 / 用户 / 直播 / 专栏
- 扫码登录、收藏、历史、下载（部分）

详细改动记录见 [README_维护说明.md](README_维护说明.md)。

## 编译 / 部署

需要 Visual Studio 2019（含 UWP 工作负载、Windows 10 SDK 10.0.19041）。

```powershell
# 命令行编译（x86 Debug）
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe' `
  BiliBili.sln /p:Configuration=Debug /p:Platform=x86 /restore /m

# 一键编译 + 部署 + 启动（无需签名证书，松散注册方式）
powershell -ExecutionPolicy Bypass -File .\deploy.ps1
```

> 注意：不要直接用 `bin\x86\Debug` 目录注册（缺少 Assets/entrypoint/WinMetadata，会闪退），
> 请用 `deploy.ps1` 或 VS 的 F5 部署。

ARM 包（HoloLens 一代 / Windows 10 Mobile）可以这样出：

```powershell
& $msbuild 'BiliBili.sln' /p:Configuration=Release /p:Platform=ARM /p:AppxBundle=Always /p:UapAppxPackageBuildMode=StoreUpload
```

## 说明

- 本项目仅供学习交流，接口随时可能失效，请勿用于商业用途。
- 弹幕使用的是原作者另一个项目 [NSDanmaku](https://github.com/xiaoyaocz/NSDanmaku)。

## 引用及参考

- 原项目：[xiaoyaocz/biliuwp](https://github.com/xiaoyaocz/biliuwp)
- [You-get](https://github.com/soimort/you-get)
- [SYEngine](https://github.com/amamiya/SYEngine)
- [VLC.MediaElement](https://github.com/kakone/VLC.MediaElement)
- [biliroku](https://github.com/zyzsdy/biliroku)
- [bilibili-service](https://github.com/WhiteBlue/bilibili-service)
- [弹弹Play](https://api.acplay.net/swagger/ui/index)
- [Biliplus](https://www.biliplus.com)
- 接口参考：[Richasy/Bili.Uwp](https://github.com/Richasy/Bili.Uwp)

## License

[MIT](LICENSE)，版权归原作者 [xiaoyaocz](https://github.com/xiaoyaocz) 所有；本分叉的修改同样以 MIT 发布。
