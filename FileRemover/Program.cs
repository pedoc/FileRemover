using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Humanizer;
using Microsoft.VisualBasic.FileIO;
using Serilog;
using Serilog.Core;
using SearchOption = System.IO.SearchOption;

namespace FileRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeLogger();
            PrintVersion();

            //Options.GenerateTestData();
            Options.GenerateSampleJsonFile();

            var options = Options.LoadFromJsonFile(Info);
            if (options == null || options.Rules == null || !options.Rules.Any()) Warn("配置文件不包含任何有效规则");
            else
            {
                var activeRules = options.Rules.Where(item => item.IsEnable == true).ToList();
                if (!activeRules.Any()) Warn("未启用任何有效规则");
                else
                {
                    var sw = Stopwatch.StartNew();
                    var deletedCount = 0;
                    var errorCount = 0;
                    double totalSize = 0;
                    double deletedSize = 0;
                    foreach (var rule in activeRules)
                    {
                        if (!Directory.Exists(rule.Directory))
                        {
                            Warn($"指定的目录({rule.Directory})不存在，跳过此规则");
                            continue;
                        }

                        var files = Directory
                            .EnumerateFiles(rule.Directory, "*.*",
                            rule.ContainSubDirectory ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                            .ToList();
                        Info($"{rule.Directory} 下共有 {files.Count()} 个文件，开始匹配过程，规则详情：{Environment.NewLine} {rule.ToString()}{Environment.NewLine}");
                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);
                            totalSize += fileInfo.Length;
                            if (rule.Match(fileInfo))
                            {
                                if (rule.SecondsAgo.HasValue && rule.SecondsAgo > 0 &&
                                    DateTime.Now - fileInfo.CreationTime < TimeSpan.FromSeconds(rule.SecondsAgo.Value))
                                {
                                    Dbg($"{file} 还在有效期内，忽略删除");
                                    continue;
                                }
                                try
                                {
                                    if (rule.DeleteMode == DeleteMode.RecycleBin)
                                        FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs,
                                            RecycleOption.SendToRecycleBin);
                                    else fileInfo.Delete();
                                    deletedCount++;
                                    deletedSize += fileInfo.Length;
                                    Warn($"{file} 已删除");
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    Warn($"{file} 删除失败，详情：{ex.Message}");
                                }
                            }
                        }

                        if (rule.DeleteDirectoryIfEmpty && deletedCount == files.Count)
                        {
                            Info($"{rule.Directory} 下的文件已全部删除，目录也将一并删除");
                            try
                            {
                                if (rule.DeleteMode == DeleteMode.RecycleBin)
                                    FileSystem.DeleteDirectory(rule.Directory, UIOption.OnlyErrorDialogs,
                                        RecycleOption.SendToRecycleBin);
                                else Directory.Delete(rule.Directory);
                            }
                            catch (Exception ex)
                            {
                                Warn($"{rule.Directory} 删除失败，详情：{ex.Message}");
                            }
                        }
                    }
                    sw.Stop();
                    Info(Environment.NewLine);
                    Info($"合计规则数：{activeRules.Count.ToString()}，总大小：{totalSize.Bytes().Humanize()}， 删除成功：{deletedCount.ToString()}/{deletedSize.Bytes().Humanize()}，删除失败：{errorCount}，耗时={sw.ElapsedMilliseconds} ms");
                }

                if (options.WaitExit)
                {
                    Info("请输入任意键退出");
                    Console.ReadKey();
                }
            }
        }

        static void PrintVersion()
        {
            Info($"当前版本 Branch={ThisAssembly.Git.Branch} Commit={ThisAssembly.Git.Commit}");
        }

        static void Info(string msg)
        {
            Log.Logger.Information(msg);
        }

        static void Dbg(string msg)
        {
            Log.Logger.Debug(msg);
        }

        static void Warn(string msg)
        {
            Log.Logger.Warning(msg);
        }

        static void InitializeLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .ColoredConsole()
             .WriteTo
                .RollingFile("logs\\{Date}.log", shared: true)
                .CreateLogger();
        }
    }
}
