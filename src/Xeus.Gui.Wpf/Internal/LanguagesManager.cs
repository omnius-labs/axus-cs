using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Lxna.Gui.Wpf.Internal;

namespace Lxna.Gui.Wpf.Internal
{
    sealed partial class LanguagesManager : INotifyPropertyChanged
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public static LanguagesManager Instance { get; } = new LanguagesManager();

        private Dictionary<string, Dictionary<string, string>> _dic = new Dictionary<string, Dictionary<string, string>>();
        private string _currentLanguage;

        private LanguagesManager()
        {
            if ((bool)(System.ComponentModel.DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue))
            {
                string path = @"C:\Local\Projects\OmniusLabs\Lxna\src\Lxna.Gui.Wpf\Languages";
                if (!Directory.Exists(path)) path = LxnaEnvironment.Paths.LanguagesDirectoryPath;

                this.Load(path);
            }
            else
            {
                this.Load(LxnaEnvironment.Paths.LanguagesDirectoryPath);
            }
        }

        private void Load(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            _dic.Clear();

            foreach (string path in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var dic = new Dictionary<string, string>();

                    using (var xml = new XmlTextReader(path))
                    {
                        while (xml.Read())
                        {
                            if (xml.NodeType == XmlNodeType.Element)
                            {
                                if (xml.LocalName == "Property")
                                {
                                    dic[xml.GetAttribute("Name")] = xml.GetAttribute("Value");
                                }
                            }
                        }
                    }

                    _dic[Path.GetFileNameWithoutExtension(path)] = dic;
                }
                catch (XmlException e)
                {
                    _logger.Debug(e, "Load");
                }
            }

            this.SetCurrentLanguage("English");
        }

        public IEnumerable<string> Languages
        {
            get
            {
                var dic = new Dictionary<string, string>();

                foreach (string path in _dic.Keys.ToList())
                {
                    dic[System.IO.Path.GetFileNameWithoutExtension(path)] = path;
                }

                var pairs = dic.ToList();

                pairs.Sort((x, y) =>
                {
                    return x.Key.CompareTo(y.Key);
                });

                return pairs.Select(n => n.Value).ToArray();
            }
        }

        public string CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
        }

        public void SetCurrentLanguage(string key)
        {
            if (!_dic.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

            _currentLanguage = key;
            this.OnPropertyChanged(null);
        }

        public string Translate(string key)
        {
            if (_currentLanguage == null) return null;

            if (_dic[_currentLanguage].TryGetValue(key, out string result))
            {
                return Regex.Unescape(result);
            }

            return null;
        }
    }
}
