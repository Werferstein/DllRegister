/*
Ingolf Hill
werferstein.org
/*
  This program is free software. It comes without any warranty, to
  the extent permitted by applicable law. You can redistribute it
  and/or modify it under the terms of the Do What The Fuck You Want
  To Public License, Version 2, as published by Sam Hocevar.
*/


using Helper.Dll;
using Helper.Uac;
using Microsoft.Win32;
using SLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace DllRegister
{
    public partial class MainForm : Form
    {
        //[DllImport("E:\\Nextcloud\\Projekt\\C#\\Register DLL\\DLL Temlate\\BluefrogLibrary\\bin\\Release\\BluefrogLibrary.dll", EntryPoint = "#8002")]
        //public static extern void OpenTestForm(string msg);
        //private TextSource logBuffer;
        private string logBufferText;
        private bool blockGui = false;
        private string lastPath = string.Empty;
        private string timeId = DateTime.Now.ToString("ddMMyyHHmmssfff");
        private Settings Setting;




        //styles
        TextStyle nullStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        TextStyle infoStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        TextStyle warningStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Regular);
        TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);

        TextStyle BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        TextStyle BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        TextStyle GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        TextStyle MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        TextStyle GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        TextStyle BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
        TextStyle MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));


        /*
        Path structure:

            [Project name]-* Backup DLL/TLB/reg files
                           *-------------------------[Date and time as file name]-*Backup from last build

 
         */



        public MainForm()
        {
            InitializeComponent();
            
            splitContainer1.SplitterDistance = FileListcomboBox.Location.Y + 30;
            #region int Logger
            if (!Logger.LoggerIsOnline)
            {
                Logger.Instance.Stop();
                Logger.Instance.Start( Output.ADD_InnerException | Output.ADD_StackTrace);
            }
            Logger.Instance.OnLogMessage += Instance_OnLogMessage; 
            #endregion

            Setting = Settings.Load("*.regdll");
            if (!String.IsNullOrWhiteSpace(Settings.LoadedFrom)) Setting.MainOptions.OptionPath = Settings.LoadedFrom;
            if (Setting == null) Setting = new Settings();
            DllRegister.TestSettings(Setting);

            Logger.Instance.AdddLog(LogType.Info, Assembly.GetEntryAssembly().FullName + " Start!", this);
            if (Environment.Is64BitProcess)
            {
                Logger.Instance.AdddLog(LogType.Info, "This is a 64 bit proccess");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetValuesToForm();
        }


        private void SetValuesToForm(bool viewLog = true)
        {
            blockGui = true;
            if (SystemUtil.InternalCheckIsWow64() && !Environment.Is64BitProcess && MessageBox.Show("With a 32bit application no registry entries can be read which refer to 64bit DLLs. Please use a 64bit version of the program." + Environment.NewLine + "Close application?", "The system is 64bit but the running application is 32bit", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
            {
                Application.Exit();
                this.Close();
                return;
            }



            if(viewLog) GetInfo();
            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = "";

            #region regasm.exe
            // This is the location of the .Net Framework Registry Key
            string framworkRegPath = @"Software\Microsoft\.NetFramework";
            // Get a non-writable key from the registry
            RegistryKey netFramework = Registry.LocalMachine.OpenSubKey(framworkRegPath, false);
            // Retrieve the install root path for the framework
            string framework = netFramework.GetValue("InstallRoot").ToString();
            if (string.IsNullOrEmpty(framework) || !System.IO.Directory.Exists(framework))
            {
                framework = Environment.GetEnvironmentVariable("SystemRoot") + "\\Microsoft.NET\\";
                if (string.IsNullOrEmpty(framework) || !System.IO.Directory.Exists(framework))
                {
                    MessageBox.Show("Can`t find regasm.exe?", "EXIT", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
            }
            if (!framework.EndsWith("\\")) framework += "\\";
            if (framework.ToLower().EndsWith("framework\\")) framework = framework.Replace("Framework\\", "");
            if (framework.ToLower().EndsWith("framework64\\")) framework = framework.Replace("Framework64\\", "");

            #region fill combo box regasm.exe
            comboBoxNetLink.Items.Clear();
            comboBoxNetLink.Text = string.Empty;
            if (viewLog) Logger.Instance.AdddLog(LogType.Info, "Search in: " + framework + " for regasm.exe!", this);
            string[] files = Directory.GetFiles(framework, "regasm.exe", SearchOption.AllDirectories);
            if (viewLog) Logger.Instance.AdddLog(LogType.Info, "Found " + files.Length.ToString() + " regasm.exe files.", this);

            if (files != null && files.Length > 0)
            {
                var xfiles = files.GroupBy(x => x).Select(y => y.First()).ToList();
                foreach (string item in xfiles)
                {
                    NetItem y = new NetItem()
                    {
                        FullPath = item
                    };

                    comboBoxNetLink.Items.Add(y);

                    if (Environment.Is64BitProcess && item.Contains("64"))
                    {
                        comboBoxNetLink.Text = y.Name;
                    }
                    else
                    {
                        comboBoxNetLink.Text = y.Name;
                    }
                }
            }
            #endregion

            #endregion

            LoadDllFileItems();

            checkBoxInstallinGAC.Checked = Setting.InstallInGAC;
            checkBoxRegistry.Checked = Setting.BuildRegistryKey;
            checkBoxCodebase.Checked = Setting.Codebase;
            textBoxOutPath.Text = Setting.OutputPath;
            saveInstallLogToolStripMenuItem.Checked = Setting.SaveLogging;
            textBoxPName.Text = Setting.ProjectName;

            blockGui = false;
        }




        private void LoadDllFileItems( )
        {
            #region fill combo box DLLs            
            FileListcomboBox.Items.Clear();
            FileListcomboBox.Text = string.Empty;

            if (Setting.FileItems != null && Setting.FileItems.Count > 0)
            {
                Setting.FileItems = Setting.FileItems.GroupBy(x => new { x.FullPath, x.Name, x.RegExe }).Select(g => g.First()).ToList();
 

                foreach (var item in Setting.FileItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.FullPath))
                    {
                        FileListcomboBox.Items.Add(item);
                        FileListcomboBox.SelectedItem = item;
                        if (!string.IsNullOrWhiteSpace(item.RegExe))
                        {
                            SetRegExe(item.RegExe);
                            comboBoxNetLink.BackColor = SystemColors.Control;
                        }
                        else
                        {
                            comboBoxNetLink.Text = string.Empty;
                            comboBoxNetLink.BackColor = Color.Red;
                        }
                        lastPath = item.FullPath;
                    }
                }
                buttonAdd.Text = "ADD (" + FileListcomboBox.Items.Count.ToString() + ")";
            }
            #endregion
        }

        /// <summary>
        /// Select item in comboBoxNetLink
        /// </summary>
        /// <param name="regExePath"></param>
        private void SetRegExe (string regExePath)
        {
            if (!string.IsNullOrWhiteSpace(regExePath) && System.IO.File.Exists(regExePath))
            {
                bool found = false;
                blockGui = true;
                foreach (var item in comboBoxNetLink.Items)
                {
                    if ((item as NetItem).FullPath.ToLower() == regExePath.ToLower())
                    {                        
                        comboBoxNetLink.SelectedItem = item;
                        comboBoxNetLink.Text = (item as NetItem).Name;
                        found = true;                        
                        break;
                    }
                }
                if (!found)
                {
                    NetItem ni = new NetItem() { FullPath = regExePath };
                    comboBoxNetLink.Items.Add(ni);
                    comboBoxNetLink.SelectedItem = ni;
                    comboBoxNetLink.Text = ni.FullPath;
                }
                blockGui = false;
            }
        }


        #region AddLog
        private void Instance_OnLogMessage(object sender, LogMessageInfoEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Instance_OnLogMessage(sender, e)));
            }
            else
            {
                lock (fct_box)
                {

                    //TextStyle style = nullStyle;
                    //switch (e.Level)
                    //{
                    //    case LogType.Info:
                    //        style = infoStyle;
                    //        break;
                    //    case LogType.Debug:
                    //        break;
                    //    case LogType.Warn:
                    //        style = warningStyle;
                    //        break;
                    //    case LogType.Error:
                    //        style = errorStyle;
                    //        break;
                    //    case LogType.Off:
                    //    default:
                    //        break;
                    //}
                    ////some stuffs for best performance
                    //fct_box.BeginUpdate();
                    //fct_box.Selection.BeginUpdate();
                    ////remember user selection
                    //var userSelection = fct_box.Selection.Clone();
                    ////add text with predefined style
                    //fct_box.TextSource.CurrentTB = fct_box;
                    //fct_box.AppendText(e.LogMessage + Environment.NewLine, style);
                    fct_box.AppendText(e.LogMessage + Environment.NewLine);
                ////restore user selection
                //if (!userSelection.IsEmpty || userSelection.Start.iLine < fct_box.LinesCount - 2)
                //{
                //    fct_box.Selection.Start = userSelection.Start;
                //    fct_box.Selection.End = userSelection.End;
                //}
                //else
                //    fct_box.GoEnd();//scroll to end of the text

                //fct_box.Update();
                //fct_box.BringToFront();
                #region save log
                //if (!string.IsNullOrWhiteSpace(fct_box.Text) && !string.IsNullOrWhiteSpace(Setting.OutputPath) && System.IO.Directory.Exists(Setting.OutputPath))
                //{
                //    System.IO.File.WriteAllText(DllRegister.LogFilePath, fct_box.Text);
                //}
                #endregion
                Application.DoEvents();
                }
            }
        }
        #endregion

        #region Register
        private void ButtonRegister_Click(object sender, EventArgs e)
        {
            if ((checkBoxInstallinGAC.Checked || !checkBoxRegistry.Checked) && !UacHelper.IsRunAsAdmin())
            {
                MessageBox.Show("You need administrator rights to register a DLL!", "Administrator ?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = string.Empty;
            fct_box.Text = string.Empty;
            SetToViewRegFile();

            Logger.Instance.AdddLog(LogType.Debug, "Start to register DLLs!", this);

            GetUiInput();

            #region project name
            //test project name --set to default?
            if (string.IsNullOrWhiteSpace(Setting.ProjectName) &&
                FileListcomboBox.SelectedItem != null &&
                !string.IsNullOrWhiteSpace((FileListcomboBox.SelectedItem as FileItem).FullPath))
            {
                textBoxPName.Text = "Register_" + System.IO.Path.GetFileNameWithoutExtension((FileListcomboBox.SelectedItem as FileItem).FullPath);
            }
            else
            {
                textBoxPName.Text = "Register_DLL";
            }
            Setting.ProjectName = textBoxPName.Text;
            #endregion

            #region Regasm.exe & file links ?
            if (Setting.FileItems == null || Setting.FileItems.Count == 0)
            {
                Logger.Instance.AdddLog(LogType.Error, "Missing DLL file link ?", this);
                MessageBox.Show("Please select DLL files (add button)!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (comboBoxNetLink.SelectedItem == null || string.IsNullOrWhiteSpace((comboBoxNetLink.SelectedItem as NetItem).FullPath) || !System.IO.File.Exists((comboBoxNetLink.SelectedItem as NetItem).FullPath))
            {
                Logger.Instance.AdddLog(LogType.Error, "Framework link ?", this);
                MessageBox.Show("Regasm.exe link is not set?", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #endregion

            Cursor.Current = Cursors.WaitCursor;


            #region Register DLL
            if (DllRegister.Register(Setting, timeId))
            {
                labelResult.Text = "OK!";
                labelResult.BackColor = Color.ForestGreen;
            }
            else
            {
                labelResult.Text = "ERROR";
                labelResult.BackColor = Color.Red;
            }

            #endregion

            labelResult.BorderStyle = BorderStyle.FixedSingle;
            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region Unregister
        private void ButtonUnRegister_Click(object sender, EventArgs e)
        {
            if (!UacHelper.IsRunAsAdmin())
            {
                MessageBox.Show("You need administrator rights to unregister a DLL!", "Administrator ?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = string.Empty;
            fct_box.Text = string.Empty;
            SetToViewRegFile();

            Logger.Instance.AdddLog(LogType.Debug, "Start to unregister DLLs!", this);
            GetUiInput();

            if (comboBoxNetLink.SelectedItem == null || string.IsNullOrWhiteSpace((comboBoxNetLink.SelectedItem as NetItem).FullPath) || !System.IO.File.Exists((comboBoxNetLink.SelectedItem as NetItem).FullPath))
            {
                Logger.Instance.AdddLog(LogType.Error, "Framework link ?", this);
                return;
            }
            if (Setting.FileItems == null || Setting.FileItems.Count == 0)
            {
                Logger.Instance.AdddLog(LogType.Error, "Missing DLL file link ?", this);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            labelResult.BorderStyle = BorderStyle.FixedSingle;
            if (DllRegister.UnRegister(Setting, timeId))
            {
                labelResult.Text = "OK!";
                labelResult.BackColor = Color.ForestGreen;
            }

            else
            {
                labelResult.Text = "ERROR";
                labelResult.BackColor = Color.Red;                
            }

            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region UAC Info
        private void GetInfo()
        {
            string message = string.Empty;
            
            // Get and display whether the primary access token of the process belongs 
            // to user account that is a member of the local Administrators group even 
            // if it currently is not elevated (IsUserInAdminGroup).
            try
            {
                message += "Is user in admin group:" + UacHelper.IsUserInAdminGroup().ToString();
            }
            catch
            {
                message += "Is user in admin group:N/A";
            }

            // Get and display whether the process is run as administrator or not 
            // (IsRunAsAdmin).
            try
            {
                message += " |Run as admin:" + UacHelper.IsRunAsAdmin().ToString();
            }
            catch
            {
                message += " |Run as admin:N/A";
            }


            // Get and display the process elevation information (IsProcessElevated) 
            // and integrity level (GetProcessIntegrityLevel). The information is not 
            // available on operating systems prior to Windows Vista.
            if (Environment.OSVersion.Version.Major >= 6)
            {
                // Running Windows Vista or later (major version >= 6). 

                try
                {
                    message += " |Is process elevated:" + UacHelper.IsProcessElevated().ToString();
                }
                catch
                {
                    message += " |Is process elevated:N/A";
                }

                try
                {
                    // Get and display the process integrity level.
                    int IL = UacHelper.GetProcessIntegrityLevel();
                    message += " |Integrity level:"; 

                    switch (IL)
                    {
                        case NativeMethods.SECURITY_MANDATORY_UNTRUSTED_RID:
                            message += "Untrusted"; break;
                        case NativeMethods.SECURITY_MANDATORY_LOW_RID:
                            message += "Low"; break;
                        case NativeMethods.SECURITY_MANDATORY_MEDIUM_RID:
                            message += "Medium"; break;
                        case NativeMethods.SECURITY_MANDATORY_HIGH_RID:
                            message += "High"; break;
                        case NativeMethods.SECURITY_MANDATORY_SYSTEM_RID:
                            message += "System"; break;
                        default:
                            message += "Unknown"; break;
                    }
                }
                catch
                {
                    message += "|Integrity level:N/A";
                }
            }
            else
            {
                message += " |Is process elevated:N/A |Integrity level:N/A";                
            }
            Logger.Instance.AdddLog(LogType.Warn, message, this);            
        }
        #endregion

        #region Load & Save
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetUiInput();


            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            string name = "dll_register_job.regdll";
            if (!string.IsNullOrWhiteSpace(Setting.MainOptions.OptionPath) && System.IO.File.Exists(Setting.MainOptions.OptionPath))
            {
                path = System.IO.Path.GetDirectoryName(Setting.MainOptions.OptionPath);
                name = System.IO.Path.GetFileName(Setting.MainOptions.OptionPath);
            }




            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "dll register files (*.regdll)|*.REGDLL|All files (*.*)|*.*",
                InitialDirectory = path,
                FileName = name,
                Title = "Save a DLL register job!"
            };

            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                Setting.MainOptions.OptionPath = dialog.FileName;
                Setting.Save(dialog.FileName);
            }
        }

        private void GetUiInput()
        {
            Setting.InstallInGAC = checkBoxInstallinGAC.Checked;
            Setting.BuildRegistryKey = checkBoxRegistry.Checked;
            Setting.Codebase = checkBoxCodebase.Checked;
            Setting.OutputPath = textBoxOutPath.Text;
            Setting.SaveLogging = saveInstallLogToolStripMenuItem.Checked;
            Setting.ProjectName = textBoxPName.Text;
            if (string.IsNullOrWhiteSpace(Setting.ProjectName)) Setting.ProjectName = "Backup";
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            if (!string.IsNullOrWhiteSpace(Setting.MainOptions.OptionPath) && System.IO.File.Exists(Setting.MainOptions.OptionPath))
            {
                path = System.IO.Path.GetDirectoryName(Setting.MainOptions.OptionPath);
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "dll register files (*.regdll)|*.REGDLL|All files (*.*)|*.*",
                InitialDirectory = path,
                Title = "Please select a DLL register job!",
                Multiselect = false
            };

            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                Setting = Settings.Load(out string error, dialog.FileName);
                if (error == string.Empty) SetValuesToForm(false);
                Setting.MainOptions.OptionPath = dialog.FileName;
                timeId = DateTime.Now.ToString("ddMMyyHHmmssfff");
            }
        }
        public void SetFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

            if (!string.IsNullOrWhiteSpace(lastPath) && (System.IO.File.Exists(lastPath) || System.IO.Directory.Exists(lastPath)))
            {
                if (lastPath.ToLower().EndsWith("dll"))
                {
                    path = System.IO.Path.GetDirectoryName(lastPath);
                }
                else
                {
                    path = lastPath;
                }
            }
            else
            if (!string.IsNullOrWhiteSpace(FileListcomboBox.Text) && System.IO.File.Exists(FileListcomboBox.Text))
            {
                path = System.IO.Path.GetDirectoryName(FileListcomboBox.Text);
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "dll files (*.dll)|*.DLL|All files (*.*)|*.*",                
                InitialDirectory = path,
                Title = "Please select all dlls to register!",
                Multiselect = true
            };

            if (dialog.ShowDialog() == DialogResult.OK && dialog.FileNames.Length > 0)
            {
                //bool add = false;
                foreach (string item in dialog.FileNames)
                {
                    //    if (Setting.FileItems.FirstOrDefault(r => r.FullPath == item) == null)
                    //    {
                    FileItem fileItem = new FileItem()
                    {
                        FullPath = item,
                        Name = System.IO.Path.GetFileName(item),
                        RegExe = comboBoxNetLink.SelectedItem != null ? (comboBoxNetLink.SelectedItem as NetItem).FullPath : string.Empty
                    };
                    Setting.FileItems.Add(new FileItem() { FullPath = item, Name = System.IO.Path.GetFileName(item) });                    
                    lastPath = item;
                }
                    
                //}
                //if (add)
                //{
                    timeId = DateTime.Now.ToString("ddMMyyHHmmssfff");

                FileItem lastItem = (FileListcomboBox.SelectedItem as FileItem);
                LoadDllFileItems();
                if (lastItem != null)
                {
                    FileListcomboBox.SelectedItem = lastItem;
                }
            }
        }
        #endregion

        #region Menue & ETC
        private void Button2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FilelistContextMenuStrip.Show(this, new Point(e.X, e.Y));//places the menu at the pointer position
                return;
            }
        }
        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetFile();
        }

        private void RemoveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileListcomboBox.Items.Clear();
            Setting.FileItems.Clear();
            FileListcomboBox.Text = string.Empty;
            buttonAdd.Text = "ADD";            
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileListcomboBox.SelectedItem != null)
            {
                int index = FileListcomboBox.Items.IndexOf(FileListcomboBox.SelectedItem);
                Setting.FileItems.Remove(FileListcomboBox.SelectedItem as FileItem);
                FileListcomboBox.Items.Remove(FileListcomboBox.SelectedItem);
                if (index > 0) FileListcomboBox.SelectedItem = FileListcomboBox.Items[index - 1];
                if (FileListcomboBox.Items.Count == 0) FileListcomboBox.Text = string.Empty;
                buttonAdd.Text = "ADD (" + FileListcomboBox.Items.Count.ToString() + ")";
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            SetFile();
        }

        #region werferstein_org info

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            BaseApp.Base.ViewInfo("About "  + Program.ProgramName, Program.ProgramName, "1.0.0.1", "Simple program to register or deregister dotnet DLLs. \r\n\r\nThis program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.") ;
        }
        #endregion

        private void FileListcomboBox_MouseHover(object sender, EventArgs e)
        {
            if (FileListcomboBox.Text == "")
            {
                toolTip1.SetToolTip(FileListcomboBox, "Left click for options!");
                return;
            }
            toolTip1.SetToolTip(FileListcomboBox, "Left click for options!" + Environment.NewLine + FileListcomboBox.Text);                        
        }

        private void ComboBoxNetLink_MouseHover(object sender, EventArgs e)
        {
            if (blockGui || comboBoxNetLink.SelectedItem == null) return;            
            toolTip1.SetToolTip(comboBoxNetLink, (comboBoxNetLink.SelectedItem as NetItem).FullPath);
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = string.Empty;
            fct_box.Text = string.Empty;
            bool error = false;
            SetToViewRegFile();
            Logger.Instance.AdddLog(LogType.Debug, "Start to find DLLs in registry!", this);
            if (Environment.Is64BitProcess)
            {
                Logger.Instance.AdddLog(LogType.Info, "This is a 64 bit proccess!");
            }


            if (comboBoxNetLink.SelectedItem == null || string.IsNullOrWhiteSpace((comboBoxNetLink.SelectedItem as NetItem).FullPath) || !System.IO.File.Exists((comboBoxNetLink.SelectedItem as NetItem).FullPath))
            {
                Logger.Instance.AdddLog(LogType.Error, "Framework link ?", this);
                return;
            }
            if (Setting.FileItems == null || Setting.FileItems.Count == 0)
            {
                Logger.Instance.AdddLog(LogType.Error, "Missing DLL file link ?", this);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            Setting.FileItems = Setting.FileItems.GroupBy(x => new { x.FullPath, x.Name, x.RegExe }).Select(g => g.First()).ToList();
            foreach (var FileItem in Setting.FileItems)
            {
                if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;
                if (Setting.FileItems.Count > 1)
                {
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", this);
                    Logger.Instance.AdddLog(LogType.Info, "------------------------------------------------------------------------------------------", this);
                }
                if (!System.IO.File.Exists(FileItem.FullPath))
                {
                    Logger.Instance.AdddLog(LogType.Error, "File not exist! " + FileItem.FullPath);
                    error = true;
                    //continue;
                }


                DllRegister.IsRegistered(out bool bit64, out bool gac, FileItem.FullPath,false, textBoxOutPath.Text,timeId);
            }

            labelResult.BorderStyle = BorderStyle.FixedSingle;
            if (error)
            {
                labelResult.Text = "ERROR";
                labelResult.BackColor = Color.Red;
            }

            else
            {
                labelResult.Text = "OK!";
                labelResult.BackColor = Color.ForestGreen;
            }
            Cursor.Current = Cursors.Default;
        }

        #region Checked !
        private void CheckBoxInstallinGAC_CheckedChanged(object sender, EventArgs e)
        {
            if (blockGui) return;
            blockGui = true;
            checkBoxRegistry.Checked = false;
            checkBoxCodebase.Checked = false;
            blockGui = false;
        }

        private void CheckBoxRegistry_CheckedChanged(object sender, EventArgs e)
        {
            if (blockGui) return;
            blockGui = true;
            checkBoxInstallinGAC.Checked = false;
            blockGui = false;
        }

        private void CheckBoxCodebase_CheckedChanged(object sender, EventArgs e)
        {
            if (blockGui) return;
            blockGui = true;
            checkBoxInstallinGAC.Checked = false;
            blockGui = false;
        }
        #endregion

        private void SaveInstallLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (blockGui) return;
            saveInstallLogToolStripMenuItem.Checked = !saveInstallLogToolStripMenuItem.Checked;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = textBoxOutPath.Text,
                Description = "Output path to save the registration/ log files"
            };

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                textBoxOutPath.Text = folderBrowser.SelectedPath;
                Setting.OutputPath = textBoxOutPath.Text;
            }
        }
        #endregion

        #region RegEdit
        Process procRegEditExe;
        private void ToolStripMenuItemRegedit_Click(object sender, EventArgs e)
        {
            if (procRegEditExe != null) { procRegEditExe.Kill(); procRegEditExe = null; }
            Cursor.Current = Cursors.WaitCursor;
            string registryLocation = "CLSID";
            if (FileListcomboBox.SelectedItem != null)
            {
                List<RegData> regData = DllRegister.SearchRegistryNative(System.IO.Path.GetFileName((FileListcomboBox.SelectedItem as FileItem).FullPath), "Wow6432Node\\CLSID");
                if (regData != null && regData.Count > 0 && !string.IsNullOrWhiteSpace(regData[0].RegistryPath))
                {
                    registryLocation = regData[0].RegistryPath;
                    if (!string.IsNullOrWhiteSpace(regData[0].CLSID)) Logger.Instance.AdddLog(LogType.Info, "Found CLSID in Wow6432Node       : " + regData[0].CLSID, "DllRegister");
                    if (!string.IsNullOrWhiteSpace(regData[0].Path)) Logger.Instance.AdddLog(LogType.Info, "Found path in  Wow6432Node\\CLSID: " + regData[0].Path, "DllRegister");
                }
                else
                {
                    regData = DllRegister.SearchRegistryNative(System.IO.Path.GetFileName((FileListcomboBox.SelectedItem as FileItem).FullPath), "Wow6432Node\\CLSID");
                    if (regData != null && regData.Count > 0 && !string.IsNullOrWhiteSpace(regData[0].RegistryPath))
                    {
                        registryLocation = regData[0].RegistryPath;
                        if (regData != null && !string.IsNullOrWhiteSpace(regData[0].CLSID)) Logger.Instance.AdddLog(LogType.Info, "Found CLSID in Wow6432Node       : " + regData[0].CLSID, "DllRegister");
                        if (regData != null && !string.IsNullOrWhiteSpace(regData[0].Path)) Logger.Instance.AdddLog(LogType.Info, "Found path in  Wow6432Node\\CLSID: " + regData[0].Path, "DllRegister");
                    }
                }
            }
            Cursor.Current = Cursors.Default;
            OpenRegEdit(registryLocation);
        }

        private void ProcRegEditExe_Exited(object sender, EventArgs e)
        {
            procRegEditExe = null;
        }

        private void BitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (procRegEditExe != null) { procRegEditExe.Kill(); procRegEditExe = null; }
            Cursor.Current = Cursors.WaitCursor;
            string registryLocation = "Wow6432Node\\CLSID";
            if (FileListcomboBox.SelectedItem != null)
            {
                List<RegData> regData = DllRegister.SearchRegistryNative(System.IO.Path.GetFileName((FileListcomboBox.SelectedItem as FileItem).FullPath), "Wow6432Node\\CLSID");
                if (regData != null && regData.Count > 0 && !string.IsNullOrWhiteSpace(regData[0].RegistryPath) )
                {
                    registryLocation = regData[0].RegistryPath;
                    if (!string.IsNullOrWhiteSpace(regData[0].CLSID)) Logger.Instance.AdddLog(LogType.Info, "Found CLSID in Wow6432Node       : " + regData[0].CLSID, "DllRegister");
                    if (!string.IsNullOrWhiteSpace(regData[0].Path)) Logger.Instance.AdddLog(LogType.Info, "Found path in  Wow6432Node\\CLSID: " + regData[0].Path, "DllRegister");
                }
            }
            Cursor.Current = Cursors.Default;
            OpenRegEdit(registryLocation);
        }
        private void OpenRegEdit(string registryLocation)
        {
            var registryLastKey = @"HKEY_CLASSES_ROOT" + "\\" + registryLocation;
            try
            {
                // Set LastKey value that regedit will go directly to                
                RegistryKey rKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true);
                if (rKey != null)
                {
                    rKey.SetValue("LastKey", registryLastKey);
                }
                procRegEditExe = new Process();
                procRegEditExe.Exited += ProcRegEditExe_Exited; ;
                procRegEditExe.StartInfo.FileName = @"regedit.exe";
                procRegEditExe.EnableRaisingEvents = true;
                procRegEditExe.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileListcomboBox.SelectedItem != null && System.IO.Directory.Exists(System.IO.Path.GetDirectoryName((FileListcomboBox.SelectedItem as FileItem).FullPath)))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo
                {
                    FileName = System.IO.Path.GetDirectoryName((FileListcomboBox.SelectedItem as FileItem).FullPath)
                };
                Process process = Process.Start(StartInformation);
            }
        }
        private void TextBoxOutPath_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxOutPath.Text) && System.IO.Directory.Exists(textBoxOutPath.Text))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo
                {
                    FileName = textBoxOutPath.Text
                };
                Process process = Process.Start(StartInformation);
            }
        }


        private void TextBoxPName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxPName.Text) || blockGui) return;
            blockGui = true;            
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9 -]");
            textBoxPName.Text = rgx.Replace(textBoxPName.Text, "");
            blockGui = false;
        }


        #endregion

        private void ComboBoxNetLink_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (blockGui) return;
            if (FileListcomboBox.SelectedItem != null && comboBoxNetLink.SelectedItem != null)
            {
                blockGui = true;
                (FileListcomboBox.SelectedItem as FileItem).RegExe = (comboBoxNetLink.SelectedItem as NetItem).FullPath;
                comboBoxNetLink.BackColor = SystemColors.Control;
                blockGui = false;
            }
        }

        private void FileListcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (blockGui) return;
            if (FileListcomboBox.SelectedItem != null && comboBoxNetLink.SelectedItem != null)
            {
                blockGui = true;
                if (!string.IsNullOrWhiteSpace((FileListcomboBox.SelectedItem as FileItem).RegExe))
                {
                    SetRegExe((FileListcomboBox.SelectedItem as FileItem).RegExe);
                    comboBoxNetLink.BackColor = SystemColors.Control;
                }
                else
                {
                    comboBoxNetLink.Text = string.Empty;
                    comboBoxNetLink.BackColor = Color.Red;
                }
                blockGui = false;
            }
        }

        private void ViewRegFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viewRegFileToolStripMenuItem.Checked = !viewRegFileToolStripMenuItem.Checked;

            if (viewRegFileToolStripMenuItem.Checked)
            {
                

                GetUiInput();
                if (!DllRegister.TestSettings(Setting))
                {
                    Logger.Instance.AdddLog(LogType.Error, "Project or file name or no write access? ", "DllRegister", "");
                    SetValuesToForm(false);
                }


                FileItem fileItem = FileListcomboBox.SelectedItem as FileItem;

                if (FileListcomboBox.SelectedItem != null &&
                    !string.IsNullOrWhiteSpace(fileItem.FullPath) &&
                    !string.IsNullOrWhiteSpace(Setting.OutputPath) &&
                    System.IO.Directory.Exists(Setting.OutputPath) 
                    )
                {

                    string path = string.Empty;
                    if (!string.IsNullOrWhiteSpace(fileItem.RegBuildPath) && System.IO.File.Exists(fileItem.RegBuildPath))
                    {
                        path = fileItem.RegBuildPath;
                    }
                    else
                    {
                        path = Setting.OutputPath + System.IO.Path.GetFileNameWithoutExtension(fileItem.FullPath) + ".reg";
                        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                        {
                            path = string.Empty;
                        }
                    }
                   

                    if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                    {
                        try
                        {
                            var lines = File.ReadAllText(path);
                            if (!string.IsNullOrWhiteSpace(lines))
                            {
                                logBufferText = fct_box.Text;
                                //logBuffer = fct_box.TextSource;

                                fct_box.Language = Language.CSharp;
                                fct_box.Text = lines;
                                viewRegFileToolStripMenuItem.Text = "View Log file";
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.AdddLog(LogType.Error, ex.Message,this,"",ex);
                        }                        
                    }
                }

                viewRegFileToolStripMenuItem.Checked = false;
            }
            else
            {
                SetToViewRegFile();
            }
        }

        private void SetToViewRegFile()
        {
            if (logBufferText != null)
            {
                viewRegFileToolStripMenuItem.Text = "View Reg. file";
                //fct_box.TextSource = logBuffer;
                fct_box.Text = logBufferText;
                fct_box.Language = Language.Lua;
                logBufferText = null;
            }   
        }

        private void CSharpSyntaxHighlight(TextChangedEventArgs e)
        {
            //[HKEY_CLASSES_ROOT\CLSID\{403E7AD7-2281-4651-95DD-2775EAEF4920}\InprocServer32\2.0.0.5]



            fct_box.LeftBracket = '(';
            fct_box.RightBracket = ')';
            fct_box.LeftBracket2 = '\x0';
            fct_box.RightBracket2 = '\x0';
            //clear style of changed range
            e.ChangedRange.ClearStyle(BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle);

            //string highlighting
            e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            //comment highlighting
            e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            //number highlighting
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            //attribute highlighting
            e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            //class name highlighting
            e.ChangedRange.SetStyle(BoldStyle, @"\b(Class|Assembly|RuntimeVersion|CodeBase|file)\s+(?<range>\w+?)\b");
            //keyword highlighting
            e.ChangedRange.SetStyle(errorStyle, @"\b(DllRegister|ERROR|error|Not fond|Version)\b|#region\b|#endregion\b");

            //keyword highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(HKEY_CLASSES_ROOT|CLSID|InprocServer32|CLASSES_ROOT|Wow6432Node|Keys|Key|dll|DLL)\b|#region\b|#endregion\b");



            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");//allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");//allow to collapse #region blocks
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
        }

        private void fct_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (blockGui) return;
            CSharpSyntaxHighlight(e);//custom highlighting
        }

        private void fct_box_AutoIndentNeeded(object sender, AutoIndentEventArgs args)
        {
            //block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{.*\}[^""']*$"))
                return;
            //start of block {}
            if (Regex.IsMatch(args.LineText, @"^[^""']*\{"))
            {
                args.ShiftNextLines = args.TabLength;
                return;
            }
            //end of block {}
            if (Regex.IsMatch(args.LineText, @"}[^""']*$"))
            {
                args.Shift = -args.TabLength;
                args.ShiftNextLines = -args.TabLength;
                return;
            }
            //label
            if (Regex.IsMatch(args.LineText, @"^\s*\w+\s*:\s*($|//)") &&
                !Regex.IsMatch(args.LineText, @"^\s*default\s*:"))
            {
                args.Shift = -args.TabLength;
                return;
            }
            //some statements: case, default
            if (Regex.IsMatch(args.LineText, @"^\s*(case|default)\b.*:\s*($|//)"))
            {
                args.Shift = -args.TabLength / 2;
                return;
            }
            //is unclosed operator in previous line ?
            if (Regex.IsMatch(args.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$"))
                if (!Regex.IsMatch(args.PrevLineText, @"(;\s*$)|(;\s*//)"))//operator is unclosed
                {
                    args.Shift = args.TabLength;
                    return;
                }
        }

        private void fct_box_CustomAction(object sender, CustomActionEventArgs e)
        {

        }

        private void fct_box_SelectionChangedDelayed(object sender, EventArgs e)
        {
            fct_box.VisibleRange.ClearStyle(SameWordsStyle);
            if (!fct_box.Selection.IsEmpty)
                return;//user selected diapason

            //get fragment around caret
            var fragment = fct_box.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;
            //highlight same words
            var ranges = fct_box.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(SameWordsStyle);
        }
    }
}
