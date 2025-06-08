using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;
using MonoGame.Extended;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System.Linq;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.IO;
using IniParser.Model.Configuration;
using System.Diagnostics;

namespace STOLON
{
    public class GameConfig
    {
        private IniDataParser _parser;
        private IniData _data;
        public GameConfig()
        {
            string path = @"user.cfg";
            STOLON.Debug.Log(">registering config from path: " + path);
            _parser = new IniDataParser();
            STOLON.Debug.Log(">parsing config");
            _data = _parser.Parse(File.ReadAllText(path));
            STOLON.Debug.Success();
            STOLON.Debug.Log(">managing overrides");


            STOLON.Debug.Success();
            STOLON.Debug.Success();
        }
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
            throw new FormatException($"Failed to parse '{key}' as {typeof(T).Name} or find it without defautValue set");
        }
        private delegate bool TryParseHandler<T>(string input, out T result);
        private bool TryParseBool(string input, out bool result)
        {
            switch (input.ToLowerInvariant())
            {
                case "true":
                case "1":
                case "yes":
                case "on":
                    result = true;
                    return true;
                case "false":
                case "0":
                case "no":
                case "off":
                    result = false;
                    return true;
                default:
                    result = default;
                    return false;
            }
        }
    }
}
