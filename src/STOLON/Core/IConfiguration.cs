using System.Collections.Frozen;

namespace STOLON
{
    public interface IConfiguration
    {
        FrozenDictionary<string, object> Defaults { get; }
        FrozenDictionary<string, object> TomlValues { get; }

        object Get(string key);
        T Get<T>(string key);
        T[] GetArray<T>(string key);
        bool GetBool(string key);
        int GetByte(string key);
        char GetChar(string key);
        double GetDouble(string key);
        float GetFloat(string key);
        int GetInt(string key);
        string GetString(string key);
        void Reload();
        void Reset(string key);
        void Set(string key, object value);
        void Validate();
    }
}