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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DllRegister
{





    public class DllRegister
    {
        public const string CONSTDllRegisterProject = "DllRegisterProject";


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

        public static bool TestSettings(Settings settings)
        {
            bool error = false;

            #region ProjectName
            if (string.IsNullOrWhiteSpace(settings.ProjectName)) { settings.ProjectName = CONSTDllRegisterProject; error = true; }
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9 -_]");
            if (settings.ProjectName != rgx.Replace(settings.ProjectName, "")) { settings.ProjectName = rgx.Replace(settings.ProjectName, ""); error = true;}
            #endregion

            #region TestOutputPath          
            if (string.IsNullOrWhiteSpace(settings.OutputPath) || !System.IO.Directory.Exists(settings.OutputPath) || !Logger.IsWritable(settings.OutputPath))
            {
                error = true;
                string internalLink = string.Empty;
                if(!Logger.IsWritable(settings.OutputPath))Logger.Instance.AdddLog(LogType.Warn, "No write access to the path: " + settings.OutputPath, "DllRegister", "");

                //A
                #region set to dll path ?
                if (settings.FileItems != null && settings.FileItems.Count > 0)
                {
                    internalLink = System.IO.Path.GetDirectoryName(settings.FileItems[0].FullPath);
                    foreach (var item in settings.FileItems)
                    {
                        if (internalLink != System.IO.Path.GetDirectoryName(item.FullPath))
                        {
                            internalLink = string.Empty;
                            break;
                        }
                    }                    
                }
                #endregion

                if (string.IsNullOrEmpty(internalLink)) internalLink = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                if (string.IsNullOrEmpty(internalLink) || !Logger.IsWritable(internalLink)) throw new System.IO.DirectoryNotFoundException("Output path is not set or not writable!");
                if (!internalLink.EndsWith("\\")) internalLink += "\\";
                settings.OutputPath = internalLink + settings.ProjectName + "\\";
                Logger.Instance.AdddLog(LogType.Warn, "Check the output path! set output path to (dll folder): " + settings.OutputPath, "DllRegister", "");
            }
            #endregion

            if (!settings.OutputPath.EndsWith("\\")) settings.OutputPath += "\\";

           
            if (!System.IO.Directory.Exists(settings.OutputPath)) System.IO.Directory.CreateDirectory(settings.OutputPath);
            if (!System.IO.Directory.Exists(settings.OutputPath + "Backup")) System.IO.Directory.CreateDirectory(settings.OutputPath + "Backup");

            LogFilePath = settings.OutputPath + settings.ProjectName + ".log";  //set log file path          
            return !error;
        }






        public static bool Register(Settings setting, string timeId)
        //(string outpath, string dllpath, string regasmPath,  bool setting.InstallInGAC, bool buildRegfile, bool codebase,string time, out string registryCode )
        {
            if (setting == null || setting.FileItems == null || setting.FileItems.Count == 0)
            {
                Logger.Instance.AdddLog(LogType.Error, "Missing values in setting!", "DllRegister", "");
                return false;
            }
            int regCount = 0;
            bool error = false;
            bool IsCopied = false;

            if (!TestSettings(setting))
            {
                Logger.Instance.AdddLog(LogType.Error, "Project or file name or no write access? ", "DllRegister", "");
            }
            
            //set previous result path
            string previousPath = setting.OutputPath + "Backup\\" + timeId + "\\";
            if (!System.IO.Directory.Exists(previousPath))
            {
                System.IO.Directory.CreateDirectory(previousPath);
            }
            else
            {
                IsCopied = true;
            }

            //group by!
            setting.FileItems = setting.FileItems.GroupBy(x => new { x.FullPath, x.Name, x.RegExe }).Select(g => g.First()).ToList();
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
                if (string.IsNullOrWhiteSpace(FileItem.RegExe) || !System.IO.File.Exists(FileItem.RegExe))
                {
                    Logger.Instance.AdddLog(LogType.Error, FileItem.RegExe + " -> File not exist! " + FileItem.FullPath);
                    error = true;
                    continue;
                }

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
                            if (!UnRegister(FileItem.FullPath, FileItem.RegExe, setting.OutputPath, timeId, setting.InstallInGAC))
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
                            tlbPath = setting.OutputPath;
                            Logger.Instance.AdddLog(LogType.Error, "Set tlb output to : " + tlbPath, "DllRegister", "");
                        }
                        tlbPath += fileName + ".tlb";


                        #endregion

                        #region Backup?                                       



                        //copy reg
                        if (!IsCopied && setting.BuildRegistryKey && System.IO.File.Exists(setting.OutputPath + fileName + ".reg"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".reg")) System.IO.File.Delete(previousPath + fileName + ".reg");
                            System.IO.File.Move(setting.OutputPath + fileName + ".reg", previousPath + fileName + ".reg");
                        }
                        //copy tlb
                        if (!IsCopied && !setting.BuildRegistryKey && System.IO.File.Exists(setting.OutputPath + fileName + ".tlb"))
                        {
                            if (System.IO.File.Exists(previousPath + "bak_" + fileName + ".tlb")) System.IO.File.Delete(previousPath + "bak_" + fileName + ".tlb");
                            System.IO.File.Move(setting.OutputPath + fileName + ".tlb", previousPath + "bak_" + fileName + ".tlb");
                        }

                        if (!IsCopied && System.IO.File.Exists(path + fileName + ".tlb"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".tlb")) System.IO.File.Delete(previousPath + fileName + ".tlb");
                            System.IO.File.Move(path + fileName + ".tlb", previousPath + fileName + ".tlb");
                        }
                        //copy dll
                        if (!IsCopied && !setting.BuildRegistryKey && System.IO.File.Exists(setting.OutputPath + fileName + ".dll"))
                        {
                            if (System.IO.File.Exists(previousPath + "bak_" + fileName + ".dll")) System.IO.File.Delete(previousPath + "bak_" + fileName + ".dll");
                            System.IO.File.Move(setting.OutputPath + fileName + ".dll", previousPath + "bak_" + fileName + ".dll");
                        }

                        if (!IsCopied && !setting.BuildRegistryKey && System.IO.File.Exists(path + fileName + ".dll"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".dll") && path != previousPath) System.IO.File.Delete(previousPath + fileName + ".dll");
                            System.IO.File.Copy(path + fileName + ".dll", previousPath + fileName + ".dll");
                        }
                        //copy log
                        if (!IsCopied && System.IO.File.Exists(setting.OutputPath + fileName + ".log"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".log")) System.IO.File.Delete(previousPath + fileName + ".log");
                            System.IO.File.Move(setting.OutputPath + fileName + ".log", previousPath + fileName + ".log");
                        }
                        if (!IsCopied && System.IO.File.Exists(setting.OutputPath + fileName + ".logReg"))
                        {
                            if (System.IO.File.Exists(previousPath + fileName + ".logReg")) System.IO.File.Delete(previousPath + fileName + ".logReg");
                            System.IO.File.Move(setting.OutputPath + fileName + ".logReg", previousPath + fileName + ".logReg");
                        }
                        #endregion


                        string fnameTlb = " /tlb:\"" + tlbPath + "\"";
                        string regFile = " /regfile:\"" + setting.OutputPath + fileName + ".reg\"";
                        FileItem.RegBuildPath = setting.OutputPath + fileName + ".reg";
                        string dll = " \"" + FileItem.FullPath + "\"";

                        string argumentsReg = dll + regFile + (setting.Codebase ? " /codebase" : "") + " /verbose";
                        string arguments = dll + fnameTlb + (setting.Codebase ? " /codebase" : "") + " /verbose";

                        #region Build RegFile
                        if (System.IO.File.Exists(setting.OutputPath + fileName + ".reg")) System.IO.File.Delete(setting.OutputPath + fileName + ".reg");
                        if (!ExecProcess(argumentsReg, FileItem.RegExe, path) || !System.IO.File.Exists(setting.OutputPath + fileName + ".reg"))
                        {
                            Logger.Instance.AdddLog(LogType.Error, "Could not create registry data ! --> exit (" + FileItem.FullPath + ")");
                            error = true;
                            continue;
                        }
                        //read reg code from file
                        tmpRegistryCode = System.IO.File.ReadAllText(setting.OutputPath + fileName + ".reg");
                        #endregion

                        #region Register Dll
                        if (!setting.BuildRegistryKey)
                        {
                            //delete old result
                            if (System.IO.File.Exists(tlbPath)) System.IO.File.Delete(tlbPath);
                            if (!ExecProcess(arguments, FileItem.RegExe, path) || !System.IO.File.Exists(tlbPath))
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
                                    if (System.IO.File.Exists(setting.OutputPath + fileName + ".dll")) System.IO.File.Delete(setting.OutputPath + fileName + ".dll");
                                    System.IO.File.Copy(path + fileName + ".dll", setting.OutputPath + fileName + ".dll");
                                }
                                if (System.IO.File.Exists(path + fileName + ".tlb"))
                                {
                                    if (System.IO.File.Exists(setting.OutputPath + fileName + ".tlb")) System.IO.File.Delete(setting.OutputPath + fileName + ".tlb");
                                    System.IO.File.Copy(path + fileName + ".tlb", setting.OutputPath + fileName + ".tlb");
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
            if (regCount > 1 && !string.IsNullOrWhiteSpace(registryCode) && !string.IsNullOrWhiteSpace(setting.OutputPath) && System.IO.Directory.Exists(setting.OutputPath))
            {
                System.IO.File.WriteAllText(DllRegister.LogFilePath + "Reg", registryCode);
            }
            #endregion
            return !error;
        }

        public static bool UnRegister(Settings setting, string timeId)
        {
            if (setting == null || setting.FileItems == null) throw new Exception("Missing values in Setting!");
            bool error = false;


            if (!TestSettings(setting))
            {
                Logger.Instance.AdddLog(LogType.Error, "Project or file name or no write access? ", "DllRegister", "");
            }


            //group by!
            setting.FileItems = setting.FileItems.GroupBy(x => new { x.FullPath, x.Name, x.RegExe }).Select(g => g.First()).ToList();

            foreach (var FileItem in setting.FileItems)
            {
                if (setting.FileItems.Count > 1)
                {
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", "DllRegister");
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", "DllRegister");
                }

                #region file item test                
                //if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;

                if (string.IsNullOrWhiteSpace(FileItem.RegExe) || !System.IO.File.Exists(FileItem.RegExe))
                {
                    Logger.Instance.AdddLog(LogType.Error, FileItem.RegExe + " -> File not exist! " + FileItem.FullPath);
                    continue;
                }

                if (!System.IO.File.Exists(FileItem.FullPath))
                {
                    Logger.Instance.AdddLog(LogType.Error, "File not exist! " + FileItem.FullPath);
                }
                #endregion

                //UnRegister
                if (!UnRegister(FileItem.FullPath, FileItem.RegExe, setting.OutputPath, timeId, setting.InstallInGAC))
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

                        if (!System.IO.Directory.Exists(outpath)) Logger.Instance.AdddLog(LogType.Error, "Backup path?: " + outpath, "DllRegister", "");                        
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




        #region IsRegistered
        public static bool IsRegistered(out bool bit64, out bool gac, string path, bool correct,string outPath, string time)
        {
            gac = false;
            bit64 = false;
            int error = 0;
            bool diff64 = false;
            bool diff32 = false;

            gac = TestIsInGAG(path, out string _);

            #region Direct loadable?
            if (DllRegister.IsComDllLoadable(path))
            {
                Logger.Instance.AdddLog(LogType.Info, "The DLL is direct loadable over path: " + path, "DllRegister");                
            }
            else
            {
                Logger.Instance.AdddLog(LogType.Error, "The DLL is not direct loadable over path! (" + path +")", "DllRegister");                
                error++;
            }
            #endregion

            #region main 32bit & 64bit            
            List<RegData> regData = SearchRegistryNative(System.IO.Path.GetFileName(path), "CLSID", true);
            if (regData == null|| regData.Count == 0 )
            {                
                Logger.Instance.AdddLog(LogType.Warn, @"(CLASSES_ROOT\CLSID) Not fond! ->" + System.IO.Path.GetFileName(path), "DllRegister");
            }
            else
            {
                foreach (RegData item in regData)
                {
                    string tempCLSID = string.Empty;

                    #region differ in path
                    if (!string.IsNullOrWhiteSpace(item.Path) && path.ToUpper() != item.Path.ToUpper())
                    {

                        Logger.Instance.AdddLog(LogType.Error, "The DLL path found in the registry differs from the new path to DLL (CLSID)", "DllRegister");
                        Logger.Instance.AdddLog(LogType.Error, "DLL path: " + path, "DllRegister");
                        Logger.Instance.AdddLog(LogType.Error, "New path: " + item.Path, "DllRegister");

                        if (correct &&
                            MessageBox.Show("The DLL path found in the registry differs from the new path to DLL. Should this path be corrected?" + Environment.NewLine + item.ToString(), "Should this path be corrected", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {                            
                            UnRegister(item.Path, RegasmPath, outPath, time);
                            CleanReg(item.CLSID, item.Path);
                            return true;
                        }
                    } 
                    #endregion

                    #region differ in CLSID
                    if (!string.IsNullOrWhiteSpace(regData.ToString()) && string.IsNullOrWhiteSpace(tempCLSID)) tempCLSID = regData.ToString();
                    if (!string.IsNullOrWhiteSpace(tempCLSID) &&
                        !string.IsNullOrWhiteSpace(regData.ToString()) &&
                        tempCLSID != regData.ToString())                        
                    {                        
                        Logger.Instance.AdddLog(LogType.Error, "Found different CLSIDs in registry! (CLSID)", "DllRegister");
                        diff32 = true;
                    } 
                    #endregion
                } 
            }
            #endregion

            #region 64bit
            List<RegData> regData64 = SearchRegistryNative(System.IO.Path.GetFileName(path), "Wow6432Node\\CLSID",true);
            
            if (regData64 == null || regData64.Count == 0)
            {                
                Logger.Instance.AdddLog(LogType.Warn, @"(CLASSES_ROOT\Wow6432Node\CLSID) Not fond! ->" + System.IO.Path.GetFileName(path), "DllRegister");
            }
            else
            {
                foreach (RegData item64 in regData64)
                {
                    bit64 = true;
                    string tempCLSID = string.Empty;

                    #region differ in path
                    if (!string.IsNullOrWhiteSpace(item64.Path) && path.ToUpper() != item64.Path.ToUpper())
                    {
                        Logger.Instance.AdddLog(LogType.Error, "The DLL path found in the registry differs from the new path to DLL (Wow6432Node)", "DllRegister");
                        Logger.Instance.AdddLog(LogType.Error, "DLL path: " + path, "DllRegister");
                        Logger.Instance.AdddLog(LogType.Error, "New path: " + item64.Path, "DllRegister");


                        if (correct &&
                            MessageBox.Show("The DLL path found in the registry (Wow6432Node) differs from the new path to DLL. Should this path be corrected?" + Environment.NewLine + item64.ToString(), "Should this path be corrected", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {                            
                            UnRegister(item64.Path, RegasmPath, outPath, time);
                            CleanReg(item64.CLSID, item64.Path);
                            return true;
                        }
                    } 
                    #endregion

                    #region differ in CLSID
                    if (!string.IsNullOrWhiteSpace(regData64.ToString()) && string.IsNullOrWhiteSpace(tempCLSID)) tempCLSID = regData64.ToString();
                    if (!string.IsNullOrWhiteSpace(tempCLSID) &&
                        !string.IsNullOrWhiteSpace(regData64.ToString()) &&
                         tempCLSID != regData64.ToString())
                    {                        
                        Logger.Instance.AdddLog(LogType.Error, "Found different CLSIDs in registry! (Wow6432Node)", "DllRegister");
                        diff32 = true;
                    } 
                    #endregion
                }
            }
            #endregion


            if (regData != null && regData.Count > 0)
            {
                foreach (var regItem in regData)
                {
                    if (!string.IsNullOrWhiteSpace(regItem.Class))
                    {                        
                        Logger.Instance.AdddLog(LogType.Info, "Search in root for: " + regItem.Class + " ->", "DllRegister");
                        string Id = GetCLSIDFromClassName(regItem.Class, regItem.CLSID, out int tempError);
                        error += tempError;
                        if (Id != string.Empty)
                        {
                            if (Id == regItem.CLSID)
                            {                                
                                Logger.Instance.AdddLog(LogType.Info, "Found!", "DllRegister");
                            }
                            else
                            {
                                
                                Logger.Instance.AdddLog(LogType.Error,"Found class name in root but CLSID is different !" + 
                                    Environment.NewLine +
                                    "CLSID    : " + regItem.CLSID + Environment.NewLine +
                                    "new CLSID: " + Id, "DllRegister");

                                string className = regItem.Class;
                                error++;

                                #region 32bit

                                RegData newregData;
                                RegistryKey t_clsidSubKey = Registry.ClassesRoot.OpenSubKey("CLSID" + "\\{" + Id + "}\\InProcServer32");
                                if (!string.IsNullOrEmpty(className) && t_clsidSubKey != null)
                                {

                                    StringBuilder message = new StringBuilder();
                                    message.AppendLine("--------------------------------------------------------------------------------------------------------");
                                    message.AppendLine("CLSID" + "\\{" + Id + "}\\InProcServer32");
                                    if (SearchValueRegistryNative(Id, t_clsidSubKey.Name.Replace("HKEY_CLASSES_ROOT\\", ""), "", out newregData, className))
                                    {
                                        message.AppendLine("Found ->" + Environment.NewLine + "Registry:       " + t_clsidSubKey.Name + Environment.NewLine + newregData.GetInfo + Environment.NewLine + "(Keys: " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")");

                                        if (!System.IO.File.Exists(newregData.Path)) message.AppendLine("File not found Path from RegKey-> Path: " + newregData.Path);
                                    }
                                    message.AppendLine("--------------------------------------------------------------------------------------------------------");
                                    Logger.Instance.AdddLog(LogType.Info, message.ToString(), "DllRegister");
                                }
                                #endregion
                                #region 64bit
                                t_clsidSubKey = Registry.ClassesRoot.OpenSubKey("Wow6432Node\\CLSID" + "\\{" + Id + "}\\InProcServer32");
                                if (!string.IsNullOrEmpty(className) && t_clsidSubKey != null)
                                {
                                    StringBuilder message = new StringBuilder();
                                    message.AppendLine("--------------------------------------------------------------------------------------------------------");
                                    message.AppendLine("Wow6432Node\\CLSID" + "\\{" + Id + "}\\InProcServer32");
                                    if (SearchValueRegistryNative(Id, t_clsidSubKey.Name.Replace("HKEY_CLASSES_ROOT\\", ""), "", out newregData, className))
                                    {
                                        message.AppendLine("Found ->" + Environment.NewLine + "Registry:       " + t_clsidSubKey.Name + Environment.NewLine + newregData.GetInfo + Environment.NewLine + "(Keys: " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")");

                                        if (!System.IO.File.Exists(newregData.Path)) message.AppendLine("File not found Path from RegKey-> Path: " + newregData.Path);
                                    }
                                    message.AppendLine("--------------------------------------------------------------------------------------------------------");
                                    Logger.Instance.AdddLog(LogType.Info, message.ToString(), "DllRegister");
                                }
                                #endregion
                            }
                        }
                    }
                }                                
            }
            else
            {
                Logger.Instance.AdddLog(LogType.Error, "No CLSID fond in registry!(CLSID is empty from Dll name)" + System.IO.Path.GetFileName(path), "DllRegister");                                
                if (error > 0) Logger.Instance.AdddLog(LogType.Error, "Check the log file (" + error.ToString() + " errors)!", "DllRegister");
                return false;
            }



            if (regData != null && regData64 != null && regData.Count > 0 && regData64.Count > 0 && diff32 == false && diff64 == false)
            {

                if (!string.IsNullOrEmpty(regData[0].CLSID) &&
                    !string.IsNullOrEmpty(regData64[0].CLSID) &&
                    regData[0].CLSID != regData64[0].CLSID)
                {
                    Logger.Instance.AdddLog(LogType.Error, "ERROR| The CLSID is different from Wow6432Node:", "DllRegister");
                    Logger.Instance.AdddLog(LogType.Error, "in CLSID      : " + regData[0].CLSID, "DllRegister");
                    Logger.Instance.AdddLog(LogType.Error, "in Wow6432Node: " + regData64[0].CLSID, "DllRegister");
                    error++;
                }

                if (!string.IsNullOrEmpty(regData[0].Path) && !string.IsNullOrEmpty(regData64[0].Path) &&
                     regData[0].Path.ToLower() != regData64[0].Path.ToLower())
                {
                    Logger.Instance.AdddLog(LogType.Error, "ERROR| The CLSID path is different from Wow6432Node path:", "DllRegister");
                    Logger.Instance.AdddLog(LogType.Error, "in CLSID      : " + regData[0].Path, "DllRegister");
                    Logger.Instance.AdddLog(LogType.Error, "in Wow6432Node: " + regData64[0].Path, "DllRegister");
                    error++;
                }
            }

            if (error > 0)Logger.Instance.AdddLog(LogType.Error, "ERROR| Check the log file (" + error.ToString() + " errors)!", "DllRegister");            
            return true;
        }

        private static bool TestIsInGAG(string path,out string newPath )
        {
            bool gac = false;
            newPath = string.Empty;
            if (SystemUtil.IsAssemblyInGAC(System.IO.Path.GetFileNameWithoutExtension(path)))
            {                
                Logger.Instance.AdddLog(LogType.Info, "(GAC) The DLL is registered in GAC (Fusion): " + System.IO.Path.GetFileName(path), "DllRegister");
                if (!gac) gac = true;
            }

            if (FindInGAC_Reg(System.IO.Path.GetFileNameWithoutExtension(path), out string value))
            {                
                Logger.Instance.AdddLog(LogType.Info, "(GAC) The DLL is registered in GAC (Registry): " + value, "DllRegister");
                if (!gac) gac = true;
            }
            if (DllRegister.IsAssemblyInGAC(path, out string xpath))
            {
                newPath = xpath;                
                Logger.Instance.AdddLog(LogType.Info, "(GAC) The DLL is registered in GAC (Assembly): " + xpath, "DllRegister");
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

        ///// <summary>
        ///// Search and Find Registry Function
        ///// </summary>
        ///// <param name="dllName"></param>
        ///// <param name="CLSID"></param>
        ///// <param name="path">filepath</param>
        ///// <param name="root">Start point in reg</param>
        ///// <returns></returns>
        //public static string SearchRegistryNative(string dllName, string root, out RegData regData)
        //{
        //    regData = null;
        //    if (string.IsNullOrWhiteSpace(dllName) || !dllName.ToLower().EndsWith(".dll")) return string.Empty;
        //    SearchInValues = 0;
        //    SearchKeys = 0;

        //    //Registry.ClassesRoot.DeleteSubKeyTree("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}");
        //    //Registry.ClassesRoot.CreateSubKey("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}\\InProcServer32");

        //    //RegistryKey t_ = Registry.ClassesRoot.OpenSubKey("CLSID\\{88ABA9D3-5980-4A42-871E-fAB235F8A7CD}\\InProcServer32");
        //    //Registry.SetValue(t_.Name,"", @"E:\Nextcloud\Projekt\C#\Register DLL\DLL Temlate\BluefrogLibrary\bin\Releas\BluefrogLibary.dll", RegistryValueKind.String);
        //    //RegistryKey t_test = Registry.ClassesRoot.OpenSubKey("CLSID\\{7875D9D8-1B94-4AE4-A8F0-63B3F8609336}", System.Security.AccessControl.RegistryRights.FullControl);

        //    //Open the HKEY_CLASSES_ROOT\CLSID which contains the list of all registered COM files (.ocx,.dll, .ax) 
        //    //on the system no matters if is 32 or 64 bits.
        //    RegistryKey t_clsidKey = Registry.ClassesRoot.OpenSubKey(root);
            
           

        //    //Get all the sub keys it contains, wich are the generated GUID of each COM.            
        //    foreach (string subKeyName in t_clsidKey.GetSubKeyNames())
        //    {
        //        //For each CLSID\GUID key we get the InProcServer32 sub-key .
        //        string regPath = root + "\\" + subKeyName + "\\InProcServer32";
        //        if (SearchValueRegistryNative(subKeyName, regPath, dllName, out regData))
        //        {                         
        //            Logger.Instance.AdddLog(LogType.Info, "Found ->" + Environment.NewLine + "Registry:       " + regPath + Environment.NewLine +  regData.GetInfo + Environment.NewLine + "(Keys: "  + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")" + Environment.NewLine , "DllRegister", "");

        //            if (!System.IO.File.Exists(regData.Path)) Logger.Instance.AdddLog(LogType.Error, "File not found Path from RegKey-> (" + root + "\\" + regData.CLSID + ") Path: " + regData.Path, "DllRegister", "");
        //            return regPath;
        //        }
        //    }

        //    //if not exist, return nothing
        //    Logger.Instance.AdddLog(LogType.Error, "Not found in Keys (" + root + "): " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString(), "DllRegister", "");
        //    return string.Empty;
        //}

        public static List<RegData> SearchRegistryNative(string dllName, string root, bool allKeys = false)
        {
            List<RegData> regData = null;
            if (string.IsNullOrWhiteSpace(dllName) || !dllName.ToLower().EndsWith(".dll")) return regData;
            
            SearchInValues = 0;
            SearchKeys = 0;
            int count = 0;
            //Open the HKEY_CLASSES_ROOT\CLSID which contains the list of all registered COM files (.ocx,.dll, .ax) 
            //on the system no matters if is 32 or 64 bits.
            RegistryKey t_clsidKey = Registry.ClassesRoot.OpenSubKey(root);



            //Get all the sub keys it contains, wich are the generated GUID of each COM.            
            foreach (string subKeyName in t_clsidKey.GetSubKeyNames())
            {
                //For each CLSID\GUID key we get the InProcServer32 sub-key .
                string regPath = root + "\\" + subKeyName + "\\InProcServer32";
                 
                if (SearchValueRegistryNative(subKeyName, regPath, dllName, out RegData newValue))
                {
                    count++;
                    if (regData == null) regData = new List<RegData>();                    
                    Logger.Instance.AdddLog(LogType.Info, "(" + root + ")" +  " Found ->Registry: " + regPath + Environment.NewLine + newValue.GetInfo + Environment.NewLine + "(Keys: " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString() + ")" + Environment.NewLine, "DllRegister", "");                                        
                    if (!System.IO.File.Exists(newValue.Path)) Logger.Instance.AdddLog(LogType.Error, "File not found! path from RegKey-> (" + root + "\\" + newValue.CLSID + ") Path: " + newValue.Path, "DllRegister", "");                    
                    regData.Add(newValue);                    
                    if (!allKeys) return regData;
                }
            }            
            if(count==0)
                Logger.Instance.AdddLog(LogType.Error, "(" + root + ")" + " Not fond! ->Keys: " + SearchKeys.ToString() + ", Values: " + SearchInValues.ToString(), "DllRegister", "");
            return regData;
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
                if (foundDllName)
                {
                    regData.RegistryPath = regPath;
                    return true;
                }
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
            try
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(path);                
                if (TestIsInGAG(path, out string newpath))
                {
                    ok = true;
                    if (!string.IsNullOrWhiteSpace(newpath) && System.IO.File.Exists(newpath))
                    {                        
                        Logger.Instance.AdddLog(LogType.Info, "Register in GAC->Ok!" + newpath, "DllRegister", "");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.AdddLog(LogType.Error, "Register in GAC?", "DllRegister", "", ex);
            }            
            return ok;
        }
        public static bool UnregisterInGAC(string path)

        {
            bool ok = false;      
            try
            {
                if (System.IO.File.Exists(path))
                {                    
                    if (TestIsInGAG(path, out string newpath))
                    {
                        if (!string.IsNullOrWhiteSpace(newpath) && System.IO.File.Exists(newpath))
                        {
                            new System.EnterpriseServices.Internal.Publish().GacRemove(newpath);
                            if (!System.IO.File.Exists(newpath))
                            {
                                Logger.Instance.AdddLog(LogType.Info, "Unregister in GAC->Ok!" + newpath, "DllRegister", "");                                
                                ok = true;                                
                            }
                        }
                        else
                        {
                            Logger.Instance.AdddLog(LogType.Warn, "Unregister in GAC (path not fount over assenbly!) -> " + path, "");                            
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
            Assembly ass1;
            Assembly ass2;
            bool result = false;
            try
            {
                ass1 = Assembly.LoadFrom(assemblyFullName);
                if (ass1 == null) return false;
                ass2 = Assembly.Load(ass1.GetName());
                if (ass2 == null) { ass1 = null; return false; }                
                result = ass2.GlobalAssemblyCache;                                
                if (!result) { ass1 = null; ass2 = null; return false; }
                
                if (System.IO.File.Exists(ass2.Location)) 
                    path = ass2.Location;
                else 
                    result = false;
            }
            catch {}

            ass1 = null; ass2 = null;
            return false;
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
