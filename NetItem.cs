/*
Ingolf Hill
werferstein.org
/*
  This program is free software. It comes without any warranty, to
  the extent permitted by applicable law. You can redistribute it
  and/or modify it under the terms of the Do What The Fuck You Want
  To Public License, Version 2, as published by Sam Hocevar.
*/

using System;
using System.Xml.Serialization;

namespace DllRegister
{
    public class RegData
    {
        public RegData()
        { Default = string.Empty;
          Assembly = string.Empty;
            Class = string.Empty;
            CodeBase = string.Empty;
            RuntimeVersion = string.Empty;
            ThreadingModel = string.Empty;
            CLSID = string.Empty;
        }

        [XmlAttribute]
        public string Default { get; set; }
        [XmlAttribute]
        public string Assembly { get; set; }
        [XmlAttribute]
        public string Class { get; set; }
        private string codeBase;
        [XmlAttribute]
        public string CodeBase { get { return codeBase;}
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Path = value.Replace(@"file:///", "").Replace(@"FILE:///", "").Replace(@"/", @"\");
                }
                else
                {
                    Path = string.Empty;
                }
                codeBase = value;
            }
        }
        [XmlIgnore]
        public string Path { get; private set; }

        [XmlAttribute]
        public string RuntimeVersion { get; set; }
        [XmlAttribute]
        public string ThreadingModel { get; set; }

        private string clsid;
        [XmlAttribute]
        public string CLSID { get { return clsid; } set { clsid = value.Replace("{", "").Replace("}", ""); } }        
        public override string ToString()
        {
            return CLSID;
        }

        [XmlIgnore]
        public string GetInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CLSID)) return string.Empty;
                string outptut = string.Empty;
                outptut = "CLSID         : {" + CLSID + "}" + Environment.NewLine;
                outptut += !string.IsNullOrEmpty(Default) ? "Default       : " + Default + Environment.NewLine : "";
                outptut += !string.IsNullOrEmpty(Assembly) ? "Assembly      : " + Assembly + Environment.NewLine : "";
                outptut += !string.IsNullOrEmpty(Class) ? "Class         : " + Class + Environment.NewLine : "";
                outptut += !string.IsNullOrEmpty(CodeBase) ? "CodeBase      : " + CodeBase + Environment.NewLine : "";
                outptut += !string.IsNullOrEmpty(RuntimeVersion) ? "RuntimeVersion: " + RuntimeVersion + Environment.NewLine : "";
                outptut += !string.IsNullOrEmpty(ThreadingModel) ? "ThreadingModel: " + ThreadingModel + Environment.NewLine : "";
                return outptut;
            }
        }

        public bool SetValue(string valueName, string value)
        {
            if (valueName == string.Empty)
            {
                Default = value;
                return true;
            }
            else if (valueName.ToUpper() == "CLSID")
            {
                CLSID = value;
                return true;
            }
            else if (valueName.ToUpper() == "CODEBASE")
            {
                CodeBase = value;
                return true;
            }
            else if (valueName.ToUpper() == "ASSEMBLY")
            {
                Assembly = value;
                return true;
            }
            else if (valueName.ToUpper() == "CLASS")
            {
                Class = value;
                return true;
            }
            else if (valueName.ToUpper() == "RUNTIMEVERSION")
            {
                RuntimeVersion = value;
                return true;
            }
            else if (valueName.ToUpper() == "THREADINGMODEL")
            {
                ThreadingModel = value;
                return true;
            }
            return false;
        }
    }






    [Serializable]
    public class FileItem
    {
        public FileItem() { Name = string.Empty;FullPath = string.Empty; RegExe = string.Empty;}        
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string RegExe { get; set; }
        [XmlAttribute]
        public string FullPath { get; set; }          
        public override string ToString()
        {
            return FullPath;
        }       
    }




    [Serializable]
    public class NetItem
    {
        public string Name { get;set;}
        private string mFullPath;
        public string FullPath { get { return mFullPath; }
            set {
                mFullPath = value;
                string version = string.Empty;
                string[] sp = value.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                if (sp != null && sp.Length > 0)
                {
                    foreach (var item in sp)
                    {
                        if (item.StartsWith("v") && int.TryParse(item.Substring(1, item.Length - 1).Replace(".", ""), out int n))
                        {
                            version = item.Substring(1, item.Length - 1);
                        }
                    }

                }

                if (mFullPath.ToLower().Contains("framework") && mFullPath.Contains("64"))
                {
                    Name = "64 Bit Net Version: " + version;
                }
                else if (mFullPath.ToLower().Contains("framework"))
                {
                    Name = "32 Bit Net Version: " + version;
                }
                else
                {
                    Name = mFullPath;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
