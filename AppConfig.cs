using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Drawing;

namespace LanguageLayoutOsd
{
    [DataContract]
    internal sealed class AppConfig
    {
        [DataMember(Name = "displayDurationMs")]
        public int DisplayDurationMs { get; set; }

        [DataMember(Name = "fontSizePt")]
        public float FontSizePt { get; set; }

        [DataMember(Name = "pollingIntervalMs")]
        public int PollingIntervalMs { get; set; }

        [DataMember(Name = "defaultBackColor")]
        public string DefaultBackColor { get; set; }

        [DataMember(Name = "defaultTextColor")]
        public string DefaultTextColor { get; set; }

        [DataMember(Name = "ruBackColor")]
        public string RuBackColor { get; set; }

        [DataMember(Name = "ruTextColor")]
        public string RuTextColor { get; set; }

        [DataMember(Name = "languageStyles")]
        public Dictionary<string, LanguageStyle> LanguageStyles { get; set; }

        public static AppConfig Load()
        {
            var defaults = CreateDefault();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "osd-config.json");

            if (!File.Exists(path))
            {
                return defaults;
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var serializer = new DataContractJsonSerializer(typeof(AppConfig));
                    var parsed = serializer.ReadObject(stream) as AppConfig;
                    return MergeWithDefaults(parsed, defaults);
                }
            }
            catch
            {
                return defaults;
            }
        }

        public Color GetBackgroundColor(string layoutCode)
        {
            var style = GetLanguageStyle(layoutCode);
            if (style != null)
            {
                return ParseColorOrFallback(style.BackColor, Color.FromArgb(160, 40, 40, 40));
            }

            if (string.Equals(layoutCode, "RU", StringComparison.OrdinalIgnoreCase))
            {
                return ParseColorOrFallback(RuBackColor, Color.FromArgb(180, 120, 40, 20));
            }

            return ParseColorOrFallback(DefaultBackColor, Color.FromArgb(160, 40, 40, 40));
        }

        public Color GetTextColor(string layoutCode)
        {
            var style = GetLanguageStyle(layoutCode);
            if (style != null)
            {
                return ParseColorOrFallback(style.TextColor, Color.WhiteSmoke);
            }

            if (string.Equals(layoutCode, "RU", StringComparison.OrdinalIgnoreCase))
            {
                return ParseColorOrFallback(RuTextColor, Color.White);
            }

            return ParseColorOrFallback(DefaultTextColor, Color.WhiteSmoke);
        }

        private static AppConfig CreateDefault()
        {
            return new AppConfig
            {
                DisplayDurationMs = 500,
                FontSizePt = 42.0f,
                PollingIntervalMs = 150,
                DefaultBackColor = "#A0282828",
                DefaultTextColor = "#F5F5F5",
                RuBackColor = "#B4782814",
                RuTextColor = "#FFFFFF",
                LanguageStyles = new Dictionary<string, LanguageStyle>(StringComparer.OrdinalIgnoreCase)
                {
                    { "RU", new LanguageStyle { BackColor = "#B4782814", TextColor = "#FFFFFF" } }
                }
            };
        }

        private static AppConfig MergeWithDefaults(AppConfig parsed, AppConfig defaults)
        {
            if (parsed == null)
            {
                return defaults;
            }

            if (parsed.DisplayDurationMs < 100 || parsed.DisplayDurationMs > 5000)
            {
                parsed.DisplayDurationMs = defaults.DisplayDurationMs;
            }

            if (parsed.FontSizePt < 10 || parsed.FontSizePt > 120)
            {
                parsed.FontSizePt = defaults.FontSizePt;
            }

            if (parsed.PollingIntervalMs < 50 || parsed.PollingIntervalMs > 1000)
            {
                parsed.PollingIntervalMs = defaults.PollingIntervalMs;
            }

            if (string.IsNullOrWhiteSpace(parsed.DefaultBackColor))
            {
                parsed.DefaultBackColor = defaults.DefaultBackColor;
            }

            if (string.IsNullOrWhiteSpace(parsed.DefaultTextColor))
            {
                parsed.DefaultTextColor = defaults.DefaultTextColor;
            }

            if (string.IsNullOrWhiteSpace(parsed.RuBackColor))
            {
                parsed.RuBackColor = defaults.RuBackColor;
            }

            if (string.IsNullOrWhiteSpace(parsed.RuTextColor))
            {
                parsed.RuTextColor = defaults.RuTextColor;
            }

            if (parsed.LanguageStyles == null)
            {
                parsed.LanguageStyles = new Dictionary<string, LanguageStyle>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                parsed.LanguageStyles = NormalizeStyles(parsed.LanguageStyles);
            }

            return parsed;
        }

        private LanguageStyle GetLanguageStyle(string layoutCode)
        {
            if (LanguageStyles == null || string.IsNullOrWhiteSpace(layoutCode))
            {
                return null;
            }

            LanguageStyle style;
            if (LanguageStyles.TryGetValue(layoutCode.Trim(), out style))
            {
                return style;
            }

            return null;
        }

        private static Dictionary<string, LanguageStyle> NormalizeStyles(Dictionary<string, LanguageStyle> styles)
        {
            var normalized = new Dictionary<string, LanguageStyle>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in styles)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                var key = pair.Key.Trim().ToUpperInvariant();
                if (normalized.ContainsKey(key))
                {
                    continue;
                }

                normalized[key] = pair.Value ?? new LanguageStyle();
            }

            return normalized;
        }

        private static Color ParseColorOrFallback(string text, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return fallback;
            }

            text = text.Trim();
            if (!text.StartsWith("#", StringComparison.Ordinal))
            {
                return fallback;
            }

            try
            {
                if (text.Length == 7)
                {
                    return ColorTranslator.FromHtml(text);
                }

                if (text.Length == 9)
                {
                    var a = Convert.ToByte(text.Substring(1, 2), 16);
                    var r = Convert.ToByte(text.Substring(3, 2), 16);
                    var g = Convert.ToByte(text.Substring(5, 2), 16);
                    var b = Convert.ToByte(text.Substring(7, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }
            }
            catch
            {
            }

            return fallback;
        }
    }

    [DataContract]
    internal sealed class LanguageStyle
    {
        [DataMember(Name = "backColor")]
        public string BackColor { get; set; }

        [DataMember(Name = "textColor")]
        public string TextColor { get; set; }
    }
}
