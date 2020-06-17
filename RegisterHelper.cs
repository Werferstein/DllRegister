/*
Ingolf Hill
werferstein.org
/*
  This program is free software. It comes without any warranty, to
  the extent permitted by applicable law. You can redistribute it
  and/or modify it under the terms of the Do What The Fuck You Want
  To Public License, Version 2, as published by Sam Hocevar.
*/

using Microsoft.Win32;
using SLogging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DllRegister
{





    public class DllRegister
    {

        [DllImport("kernel32")]
        public extern static bool FreeLibrary(int hLibModule);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public extern static IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        
        // Delegate with function signature for the GetVersion function
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U4)]
        delegate UInt32 GetVersionDelegate(
            [OutAttribute][InAttribute] StringBuilder versionString,
            [OutAttribute] UInt32 length);


        private static string RegasmPath = string.Empty;
        public static string BaseBackupPath { get; private set; }
        public static string LogFilePath { get; private set; }


        public static bool IsComDllLoadable(string DllName)
        {
            //return false;
            if (String.IsNullOrEmpty(DllName)) throw new ArgumentNullException("libPath");

            IntPtr moduleHandle = LoadLibrary(DllName);
            if (moduleHandle == IntPtr.Zero)
            {
                var lasterror = Marshal.GetLastWin32Error();
                var innerEx = new Win32Exception(lasterror);
                innerEx.Data.Add("LastWin32Error", lasterror);
                Logger.Instance.AdddLog(LogType.Error, "Can't load DLL! " + DllName, "DllRegister","", innerEx);
                
                return false;
            }


            //// Get handle to GetVersion method
            //IntPtr get_version_handle = GetProcAddress(moduleHandle, "Version");
            ////If we get a handle, we can get the function pointer and cast it to the delegate.
            //// If successful, load function pointer
            //if (get_version_handle != IntPtr.Zero)
            //{
            //    var _getversion = (GetVersionDelegate)Marshal.GetDelegateForFunctionPointer(
            //        get_version_handle,
            //        typeof(GetVersionDelegate));

            //    if (_getversion != null)
            //    {
            //        // Allocate buffer
            //        var size = 100;
            //        StringBuilder builder = new StringBuilder(size);

            //        // Get version string
            //        _getversion(builder, (uint)size);

            //        // Return string
            //        //return builder.ToString();
            //    }                
            //}
            FreeLibrary(moduleHandle);
            return true;
            //int libId = LoadLibrary(DllName);
            //if (libId != 0) FreeLibrary(libId);
            //return (libId > 0);
        }


        public static bool Register(Settings setting, string timeId)
        //(string outpath, string dllpath, string regasmPath,  bool setting.InstallInGAC, bool buildRegfile, bool codebase,string time, out string registryCode )
        {
            if (setting == null || setting.FileItems == null || setting.FileItems.Count == 0 || string.IsNullOrWhiteSpace(setting.RegisterEXE))
            {
                Logger.Instance.AdddLog(LogType.Error, "Missing values in setting!", "DllRegister", "");
                return false;
            }
            int regCount = 0;
            bool error = false;
            bool IsCopied = false;

            if (!TestOutputPath(setting))
            {
                Logger.Instance.AdddLog(LogType.Warn, "No write access to the path: " + setting.OutputPath, "DllRegister", "");
                return false;
            }






            BaseBackupPath = setting.OutputPath + setting.ProjectName + "\\";
            if (!System.IO.Directory.Exists(BaseBackupPath)) System.IO.Directory.CreateDirectory(BaseBackupPath);
            if (!System.IO.Directory.Exists(BaseBackupPath + "Backup")) System.IO.Directory.CreateDirectory(BaseBackupPath + "Backup");

            //set previous result path
            string previousPath = BaseBackupPath + "Backup\\" + timeId + "\\";
            if (!System.IO.Directory.Exists(previousPath))
            { System.IO.Directory.CreateDirectory(previousPath); }
            else
            {
                IsCopied = true;
            }



            string registryCode = string.Empty;
            foreach (var FileItem in setting.FileItems)
            {
                if (setting.FileItems.Count > 1)
                {
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", "DllRegister");
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", "DllRegister");
                }
                string tmpRegistryCode = string.Empty;


                #region file item test                
                if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;
                if (!System.IO.File.Exists(FileItem.FullPath))
                {
                    Logger.Instance.AdddLog(LogType.Error, "File not exist! " + FileItem.FullPath);
                    error = true;
                    continue;
                }
                #endregion

                try
                {
                    if (!setting.InstallInGAC)
                    {
                        #region UnRegister ?
                        if (!setting.BuildRegistryKey && DllRegister.IsRegistered(out bool bit64, out bool gac, FileItem.FullPath, true, setting.OutputPath, timeId))
                        {
                            if (!UnRegister(FileItem.FullPath, setting.RegisterEXE, setting.OutputPath, timeId, setting.InstallInGAC))
                            {
                                Logger.Instance.AdddLog(LogType.Error, "The already registered Dll cannot be unregistered! --> exit " + FileItem.FullPath, "DllRegister", "");
                                error = true;
                                //continue;
                            }
                        }
                        #endregion

                        #region set path
                        //Get Dll paht
                        string path = System.IO.Path.GetDirectoryName(FileItem.FullPath) + "\\";

                        //get Dll name
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(FileItem.FullPath);

                        //set tlb path
                        string tlbPath = path;
                        //Is Dll paht writable?
                        if (!Logger.IsWritable(tlbPath))
                        {
                            Logger.Instance.AdddLog(LogType.Error, "No write access to the path: " + path, "DllRegister", "");
                            tlbPath = BaseBackupPath;
                            Logger.Instance.AdddLog(LogType.Error, "Set tlb output to : " + tlbPath, "DllRegister", "");
                        }
                        tlbPath += fileName + ".tlb";


                        #endregion

                        #region Backup?                                       



                        //copy reg
                        if (!IsCopied && setting.BuildRegistryKey && System.IO.File.Exists(BaseBackupPath + fileName + ".reg"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".reg")) System.IO.File.Delete(previousPath + fileName + ".reg");
                            System.IO.File.Move(BaseBackupPath + fileName + ".reg", previousPath + fileName + ".reg");
                        }
                        //copy tlb
                        if (!IsCopied && !setting.BuildRegistryKey && System.IO.File.Exists(BaseBackupPath + fileName + ".tlb"))
                        {
                            if (System.IO.File.Exists(previousPath + "bak_" + fileName + ".tlb")) System.IO.File.Delete(previousPath + "bak_" + fileName + ".tlb");
                            System.IO.File.Move(BaseBackupPath + fileName + ".tlb", previousPath + "bak_" + fileName + ".tlb");
                        }

                        if (!IsCopied && System.IO.File.Exists(path + fileName + ".tlb"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".tlb")) System.IO.File.Delete(previousPath + fileName + ".tlb");
                            System.IO.File.Move(path + fileName + ".tlb", previousPath + fileName + ".tlb");
                        }
                        //copy dll
                        if (!IsCopied && !setting.BuildRegistryKey && System.IO.File.Exists(BaseBackupPath + fileName + ".dll"))
                        {
                            if (System.IO.File.Exists(previousPath + "bak_" + fileName + ".dll")) System.IO.File.Delete(previousPath + "bak_" + fileName + ".dll");
                            System.IO.File.Move(BaseBackupPath + fileName + ".dll", previousPath + "bak_" + fileName + ".dll");
                        }

                        if (!IsCopied && !setting.BuildRegistryKey && System.IO.File.Exists(path + fileName + ".dll"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".dll") && path != previousPath) System.IO.File.Delete(previousPath + fileName + ".dll");
                            System.IO.File.Copy(path + fileName + ".dll", previousPath + fileName + ".dll");
                        }
                        //copy log
                        if (!IsCopied && System.IO.File.Exists(BaseBackupPath + fileName + ".log"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".log")) System.IO.File.Delete(previousPath + fileName + ".log");
                            System.IO.File.Move(BaseBackupPath + fileName + ".log", previousPath + fileName + ".log");
                        }
                        if (!IsCopied && System.IO.File.Exists(BaseBackupPath + fileName + ".logReg"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".logReg")) System.IO.File.Delete(previousPath + fileName + ".logReg");
                            System.IO.File.Move(BaseBackupPath + fileName + ".logReg", previousPath + fileName + ".logReg");
                        }
                        #endregion

                        string fnameTlb = " /tlb:\"" + tlbPath + "\"";
                        string regFile = " /regfile:\"" + BaseBackupPath + fileName + ".reg\"";
                        string dll = " \"" + FileItem.FullPath + "\"";

                        string argumentsReg = dll + regFile + (setting.Codebase ? " /codebase" : "") + " /verbose";
                        string arguments = dll + fnameTlb + (setting.Codebase ? " /codebase" : "") + " /verbose";

                        #region Build RegFile
                        if (System.IO.File.Exists(BaseBackupPath + fileName + ".reg")) System.IO.File.Delete(BaseBackupPath + fileName + ".reg");
                        if (!ExecProcess(argumentsReg, setting.RegisterEXE, path) || !System.IO.File.Exists(BaseBackupPath + fileName + ".reg"))
                        {
                            Logger.Instance.AdddLog(LogType.Error, "Could not create registry data ! --> exit (" + FileItem.FullPath + ")");
                            error = true;
                            continue;
                        }
                        //read reg code from file
                        tmpRegistryCode = System.IO.File.ReadAllText(BaseBackupPath + fileName + ".reg");
                        #endregion

                        #region Register Dll
                        if (!setting.BuildRegistryKey)
                        {
                            //delete old result
                            if (System.IO.File.Exists(tlbPath)) System.IO.File.Delete(tlbPath);
                            if (!ExecProcess(arguments, setting.RegisterEXE, path) || !System.IO.File.Exists(tlbPath))
                            {
                                //error
                                Logger.Instance.AdddLog(LogType.Error, "Could not register dll! --> exit (" + FileItem.FullPath + ")");
                                error = true;
                                continue;
                            }
                            #region is registered?                            


                            if (DllRegister.IsRegistered(out bit64, out gac, FileItem.FullPath, false, setting.OutputPath, timeId))
                            {
                                Logger.Instance.AdddLog(LogType.Info, "DLL Register " + (bit64 ? "(64bit) " : "") + (gac ? "installed in GAC " : "") + "-->" + FileItem.FullPath + " OK!", "DllRegister", "");

                                if (System.IO.File.Exists(path + fileName + ".dll"))
                                {
                                    if (System.IO.File.Exists(BaseBackupPath + fileName + ".dll")) System.IO.File.Delete(BaseBackupPath + fileName + ".dll");
                                    System.IO.File.Copy(path + fileName + ".dll", BaseBackupPath + fileName + ".dll");
                                }
                                if (System.IO.File.Exists(path + fileName + ".tlb"))
                                {
                                    if (System.IO.File.Exists(BaseBackupPath + fileName + ".tlb")) System.IO.File.Delete(BaseBackupPath + fileName + ".tlb");
                                    System.IO.File.Copy(path + fileName + ".tlb", BaseBackupPath + fileName + ".tlb");
                                }
                            }
                            else
                            {
                                Logger.Instance.AdddLog(LogType.Error, "DLL Register " + (bit64 ? "(64bit) " : "") + (gac ? "installed in GAC " : "") + "-->" + FileItem.FullPath + " ERROR!", "DllRegister", "");
                                error = true;
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region GAC
                        error = !DllRegister.RegisterInGAC(FileItem.FullPath);
                        if (!error) Logger.Instance.AdddLog(LogType.Info, "DLL Register in GAC OK! " + FileItem.FullPath, "DllRegister");   
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    Logger.Instance.AdddLog(LogType.Error, "Register? " + FileItem.FullPath, "DllRegister", "", ex);
                }

                #region Build registry code
                if (!string.IsNullOrWhiteSpace(tmpRegistryCode))
                {
                    regCount++;
                    registryCode += "/*--" + FileItem.FullPath + Environment.NewLine +
                        "------------------------------------------------------" + Environment.NewLine + Environment.NewLine +
                        tmpRegistryCode + Environment.NewLine + Environment.NewLine;
                }
                #endregion

            }//foreach (var FileItem in setting.FileItems)



            #region save all reg code
            if (regCount > 1 && !string.IsNullOrWhiteSpace(registryCode) && !string.IsNullOrWhiteSpace(DllRegister.BaseBackupPath) && System.IO.Directory.Exists(DllRegister.BaseBackupPath))
            {
                System.IO.File.WriteAllText(DllRegister.LogFilePath + "Reg", registryCode);
            }
            #endregion
            return !error;
        }

        public static bool UnRegister(Settings setting, string timeId)
        {
            if (setting == null || setting.FileItems == null || string.IsNullOrWhiteSpace(setting.RegisterEXE)) throw new Exception("Missing values in Setting!");
            int regCount = 0;
            bool error = false;
            bool IsCopied = false;

            if (!TestOutputPath(setting))
            {
                Logger.Instance.AdddLog(LogType.Warn, "No write access to the path: " + setting.OutputPath, "DllRegister", "");
                return false;
            }


            BaseBackupPath = setting.OutputPath + setting.ProjectName + "\\";
            if (!System.IO.Directory.Exists(BaseBackupPath)) System.IO.Directory.CreateDirectory(BaseBackupPath);
            //if (!System.IO.Directory.Exists(BaseBackupPath + "Backup")) System.IO.Directory.CreateDirectory(BaseBackupPath + "Backup");

            ////set previous result path
            //string previousPath = BaseBackupPath + "Backup\\" + timeId + "\\";
            //if (!System.IO.Directory.Exists(previousPath))
            //{ System.IO.Directory.CreateDirectory(previousPath); }
            //else
            //{
            //    IsCopied = true;
            //}

      
            foreach (var FileItem in setting.FileItems)
            {
                if (setting.FileItems.Count > 1)
                {
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", "DllRegister");
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", "DllRegister");
                }

                #region file item test                
                if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;
                if (!System.IO.File.Exists(FileItem.FullPath))
                {
                    Logger.Instance.AdddLog(LogType.Error, "File not exist! " + FileItem.FullPath);
                }
                #endregion

                //UnRegister
                if (!UnRegister(FileItem.FullPath, setting.RegisterEXE, setting.OutputPath, timeId, setting.InstallInGAC))
                {
                    Logger.Instance.AdddLog(LogType.Error, "Unregister ERROR! " + FileItem.FullPath, "DllRegister", "");
                    error = true;
                    continue;
                }               
            }//foreach (var FileItem in setting.FileItems)
            return !error;
        }



        public static bool UnRegister(string dllpath, string regasmPath, string outpath, string time, bool installInGac = false)
        {
            RegasmPath = regasmPath;
    
            bool ok = false;
            try
            {
                if (!installInGac)
                {
                    DllRegister.IsRegistered(out bool bit64, out bool gac, dllpath, true, outpath, time);

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

                        if (!System.IO.Directory.Exists(BaseBackupPath)) Logger.Instance.AdddLog(LogType.Error, "Backup path?: " + BaseBackupPath, "DllRegister", "");                        
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
                    ok = DllRegister.UnregisterInGAC(dllpath);
                    if (ok) Logger.Instance.AdddLog(LogType.Info, "DLL Unregister in GAC OK! " + dllpath, "DllRegister", "");
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Logger.Instance.AdddLog(LogType.Error, "Unregister? " + dllpath, "DllRegister", "", ex);
            }


            return ok;
        }


        #region TestOutputPath
        private static bool TestOutputPath(Settings setting)
        {
            if (Environment.Is64BitProcess)
            {
                Logger.Instance.AdddLog(LogType.Info, "This is a 64 bit proccess!");
            }


            if (string.IsNullOrWhiteSpace(setting.OutputPath) || !System.IO.Directory.Exists(setting.OutputPath) || !Logger.IsWritable(setting.OutputPath))
            {

                #region set to dll path ?
                string internalLink = System.IO.Path.GetDirectoryName(setting.FileItems[0].FullPath);
                foreach (var item in setting.FileItems)
                {
                    if (internalLink != System.IO.Path.GetDirectoryName(item.FullPath))
                    {
                        internalLink = string.Empty;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(internalLink) || !Logger.IsWritable(internalLink)) internalLink = string.Empty;
                #endregion

                if (!string.IsNullOrEmpty(internalLink))
                {
                    setting.OutputPath = internalLink;
                    Logger.Instance.AdddLog(LogType.Warn, "Check the output path! set output path to (dll folder): " + setting.OutputPath, "DllRegister", "");
                }
                else
                {
                    setting.OutputPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                    Logger.Instance.AdddLog(LogType.Warn, "Check the output path! set output path to: " + setting.OutputPath, "DllRegister", "");
                }
            }

            if (!setting.OutputPath.EndsWith("\\")) setting.OutputPath += "\\";
            



            if (!Logger.IsWritable(setting.OutputPath))
            {
                return false;
            }


            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9 -]");
            setting.ProjectName = rgx.Replace(setting.ProjectName, "");
            if (string.IsNullOrWhiteSpace(setting.ProjectName)) setting.ProjectName = "RegisterDLL";

            //set log file path
            LogFilePath = setting.OutputPath + setting.ProjectName + ".log";

            return true;
        }
        #endregion

        #region IsRegistered
        public static bool IsRegistered(out bool bit64, out bool gac, string path, bool correct,string outPath, string time)
        {
            gac = false;
            bit64 = false;
            int error = 0;


            StringBuilder message = new StringBuilder();
            message.AppendLine();

            gac = TestIsInGAG(path, message, out string _);

            #region Direct loadable?
            if (DllRegister.IsComDllLoadable(path))
            {
                message.AppendLine("The DLL is direct loadable over path: " + path);
            }
            else
            {
                message.AppendLine("Warning: The DLL is not direct loadable over path! " + System.IO.Path.GetFileName(path));
                //error++;
            }
            #endregion

            #region main 32bit & 64bit
            string regKey = string.Empty;


            RegData regData;
            if ((regKey = SearchRegistryNative(System.IO.Path.GetFileName(path),"CLSID", out regData)) == string.Empty)
            {
                message.AppendLine(@"No CLSID fond in CLASSES_ROOT\CLSID (for DLLs 32bit) ! " + System.IO.Path.GetFileName(path));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(regData.Path) && path.ToUpper() != regData.Path.ToUpper())
                {
                    message.AppendLine(@"ERROR| The DLL path found in the registry differs from the new path to DLL");
                    message.AppendLine(@"DLL path: " + path);
                    message.AppendLine(@"new path: " + regData.Path);
                    if (correct &&
                        MessageBox.Show("The DLL path found in the registry differs from the new path to DLL. Should this path be corrected?" + Environment.NewLine + regKey, "Should this path be corrected", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    {
                        Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                        UnRegister(regData.Path, RegasmPath, outPath, time);
                        CleanReg(regData.CLSID, regData.Path);
                        return true;
                    }
                }
            }
            #endregion

            #region 64bit
            RegData regData64;
            if ((regKey = SearchRegistryNative(System.IO.Path.GetFileName(path), "Wow6432Node\\CLSID", out regData64)) == string.Empty)
            {
                message.AppendLine(@"No CLSID fond in CLASSES_ROOT\Wow6432Node\CLSID (for DLLs 64bit) ! " + System.IO.Path.GetFileName(path));
            }
            else
            {
                bit64 = true;
                if (!string.IsNullOrWhiteSpace(regData64.Path) &&  path.ToUpper() != regData64.Path.ToUpper())
                {
                    message.AppendLine(@"ERROR| The DLL path found in the registry (Wow6432Node) differs from the new path to DLL:");
                    message.AppendLine(@"DLL path: " + path);
                    message.AppendLine(@"new path: " + regData64.Path);
                    if (correct &&
                        MessageBox.Show("The DLL path found in the registry (Wow6432Node) differs from the new path to DLL. Should this path be corrected?" + Environment.NewLine + regKey, "Should this path be corrected", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    {
                        Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                        UnRegister(regData64.Path, RegasmPath, outPath, time);
                        CleanReg(regData64.CLSID, regData64.Path);
                        return true;
                    }
                }
            }
            #endregion


            if (regData != null && !string.IsNullOrWhiteSpace(regData.Class))
            {
                message.Append(@"Search in root for: " + regData.Class + " ->");
                string Id = GetCLSIDFromClassName(regData.Class, regData.CLSID, out int tempError);
                error += tempError;
                if (Id != string.Empty)
                {
                    if (Id == regData.CLSID)
                    {
                        message.AppendLine(@" Found!");
                    }
                    else
                    {
                        message.AppendLine(@"ERROR| Found class name in root but CLSID is different !" + Environment.NewLine + 
                            "CLSID    : " + regData.CLSID + Environment.NewLine +
                            "new CLSID: " + Id);
                        string className = regData.Class;
                        error++;

                        #region 32bit
                        RegData newregData;
                        RegistryKey t_clsidSubKey = Registry.ClassesRoot.OpenSubKey("CLSID" + "\\{" + Id + "}\\InProcServer32");
                        if (!string.IsNullOrEmpty(className) &&  t_clsidSubKey != null)
                        {
                            
                            message.AppendLine("--------------------------------------------------------------------------------------------------------");
                            message.AppendLine("CLSID" + "\\{" + Id + "}\\InProcServer32");                            
                            if (SearchValueRegistryNative(Id, t_clsidSubKey.Name.Replace("HKEY_CLASSES_ROOT\\",""), "", out newregData, className))
                            {
                                message.AppendLine("Found ->" + Environment.NewLine + "Registry:       " + t_clsidSubKey.Name + Environment.NewLine + newregData.GetInfo + Environment.NewLine + "(Keys: " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")");

                                if (!System.IO.File.Exists(newregData.Path)) message.AppendLine("File not found Path from RegKey-> Path: " + newregData.Path);
                            }
                            message.AppendLine("--------------------------------------------------------------------------------------------------------");
                        } 
                        #endregion
                        #region 64bit
                        t_clsidSubKey = Registry.ClassesRoot.OpenSubKey("Wow6432Node\\CLSID" + "\\{" + Id + "}\\InProcServer32");
                        if (!string.IsNullOrEmpty(className) && t_clsidSubKey != null)
                        {
                            message.AppendLine("--------------------------------------------------------------------------------------------------------");
                            message.AppendLine("Wow6432Node\\CLSID" + "\\{" + Id + "}\\InProcServer32");
                            if (SearchValueRegistryNative(Id, t_clsidSubKey.Name.Replace("HKEY_CLASSES_ROOT\\", ""), "", out newregData, className))
                            {
                                message.AppendLine("Found ->" + Environment.NewLine + "Registry:       " + t_clsidSubKey.Name + Environment.NewLine + newregData.GetInfo + Environment.NewLine + "(Keys: " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")");

                                if (!System.IO.File.Exists(newregData.Path)) message.AppendLine("File not found Path from RegKey-> Path: " + newregData.Path);
                            }
                            message.AppendLine("--------------------------------------------------------------------------------------------------------");
                        } 
                        #endregion
                    }
                }
            }
            else
            {
                message.AppendLine(@"No CLSID fond in registry! (CLSID is empty from Dll name)" + System.IO.Path.GetFileName(path));
                Logger.Instance.AdddLog(LogType.Error, message.ToString(), "DllRegister");
                if (error > 0) Logger.Instance.AdddLog(LogType.Error, "ERROR| Check the log file (" + error.ToString() + " errors)!", "DllRegister");
                return false;
            }



            if (regData != null && regData64 != null)
            {
                if (!string.IsNullOrEmpty(regData.CLSID) && !string.IsNullOrEmpty(regData64.CLSID) &&
                    regData.CLSID != regData64.CLSID)
                {
                    message.AppendLine("ERROR| The CLSID is different from Wow6432Node:");
                    message.AppendLine("in CLSID      : " + regData.CLSID);
                    message.AppendLine("in Wow6432Node: " + regData64.CLSID);
                    error++;
                }


            if (!string.IsNullOrEmpty(regData.Path) && !string.IsNullOrEmpty(regData64.Path) &&
                 regData.Path.ToLower() != regData64.Path.ToLower())
            {
                    message.AppendLine("ERROR| The CLSID path is different from Wow6432Node path:");
                    message.AppendLine("in CLSID      : " + regData.Path);
                    message.AppendLine("in Wow6432Node: " + regData64.Path);
                    error++;
                }
            }

            message.AppendLine("The CLSID " + regData.CLSID + " was found in the registry!");
            Logger.Instance.AdddLog(LogType.Info, message.ToString(), "DllRegister");
            if (error > 0)Logger.Instance.AdddLog(LogType.Error, "ERROR| Check the log file (" + error.ToString() + " errors)!", "DllRegister");            
            return true;
        }

        private static bool TestIsInGAG(string path, StringBuilder message,out string newPath )
        {
            bool gac = false;
            newPath = string.Empty;
            if (SystemUtil.IsAssemblyInGAC(System.IO.Path.GetFileNameWithoutExtension(path)))
            {
                message.AppendLine("The DLL is registered in GAC (Fusion): " + System.IO.Path.GetFileName(path));
                if (!gac) gac = true;
            }

            if (FindInGAC_Reg(System.IO.Path.GetFileNameWithoutExtension(path), out string value))
            {
                message.AppendLine("The DLL is registered in GAC (Registry): " + value);
                if (!gac) gac = true;
            }
            if (DllRegister.IsAssemblyInGAC(path, out string xpath))
            {
                newPath = xpath;
                message.AppendLine("The DLL is registered in GAC (Assembly): " + xpath);
                if (!gac) gac = true;
            }
            return gac;
        }
        #endregion

        #region Get CLSID from Dll name
        public static string GetCLSIDFromClassName(string className,string  clsid, out int error)
        {
            error = 0;
            string CLSIDSTR = clsid;
            if (!string.IsNullOrWhiteSpace(className) && !className.ToLower().EndsWith(".dll"))
            {

                string[] firstPart = className.Split(new char[] { '.' });
                if (firstPart.Length != 0 && !string.IsNullOrWhiteSpace(firstPart[0]) && firstPart[0].Length > 4)
                {
                    className = firstPart[0] + "." ;
                }

                List<RegistryKey> OurKey = GetRegValueSave(className, Registry.ClassesRoot);
                if (OurKey != null)
                {

                    foreach (RegistryKey item in OurKey)
                    {
                        Logger.Instance.AdddLog(LogType.Info, "(" + className + ") Fond in class name ->" + item.Name, "DllRegister", "");
                        try
                        {
                            RegistryKey t = item.OpenSubKey("CLSID");
                            if (t != null)
                            {
                                string ID = t.GetValue("").ToString().Replace("{", "").Replace("}", "");
                                Logger.Instance.AdddLog(LogType.Info, "(" + className + ") Found CLSID: " + ID, "DllRegister", "");
                                if (!string.IsNullOrWhiteSpace(CLSIDSTR) && CLSIDSTR != ID)
                                {
                                    Logger.Instance.AdddLog(LogType.Error, "(" + className + ") ERROR| The CLSID is different !", "DllRegister", "");
                                }
                                CLSIDSTR = ID;
                            }
                            else
                            {
                                Logger.Instance.AdddLog(LogType.Error, "ERROR| No CLSID sub key!", "DllRegister", "");
                                error ++;
                            }
                        }
                        catch
                        {
                            Logger.Instance.AdddLog(LogType.Error, "ERROR| CLSID from class name (" + className + " open value in reg.) ?", "DllRegister", "");
                            error ++;
                        }
                    }
                }
            }
            if (CLSIDSTR == string.Empty)
            {
                Logger.Instance.AdddLog(LogType.Error, "ERROR| No CLSID from class name (" + className + ") ?", "DllRegister", "");
                error ++;
            }
            return CLSIDSTR;
        }
        #endregion

        #region GetRegValueSave
        private static List<RegistryKey> GetRegValueSave(string RegKeyName, RegistryKey RegKey, bool all = false)
        {
            List <RegistryKey> regList = new List<RegistryKey>();
            if (string.IsNullOrEmpty(RegKeyName) || RegKey == null) return null;
            foreach (string Keyname in RegKey.GetSubKeyNames())
            {
                if (Keyname.Contains(RegKeyName))
                {
                    regList.Add(RegKey.OpenSubKey(Keyname));
                    if (!all)
                    { return regList; }
                }
            }
            if (regList.Count == 0) regList = null;
            return regList;
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
        public static string SearchRegistryNative(string dllName, string root, out RegData regData)
        {
            regData = null;
            if (string.IsNullOrWhiteSpace(dllName) || !dllName.ToLower().EndsWith(".dll")) return string.Empty;
            SearchInValues = 0;
            SearchKeys = 0;

            //Registry.ClassesRoot.DeleteSubKeyTree("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}");
            //Registry.ClassesRoot.CreateSubKey("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}\\InProcServer32");

            //RegistryKey t_ = Registry.ClassesRoot.OpenSubKey("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}\\InProcServer32");
            //Registry.SetValue(t_.Name,"", @"E:\Nextcloud\Projekt\C#\Register DLL\DLL Temlate\BluefrogLibrary\bin\Releas\BluefrogLibary.dll", RegistryValueKind.String);
            //RegistryKey t_test = Registry.ClassesRoot.OpenSubKey("CLSID\\{7875D9D8-1B94-4AE4-A8F0-63B3F8609336}", System.Security.AccessControl.RegistryRights.FullControl);

            //Open the HKEY_CLASSES_ROOT\CLSID which contains the list of all registered COM files (.ocx,.dll, .ax) 
            //on the system no matters if is 32 or 64 bits.
            RegistryKey t_clsidKey = Registry.ClassesRoot.OpenSubKey(root);
            
           

            //Get all the sub keys it contains, wich are the generated GUID of each COM.            
            foreach (string subKeyName in t_clsidKey.GetSubKeyNames())
            {
                //For each CLSID\GUID key we get the InProcServer32 sub-key .
                string regPath = root + "\\" + subKeyName + "\\InProcServer32";
                if (SearchValueRegistryNative(subKeyName, regPath, dllName, out regData))
                {                         
                    Logger.Instance.AdddLog(LogType.Info, "Found ->" + Environment.NewLine + "Registry:       " + regPath + Environment.NewLine +  regData.GetInfo + Environment.NewLine + "(Keys: "  + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")" + Environment.NewLine , "DllRegister", "");

                    if (!System.IO.File.Exists(regData.Path)) Logger.Instance.AdddLog(LogType.Error, "File not found Path from RegKey-> (" + root + "\\" + regData.CLSID + ") Path: " + regData.Path, "DllRegister", "");
                    return regPath;
                }
            }

            //if not exist, return nothing
            Logger.Instance.AdddLog(LogType.Error, "Not found in Keys (" + root + "): " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString(), "DllRegister", "");
            return string.Empty;
        }

        private static int SearchInValues = 0;
        private static int SearchKeys = 0;

        /// <summary>
        /// Find Values
        /// </summary>
        /// <param name="regPath"></param>
        /// <param name="dllName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static  bool SearchValueRegistryNative(string CLSID, string regPath, string dllName, out RegData regData, string className = "")
        {
            regData = null;
            SearchKeys++;

            RegistryKey t_clsidSubKey = Registry.ClassesRoot.OpenSubKey(regPath);
            if ((t_clsidSubKey != null) && (t_clsidSubKey.GetValueNames()).Length > 0)
            {
                regData = new RegData();
                bool foundDllName = false;
                foreach (string valueName in t_clsidSubKey.GetValueNames())
                {
                    SearchInValues++;
                    if (className != string.Empty)
                    {
                        #region find class name
                        if (regData.SetValue(valueName, t_clsidSubKey.GetValue(valueName).ToString()) && valueName.ToUpper() == "CLASS")
                        {
                            if (regData.Class.ToUpper() == className.ToUpper())
                            {
                                foundDllName = true;
                                regData.CLSID = CLSID;
                            }
                        } 
                        #endregion
                    }
                    else
                    {
                        #region find dll name
                        if (regData.SetValue(valueName, t_clsidSubKey.GetValue(valueName).ToString()) && valueName.ToUpper() == "CODEBASE")
                        {
                            if (regData.Path.ToUpper().EndsWith(dllName.ToUpper()))
                            {
                                foundDllName = true;
                                regData.CLSID = CLSID;
                            }
                        } 
                        #endregion
                    }                 
                }
                if (foundDllName) return true;
            }
            regData = null;
            return false;
        }
        #endregion

        #region GAC
        public static bool RegisterInGAC(string path)
        {
            UnregisterInGAC(path);
            bool ok = false;
            StringBuilder message = new StringBuilder();
            try
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(path);                
                if (TestIsInGAG(path, message, out string newpath))
                {
                    ok = true;
                    if (!string.IsNullOrWhiteSpace(newpath) && System.IO.File.Exists(newpath))
                    {
                        message.AppendLine("Register in GAC -> Ok! " + newpath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.AdddLog(LogType.Error, "Register in GAC?", "DllRegister", "", ex);
            }
            Logger.Instance.AdddLog(LogType.Debug, message.ToString(), "DllRegister", "");
            return ok;
        }
        public static bool UnregisterInGAC(string path)

        {
            bool ok = false;
            StringBuilder message = new StringBuilder();

            try
            {
                if (System.IO.File.Exists(path))
                {                    
                    if (TestIsInGAG(path, message, out string newpath))
                    {
                        if (!string.IsNullOrWhiteSpace(newpath) && System.IO.File.Exists(newpath))
                        {
                            new System.EnterpriseServices.Internal.Publish().GacRemove(newpath);
                            if (!System.IO.File.Exists(newpath))
                            {
                                message.AppendLine("Unregister in GAC -> Ok! " + newpath);                                ;
                                ok = true;                                
                            }
                        }
                        else
                        {
                            message.AppendLine("Unregister in GAC (path not fount over assenbly!) -> " + path);
                            new System.EnterpriseServices.Internal.Publish().GacRemove(path);
                            ok = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.AdddLog(LogType.Error, "Unregister in GAC?", "DllRegister", "", ex);                
            }
            Logger.Instance.AdddLog(LogType.Debug, message.ToString(), "DllRegister", "");
            return ok;


            //try
            //{
            //    new System.EnterpriseServices.Internal.Publish().GacRemove(path);
            //    Logger.Instance.AdddLog(LogType.Debug, "Unregister in GAC -> Ok!", "DllRegister", "");
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    Logger.Instance.AdddLog(LogType.Error, "Unregister in GAC?", "DllRegister", "", ex);
            //    return false;
            //}
        }

        public static bool IsAssemblyInGAC(string assemblyFullName, out string path)
        {
            path = string.Empty;
            try
            {
                Assembly U = Assembly.LoadFrom(assemblyFullName);                
                Assembly t = Assembly.LoadWithPartialName(U.FullName);
                U = null;
                bool result = t.GlobalAssemblyCache;
                if (!result) return false;                
                if (System.IO.File.Exists(t.Location))
                {
                    path = t.Location;
                    t = null;
                }
                else
                {
                    return false;
                }                
                return result;
            }
            catch
            {                
                return false;
            }
        }

        public static bool FindInGAC_Reg(string dllName, out string info)
        {
            info = string.Empty;
            RegistryKey t_clsidSubKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Fusion\\GACChangeNotification\\Default");
            if ((t_clsidSubKey != null))
            {
                foreach (string item in t_clsidSubKey.GetValueNames())
                {
                    if (item.Contains(dllName))
                    {
                        info = item;
                        string h = t_clsidSubKey.GetValue(item).ToString();                        
                        return true;
                    }
                }
            }
            return false;
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
