using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masuit.Tools.NoSQL.MongoDBClient
{
    public class MongoDbClient
    {
        public MongoClient Client { get; set; }

        public IMongoDatabase Database { get; set; }

        private MongoDbClient(string url, string database) : this(MongoClientSettings.FromConnectionString(url), database)
        {
        }

        private MongoDbClient(MongoClientSettings settings, string database)
        {
            settings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };
            Client = new MongoClient(settings);
            Database = Client.GetDatabase(database);
        }

        /// <summary>
        /// 获取mongo单例
        /// </summary>
        /// <param name="url">连接字符串</param>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static MongoDbClient GetInstance(string url, string database)
        {
            return new MongoDbClient(url, database);
        }

        /// <summary>
        /// 获取mongo线程内唯一对象
        /// </summary>
        /// <param name="url">连接字符串</param>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static MongoDbClient ThreadLocalInstance(string url, string database)
        {
            MongoDbClient instance;
            if (CallContext<MongoDbClient>.GetData(url + database) == null)
            {
                instance = new MongoDbClient(url, database);
                CallContext<MongoDbClient>.SetData(url + database, instance);
            }

            instance = CallContext<MongoDbClient>.GetData(url + database);
            return instance;
        }

        /// <summary>
        /// 获取表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>(string collection)
        {
            return Database.GetCollection<T>(collection);
        }

        #region 插入

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="t">数据</param>
        public void InsertOne<T>(string collection, T t)
        {
            Database.GetCollection<T>(collection).InsertOne(t);
        }

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="doc">文档</param>
        public void InsertOne(string collection, BsonDocument doc)
        {
            Database.GetCollection<BsonDocument>(collection).InsertOne(doc);
        }

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="t">数据</param>
        public Task InsertOneAsync<T>(string collection, T t)
        {
            return Database.GetCollection<T>(collection).InsertOneAsync(t);
        }

        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="doc">文档</param>
        public Task InsertOneAsync(string collection, BsonDocument doc)
        {
            return Database.GetCollection<BsonDocument>(collection).InsertOneAsync(doc);
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="list">集合</param>
        public void InsertMany<T>(string collection, IEnumerable<T> list)
        {
            Database.GetCollection<T>(collection).InsertMany(list);
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="list">Bson集合</param>
        public void InsertMany(string collection, IEnumerable<BsonDocument> list)
        {
            Database.GetCollection<BsonDocument>(collection).InsertMany(list);
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="list">集合</param>
        public Task InsertManyAsync<T>(string collection, IEnumerable<T> list)
        {
            return Database.GetCollection<T>(collection).InsertManyAsync(list);
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="list">Bson集合</param>
        public Task InsertManyAsync(string collection, IEnumerable<BsonDocument> list)
        {
            return Database.GetCollection<BsonDocument>(collection).InsertManyAsync(list);
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="list">数据集合</param>
        /// <returns></returns>
        public List<WriteModel<T>> BulkInsert<T>(string collection, IEnumerable<WriteModel<T>> list)
        {
            BulkWriteResult<T> result = Database.GetCollection<T>(collection).BulkWrite(list);
            return result.ProcessedRequests.ToList();
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="list">Bson数据集合</param>
        /// <returns></returns>
        public List<WriteModel<BsonDocument>> BulkInsert(string collection, IEnumerable<WriteModel<BsonDocument>> list)
        {
            BulkWriteResult<BsonDocument> result = Database.GetCollection<BsonDocument>(collection).BulkWrite(list);
            return result.ProcessedRequests.ToList();
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="list">数据集合</param>
        /// <returns></returns>
        public async Task<List<WriteModel<T>>> BulkInsertAsync<T>(string collection, IEnumerable<WriteModel<T>> list)
        {
            BulkWriteResult<T> result = await Database.GetCollection<T>(collection).BulkWriteAsync(list);
            return result.ProcessedRequests.ToList();
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="list">Bson数据集合</param>
        /// <returns></returns>
        public async Task<List<WriteModel<BsonDocument>>> BulkInsertAsync(string collection, IEnumerable<WriteModel<BsonDocument>> list)
        {
            BulkWriteResult<BsonDocument> result = await Database.GetCollection<BsonDocument>(collection).BulkWriteAsync(list);
            return result.ProcessedRequests.ToList();
        }

        #endregion 插入

        #region 更新

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public UpdateResult UpdateOne<T>(string collection, Expression<Func<T, bool>> filter, UpdateDefinition<T> update, bool upsert)
        {
            return Database.GetCollection<T>(collection).UpdateOne(filter, update, new UpdateOptions()
            {
                IsUpsert = upsert
            });
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public UpdateResult UpdateOne(string collection, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert)
        {
            return Database.GetCollection<BsonDocument>(collection).UpdateOne(filter, update, new UpdateOptions()
            {
                IsUpsert = upsert
            });
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateOneAsync<T>(string collection, Expression<Func<T, bool>> filter, UpdateDefinition<T> update, bool upsert)
        {
            return await Database.GetCollection<T>(collection).UpdateOneAsync(filter, update, new UpdateOptions()
            {
                IsUpsert = upsert
            });
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="collection">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateOneAsync(string collection, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert)
        {
            return await Database.GetCollection<BsonDocument>(collection).UpdateOneAsync(filter, update, new UpdateOptions()
            {
                IsUpsert = upsert
            });
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public long UpdateMany<T>(string collName, Expression<Func<T, bool>> filter, UpdateDefinition<T> update, bool upsert = false)
        {
            UpdateResult result = Database.GetCollection<T>(collName).UpdateMany(filter, update, new UpdateOptions
            {
                IsUpsert = upsert
            });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public long UpdateMany(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert = false)
        {
            UpdateResult result = Database.GetCollection<BsonDocument>(collName).UpdateMany(filter, update, new UpdateOptions
            {
                IsUpsert = upsert
            });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改多个文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public async Task<long> UpdateManyAsync<T>(string collName, Expression<Func<T, bool>> filter, UpdateDefinition<T> update, bool upsert = false)
        {
            UpdateResult result = await Database.GetCollection<T>(collName).UpdateManyAsync(filter, update, new UpdateOptions
            {
                IsUpsert = upsert
            });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改多个文档
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public async Task<long> UpdateManyAsync(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert = false)
        {
            UpdateResult result = await Database.GetCollection<BsonDocument>(collName).UpdateManyAsync(filter, update, new UpdateOptions
            {
                IsUpsert = upsert
            });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的数据</param>
        /// <returns></returns>
        public T UpdateOne<T>(string collName, Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            T result = Database.GetCollection<T>(collName).FindOneAndUpdate(filter, update);
            return result;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的Bson数据</param>
        /// <returns></returns>
        public BsonDocument UpdateOne(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update)
        {
            BsonDocument result = Database.GetCollection<BsonDocument>(collName).FindOneAndUpdate(filter, update);
            return result;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的数据</param>
        /// <returns></returns>
        public async Task<T> UpdateOneAsync<T>(string collName, Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            T result = await Database.GetCollection<T>(collName).FindOneAndUpdateAsync(filter, update);
            return result;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的Bson数据</param>
        /// <returns></returns>
        public async Task<BsonDocument> UpdateOneAsync(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update)
        {
            BsonDocument result = await Database.GetCollection<BsonDocument>(collName).FindOneAndUpdateAsync(filter, update);
            return result;
        }

        #endregion 更新

        #region 删除

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long Delete<T>(string collection, BsonDocument document)
        {
            DeleteResult result = Database.GetCollection<T>(collection).DeleteOne(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long DeleteMany<T>(string collection, BsonDocument document)
        {
            DeleteResult result = Database.GetCollection<T>(collection).DeleteMany(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long Delete(string collection, BsonDocument document)
        {
            DeleteResult result = Database.GetCollection<BsonDocument>(collection).DeleteOne(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long DeleteMany(string collection, BsonDocument document)
        {
            DeleteResult result = Database.GetCollection<BsonDocument>(collection).DeleteMany(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<T>(string collection, BsonDocument document)
        {
            DeleteResult result = await Database.GetCollection<T>(collection).DeleteOneAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<T>(string collection, BsonDocument document)
        {
            DeleteResult result = await Database.GetCollection<T>(collection).DeleteManyAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collection, BsonDocument document)
        {
            DeleteResult result = await Database.GetCollection<BsonDocument>(collection).DeleteOneAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collection">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collection, BsonDocument document)
        {
            DeleteResult result = await Database.GetCollection<BsonDocument>(collection).DeleteManyAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long Delete<T>(string collName, string json)
        {
            var result = Database.GetCollection<T>(collName).DeleteOne(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long DeleteMany<T>(string collName, string json)
        {
            var result = Database.GetCollection<T>(collName).DeleteMany(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long Delete(string collName, string json)
        {
            var result = Database.GetCollection<BsonDocument>(collName).DeleteOne(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long DeleteMany(string collName, string json)
        {
            var result = Database.GetCollection<BsonDocument>(collName).DeleteMany(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<T>(string collName, string json)
        {
            var result = await Database.GetCollection<T>(collName).DeleteOneAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<T>(string collName, string json)
        {
            var result = await Database.GetCollection<T>(collName).DeleteManyAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collName, string json)
        {
            var result = await Database.GetCollection<BsonDocument>(collName).DeleteOneAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collName, string json)
        {
            var result = await Database.GetCollection<BsonDocument>(collName).DeleteManyAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long Delete<T>(string collName, Expression<Func<T, bool>> predicate)
        {
            var result = Database.GetCollection<T>(collName).DeleteOne(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long DeleteMany<T>(string collName, Expression<Func<T, bool>> predicate)
        {
            var result = Database.GetCollection<T>(collName).DeleteMany(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long Delete(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = Database.GetCollection<BsonDocument>(collName).DeleteOne(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long DeleteMany(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = Database.GetCollection<BsonDocument>(collName).DeleteMany(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<T>(string collName, Expression<Func<T, bool>> predicate)
        {
            var result = await Database.GetCollection<T>(collName).DeleteOneAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<T>(string collName, Expression<Func<T, bool>> predicate)
        {
            var result = await Database.GetCollection<T>(collName).DeleteManyAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = await Database.GetCollection<BsonDocument>(collName).DeleteOneAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = await Database.GetCollection<BsonDocument>(collName).DeleteManyAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long Delete<T>(string collName, FilterDefinition<T> filter)
        {
            var result = Database.GetCollection<T>(collName).DeleteOne(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long DeleteMany<T>(string collName, FilterDefinition<T> filter)
        {
            var result = Database.GetCollection<T>(collName).DeleteMany(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long Delete(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = Database.GetCollection<BsonDocument>(collName).DeleteOne(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long DeleteMany(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = Database.GetCollection<BsonDocument>(collName).DeleteMany(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<T>(string collName, FilterDefinition<T> filter)
        {
            var result = await Database.GetCollection<T>(collName).DeleteOneAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<T>(string collName, FilterDefinition<T> filter)
        {
            var result = await Database.GetCollection<T>(collName).DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = await Database.GetCollection<BsonDocument>(collName).DeleteOneAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = await Database.GetCollection<BsonDocument>(collName).DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public T DeleteOne<T>(string collName, Expression<Func<T, bool>> filter)
        {
            T result = Database.GetCollection<T>(collName).FindOneAndDelete(filter);
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public BsonDocument DeleteOne(string collName, Expression<Func<BsonDocument, bool>> filter)
        {
            BsonDocument result = Database.GetCollection<BsonDocument>(collName).FindOneAndDelete(filter);
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<T> DeleteOneAsync<T>(string collName, Expression<Func<T, bool>> filter)
        {
            T result = await Database.GetCollection<T>(collName).FindOneAndDeleteAsync(filter);
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<BsonDocument> DeleteOneAsync(string collName, Expression<Func<BsonDocument, bool>> filter)
        {
            BsonDocument result = await Database.GetCollection<BsonDocument>(collName).FindOneAndDeleteAsync(filter);
            return result;
        }

        #endregion 删除

        #region 查询

        /// <summary>
        /// 查询，复杂查询直接用Linq处理
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <returns>要查询的对象</returns>
        public IMongoQueryable<T> GetQueryable<T>(string collName)
        {
            return Database.GetCollection<T>(collName).AsQueryable();
        }

        /// <summary>
        /// 查询，复杂查询直接用Linq处理
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <returns>要查询的对象</returns>
        public IMongoQueryable<BsonDocument> GetQueryable(string collName)
        {
            return Database.GetCollection<BsonDocument>(collName).AsQueryable();
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public T Get<T>(string collName, FilterDefinition<T> filter)
        {
            IFindFluent<T, T> find = Database.GetCollection<T>(collName).Find(filter);
            return find.FirstOrDefault();
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public BsonDocument Get(string collName, FilterDefinition<BsonDocument> filter)
        {
            IFindFluent<BsonDocument, BsonDocument> find = Database.GetCollection<BsonDocument>(collName).Find(filter);
            return find.FirstOrDefault();
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string collName, FilterDefinition<T> filter)
        {
            IAsyncCursor<T> find = await Database.GetCollection<T>(collName).FindAsync(filter);
            return await find.FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<BsonDocument> GetAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            IAsyncCursor<BsonDocument> find = await Database.GetCollection<BsonDocument>(collName).FindAsync(filter);
            return await find.FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public IEnumerable<T> GetMany<T>(string collName, FilterDefinition<T> filter)
        {
            IFindFluent<T, T> find = Database.GetCollection<T>(collName).Find(filter);
            return find.ToEnumerable();
        }

        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public IEnumerable<BsonDocument> GetMany(string collName, FilterDefinition<BsonDocument> filter)
        {
            IFindFluent<BsonDocument, BsonDocument> find = Database.GetCollection<BsonDocument>(collName).Find(filter);
            return find.ToEnumerable();
        }

        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetManyAsync<T>(string collName, FilterDefinition<T> filter)
        {
            IAsyncCursor<T> find = await Database.GetCollection<T>(collName).FindAsync(filter);
            return find.ToEnumerable();
        }

        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<IEnumerable<BsonDocument>> GetManyAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            IAsyncCursor<BsonDocument> find = await Database.GetCollection<BsonDocument>(collName).FindAsync(filter);
            return find.ToEnumerable();
        }

        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool Any<T>(string collName, FilterDefinition<T> filter)
        {
            IFindFluent<T, T> find = Database.GetCollection<T>(collName).Find(filter);
            return find.Any();
        }

        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool Any(string collName, FilterDefinition<BsonDocument> filter)
        {
            IFindFluent<BsonDocument, BsonDocument> find = Database.GetCollection<BsonDocument>(collName).Find(filter);
            return find.Any();
        }

        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<bool> AnyAsync<T>(string collName, FilterDefinition<T> filter)
        {
            IAsyncCursor<T> find = await Database.GetCollection<T>(collName).FindAsync(filter);
            return await find.AnyAsync();
        }

        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<bool> AnyAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            IAsyncCursor<BsonDocument> find = await Database.GetCollection<BsonDocument>(collName).FindAsync(filter);
            return await find.AnyAsync();
        }

        #endregion 查询

        #region 索引

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string CreateIndex(string collection, string index, bool asc = true)
        {
            IMongoIndexManager<BsonDocument> mgr = Database.GetCollection<BsonDocument>(collection).Indexes;
            var list = mgr.List();
            while (list.MoveNext())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return mgr.CreateOne(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> CreateIndexAsync(string collection, string index, bool asc = true)
        {
            IMongoIndexManager<BsonDocument> mgr = Database.GetCollection<BsonDocument>(collection).Indexes;
            var list = mgr.List();
            while (list.MoveNext())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return await mgr.CreateOneAsync(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string UpdateIndex(string collection, string index, bool asc = true)
        {
            IMongoIndexManager<BsonDocument> mgr = Database.GetCollection<BsonDocument>(collection).Indexes;
            return mgr.CreateOne(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> UpdateIndexAsync(string collection, string index, bool asc = true)
        {
            IMongoIndexManager<BsonDocument> mgr = Database.GetCollection<BsonDocument>(collection).Indexes;
            return await mgr.CreateOneAsync(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <returns></returns>
        public void DropIndex(string collection, string index)
        {
            Database.GetCollection<BsonDocument>(collection).Indexes.DropOne(index);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <returns></returns>
        public Task DropIndexAsync(string collection, string index)
        {
            return Database.GetCollection<BsonDocument>(collection).Indexes.DropOneAsync(index);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string CreateIndex<T>(string collection, string index, Expression<Func<T, object>> key, bool asc = true)
        {
            IMongoIndexManager<T> mgr = Database.GetCollection<T>(collection).Indexes;
            var list = mgr.List();
            while (list.MoveNext())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return mgr.CreateOne(new CreateIndexModel<T>(asc ? Builders<T>.IndexKeys.Ascending(key) : Builders<T>.IndexKeys.Descending(key)));
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> CreateIndexAsync<T>(string collection, string index, Expression<Func<T, object>> key, bool asc = true)
        {
            IMongoIndexManager<T> mgr = Database.GetCollection<T>(collection).Indexes;
            var list = mgr.List();
            while (list.MoveNext())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return await mgr.CreateOneAsync(new CreateIndexModel<T>(asc ? Builders<T>.IndexKeys.Ascending(key) : Builders<T>.IndexKeys.Descending(key)));
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string UpdateIndex<T>(string collection, Expression<Func<T, object>> key, bool asc = true)
        {
            IMongoIndexManager<T> mgr = Database.GetCollection<T>(collection).Indexes;
            return mgr.CreateOne(new CreateIndexModel<T>(asc ? Builders<T>.IndexKeys.Ascending(key) : Builders<T>.IndexKeys.Descending(key)));
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="collection">集合名</param>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> UpdateIndexAsync<T>(string collection, Expression<Func<T, object>> key, bool asc = true)
        {
            IMongoIndexManager<T> mgr = Database.GetCollection<T>(collection).Indexes;
            return await mgr.CreateOneAsync(new CreateIndexModel<T>(asc ? Builders<T>.IndexKeys.Ascending(key) : Builders<T>.IndexKeys.Descending(key)));
        }

        #endregion 索引
    }
}
