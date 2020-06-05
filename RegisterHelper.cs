using Microsoft.Win32;
using SLogging;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DllRegister
{





    public class DllRegister
    {

        [DllImport("kernel32")]
        public extern static bool FreeLibrary(int hLibModule);

        [DllImport("kernel32")]
        public extern static int LoadLibrary(string lpLibFileName);
        
        
        private static string RegasmPath = string.Empty;
        public static string BackupPath { get; private set; }
        public static string LogFilePath { get; private set; }

        
        public static bool IsComDllLoadable(string DllName)
        {
            int libId = LoadLibrary(DllName);
            if (libId > 0) FreeLibrary(libId);
            return (libId > 0);
        }


        public static bool Register(string outpath, string dllpath, string regasmPath,  bool installInGac, bool buildRegfile, bool codebase,string time, out string registryCode )
        {
            registryCode = string.Empty;
            RegasmPath = regasmPath;
            bool IsCopied = false;
            bool ok = false;
            try
            {
                if (!installInGac)
                {
                    #region UnRegister ?
                    if (!buildRegfile && DllRegister.IsRegistered(out bool bit64, out bool gac, dllpath, true, outpath, time))
                    {
                        if (!UnRegister(dllpath, regasmPath, outpath, time, installInGac))
                        {
                            Logger.Instance.AdddLog(LogType.Error, "The already registered Dll cannot be unregistered! --> exit " + dllpath, "DllRegister", "");
                            return false;
                        }
                    }
                    #endregion

                    #region set path
                    string path = System.IO.Path.GetDirectoryName(dllpath) + "\\";
                    if (!Logger.IsWritable(path))
                    {
                        Logger.Instance.AdddLog(LogType.Error, "No write access to the path: " + path, "DllRegister", "");
                        return false;
                    }
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(dllpath);
                    string fileName_backup = fileName + "_reg\\";
                    if (string.IsNullOrWhiteSpace(outpath) || !System.IO.Directory.Exists(outpath) || !Logger.IsWritable(outpath))
                    {
                        Logger.Instance.AdddLog(LogType.Error, "Check the output path, set to: " + path, "DllRegister", "");
                        outpath = path;
                    }

                    if (!outpath.EndsWith("\\")) outpath += "\\";
                    #endregion

                    #region Backup?                                       
                    BackupPath = outpath + fileName_backup;
                    LogFilePath = BackupPath + fileName + ".log";
                    if (!System.IO.Directory.Exists(BackupPath)) System.IO.Directory.CreateDirectory(BackupPath);
                    if (!System.IO.Directory.Exists(BackupPath + "Backup")) System.IO.Directory.CreateDirectory(BackupPath + "Backup");
                    string previousPath = BackupPath + "Backup\\" + time + "\\";
                    if (!System.IO.Directory.Exists(previousPath))
                    { System.IO.Directory.CreateDirectory(previousPath); }
                    else
                    {
                        IsCopied = true;
                    }
                        

                 
     
                    //copy reg
                    if (!IsCopied && buildRegfile && System.IO.File.Exists(BackupPath + fileName + ".reg"))
                    {
                        if (System.IO.File.Exists(previousPath + fileName + ".reg")) System.IO.File.Delete(previousPath + fileName + ".reg");
                        System.IO.File.Move(BackupPath + fileName + ".reg", previousPath  + fileName +  ".reg");                        
                    }
                    //copy tlb
                    if (!IsCopied && !buildRegfile && System.IO.File.Exists(BackupPath + fileName + ".tlb"))
                    {
                        if (System.IO.File.Exists(previousPath + "bak_" + fileName + ".tlb")) System.IO.File.Delete(previousPath + "bak_" + fileName + ".tlb");
                        System.IO.File.Move(BackupPath + fileName + ".tlb", previousPath + "bak_" + fileName + ".tlb");
                    }
                        
                    if (!IsCopied && System.IO.File.Exists(path + fileName + ".tlb"))
                    {
                        if (System.IO.File.Exists(previousPath + fileName + ".tlb")) System.IO.File.Delete(previousPath + fileName + ".tlb");
                        System.IO.File.Move(path + fileName + ".tlb", previousPath  + fileName + ".tlb");
                    }
                    //copy dll
                    if (!IsCopied && !buildRegfile && System.IO.File.Exists(BackupPath + fileName + ".dll"))
                    {
                        if (System.IO.File.Exists(previousPath + "bak_" + fileName + ".dll")) System.IO.File.Delete(previousPath + "bak_" + fileName + ".dll");
                        System.IO.File.Move(BackupPath + fileName + ".dll", previousPath + "bak_" + fileName + ".dll");
                    }
                        
                    if (!IsCopied && System.IO.File.Exists(path + fileName + ".dll"))
                    {
                        if (System.IO.File.Exists(previousPath + fileName + ".dll")) System.IO.File.Delete(previousPath + fileName + ".dll");
                        System.IO.File.Copy(path + fileName + ".dll", previousPath  + fileName + ".dll");
                    }
                    //copy log
                    if (!IsCopied && System.IO.File.Exists(BackupPath + fileName + ".log"))
                    {
                        if (System.IO.File.Exists(previousPath + fileName + ".log")) System.IO.File.Delete(previousPath + fileName + ".log");
                        System.IO.File.Move(BackupPath + fileName + ".log", previousPath  + fileName + ".log");
                    }
                    if (!IsCopied && System.IO.File.Exists(BackupPath + fileName + ".logReg"))
                    {
                        if (System.IO.File.Exists(previousPath + fileName + ".logReg")) System.IO.File.Delete(previousPath + fileName + ".logReg");
                        System.IO.File.Move(BackupPath + fileName + ".logReg", previousPath + fileName + ".logReg");
                    }
                    #endregion

                    string fnameTlb = " /tlb:\"" + path + fileName + ".tlb\"";                    
                    string regFile = " /regfile:\"" + BackupPath + fileName + ".reg\"";
                    string dll = " \"" + dllpath + "\"";

                    string argumentsReg = dll +  regFile + (codebase ? " /codebase" : "") + " /verbose";
                    string arguments = dll + fnameTlb + (codebase ? " /codebase" : "") + " /verbose";

                    #region Build RegFile
                    if (System.IO.File.Exists(BackupPath + fileName + ".reg")) System.IO.File.Delete(BackupPath + fileName + ".reg");
                    if (!ExecProcess(argumentsReg, regasmPath, path) || !System.IO.File.Exists(BackupPath + fileName + ".reg"))
                    {
                        Logger.Instance.AdddLog(LogType.Error, "Could not create registry data ! --> exit (" + dllpath + ")");
                        ok = false;
                    }
                    #endregion

                    #region Register Dll
                    if (!buildRegfile && System.IO.File.Exists(path + fileName + ".tlb")) System.IO.File.Delete(path + fileName + ".tlb");
                    if ((!buildRegfile && !ExecProcess(arguments, regasmPath, path)) || !System.IO.File.Exists(path + fileName + ".tlb"))
                    {
                        return false;
                    } 
                    #endregion
                    else
                    {
                        #region is registered?
                        if (!buildRegfile)
                        {
                            if (DllRegister.IsRegistered(out bit64, out gac, dllpath, false, outpath, time))
                            {
                                Logger.Instance.AdddLog(LogType.Info, "DLL Register " + (bit64 ? "(64bit) " : "") + (gac ? "installed in GAC " : "") + "-->" + dllpath + " OK!", "DllRegister", "");
                                ok = true;

                                if (System.IO.File.Exists(path + fileName + ".dll"))
                                {
                                    if (System.IO.File.Exists(BackupPath + fileName + ".dll")) System.IO.File.Delete(BackupPath + fileName + ".dll");
                                    System.IO.File.Copy(path + fileName + ".dll", BackupPath + fileName + ".dll");
                                }
                                if (System.IO.File.Exists(path + fileName + ".tlb"))
                                {
                                    if (System.IO.File.Exists(BackupPath + fileName + ".tlb")) System.IO.File.Delete(BackupPath + fileName + ".tlb");
                                    System.IO.File.Copy(path + fileName + ".tlb", BackupPath + fileName + ".tlb");
                                }
                            }
                            else
                            {
                                Logger.Instance.AdddLog(LogType.Error, "DLL Register " + (bit64 ? "(64bit) " : "") + (gac ? "installed in GAC " : "") + "-->" + dllpath + " ERROR!", "DllRegister", "");
                                ok = false;
                            }
                        }
                        else
                        {
                            if (System.IO.File.Exists(BackupPath + fileName + ".reg"))
                            {
                                ok = true;

                                registryCode = System.IO.File.ReadAllText(BackupPath + fileName + ".reg");

                                //foreach (var item in System.IO.File.ReadAllLines(outpath + fileName + ".reg"))
                                //{
                                //    registryCode += item + Environment.NewLine;
                                //}
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    #region GAC
                    if (DllRegister.RegisterInGAC(dllpath))
                    {
                        Logger.Instance.AdddLog(LogType.Info, "DLL Register in GAC OK! " + dllpath, "DllRegister");
                    } 
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Logger.Instance.AdddLog(LogType.Error, "Register? " + dllpath, "DllRegister", "", ex);
            }
            return ok;
        }

        public static bool UnRegister(string dllpath, string regasmPath, string outpath, string time, bool installInGac = false)
        {
            RegasmPath = regasmPath;
    
            bool ok = false;
            try
            {
                if (!installInGac)
                {
                    if (DllRegister.IsRegistered(out bool bit64, out bool gac, dllpath,true, outpath, time))
                    {

                        #region set path
                        string path = System.IO.Path.GetDirectoryName(dllpath) + "\\";
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(dllpath);
                        string fileName_backup = fileName + "_reg\\";

                        if (string.IsNullOrWhiteSpace(outpath) || !System.IO.Directory.Exists(outpath) || !Logger.IsWritable(outpath))
                        {
                            Logger.Instance.AdddLog(LogType.Error, "Check the output path, set to: " + path, "DllRegister", "");
                            outpath = path;
                        }
                        if (!outpath.EndsWith("\\")) outpath += "\\";
                        BackupPath = outpath + fileName_backup;
                        LogFilePath = BackupPath + "UnReg_" + fileName + ".log"; 
                        #endregion


                        //FileInfo fi = new FileInfo(dllpath);
                        string dll = "\"" + dllpath + "\"";
                        string fnameTlb = path + fileName + ".tlb";
                        //tlb ?
                        if (System.IO.File.Exists(fnameTlb))
                        { fnameTlb = ":\"" + fnameTlb + "\""; }
                        else
                        { fnameTlb = string.Empty; }

                        string arguments = dll + " /unregister" + " /tlb" + fnameTlb;

                        if(!ExecProcess(arguments, regasmPath, path)) return false;

                        Logger.Instance.AdddLog(LogType.Info, "Test:" + dllpath, "DllRegister", "");
                        if (!DllRegister.IsRegistered(out bit64, out gac, dllpath,false, outpath, time))
                        {
                            Logger.Instance.AdddLog(LogType.Info, "DLL Unregister  " + dllpath + " OK!", "DllRegister", "");
                            ok = true;
                        }
                        else
                        {
                            Logger.Instance.AdddLog(LogType.Error, "DLL Unregister  " + dllpath + " ERROR?", "DllRegister", "");
                            ok = false;
                        }

                    }
                }
                else
                {
                    if (DllRegister.UnregisterInGAC(dllpath))
                    {
                        Logger.Instance.AdddLog(LogType.Info, "DLL Unregister in GAC OK! " + dllpath, "DllRegister", "");
                    }
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Logger.Instance.AdddLog(LogType.Error, "Unregister? " + dllpath, "DllRegister", "", ex);
            }


            return ok;
        }

        #region IsRegistered
        public static bool IsRegistered(out bool bit64, out bool gac, string path, bool correct,string outPath, string time)
        {
            gac = false;
            bit64 = false;
            int error = 0;
            StringBuilder message = new StringBuilder();

            #region Direct loadable?
            if (DllRegister.IsComDllLoadable(path))
            {
                message.AppendLine("The DLL is loadable over path: " + path);
            }
            else
            {
                if (DllRegister.IsComDllLoadable(System.IO.Path.GetFileName(path)))
                {
                    message.AppendLine("The DLL is (loadable and) registered in GAC: " + System.IO.Path.GetFileName(path));
                    gac = true;
                }
                else
                {
                    message.AppendLine("Error: The DLL is not loadable! " + System.IO.Path.GetFileName(path));
                    error++;
                }
            } 
            #endregion

            #region main 32bit & 64bit
            string regKey = string.Empty;
            string newpath = string.Empty;
            string CLSID = string.Empty;
            if ((regKey = SearchRegistryNative(System.IO.Path.GetFileName(path), out CLSID, out newpath, "CLSID")) == string.Empty)
            {
                message.AppendLine(@"No CLSID fond in CLASSES_ROOT\CLSID (for DLLs 32bit) ! " + System.IO.Path.GetFileName(path));
            }
            else
            {
                if (path.ToUpper() != newpath.ToUpper())
                {
                    message.AppendLine(@"ERROR| The DLL path found in the registry differs from the new path to DLL");
                    message.AppendLine(@"DLL path: " + path);
                    message.AppendLine(@"new path: " + newpath);
                    if (correct &&
                        MessageBox.Show("The DLL path found in the registry differs from the new path to DLL. Should this path be corrected?" + Environment.NewLine + regKey, "Should this path be corrected", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    {
                        Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                        UnRegister(newpath, RegasmPath, outPath, time);
                        CleanReg(CLSID, newpath);
                        return true;
                    }
                }
            }
            #endregion

            #region 64bit
            string newpath64 = string.Empty;
            string CLSID64 = string.Empty;
            if ((regKey = SearchRegistryNative(System.IO.Path.GetFileName(path), out CLSID64, out newpath64, "Wow6432Node\\CLSID")) == string.Empty)
            {
                message.AppendLine(@"No CLSID fond in CLASSES_ROOT\Wow6432Node\CLSID (for DLLs 64bit) ! " + System.IO.Path.GetFileName(path));
            }
            else
            {
                bit64 = true;
                if (path.ToUpper() != newpath64.ToUpper())
                {
                    message.AppendLine(@"ERROR| The DLL path found in the registry (Wow6432Node) differs from the new path to DLL:");
                    message.AppendLine(@"DLL path: " + path);
                    message.AppendLine(@"new path: " + newpath64);
                    if (correct &&
                        MessageBox.Show("The DLL path found in the registry (Wow6432Node) differs from the new path to DLL. Should this path be corrected?" + Environment.NewLine + regKey, "Should this path be corrected", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    {
                        Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                        UnRegister(newpath, RegasmPath, outPath,time);
                        CleanReg(CLSID, newpath);
                        return true;
                    }
                }
            } 
            #endregion


            if (string.IsNullOrEmpty(CLSID) || string.IsNullOrEmpty(CLSID64))
            {
                string Id = GetCLSIDFromDllName(System.IO.Path.GetFileNameWithoutExtension(path));
                if (Id !=  string.Empty)
                {
                    RegistryKey t_clsidSubKey = Registry.ClassesRoot.OpenSubKey("CLSID" + "\\{" + Id + "}\\InProcServer32");
                    if (t_clsidSubKey == null)
                    {
                        t_clsidSubKey = Registry.ClassesRoot.OpenSubKey("Wow6432Node\\CLSID" + "\\{" + Id + "}\\InProcServer32");
                    }
                    if (t_clsidSubKey == null)
                    {
                        message.AppendLine(@"No CLSID fond in registry! " + System.IO.Path.GetFileName(path));                        
                        Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                        return false;
                    }


                    if (SearchValueRegistryNative(t_clsidSubKey.Name, System.IO.Path.GetFileName(path), out string value))
                    {
                        regKey = t_clsidSubKey.Name;
                        newpath = value;
                        CLSID = Id;
                    }
                    else
                    {
                        message.AppendLine(@"No CLSID fond in registry! " + System.IO.Path.GetFileName(path));
                        Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                        return false;
                    }
                }
                else
                {
                    message.AppendLine(@"No CLSID fond in registry! " + System.IO.Path.GetFileName(path));
                    Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                    return false;
                }
            }
            if (CLSID != CLSID64)
            {
                message.AppendLine("ERROR| The CLSID is different from Wow6432Node:");
                message.AppendLine("in CLSID      : " + CLSID);
                message.AppendLine("in Wow6432Node: " + CLSID64);
            }
            if (newpath != newpath64)
            {
                message.AppendLine("ERROR| The CLSID path is different from Wow6432Node path:");
                message.AppendLine("in CLSID      : " + newpath);
                message.AppendLine("in Wow6432Node: " + newpath64);
            }

            message.AppendLine("The CLSID " + CLSID + " " + (CLSID64 != ""  && CLSID != CLSID64 ? "(64bit " + CLSID + ")" :"" ) + " was found in the registry!");
            Logger.Instance.AdddLog(LogType.Info, message.ToString(), "DllRegister");
            return true;
        } 
        #endregion

        #region Get CLSID from Dll name
        public static string GetCLSIDFromDllName(string dllName)
        {            
            Guid ? CLSID = null;
            string CLSIDSTR = string.Empty;
            if (!string.IsNullOrWhiteSpace(dllName) && !dllName.ToLower().EndsWith(".dll"))
            {
                try
                {
                    RegistryKey OurKey = GetRegValueSave(dllName, Registry.ClassesRoot);
                    if (OurKey != null)
                    {
                        OurKey = GetRegValueSave("CLSID", OurKey);
                        if (OurKey != null)
                        {
                            var t = OurKey.GetValue("");
                            if (t != null && t.GetType() == typeof(string))
                            {
                                CLSIDSTR = t.ToString().Replace("{", "").Replace("}", "");
                                if (!string.IsNullOrWhiteSpace(CLSIDSTR))
                                {
                                    CLSID = Guid.Parse(CLSIDSTR);
                                    Logger.Instance.AdddLog(LogType.Info, "Found -> CLSID:" + CLSIDSTR + Environment.NewLine + " Path: " + dllName + Environment.NewLine + OurKey.Name + Environment.NewLine, "DllRegister", "");
                                }
                            }
                            else
                            {
                                Logger.Instance.AdddLog(LogType.Error, "No default value in reg. Key (CLSID) ?", "DllRegister", "");
                            }
                        }
                        else
                        {
                            Logger.Instance.AdddLog(LogType.Error, "CLSID key not fount ?", "DllRegister", "");
                        }
                    }
                    else
                    {
                        Logger.Instance.AdddLog(LogType.Error, dllName + " key not fount ?", "DllRegister", "");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.AdddLog(LogType.Error, "Get CLSID?", "DllRegister", "", ex);
                    return string.Empty;
                }
            }
            return CLSIDSTR;
        }
        #endregion

        #region GetRegValueSave
        private static RegistryKey GetRegValueSave(string RegKeyName, RegistryKey RegKey)
        {
            if (string.IsNullOrEmpty(RegKeyName) || RegKey == null) return null;
            foreach (string Keyname in RegKey.GetSubKeyNames())
            {
                if (Keyname.Contains(RegKeyName))
                {
                    return RegKey.OpenSubKey(Keyname);
                }
            }
            Logger.Instance.AdddLog(LogType.Error, "Registration key not found? " + RegKeyName, "DllRegister", "");
            return null;
        }
        #endregion

        #region SearchRegistryNative

        /// <summary>
        /// Search and Find Registry Function
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="CLSID"></param>
        /// <param name="path">filepath</param>
        /// <param name="root">Start point in reg</param>
        /// <returns></returns>
        public static string SearchRegistryNative(string dllName, out string CLSID, out string path, string root)
        {
            CLSID = string.Empty;
            path = string.Empty;
            if (string.IsNullOrWhiteSpace(dllName) || !dllName.ToLower().EndsWith(".dll")) return string.Empty;


            //Registry.ClassesRoot.DeleteSubKeyTree("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}");
            //Registry.ClassesRoot.CreateSubKey("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}\\InProcServer32");

            //RegistryKey t_ = Registry.ClassesRoot.OpenSubKey("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}\\InProcServer32");
            //Registry.SetValue(t_.Name,"", @"E:\Nextcloud\Projekt\C#\Register DLL\DLL Temlate\BluefrogLibrary\bin\Releas\BluefrogLibary.dll", RegistryValueKind.String);


            //Open the HKEY_CLASSES_ROOT\CLSID which contains the list of all registered COM files (.ocx,.dll, .ax) 
            //on the system no matters if is 32 or 64 bits.
            RegistryKey t_clsidKey = Registry.ClassesRoot.OpenSubKey(root);

            //Get all the sub keys it contains, wich are the generated GUID of each COM.            
            foreach (string subKeyName in t_clsidKey.GetSubKeyNames())
            {
                //if (subKeyName.ToUpper() == "{88ABA9D3-5980-4A42-871E-FAB235F8A7CD}".ToUpper())
                //{
                //}

                //For each CLSID\GUID key we get the InProcServer32 sub-key .
                string regPath = root + "\\" + subKeyName + "\\InProcServer32";
                if (SearchValueRegistryNative(regPath, dllName, out string value))
                {
                    CLSID = subKeyName;
                    path = value.Replace(@"file:///", "").Replace(@"FILE:///", "").Replace(@"/",@"\");
                    Logger.Instance.AdddLog(LogType.Info, "Found -> CLSID:" + CLSID + " Path: " + path + Environment.NewLine + regPath + Environment.NewLine, "DllRegister", "");
                    if (!System.IO.File.Exists(path)) Logger.Instance.AdddLog(LogType.Error, "File not found Path from RegKey-> (" + root + "\\" + CLSID + ") Path: " + path, "DllRegister", "");
                    return regPath;
                }
            }

            //if not exist, return nothing
            return string.Empty;
        }


        /// <summary>
        /// Find Values
        /// </summary>
        /// <param name="regPath"></param>
        /// <param name="dllName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static  bool SearchValueRegistryNative(string regPath, string dllName, out string value)
        {
            value = string.Empty;

            RegistryKey t_clsidSubKey = Registry.ClassesRoot.OpenSubKey(regPath);
            if ((t_clsidSubKey != null) && (t_clsidSubKey.GetValueNames()).Length > 0)
            {
                foreach (var t_value in from string ValueName in t_clsidSubKey.GetValueNames()
                                        let t_value = t_clsidSubKey.GetValue(ValueName).ToString()
                                        where t_value.ToUpper().EndsWith(dllName.ToUpper())
                                        select t_value)
                {
                    value = t_value;
                    return true;
                }
            }
            return false;
        }
        #endregion

            #region GAC
            public static bool RegisterInGAC(string path)
        {
            UnregisterInGAC(path);
            try
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(path);
                Logger.Instance.AdddLog(LogType.Debug, "Register in GAC -> Ok!", "DllRegister", "");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.AdddLog(LogType.Error, "Register in GAC?", "DllRegister", "", ex);
                return false;
            }
        }
        public static bool UnregisterInGAC(string path)

        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    Assembly z = System.Reflection.Assembly.LoadFile(path);
                    if (z != null && IsAssemblyInGAC(z.FullName))
                    {
                        Logger.Instance.AdddLog(LogType.Debug, "DLL found in GAC!", "DllRegister", "");
                        z = null;
                    }
                }
            }
            catch { }


            try
            {
                new System.EnterpriseServices.Internal.Publish().GacRemove(path);
                Logger.Instance.AdddLog(LogType.Debug, "Unregister in GAC -> Ok!", "DllRegister", "");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.AdddLog(LogType.Error, "Unregister in GAC?", "DllRegister", "", ex);
                return false;
            }
        }

        public static bool IsAssemblyInGAC(string assemblyFullName)
        {
            try
            {
                Assembly t = Assembly.ReflectionOnlyLoad(assemblyFullName);

                return t.GlobalAssemblyCache;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAssemblyInGAC(Assembly assembly)
        {
            return assembly.GlobalAssemblyCache;
        } 
        #endregion

        public static bool CleanReg(string CLSID, string path) 
        {
            //RegistryKey p = Registry.ClassesRoot.OpenSubKey(root + "\\" + subKeyName);
            //if (p != null)
            //{
            //    Registry.ClassesRoot.DeleteSubKeyTree(root + "\\" + subKeyName, false);
            //}
            return false; 
        }

        #region ExecProcess
        private static bool ExecProcess(string arguments, string regasemfileName, string workingDirectory)
        {
            if (!System.IO.Directory.Exists(workingDirectory))
            {
                Logger.Instance.AdddLog(LogType.Error, "Working directory not found for DLL!", "DllRegister");
                return false;
            }



            Logger.Instance.AdddLog(LogType.Info, regasemfileName + " " + arguments, "DllRegister");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.StartInfo.FileName = regasemfileName;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.OutputDataReceived += P_OutputDataReceived1;
            p.ErrorDataReceived += P_ErrorDataReceived1;
            p.EnableRaisingEvents = true;

            p.Start();
            Logger.Instance.AdddLog(LogType.Info, Environment.NewLine + p.StandardOutput.ReadToEnd(), "DllRegister");
            p.WaitForExit();
            return true;
        }

        #region Console output
        private static void P_ErrorDataReceived1(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Logger.Instance.AdddLog(LogType.Error, "Data received! Data->" + (e.Data != null && e.Data.Length > 0 ? e.Data : ""), "DllRegister", "");
        }

        private static void P_OutputDataReceived1(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Logger.Instance.AdddLog(LogType.Error, "Error data received! Data->" + (e.Data != null && e.Data.Length > 0 ? e.Data : ""), "DllRegister", "");
        }
        #endregion 
        #endregion

    }
}
