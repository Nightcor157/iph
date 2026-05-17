<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        gbSelectDBType = New GroupBox()
        gbOptions = New GroupBox()
        rbtnJSON = New RadioButton()
        chkEUFormat = New CheckBox()
        rbtnSQLiteDB = New RadioButton()
        rbtnSQLServer = New RadioButton()
        rbtnAccess = New RadioButton()
        rbtnPostgreSQL = New RadioButton()
        rbtnCSV = New RadioButton()
        lblServerName = New Label()
        rbtnMySQL = New RadioButton()
        txtPort = New TextBox()
        txtServerName = New TextBox()
        lblPort = New Label()
        txtDBName = New TextBox()
        lblDBName = New Label()
        txtPassword = New TextBox()
        txtUserName = New TextBox()
        lblUserName = New Label()
        lblPassword = New Label()
        gbLanguage = New GroupBox()
        rbtnSpanish = New RadioButton()
        rbtnKorean = New RadioButton()
        rbtnChinese = New RadioButton()
        rbtnRussian = New RadioButton()
        rbtnJapanese = New RadioButton()
        rbtnFrench = New RadioButton()
        rbtnGerman = New RadioButton()
        rbtnEnglish = New RadioButton()
        MenuStrip1 = New MenuStrip()
        FileToolStripMenuItem = New ToolStripMenuItem()
        ResetSavedBuildNumberToolStripMenuItem = New ToolStripMenuItem()
        ExitToolStripMenuItem = New ToolStripMenuItem()
        AboutToolStripMenuItem = New ToolStripMenuItem()
        CheckForUpdatesToolStripMenuItem = New ToolStripMenuItem()
        AboutToolStripMenuItem1 = New ToolStripMenuItem()
        DeveloperToolStripMenuItem = New ToolStripMenuItem()
        PrepareFilesForUpdateToolStripMenuItem = New ToolStripMenuItem()
        BuildBinaryToolStripMenuItem = New ToolStripMenuItem()
        gbFilePathSelect = New GroupBox()
        pgBar = New ProgressBar()
        btnCancelDownload = New Button()
        btnDownloadSDE = New Button()
        lblDownload = New Label()
        lblDownloadFolderPath = New Label()
        btnSelectDownloadPath = New Button()
        btnCheckNoGridItems = New Button()
        btnCheckAllGridItems = New Button()
        lblMediaFire = New Label()
        btnSelectFinalDBPath = New Button()
        lblSDEPath = New Label()
        lblFinalDBFolder = New Label()
        lblFinalDBPath = New Label()
        btnSelectSDEPath = New Button()
        TableLayoutPanel1 = New TableLayoutPanel()
        dgMain = New DataGridView()
        FileSelect = New DataGridViewCheckBoxColumn()
        FileName = New DataGridViewTextBoxColumn()
        Progress = New DataGridViewTextBoxColumn()
        btnClose = New Button()
        btnCancel = New Button()
        btnBuildDatabase = New Button()
        btnSaveSettings = New Button()
        pnlMain = New StatusStrip()
        lblStatus = New ToolStripStatusLabel()
        pgMain = New ToolStripProgressBar()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        FBDialog = New FolderBrowserDialog()
        ToolTip1 = New ToolTip(components)
        gbSelectDBType.SuspendLayout()
        gbOptions.SuspendLayout()
        gbLanguage.SuspendLayout()
        MenuStrip1.SuspendLayout()
        gbFilePathSelect.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        CType(dgMain, ComponentModel.ISupportInitialize).BeginInit()
        pnlMain.SuspendLayout()
        SuspendLayout()
        ' 
        ' gbSelectDBType
        ' 
        gbSelectDBType.BackColor = Color.Transparent
        gbSelectDBType.Controls.Add(gbOptions)
        gbSelectDBType.Controls.Add(gbLanguage)
        gbSelectDBType.Location = New Point(10, 31)
        gbSelectDBType.Name = "gbSelectDBType"
        gbSelectDBType.Size = New Size(702, 177)
        gbSelectDBType.TabIndex = 24
        gbSelectDBType.TabStop = False
        gbSelectDBType.Text = "Select Options:"
        ' 
        ' gbOptions
        ' 
        gbOptions.Controls.Add(rbtnJSON)
        gbOptions.Controls.Add(chkEUFormat)
        gbOptions.Controls.Add(rbtnSQLiteDB)
        gbOptions.Controls.Add(rbtnSQLServer)
        gbOptions.Controls.Add(rbtnAccess)
        gbOptions.Controls.Add(rbtnPostgreSQL)
        gbOptions.Controls.Add(rbtnCSV)
        gbOptions.Controls.Add(lblServerName)
        gbOptions.Controls.Add(rbtnMySQL)
        gbOptions.Controls.Add(txtPort)
        gbOptions.Controls.Add(txtServerName)
        gbOptions.Controls.Add(lblPort)
        gbOptions.Controls.Add(txtDBName)
        gbOptions.Controls.Add(lblDBName)
        gbOptions.Controls.Add(txtPassword)
        gbOptions.Controls.Add(txtUserName)
        gbOptions.Controls.Add(lblUserName)
        gbOptions.Controls.Add(lblPassword)
        gbOptions.Location = New Point(6, 22)
        gbOptions.Name = "gbOptions"
        gbOptions.Size = New Size(497, 149)
        gbOptions.TabIndex = 0
        gbOptions.TabStop = False
        gbOptions.Text = "Database Type:"
        ' 
        ' rbtnJSON
        ' 
        rbtnJSON.AutoSize = True
        rbtnJSON.Location = New Point(13, 117)
        rbtnJSON.Name = "rbtnJSON"
        rbtnJSON.Size = New Size(62, 24)
        rbtnJSON.TabIndex = 18
        rbtnJSON.Text = "JSON"
        rbtnJSON.UseVisualStyleBackColor = True
        ' 
        ' chkEUFormat
        ' 
        chkEUFormat.AutoSize = True
        chkEUFormat.Location = New Point(152, 117)
        chkEUFormat.Name = "chkEUFormat"
        chkEUFormat.Size = New Size(142, 24)
        chkEUFormat.TabIndex = 6
        chkEUFormat.Text = "European Format"
        chkEUFormat.UseVisualStyleBackColor = True
        ' 
        ' rbtnSQLiteDB
        ' 
        rbtnSQLiteDB.AutoSize = True
        rbtnSQLiteDB.Location = New Point(13, 21)
        rbtnSQLiteDB.Name = "rbtnSQLiteDB"
        rbtnSQLiteDB.Size = New Size(70, 24)
        rbtnSQLiteDB.TabIndex = 0
        rbtnSQLiteDB.Text = "SQLite"
        rbtnSQLiteDB.UseVisualStyleBackColor = True
        ' 
        ' rbtnSQLServer
        ' 
        rbtnSQLServer.AutoSize = True
        rbtnSQLServer.Location = New Point(13, 93)
        rbtnSQLServer.Name = "rbtnSQLServer"
        rbtnSQLServer.Size = New Size(165, 24)
        rbtnSQLServer.TabIndex = 1
        rbtnSQLServer.Text = "Microsoft SQL Server"
        rbtnSQLServer.UseVisualStyleBackColor = True
        ' 
        ' rbtnAccess
        ' 
        rbtnAccess.AutoSize = True
        rbtnAccess.Location = New Point(13, 69)
        rbtnAccess.Name = "rbtnAccess"
        rbtnAccess.Size = New Size(138, 24)
        rbtnAccess.TabIndex = 4
        rbtnAccess.Text = "Microsoft Access"
        rbtnAccess.UseVisualStyleBackColor = True
        ' 
        ' rbtnPostgreSQL
        ' 
        rbtnPostgreSQL.AutoSize = True
        rbtnPostgreSQL.Location = New Point(13, 45)
        rbtnPostgreSQL.Name = "rbtnPostgreSQL"
        rbtnPostgreSQL.Size = New Size(102, 24)
        rbtnPostgreSQL.TabIndex = 3
        rbtnPostgreSQL.Text = "PostgreSQL"
        rbtnPostgreSQL.UseVisualStyleBackColor = True
        ' 
        ' rbtnCSV
        ' 
        rbtnCSV.AutoSize = True
        rbtnCSV.Location = New Point(89, 117)
        rbtnCSV.Name = "rbtnCSV"
        rbtnCSV.Size = New Size(53, 24)
        rbtnCSV.TabIndex = 5
        rbtnCSV.Text = "CSV"
        rbtnCSV.UseVisualStyleBackColor = True
        ' 
        ' lblServerName
        ' 
        lblServerName.Location = New Point(174, 52)
        lblServerName.Name = "lblServerName"
        lblServerName.Size = New Size(120, 20)
        lblServerName.TabIndex = 17
        lblServerName.Text = "Instance Name:"
        lblServerName.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' rbtnMySQL
        ' 
        rbtnMySQL.AutoSize = True
        rbtnMySQL.Location = New Point(89, 21)
        rbtnMySQL.Name = "rbtnMySQL"
        rbtnMySQL.Size = New Size(73, 24)
        rbtnMySQL.TabIndex = 2
        rbtnMySQL.Text = "MySQL"
        rbtnMySQL.UseVisualStyleBackColor = True
        ' 
        ' txtPort
        ' 
        txtPort.Location = New Point(451, 113)
        txtPort.Name = "txtPort"
        txtPort.Size = New Size(40, 27)
        txtPort.TabIndex = 16
        txtPort.Text = "9999"
        txtPort.TextAlign = HorizontalAlignment.Center
        ' 
        ' txtServerName
        ' 
        txtServerName.Location = New Point(300, 51)
        txtServerName.Name = "txtServerName"
        txtServerName.Size = New Size(191, 27)
        txtServerName.TabIndex = 10
        ' 
        ' lblPort
        ' 
        lblPort.AutoSize = True
        lblPort.Location = New Point(454, 90)
        lblPort.Name = "lblPort"
        lblPort.Size = New Size(38, 20)
        lblPort.TabIndex = 15
        lblPort.Text = "Port:"
        ' 
        ' txtDBName
        ' 
        txtDBName.Location = New Point(300, 20)
        txtDBName.Name = "txtDBName"
        txtDBName.Size = New Size(191, 27)
        txtDBName.TabIndex = 8
        ' 
        ' lblDBName
        ' 
        lblDBName.Location = New Point(175, 23)
        lblDBName.Name = "lblDBName"
        lblDBName.Size = New Size(119, 20)
        lblDBName.TabIndex = 7
        lblDBName.Text = "Database Name:"
        lblDBName.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' txtPassword
        ' 
        txtPassword.Location = New Point(300, 113)
        txtPassword.Name = "txtPassword"
        txtPassword.PasswordChar = "*"c
        txtPassword.Size = New Size(147, 27)
        txtPassword.TabIndex = 14
        ' 
        ' txtUserName
        ' 
        txtUserName.Location = New Point(300, 82)
        txtUserName.Name = "txtUserName"
        txtUserName.Size = New Size(147, 27)
        txtUserName.TabIndex = 12
        ' 
        ' lblUserName
        ' 
        lblUserName.Location = New Point(213, 85)
        lblUserName.Name = "lblUserName"
        lblUserName.Size = New Size(81, 20)
        lblUserName.TabIndex = 11
        lblUserName.Text = "UserName:"
        lblUserName.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' lblPassword
        ' 
        lblPassword.Location = New Point(221, 116)
        lblPassword.Name = "lblPassword"
        lblPassword.Size = New Size(73, 20)
        lblPassword.TabIndex = 13
        lblPassword.Text = "Password:"
        lblPassword.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' gbLanguage
        ' 
        gbLanguage.Controls.Add(rbtnSpanish)
        gbLanguage.Controls.Add(rbtnKorean)
        gbLanguage.Controls.Add(rbtnChinese)
        gbLanguage.Controls.Add(rbtnRussian)
        gbLanguage.Controls.Add(rbtnJapanese)
        gbLanguage.Controls.Add(rbtnFrench)
        gbLanguage.Controls.Add(rbtnGerman)
        gbLanguage.Controls.Add(rbtnEnglish)
        gbLanguage.Location = New Point(509, 22)
        gbLanguage.Name = "gbLanguage"
        gbLanguage.Size = New Size(187, 149)
        gbLanguage.TabIndex = 1
        gbLanguage.TabStop = False
        gbLanguage.Text = "Language:"
        ' 
        ' rbtnSpanish
        ' 
        rbtnSpanish.AutoSize = True
        rbtnSpanish.Location = New Point(94, 110)
        rbtnSpanish.Name = "rbtnSpanish"
        rbtnSpanish.Size = New Size(78, 24)
        rbtnSpanish.TabIndex = 7
        rbtnSpanish.Text = "Spanish"
        rbtnSpanish.UseVisualStyleBackColor = True
        ' 
        ' rbtnKorean
        ' 
        rbtnKorean.AutoSize = True
        rbtnKorean.Location = New Point(12, 110)
        rbtnKorean.Name = "rbtnKorean"
        rbtnKorean.Size = New Size(74, 24)
        rbtnKorean.TabIndex = 6
        rbtnKorean.Text = "Korean"
        rbtnKorean.UseVisualStyleBackColor = True
        ' 
        ' rbtnChinese
        ' 
        rbtnChinese.AutoSize = True
        rbtnChinese.Location = New Point(94, 80)
        rbtnChinese.Name = "rbtnChinese"
        rbtnChinese.Size = New Size(78, 24)
        rbtnChinese.TabIndex = 5
        rbtnChinese.Text = "Chinese"
        rbtnChinese.UseVisualStyleBackColor = True
        ' 
        ' rbtnRussian
        ' 
        rbtnRussian.AutoSize = True
        rbtnRussian.Location = New Point(12, 80)
        rbtnRussian.Name = "rbtnRussian"
        rbtnRussian.Size = New Size(76, 24)
        rbtnRussian.TabIndex = 4
        rbtnRussian.Text = "Russian"
        rbtnRussian.UseVisualStyleBackColor = True
        ' 
        ' rbtnJapanese
        ' 
        rbtnJapanese.AutoSize = True
        rbtnJapanese.Location = New Point(94, 50)
        rbtnJapanese.Name = "rbtnJapanese"
        rbtnJapanese.Size = New Size(87, 24)
        rbtnJapanese.TabIndex = 3
        rbtnJapanese.Text = "Japanese"
        rbtnJapanese.UseVisualStyleBackColor = True
        ' 
        ' rbtnFrench
        ' 
        rbtnFrench.AutoSize = True
        rbtnFrench.Location = New Point(12, 50)
        rbtnFrench.Name = "rbtnFrench"
        rbtnFrench.Size = New Size(70, 24)
        rbtnFrench.TabIndex = 2
        rbtnFrench.Text = "French"
        rbtnFrench.UseVisualStyleBackColor = True
        ' 
        ' rbtnGerman
        ' 
        rbtnGerman.AutoSize = True
        rbtnGerman.Location = New Point(94, 20)
        rbtnGerman.Name = "rbtnGerman"
        rbtnGerman.Size = New Size(79, 24)
        rbtnGerman.TabIndex = 1
        rbtnGerman.Text = "German"
        rbtnGerman.UseVisualStyleBackColor = True
        ' 
        ' rbtnEnglish
        ' 
        rbtnEnglish.AutoSize = True
        rbtnEnglish.Location = New Point(12, 20)
        rbtnEnglish.Name = "rbtnEnglish"
        rbtnEnglish.Size = New Size(74, 24)
        rbtnEnglish.TabIndex = 0
        rbtnEnglish.Text = "English"
        rbtnEnglish.UseVisualStyleBackColor = True
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {FileToolStripMenuItem, AboutToolStripMenuItem, DeveloperToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Size = New Size(723, 28)
        MenuStrip1.TabIndex = 25
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FileToolStripMenuItem
        ' 
        FileToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {ResetSavedBuildNumberToolStripMenuItem, ExitToolStripMenuItem})
        FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        FileToolStripMenuItem.Size = New Size(44, 24)
        FileToolStripMenuItem.Text = "File"
        ' 
        ' ResetSavedBuildNumberToolStripMenuItem
        ' 
        ResetSavedBuildNumberToolStripMenuItem.Name = "ResetSavedBuildNumberToolStripMenuItem"
        ResetSavedBuildNumberToolStripMenuItem.Size = New Size(226, 24)
        ResetSavedBuildNumberToolStripMenuItem.Text = "Reset SDE Build Check"
        ' 
        ' ExitToolStripMenuItem
        ' 
        ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        ExitToolStripMenuItem.Size = New Size(226, 24)
        ExitToolStripMenuItem.Text = "Exit"
        ' 
        ' AboutToolStripMenuItem
        ' 
        AboutToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {CheckForUpdatesToolStripMenuItem, AboutToolStripMenuItem1})
        AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        AboutToolStripMenuItem.Size = New Size(62, 24)
        AboutToolStripMenuItem.Text = "About"
        ' 
        ' CheckForUpdatesToolStripMenuItem
        ' 
        CheckForUpdatesToolStripMenuItem.Name = "CheckForUpdatesToolStripMenuItem"
        CheckForUpdatesToolStripMenuItem.Size = New Size(297, 24)
        CheckForUpdatesToolStripMenuItem.Text = "Check for Updates"
        ' 
        ' AboutToolStripMenuItem1
        ' 
        AboutToolStripMenuItem1.Name = "AboutToolStripMenuItem1"
        AboutToolStripMenuItem1.Size = New Size(297, 24)
        AboutToolStripMenuItem1.Text = "About EVE SDE Database Builder"
        ' 
        ' DeveloperToolStripMenuItem
        ' 
        DeveloperToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {PrepareFilesForUpdateToolStripMenuItem, BuildBinaryToolStripMenuItem})
        DeveloperToolStripMenuItem.Name = "DeveloperToolStripMenuItem"
        DeveloperToolStripMenuItem.Size = New Size(90, 24)
        DeveloperToolStripMenuItem.Text = "Developer"
        ' 
        ' PrepareFilesForUpdateToolStripMenuItem
        ' 
        PrepareFilesForUpdateToolStripMenuItem.Name = "PrepareFilesForUpdateToolStripMenuItem"
        PrepareFilesForUpdateToolStripMenuItem.Size = New Size(238, 24)
        PrepareFilesForUpdateToolStripMenuItem.Text = "Prepare Files for Update"
        ' 
        ' BuildBinaryToolStripMenuItem
        ' 
        BuildBinaryToolStripMenuItem.Name = "BuildBinaryToolStripMenuItem"
        BuildBinaryToolStripMenuItem.Size = New Size(238, 24)
        BuildBinaryToolStripMenuItem.Text = "Build Binary"
        ' 
        ' gbFilePathSelect
        ' 
        gbFilePathSelect.Controls.Add(pgBar)
        gbFilePathSelect.Controls.Add(btnCancelDownload)
        gbFilePathSelect.Controls.Add(btnDownloadSDE)
        gbFilePathSelect.Controls.Add(lblDownload)
        gbFilePathSelect.Controls.Add(lblDownloadFolderPath)
        gbFilePathSelect.Controls.Add(btnSelectDownloadPath)
        gbFilePathSelect.Controls.Add(btnCheckNoGridItems)
        gbFilePathSelect.Controls.Add(btnCheckAllGridItems)
        gbFilePathSelect.Controls.Add(lblMediaFire)
        gbFilePathSelect.Controls.Add(btnSelectFinalDBPath)
        gbFilePathSelect.Controls.Add(lblSDEPath)
        gbFilePathSelect.Controls.Add(lblFinalDBFolder)
        gbFilePathSelect.Controls.Add(lblFinalDBPath)
        gbFilePathSelect.Controls.Add(btnSelectSDEPath)
        gbFilePathSelect.Location = New Point(10, 214)
        gbFilePathSelect.Name = "gbFilePathSelect"
        gbFilePathSelect.Size = New Size(702, 267)
        gbFilePathSelect.TabIndex = 26
        gbFilePathSelect.TabStop = False
        gbFilePathSelect.Text = "Select File Locations:"
        ' 
        ' pgBar
        ' 
        pgBar.Location = New Point(246, 74)
        pgBar.Name = "pgBar"
        pgBar.Size = New Size(436, 23)
        pgBar.TabIndex = 13
        pgBar.Visible = False
        ' 
        ' btnCancelDownload
        ' 
        btnCancelDownload.AutoSize = True
        btnCancelDownload.Enabled = False
        btnCancelDownload.Location = New Point(177, 70)
        btnCancelDownload.Name = "btnCancelDownload"
        btnCancelDownload.Size = New Size(63, 30)
        btnCancelDownload.TabIndex = 12
        btnCancelDownload.Text = "Cancel"
        btnCancelDownload.UseVisualStyleBackColor = True
        ' 
        ' btnDownloadSDE
        ' 
        btnDownloadSDE.AutoSize = True
        btnDownloadSDE.Location = New Point(83, 70)
        btnDownloadSDE.Name = "btnDownloadSDE"
        btnDownloadSDE.Size = New Size(88, 30)
        btnDownloadSDE.TabIndex = 11
        btnDownloadSDE.Text = "Download"
        btnDownloadSDE.UseVisualStyleBackColor = True
        ' 
        ' lblDownload
        ' 
        lblDownload.AutoSize = True
        lblDownload.Location = New Point(15, 23)
        lblDownload.Name = "lblDownload"
        lblDownload.Size = New Size(155, 20)
        lblDownload.TabIndex = 9
        lblDownload.Text = "SDE Download Folder"
        ' 
        ' lblDownloadFolderPath
        ' 
        lblDownloadFolderPath.BorderStyle = BorderStyle.FixedSingle
        lblDownloadFolderPath.Location = New Point(18, 43)
        lblDownloadFolderPath.Name = "lblDownloadFolderPath"
        lblDownloadFolderPath.Size = New Size(664, 24)
        lblDownloadFolderPath.TabIndex = 10
        lblDownloadFolderPath.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnSelectDownloadPath
        ' 
        btnSelectDownloadPath.AutoSize = True
        btnSelectDownloadPath.Location = New Point(18, 70)
        btnSelectDownloadPath.Name = "btnSelectDownloadPath"
        btnSelectDownloadPath.Size = New Size(59, 30)
        btnSelectDownloadPath.TabIndex = 8
        btnSelectDownloadPath.Text = "Select"
        btnSelectDownloadPath.UseVisualStyleBackColor = True
        ' 
        ' btnCheckNoGridItems
        ' 
        btnCheckNoGridItems.AutoSize = True
        btnCheckNoGridItems.Location = New Point(583, 230)
        btnCheckNoGridItems.Name = "btnCheckNoGridItems"
        btnCheckNoGridItems.Size = New Size(98, 30)
        btnCheckNoGridItems.TabIndex = 7
        btnCheckNoGridItems.Text = "Check None"
        btnCheckNoGridItems.UseVisualStyleBackColor = True
        ' 
        ' btnCheckAllGridItems
        ' 
        btnCheckAllGridItems.AutoSize = True
        btnCheckAllGridItems.Location = New Point(479, 230)
        btnCheckAllGridItems.Name = "btnCheckAllGridItems"
        btnCheckAllGridItems.Size = New Size(98, 30)
        btnCheckAllGridItems.TabIndex = 6
        btnCheckAllGridItems.Text = "Check All"
        btnCheckAllGridItems.UseVisualStyleBackColor = True
        ' 
        ' lblMediaFire
        ' 
        lblMediaFire.AutoSize = True
        lblMediaFire.Location = New Point(15, 103)
        lblMediaFire.Name = "lblMediaFire"
        lblMediaFire.Size = New Size(112, 20)
        lblMediaFire.TabIndex = 0
        lblMediaFire.Text = "SDE File Folder:"
        ' 
        ' btnSelectFinalDBPath
        ' 
        btnSelectFinalDBPath.AutoSize = True
        btnSelectFinalDBPath.Location = New Point(19, 230)
        btnSelectFinalDBPath.Name = "btnSelectFinalDBPath"
        btnSelectFinalDBPath.Size = New Size(59, 30)
        btnSelectFinalDBPath.TabIndex = 5
        btnSelectFinalDBPath.Text = "Select"
        btnSelectFinalDBPath.UseVisualStyleBackColor = True
        ' 
        ' lblSDEPath
        ' 
        lblSDEPath.BorderStyle = BorderStyle.FixedSingle
        lblSDEPath.Location = New Point(18, 123)
        lblSDEPath.Name = "lblSDEPath"
        lblSDEPath.Size = New Size(664, 24)
        lblSDEPath.TabIndex = 1
        lblSDEPath.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblFinalDBFolder
        ' 
        lblFinalDBFolder.AutoSize = True
        lblFinalDBFolder.Location = New Point(15, 183)
        lblFinalDBFolder.Name = "lblFinalDBFolder"
        lblFinalDBFolder.Size = New Size(156, 20)
        lblFinalDBFolder.TabIndex = 3
        lblFinalDBFolder.Text = "Final Database Folder:"
        ' 
        ' lblFinalDBPath
        ' 
        lblFinalDBPath.BorderStyle = BorderStyle.FixedSingle
        lblFinalDBPath.Location = New Point(18, 203)
        lblFinalDBPath.Name = "lblFinalDBPath"
        lblFinalDBPath.Size = New Size(664, 24)
        lblFinalDBPath.TabIndex = 4
        lblFinalDBPath.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnSelectSDEPath
        ' 
        btnSelectSDEPath.AutoSize = True
        btnSelectSDEPath.Location = New Point(18, 150)
        btnSelectSDEPath.Name = "btnSelectSDEPath"
        btnSelectSDEPath.Size = New Size(59, 30)
        btnSelectSDEPath.TabIndex = 2
        btnSelectSDEPath.Text = "Select"
        btnSelectSDEPath.UseVisualStyleBackColor = True
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        TableLayoutPanel1.ColumnCount = 4
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 29.4871788F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 19.23077F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 19.65812F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 31.6239319F))
        TableLayoutPanel1.Controls.Add(dgMain, 0, 0)
        TableLayoutPanel1.Controls.Add(btnClose, 3, 1)
        TableLayoutPanel1.Controls.Add(btnCancel, 2, 1)
        TableLayoutPanel1.Controls.Add(btnBuildDatabase, 0, 1)
        TableLayoutPanel1.Controls.Add(btnSaveSettings, 1, 1)
        TableLayoutPanel1.Location = New Point(10, 487)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 89.23767F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 10.762332F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        TableLayoutPanel1.Size = New Size(702, 431)
        TableLayoutPanel1.TabIndex = 27
        ' 
        ' dgMain
        ' 
        dgMain.AllowUserToAddRows = False
        dgMain.AllowUserToDeleteRows = False
        dgMain.AllowUserToResizeColumns = False
        dgMain.AllowUserToResizeRows = False
        dgMain.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = SystemColors.Control
        DataGridViewCellStyle1.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle1.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgMain.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgMain.Columns.AddRange(New DataGridViewColumn() {FileSelect, FileName, Progress})
        TableLayoutPanel1.SetColumnSpan(dgMain, 4)
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = SystemColors.Window
        DataGridViewCellStyle2.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle2.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgMain.DefaultCellStyle = DataGridViewCellStyle2
        dgMain.Location = New Point(3, 3)
        dgMain.Name = "dgMain"
        DataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = SystemColors.Control
        DataGridViewCellStyle3.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle3.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle3.WrapMode = DataGridViewTriState.True
        dgMain.RowHeadersDefaultCellStyle = DataGridViewCellStyle3
        dgMain.RowHeadersVisible = False
        dgMain.Size = New Size(696, 378)
        dgMain.TabIndex = 7
        ' 
        ' FileSelect
        ' 
        FileSelect.HeaderText = ""
        FileSelect.Name = "FileSelect"
        FileSelect.Width = 25
        ' 
        ' FileName
        ' 
        FileName.HeaderText = "File Name"
        FileName.Name = "FileName"
        FileName.ReadOnly = True
        FileName.Width = 250
        ' 
        ' Progress
        ' 
        Progress.HeaderText = "Progress"
        Progress.Name = "Progress"
        Progress.Width = 400
        ' 
        ' btnClose
        ' 
        btnClose.Anchor = AnchorStyles.Left
        btnClose.Location = New Point(483, 390)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(130, 35)
        btnClose.TabIndex = 5
        btnClose.Text = "Close"
        btnClose.UseVisualStyleBackColor = True
        ' 
        ' btnCancel
        ' 
        btnCancel.Anchor = AnchorStyles.None
        btnCancel.Location = New Point(346, 390)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(130, 35)
        btnCancel.TabIndex = 3
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnBuildDatabase
        ' 
        btnBuildDatabase.Anchor = AnchorStyles.Right
        btnBuildDatabase.Location = New Point(74, 390)
        btnBuildDatabase.Name = "btnBuildDatabase"
        btnBuildDatabase.Size = New Size(130, 35)
        btnBuildDatabase.TabIndex = 2
        btnBuildDatabase.Text = "Build Database"
        btnBuildDatabase.UseVisualStyleBackColor = True
        ' 
        ' btnSaveSettings
        ' 
        btnSaveSettings.Anchor = AnchorStyles.None
        btnSaveSettings.Location = New Point(210, 390)
        btnSaveSettings.Name = "btnSaveSettings"
        btnSaveSettings.Size = New Size(129, 35)
        btnSaveSettings.TabIndex = 4
        btnSaveSettings.Text = "Save Settings"
        btnSaveSettings.UseVisualStyleBackColor = True
        ' 
        ' pnlMain
        ' 
        pnlMain.Items.AddRange(New ToolStripItem() {lblStatus, pgMain, ToolStripStatusLabel1})
        pnlMain.Location = New Point(0, 921)
        pnlMain.Name = "pnlMain"
        pnlMain.Size = New Size(723, 25)
        pnlMain.TabIndex = 28
        pnlMain.Text = "Status"
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = False
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(300, 20)
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pgMain
        ' 
        pgMain.Name = "pgMain"
        pgMain.Size = New Size(400, 19)
        pgMain.Visible = False
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(0, 20)
        ' 
        ' frmMain
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(723, 946)
        Controls.Add(pnlMain)
        Controls.Add(TableLayoutPanel1)
        Controls.Add(gbFilePathSelect)
        Controls.Add(MenuStrip1)
        Controls.Add(gbSelectDBType)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MinimumSize = New Size(739, 985)
        Name = "frmMain"
        StartPosition = FormStartPosition.CenterScreen
        Text = "EVE SDE Database Builder"
        gbSelectDBType.ResumeLayout(False)
        gbOptions.ResumeLayout(False)
        gbOptions.PerformLayout()
        gbLanguage.ResumeLayout(False)
        gbLanguage.PerformLayout()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        gbFilePathSelect.ResumeLayout(False)
        gbFilePathSelect.PerformLayout()
        TableLayoutPanel1.ResumeLayout(False)
        CType(dgMain, ComponentModel.ISupportInitialize).EndInit()
        pnlMain.ResumeLayout(False)
        pnlMain.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents gbSelectDBType As GroupBox
    Friend WithEvents gbOptions As GroupBox
    Friend WithEvents chkEUFormat As CheckBox
    Friend WithEvents rbtnJSON As RadioButton
    Friend WithEvents lblServerName As Label
    Friend WithEvents txtPort As TextBox
    Friend WithEvents lblPort As Label
    Friend WithEvents rbtnSQLiteDB As RadioButton
    Friend WithEvents rbtnSQLServer As RadioButton
    Friend WithEvents lblPassword As Label
    Friend WithEvents rbtnMySQL As RadioButton
    Friend WithEvents rbtnAccess As RadioButton
    Friend WithEvents lblUserName As Label
    Friend WithEvents rbtnCSV As RadioButton
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents txtUserName As TextBox
    Friend WithEvents lblDBName As Label
    Friend WithEvents rbtnPostgreSQL As RadioButton
    Friend WithEvents txtServerName As TextBox
    Friend WithEvents txtDBName As TextBox
    Friend WithEvents gbLanguage As GroupBox
    Friend WithEvents rbtnSpanish As RadioButton
    Friend WithEvents rbtnKorean As RadioButton
    Friend WithEvents rbtnChinese As RadioButton
    Friend WithEvents rbtnRussian As RadioButton
    Friend WithEvents rbtnJapanese As RadioButton
    Friend WithEvents rbtnFrench As RadioButton
    Friend WithEvents rbtnGerman As RadioButton
    Friend WithEvents rbtnEnglish As RadioButton
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ResetSavedBuildNumberToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CheckForUpdatesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents DeveloperToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents PrepareFilesForUpdateToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BuildBinaryToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents gbFilePathSelect As GroupBox
    Friend WithEvents pgBar As ProgressBar
    Friend WithEvents btnCancelDownload As Button
    Friend WithEvents btnDownloadSDE As Button
    Friend WithEvents lblDownload As Label
    Friend WithEvents lblDownloadFolderPath As Label
    Friend WithEvents btnSelectDownloadPath As Button
    Friend WithEvents btnCheckNoGridItems As Button
    Friend WithEvents btnCheckAllGridItems As Button
    Friend WithEvents lblMediaFire As Label
    Friend WithEvents btnSelectFinalDBPath As Button
    Friend WithEvents lblSDEPath As Label
    Friend WithEvents lblFinalDBFolder As Label
    Friend WithEvents lblFinalDBPath As Label
    Friend WithEvents btnSelectSDEPath As Button
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents btnClose As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnBuildDatabase As Button
    Friend WithEvents btnSaveSettings As Button
    Friend WithEvents pnlMain As StatusStrip
    Friend WithEvents lblStatus As ToolStripStatusLabel
    Friend WithEvents pgMain As ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents FBDialog As FolderBrowserDialog
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents dgMain As DataGridView
    Friend WithEvents FileSelect As DataGridViewCheckBoxColumn
    Friend WithEvents FileName As DataGridViewTextBoxColumn
    Friend WithEvents Progress As DataGridViewTextBoxColumn
End Class
