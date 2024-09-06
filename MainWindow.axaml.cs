using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            keywordToDictMapping = new Dictionary<string, Action<string>>
            {
                { "GPU Driver", line => graphicInfoDict["GPU Driver"] = ExtractValue(line) },
                { "RAM", line => systemInfoDict["RAM"] = ExtractValue(line) },
                { "CPU", line => systemInfoDict["CPU"] = ExtractValue(line) },
                { "Game version", line => gameInfoDict["Game version"] = ExtractValue(line) },
            };
            
            importantKeywordMapping = new Dictionary<string, Action<string>>
            {
                { "CRASH!!!", line => importantInfoDict["CRASH!!!"] = line },
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
            {
                _settingsWindow.Focus();
            }
        }

        private async void OnUploadFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Log File",
            };

            var result = await openFileDialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                FilePathTextBlock.Text = result[0];
            }
        }

        private void OnProcessLogClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathTextBlock.Text) || !File.Exists(FilePathTextBlock.Text))
            {
                return;
            }

            string logData = File.ReadAllText(FilePathTextBlock.Text);
            FilterLogData(logData);

            // Display filtered data in corresponding text boxes
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
            // Clear previous data
            graphicInfoDict.Clear();
            gameInfoDict.Clear();
            systemInfoDict.Clear();
            importantInfoDict.Clear();

            foreach (var line in logData.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                // Process important keywords first
                foreach (var kvp in importantKeywordMapping)
                {
                    if (line.Contains(kvp.Key))
                    {
                        kvp.Value(line);
                        break; // Stop processing after finding the relevant keyword
                    }
                }

                // Process regular keywords
                foreach (var kvp in keywordToDictMapping)
                {
                    if (line.Contains(kvp.Key))
                    {
                        kvp.Value(line);
                        break; // Stop processing after finding the relevant keyword
                    }
                }
            }
        }

        private string ExtractValue(string line)
        {
            var parts = line.Split(new[] { ':' }, 2);
            return parts.Length > 1 ? parts[1].Trim() : string.Empty;
        }

        private string FormatDictionary(Dictionary<string, string> dictionary)
        {
            return string.Join(Environment.NewLine, dictionary.Select(kv => $"{kv.Key}: {kv.Value}"));
        }
    }
}