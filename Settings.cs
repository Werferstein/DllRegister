using Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DllRegister
{
    public class Settings : AppSettings<Settings>
    {
        public Settings() 
        {
            MainOptions = new Options();
            FileItems = new List<FileItem>();
            RegisterEXE = string.Empty;
            BuildRegistryKey = true;
            InstallInGAC = false;
            Codebase = true;
            SaveLogging = false;
            InstallLog = string.Empty;
        }
        
        
        public Options MainOptions { get; set; }
        public List<FileItem> FileItems { get; set; }
        [XmlAttribute]
        public string RegisterEXE { get; set; }
        
        [XmlAttribute]
        public bool InstallInGAC{ get; set; }

        [XmlAttribute]
        public bool BuildRegistryKey { get; set; }
        
        [XmlAttribute]
        public bool Codebase { get; set; }

        [XmlAttribute]
        public bool SaveLogging { get; set; }

        [XmlAttribute]
        public string InstallLog { get; set; }

        [XmlAttribute]
        public string OutputPath { get; set; }
        [XmlAttribute]
        public string RegistryBuildScript { get; set; }
    }
}
