using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Helper
{
    public class JsonLoader
    {
        public static T LoadFromFile<T>(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                T result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawdata"></param>
        /// <returns></returns>
        public static T LoadFromString<T>(string rawdata){
            T result = JsonConvert.DeserializeObject<T>(rawdata);
            return result;
        }

        public static T Deserialize<T>(object jsonObject)
        {
            return JsonConvert.DeserializeObject<T>(Convert.ToString(jsonObject));
        }

        public static string Serialize<T>(object rawdata)
        {
            string result = JsonConvert.SerializeObject(rawdata);
            return result;
        }

    }    
}
