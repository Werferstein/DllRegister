/*
Ingolf Hill
werferstein.org
/*
  This program is free software. It comes without any warranty, to
  the extent permitted by applicable law. You can redistribute it
  and/or modify it under the terms of the Do What The Fuck You Want
  To Public License, Version 2, as published by Sam Hocevar.
*/

namespace DllRegister
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxPName = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOutPath = new System.Windows.Forms.TextBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.checkBoxCodebase = new System.Windows.Forms.CheckBox();
            this.checkBoxRegistry = new System.Windows.Forms.CheckBox();
            this.checkBoxInstallinGAC = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.comboBoxNetLink = new System.Windows.Forms.ComboBox();
            this.labelResult = new System.Windows.Forms.Label();
            this.buttonUnRegister = new System.Windows.Forms.Button();
            this.buttonRegister = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveInstallLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRegedit = new System.Windows.Forms.ToolStripMenuItem();
            this.bitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRegFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileListcomboBox = new System.Windows.Forms.ComboBox();
            this.FilelistContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.openPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fct_box = new FastColoredTextBoxNS.FastColoredTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.FilelistContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fct_box)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label7);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxPName);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxOutPath);
            this.splitContainer1.Panel1.Controls.Add(this.buttonTest);
            this.splitContainer1.Panel1.Controls.Add(this.checkBoxCodebase);
            this.splitContainer1.Panel1.Controls.Add(this.checkBoxRegistry);
            this.splitContainer1.Panel1.Controls.Add(this.checkBoxInstallinGAC);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.buttonAdd);
            this.splitContainer1.Panel1.Controls.Add(this.comboBoxNetLink);
            this.splitContainer1.Panel1.Controls.Add(this.labelResult);
            this.splitContainer1.Panel1.Controls.Add(this.buttonUnRegister);
            this.splitContainer1.Panel1.Controls.Add(this.buttonRegister);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            this.splitContainer1.Panel1.Controls.Add(this.FileListcomboBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Info;
            this.splitContainer1.Panel2.Controls.Add(this.fct_box);
            this.splitContainer1.Size = new System.Drawing.Size(739, 509);
            this.splitContainer1.SplitterDistance = 226;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(17, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(126, 20);
            this.label7.TabIndex = 21;
            this.label7.Text = "Project name:";
            // 
            // textBoxPName
            // 
            this.textBoxPName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPName.Location = new System.Drawing.Point(151, 44);
            this.textBoxPName.Name = "textBoxPName";
            this.textBoxPName.Size = new System.Drawing.Size(576, 27);
            this.textBoxPName.TabIndex = 20;
            this.textBoxPName.TabStop = false;
            this.toolTip1.SetToolTip(this.textBoxPName, "Output path to save the registration/ log files");
            this.textBoxPName.TextChanged += new System.EventHandler(this.textBoxPName_TextChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(702, 151);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(25, 23);
            this.button1.TabIndex = 19;
            this.button1.TabStop = false;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 20);
            this.label1.TabIndex = 18;
            this.label1.Text = "Output path:";
            // 
            // textBoxOutPath
            // 
            this.textBoxOutPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOutPath.Location = new System.Drawing.Point(151, 151);
            this.textBoxOutPath.Name = "textBoxOutPath";
            this.textBoxOutPath.Size = new System.Drawing.Size(545, 27);
            this.textBoxOutPath.TabIndex = 17;
            this.textBoxOutPath.TabStop = false;
            this.toolTip1.SetToolTip(this.textBoxOutPath, "Output path to save the registration/ log files");
            this.textBoxOutPath.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxOutPath_MouseDoubleClick);
            // 
            // buttonTest
            // 
            this.buttonTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTest.Location = new System.Drawing.Point(151, 76);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(62, 34);
            this.buttonTest.TabIndex = 15;
            this.buttonTest.TabStop = false;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.ButtonTest_Click);
            // 
            // checkBoxCodebase
            // 
            this.checkBoxCodebase.AutoSize = true;
            this.checkBoxCodebase.Checked = true;
            this.checkBoxCodebase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCodebase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxCodebase.Location = new System.Drawing.Point(516, 87);
            this.checkBoxCodebase.Name = "checkBoxCodebase";
            this.checkBoxCodebase.Size = new System.Drawing.Size(102, 21);
            this.checkBoxCodebase.TabIndex = 14;
            this.checkBoxCodebase.TabStop = false;
            this.checkBoxCodebase.Text = "Codebase";
            this.checkBoxCodebase.UseVisualStyleBackColor = true;
            this.checkBoxCodebase.CheckedChanged += new System.EventHandler(this.CheckBoxCodebase_CheckedChanged);
            // 
            // checkBoxRegistry
            // 
            this.checkBoxRegistry.AutoSize = true;
            this.checkBoxRegistry.Checked = true;
            this.checkBoxRegistry.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRegistry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxRegistry.Location = new System.Drawing.Point(344, 87);
            this.checkBoxRegistry.Name = "checkBoxRegistry";
            this.checkBoxRegistry.Size = new System.Drawing.Size(176, 21);
            this.checkBoxRegistry.TabIndex = 12;
            this.checkBoxRegistry.TabStop = false;
            this.checkBoxRegistry.Text = "Build registry key(s)";
            this.checkBoxRegistry.UseVisualStyleBackColor = true;
            this.checkBoxRegistry.CheckedChanged += new System.EventHandler(this.CheckBoxRegistry_CheckedChanged);
            // 
            // checkBoxInstallinGAC
            // 
            this.checkBoxInstallinGAC.AutoSize = true;
            this.checkBoxInstallinGAC.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxInstallinGAC.Location = new System.Drawing.Point(217, 87);
            this.checkBoxInstallinGAC.Name = "checkBoxInstallinGAC";
            this.checkBoxInstallinGAC.Size = new System.Drawing.Size(128, 21);
            this.checkBoxInstallinGAC.TabIndex = 11;
            this.checkBoxInstallinGAC.TabStop = false;
            this.checkBoxInstallinGAC.Text = "Install in GAC";
            this.checkBoxInstallinGAC.UseVisualStyleBackColor = true;
            this.checkBoxInstallinGAC.CheckedChanged += new System.EventHandler(this.CheckBoxInstallinGAC_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(147, 118);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(202, 24);
            this.label6.TabIndex = 10;
            this.label6.Text = "RegAsem.exe version:";
            // 
            // buttonAdd
            // 
            this.buttonAdd.BackColor = System.Drawing.Color.Red;
            this.buttonAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAdd.Location = new System.Drawing.Point(12, 181);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(85, 30);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "Add";
            this.toolTip1.SetToolTip(this.buttonAdd, "Left click for options!");
            this.buttonAdd.UseVisualStyleBackColor = false;
            this.buttonAdd.Click += new System.EventHandler(this.Button2_Click);
            this.buttonAdd.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Button2_MouseDown);
            // 
            // comboBoxNetLink
            // 
            this.comboBoxNetLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxNetLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxNetLink.FormattingEnabled = true;
            this.comboBoxNetLink.Location = new System.Drawing.Point(313, 118);
            this.comboBoxNetLink.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxNetLink.Name = "comboBoxNetLink";
            this.comboBoxNetLink.Size = new System.Drawing.Size(414, 28);
            this.comboBoxNetLink.TabIndex = 5;
            this.comboBoxNetLink.TabStop = false;
            this.comboBoxNetLink.Text = "empty link";
            this.comboBoxNetLink.SelectedIndexChanged += new System.EventHandler(this.comboBoxNetLink_SelectedIndexChanged);
            this.comboBoxNetLink.MouseHover += new System.EventHandler(this.ComboBoxNetLink_MouseHover);
            // 
            // labelResult
            // 
            this.labelResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult.Location = new System.Drawing.Point(619, 83);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(107, 27);
            this.labelResult.TabIndex = 4;
            this.labelResult.Text = "................";
            this.labelResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonUnRegister
            // 
            this.buttonUnRegister.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUnRegister.Location = new System.Drawing.Point(12, 114);
            this.buttonUnRegister.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonUnRegister.Name = "buttonUnRegister";
            this.buttonUnRegister.Size = new System.Drawing.Size(133, 34);
            this.buttonUnRegister.TabIndex = 1;
            this.buttonUnRegister.TabStop = false;
            this.buttonUnRegister.Text = "Unregister DLL";
            this.buttonUnRegister.UseVisualStyleBackColor = true;
            this.buttonUnRegister.Click += new System.EventHandler(this.ButtonUnRegister_Click);
            // 
            // buttonRegister
            // 
            this.buttonRegister.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRegister.Location = new System.Drawing.Point(12, 76);
            this.buttonRegister.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonRegister.Name = "buttonRegister";
            this.buttonRegister.Size = new System.Drawing.Size(133, 34);
            this.buttonRegister.TabIndex = 0;
            this.buttonRegister.TabStop = false;
            this.buttonRegister.Text = "Register DLL";
            this.buttonRegister.UseVisualStyleBackColor = true;
            this.buttonRegister.Click += new System.EventHandler(this.ButtonRegister_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.toolStripMenuItemRegedit,
            this.bitToolStripMenuItem,
            this.viewRegFileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(739, 28);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // systemToolStripMenuItem
            // 
            this.systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveInstallLogToolStripMenuItem,
            this.toolStripSeparator2});
            this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
            this.systemToolStripMenuItem.Size = new System.Drawing.Size(145, 24);
            this.systemToolStripMenuItem.Text = "Load / Save to xml";
            this.systemToolStripMenuItem.ToolTipText = "The project can be saved or loaded as an xml file";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // saveInstallLogToolStripMenuItem
            // 
            this.saveInstallLogToolStripMenuItem.Name = "saveInstallLogToolStripMenuItem";
            this.saveInstallLogToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.saveInstallLogToolStripMenuItem.Text = "Save install log";
            this.saveInstallLogToolStripMenuItem.Visible = false;
            this.saveInstallLogToolStripMenuItem.Click += new System.EventHandler(this.SaveInstallLogToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.infoToolStripMenuItem.Image = global::DllRegister.Properties.Resources.user1;
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(67, 24);
            this.infoToolStripMenuItem.Text = "Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.InfoToolStripMenuItem_Click);
            // 
            // toolStripMenuItemRegedit
            // 
            this.toolStripMenuItemRegedit.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.toolStripMenuItemRegedit.Name = "toolStripMenuItemRegedit";
            this.toolStripMenuItemRegedit.Size = new System.Drawing.Size(139, 24);
            this.toolStripMenuItemRegedit.Text = "Open RegEdit.exe";
            this.toolStripMenuItemRegedit.ToolTipText = "Open Regeditor.exe";
            this.toolStripMenuItemRegedit.Click += new System.EventHandler(this.ToolStripMenuItemRegedit_Click);
            // 
            // bitToolStripMenuItem
            // 
            this.bitToolStripMenuItem.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.bitToolStripMenuItem.Name = "bitToolStripMenuItem";
            this.bitToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.bitToolStripMenuItem.Text = "64bit";
            this.bitToolStripMenuItem.ToolTipText = "Open Regeditor.exe";
            this.bitToolStripMenuItem.Click += new System.EventHandler(this.BitToolStripMenuItem_Click);
            // 
            // viewRegFileToolStripMenuItem
            // 
            this.viewRegFileToolStripMenuItem.AutoToolTip = true;
            this.viewRegFileToolStripMenuItem.BackColor = System.Drawing.Color.Bisque;
            this.viewRegFileToolStripMenuItem.Name = "viewRegFileToolStripMenuItem";
            this.viewRegFileToolStripMenuItem.Size = new System.Drawing.Size(110, 24);
            this.viewRegFileToolStripMenuItem.Text = "View Reg File";
            this.viewRegFileToolStripMenuItem.ToolTipText = "View the registry build file";
            this.viewRegFileToolStripMenuItem.Click += new System.EventHandler(this.viewRegFileToolStripMenuItem_Click);
            // 
            // FileListcomboBox
            // 
            this.FileListcomboBox.AllowDrop = true;
            this.FileListcomboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileListcomboBox.ContextMenuStrip = this.FilelistContextMenuStrip;
            this.FileListcomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FileListcomboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileListcomboBox.FormattingEnabled = true;
            this.FileListcomboBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.FileListcomboBox.Location = new System.Drawing.Point(151, 183);
            this.FileListcomboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.FileListcomboBox.MaxDropDownItems = 15;
            this.FileListcomboBox.Name = "FileListcomboBox";
            this.FileListcomboBox.Size = new System.Drawing.Size(576, 28);
            this.FileListcomboBox.Sorted = true;
            this.FileListcomboBox.TabIndex = 8;
            this.FileListcomboBox.TabStop = false;
            this.FileListcomboBox.SelectedIndexChanged += new System.EventHandler(this.FileListcomboBox_SelectedIndexChanged);
            this.FileListcomboBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Button2_MouseDown);
            this.FileListcomboBox.MouseHover += new System.EventHandler(this.FileListcomboBox_MouseHover);
            // 
            // FilelistContextMenuStrip
            // 
            this.FilelistContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.FilelistContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.removeAllToolStripMenuItem,
            this.toolStripSeparator3,
            this.openPathToolStripMenuItem});
            this.FilelistContextMenuStrip.Name = "FilelistContextMenuStrip";
            this.FilelistContextMenuStrip.Size = new System.Drawing.Size(153, 106);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.deleteToolStripMenuItem.Text = "Remove";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.removeAllToolStripMenuItem.Text = "Remove all";
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.RemoveAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
            // 
            // openPathToolStripMenuItem
            // 
            this.openPathToolStripMenuItem.Name = "openPathToolStripMenuItem";
            this.openPathToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.openPathToolStripMenuItem.Text = "Open path";
            this.openPathToolStripMenuItem.Click += new System.EventHandler(this.openPathToolStripMenuItem_Click);
            // 
            // fct_box
            // 
            this.fct_box.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.fct_box.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
            this.fct_box.AutoScrollMinSize = new System.Drawing.Size(31, 18);
            this.fct_box.BackBrush = null;
            this.fct_box.CharHeight = 18;
            this.fct_box.CharWidth = 10;
            this.fct_box.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fct_box.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fct_box.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fct_box.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.fct_box.IsReplaceMode = false;
            this.fct_box.Location = new System.Drawing.Point(0, 0);
            this.fct_box.Name = "fct_box";
            this.fct_box.Paddings = new System.Windows.Forms.Padding(0);
            this.fct_box.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fct_box.ServiceColors = null;
            this.fct_box.Size = new System.Drawing.Size(739, 275);
            this.fct_box.TabIndex = 18;
            this.fct_box.Zoom = 100;
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // MainForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(739, 509);
            this.Controls.Add(this.splitContainer1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(755, 448);
            this.Name = "MainForm";
            this.Text = "(Un)Register DLL";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.FilelistContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fct_box)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonUnRegister;
        private System.Windows.Forms.Button buttonRegister;
        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.ComboBox comboBoxNetLink;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem systemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ComboBox FileListcomboBox;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip FilelistContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxRegistry;
        private System.Windows.Forms.CheckBox checkBoxInstallinGAC;
        private System.Windows.Forms.CheckBox checkBoxCodebase;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem saveInstallLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxOutPath;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRegedit;
        private System.Windows.Forms.ToolStripMenuItem bitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem openPathToolStripMenuItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxPName;
        private FastColoredTextBoxNS.FastColoredTextBox fct_box;
        private System.Windows.Forms.ToolStripMenuItem viewRegFileToolStripMenuItem;
    }
}

