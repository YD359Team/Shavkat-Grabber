using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Microsoft.Playwright;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Logic.Abstract;

public abstract class DriverBase : IDisposable
{
    public delegate void LogMessageDelegate(LogMessage logMessage);

    protected IPlaywright _playwright { get; private set; }
    protected IBrowser _browser { get; private set; }
    protected IPage _page { get; private set; }
    protected AppSettings _settings { get; private set; }
    protected FileSystemManager _fsManager { get; private set; }

    protected bool _isClosed = false;

    private void Init(
        IPlaywright playwright,
        IBrowser browser,
        IPage page,
        AppSettings settings,
        FileSystemManager fsManager
    )
    {
        _playwright = playwright;
        _browser = browser;
        _page = page;
        _settings = settings;
        _fsManager = fsManager;
    }

    public static async Task<T> CreateAsync<T>(FileSystemManager fsManager, AppSettings appSettings)
        where T : DriverBase, new()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(
            new() { Headless = false, ExecutablePath = appSettings.ChromePath }
        );
        var page = await browser.NewPageAsync();
        T t = new T();
        t.Init(playwright, browser, page, appSettings, fsManager);
        return t;
    }

    private async Task CloseAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
        _isClosed = true;
    }

    public void Dispose()
    {
        if (!_isClosed)
        {
            CloseAsync();
        }
    }
}
