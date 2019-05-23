namespace GCodeInterpreter
{
    partial class InterpreterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InterpreterForm));
            this.dOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.dSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkCollisionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.generateReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.goToLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.alwaysPrintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.singleLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromLastLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripConnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripDisconnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripNextLine = new System.Windows.Forms.ToolStripButton();
            this.toolStripSendLine = new System.Windows.Forms.ToolStripButton();
            this.toolStripFromLast = new System.Windows.Forms.ToolStripButton();
            this.toolStripSendAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLastRead = new System.Windows.Forms.ToolStripLabel();
            this.lGCodeView = new System.Windows.Forms.ListView();
            this.Line = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.infoLabel = new System.Windows.Forms.Label();
            this.collisionLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dOpenFile
            // 
            this.dOpenFile.FileOk += new System.ComponentModel.CancelEventHandler(this.dFile_FileOk);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.connectionToolStripMenuItem,
            this.sendToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(581, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.clearToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.ToolTipText = "Select a gcode file to open.";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("clearToolStripMenuItem.Image")));
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.ToolTipText = "Clear current gcode loaded.";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(144, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.saveToolStripMenuItem.Text = "Save...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(144, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exitToolStripMenuItem.Image")));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkCollisionsToolStripMenuItem,
            this.toolStripSeparator8,
            this.generateReportToolStripMenuItem,
            this.toolStripSeparator3,
            this.goToLineToolStripMenuItem,
            this.highlightLineToolStripMenuItem,
            this.toolStripSeparator9,
            this.alwaysPrintToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // checkCollisionsToolStripMenuItem
            // 
            this.checkCollisionsToolStripMenuItem.Name = "checkCollisionsToolStripMenuItem";
            this.checkCollisionsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.checkCollisionsToolStripMenuItem.Text = "Check collisions";
            this.checkCollisionsToolStripMenuItem.Click += new System.EventHandler(this.checkCollisionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(186, 6);
            // 
            // generateReportToolStripMenuItem
            // 
            this.generateReportToolStripMenuItem.Enabled = false;
            this.generateReportToolStripMenuItem.Name = "generateReportToolStripMenuItem";
            this.generateReportToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.generateReportToolStripMenuItem.Text = "Generate Report";
            this.generateReportToolStripMenuItem.Click += new System.EventHandler(this.generateReportToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(186, 6);
            // 
            // goToLineToolStripMenuItem
            // 
            this.goToLineToolStripMenuItem.Enabled = false;
            this.goToLineToolStripMenuItem.Name = "goToLineToolStripMenuItem";
            this.goToLineToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.goToLineToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.goToLineToolStripMenuItem.Text = "Go to line...";
            this.goToLineToolStripMenuItem.ToolTipText = "Highlights the specified line.";
            this.goToLineToolStripMenuItem.Click += new System.EventHandler(this.goToLineToolStripMenuItem_Click);
            // 
            // highlightLineToolStripMenuItem
            // 
            this.highlightLineToolStripMenuItem.CheckOnClick = true;
            this.highlightLineToolStripMenuItem.Name = "highlightLineToolStripMenuItem";
            this.highlightLineToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.highlightLineToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.highlightLineToolStripMenuItem.Text = "Highlight line";
            this.highlightLineToolStripMenuItem.ToolTipText = "When checked the cursor will highlight the current line being executed by the sim" +
    "ulator.";
            this.highlightLineToolStripMenuItem.CheckedChanged += new System.EventHandler(this.highlightLineToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(186, 6);
            // 
            // alwaysPrintToolStripMenuItem
            // 
            this.alwaysPrintToolStripMenuItem.CheckOnClick = true;
            this.alwaysPrintToolStripMenuItem.Name = "alwaysPrintToolStripMenuItem";
            this.alwaysPrintToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.alwaysPrintToolStripMenuItem.Text = "Always print";
            this.alwaysPrintToolStripMenuItem.ToolTipText = "When checked will add a print instruction to every sent line.";
            this.alwaysPrintToolStripMenuItem.Click += new System.EventHandler(this.alwaysPrintToolStripMenuItem_Click);
            // 
            // connectionToolStripMenuItem
            // 
            this.connectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem});
            this.connectionToolStripMenuItem.Name = "connectionToolStripMenuItem";
            this.connectionToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.connectionToolStripMenuItem.Text = "Connection";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Enabled = false;
            this.connectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("connectToolStripMenuItem.Image")));
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C)));
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.ToolTipText = "Establish connection to simulator.";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Enabled = false;
            this.disconnectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("disconnectToolStripMenuItem.Image")));
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D)));
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            this.disconnectToolStripMenuItem.ToolTipText = "Disconnect from simulator.";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
            // 
            // sendToolStripMenuItem
            // 
            this.sendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nextLineToolStripMenuItem,
            this.toolStripSeparator5,
            this.singleLineToolStripMenuItem,
            this.fromLastLineToolStripMenuItem,
            this.allLinesToolStripMenuItem});
            this.sendToolStripMenuItem.Name = "sendToolStripMenuItem";
            this.sendToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.sendToolStripMenuItem.Text = "Send";
            // 
            // nextLineToolStripMenuItem
            // 
            this.nextLineToolStripMenuItem.Enabled = false;
            this.nextLineToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("nextLineToolStripMenuItem.Image")));
            this.nextLineToolStripMenuItem.Name = "nextLineToolStripMenuItem";
            this.nextLineToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.nextLineToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.nextLineToolStripMenuItem.Text = "Next Line";
            this.nextLineToolStripMenuItem.ToolTipText = "Sends the parsed line immediately after the last executed from the simulator.";
            this.nextLineToolStripMenuItem.Click += new System.EventHandler(this.nextLineToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(161, 6);
            // 
            // singleLineToolStripMenuItem
            // 
            this.singleLineToolStripMenuItem.Enabled = false;
            this.singleLineToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("singleLineToolStripMenuItem.Image")));
            this.singleLineToolStripMenuItem.Name = "singleLineToolStripMenuItem";
            this.singleLineToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.singleLineToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.singleLineToolStripMenuItem.Text = "Single line";
            this.singleLineToolStripMenuItem.ToolTipText = "Sends the clicked lined to be executed by the simulator.";
            this.singleLineToolStripMenuItem.Click += new System.EventHandler(this.singleLineToolStripMenuItem_Click);
            // 
            // fromLastLineToolStripMenuItem
            // 
            this.fromLastLineToolStripMenuItem.Enabled = false;
            this.fromLastLineToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("fromLastLineToolStripMenuItem.Image")));
            this.fromLastLineToolStripMenuItem.Name = "fromLastLineToolStripMenuItem";
            this.fromLastLineToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.fromLastLineToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.fromLastLineToolStripMenuItem.Text = "From last line";
            this.fromLastLineToolStripMenuItem.ToolTipText = "Sends the lines since the last executed by the simulator to the clicked line.";
            this.fromLastLineToolStripMenuItem.Click += new System.EventHandler(this.fromLastLineToolStripMenuItem_Click);
            // 
            // allLinesToolStripMenuItem
            // 
            this.allLinesToolStripMenuItem.Enabled = false;
            this.allLinesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("allLinesToolStripMenuItem.Image")));
            this.allLinesToolStripMenuItem.Name = "allLinesToolStripMenuItem";
            this.allLinesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.allLinesToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.allLinesToolStripMenuItem.Text = "All lines";
            this.allLinesToolStripMenuItem.ToolTipText = "Sends all lines since the last executed line.";
            this.allLinesToolStripMenuItem.Click += new System.EventHandler(this.allLinesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.CanOverflow = false;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripOpen,
            this.toolStripClear,
            this.toolStripSeparator4,
            this.toolStripConnect,
            this.toolStripDisconnect,
            this.toolStripSeparator6,
            this.toolStripNextLine,
            this.toolStripSendLine,
            this.toolStripFromLast,
            this.toolStripSendAll,
            this.toolStripSeparator7,
            this.toolStripLabel1,
            this.toolStripLastRead});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(581, 55);
            this.toolStrip1.TabIndex = 15;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripOpen
            // 
            this.toolStripOpen.AutoSize = false;
            this.toolStripOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripOpen.Image")));
            this.toolStripOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripOpen.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripOpen.Name = "toolStripOpen";
            this.toolStripOpen.Size = new System.Drawing.Size(48, 48);
            this.toolStripOpen.Text = "toolStripButton1";
            this.toolStripOpen.ToolTipText = "Select a gcode file to open.";
            this.toolStripOpen.Click += new System.EventHandler(this.toolStripOpen_Click);
            // 
            // toolStripClear
            // 
            this.toolStripClear.AutoSize = false;
            this.toolStripClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripClear.Image = ((System.Drawing.Image)(resources.GetObject("toolStripClear.Image")));
            this.toolStripClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripClear.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripClear.Name = "toolStripClear";
            this.toolStripClear.Size = new System.Drawing.Size(48, 48);
            this.toolStripClear.Text = "toolStripButton2";
            this.toolStripClear.ToolTipText = "Clear current gcode loaded.";
            this.toolStripClear.Click += new System.EventHandler(this.toolStripClear_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 55);
            // 
            // toolStripConnect
            // 
            this.toolStripConnect.AutoSize = false;
            this.toolStripConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripConnect.Enabled = false;
            this.toolStripConnect.Image = ((System.Drawing.Image)(resources.GetObject("toolStripConnect.Image")));
            this.toolStripConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripConnect.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripConnect.Name = "toolStripConnect";
            this.toolStripConnect.Size = new System.Drawing.Size(48, 48);
            this.toolStripConnect.Text = "toolStripButton3";
            this.toolStripConnect.ToolTipText = "Establish connection to simulator.";
            this.toolStripConnect.Click += new System.EventHandler(this.toolStripConnect_Click);
            // 
            // toolStripDisconnect
            // 
            this.toolStripDisconnect.AutoSize = false;
            this.toolStripDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDisconnect.Enabled = false;
            this.toolStripDisconnect.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDisconnect.Image")));
            this.toolStripDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDisconnect.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripDisconnect.Name = "toolStripDisconnect";
            this.toolStripDisconnect.Size = new System.Drawing.Size(48, 48);
            this.toolStripDisconnect.Text = "toolStripButton4";
            this.toolStripDisconnect.ToolTipText = "Disconnect from simulator.";
            this.toolStripDisconnect.Click += new System.EventHandler(this.toolStripDisconnect_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 55);
            // 
            // toolStripNextLine
            // 
            this.toolStripNextLine.AutoSize = false;
            this.toolStripNextLine.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.toolStripNextLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripNextLine.Enabled = false;
            this.toolStripNextLine.ForeColor = System.Drawing.SystemColors.GrayText;
            this.toolStripNextLine.Image = ((System.Drawing.Image)(resources.GetObject("toolStripNextLine.Image")));
            this.toolStripNextLine.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.toolStripNextLine.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripNextLine.Name = "toolStripNextLine";
            this.toolStripNextLine.Size = new System.Drawing.Size(48, 48);
            this.toolStripNextLine.Text = "toolStripButton1";
            this.toolStripNextLine.ToolTipText = "Sends the parsed line immediately after the last executed from the simulator.";
            this.toolStripNextLine.Click += new System.EventHandler(this.toolStripNextLine_Click);
            // 
            // toolStripSendLine
            // 
            this.toolStripSendLine.AutoSize = false;
            this.toolStripSendLine.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripSendLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSendLine.Enabled = false;
            this.toolStripSendLine.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.toolStripSendLine.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSendLine.Image")));
            this.toolStripSendLine.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.toolStripSendLine.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripSendLine.Name = "toolStripSendLine";
            this.toolStripSendLine.Size = new System.Drawing.Size(48, 48);
            this.toolStripSendLine.Text = "toolStripButton2";
            this.toolStripSendLine.ToolTipText = "Sends the clicked lined to be executed by the simulator.";
            this.toolStripSendLine.Click += new System.EventHandler(this.toolStripSendLine_Click);
            // 
            // toolStripFromLast
            // 
            this.toolStripFromLast.AutoSize = false;
            this.toolStripFromLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripFromLast.Enabled = false;
            this.toolStripFromLast.Image = ((System.Drawing.Image)(resources.GetObject("toolStripFromLast.Image")));
            this.toolStripFromLast.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.toolStripFromLast.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripFromLast.Name = "toolStripFromLast";
            this.toolStripFromLast.Size = new System.Drawing.Size(48, 48);
            this.toolStripFromLast.Text = "toolStripButton3";
            this.toolStripFromLast.ToolTipText = "Sends the lines since the last executed by the simulator to the clicked line.";
            this.toolStripFromLast.Click += new System.EventHandler(this.toolStripFromLast_Click);
            // 
            // toolStripSendAll
            // 
            this.toolStripSendAll.AutoSize = false;
            this.toolStripSendAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSendAll.Enabled = false;
            this.toolStripSendAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSendAll.Image")));
            this.toolStripSendAll.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.toolStripSendAll.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripSendAll.MergeIndex = 0;
            this.toolStripSendAll.Name = "toolStripSendAll";
            this.toolStripSendAll.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripSendAll.Size = new System.Drawing.Size(48, 48);
            this.toolStripSendAll.Text = "toolStripButton4";
            this.toolStripSendAll.ToolTipText = "Sends all lines since the last executed line.";
            this.toolStripSendAll.Click += new System.EventHandler(this.toolStripSendAll_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 55);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(47, 52);
            this.toolStripLabel1.Text = "Line";
            // 
            // toolStripLastRead
            // 
            this.toolStripLastRead.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLastRead.Name = "toolStripLastRead";
            this.toolStripLastRead.Size = new System.Drawing.Size(29, 52);
            this.toolStripLastRead.Text = "--";
            this.toolStripLastRead.ToolTipText = "Last line executed by the simulator.";
            // 
            // lGCodeView
            // 
            this.lGCodeView.AllowDrop = true;
            this.lGCodeView.AutoArrange = false;
            this.lGCodeView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Line,
            this.GCode});
            this.lGCodeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lGCodeView.FullRowSelect = true;
            this.lGCodeView.GridLines = true;
            this.lGCodeView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lGCodeView.LabelWrap = false;
            this.lGCodeView.Location = new System.Drawing.Point(12, 82);
            this.lGCodeView.MultiSelect = false;
            this.lGCodeView.Name = "lGCodeView";
            this.lGCodeView.Size = new System.Drawing.Size(395, 391);
            this.lGCodeView.TabIndex = 2;
            this.lGCodeView.UseCompatibleStateImageBehavior = false;
            this.lGCodeView.View = System.Windows.Forms.View.Details;
            this.lGCodeView.VirtualListSize = 10000000;
            this.lGCodeView.VirtualMode = true;
            this.lGCodeView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lGCodeView_RetrieveVirtualItem);
            this.lGCodeView.SelectedIndexChanged += new System.EventHandler(this.lGCodeView_SelectedIndexChanged);
            this.lGCodeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.lGCodeView_DragDrop);
            this.lGCodeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.lGCodeView_DragEnter);
            // 
            // Line
            // 
            this.Line.Text = "Line";
            this.Line.Width = 75;
            // 
            // GCode
            // 
            this.GCode.Text = "GCode";
            this.GCode.Width = 500;
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(413, 82);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(155, 391);
            this.treeView1.TabIndex = 17;
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.infoLabel.BackColor = System.Drawing.SystemColors.Info;
            this.infoLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.infoLabel.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.infoLabel.Location = new System.Drawing.Point(12, 479);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(395, 29);
            this.infoLabel.TabIndex = 18;
            this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // collisionLabel
            // 
            this.collisionLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.collisionLabel.BackColor = System.Drawing.SystemColors.Info;
            this.collisionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.collisionLabel.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.collisionLabel.Location = new System.Drawing.Point(413, 479);
            this.collisionLabel.Name = "collisionLabel";
            this.collisionLabel.Size = new System.Drawing.Size(156, 29);
            this.collisionLabel.TabIndex = 19;
            this.collisionLabel.Text = "Collisions 0";
            this.collisionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.collisionLabel.Click += new System.EventHandler(this.collisionLabel_Click);
            // 
            // InterpreterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(581, 517);
            this.Controls.Add(this.collisionLabel);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.lGCodeView);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(597, 446);
            this.Name = "InterpreterForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Interpreter";
            this.TransparencyKey = System.Drawing.SystemColors.ActiveBorder;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog dOpenFile;
        private System.Windows.Forms.SaveFileDialog dSaveFile;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem highlightLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripOpen;
        private System.Windows.Forms.ToolStripButton toolStripClear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem connectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem singleLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromLastLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripDisconnect;
        private System.Windows.Forms.ToolStripButton toolStripConnect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton toolStripNextLine;
        private System.Windows.Forms.ToolStripButton toolStripSendLine;
        private System.Windows.Forms.ToolStripButton toolStripFromLast;
        private System.Windows.Forms.ToolStripButton toolStripSendAll;
        private System.Windows.Forms.ToolStripLabel toolStripLastRead;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ListView lGCodeView;
        private System.Windows.Forms.ColumnHeader Line;
        private System.Windows.Forms.ColumnHeader GCode;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.ToolStripMenuItem checkCollisionsToolStripMenuItem;
        private System.Windows.Forms.Label collisionLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem generateReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysPrintToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
    }
}

