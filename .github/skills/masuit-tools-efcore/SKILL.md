---
name: masuit-tools-efcore
description: "在 Masuit.Tools.Core 中开发、调试或测试 EF Core、DbContext 变更跟踪、IQueryable 扩展、Include 或数据库相关能力时使用。"
---

# Masuit.Tools EF Core 与数据访问

主要代码位于 `Masuit.Tools.Core\Database` 和 `Masuit.Tools.Core\Models`。该包依赖 `Masuit.Tools.Abstractions`；`Masuit.Tools.Core.Test` 是现代 EF Core 扩展的定向测试项目。

## 目标框架与依赖

`Masuit.Tools.Core.csproj` 多目标到 `netstandard2.0`、`netstandard2.1`、`net6`、`net8`、`net9`、`net10.0`，并按目标框架选择 EF Core 依赖版本。版本条件是有意的兼容映射；任何升级或 API 改动都需检查全部条件组，而不是只验证当前 SDK 默认目标。

传统 `Masuit.Tools` / `Masuit.Tools.Net45` 与 Core 并非同一个 EF Core 包面；确认目标包再添加功能。

## 工作流程

1. 在 `Masuit.Tools.Core` 搜索现有扩展和调用点；检查 Abstractions 中关联实体/帮助类型，避免重复定义共享模型。
2. 先确认所有目标框架实际引用的 EF Core 版本与 API 可用性。
3. 保持 IQueryable 扩展可组合、延迟执行等现有语义；避免在通用扩展中意外枚举查询或改变跟踪状态。
4. 对 `DbContext`/变更对比逻辑覆盖新增、修改、删除、空值及导航/复杂属性边界，遵循现有测试安排。
5. 用 `Test\Masuit.Tools.Core.Test` 添加并运行回归测试；该项目引用 `Masuit.Tools.AspNetCore` 并包含 EF Core InMemory，若改动仅属 Core，可先构建 Core 项目确认编译。
6. 如编辑目标框架条件或 EF 依赖，验证对应条件而不仅是当前单一目标。

## 定向验证

```powershell
dotnet test Test\Masuit.Tools.Core.Test\Masuit.Tools.Core.Test.csproj
dotnet build Masuit.Tools.Core\Masuit.Tools.Core.csproj
```

不要因测试项目引用 AspNetCore 就把 Core 专属实现放进 Web 包；保持依赖方向和包边界。
