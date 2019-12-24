using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace FileRemover
{
    public class Options
    {
        public bool WaitExit { get; set; }
        private const string JsonFile = "options.json";
        public List<Rule> Rules { get; set; }

        public static Options LoadFromJsonFile(Action<string> logger)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Options>(File.ReadAllText(JsonFile));
            }
            catch (Exception ex)
            {
                logger?.Invoke($"读取配置文件异常，详情={ex.Message}");
                return null;
            }
        }

        private const string TestDir = "testDir";
        private const string TestSuffix = ".txt";
        public static void GenerateSampleJsonFile()
        {
            if (!File.Exists(JsonFile))
                File.WriteAllText(JsonFile, JsonConvert.SerializeObject(new Options()
                {
                    WaitExit = true,
                    Rules = new List<Rule>()
                    {
                        new Rule()
                        {
                            ContainSubDirectory = true,
                            DeleteMode = DeleteMode.RecycleBin,
                            Directory = TestDir,
                            Filter = @$"(.*)(\{TestSuffix})$",
                            IsEnable = true,
                            MatchMode = MatchMode.Regex,
                            SecondsAgo = 60,
                            DeleteDirectoryIfEmpty = true
                        }
                    }
                }));
        }

        public static void GenerateTestData()
        {
            if (!Directory.Exists(TestDir)) Directory.CreateDirectory(TestDir);
            for (int i = 0; i < 10; i++)
            {
                File.WriteAllText(Path.Combine(TestDir, Guid.NewGuid().ToString() + TestSuffix), i.ToString());
            }
        }
    }
}
