---
name: masuit-tools-utilities
description: "在 Masuit.Tools 中新增、修改或排查通用 C# 工具、扩展方法、文件/日期/字符串/集合/媒体/安全/硬件功能时使用。"
---

# Masuit.Tools 通用工具

适用于 `Masuit.Tools.Abstractions` 中的可复用基础能力，以及需要兼容旧版 .NET 包的相关改动。EF Core、ASP.NET Core、Excel、MongoDB 和数字水印专属功能请使用相应模块 Skill。

## 常见代码位置

- `Masuit.Tools.Abstractions\Extensions`：基础类型、集合、字符串等扩展。
- `Files`、`DateTimeExt`、`Media`、`Hardware`、`Maths`、`Mime`、`RandomSelector`、`Security`、`Strings`、`Systems`、`TextDiff`、`Validator`：按领域查找。
- `Masuit.Tools.Abstractions\Models`、`Reflection`、`Database`：共享模型、反射及数据表帮助代码。
- `Masuit.Tools`：较早的兼容实现，例如 `Net`、`Mvc`、`Systems`。
- `Masuit.Tools.Net45`：框架 4.5 专用包，部分 Abstractions 文件通过链接引入。

先 `rg` 搜索 API 名、领域目录和测试，优先扩展现有工具，不要重复实现已有能力。项目的公开示例可在根目录 `README.md` 中查找。

## 实现流程

1. 确认需求属于跨场景基础能力；若依赖 ASP.NET Core、EF Core 或某个独立包，改在对应项目实现。
2. 检查同目录的命名空间、类组织、异常处理、注释和目标框架特定实现，遵循局部现有风格。
3. 将通用代码放到 Abstractions 的正确领域目录；不要把不必要的第三方依赖引入公共基础层。
4. 对新增或改变的公共 API 补充 XML 文档，并评估返回值、边界输入、文化/时区、资源释放及线程安全行为。
5. 如需支持 `Masuit.Tools.Net45`，核对 `Masuit.Tools.Net45.csproj` 中对应的显式 `Compile Include`；该项目不会自动编译 Abstractions 的全部源文件。
6. 在 `Test\Masuit.Tools.Abstractions.Test` 或与 API 实际归属相符的测试项目添加回归测试。
7. 运行定向测试，并按需构建受影响目标框架；测试项目和命令见 [测试 Skill](../masuit-tools-tests/SKILL.md)。

## 兼容性注意

- `Masuit.Tools.Abstractions.csproj` 面向 `netstandard2.0`、`netstandard2.1` 及多个现代 .NET 目标；只使用所有目标框架都可用的 API，或采用项目已有的条件编译模式。
- `Masuit.Tools.Net.csproj` 与 `Masuit.Tools.Net45.csproj` 是不同的兼容包；不要仅凭相似源码推断 API/依赖一致。
- 保持目录和命名空间与相邻类型一致；此仓库同时存在文件级命名空间与大括号命名空间，跟随目标目录现有格式。
- 日期、文件系统、网络与随机算法等工具应保持已公开的行为；涉及行为变更时，增加覆盖边界情况的测试并同步 README 示例（若示例因此过时）。
