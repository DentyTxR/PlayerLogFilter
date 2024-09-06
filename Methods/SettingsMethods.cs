using Avalonia;
using Avalonia.Styling;
using PlayerLogFilter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLogFilter.Methods
{
    public class SettingsMethods
    {
        public static void ApplyTheme(ThemeOption theme)
        {
            var themeVariant = theme == ThemeOption.Light ? ThemeVariant.Light : ThemeVariant.Dark;
            var app = Application.Current;
            if (app != null && app.RequestedThemeVariant != themeVariant)
            {
                app.RequestedThemeVariant = themeVariant;
            }
        }
    }
}
