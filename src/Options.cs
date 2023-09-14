using CommandLine;

namespace breakpad_dump_analyzer
{
    public class Options
    {
        [Option('d', "Debug", Required = false, HelpText = "Debug")]
        public bool Debug { get; set; }
    }
}
