using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Masuit.Tools.Core;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class TestDbContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
}

public class DbContextExtTests
{
    private DbContextOptions<TestDbContext> CreateNewContextOptions()
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        return builder.Options;
    }

    [Fact]
    public void GetAllChanges_ShouldReturnAllChanges()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        var entity1 = new TestEntity { Name = "Original" };
        var entity2 = new TestEntity { Name = "New" };
        context.TestEntities.Add(entity1);
        context.TestEntities.Add(entity2);
        context.SaveChanges();

        entity1.Name = "Modified";
        context.TestEntities.Update(entity1);
        context.TestEntities.Remove(entity2);

        // Act
        var allChanges = context.GetAllChanges<TestEntity>().ToList();

        // Assert
        Assert.Equal(2, allChanges.Count);
        Assert.Contains(allChanges, e => e.EntityState == EntityState.Modified);
        Assert.Contains(allChanges, e => e.EntityState == EntityState.Deleted);
    }

    [Fact]
    public async Task ToListWithNoLockAsync_ShouldReturnEntities()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        await context.SaveChangesAsync();

        // Act
        var entities = await context.TestEntities.ToListWithNoLockAsync();

        // Assert
        Assert.Equal(2, entities.Count);
    }

    [Fact]
    public void ToListWithNoLock_ShouldReturnEntities()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        context.SaveChanges();

        // Act
        var entities = context.TestEntities.ToListWithNoLock();

        // Assert
        Assert.Equal(2, entities.Count);
    }

    [Fact]
    public async Task CountWithNoLockAsync_ShouldReturnCount()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        await context.SaveChangesAsync();

        // Act
        var count = await context.TestEntities.CountWithNoLockAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void CountWithNoLock_ShouldReturnCount()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        context.SaveChanges();

        // Act
        var count = context.TestEntities.CountWithNoLock();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task AnyWithNoLockAsync_ShouldReturnTrueIfAny()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        await context.SaveChangesAsync();

        // Act
        var any = await context.TestEntities.AnyWithNoLockAsync();

        // Assert
        Assert.True(any);
    }

    [Fact]
    public void AnyWithNoLock_ShouldReturnTrueIfAny()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.SaveChanges();

        // Act
        var any = context.TestEntities.AnyWithNoLock();

        // Assert
        Assert.True(any);
    }

    [Fact]
    public async Task FirstOrDefaultWithNoLockAsync_ShouldReturnFirstEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        await context.SaveChangesAsync();

        // Act
        var entity = await context.TestEntities.FirstOrDefaultWithNoLockAsync(e => e.Name == "Entity1");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Entity1", entity.Name);
    }

    [Fact]
    public void FirstOrDefaultWithNoLock_ShouldReturnFirstEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        context.SaveChanges();

        // Act
        var entity = context.TestEntities.FirstOrDefaultWithNoLock(e => e.Name == "Entity1");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Entity1", entity.Name);
    }

    [Fact]
    public async Task SingleOrDefaultWithNoLockAsync_ShouldReturnSingleEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        await context.SaveChangesAsync();

        // Act
        var entity = await context.TestEntities.SingleOrDefaultWithNoLockAsync(e => e.Name == "Entity1");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Entity1", entity.Name);
    }

    [Fact]
    public void SingleOrDefaultWithNoLock_ShouldReturnSingleEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.SaveChanges();

        // Act
        var entity = context.TestEntities.SingleOrDefaultWithNoLock(e => e.Name == "Entity1");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Entity1", entity.Name);
    }

    [Fact]
    public async Task AllWithNoLockAsync_ShouldReturnTrueIfAllMatch()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        await context.SaveChangesAsync();

        // Act
        var all = await context.TestEntities.AllWithNoLockAsync(e => e.Name.StartsWith("Entity"));

        // Assert
        Assert.True(all);
    }

    [Fact]
    public void AllWithNoLock_ShouldReturnTrueIfAllMatch()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.TestEntities.Add(new TestEntity { Name = "Entity2" });
        context.SaveChanges();

        // Act
        var all = context.TestEntities.AllWithNoLock(e => e.Name.StartsWith("Entity"));

        // Assert
        Assert.True(all);
    }

    [Fact]
    public void NoLock_ShouldExecuteWithNoLock()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        context.SaveChanges();

        // Act
        var result = context.NoLock(ctx => ctx.TestEntities.Count());

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task NoLock_ShouldExecuteWithNoLockAsync()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Name = "Entity1" });
        await context.SaveChangesAsync();

        // Act
        var result = await context.NoLock(async ctx => await ctx.TestEntities.CountAsync());

        // Assert
        Assert.Equal(1, result);
    }
}