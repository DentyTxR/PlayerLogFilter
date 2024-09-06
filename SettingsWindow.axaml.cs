using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Models;
using PlayerLogFilter.Enums;
using PlayerLogFilter.Methods;
using PlayerLogFilter.Models;
using System;
using System.ComponentModel;

namespace PlayerLogFilter;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        var configPageModel = ConfigFactory.ConfigFactory.Build<ProgramConfig>();

        ConfigPage.DataContext = configPageModel;

        ApplyTheme(ProgramConfig.Shared.SelectedTheme);

        ProgramConfig.Shared.PropertyChanged += OnConfigPropertyChanged;
    }

    private void OnConfigPropertyChanged(object sender, PropertyChangedEventArgs ev)
    {
        if (ev.PropertyName == nameof(ProgramConfig.SelectedTheme))
        {
            ApplyTheme(ProgramConfig.Shared.SelectedTheme);
        }
    }

    private void ApplyTheme(ThemeOption theme)
    {
        var themeVariant = theme == ThemeOption.Light ? ThemeVariant.Light : ThemeVariant.Dark;
        Application.Current.RequestedThemeVariant = themeVariant;
    }
}