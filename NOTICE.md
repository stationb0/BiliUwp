# NOTICE / 声明

本项目是 [xiaoyaocz/biliuwp](https://github.com/xiaoyaocz/biliuwp) 的修改版本（复活分叉）。

## 原作品

- 项目：biliuwp — 哔哩哔哩第三方 UWP 客户端
- 作者：逍遥橙子 (xiaoyaocz)
- 仓库：https://github.com/xiaoyaocz/biliuwp
- 许可：MIT License，Copyright (c) 2019 xiaoyaocz
- 原项目已于 2023 年前后停止更新

## 本分叉的修改

完整改动记录见 [README_维护说明.md](README_维护说明.md)，主要包含：

- 视频详情、评论、播放地址、清晰度、搜索、排行榜、分区、番剧播放等接口由已失效的客户端私有接口迁移到网页端接口
- 新增全局网页会话（`BiliWebSession`）：统一 cookie 管理，用 access_key 换取 SESSDATA 以支持高清晰度播放
- 修复一系列运行时报错（首页推荐反序列化溢出、BV 号误判、分区点击无响应、重复加载等）
- 新增 GitHub Actions 自动编译、部署脚本与签名辅助脚本

修改部分同样以 MIT License 发布，详见 [LICENSE](LICENSE)。

## 免责声明

- 本项目为个人学习与怀旧项目，**非哔哩哔哩官方客户端**，与哔哩哔哩（bilibili）官方无关。
- 所有接口来自公开的网页端接口，项目不提供、不存储任何视频内容；内容版权归原作者及平台所有。
- 接口随时可能失效，使用产生的任何风险（包括账号相关风险）由使用者自行承担。
- 请勿将本项目用于商业用途或任何违反相关服务条款的场景。

## 依赖与参考

- 弹幕：[NSDanmaku](https://github.com/xiaoyaocz/NSDanmaku)（原作者另一项目）
- 接口参考：[Richasy/Bili.Uwp](https://github.com/Richasy/Bili.Uwp)
- 播放/解析参考：You-get、SYEngine、VLC.MediaElement、biliroku、bilibili-service、Biliplus
