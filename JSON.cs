using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SP_TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            solve1();
            solve2();
            solve3();
        }
        static void solve1()
        {
            JObject jsonObj = new JObject();

            jsonObj.Add("name", "spiderman");
            jsonObj.Add("age", 45);
            jsonObj.Add("married", true);

            JArray jsonArr = new JArray();
            jsonArr.Add("martial art");
            jsonArr.Add("gun");
            jsonObj.Add("specialty", jsonArr);

            JObject jsonObj2 = new JObject();
            jsonObj2.Add("1st", "done");
            jsonObj2.Add("2nd", "expected");
            jsonObj2.Add("3rd", null);
            jsonObj.Add("vaccine", jsonObj2);

            JArray jsonArr2 = new JArray();
            jsonObj2 = new JObject();
            jsonObj2.Add("name", "spiderboy");
            jsonObj2.Add("age", 10);
            jsonArr2.Add(jsonObj2);

            jsonObj2 = new JObject();
            jsonObj2.Add("name", "spidergirl");
            jsonObj2.Add("age", 8);
            jsonArr2.Add(jsonObj2);

            jsonObj.Add("children", jsonArr2);
            jsonObj.Add("address", null);


            Console.WriteLine(jsonObj);
            using (StreamWriter file = File.CreateText("sample.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, jsonObj);
            }
        }

        static void solve2()
        {
            String filePath = "sample.json";
            StreamReader sr = new StreamReader(filePath);
            String jsonString = sr.ReadToEnd();
            sr.Close();
            JObject jo = JObject.Parse(jsonString);
            String name = jo["name"].ToString();
            int age = (int)jo["age"];
            Console.WriteLine("name(age) : " + name + "(" + age + ")");
            name = jo["children"][1]["name"].ToString();
            age = (int)jo["children"][1]["age"];
            Console.WriteLine("name(age) : " + name + "(" + age + ")");
        }

        static void solve3()
        {
            String filePath = "sample.json";
            JObject jo = JObject.Parse(File.ReadAllText(filePath));
            foreach (KeyValuePair<string, JToken> itemJo in jo)
            {
                Console.WriteLine("Key : " + itemJo.Key + " / Value Type : " + itemJo.Value.Type);
            }
        }
    }
}
