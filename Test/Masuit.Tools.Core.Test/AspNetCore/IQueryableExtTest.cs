using System;
using System.Linq;
using System.Threading.Tasks;
using Masuit.Tools.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class IQueryableExtTests
    {
        private DbContextOptions<TestDbContext> CreateNewContextOptions()
        {
            var builder = new DbContextOptionsBuilder<TestDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            return builder.Options;
        }

        [Fact]
        public async Task ToPagedListAsync_ShouldReturnPagedList()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            for (int i = 1; i <= 20; i++)
            {
                context.TestEntities.Add(new TestEntity { Name = $"Entity{i}" });
            }
            await context.SaveChangesAsync();

            // Act
            var pagedList = await context.TestEntities.AsQueryable().ToPagedListAsync(2, 5);

            // Assert
            Assert.Equal(2, pagedList.CurrentPage);
            Assert.Equal(5, pagedList.PageSize);
            Assert.Equal(20, pagedList.TotalCount);
            Assert.Equal(4, pagedList.TotalPages);
            Assert.Equal(5, pagedList.Data.Count);
        }

        [Fact]
        public async Task ToPagedListNoLockAsync_ShouldReturnPagedList()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            for (int i = 1; i <= 20; i++)
            {
                context.TestEntities.Add(new TestEntity { Name = $"Entity{i}" });
            }
            await context.SaveChangesAsync();

            // Act
            var pagedList = await context.TestEntities.AsQueryable().ToPagedListNoLockAsync(2, 5);

            // Assert
            Assert.Equal(2, pagedList.CurrentPage);
            Assert.Equal(5, pagedList.PageSize);
            Assert.Equal(20, pagedList.TotalCount);
            Assert.Equal(4, pagedList.TotalPages);
            Assert.Equal(5, pagedList.Data.Count);
        }

        [Fact]
        public void ToPagedList_ShouldReturnPagedList()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            for (int i = 1; i <= 20; i++)
            {
                context.TestEntities.Add(new TestEntity { Name = $"Entity{i}" });
            }
            context.SaveChanges();

            // Act
            var pagedList = context.TestEntities.AsQueryable().ToPagedList(2, 5);

            // Assert
            Assert.Equal(2, pagedList.CurrentPage);
            Assert.Equal(5, pagedList.PageSize);
            Assert.Equal(20, pagedList.TotalCount);
            Assert.Equal(4, pagedList.TotalPages);
            Assert.Equal(5, pagedList.Data.Count);
        }

        [Fact]
        public void ToPagedListNoLock_ShouldReturnPagedList()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            for (int i = 1; i <= 20; i++)
            {
                context.TestEntities.Add(new TestEntity { Name = $"Entity{i}" });
            }
            context.SaveChanges();

            // Act
            var pagedList = context.TestEntities.AsQueryable().ToPagedListNoLock(2, 5);

            // Assert
            Assert.Equal(2, pagedList.CurrentPage);
            Assert.Equal(5, pagedList.PageSize);
            Assert.Equal(20, pagedList.TotalCount);
            Assert.Equal(4, pagedList.TotalPages);
            Assert.Equal(5, pagedList.Data.Count);
        }

        [Fact]
        public void ToPagedList_FromEnumerable_ShouldReturnPagedList()
        {
            // Arrange
            var list = Enumerable.Range(1, 20).Select(i => new TestEntity { Id = i, Name = $"Entity{i}" });

            // Act
            var pagedList = list.ToPagedList(2, 5);

            // Assert
            Assert.Equal(2, pagedList.CurrentPage);
            Assert.Equal(5, pagedList.PageSize);
            Assert.Equal(20, pagedList.TotalCount);
            Assert.Equal(4, pagedList.TotalPages);
            Assert.Equal(5, pagedList.Data.Count);
        }
    }
}