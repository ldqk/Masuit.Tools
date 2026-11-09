---
name: masuit-tools-aspnetcore
description: "为 Masuit.Tools.AspNetCore 实现或排查 ASP.NET Core 中间件、模型绑定、DI、缓存、Web 扩展、文件上传或断点续传响应时使用。"
---

# Masuit.Tools ASP.NET Core

主要代码位于 `Masuit.Tools.AspNetCore\AspNetCore`、`Extensions`、`ModelBinder` 和 `Net`。该项目引用 `Masuit.Tools.Core` 并使用 `Microsoft.AspNetCore.App` 框架引用。

## 重要区域

- `ModelBinder`：如 `BodyOrDefaultModelBinder`、属性与注册扩展。
- `AspNetCore\ResumeFileResult`、`Executor`：启用 Range 请求的文件结果和执行器。
- `AspNetCore\ServiceCollectionExtensions.cs`、`Extensions\ServiceCollectionExt.cs`：服务注册相关扩展。
- `Extensions`：分布式缓存、multipart 上传、ViewData、无锁拦截器等。
- `Test\Masuit.Tools.Core.Test`：Web 集成测试（包括 TestHost）；`Test\Masuit.Tools.AspNetCore.ResumeFileResults.WebTest`：断点续传样例 Web 项目。

## 工作流程

1. 根据功能定位实现、扩展方法注册位置、对应 executor/middleware 和调用方；不要只改主类而漏掉 DI 或执行管线。
2. 检查 `Masuit.Tools.AspNetCore.csproj` 的目标框架条件及所引用的 Core/EF 版本。该项目覆盖 `netcoreapp3.1`、`net6`、`net8`、`net9`、`net10.0`。
3. 遵循 ASP.NET Core 的请求生命周期、依赖注入和取消语义；不要吞掉请求/流异常或在未确认所有权时关闭调用方资源。
4. 文件结果变更需验证 Range/条件请求、状态码、响应头、空/不可定位流及流释放行为；模型绑定变更需验证不同 Content-Type 和模型状态。
5. 在 `Test\Masuit.Tools.Core.Test` 或 WebTest 项目增加针对行为的测试，按 [测试 Skill](../masuit-tools-tests/SKILL.md) 运行。
6. 更新受影响的 README 示例，确保示例包名、服务注册方式和 API 与实际项目一致。

## 定向验证

```powershell
dotnet test Test\Masuit.Tools.Core.Test\Masuit.Tools.Core.Test.csproj
dotnet build Masuit.Tools.AspNetCore\Masuit.Tools.AspNetCore.csproj
dotnet build Test\Masuit.Tools.AspNetCore.ResumeFileResults.WebTest\Masuit.Tools.AspNetCore.ResumeFileResults.WebTest.csproj
```

不要把 ASP.NET Core 依赖引入 Abstractions，也不要将 Web 专属功能误放入 `Masuit.Tools.Core`。
