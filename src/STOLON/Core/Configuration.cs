using AsitLib;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace STOLON
{
    public class Configuration
    {
        public readonly record struct Entry(string Path, object DefaultValue);

        public FrozenDictionary<string, Entry> Entries { get; }

        private TomlTable _model;
        private const string PATH = @"Configs\user.toml";

        public Configuration() // no debug printing svp!
        {
            int ForKeys(Action<string, object?> forKeys, TomlTable? table = null, string prefix = "")
            {
                table = table ?? _model;
                int count = 0;

                foreach (string key in table.Keys)
                {
                    string fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                    object? value = table[key];

                    if (value is TomlTable subTable) count += ForKeys(forKeys, subTable, fullKey);
                    else
                    {
                        forKeys(fullKey, value);
                        count++;
                    }
                }

                return count;
            }

            bool IsSameOrConvertible(object? value, Type expectedType)
            {
                if (value == null) return false;
                if (expectedType.IsAssignableFrom(value.GetType())) return true;
                try
                {
                    Convert.ChangeType(value, expectedType);
                    return true;
                }
                catch
                {
                    return false;
                }
            }


            _model = Toml.ToModel(File.ReadAllText(PATH));
            Dictionary<string, object> defaults = new Dictionary<string, object>{
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
                {"debug.skip.parameters", Array.Empty<string>()},

                {"graphics.theme", "default"},
                {"graphics.entities_show_on_menu", true},
                {"graphics.splashtexts_show", true},
                {"graphics.fullscreen", false},
                {"graphics.res.w", 1920},
                {"graphics.res.h", 1080},
                {"graphics.crt.enable", true},

                {"cli.catch_errors", true},
                {"cli.global_flags", Array.Empty<string>()},
                {"cli.enable_global_flags_on_startup_arguments", true}
                //{"___", true},
            };

            int filekeyCount = ForKeys((k, v) =>
            {
                if (!defaults.TryGetValue(k, out object? defaultValue)) throw new Exception($"Unexpected key '{k}' found.");
                if (v is TomlArray tomlArray)
                {
                    Type elemType = ((Array)defaultValue).GetType().GetElementType()!;
                    for (int i = 0; i < tomlArray.Count; i++)
                        if (!IsSameOrConvertible(tomlArray[i], elemType))
                            throw new Exception($"Type mismatch in '{k}[{i}]'. Expected {elemType}, got {tomlArray[i]?.GetType()}");
                }
                else if (!IsSameOrConvertible(v, defaultValue.GetType())) throw new Exception($"Type mismatch for '{k}'. Expected {defaultValue.GetType()}, got {v?.GetType()}.");
            });
            //ForKeys((k, v) => Console.WriteLine($"{k} ({v?.GetType().ToString() ?? "null"})"));

            //if (filekeyCount != defaults.Count) throw new Exception($"Key count mismatch. File has {filekeyCount}, expected {defaults.Count}.");

            Entries = defaults.Select(d => new KeyValuePair<string, Entry>(d.Key, new Entry(d.Key, d.Value))).ToFrozenDictionary();
        }
        public float GetFloat(string key) => GetValue<float>(key);
        public int GetInt(string key) => GetValue<int>(key);
        public int GetByte(string key) => GetValue<byte>(key);
        public double GetDouble(string key) => GetValue<double>(key);
        public bool GetBool(string key) => GetValue<bool>(key);
        public string GetString(string key) => GetValue<string>(key);
        public char GetChar(string key) => GetValue<char>(key);
        public object GetValue(string key) => GetValue<object>(key);
        public T[] GetArray<T>(string key) => GetValue<T[]>(key);
        public T GetValue<T>(string key)
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
                    else throw new InvalidCastException($"Cannot convert value at '{key}' to array type {typeof(T)}.");
                if (typeof(T) == typeof(object)) return (T)currentValue;
                return (T)Convert.ChangeType(currentValue, typeof(T));
            }

            string[] segments = key.Split('.');
            object? currentValue = _model;

            foreach (string segment in segments)
                if (currentValue is TomlTable table && table.ContainsKey(segment)) currentValue = table[segment];
                else return (T)Entries[key].DefaultValue;
            //else throw new KeyNotFoundException($"Key '{segment}' not found.");
            if (currentValue == null) throw new InvalidCastException($"Cannot convert value at '{key}' to type {typeof(T)}.");

            return ParseToType(currentValue);
        }
    }
}
