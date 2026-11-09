---
name: masuit-tools-project
description: "了解和导航 Masuit.Tools 仓库、选择 NuGet 包、定位功能模块或规划跨项目改动时使用。"
---

# Masuit.Tools 项目导航

当任务涉及多个项目、包选择、仓库结构，或不确定应修改哪个项目时，先使用本 Skill。实现某个专用包时，再加载对应模块 Skill。

## 包与项目路由

| 需求 | 项目 | 主要职责 |
|---|---|---|
| 通用基础类型与工具扩展 | `Masuit.Tools.Abstractions` | 文件、字符串、日期、集合、媒体、硬件、数学、安全、验证等基础能力 |
| EF Core / IQueryable 工具 | `Masuit.Tools.Core` | `DbContext` 变更跟踪、`IQueryable` 等数据访问扩展；依赖 Abstractions |
| ASP.NET Core Web 集成 | `Masuit.Tools.AspNetCore` | Web 扩展、模型绑定、DI、分布式缓存、断点续传文件结果；依赖 Core |
| Excel 导入/导出 | `Masuit.Tools.Excel` | 独立 Excel 包，基于 ClosedXML；依赖 Abstractions |
| MongoDB 客户端 | `Masuit.Tools.NoSQL.MongoDBClient` | 独立 MongoDB 驱动封装 |
| 数字水印 | `Masuit.Tools.DigtalWatermarker` | 基于 OpenCvSharp 的独立数字水印包 |
| .NET Framework 4.6.1+ 兼容包 | `Masuit.Tools` | `Masuit.Tools.Net.csproj`，包含传统 ASP.NET MVC 支持 |
| .NET Framework 4.5 兼容包 | `Masuit.Tools.Net45` | 专用兼容项目；通过显式链接复用部分 Abstractions 源码 |

项目根目录的 `README.md` 介绍包用途和公开示例；解决方案为 `Masuit.Tools.sln`。实际目标框架、依赖和编译包含项以各 `.csproj` 为准，不要仅根据包名或 README 推断。

## 关键架构约束

- Abstractions 是共享基础层；Core、Excel 等项目按需引用它，避免把 Web、EF 或数据库专属依赖塞进基础层。
- 多目标框架项目需要保留各目标框架兼容性；检查目标项目的条件 `PackageReference`、`Compile Remove` 和 `ProjectReference` 后再改动。
- `Masuit.Tools.Net45` 通过 `.csproj` 显式链接 Abstractions 文件，不会自动包含所有新增源文件。若改动必须进入此包，核对并同步其编译项；不要假定两个旧版包能力完全一致。
- 仓库同时包含多个独立 NuGet 包和不同年代的测试项目，选择最接近改动的测试项目运行，而非假定所有测试都可由同一目标框架执行。

## 工作流程

1. 用本表确定拥有该 API 的项目，并查找相邻实现、调用方和测试。
2. 检查项目文件确认目标框架、依赖关系和条件编译差异。
3. 保持公共 API 所属包边界；如果确实需要跨包改动，分别更新各自测试与文档。
4. 修改后运行对应模块 Skill 或 [测试 Skill](../masuit-tools-tests/SKILL.md) 中的定向验证命令。

## 模块 Skills

- 通用工具：[masuit-tools-utilities](../masuit-tools-utilities/SKILL.md)
- EF Core：[masuit-tools-efcore](../masuit-tools-efcore/SKILL.md)
- ASP.NET Core：[masuit-tools-aspnetcore](../masuit-tools-aspnetcore/SKILL.md)
- Excel：[masuit-tools-excel](../masuit-tools-excel/SKILL.md)
- MongoDB：[masuit-tools-mongodb](../masuit-tools-mongodb/SKILL.md)
- 数字水印：[masuit-tools-watermark](../masuit-tools-watermark/SKILL.md)
- 测试与验证：[masuit-tools-tests](../masuit-tools-tests/SKILL.md)
