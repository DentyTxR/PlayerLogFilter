using Avalonia.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using PlayerLogFilter.Enums;
using PlayerLogFilter.Methods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLogFilter.Models
{
    public partial class ProgramConfig : ConfigModule<ProgramConfig>
    {
        [ObservableProperty]
        [property: Config(
            Header = "Theme",
            Description = "Changes the UI theme",
            Category = "Appearance",
            Group = "UI")]
        private ThemeOption _selectedTheme = ThemeOption.Dark;
    }
}