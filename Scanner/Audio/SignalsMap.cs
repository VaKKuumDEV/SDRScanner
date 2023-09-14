using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Audio
{
    public class SignalsMap
    {
        public string Path { get; }
        public Dictionary<string, List<string>> Map { get; } = new();

        public SignalsMap(string filePath)
        {
            Path = filePath;
            Map = Load();
        }

        public void AddSamples(string key, List<string> samples)
        {
            if (!Map.ContainsKey(key)) Map[key] = new();
            Map[key].AddRange(samples);
        }

        public Dictionary<string, List<string>> Load()
        {
            Dictionary<string, List<string>> map = new();
            if (File.Exists(Path))
            {
                JObject mapArray = JObject.Parse(File.ReadAllText(Path));

                foreach (var kv in mapArray)
                {
                    string key = kv.Key;

                    if (kv.Value == null) continue;
                    if (kv.Value.Type == JTokenType.Array)
                    {
                        map[key] = new();

                        JArray objArray = (JArray)kv.Value;
                        foreach (JToken val in objArray)
                        {
                            if (val.Type == JTokenType.String)
                            {
                                string valStr = val.Value<string>() ?? "";
                                if (valStr.Length > 0) map[key].Add(valStr);
                            }
                        }
                    }
                }
            }

            return map;
        }

        public void Reload()
        {
            var newMap = Load();
            Map.Clear();
            foreach (var kv in newMap) Map[kv.Key] = kv.Value;
        }

        public void Save()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(Map, Formatting.Indented));
        }
    }
}
