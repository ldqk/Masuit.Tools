---
name: masuit-tools-mongodb
description: "在 Masuit.Tools.NoSQL.MongoDBClient 中实现或排查 MongoDB CRUD、MongoDbClient 单例封装、MongoDB.Driver 查询或配置时使用。"
---

# Masuit.Tools MongoDB 客户端

MongoDB 是独立 NuGet 项目 `Masuit.Tools.NoSQL.MongoDBClient`，面向 `netstandard2.1` 并依赖 MongoDB.Driver。项目描述记录了 `MongoDbClient.GetInstance()` 单例入口和 CRUD 封装；具体成员、配置方式与线程安全约定以当前源码为准。

## 工作流程

1. 检查客户端、集合访问、序列化配置和 CRUD API 的当前实现与调用点，沿用既有抽象，不臆造未提供的 API。
2. 明确数据库/集合选择、过滤器、排序分页、更新语义与异常传播；不要将用户输入拼接成未经验证的查询表达式。
3. 保持 MongoDB 驱动对象生命周期与客户端复用模式一致；不要在每次操作中无意创建额外客户端。
4. 对 CRUD 和序列化行为添加可重复的测试。若测试必须依赖外部 MongoDB 服务，说明前置条件，不要将连接失败伪装成成功。
5. 运行包构建，并核对改动没有引入其它包不需要的 MongoDB 依赖。

## 定向验证

```powershell
dotnet build Masuit.Tools.NoSQL.MongoDBClient\Masuit.Tools.NoSQL.MongoDBClient.csproj
```

此项目当前可能没有专属测试项目；添加测试时先确认仓库现有测试基础设施和是否需要真实 MongoDB 实例。
