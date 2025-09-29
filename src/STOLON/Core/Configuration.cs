using AsitLib;
using System;
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
        public Configuration()
        {
            string path = @"Configs\user.toml";

            _model = Toml.ToModel(File.ReadAllText(path));

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
        public float GetFloat(string path, float? defaultValue = null) => GetValue<float>(path);
        public int GetInt(string path, int? defaultValue = null) => GetValue<int>(path);
        public double GetDouble(string path, double? defaultValue = null) => GetValue<double>(path);
        public bool GetBool(string path, bool? defaultValue = null) => GetValue<bool>(path);
        public string GetString(string path, string? defaultValue = null) => GetValue<string>(path);
        public T GetValue<T>(string path, object? defaultValue = null)
        {
            string[] segments = path.Split('.');
            object? currentValue = _model;

            foreach (string segment in segments)
                if (currentValue is TomlTable table && table.ContainsKey(segment)) currentValue = table[segment];
                else return defaultValue != null ? (T)defaultValue : throw new KeyNotFoundException($"Key '{segment}' not found.");

            if (currentValue == null)
                return defaultValue != null ? (T)defaultValue : throw new InvalidCastException($"Cannot convert value at '{path}' to type {typeof(T)}.");
            if (typeof(T).IsArray)
            {
                if (currentValue is TomlArray tomlArray)
                {
                    Type elementType = typeof(T).GetElementType()!;
                    Array array = Array.CreateInstance(elementType, tomlArray.Count);
                    for (int i = 0; i < tomlArray.Count; i++) array.SetValue(Convert.ChangeType(tomlArray[i], elementType), i);
                    return (T)(object)array;
                }
                else
                {
                    return defaultValue != null ? (T)defaultValue : throw new InvalidCastException($"Cannot convert value at '{path}' to array type {typeof(T)}.");
                }
            }
            if (typeof(T) == typeof(object))
            {
                return (T)currentValue;
            }
            return (T)Convert.ChangeType(currentValue, typeof(T));
        }
    }
}
