using Helper.Dll;
using Helper.Uac;
using Microsoft.Win32;
using SLogging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DllRegister
{
    public partial class MainForm : Form
    {
        //[DllImport("E:\\Nextcloud\\Projekt\\C#\\Register DLL\\DLL Temlate\\BluefrogLibrary\\bin\\Release\\BluefrogLibrary.dll", EntryPoint = "#8002")]
        //public static extern void OpenTestForm(string msg);

        private bool blockGui = false;
        private string lastPath = string.Empty;
        private string timeId = DateTime.Now.ToString("ddMMyyHHmmssfff");
        private Settings Setting;

        
        public MainForm()
        {
            InitializeComponent();

            #region int Logger
            if (!Logger.LoggerIsOnline)
            {
                Logger.Instance.Stop();
                Logger.Instance.Start(Output.ADD_Counter | Output.ADD_InnerException | Output.ADD_StackTrace);
            }
            Logger.Instance.OnLogMessage += Instance_OnLogMessage; 
            #endregion

            Setting = Settings.Load("*.regdll");
            if (!String.IsNullOrWhiteSpace(Settings.LoadedFrom)) Setting.MainOptions.OptionPath = Settings.LoadedFrom;
            if (Setting == null) Setting = new Settings();           
            
            Logger.Instance.AdddLog(LogType.Info, "DLL_Register " + String.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version.ToString()) + " start!", this);
            
            //this.FormBorderStyle = FormBorderStyle.Fixed3D;
            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            blockGui = true;

            GetInfo();
            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = "";

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
                    MessageBox.Show("Can`t find regasm.exe?", "EXIT" ,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
            }

            #region fill combo box regasm.exe
            comboBoxNetLink.Items.Clear();
            comboBoxNetLink.Text = string.Empty;
            Logger.Instance.AdddLog(LogType.Info, "Search in: "+ framework + " for regasm.exe!", this);
            string[] files = Directory.GetFiles(framework, "regasm.exe", SearchOption.AllDirectories);
            Logger.Instance.AdddLog(LogType.Info, "Found " + files.Length.ToString()  + " regasm.exe files.", this);

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

                    if (Environment.Is64BitOperatingSystem && item.Contains("64"))
                    {
                        comboBoxNetLink.Text = y.Name;
                    }
                    else
                    {
                        comboBoxNetLink.Text = y.Name;
                    }
                }
                if (!string.IsNullOrWhiteSpace(Setting.RegisterEXE) && System.IO.File.Exists(Setting.RegisterEXE))
                {
                    bool found = false;
                    foreach (var item in comboBoxNetLink.Items)
                    {
                        if ((item as NetItem).FullPath == Setting.RegisterEXE)
                        {
                            comboBoxNetLink.SelectedItem = item;
                            found = true;
                            break;
                        }

                    }
                    if (!found)
                    {
                        NetItem ni = new NetItem() { FullPath = Setting.RegisterEXE };
                        comboBoxNetLink.Items.Add(ni);
                        comboBoxNetLink.SelectedItem = ni;
                    }
                }
            }
            #endregion

            #region fill combo box DLLs            
            FileListcomboBox.Items.Clear();
            FileListcomboBox.Text = string.Empty;

            if (Setting.FileItems != null && Setting.FileItems.Count > 0)
            {
                Setting.FileItems = Setting.FileItems.GroupBy(x => x).Select(y => y.First()).ToList();
                foreach (var item in Setting.FileItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.FullPath))
                    {
                        FileListcomboBox.Items.Add(item);
                        FileListcomboBox.SelectedItem = item;
                        lastPath = item.FullPath;
                    }
                }
                labelCount.Text = FileListcomboBox.Items.Count.ToString() + " Files";
            }


            #endregion

            checkBoxInstallinGAC.Checked = Setting.InstallInGAC;
            checkBoxRegistry.Checked = Setting.BuildRegistryKey;
            checkBoxCodebase.Checked = Setting.Codebase;
            textBoxOutPath.Text = Setting.OutputPath;
            saveInstallLogToolStripMenuItem.Checked = Setting.SaveLogging;

            blockGui = false;
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
                lock (richTextBox)
                {
                    richTextBox.AppendText(e.LogMessage + Environment.NewLine);
                    // set the current caret position to the end
                    richTextBox.SelectionStart = richTextBox.Text.Length;
                    // scroll it automatically
                    richTextBox.ScrollToCaret();

                    #region save log
                    if (!string.IsNullOrWhiteSpace(richTextBox.Text) && !string.IsNullOrWhiteSpace(DllRegister.BackupPath) && System.IO.Directory.Exists(DllRegister.BackupPath))
                    {
                        System.IO.File.WriteAllText(DllRegister.LogFilePath, richTextBox.Text);
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region Register
        private void ButtonRegister_Click(object sender, EventArgs e)
        {
            if ((checkBoxInstallinGAC.Checked || !checkBoxRegistry.Checked) && !UacHelper.IsRunAsAdmin())
            {
                MessageBox.Show("You need administrator rights to register a dll!", "Administrator ?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = string.Empty;
            richTextBox.Text = string.Empty;
            bool error = false;

            Logger.Instance.AdddLog(LogType.Debug, "Start to register DLLs!", this);

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


            string registryCode = string.Empty;
            int count = 0;
            Cursor.Current = Cursors.WaitCursor;       
            foreach (var FileItem in Setting.FileItems)
            {
                string tmpRegistryCode = string.Empty;
                if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;
                if (!System.IO.File.Exists(FileItem.FullPath))
                {
                    Logger.Instance.AdddLog(LogType.Error, "File not exist! " + FileItem.FullPath);
                    error = true;
                    continue;
                }

                #region Register DLL
                if (!DllRegister.Register(textBoxOutPath.Text, FileItem.FullPath, (comboBoxNetLink.SelectedItem as NetItem).FullPath, checkBoxInstallinGAC.Checked, checkBoxRegistry.Checked, checkBoxCodebase.Checked, timeId, out tmpRegistryCode))
                {
                    if (!checkBoxRegistry.Checked) error = true;
                }
                #endregion


                if (!string.IsNullOrWhiteSpace(tmpRegistryCode))
                {
                    count++;
                    registryCode += tmpRegistryCode + Environment.NewLine + Environment.NewLine;
                }



            }//foreach (var item in Setting.FileItems)

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

                #region save all reg code
                if (count > 1 && !string.IsNullOrWhiteSpace(registryCode) && !string.IsNullOrWhiteSpace(DllRegister.BackupPath) && System.IO.Directory.Exists(DllRegister.BackupPath))
                {
                    System.IO.File.WriteAllText(DllRegister.LogFilePath + "Reg", registryCode);
                } 
                #endregion
            }

            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region Unregister
        private void ButtonUnRegister_Click(object sender, EventArgs e)
        {
            if (!UacHelper.IsRunAsAdmin())
            {
                MessageBox.Show("You need administrator rights to deregister a dll!", "Administrator ?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            labelResult.BorderStyle = BorderStyle.None;
            labelResult.BackColor = this.BackColor;
            labelResult.Text = string.Empty;
            richTextBox.Text = string.Empty;
            bool error = false;

            Logger.Instance.AdddLog(LogType.Debug, "Start to unregister DLLs!", this);

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
            foreach (var FileItem in Setting.FileItems)
            {
                if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;
                if (!System.IO.File.Exists(FileItem.FullPath))
                {
                    Logger.Instance.AdddLog(LogType.Error, "File not exist! " + FileItem.FullPath);
                    error = true;
                    //continue;
                }

                if (!DllRegister.UnRegister(FileItem.FullPath, (comboBoxNetLink.SelectedItem as NetItem).FullPath, textBoxOutPath.Text, timeId, checkBoxInstallinGAC.Checked))
                {
                    error = true;
                }
            }//foreach (var item in Setting.FileItems)

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
        #endregion

        #region UAC Info
        private void GetInfo()
        {
            // Get and display whether the primary access token of the process belongs 
            // to user account that is a member of the local Administrators group even 
            // if it currently is not elevated (IsUserInAdminGroup).
            try
            {
                bool fInAdminGroup = UacHelper.IsUserInAdminGroup();
                this.lbInAdminGroup.Text = fInAdminGroup.ToString();
            }
            catch (Exception ex)
            {
                this.lbInAdminGroup.Text = "N/A";
                MessageBox.Show(ex.Message, "An error occurred in IsUserInAdminGroup",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Get and display whether the process is run as administrator or not 
            // (IsRunAsAdmin).
            try
            {
                bool fIsRunAsAdmin = UacHelper.IsRunAsAdmin();
                this.lbIsRunAsAdmin.Text = fIsRunAsAdmin.ToString();
            }
            catch (Exception ex)
            {
                this.lbIsRunAsAdmin.Text = "N/A";
                MessageBox.Show(ex.Message, "An error occurred in IsRunAsAdmin",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            // Get and display the process elevation information (IsProcessElevated) 
            // and integrity level (GetProcessIntegrityLevel). The information is not 
            // available on operating systems prior to Windows Vista.
            if (Environment.OSVersion.Version.Major >= 6)
            {
                // Running Windows Vista or later (major version >= 6). 

                try
                {
                    // Get and display the process elevation information.
                    bool fIsElevated = UacHelper.IsProcessElevated();
                    this.lbIsElevated.Text = fIsElevated.ToString();

                    // Update the Self-elevate button to show the UAC shield icon on 
                    // the UI if the process is not elevated.
                    //this.btnElevate.FlatStyle = FlatStyle.System;
                    //NativeMethods.SendMessage(btnElevate.Handle,
                    //    NativeMethods.BCM_SETSHIELD, (IntPtr)0,
                    //    fIsElevated ? IntPtr.Zero : (IntPtr)1);
                }
                catch (Exception ex)
                {
                    this.lbIsElevated.Text = "N/A";
                    MessageBox.Show(ex.Message, "An error occurred in IsProcessElevated",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                try
                {
                    // Get and display the process integrity level.
                    int IL = UacHelper.GetProcessIntegrityLevel();
                    switch (IL)
                    {
                        case NativeMethods.SECURITY_MANDATORY_UNTRUSTED_RID:
                            this.lbIntegrityLevel.Text = "Untrusted"; break;
                        case NativeMethods.SECURITY_MANDATORY_LOW_RID:
                            this.lbIntegrityLevel.Text = "Low"; break;
                        case NativeMethods.SECURITY_MANDATORY_MEDIUM_RID:
                            this.lbIntegrityLevel.Text = "Medium"; break;
                        case NativeMethods.SECURITY_MANDATORY_HIGH_RID:
                            this.lbIntegrityLevel.Text = "High"; break;
                        case NativeMethods.SECURITY_MANDATORY_SYSTEM_RID:
                            this.lbIntegrityLevel.Text = "System"; break;
                        default:
                            this.lbIntegrityLevel.Text = "Unknown"; break;
                    }
                }
                catch (Exception ex)
                {
                    this.lbIntegrityLevel.Text = "N/A";
                    MessageBox.Show(ex.Message, "An error occurred in GetProcessIntegrityLevel",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                this.lbIsElevated.Text = "N/A";
                this.lbIntegrityLevel.Text = "N/A";
            }
        }
        #endregion

        #region Load & Save
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setting.InstallInGAC = checkBoxInstallinGAC.Checked;
            Setting.BuildRegistryKey = checkBoxRegistry.Checked;
            Setting.Codebase = checkBoxCodebase.Checked;
            Setting.OutputPath = textBoxOutPath.Text;
            Setting.SaveLogging = saveInstallLogToolStripMenuItem.Checked;


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
                if (comboBoxNetLink.SelectedItem != null)
                {
                    if (System.IO.File.Exists((comboBoxNetLink.SelectedItem as NetItem).FullPath))
                    {
                        Setting.RegisterEXE = (comboBoxNetLink.SelectedItem as NetItem).FullPath;
                    }
                    else
                    {
                        Setting.RegisterEXE = string.Empty;
                    }
                }
                Setting.MainOptions.OptionPath = dialog.FileName;
                Setting.Save(dialog.FileName);
            }
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
                if (error == string.Empty) MainForm_Load(this, null);
                Setting.MainOptions.OptionPath = dialog.FileName;
                timeId = DateTime.Now.ToString("ddMMyyHHmmssfff");
            }
        }
        public void SetFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

            if (string.IsNullOrWhiteSpace(lastPath) && System.IO.Directory.Exists(lastPath))
            {
                path = lastPath;
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
                bool add = false;
                foreach (string item in dialog.FileNames)
                {
                    if (Setting.FileItems.FirstOrDefault(r => r.FullPath == item) == null)
                    {
                        Setting.FileItems.Add(new FileItem() { FullPath = item, Name = System.IO.Path.GetFileName(item) });
                        add = true;
                    }
                    lastPath = item;
                }
                if (add)
                {
                    timeId = DateTime.Now.ToString("ddMMyyHHmmssfff");
                    FileListcomboBox.Items.Clear();
                    foreach (var item in Setting.FileItems)
                    {
                        FileListcomboBox.Items.Add(item);
                        FileListcomboBox.SelectedItem = item;
                    }
                    if (FileListcomboBox.SelectedItem is FileItem lastItem) FileListcomboBox.SelectedItem = lastItem;
                }

                labelCount.Text = FileListcomboBox.Items.Count.ToString() + " Files";
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
            labelCount.Text = "0 Files";
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
                labelCount.Text = FileListcomboBox.Items.Count.ToString() + " Files";
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            SetFile();
        }

        #region werferstein_org info
        werferstein_org info = null;
        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                bool ok = false;
                if (info == null || info.IsDisposed)
                {
                    var T = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly item in T)
                    {
                        if (item.FullName.StartsWith(Program.ProgramName))
                        {
                            info = new werferstein_org(item);
                            info.Disposed += Info_Disposed;
                            ok = true;
                            break;
                        }
                    }
                }

                if (ok)
                {
                    info.Show();
                    info.BringToFront();
                    info.Focus();
                }

            }
            catch { }
        }

        private void Info_Disposed(object sender, EventArgs e)
        {
            info = null;
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
            richTextBox.Text = string.Empty;
            bool error = false;
            Logger.Instance.AdddLog(LogType.Debug, "Start to find DLLs in registry!", this);

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
            foreach (var FileItem in Setting.FileItems)
            {
                if (string.IsNullOrWhiteSpace(FileItem.FullPath)) continue;
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
                registryLocation = DllRegister.SearchRegistryNative(System.IO.Path.GetFileName((FileListcomboBox.SelectedItem as FileItem).FullPath), out string CLSID, out string path, "CLSID");
                if (!string.IsNullOrWhiteSpace(CLSID)) Logger.Instance.AdddLog(LogType.Info, "Found CLSID        : " + CLSID, "DllRegister");
                if (!string.IsNullOrWhiteSpace(path)) Logger.Instance.AdddLog(LogType.Info, "Found path in CLSID: " + path, "DllRegister");

                if (string.IsNullOrEmpty(registryLocation))
                {
                    registryLocation = DllRegister.SearchRegistryNative(System.IO.Path.GetFileName((FileListcomboBox.SelectedItem as FileItem).FullPath), out CLSID, out path, "Wow6432Node\\CLSID");
                    if (!string.IsNullOrWhiteSpace(CLSID)) Logger.Instance.AdddLog(LogType.Info, "Found CLSID in Wow6432Node       : " + CLSID, "DllRegister");
                    if (!string.IsNullOrWhiteSpace(path)) Logger.Instance.AdddLog(LogType.Info, "Found path in  Wow6432Node\\CLSID: " + path, "DllRegister");
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
                registryLocation = DllRegister.SearchRegistryNative(System.IO.Path.GetFileName((FileListcomboBox.SelectedItem as FileItem).FullPath), out string CLSID, out string path, "Wow6432Node\\CLSID");
                if (!string.IsNullOrWhiteSpace(CLSID)) Logger.Instance.AdddLog(LogType.Info, "Found CLSID in Wow6432Node\\CLSID: " + CLSID, "DllRegister");
                if (!string.IsNullOrWhiteSpace(path)) Logger.Instance.AdddLog(LogType.Info, "Found path in  Wow6432Node\\CLSID: " + path, "DllRegister");
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

        private void openPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileListcomboBox.SelectedItem != null && System.IO.Directory.Exists(System.IO.Path.GetDirectoryName((FileListcomboBox.SelectedItem as FileItem).FullPath)))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = System.IO.Path.GetDirectoryName((FileListcomboBox.SelectedItem as FileItem).FullPath);
                Process process = Process.Start(StartInformation);
            }
        }
        private void textBoxOutPath_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxOutPath.Text) && System.IO.Directory.Exists(textBoxOutPath.Text))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = textBoxOutPath.Text;
                Process process = Process.Start(StartInformation);
            }
        }
        #endregion


        //[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        //private static extern IntPtr CreateRoundRectRgn
        //(
        //    int nLeftRect,     // x-coordinate of upper-left corner
        //    int nTopRect,      // y-coordinate of upper-left corner
        //    int nRightRect,    // x-coordinate of lower-right corner
        //    int nBottomRect,   // y-coordinate of lower-right corner
        //    int nWidthEllipse, // width of ellipse
        //    int nHeightEllipse // height of ellipse
        //);
    }
}
