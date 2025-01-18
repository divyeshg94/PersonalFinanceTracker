using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PersonalFinanceTracker.Model.Helpers
{
    public class JsonParameters : Dictionary<string, string> // TOOD: use JsonObject for value
    {
        public JsonParameters() : base(StringComparer.InvariantCultureIgnoreCase) { }
        public JsonParameters(string jsonData)
            : this()
        {
            if (jsonData != null)
            {
                var data = JsonConvert.DeserializeObject<JsonParameters>(jsonData);
                foreach (var kvp in data)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public JsonParameters(JsonParameters jsonParams)
        {
            if (jsonParams != null)
            {
                foreach (var kvp in jsonParams)
                {
                    this.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public JsonParameters(Dictionary<string, string> parameters)
        {
            foreach (var kvp in parameters)
            {
                this.Add(kvp.Key, kvp.Value);
            }
        }
        public new string this[string key]
        {
            get { return base[key.ToLower()]; }
            set { base[key.ToLower()] = value; }
        }

        public new bool ContainsKey(string key)
        {
            return base.ContainsKey(key.ToLower());
        }

        public void AddOrUpdate(string name, string value)
        {
            if (base.ContainsKey(name))
            {
                base[name] = value;
            }
            else
            {
                base.Add(name.ToLower(), value);
            }
        }

        public void Add(string name, object value)
        {
            if (value is string)
            {
                base.Add(name.ToLower(), (string)value);
            }
            else if (value is JsonObject)
            {
                base.Add(name.ToLower(), value.ToString());
            }
            else
            {
                //base.Add(name.ToLower(), new JsonObject(value).ToString());
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        static JsonParameters _empty = new JsonParameters();
        public static JsonParameters Empty { get { return _empty; } }
    }
}
