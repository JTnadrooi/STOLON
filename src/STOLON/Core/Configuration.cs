using AsitLib;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;

namespace STOLON
{
    public class Configuration
    {
        private struct Entry
        {
            public string Path;
            public object Value;
            public Entry(string path, object? value, object defaultValue) // boxing galore
            {
                Path = path;
                Value = value ?? defaultValue;
            }
        }
        private TomlTable _model;
        private FrozenDictionary<string, object> _defaults;
        public Configuration() // no debug printing svp!
        {
            string path = @"Configs\user.toml";

            _model = Toml.ToModel(File.ReadAllText(path));
            _defaults = new Dictionary<string, object>{
                {"audio.enable", true},
                {"audio.stereo", true},
                {"audio.vol.fx", 0.5},
                {"audio.vol.ost", 1.0},
                {"audio.vol.master", 1.0},
                {"debug.console.enable", true},
                {"debug.console.theme", "default"},
                {"debug.log.enable", true},
                {"debug.log.style", "default"},
                {"debug.skip.enable", true},
                {"debug.skip.target", "main_menu"},
                {"debug.skip.skip_gamestage_animation", true},
                {"graphics.theme", "default"},
                {"graphics.entities_show_on_menu", true},
                {"graphics.splashtexts_show", true},
                {"graphics.fullscreen", false},
                {"graphics.resolution.w", 1920},
                {"graphics.resolution.h", 1080},
                {"graphics.crt.enable", true},
                {"cli.catch_errors", true},
                {"cli.global_flags", Array.Empty<string>()},
                {"cli.enable_global_flags_on_startup_arguments", true}
                //{"___", true},
            }.ToFrozenDictionary();

            void PrintKeys(TomlTable table, string prefix = "")
            {
                foreach (string key in table.Keys)
                {
                    string fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                    object? value = table[key];

                    string typeName = value?.GetType().ToString() ?? "null";

                    Console.WriteLine($"{fullKey} ({typeName})");

                    if (value is TomlTable subTable)
                        PrintKeys(subTable, fullKey);
                }
            }
            //PrintKeys(_model);
        }
        public float GetFloat(string path) => GetValue<float>(path);
        public int GetInt(string path) => GetValue<int>(path);
        public double GetDouble(string path) => GetValue<double>(path);
        public bool GetBool(string path) => GetValue<bool>(path);
        public string GetString(string path) => GetValue<string>(path);
        public T GetValue<T>(string path)
        {
            T ParseToType(object currentValue)
            {
                if (typeof(T).IsArray)
                    if (currentValue is TomlArray tomlArray)
                    {
                        Type elementType = typeof(T).GetElementType()!;
                        Array array = Array.CreateInstance(elementType, tomlArray.Count);
                        for (int i = 0; i < tomlArray.Count; i++) array.SetValue(Convert.ChangeType(tomlArray[i], elementType), i);
                        return (T)(object)array;
                    }
                    else throw new InvalidCastException($"Cannot convert value at '{path}' to array type {typeof(T)}.");
                if (typeof(T) == typeof(object)) return (T)currentValue;
                return (T)Convert.ChangeType(currentValue, typeof(T));
            }

            string[] segments = path.Split('.');
            object? currentValue = _model;

            foreach (string segment in segments)
                if (currentValue is TomlTable table && table.ContainsKey(segment)) currentValue = table[segment];
                else return (T)_defaults[path];
            //else throw new KeyNotFoundException($"Key '{segment}' not found.");
            if (currentValue == null) throw new InvalidCastException($"Cannot convert value at '{path}' to type {typeof(T)}.");

            return ParseToType(currentValue);
        }
    }
}
