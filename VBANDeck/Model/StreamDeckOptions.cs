using CommandLine;

namespace VBANDeck.Model
{
    public class StreamDeckOptions
    {
        [Option("port", Required = true)]
        public int Port { get; set; }
        
        [Option("uuid", Required = false)]
        public string Uuid { get; set; }
        
        [Option("registerEvent", Required = false)]
        public string RegisterEvent { get; set; }
        
        [Option("info", Required = false)]
        public string Info { get; set; }
    }
}