using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OracleDataTools.common
{
    public class JsonHelper
    {
        private string _fileName = AppDomain.CurrentDomain.FriendlyName + ".json";

        public string GetJsonFromFile()
        {
            string json = string.Empty;
            string line;
            using (StreamReader sr =  new StreamReader(_fileName))
            {
                while ((line = sr.ReadLine())!=null)
                {
                    json = json + line;
                }
            }

            return json;
        }

        public void SaveJsonToFile(string json)
        {
            try
            {
                FileStream fs = new FileStream(_fileName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

                sw.Write(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public string ToJson<T>(List<T> lists)
        {
           string json =  Newtonsoft.Json.JsonConvert.SerializeObject(lists);

            return json;
        }

        public string ToObject<T>(string json)
        {
            List<T> lists = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);

            return lists;
        }
    }
}
