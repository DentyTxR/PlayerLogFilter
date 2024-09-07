using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ConfigFactory;
using ConfigFactory.Avalonia;
using ConfigFactory.Models;
using PlayerLogFilter.Models;

namespace PlayerLogFilter
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SettingsWindow? _settingsWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string _importantInfoTabHeader;
        private Dictionary<string, string> importantInfoDict = new();
        private Dictionary<string, string> graphicInfoDict = new();
        private Dictionary<string, string> gameInfoDict = new();
        private Dictionary<string, string> systemInfoDict = new();

        private readonly Dictionary<string, Action<string>> keywordToDictMapping;
        private readonly Dictionary<string, Action<string>> importantKeywordMapping;

        public string ImportantInfoTabHeader
        {
            get => _importantInfoTabHeader;
            set
            {
                if (_importantInfoTabHeader != value)
                {
                    _importantInfoTabHeader = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            this.Closing += OnMainWindowClosing;

            ImportantInfoTabHeader = "Important Info (OK)";

            //Important stuff
            importantKeywordMapping = new Dictionary<string, Action<string>>
            {
                { "CRASH!!!", line => importantInfoDict["The user crashed"] = line },
                { "Starting microphone failed: \"Error initializing output device. \" (60)", line => importantInfoDict["Starting microphone error (60)"] = line },
                { "Starting microphone failed: \"Unsupported file or audio format. \" (25)", line => importantInfoDict["Starting microphone error (25)"] = line },

            };
            
            keywordToDictMapping = new Dictionary<string, Action<string>>
            {
            //Graphic
                { "GPU:", line => graphicInfoDict["GPU Model"] = ExtractValue(line) },
                { "Vendor:", line => graphicInfoDict["GPU Vendor"] = ExtractValue(line) },
                { "VRAM:", line => graphicInfoDict["GPU VRAM"] = ExtractValue(line) },
                { "GPU Driver:", line => graphicInfoDict["GPU Driver"] = ExtractValue(line) },
                { "Info:", line => graphicInfoDict["Graphic API (In-Game)"] = ExtractValue(line) },
                { "Resolution:", line => graphicInfoDict["Screen Resolution (In-Game)"] = ExtractValue(line) },

            //Game
                { "Game version:", line => gameInfoDict["Game Version"] = ExtractValue(line) },
                { "Preauth version:", line => gameInfoDict["Preauth Version"] = ExtractValue(line) },
                { "Build type:", line => gameInfoDict["Build Type"] = ExtractValue(line) },
                { "Unity:", line => gameInfoDict["Unity Version"] = ExtractValue(line) },
                { "Unity version:", line => gameInfoDict["Unity Version"] = ExtractValue(line) },

            //System
                { "OS:", line => systemInfoDict["OS"] = ExtractValue(line) },
                { "CPU:", line => systemInfoDict["CPU"] = ExtractValue(line) },
                { "Threads:", line => systemInfoDict["CPU Threads"] = ExtractValue(line) },
                { "Frequency:", line => systemInfoDict["CPU MHz"] = ExtractValue(line) },
                { "RAM:", line => systemInfoDict["System Memory"] = ExtractValue(line) },
            };
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            _settingsWindow?.Close();
        }

        private void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Closed += (s, args) => _settingsWindow = null;
                _settingsWindow.Show();
            }
            else
                _settingsWindow.Focus();
        }

        private async void OnUploadFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Log File",
            };

            var result = await openFileDialog.ShowAsync(this);

            if (result != null && result.Length > 0)
                FilePathTextBlock.Text = result[0];
        }

        private void OnProcessLogClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathTextBlock.Text) || !File.Exists(FilePathTextBlock.Text))
            {
                return;
            }

            string logData = File.ReadAllText(FilePathTextBlock.Text);
            FilterLogData(logData);

            GraphicInfoTextBox.Text = FormatDictionary(graphicInfoDict);
            GameInfoTextBox.Text = FormatDictionary(gameInfoDict);
            SystemInfoTextBox.Text = FormatDictionary(systemInfoDict);
            ImportantInfoTextBox.Text = FormatDictionary(importantInfoDict);

            ImportantInfoTabHeader = importantInfoDict.Any() ? "Important Info ⚠️" : "Important Info (OK)";
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            FilePathTextBlock.Text = "No file selected";
            GraphicInfoTextBox.Text = string.Empty;
            GameInfoTextBox.Text = string.Empty;
            SystemInfoTextBox.Text = string.Empty;
            ImportantInfoTextBox.Text = string.Empty;

            ImportantInfoTabHeader = "Important Info (OK)";
        }

        private void FilterLogData(string logData)
        {
            graphicInfoDict.Clear();
            gameInfoDict.Clear();
            systemInfoDict.Clear();
            importantInfoDict.Clear();

            var lines = logData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                // Check and handle the important information first
                foreach (var kvp in importantKeywordMapping)
                {
                    if (line.Trim().StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        kvp.Value(line);
                        break;
                    }
                }

                // Handle graphic information
                foreach (var kvp in keywordToDictMapping)
                {
                    if (line.Trim().StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        kvp.Value(line);
                        break;
                    }
                }
            }
        }

        private string ExtractValue(string line)
        {
            var parts = line.Split(new[] { ':' }, 2);

            if (parts.Length > 1)
            {
                var value = parts[1].Trim();

                value = value.Split('(')[0].Trim();

                return value;
            }
            return "Value not found";
        }

        private string FormatDictionary(Dictionary<string, string> dictionary)
        {
            var sortedDictionary = dictionary.OrderBy(kv => kv.Key);
            return sortedDictionary.Count() > 0
                ? string.Join(Environment.NewLine, sortedDictionary.Select(kv => $"{kv.Key}: {kv.Value}")) : "No relevant data found.";
        }
    }
}