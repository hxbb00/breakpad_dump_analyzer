using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CommandLine;
using Mono.Unix;

namespace breakpad_dump_analyzer
{
    public class Program
    {
        private const string EndTips = "按 Enter 完成输入 或者 按 q 键结束本程序...";

        public static int Main(string[] args)
        {
            Console.WriteLine("=====================================================================");
            Console.WriteLine("=============================dump_analyzer===========================");
            Console.WriteLine("=====================================================================");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {

            }

            int ret = DoMain(args);
            return ret;
        }

        private static int DoMain(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Stopwatch s = Stopwatch.StartNew();

            var rst = Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(ParseHandle);

            Console.WriteLine("End in: " + s.Elapsed);
            Console.WriteLine("=====================================================================");

            return 0;
        }

        private static void ParseHandle(Options o)
        {
            if (o.Debug)
            {
                Console.WriteLine("Debug 模式，，，按 任意键 继续...");
                Console.ReadKey();
            }

            if (!CheckEnv())
            {
                return;
            }

            ConsoleWriteLine("请选择操作类型: 生成sym(1)? 分析dump(2)?");
            var oper = ConsoleReadLine();
            while (!"1".Equals(oper) && !"2".Equals(oper))
            {
                if (ShouldQuit(oper))
                {
                    return;
                }

                ConsoleWriteLine("操作类型无效, 请重新输入");
                oper = ConsoleReadLine();
            }

            if ("1".Equals(oper))
            {
                ParseSymHandle();
            }
            else if ("2".Equals(oper))
            {
                ParseDumpHandle();
            }
        }

        private static bool CheckEnv()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            var dump_analyzer = Path.Combine(appDir, "dump_analyzer");
            if (!Directory.Exists(dump_analyzer))
            {
                Console.WriteLine($"{dump_analyzer} 目录不存在, 请将相关bin文件放置于此...");
                return false;
            }

            var _gensyms = Path.Combine(appDir, "_gensyms.sh");
            var _analyze = Path.Combine(appDir, "_analyze.sh");

            ChmodAddX(_gensyms);
            ChmodAddX(_analyze);

            return true;
        }

        private static void ChmodAddX(string destFile)
        {
            var unixFileInfo = new Mono.Unix.UnixFileInfo(destFile);
            // set file permission
            unixFileInfo.FileAccessPermissions |=
                FileAccessPermissions.UserExecute
                | FileAccessPermissions.OtherExecute
                | FileAccessPermissions.GroupExecute;
        }

        private static void ParseDumpHandle()
        {
            ConsoleWriteLine("请输入MiniDump文件路径");
            var pathMiniDump = ConsoleReadLine();
            while (!File.Exists(pathMiniDump))
            {
                if (ShouldQuit(pathMiniDump))
                {
                    return;
                }

                ConsoleWriteLine("MiniDump文件路径无效, 请重新输入");
                pathMiniDump = ConsoleReadLine();
            }

            string fileSyms = GetSymsPath();

            DumpAnalysis(pathMiniDump, fileSyms);
        }

        private static string GetSymsPath()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            var fileSyms = Path.Combine(appDir, "_syms");
            Directory.CreateDirectory(fileSyms);
            return fileSyms;
        }

        private static void ParseSymHandle()
        {
            ConsoleWriteLine("请输入崩溃程序运行所在路径");
            var fileProgram = ConsoleReadLine();
            while (!Directory.Exists(fileProgram))
            {
                if (ShouldQuit(fileProgram))
                {
                    return;
                }

                ConsoleWriteLine("崩溃程序运行所在路径无效, 请重新输入");
                fileProgram = ConsoleReadLine();
            }

            string fileSyms = GetSymsPath();
            GenSyms(fileProgram, fileSyms);
        }

        private static void GenSyms(string fileProgram, string fileSyms)
        {
            var pureSos = Directory.GetFiles(fileProgram, "lib*.so");
            foreach (var so in pureSos)
            {
                var sym = Path.Combine(fileSyms, $"{Path.GetFileName(so)}.sym");

                var line1 = RunProc("_gensyms.sh", $"{so} {sym}");

                var line2 = RunProc("head", $"{sym} -n 1");
                if (!string.IsNullOrEmpty(line2) && line2.StartsWith("MODULE"))
                {
                    var spits = line2.Split(' ', '\t');
                    if (spits.Length == 5)
                    {
                        var dir = Path.Combine(fileSyms, spits[spits.Length - 1], spits[spits.Length - 2]);
                        Directory.CreateDirectory(dir);
                        string destFileName = Path.Combine(dir, $"{spits[spits.Length - 1]}.sym");
                        File.Move(sym, destFileName, true);
                        Console.WriteLine($"{destFileName} 生成完毕...");
                    }
                }
            }
        }

        private static string RunProc(string fileName, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var line = process.StandardOutput.ReadLine();
            process.WaitForExit();
            process.Dispose();

            return line;
        }

        private static void DumpAnalysis(string pathMiniDump, string fileSyms)
        {
            var line1 = RunProc("_analyze.sh", $"{pathMiniDump} {fileSyms}");
        }

        private static string ConsoleReadLine()
        {
            var readLine = Console.ReadLine();

            if (string.IsNullOrEmpty(readLine))
            {
                return readLine;
            }

            return readLine.Trim().Trim('"').TrimEnd('\\', '/');
        }

        private static void ConsoleWriteLine(string line)
        {
            Console.WriteLine($">| {line}，{EndTips}");
        }

        private static bool ShouldQuit(string cmd)
        {
            return (!string.IsNullOrEmpty(cmd) && "q".Equals(cmd.Trim().ToLower()));
        }
    }
}
