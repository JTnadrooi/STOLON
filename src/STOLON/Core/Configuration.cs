using AsitLib;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;

namespace STOLON
{
    public sealed class Configuration
    {
        public FrozenDictionary<string, object> Defaults { get; }

        private TomlTable _model;
        public FrozenDictionary<string, object> TomlValues { get; private set; }
        private const string PATH = @"Configs\user.toml";

        public Configuration() // no debug printing!
        {

            _model = Toml.ToModel(File.ReadAllText(PATH));
            TomlValues = GetTomlValues(_model);

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
                {"graphics.res_w", 1920},
                {"graphics.res_h", 1080},
                {"graphics.crt.enable", true},

                {"cli.catch_errors", true},
                {"cli.global_flags", Array.Empty<string>()},
                {"cli.global_flags_on_startup_arguments", Array.Empty<string>()}
                //{"___", true},
            };

            Defaults = defaults.ToFrozenDictionary();
            Validate();
        }

        private FrozenDictionary<string, object> GetTomlValues(TomlTable table, string prefix = "")
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            foreach (string key in table.Keys)
            {
                string fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                object value = table[key];

                if (value is TomlTable subTable) foreach (KeyValuePair<string, object> kvp in GetTomlValues(subTable, fullKey)) result.Add(kvp.Key, kvp.Value);
                else result.Add(fullKey, value);
            }

            return result.ToFrozenDictionary();
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
        public float GetFloat(string key) => Get<float>(key);
        public int GetInt(string key) => Get<int>(key);
        public int GetByte(string key) => Get<byte>(key);
        public double GetDouble(string key) => Get<double>(key);
        public bool GetBool(string key) => Get<bool>(key);
        public string GetString(string key) => Get<string>(key);
        public char GetChar(string key) => Get<char>(key);
        public T[] GetArray<T>(string key) => Get<T[]>(key);
        public object Get(string key) => Get<object>(key);
        public T Get<T>(string key)
        {
            try
            {
                return TomlParseToType<T>(TomlValues[key]);
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException($"Get key value for key {key} failed; " + e.Message);
            }
        }

        public T TomlParseToType<T>(object value)
        {
            switch (value)
            {
                case TomlArray tomlArray:
                    if (typeof(T).IsArray)
                    {
                        Type elementType = typeof(T).GetElementType()!;
                        Array array = Array.CreateInstance(elementType, tomlArray.Count);
                        for (int i = 0; i < tomlArray.Count; i++) array.SetValue(Convert.ChangeType(tomlArray[i], elementType), i);
                        return (T)(object)array;
                    }
                    else throw new InvalidCastException($"Cannot convert value to array type {typeof(T)}.");
                case TomlTable: throw new InvalidCastException($"Cannot get table as any value.");
                default: return (T)Convert.ChangeType(value, typeof(T));
            }
        }


        public void Set(string key, object value)
        {
            object? current = _model;
            var parts = key.Split('.');

            for (int i = 0; i < parts.Length; i++)
                if (current is TomlTable t)
                    if (i == parts.Length - 1) t[parts[i]] = value!;
                    else if (t.ContainsKey(parts[i])) current = t[parts[i]];
                    else throw new KeyNotFoundException($"Key '{parts[i]}' not found.");
                else throw new KeyNotFoundException($"Key '{parts[i]}' not found (a segment is not a TomlTable).");

            File.WriteAllText(PATH, Toml.FromModel(_model));
            TomlValues = GetTomlValues(_model);
            //Console.WriteLine(Toml.FromModel(_model));
        }

        public void Reload()
        {
            TomlValues = GetTomlValues(_model);
        }

        public void Validate()
        {
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

            foreach (KeyValuePair<string, object> kvp in TomlValues)
            {
                if (!Defaults.TryGetValue(kvp.Key, out object? defaultValue)) throw new Exception($"Unexpected key '{kvp.Key}' found.");
                if (kvp.Value is TomlArray tomlArray)
                {
                    Type elemType = ((Array)defaultValue).GetType().GetElementType()!;
                    for (int i = 0; i < tomlArray.Count; i++)
                        if (!IsSameOrConvertible(tomlArray[i], elemType))
                            throw new Exception($"Type mismatch in '{kvp.Key}[{i}]'. Expected {elemType}, got {tomlArray[i]?.GetType()}");
                }
                else if (!IsSameOrConvertible(kvp.Value, defaultValue.GetType())) throw new Exception($"Type mismatch for '{kvp.Key}'. Expected {defaultValue.GetType()}, got {kvp.Value?.GetType()}.");
            }
        }

        public void Reset(string key) => Set(key, Defaults[key]);
    }
}
