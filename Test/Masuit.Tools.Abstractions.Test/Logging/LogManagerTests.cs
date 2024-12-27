using System;
using System.IO;
using System.Linq;
using System.Threading;
using Masuit.Tools.Logging;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Logging;

public class LogManagerTests
{
    private readonly string _logDirectory;

    public LogManagerTests()
    {
        _logDirectory = Path.Combine(Path.GetTempPath(), "logs");
        LogManager.LogDirectory = _logDirectory;

        if (Directory.Exists(_logDirectory))
        {
            Directory.Delete(_logDirectory, true);
        }

        Directory.CreateDirectory(_logDirectory);
    }

    [Fact]
    public void Info_ShouldWriteInfoLog()
    {
        LogManager.Info("Test info message");
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("INFO", logContent);
        Assert.Contains("Test info message", logContent);
    }

    [Fact]
    public void Info_WithSource_ShouldWriteInfoLog()
    {
        LogManager.Info("TestSource", "Test info message");
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("INFO", logContent);
        Assert.Contains("TestSource", logContent);
        Assert.Contains("Test info message", logContent);
    }

    [Fact]
    public void Debug_ShouldWriteDebugLog()
    {
        LogManager.Debug("Test debug message");
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("DEBUG", logContent);
        Assert.Contains("Test debug message", logContent);
    }

    [Fact]
    public void Debug_WithSource_ShouldWriteDebugLog()
    {
        LogManager.Debug("TestSource", "Test debug message");
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("DEBUG", logContent);
        Assert.Contains("TestSource", logContent);
        Assert.Contains("Test debug message", logContent);
    }

    [Fact]
    public void Error_ShouldWriteErrorLog()
    {
        var exception = new Exception("Test error message");
        LogManager.Error(exception);
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("ERROR", logContent);
        Assert.Contains("Test error message", logContent);
    }

    [Fact]
    public void Error_WithSource_ShouldWriteErrorLog()
    {
        var exception = new Exception("Test error message");
        LogManager.Error("TestSource", exception);
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("ERROR", logContent);
        Assert.Contains("TestSource", logContent);
        Assert.Contains("Test error message", logContent);
    }

    [Fact]
    public void Fatal_ShouldWriteFatalLog()
    {
        var exception = new Exception("Test fatal message");
        LogManager.Fatal(exception);
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("FATAL", logContent);
        Assert.Contains("Test fatal message", logContent);
    }

    [Fact]
    public void Fatal_WithSource_ShouldWriteFatalLog()
    {
        var exception = new Exception("Test fatal message");
        LogManager.Fatal("TestSource", exception);
        Thread.Sleep(2000); // 等待日志写入

        var logFile = Directory.GetFiles(_logDirectory).FirstOrDefault();
        Assert.NotNull(logFile);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("FATAL", logContent);
        Assert.Contains("TestSource", logContent);
        Assert.Contains("Test fatal message", logContent);
    }

    [Fact]
    public void LogDirectory_ShouldSetAndGetLogDirectory()
    {
        var customLogDirectory = Path.Combine(Path.GetTempPath(), "custom_logs");
        LogManager.LogDirectory = customLogDirectory;

        Assert.Equal(customLogDirectory, LogManager.LogDirectory);
    }

    [Fact]
    public void Event_ShouldTriggerOnLog()
    {
        var eventTriggered = false;
        LogManager.Event += log => eventTriggered = true;

        LogManager.Info("Test info message");
        Thread.Sleep(2000); // 等待日志写入

        Assert.True(eventTriggered);
    }
}