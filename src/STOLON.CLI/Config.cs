using AsitLib;
using IniParser.Model;
using IniParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class CLIConfig
    {
        private IniDataParser _parser;
        private IniData _data;
        public CLIConfig()
        {
            string path = @"user.ini";
            _parser = new IniDataParser();
            _data = _parser.Parse(File.ReadAllText(path));
            GlobalFlags = GetString("CLI.global_flags").Split(",").ToHashSet();
        }
        public HashSet<string> GlobalFlags { get; }
        public float GetFloat(string key, float? defaultValue = null) => TryGetParsedValue(key, float.TryParse, defaultValue);
        public int GetInt(string key, int? defaultValue = null) => TryGetParsedValue(key, int.TryParse, defaultValue);
        public double GetDouble(string key, double? defaultValue = null) => TryGetParsedValue(key, double.TryParse, defaultValue);
        public bool GetBool(string key, bool? defaultValue = null) => TryGetParsedValue(key, TryParseBool, defaultValue);
        public string GetString(string key, string? defaultValue = null)
        {
            if (_data.TryGetKey(key, out var value)) return value;
            if (defaultValue != null) return defaultValue;
            throw new FormatException($"Key '{key}' not found.");
        }
        private T TryGetParsedValue<T>(string key, TryParseHandler<T> parser, T? defaultValue) where T : struct
        {
            if (_data.TryGetKey(key, out string toparse) && parser(toparse, out T toret)) return toret;
            if (defaultValue.HasValue) return defaultValue.Value;
            throw new FormatException($"Failed to parse '{toparse}' as {typeof(T).Name} or find it without defautValue set");
        }
        private delegate bool TryParseHandler<T>(string input, out T result);
        private bool TryParseBool(string input, out bool result)
        {
            switch (input.ToLowerInvariant().Trim())
            {
                case "true" or "1" or "yes" or "on":
                    result = true;
                    return true;
                case "false" or "0" or "no" or "off":
                    result = false;
                    return true;
                default:
                    result = default;
                    return false;
            }
        }

    }
}
