using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using ConfigFactory.Avalonia.Helpers;
using PlayerLogFilter.Models;
using static PlayerLogFilter.Models.ProgramConfig;
using System;
using ConfigFactory.Models;
using ConfigFactory;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using PlayerLogFilter.Enums;
using PlayerLogFilter.Methods;

namespace PlayerLogFilter;

public partial class App : Application
{
    public static string Version { get; private set; } = "0.4.0";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var programConfig = ProgramConfig.Shared;
        
        SettingsMethods.ApplyTheme(programConfig.SelectedTheme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}