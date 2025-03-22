using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OnekoSharp
{
    public class Config
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Config));
        public static Config Instance { get; set; } = LoadConfig();


        public int OnekoSize { get; set; } = 32;
        public int OnekoSpeed { get; set; } = 12;
        public ModifierKeys ToggleBoxShortkeyModifier { get; set; } = ModifierKeys.Win | ModifierKeys.Shift;
        public Keys ToggleBoxShortkeyKey { get; set; } = Keys.O;


        public static Config LoadConfig()
        {
            if (!File.Exists("config.xml")) return new Config();
            var f = File.OpenRead("config.xml");
            Config config = serializer.Deserialize(f) as Config;
            f.Close();
            return config;
        }
        public void SaveConfig()
        {
            var f = File.OpenWrite("config.xml");
            serializer.Serialize(f, this);
            f.SetLength(f.Position);
            f.Close();
        }

    }
}
