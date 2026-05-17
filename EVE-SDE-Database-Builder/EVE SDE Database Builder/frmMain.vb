Imports System.Globalization ' For culture info
Imports System.IO
Imports System.IO.Compression
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Xml
Imports Google.Protobuf.WellKnownTypes
Imports Mysqlx.Crud
Imports Mysqlx.XDevAPI.Common

Public Class frmMain
    Private FirstLoad As Boolean

    ' For deploying the files to XML for updates
    Private ReadOnly LatestFilesFolder As String
    Private Const MainEXEFile As String = "EVE SDE Database Builder.exe"
    Private Const UpdaterEXEFile As String = "EVE SDE Database Builder Updater.exe"

    Private Const LatestVersionXML As String = "LatestVersionESDEDB.xml"

    ' File URLs
    Private Const MainEXEFileURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/EVE%20SDE%20Database%20Builder.exe"
    Private Const UpdaterEXEFileURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/ESDEDB%20Updater.exe"
    Private Const MainEXEConfigURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/EVE%20SDE%20Database%20Builder.exe.config"
    Private Const UpdaterEXEConfigURL As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/ESDEDB%20Updater.exe.config"
    Private Const SQLiteInteropURL86 As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/x86/SQLite.Interop.dll"
    Private Const SQLiteInteropURL64 As String = "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/x64/SQLite.Interop.dll"

    Private CheckedFilesList As List(Of String)

    Private ReadOnly LocalCulture As New CultureInfo("en-US")

    ' For use with updating the grid with files
    Public Structure FileListItem
        Dim FileName As String
        Dim RowLocation As Integer
    End Structure

    ' For use in filling the grid with checks
    Public Structure GridFileItem
        Dim FileName As String
        Dim Checked As Integer
    End Structure

#Region "Settings"

    ''' <summary>
    ''' Saves all settings, including the files checked
    ''' </summary>
    Private Sub SaveSettings(SupressMessage As Boolean)

        If Not ConductErrorChecks(False) Then
            Exit Sub
        End If

        With UserApplicationSettings
            .DatabaseName = txtDBName.Text
            .SDEDirectory = lblSDEPath.Text
            .FinalDBPath = lblFinalDBPath.Text
            .DownloadFolderPath = lblDownloadFolderPath.Text

            ' Get the specific settings for each option
            If rbtnAccess.Checked Then
                .SelectedDB = rbtnAccess.Text
                .AccessPassword = txtPassword.Text
            ElseIf rbtnSQLServer.Checked Then
                .SelectedDB = rbtnSQLServer.Text
                .SQLConnectionString = txtServerName.Text
                .SQLPassword = txtPassword.Text
                .SQLUserName = txtUserName.Text
            ElseIf rbtnMySQL.Checked Then
                .SelectedDB = rbtnMySQL.Text
                .MySQLPassword = txtPassword.Text
                .MySQLConnectionString = txtServerName.Text
                .MySQLUserName = txtUserName.Text
                .MySQLPort = txtPort.Text
            ElseIf rbtnPostgreSQL.Checked Then
                .SelectedDB = rbtnPostgreSQL.Text
                .PostgreSQLPassword = txtPassword.Text
                .PostgreSQLConnectionString = txtServerName.Text
                .PostgreSQLUserName = txtUserName.Text
                .PostgreSQLPort = txtPort.Text
            ElseIf rbtnSQLiteDB.Checked Then
                .SelectedDB = rbtnSQLiteDB.Text
            ElseIf rbtnCSV.Checked Then
                .SelectedDB = rbtnCSV.Text
                .CSVEUCheck = chkEUFormat.Checked
            ElseIf rbtnJSON.Checked Then
                .SelectedDB = rbtnJSON.Text
            End If

            ' Language
            If rbtnEnglish.Checked Then
                .SelectedLanguage = rbtnEnglish.Text
            ElseIf rbtnGerman.Checked Then
                .SelectedLanguage = rbtnGerman.Text
            ElseIf rbtnFrench.Checked Then
                .SelectedLanguage = rbtnFrench.Text
            ElseIf rbtnJapanese.Checked Then
                .SelectedLanguage = rbtnJapanese.Text
            ElseIf rbtnRussian.Checked Then
                .SelectedLanguage = rbtnRussian.Text
            ElseIf rbtnChinese.Checked Then
                .SelectedLanguage = rbtnChinese.Text
            ElseIf rbtnKorean.Checked Then
                .SelectedLanguage = rbtnKorean.Text
            ElseIf rbtnSpanish.Checked Then
                .SelectedLanguage = rbtnSpanish.Text
            End If
        End With

        ' Save the settings
        Call AllSettings.SaveApplicationSettings(UserApplicationSettings)

        ' Save the grid checks now as a stream
        Dim MyStream As StreamWriter
        MyStream = File.CreateText(EXEFileFolder & "\GridSettings.txt")

        ' Loop through the grid and save what is checked - nothing fancy just a list of the file names in a text file
        For i = 0 To dgMain.RowCount - 1
            If dgMain.Rows(i).Cells(0).Value <> 0 Then
                MyStream.Write(dgMain.Rows(i).Cells(1).Value & Environment.NewLine)
            End If
        Next

        MyStream.Flush()
        MyStream.Close()

        If Not SupressMessage Then
            MsgBox("Settings Saved", vbInformation, Application.ProductName)
        End If

    End Sub

    ''' <summary>
    ''' Gets the data for file paths and other settings from a simple text file saved in local directory
    ''' </summary>
    Private Sub GetSettings()
        ' Read the settings file and lines
        Dim BPStream As StreamReader = Nothing
        Dim FieldType As String = ""
        Dim Language As String = ""
        Dim TempLanguage As String = ""

        UserApplicationSettings = AllSettings.LoadApplicationSettings

        With UserApplicationSettings
            txtDBName.Text = .DatabaseName
            lblFinalDBPath.Text = .FinalDBPath
            lblSDEPath.Text = .SDEDirectory
            lblDownloadFolderPath.Text = .DownloadFolderPath

            ' Set the option
            Select Case .SelectedDB
                Case rbtnAccess.Text
                    rbtnAccess.Checked = True
                Case rbtnCSV.Text
                    rbtnCSV.Checked = True
                Case rbtnJSON.Text
                    rbtnJSON.Checked = True
                Case rbtnSQLiteDB.Text
                    rbtnSQLiteDB.Checked = True
                Case rbtnSQLServer.Text
                    rbtnSQLServer.Checked = True
                Case rbtnMySQL.Text
                    rbtnMySQL.Checked = True
                Case rbtnPostgreSQL.Text
                    rbtnPostgreSQL.Checked = True
            End Select

            Select Case .SelectedLanguage
                Case rbtnEnglish.Text
                    rbtnEnglish.Checked = True
                Case rbtnFrench.Text
                    rbtnFrench.Checked = True
                Case rbtnGerman.Text
                    rbtnGerman.Checked = True
                Case rbtnJapanese.Text
                    rbtnJapanese.Checked = True
                Case rbtnRussian.Text
                    rbtnRussian.Checked = True
                Case rbtnChinese.Text
                    rbtnChinese.Checked = True
                Case rbtnKorean.Text
                    rbtnKorean.Checked = True
                Case rbtnSpanish.Text
                    rbtnSpanish.Checked = True
                Case Else
                    rbtnEnglish.Checked = True
            End Select
        End With

        ' Now load all the settings based on that option
        Call LoadFormSettings()

    End Sub

    ''' <summary>
    ''' Gets the boxes checked for loading into grid from file
    ''' </summary>
    Private Sub GetGridSettings()
        ' Read the settings file and save all the files checked
        Dim BPStream As StreamReader
        CheckedFilesList = New List(Of String)

        Dim Line As String

        If File.Exists(EXEFileFolder & "\GridSettings.txt") Then
            BPStream = New StreamReader(EXEFileFolder & "\GridSettings.txt")

            Do
                Line = BPStream.ReadLine()
                If Not IsNothing(Line) Then
                    CheckedFilesList.Add(Line)
                End If
            Loop Until Line Is Nothing

            BPStream.Close()
        End If
    End Sub

#End Region

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        EXEFileFolder = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)

        ' Update files first unless dev 
        If Not File.Exists(Path.Combine(EXEFileFolder, "Developer.txt")) Then
            Call CheckForUpdates(False)
        End If

        FirstLoad = True

        ' Remove the 'Developer' menu if no developer file
        If Not File.Exists(Path.Combine(EXEFileFolder, "Developer.txt")) Then
            MenuStrip1.Items.Remove(DeveloperToolStripMenuItem)
        End If

        ' Set the latest files folder path, which is one folder up from the root directory
        LatestFilesFolder = Path.Combine(EXEFileFolder, "Latest Files")

        If Not Directory.Exists(LatestFilesFolder) Then
            CreateNewDirectory(LatestFilesFolder)
        End If

        ' Add any initialization after the InitializeComponent() call.
        Call GetSettings()
        Call GetGridSettings()

        ' set a tool tip for the EU check box
        ToolTip1.SetToolTip(chkEUFormat, "Replaces commas with semicolons and all decimals with commas in a CSV file")

        ' Sets the CurrentCulture 
        CultureInfo.DefaultThreadCurrentCulture = LocalCulture
        CultureInfo.DefaultThreadCurrentUICulture = LocalCulture

        FirstLoad = False

    End Sub

    Private Sub frmMain_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' Load the file names in the grid
        Call LoadFileListtoGrid()
        FirstLoad = False
    End Sub

    Private Sub btnBuildDatabase_Click(sender As Object, e As EventArgs) Handles btnBuildDatabase.Click
        Dim FullDBPathName As String = UserApplicationSettings.FinalDBPath & "\" & UserApplicationSettings.DatabaseName
        Dim WasSuccessful As Boolean = False

        If Not ConductErrorChecks(True) Then
            Exit Sub
        End If

        ' Prep form
        CancelImport = False
        btnBuildDatabase.Enabled = False
        btnSaveSettings.Enabled = False
        gbSelectDBType.Enabled = False
        gbFilePathSelect.Enabled = False
        MenuStrip1.Enabled = False
        dgMain.ReadOnly = True
        btnClose.Enabled = False
        btnCancel.Enabled = True
        btnCancel.Focus()

        Dim TimeCheck As Date = Now

        With UserApplicationSettings
            ' Build the db based on selections
            If rbtnSQLiteDB.Checked Then ' SQLite

                Dim NewSQLiteDB As New SQLiteDB(FullDBPathName & ".sqlite", UserApplicationSettings.FinalDBPath, WasSuccessful)

                If WasSuccessful Then
                    Call NewSQLiteDB.BeginSQLiteTransaction()
                    Call BuildEVEDatabase(NewSQLiteDB, DatabaseType.SQLite)
                    lblStatus.Text = "Finalizing..."
                    Application.DoEvents()
                    Call NewSQLiteDB.CommitSQLiteTransaction()

                    ' Run a vacuum on the new DB to optimize and save space
                    Call NewSQLiteDB.ExecuteNonQuerySQL("VACUUM")
                    Call NewSQLiteDB.ExecuteNonQuerySQL("PRAGMA integrity_check")
                    Call NewSQLiteDB.CloseDB()
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnSQLServer.Checked Then ' Microsoft SQL Server

                Dim NewSQLServerDB As New msSQLDB(.DatabaseName, .SQLConnectionString, .SQLUserName, .SQLPassword, WasSuccessful)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewSQLServerDB, DatabaseType.SQLServer)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnAccess.Checked Then ' Microsoft Access

                Dim NewAccessDB As New msAccessDB(FullDBPathName & ".accdb", .AccessPassword, WasSuccessful)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewAccessDB, DatabaseType.MSAccess)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnCSV.Checked Then ' CSV

                Dim NewCSVDB As New CSVDB(FullDBPathName & "_CSV", WasSuccessful, False, False, chkEUFormat.Checked)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewCSVDB, DatabaseType.CSV)
                Else
                    GoTo ExitProc
                End If
            ElseIf rbtnJSON.Checked Then ' JSON
                Dim NewJSONDB As New JSONDB(FullDBPathName & "_JSON", WasSuccessful)
                If WasSuccessful Then
                    Call BuildEVEDatabase(NewJSONDB, DatabaseType.JSON)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnMySQL.Checked Then ' MySQL

                Dim NewMySQLDB As New MySQLDB(.DatabaseName, .MySQLConnectionString, .MySQLUserName, .MySQLPassword, Trim(txtPort.Text), WasSuccessful)

                If WasSuccessful Then
                    Call BuildEVEDatabase(NewMySQLDB, DatabaseType.MySQL)
                Else
                    GoTo ExitProc
                End If

            ElseIf rbtnPostgreSQL.Checked Then ' postgreSQL

                Dim NewPostgreSQLDB As New postgreSQLDB(.DatabaseName, .PostgreSQLConnectionString, .PostgreSQLUserName, .PostgreSQLPassword, .PostgreSQLPort, WasSuccessful)

                If WasSuccessful Then
                    Call BuildEVEDatabase(NewPostgreSQLDB, DatabaseType.PostgreSQL)
                Else
                    GoTo ExitProc
                End If

            End If
        End With

        lblStatus.Text = ""
        Me.Cursor = Cursors.Default
        Application.DoEvents()

        If CancelImport Then
            CancelImport = False
            Call ResetProgressColumn()
            Call MsgBox("Import Canceled", vbInformation, Application.ProductName)
        Else
            Dim Seconds As Integer = CInt(DateDiff(DateInterval.Second, TimeCheck, Now))
            Call MsgBox("Files Imported in: " & CInt(Seconds \ 60) & " min " & CInt(Seconds Mod 60) & " sec", vbInformation, Application.ProductName)
        End If

ExitProc:
        btnBuildDatabase.Enabled = True
        btnSaveSettings.Enabled = True
        gbSelectDBType.Enabled = True
        gbFilePathSelect.Enabled = True
        MenuStrip1.Enabled = True
        btnClose.Enabled = True
        btnCancel.Enabled = False
        dgMain.ReadOnly = False
        Call ClearMainProgressBar()
        btnBuildDatabase.Focus()

    End Sub

    ''' <summary>
    ''' Builds the EVE Database for the database type sent.
    ''' </summary>
    ''' <param name="UpdateDatabase">Database class to use for building database and import data into.</param>
    ''' <param name="DatabaseType">Type of Database class</param>
    Private Sub BuildEVEDatabase(UpdateDatabase As Object, DBType As DatabaseType)
        Dim ImportFileList As New List(Of FileListItem)
        Dim Parameters As SDEFilesBase.ImportParameters
        Dim WorkingDirectory As String = ""

        ' Set up the importfile list
        ImportFileList = GetImportFileList()

        ' Reset the third column so it updates properly
        Call ResetProgressColumn()

        lblStatus.Text = "Preparing files..."
        Application.DoEvents()

        ' Depending on the database, we may need to change the CSV directory to process later - also set if we import records in insert statements or bulk here
        If DBType = DatabaseType.MySQL Then
            WorkingDirectory = UpdateDatabase.GetCSVDirectory
            Parameters.InsertRecords = False
        Else
            Parameters.InsertRecords = True
        End If

        Parameters.DatabaseType = DBType

        With Parameters
            If rbtnEnglish.Checked Then
                .ImportLanguageCode = "en"
            ElseIf rbtnFrench.Checked Then
                .ImportLanguageCode = "fr"
            ElseIf rbtnRussian.Checked Then
                .ImportLanguageCode = "ru"
            ElseIf rbtnChinese.Checked Then
                .ImportLanguageCode = "zh"
            ElseIf rbtnGerman.Checked Then
                .ImportLanguageCode = "de"
            ElseIf rbtnKorean.Checked Then
                .ImportLanguageCode = "ko"
            ElseIf rbtnJapanese.Checked Then
                .ImportLanguageCode = "ja"
            ElseIf rbtnSpanish.Checked Then
                .ImportLanguageCode = "es"
            Else
                .ImportLanguageCode = "en"
            End If
        End With

        If CancelImport Then
            GoTo CancelImportProcessing
        End If

        ' If we want to convert the jsonl files to json, then just do that here and exit
        If DBType = DatabaseType.JSON Then
            Call ImportJSONDatabase(CType(UpdateDatabase, JSONDB).DatabasePath)
            GoTo FinalizeProcessing
        End If

#Region "Import functions"
        ' Declare all tasks here for threading the imports
        Dim AgentsInSpaceTasks As New List(Of Task(Of Dictionary(Of Long, agentInSpace)))
        Dim AgentTypesTasks As New List(Of Task(Of Dictionary(Of Long, agentType)))
        Dim AncestriesTasks As New List(Of Task(Of Dictionary(Of Long, ancestry)))
        Dim BloodlinesTasks As New List(Of Task(Of Dictionary(Of Long, bloodline)))
        Dim BlueprintsTasks As New List(Of Task(Of Dictionary(Of Long, blueprint)))
        Dim CategoriesTasks As New List(Of Task(Of Dictionary(Of Long, category)))
        Dim CertificatesTasks As New List(Of Task(Of Dictionary(Of Long, certificate)))
        Dim CharacterAttributesTasks As New List(Of Task(Of Dictionary(Of Long, characterAttribute)))
        Dim CloneGradesTasks As New List(Of Task(Of Dictionary(Of Long, cloneGrade)))
        Dim CompressibleTypesTasks As New List(Of Task(Of Dictionary(Of Long, compressibleType)))
        Dim ContrabandTypesTasks As New List(Of Task(Of Dictionary(Of Long, contrabandType)))
        Dim ControlTowerResourcesTasks As New List(Of Task(Of Dictionary(Of Long, _controlTowerResources)))
        Dim CorporationActivitiesTasks As New List(Of Task(Of Dictionary(Of Long, corporationActivity)))
        Dim DebuffTypesTasks As New List(Of Task(Of Dictionary(Of Long, dbuffCollection)))
        Dim DogmaAttributesTasks As New List(Of Task(Of Dictionary(Of Long, dogmaAttribute)))
        Dim DogmaAttributeCategoriesTasks As New List(Of Task(Of Dictionary(Of Long, dogmaAttributeCategory)))
        Dim DogmaEffectsTasks As New List(Of Task(Of Dictionary(Of Long, dogmaEffect)))
        Dim DogmaUnitsTasks As New List(Of Task(Of Dictionary(Of Long, dogmaUnit)))
        Dim DynamicItemAttributesTasks As New List(Of Task(Of Dictionary(Of Long, dynamicItemAttribute)))
        Dim FactionTasks As New List(Of Task(Of Dictionary(Of Long, faction)))
        Dim FreelanceJobSchemaTasks As New List(Of Task(Of Dictionary(Of Long, freelanceJob)))
        Dim GraphicTasks As New List(Of Task(Of Dictionary(Of Long, graphic)))
        Dim GroupTasks As New List(Of Task(Of Dictionary(Of Long, group)))
        Dim IconTasks As New List(Of Task(Of Dictionary(Of Long, _icon)))
        Dim LandmarkTasks As New List(Of Task(Of Dictionary(Of Long, landmark)))
        Dim MapAsteroidsTasks As New List(Of Task(Of Dictionary(Of Long, asteroidBelt)))
        Dim MapConstellationsTasks As New List(Of Task(Of Dictionary(Of Long, constellation)))
        Dim MapMoonsTasks As New List(Of Task(Of Dictionary(Of Long, moon)))
        Dim MapPlanetsTasks As New List(Of Task(Of Dictionary(Of Long, planet)))
        Dim MapRegionsTasks As New List(Of Task(Of Dictionary(Of Long, region)))
        Dim MapSolarSystemsTasks As New List(Of Task(Of Dictionary(Of Long, solarSystem)))
        Dim MapStargatesTasks As New List(Of Task(Of Dictionary(Of Long, stargate)))
        Dim MapStarsTasks As New List(Of Task(Of Dictionary(Of Long, star)))
        Dim MarketGroupsTasks As New List(Of Task(Of Dictionary(Of Long, marketGroup)))
        Dim MasteriesTasks As New List(Of Task(Of Dictionary(Of Long, Dictionary(Of Integer, List(Of Integer)))))
        Dim MercenaryTacticalTasks As New List(Of Task(Of Dictionary(Of Long, tacticalOperation)))
        Dim MetaGroupsTasks As New List(Of Task(Of Dictionary(Of Long, metaGroup)))
        Dim NPCCharactersTasks As New List(Of Task(Of Dictionary(Of Long, npcCharacter)))
        Dim NPCCorporationDivisionsTasks As New List(Of Task(Of Dictionary(Of Long, npcCorporationDivision)))
        Dim NPCCorporationsTasks As New List(Of Task(Of Dictionary(Of Long, npcCorporation)))
        Dim NPCStationsTasks As New List(Of Task(Of Dictionary(Of Long, npcStation)))
        Dim PlanetResourcesTasks As New List(Of Task(Of Dictionary(Of Long, planetResource)))
        Dim PlanetSchematicsTasks As New List(Of Task(Of Dictionary(Of Long, planetSchematic)))
        Dim RaceTasks As New List(Of Task(Of Dictionary(Of Long, race)))
        Dim SkinsTasks As New List(Of Task(Of Dictionary(Of Long, skin)))
        Dim SkinLicensesTasks As New List(Of Task(Of Dictionary(Of Long, skinLicense)))
        Dim SkinMaterialsTasks As New List(Of Task(Of Dictionary(Of Long, skinMaterial)))
        Dim SovereigntyUpgradesTasks As New List(Of Task(Of Dictionary(Of Long, sovereigntyUpgrade)))
        Dim StationServicesTasks As New List(Of Task(Of Dictionary(Of Long, stationService)))
        Dim StationOperationsTasks As New List(Of Task(Of Dictionary(Of Long, stationOperation)))
        Dim TypeBonusesTasks As New List(Of Task(Of Dictionary(Of Long, bonus)))
        Dim TypeDogmaTasks As New List(Of Task(Of Dictionary(Of Long, dogmatype)))
        Dim TypeMaterialsTasks As New List(Of Task(Of Dictionary(Of Long, typeMaterial)))
        Dim TypesTasks As New List(Of Task(Of Dictionary(Of Long, typeID)))
        Dim MapSecondarySunsTasks As New List(Of Task(Of Dictionary(Of Long, secondSun)))

        Dim Counter As Integer = 0

        lblStatus.Text = "Importing file data..."
        Application.DoEvents()

        ' Now open threads for each of the checked files and import them
        For Each SDEFile In ImportFileList
            With SDEFile
                Select Case .FileName
                    Case agentsinSpace.BaseFileName
                        Dim ImportAgentsinSpace As agentsinSpace
                        AgentsInSpaceTasks.Add(Task.Run(Function()
                                                            ImportAgentsinSpace = New agentsinSpace(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                            Return ImportAgentsinSpace.ImportFile()
                                                        End Function))
                    Case agentTypes.BaseFileName
                        Dim ImportAgentTypes As agentTypes
                        AgentTypesTasks.Add(Task.Run(Function()
                                                         ImportAgentTypes = New agentTypes(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportAgentTypes.ImportFile()
                                                     End Function))
                    Case ancestries.BaseFileName
                        Dim ImportAncestries As ancestries
                        AncestriesTasks.Add(Task.Run(Function()
                                                         ImportAncestries = New ancestries(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportAncestries.ImportFile()
                                                     End Function))
                    Case bloodLines.BaseFileName
                        Dim ImportBloodlines As bloodLines
                        BloodlinesTasks.Add(Task.Run(Function()
                                                         ImportBloodlines = New bloodLines(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportBloodlines.ImportFile()
                                                     End Function))
                    Case blueprints.BaseFileName
                        Dim ImportBlueprints As blueprints
                        BlueprintsTasks.Add(Task.Run(Function()
                                                         ImportBlueprints = New blueprints(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportBlueprints.ImportFile()
                                                     End Function))
                    Case categories.BaseFileName
                        Dim Importcategories As categories
                        CategoriesTasks.Add(Task.Run(Function()
                                                         Importcategories = New categories(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return Importcategories.ImportFile()
                                                     End Function))
                    Case certificates.BaseFileName
                        Dim Importcertificates As certificates
                        CertificatesTasks.Add(Task.Run(Function()
                                                           Importcertificates = New certificates(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                           Return Importcertificates.ImportFile()
                                                       End Function))
                    Case characterAttributes.BaseFileName
                        Dim ImportCharacterAttributes As characterAttributes
                        CharacterAttributesTasks.Add(Task.Run(Function()
                                                                  ImportCharacterAttributes = New characterAttributes(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                  Return ImportCharacterAttributes.ImportFile()
                                                              End Function))
                    Case cloneGrades.BaseFileName
                        Dim Importclonegrades As cloneGrades
                        CloneGradesTasks.Add(Task.Run(Function()
                                                          Importclonegrades = New cloneGrades(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                          Return Importclonegrades.ImportFile()
                                                      End Function))
                    Case compressibleTypes.BaseFileName
                        Dim Importcompressibletypes As compressibleTypes
                        CompressibleTypesTasks.Add(Task.Run(Function()
                                                                Importcompressibletypes = New compressibleTypes(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                Return Importcompressibletypes.ImportFile()
                                                            End Function))
                    Case contrabandTypes.BaseFileName
                        Dim Importcontrabandtypes As contrabandTypes
                        ContrabandTypesTasks.Add(Task.Run(Function()
                                                              Importcontrabandtypes = New contrabandTypes(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                              Return Importcontrabandtypes.ImportFile()
                                                          End Function))
                    Case controlTowerResources.BaseFileName
                        Dim Importcontroltowerresources As controlTowerResources
                        ControlTowerResourcesTasks.Add(Task.Run(Function()
                                                                    Importcontroltowerresources = New controlTowerResources(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                    Return Importcontroltowerresources.ImportFile()
                                                                End Function))
                    Case corporationActivities.BaseFileName
                        Dim ImportcorporationActivities As corporationActivities
                        CorporationActivitiesTasks.Add(Task.Run(Function()
                                                                    ImportcorporationActivities = New corporationActivities(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                    Return ImportcorporationActivities.ImportFile()
                                                                End Function))
                    Case dbuffCollections.BaseFileName
                        Dim ImportdbuffCollections As dbuffCollections
                        DebuffTypesTasks.Add(Task.Run(Function()
                                                          ImportdbuffCollections = New dbuffCollections(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                          Return ImportdbuffCollections.ImportFile()
                                                      End Function))
                    Case dogmaAttributeCategories.BaseFileName
                        Dim ImportdogmaAttributeCategories As dogmaAttributeCategories
                        DogmaAttributeCategoriesTasks.Add(Task.Run(Function()
                                                                       ImportdogmaAttributeCategories = New dogmaAttributeCategories(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                       Return ImportdogmaAttributeCategories.ImportFile()
                                                                   End Function))
                    Case dogmaAttributes.BaseFileName
                        Dim ImportdogmaAttributes As dogmaAttributes
                        DogmaAttributesTasks.Add(Task.Run(Function()
                                                              ImportdogmaAttributes = New dogmaAttributes(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                              Return ImportdogmaAttributes.ImportFile()
                                                          End Function))
                    Case dogmaEffects.BaseFileName
                        Dim ImportdogmaEffects As dogmaEffects
                        DogmaEffectsTasks.Add(Task.Run(Function()
                                                           ImportdogmaEffects = New dogmaEffects(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                           Return ImportdogmaEffects.ImportFile()
                                                       End Function))
                    Case dogmaUnits.BaseFileName
                        Dim ImportdogmaUnits As dogmaUnits
                        DogmaUnitsTasks.Add(Task.Run(Function()
                                                         ImportdogmaUnits = New dogmaUnits(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportdogmaUnits.ImportFile()
                                                     End Function))
                    Case dynamicItemAttributes.BaseFileName
                        Dim ImportdynamicAttributes As dynamicItemAttributes
                        DynamicItemAttributesTasks.Add(Task.Run(Function()
                                                                    ImportdynamicAttributes = New dynamicItemAttributes(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                    Return ImportdynamicAttributes.ImportFile()
                                                                End Function))
                    Case factions.BaseFileName
                        Dim Importfactions As factions
                        FactionTasks.Add(Task.Run(Function()
                                                      Importfactions = New factions(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                      Return Importfactions.ImportFile()
                                                  End Function))
                    Case freelanceJobSchemas.BaseFileName
                        Dim importer As freelanceJobSchemas
                        FreelanceJobSchemaTasks.Add(
                            Task.Run(Function()
                                         importer = New freelanceJobSchemas(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                         Return importer.ImportFile()   ' returns Dictionary(Of Long, freelanceJob)
                                     End Function))
                    Case graphics.BaseFileName
                        Dim Importgraphics As graphics
                        GraphicTasks.Add(Task.Run(Function()
                                                      Importgraphics = New graphics(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                      Return Importgraphics.ImportFile()
                                                  End Function))
                    Case groups.BaseFileName
                        Dim Importgroups As groups
                        GroupTasks.Add(Task.Run(Function()
                                                    Importgroups = New groups(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                    Return Importgroups.ImportFile()
                                                End Function))
                    Case icons.BaseFileName
                        Dim ImportIcons As icons
                        IconTasks.Add(Task.Run(Function()
                                                   ImportIcons = New icons(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                   Return ImportIcons.ImportFile()
                                               End Function))
                    Case landmarks.BaseFileName
                        Dim Importlandmarks As landmarks
                        LandmarkTasks.Add(Task.Run(Function()
                                                       Importlandmarks = New landmarks(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                       Return Importlandmarks.ImportFile()
                                                   End Function))
                    Case mapAsteroidBelts.BaseFileName
                        Dim Importasteroidbelts As mapAsteroidBelts
                        MapAsteroidsTasks.Add(Task.Run(Function()
                                                           Importasteroidbelts = New mapAsteroidBelts(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                           Return Importasteroidbelts.ImportFile()
                                                       End Function))
                    Case mapConstellations.BaseFileName
                        Dim ImportConstellations As mapConstellations
                        MapConstellationsTasks.Add(Task.Run(Function()
                                                                ImportConstellations = New mapConstellations(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                Return ImportConstellations.ImportFile()
                                                            End Function))
                    Case mapMoons.BaseFileName
                        Dim ImportMoons As mapMoons
                        MapMoonsTasks.Add(Task.Run(Function()
                                                       ImportMoons = New mapMoons(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                       Return ImportMoons.ImportFile()
                                                   End Function))
                    Case mapPlanets.BaseFileName
                        Dim ImportPlanets As mapPlanets
                        MapPlanetsTasks.Add(Task.Run(Function()
                                                         ImportPlanets = New mapPlanets(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportPlanets.ImportFile()
                                                     End Function))
                    Case mapRegions.BaseFileName
                        Dim ImportRegions As mapRegions
                        MapRegionsTasks.Add(Task.Run(Function()
                                                         ImportRegions = New mapRegions(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportRegions.ImportFile()
                                                     End Function))
                    Case mapSolarSystems.BaseFileName
                        Dim ImportSolarSystems As mapSolarSystems
                        MapSolarSystemsTasks.Add(Task.Run(Function()
                                                              ImportSolarSystems = New mapSolarSystems(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                              Return ImportSolarSystems.ImportFile()
                                                          End Function))
                    Case mapStargates.BaseFileName
                        Dim ImportStargates As mapStargates
                        MapStargatesTasks.Add(Task.Run(Function()
                                                           ImportStargates = New mapStargates(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                           Return ImportStargates.ImportFile()
                                                       End Function))
                    Case mapStars.BaseFileName
                        Dim ImportStars As mapStars
                        MapStarsTasks.Add(Task.Run(Function()
                                                       ImportStars = New mapStars(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                       Return ImportStars.ImportFile()
                                                   End Function))
                    Case mapSecondarySuns.BaseFileName
                        Dim ImportSecondSuns As mapSecondarySuns
                        MapSecondarySunsTasks.Add(Task.Run(Function()
                                                               ImportSecondSuns = New mapSecondarySuns(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                               Return ImportSecondSuns.ImportFile()
                                                           End Function))
                    Case marketGroups.BaseFileName
                        Dim ImportMarketGroups As marketGroups
                        MarketGroupsTasks.Add(Task.Run(Function()
                                                           ImportMarketGroups = New marketGroups(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                           Return ImportMarketGroups.ImportFile()
                                                       End Function))
                    Case masteries.BaseFileName
                        Dim importer As masteries
                        MasteriesTasks.Add(
                            Task.Run(Function()
                                         importer = New masteries(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                         Return importer.ImportFile()   ' returns Dictionary(Of Long, Dictionary(Of Integer, List(Of Integer)))
                                     End Function))
                    Case mercenaryTacticalOperations.BaseFileName
                        Dim ImportTypes As mercenaryTacticalOperations
                        MercenaryTacticalTasks.Add(Task.Run(Function()
                                                                ImportTypes = New mercenaryTacticalOperations(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                Return ImportTypes.ImportFile()
                                                            End Function))
                    Case metaGroups.BaseFileName
                        Dim ImportStars As metaGroups
                        MetaGroupsTasks.Add(Task.Run(Function()
                                                         ImportStars = New metaGroups(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                         Return ImportStars.ImportFile()
                                                     End Function))
                    Case npcCharacters.BaseFileName
                        Dim ImportStars As npcCharacters
                        NPCCharactersTasks.Add(Task.Run(Function()
                                                            ImportStars = New npcCharacters(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                            Return ImportStars.ImportFile()
                                                        End Function))
                    Case npcCorporationDivisions.BaseFileName
                        Dim ImportStars As npcCorporationDivisions
                        NPCCorporationDivisionsTasks.Add(Task.Run(Function()
                                                                      ImportStars = New npcCorporationDivisions(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                      Return ImportStars.ImportFile()
                                                                  End Function))
                    Case npcCorporations.BaseFileName
                        Dim ImportStars As npcCorporations
                        NPCCorporationsTasks.Add(Task.Run(Function()
                                                              ImportStars = New npcCorporations(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                              Return ImportStars.ImportFile()
                                                          End Function))
                    Case npcStations.BaseFileName
                        Dim ImportStars As npcStations
                        NPCStationsTasks.Add(Task.Run(Function()
                                                          ImportStars = New npcStations(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                          Return ImportStars.ImportFile()
                                                      End Function))
                    Case planetSchematics.BaseFileName
                        Dim ImportPlanetSchematics As planetSchematics
                        PlanetSchematicsTasks.Add(Task.Run(Function()
                                                               ImportPlanetSchematics = New planetSchematics(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                               Return ImportPlanetSchematics.ImportFile()
                                                           End Function))
                    Case planetResources.BaseFileName
                        Dim ImportPlanetResources As planetResources
                        PlanetResourcesTasks.Add(Task.Run(Function()
                                                              ImportPlanetResources = New planetResources(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                              Return ImportPlanetResources.ImportFile()
                                                          End Function))
                    Case races.BaseFileName
                        Dim ImportRaces As races
                        RaceTasks.Add(Task.Run(Function()
                                                   ImportRaces = New races(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                   Return ImportRaces.ImportFile()
                                               End Function))
                    Case skins.BaseFileName
                        Dim ImportSkins As skins
                        SkinsTasks.Add(Task.Run(Function()
                                                    ImportSkins = New skins(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                    Return ImportSkins.ImportFile()
                                                End Function))
                    Case skinLicenses.BaseFileName
                        Dim ImportSkinLiscense As skinLicenses
                        SkinLicensesTasks.Add(Task.Run(Function()
                                                           ImportSkinLiscense = New skinLicenses(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                           Return ImportSkinLiscense.ImportFile()
                                                       End Function))
                    Case skinMaterials.BaseFileName
                        Dim ImportSkinmaterial As skinMaterials
                        SkinMaterialsTasks.Add(Task.Run(Function()
                                                            ImportSkinmaterial = New skinMaterials(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                            Return ImportSkinmaterial.ImportFile()
                                                        End Function))
                    Case sovereigntyUpgrades.BaseFileName
                        Dim ImportSovUpgrades As sovereigntyUpgrades
                        SovereigntyUpgradesTasks.Add(Task.Run(Function()
                                                                  ImportSovUpgrades = New sovereigntyUpgrades(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                  Return ImportSovUpgrades.ImportFile()
                                                              End Function))
                    Case stationOperations.BaseFileName
                        Dim ImportStationOps As stationOperations
                        StationOperationsTasks.Add(Task.Run(Function()
                                                                ImportStationOps = New stationOperations(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                                Return ImportStationOps.ImportFile()
                                                            End Function))
                    Case stationServices.BaseFileName
                        Dim Importraces As stationServices
                        StationServicesTasks.Add(Task.Run(Function()
                                                              Importraces = New stationServices(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                              Return Importraces.ImportFile()
                                                          End Function))
                    Case typeBonus.BaseFileName
                        Dim ImportTypeBonus As typeBonus
                        TypeBonusesTasks.Add(Task.Run(Function()
                                                          ImportTypeBonus = New typeBonus(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                          Return ImportTypeBonus.ImportFile()
                                                      End Function))
                    Case typeDogma.BaseFileName
                        Dim ImportTypeDogma As typeDogma
                        TypeDogmaTasks.Add(Task.Run(Function()
                                                        ImportTypeDogma = New typeDogma(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                        Return ImportTypeDogma.ImportFile()
                                                    End Function))
                    Case typeMaterials.BaseFileName
                        Dim ImportTypeMaterials As typeMaterials
                        TypeMaterialsTasks.Add(Task.Run(Function()
                                                            ImportTypeMaterials = New typeMaterials(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                            Return ImportTypeMaterials.ImportFile()
                                                        End Function))
                    Case types.BaseFileName
                        Dim ImportTypes As types
                        TypesTasks.Add(Task.Run(Function()
                                                    ImportTypes = New types(.FileName, UserApplicationSettings.SDEDirectory, Nothing)
                                                    Return ImportTypes.ImportFile()
                                                End Function))

                End Select
            End With

            If CancelImport Then
                GoTo CancelImportProcessing
            End If
        Next

        ' Wait for all tasks to complete
        Task.WaitAll(AgentsInSpaceTasks.ToArray())
        Task.WaitAll(AgentTypesTasks.ToArray())
        Task.WaitAll(AncestriesTasks.ToArray())
        Task.WaitAll(BloodlinesTasks.ToArray())
        Task.WaitAll(BlueprintsTasks.ToArray())
        Task.WaitAll(CategoriesTasks.ToArray())
        Task.WaitAll(CertificatesTasks.ToArray())
        Task.WaitAll(CharacterAttributesTasks.ToArray())
        Task.WaitAll(CloneGradesTasks.ToArray())
        Task.WaitAll(CompressibleTypesTasks.ToArray())
        Task.WaitAll(ContrabandTypesTasks.ToArray())
        Task.WaitAll(ControlTowerResourcesTasks.ToArray())
        Task.WaitAll(CorporationActivitiesTasks.ToArray())
        Task.WaitAll(DebuffTypesTasks.ToArray())
        Task.WaitAll(DogmaAttributesTasks.ToArray())
        Task.WaitAll(DogmaAttributeCategoriesTasks.ToArray())
        Task.WaitAll(DogmaEffectsTasks.ToArray())
        Task.WaitAll(DogmaUnitsTasks.ToArray())
        Task.WaitAll(DynamicItemAttributesTasks.ToArray())
        Task.WaitAll(FactionTasks.ToArray())
        Task.WaitAll(FreelanceJobSchemaTasks.ToArray())
        Task.WaitAll(GraphicTasks.ToArray())
        Task.WaitAll(GroupTasks.ToArray())
        Task.WaitAll(IconTasks.ToArray())
        Task.WaitAll(LandmarkTasks.ToArray())
        Task.WaitAll(MapAsteroidsTasks.ToArray())
        Task.WaitAll(MapConstellationsTasks.ToArray())
        Task.WaitAll(MapMoonsTasks.ToArray())
        Task.WaitAll(MapPlanetsTasks.ToArray())
        Task.WaitAll(MapSecondarySunsTasks.ToArray())
        Task.WaitAll(MapRegionsTasks.ToArray())
        Task.WaitAll(MapSolarSystemsTasks.ToArray())
        Task.WaitAll(MapStargatesTasks.ToArray())
        Task.WaitAll(MapStarsTasks.ToArray())
        Task.WaitAll(MarketGroupsTasks.ToArray())
        Task.WaitAll(MasteriesTasks.ToArray())
        Task.WaitAll(MetaGroupsTasks.ToArray())
        Task.WaitAll(MercenaryTacticalTasks.ToArray())
        Task.WaitAll(NPCCharactersTasks.ToArray())
        Task.WaitAll(NPCCorporationDivisionsTasks.ToArray())
        Task.WaitAll(NPCCorporationsTasks.ToArray())
        Task.WaitAll(NPCStationsTasks.ToArray())
        Task.WaitAll(PlanetResourcesTasks.ToArray())
        Task.WaitAll(PlanetSchematicsTasks.ToArray())
        Task.WaitAll(RaceTasks.ToArray())
        Task.WaitAll(SkinsTasks.ToArray())
        Task.WaitAll(SkinLicensesTasks.ToArray())
        Task.WaitAll(SkinMaterialsTasks.ToArray())
        Task.WaitAll(SovereigntyUpgradesTasks.ToArray())
        Task.WaitAll(StationOperationsTasks.ToArray())
        Task.WaitAll(StationServicesTasks.ToArray())
        Task.WaitAll(TypeBonusesTasks.ToArray())
        Task.WaitAll(TypeDogmaTasks.ToArray())
        Task.WaitAll(TypeMaterialsTasks.ToArray())
        Task.WaitAll(TypesTasks.ToArray())

        ' Now, get the data imported from the threads
        Dim AgentsInSpaceData As Dictionary(Of Long, agentInSpace) = GetData(AgentsInSpaceTasks)
        Dim AgentTypesData As Dictionary(Of Long, agentType) = GetData(AgentTypesTasks)
        Dim AncestriesData As Dictionary(Of Long, ancestry) = GetData(AncestriesTasks)
        Dim BloodlinesData As Dictionary(Of Long, bloodline) = GetData(BloodlinesTasks)
        Dim BlueprintsData As Dictionary(Of Long, blueprint) = GetData(BlueprintsTasks)
        Dim CategoriesData As Dictionary(Of Long, category) = GetData(CategoriesTasks)
        Dim CertificatesData As Dictionary(Of Long, certificate) = GetData(CertificatesTasks)
        Dim CharacterAttributesData As Dictionary(Of Long, characterAttribute) = GetData(CharacterAttributesTasks)
        Dim CloneGradesData As Dictionary(Of Long, cloneGrade) = GetData(CloneGradesTasks)
        Dim CompressedData As Dictionary(Of Long, compressibleType) = GetData(CompressibleTypesTasks)
        Dim ContrabandTypesData As Dictionary(Of Long, contrabandType) = GetData(ContrabandTypesTasks)
        Dim ControlTowerResourcesData As Dictionary(Of Long, _controlTowerResources) = GetData(ControlTowerResourcesTasks)
        Dim CorporationActivitiesData As Dictionary(Of Long, corporationActivity) = GetData(CorporationActivitiesTasks)
        Dim DebuffTypesData As Dictionary(Of Long, dbuffCollection) = GetData(DebuffTypesTasks)
        Dim DogmaAttributesData As Dictionary(Of Long, dogmaAttribute) = GetData(DogmaAttributesTasks)
        Dim DogmaAttributeCategoriesData As Dictionary(Of Long, dogmaAttributeCategory) = GetData(DogmaAttributeCategoriesTasks)
        Dim DogmaEffectsData As Dictionary(Of Long, dogmaEffect) = GetData(DogmaEffectsTasks)
        Dim DogmaUnitsData As Dictionary(Of Long, dogmaUnit) = GetData(DogmaUnitsTasks)
        Dim DynamicItemAttributesData As Dictionary(Of Long, dynamicItemAttribute) = GetData(DynamicItemAttributesTasks)
        Dim FactionData As Dictionary(Of Long, faction) = GetData(FactionTasks)
        Dim FreelanceJobData As Dictionary(Of Long, freelanceJob) = GetData(FreelanceJobSchemaTasks)
        Dim GraphicData As Dictionary(Of Long, graphic) = GetData(GraphicTasks)
        Dim GroupData As Dictionary(Of Long, group) = GetData(GroupTasks)
        Dim IconData As Dictionary(Of Long, _icon) = GetData(IconTasks)
        Dim LandmarkData As Dictionary(Of Long, landmark) = GetData(LandmarkTasks)
        Dim MapAsteroidsData As Dictionary(Of Long, asteroidBelt) = GetData(MapAsteroidsTasks)
        Dim MapConstellationsData As Dictionary(Of Long, constellation) = GetData(MapConstellationsTasks)
        Dim MapMoonsData As Dictionary(Of Long, moon) = GetData(MapMoonsTasks)
        Dim MapPlanetsData As Dictionary(Of Long, planet) = GetData(MapPlanetsTasks)
        Dim MapSecondarySunsData As Dictionary(Of Long, secondSun) = GetData(MapSecondarySunsTasks)
        Dim MapRegionsData As Dictionary(Of Long, region) = GetData(MapRegionsTasks)
        Dim MapSolarSystemsData As Dictionary(Of Long, solarSystem) = GetData(MapSolarSystemsTasks)
        Dim MapStargatesData As Dictionary(Of Long, stargate) = GetData(MapStargatesTasks)
        Dim MapStarsData As Dictionary(Of Long, star) = GetData(MapStarsTasks)
        Dim MarketGroupsData As Dictionary(Of Long, marketGroup) = GetData(MarketGroupsTasks)
        Dim MasteriesData As Dictionary(Of Long, Dictionary(Of Integer, List(Of Integer))) = GetData(MasteriesTasks)
        Dim MercenaryTacticalData As Dictionary(Of Long, tacticalOperation) = GetData(MercenaryTacticalTasks)
        Dim MetaGroupsData As Dictionary(Of Long, metaGroup) = GetData(MetaGroupsTasks)
        Dim NPCCharactersData As Dictionary(Of Long, npcCharacter) = GetData(NPCCharactersTasks)
        Dim npcCorporationDivisionsData As Dictionary(Of Long, npcCorporationDivision) = GetData(NPCCorporationDivisionsTasks)
        Dim NPCCorporationsData As Dictionary(Of Long, npcCorporation) = GetData(NPCCorporationsTasks)
        Dim NPCStationsData As Dictionary(Of Long, npcStation) = GetData(NPCStationsTasks)
        Dim PlanetResourcesData As Dictionary(Of Long, planetResource) = GetData(PlanetResourcesTasks)
        Dim PlanetSchematicsData As Dictionary(Of Long, planetSchematic) = GetData(PlanetSchematicsTasks)
        Dim RaceData As Dictionary(Of Long, race) = GetData(RaceTasks)
        Dim SkinsData As Dictionary(Of Long, skin) = GetData(SkinsTasks)
        Dim SkinLicensesData As Dictionary(Of Long, skinLicense) = GetData(SkinLicensesTasks)
        Dim SkinMaterialsData As Dictionary(Of Long, skinMaterial) = GetData(SkinMaterialsTasks)
        Dim SovereigntyUpgradesData As Dictionary(Of Long, sovereigntyUpgrade) = GetData(SovereigntyUpgradesTasks)
        Dim StationOperationsData As Dictionary(Of Long, stationOperation) = GetData(StationOperationsTasks)
        Dim StationServicesData As Dictionary(Of Long, stationService) = GetData(StationServicesTasks)
        Dim TypeBonusesData As Dictionary(Of Long, bonus) = GetData(TypeBonusesTasks)
        Dim TypeDogmaData As Dictionary(Of Long, dogmatype) = GetData(TypeDogmaTasks)
        Dim TypeMaterialsData As Dictionary(Of Long, typeMaterial) = GetData(TypeMaterialsTasks)
        Dim TypesData As Dictionary(Of Long, typeID) = GetData(TypesTasks)

        ' Finally, import each set of data into the database
        Dim IC As New ImportCoordinator
        For Each SDEFile In ImportFileList
            If CancelImport Then
                GoTo CancelImportProcessing
            End If

            ' Set the row location
            Parameters.RowLocation = SDEFile.RowLocation
            With IC
                Select Case SDEFile.FileName
                    Case agentsinSpace.BaseFileName
                        .RunDatabaseImport(Of agentInSpace)(New agentsinSpace(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), AgentsInSpaceData, Parameters)
                    Case agentTypes.BaseFileName
                        .RunDatabaseImport(Of agentType)(New agentTypes(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), AgentTypesData, Parameters)
                    Case ancestries.BaseFileName
                        .RunDatabaseImport(Of ancestry)(New ancestries(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), AncestriesData, Parameters)
                    Case bloodLines.BaseFileName
                        .RunDatabaseImport(Of bloodline)(New bloodLines(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), BloodlinesData, Parameters)
                    Case blueprints.BaseFileName
                        .RunDatabaseImport(Of blueprint)(New blueprints(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), BlueprintsData, Parameters)
                    Case categories.BaseFileName
                        .RunDatabaseImport(Of category)(New categories(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), CategoriesData, Parameters)
                    Case certificates.BaseFileName
                        .RunDatabaseImport(Of certificate)(New certificates(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), CertificatesData, Parameters)
                    Case mapMoons.BaseFileName
                        .RunDatabaseImport(Of moon)(New mapMoons(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapMoonsData, Parameters)
                    Case characterAttributes.BaseFileName
                        .RunDatabaseImport(Of characterAttribute)(New characterAttributes(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), CharacterAttributesData, Parameters)
                    Case cloneGrades.BaseFileName
                        .RunDatabaseImport(Of cloneGrade)(New cloneGrades(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), CloneGradesData, Parameters)
                    Case compressibleTypes.BaseFileName
                        .RunDatabaseImport(Of compressibleType)(New compressibleTypes(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), CompressedData, Parameters)
                    Case contrabandTypes.BaseFileName
                        .RunDatabaseImport(Of contrabandType)(New contrabandTypes(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), ContrabandTypesData, Parameters)
                    Case controlTowerResources.BaseFileName
                        .RunDatabaseImport(Of _controlTowerResources)(New controlTowerResources(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), ControlTowerResourcesData, Parameters)
                    Case corporationActivities.BaseFileName
                        .RunDatabaseImport(Of corporationActivity)(New corporationActivities(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), CorporationActivitiesData, Parameters)
                    Case dbuffCollections.BaseFileName
                        .RunDatabaseImport(Of dbuffCollection)(New dbuffCollections(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), DebuffTypesData, Parameters)
                    Case dogmaAttributes.BaseFileName
                        .RunDatabaseImport(Of dogmaAttribute)(New dogmaAttributes(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), DogmaAttributesData, Parameters)
                    Case dogmaAttributeCategories.BaseFileName
                        .RunDatabaseImport(Of dogmaAttributeCategory)(New dogmaAttributeCategories(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), DogmaAttributeCategoriesData, Parameters)
                    Case dogmaEffects.BaseFileName
                        .RunDatabaseImport(Of dogmaEffect)(New dogmaEffects(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), DogmaEffectsData, Parameters)
                    Case dogmaUnits.BaseFileName
                        .RunDatabaseImport(Of dogmaUnit)(New dogmaUnits(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), DogmaUnitsData, Parameters)
                    Case dynamicItemAttributes.BaseFileName
                        .RunDatabaseImport(Of dynamicItemAttribute)(New dynamicItemAttributes(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), DynamicItemAttributesData, Parameters)
                    Case factions.BaseFileName
                        .RunDatabaseImport(Of faction)(New factions(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), FactionData, Parameters)
                    Case freelanceJobSchemas.BaseFileName
                        .RunDatabaseImport(Of freelanceJob)(New freelanceJobSchemas(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), FreelanceJobData, Parameters)
                    Case graphics.BaseFileName
                        .RunDatabaseImport(Of graphic)(New graphics(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), GraphicData, Parameters)
                    Case groups.BaseFileName
                        .RunDatabaseImport(Of group)(New groups(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), GroupData, Parameters)
                    Case icons.BaseFileName
                        .RunDatabaseImport(Of _icon)(New icons(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), IconData, Parameters)
                    Case landmarks.BaseFileName
                        .RunDatabaseImport(Of landmark)(New landmarks(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), LandmarkData, Parameters)
                    Case mapAsteroidBelts.BaseFileName
                        .RunDatabaseImport(Of asteroidBelt)(New mapAsteroidBelts(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapAsteroidsData, Parameters)
                    Case mapConstellations.BaseFileName
                        .RunDatabaseImport(Of constellation)(New mapConstellations(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapConstellationsData, Parameters)
                    Case mapPlanets.BaseFileName
                        .RunDatabaseImport(Of planet)(New mapPlanets(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapPlanetsData, Parameters)
                    Case mapSecondarySuns.BaseFileName
                        .RunDatabaseImport(Of secondSun)(New mapSecondarySuns(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapSecondarySunsData, Parameters)
                    Case mapRegions.BaseFileName
                        .RunDatabaseImport(Of region)(New mapRegions(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapRegionsData, Parameters)
                    Case mapSolarSystems.BaseFileName
                        .RunDatabaseImport(Of solarSystem)(New mapSolarSystems(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapSolarSystemsData, Parameters)
                    Case mapStargates.BaseFileName
                        .RunDatabaseImport(Of stargate)(New mapStargates(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapStargatesData, Parameters)
                    Case mapStars.BaseFileName
                        .RunDatabaseImport(Of star)(New mapStars(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MapStarsData, Parameters)
                    Case marketGroups.BaseFileName
                        .RunDatabaseImport(Of marketGroup)(New marketGroups(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MarketGroupsData, Parameters)
                    Case masteries.BaseFileName
                        .RunDatabaseImport(Of Dictionary(Of Integer, List(Of Integer)))(New masteries(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MasteriesData, Parameters)
                    Case metaGroups.BaseFileName
                        .RunDatabaseImport(Of metaGroup)(New metaGroups(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MetaGroupsData, Parameters)
                    Case mercenaryTacticalOperations.BaseFileName
                        .RunDatabaseImport(Of tacticalOperation)(New mercenaryTacticalOperations(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), MercenaryTacticalData, Parameters)
                    Case npcCharacters.BaseFileName
                        .RunDatabaseImport(Of npcCharacter)(New npcCharacters(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), NPCCharactersData, Parameters)
                    Case npcCorporationDivisions.BaseFileName
                        .RunDatabaseImport(Of npcCorporationDivision)(New npcCorporationDivisions(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), npcCorporationDivisionsData, Parameters)
                    Case npcCorporations.BaseFileName
                        .RunDatabaseImport(Of npcCorporation)(New npcCorporations(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), NPCCorporationsData, Parameters)
                    Case npcStations.BaseFileName
                        .RunDatabaseImport(Of npcStation)(New npcStations(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), NPCStationsData, Parameters)
                    Case planetSchematics.BaseFileName
                        .RunDatabaseImport(Of planetSchematic)(New planetSchematics(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), PlanetSchematicsData, Parameters)
                    Case planetResources.BaseFileName
                        .RunDatabaseImport(Of planetResource)(New planetResources(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), PlanetResourcesData, Parameters)
                    Case races.BaseFileName
                        .RunDatabaseImport(Of race)(New races(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), RaceData, Parameters)
                    Case skins.BaseFileName
                        .RunDatabaseImport(Of skin)(New skins(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), SkinsData, Parameters)
                    Case skinLicenses.BaseFileName
                        .RunDatabaseImport(Of skinLicense)(New skinLicenses(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), SkinLicensesData, Parameters)
                    Case skinMaterials.BaseFileName
                        .RunDatabaseImport(Of skinMaterial)(New skinMaterials(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), SkinMaterialsData, Parameters)
                    Case sovereigntyUpgrades.BaseFileName
                        .RunDatabaseImport(Of sovereigntyUpgrade)(New sovereigntyUpgrades(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), SovereigntyUpgradesData, Parameters)
                    Case stationOperations.BaseFileName
                        .RunDatabaseImport(Of stationOperation)(New stationOperations(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), StationOperationsData, Parameters)
                    Case stationServices.BaseFileName
                        .RunDatabaseImport(Of stationService)(New stationServices(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), StationServicesData, Parameters)
                    Case typeBonus.BaseFileName
                        .RunDatabaseImport(Of bonus)(New typeBonus(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), TypeBonusesData, Parameters)
                    Case typeDogma.BaseFileName
                        .RunDatabaseImport(Of dogmatype)(New typeDogma(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), TypeDogmaData, Parameters)
                    Case typeMaterials.BaseFileName
                        .RunDatabaseImport(Of typeMaterial)(New typeMaterials(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), TypeMaterialsData, Parameters)
                    Case types.BaseFileName
                        .RunDatabaseImport(Of typeID)(New types(SDEFile.FileName, UserApplicationSettings.SDEDirectory, UpdateDatabase), TypesData, Parameters)
                End Select

            End With
        Next
#End Region

CSVImport:

        ' If a CSV import, then process each of these for the databases that need it
        If DBType = DatabaseType.MySQL Then
            Dim NewCSVDB As New CSVDB(WorkingDirectory, Nothing, True, False, False, False)
            Call BuildEVEDatabase(NewCSVDB, DatabaseType.CSV)
            ' MySQL has a fixed Upload directory, so don't delete it
        End If

FinalizeProcessing:

        lblStatus.Text = "Finalizing import..."
        Application.DoEvents()

        ' CVS won't get updated names for celestials but the field will be there blank
        ' For MySQL imports, this will work fine since it's done on the MySQL DB, but for
        ' straight CSV it won't do anything.
        If DBType <> DatabaseType.CSV Then
            Call UpdateDatabase.SetCelestialNames()
        End If

        ' Finalize
        If Not CancelImport And DBType <> DatabaseType.JSON Then
            ' Finalize
            UpdateDatabase.FinalizeDataImport()
        End If

CancelImportProcessing:

        Try
            Call ResetProgressColumn()
            GC.Collect()
            GC.WaitForPendingFinalizers()
        Catch ex As Exception

        End Try

        lblStatus.Text = ""
        Application.DoEvents()

    End Sub

    Private Function GetData(Of T)(SentTasks As List(Of Task(Of Dictionary(Of Long, T)))) As Dictionary(Of Long, T)
        Return If(SentTasks.Count > 0 AndAlso SentTasks(0).Result IsNot Nothing, SentTasks(0).Result, New Dictionary(Of Long, T))
    End Function

    ''' <summary>
    ''' Conducts error checks and returns boolean to determine if they pass
    ''' </summary>
    ''' <param name="CheckFileSelection">Boolean to check selection of files in the table.</param>
    ''' <returns>Boolean on whether the checks passed</returns>
    Private Function ConductErrorChecks(CheckFileSelection As Boolean) As Boolean

        ' Error / Data checks
        If CheckedFilesList.Count = 0 And CheckFileSelection Then
            Call MsgBox("No files selected for import", vbInformation, Application.ProductName)
            Return False
        End If

        If Trim(txtDBName.Text) = "" Then
            Call MsgBox("You must select a databasename", vbInformation, Application.ProductName)
            txtDBName.Focus()
            Return False
        End If

        If Trim(lblSDEPath.Text) = "" Then
            Call MsgBox("You must select a path for the SDE files.", vbInformation, Application.ProductName)
            btnSelectSDEPath.Focus()
            Return False
        End If

        ' Do error checks based on the selections
        If rbtnAccess.Checked Or rbtnSQLiteDB.Checked Or rbtnCSV.Checked Then
            If Trim(lblFinalDBPath.Text) = "" Then
                Call MsgBox("You must select a final database path.", vbInformation, Application.ProductName)
                Return False
            End If
        End If

        If rbtnSQLServer.Checked Or rbtnMySQL.Checked Or rbtnPostgreSQL.Checked Then
            ' Check server name
            If Trim(txtServerName.Text) = "" Then
                Call MsgBox("You must select a server name", vbInformation, Application.ProductName)
                txtServerName.Focus()
                Return False
            End If
        End If

        If rbtnMySQL.Checked Or rbtnPostgreSQL.Checked Then
            ' Check user name
            If Trim(txtUserName.Text) = "" Then
                Call MsgBox("You must select a user name", vbInformation, Application.ProductName)
                txtUserName.Focus()
                Return False
            End If
        End If

        If rbtnMySQL.Checked Or rbtnPostgreSQL.Checked Then ' Access and sqlserver password can be blank
            ' Check password
            If Trim(txtPassword.Text) = "" Then
                Call MsgBox("You must select a password", vbInformation, Application.ProductName)
                txtPassword.Focus()
                Return False
            End If
        End If

        If rbtnPostgreSQL.Checked Then
            ' Check port - MySQL port can use 3306 as default if not set
            If Trim(txtPort.Text) = "" Then
                Call MsgBox("You must select a port number", vbInformation, Application.ProductName)
                txtPort.Focus()
                Return False
            End If
        End If

        Return True

    End Function

    ''' <summary>
    ''' Imports all the files in the path into an output folder like with CSV files
    ''' </summary>
    ''' <param name="JSONlFileFolder"></param>
    Public Sub ImportJSONDatabase(JSONOutputFolder As String)
        Dim FilesList As String() = Directory.GetFiles(UserApplicationSettings.SDEDirectory)
        Dim counter As Integer = 0

        Dim ImportFileList As New List(Of FileListItem)
        ImportFileList = GetImportFileList()

        Call InitalizeMainProgressBar(FilesList.Count - 1, "Importing JSONl files...")

        For Each JSONlFile In FilesList
            Dim fileName As String = Path.GetFileNameWithoutExtension(JSONlFile).ToLower()
            ' only load the ones they checked
            If ImportFileList.Exists(Function(f) f.FileName.ToLower() = fileName) Then
                counter += 1
                Call UpdateMainProgressBar(counter, "Copying " & Path.GetFileNameWithoutExtension(JSONlFile))
                ConvertJsonlToJson(JSONlFile, JSONOutputFolder)
            End If
        Next

        Call ClearMainProgressBar()

    End Sub

    ''' <summary>
    ''' Converts a JSONL file (one JSON object per line) into a JSON array file.
    ''' </summary>
    ''' <param name="inputPath">Path to the .jsonl file</param>
    ''' <param name="outputPath">Path to write the .json file</param>
    Public Sub ConvertJsonlToJson(inputPath As String, outputPath As String)
        Dim options As New JsonSerializerOptions With {
            .WriteIndented = True
        }

        Dim items As New List(Of JsonElement)

        Using reader As New StreamReader(inputPath)
            While Not reader.EndOfStream
                Dim line As String = reader.ReadLine().Trim()
                If String.IsNullOrWhiteSpace(line) Then Continue While

                ' Parse each line as a JSON element
                Dim element As JsonElement = JsonSerializer.Deserialize(Of JsonElement)(line)
                items.Add(element)
            End While
        End Using

        ' Format the outputpath to the new file name
        Dim Outputfile As String = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(inputPath) & ".json")

        ' Serialize the list as a JSON array
        Dim jsonOutput As String = JsonSerializer.Serialize(items, options)
        File.WriteAllText(Outputfile, jsonOutput)

    End Sub

#Region "UpdaterFunctions"

    ''' <summary>
    ''' Checks for program file updates and prompts user to continue
    ''' </summary>
    Public Sub CheckForUpdates(ByVal ShowUpdateMessage As Boolean)
        Dim Response As DialogResult
        ' Program Updater
        Dim Updater As New ProgramUpdater
        Dim UpdateCode As UpdateCheckResult

        ' 1 = Update Available, 0 No Update Available, -1 an error occured and msg box already shown
        UpdateCode = Updater.IsProgramUpdatable

        Select Case UpdateCode
            Case UpdateCheckResult.UpdateAvailable

                Response = MsgBox("Update Available - Do you want to update now?", MessageBoxButtons.YesNo, Application.ProductName)

                If Response = DialogResult.Yes Then
                    ' Run the updater
                    Application.UseWaitCursor = True
                    Call Updater.RunUpdate()
                    Application.UseWaitCursor = False
                End If
            Case UpdateCheckResult.UpToDate
                If ShowUpdateMessage Then
                    MsgBox("No updates available.", vbInformation, Application.ProductName)
                End If
            Case UpdateCheckResult.UpdateError
                MsgBox("Unable to run update at this time. Please try again later.", vbInformation, Application.ProductName)
        End Select

        ' Clean up files used to check
        Call Updater.CleanUpFiles()

    End Sub

    Private Sub PrepareFilesForUpdateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PrepareFilesForUpdateToolStripMenuItem.Click
        Call CopyFilesBuildXML()
    End Sub

    Private Sub BuildBinaryToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BuildBinaryToolStripMenuItem.Click
        Call BuildBinaryFile()
    End Sub

    ''' <summary>
    ''' Copies all the files from directories and then builds the xml file and saves it here for upload to github
    ''' </summary>
    Private Sub CopyFilesBuildXML()
        Dim NewFilesAdded As Boolean = False

        On Error Resume Next
        Me.Cursor = Cursors.WaitCursor
        Application.DoEvents()

        If MD5CalcFile(Path.Combine(EXEFileFolder, MainEXEFile)) <> MD5CalcFile(Path.Combine(LatestFilesFolder, MainEXEFile)) Then
            File.Copy(Path.Combine(EXEFileFolder, MainEXEFile), Path.Combine(LatestFilesFolder, MainEXEFile), True)
            NewFilesAdded = True
        End If

        If MD5CalcFile(Path.Combine(EXEFileFolder, UpdaterEXEFile)) <> MD5CalcFile(Path.Combine(LatestFilesFolder, UpdaterEXEFile)) Then
            File.Copy(Path.Combine(EXEFileFolder, UpdaterEXEFile), Path.Combine(LatestFilesFolder, UpdaterEXEFile), True)
            NewFilesAdded = True
        End If

        'Copy all the remaining DLLs in the folder if they are updated
        Dim DLLFiles As String() = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
        Dim FileName As String

        For Each DLL In DLLFiles
            FileName = System.IO.Path.GetFileName(DLL)
            If MD5CalcFile(Path.Combine(EXEFileFolder, FileName)) <> MD5CalcFile(Path.Combine(LatestFilesFolder, FileName)) Then
                File.Copy(Path.Combine(EXEFileFolder, FileName), Path.Combine(LatestFilesFolder, FileName), True)
                NewFilesAdded = True
            End If
        Next

        On Error GoTo 0

        ' Output the Latest XML File if we have updates
        If NewFilesAdded Then
            Call WriteLatestXMLFile()
        End If

        Me.Cursor = Cursors.Default
        Application.DoEvents()

        MsgBox("Files Deployed, upload to Github for user download.", vbInformation, "Complete")

    End Sub

    ''' <summary>
    ''' Writes the sent settings to the final update file name
    ''' </summary>
    Private Sub WriteLatestXMLFile()
        Dim VersionNumber As String = String.Format("Version {0}", My.Application.Info.Version.ToString)
        Dim XMLFilePath As String = Path.Combine(EXEFileFolder, LatestVersionXML)

        ' Create XmlWriterSettings.
        Dim XMLSettings As New XmlWriterSettings With {
            .Indent = True
        }

        ' Delete the current latestversion file to rebuild
        File.Delete(XMLFilePath)

        ' Loop through the settings sent and output each name and value
        Using writer As XmlWriter = XmlWriter.Create(XMLFilePath, XMLSettings)
            writer.WriteStartDocument()
            writer.WriteStartElement("EVESDEDB") ' Root.
            writer.WriteAttributeString("Version", VersionNumber)
            writer.WriteStartElement("LastUpdated")
            writer.WriteString(CStr(Now))
            writer.WriteEndElement()

            writer.WriteStartElement("result")
            writer.WriteStartElement("rowset")
            writer.WriteAttributeString("name", "filelist")
            writer.WriteAttributeString("key", "version")
            writer.WriteAttributeString("columns", "Name,Version,MD5,URL")

            ' Main EXE program
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", MainEXEFile)
            writer.WriteAttributeString("Version", VersionNumber)
            writer.WriteAttributeString("MD5", MD5CalcFile(Path.Combine(LatestFilesFolder, MainEXEFile)))
            writer.WriteAttributeString("URL", MainEXEFileURL)
            writer.WriteEndElement()

            ' Updater EXE
            writer.WriteStartElement("row")
            writer.WriteAttributeString("Name", UpdaterEXEFile)
            writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(Path.Combine(EXEFileFolder, UpdaterEXEFile)).FileVersion)
            writer.WriteAttributeString("MD5", MD5CalcFile(Path.Combine(LatestFilesFolder, UpdaterEXEFile)))
            writer.WriteAttributeString("URL", UpdaterEXEFileURL)
            writer.WriteEndElement()

            ' Add all the dlls 
            Dim DLLFiles As String() = Directory.GetFiles(LatestFilesFolder, "*.dll")
            Dim FileName As String
            For Each DLL In DLLFiles
                FileName = System.IO.Path.GetFileName(DLL)
                writer.WriteStartElement("row")
                writer.WriteAttributeString("Name", FileName)
                writer.WriteAttributeString("Version", FileVersionInfo.GetVersionInfo(Path.Combine(EXEFileFolder, FileName)).FileVersion)
                writer.WriteAttributeString("MD5", MD5CalcFile(Path.Combine(LatestFilesFolder, FileName)))
                writer.WriteAttributeString("URL", "https://raw.githubusercontent.com/EVEIPH/EVE-SDE-Database-Builder/master/Latest%20Files/" & FileName)
                writer.WriteEndElement()
            Next

            ' End document.
            writer.WriteEndDocument()
        End Using

        ' Finally, replace all the update file's crlf with lf so that when it's uploaded to git, it works properly on download
        Dim FileText As String = File.ReadAllText(XMLFilePath)
        FileText = FileText.Replace(vbCrLf, Chr(10))

        ' Write the file back out with new formatting - locally for this debugging
        File.WriteAllText(XMLFilePath, FileText)
        ' And write it to the latest files folder too
        File.WriteAllText(Path.Combine(LatestFilesFolder, LatestVersionXML), FileText)

    End Sub

    ''' <summary>
    ''' Builds the binary file for downloading and installing the program to run
    ''' </summary>
    Private Sub BuildBinaryFile()
        ' Build this in the working directory
        Dim FinalBinaryFolderPath As String = Path.Combine(LatestFilesFolder, "Temp")
        Dim FinalBinaryZipPath As String = LatestFilesFolder
        Dim FinalBinaryZipFileName As String = "EVE SDE Database Builder Install.zip"

        Application.UseWaitCursor = True
        Application.DoEvents()

        ' Make folder to put files in and zip
        Call CreateNewDirectory(FinalBinaryFolderPath)

        ' Copy all these files from the media file directory (should be most up to date) to the working directory to make the zip
        File.Copy(Path.Combine(LatestFilesFolder, LatestVersionXML), Path.Combine(FinalBinaryFolderPath, LatestVersionXML))
        File.Copy(Path.Combine(LatestFilesFolder, MainEXEFile), Path.Combine(FinalBinaryFolderPath, MainEXEFile))
        File.Copy(Path.Combine(LatestFilesFolder, UpdaterEXEFile), Path.Combine(FinalBinaryFolderPath, UpdaterEXEFile))

        Dim DLLFiles As String() = Directory.GetFiles(LatestFilesFolder, "*.dll")
        Dim FileName As String
        For Each DLL In DLLFiles
            FileName = System.IO.Path.GetFileName(DLL)
            File.Copy(Path.Combine(LatestFilesFolder, FileName), Path.Combine(FinalBinaryFolderPath, FileName))
        Next

        ' Delete the file if it already exists
        File.Delete(Path.Combine(FinalBinaryZipPath, FinalBinaryZipFileName))
        ' Compress the whole file for download
        Call ZipFile.CreateFromDirectory(FinalBinaryFolderPath, Path.Combine(FinalBinaryZipPath, FinalBinaryZipFileName), CompressionLevel.Optimal, False)

        ' Clean up working folder
        DeleteMyDirectory(FinalBinaryFolderPath)

        Application.UseWaitCursor = False
        Application.DoEvents()

        MsgBox("Binary Built", vbInformation, "Complete")

    End Sub

#End Region

#Region "Grid and file list functions"

    ''' <summary>
    ''' Imports the file names for processing from the grid 
    ''' </summary>
    ''' <returns>List of files to process</returns>
    Private Function GetImportFileList() As List(Of FileListItem)
        Dim TempFileList As New List(Of FileListItem)

        If UserApplicationSettings.SDEDirectory <> "" Then

            Dim files = Directory.GetFiles(UserApplicationSettings.SDEDirectory)

            For Each filePath In files
                Dim fileName = Path.GetFileName(filePath)

                If CheckedFilesList.Contains(fileName) Then
                    Dim item As New FileListItem With {
                    .FileName = Path.GetFileNameWithoutExtension(fileName), ' strip off extension
                    .RowLocation = GetRowLocation(fileName)
                }
                    TempFileList.Add(item)
                End If
            Next
        End If

        Return TempFileList
    End Function


    ''' <summary>
    ''' Loads the file names into the list from the SDE Directory
    ''' </summary>
    ''' <returns>Returns boolean if was able to load the list grid or not.</returns>
    Private Function LoadFileListtoGrid() As Boolean
        Dim Counter As Long = 0
        Dim TotalFileList As New List(Of GridFileItem)

        dgMain.Rows.Clear()

        If UserApplicationSettings.SDEDirectory <> "" Then
            Try
                Dim FilesList = Directory.GetFiles(UserApplicationSettings.SDEDirectory)

                For Each filePath In FilesList
                    If filePath.EndsWith(".jsonl") Then
                        Dim fileName = Path.GetFileName(filePath)
                        Dim TempFile As New GridFileItem With {
                            .FileName = fileName,
                            .Checked = GetGridCheckValue(fileName)
                        }

                        TotalFileList.Add(TempFile)
                    End If
                Next
            Catch ex As Exception
                Call ShowErrorMessage(ex)
            End Try
        End If

        If TotalFileList.Count > 0 Then
            ' Sort the file list by name
            TotalFileList.Sort(New GridFileItemComparer)

            ' Set the rows in the grid
            dgMain.RowCount = TotalFileList.Count

            For Each SDEFilePath In TotalFileList
                If SDEFilePath.FileName = "_sde.jsonl" Then
                    ' Don't load this in grid, but it will be run
                    dgMain.RowCount -= 1
                ElseIf SDEFilePath.FileName = "translationLanguages.jsonl" Then
                    ' ignore the translation table
                    dgMain.RowCount -= 1
                Else
                    ' Add the name and a blank cell to the grid - check each one
                    dgMain.Rows(Counter).Cells(0).Value = GetGridCheckValue(SDEFilePath.FileName)
                    dgMain.Rows(Counter).Cells(1).Value = SDEFilePath.FileName
                    Counter += 1
                End If
            Next
        End If
        Application.DoEvents()

        Return True

    End Function

    ''' <summary>
    ''' Returns an integer value to determine if the row is checked in the grid with the file name given
    ''' </summary>
    ''' <param name="FileName">Filename to search the grid for a check</param>
    ''' <returns></returns>
    Private Function GetGridCheckValue(FileName As String) As Integer
        If CheckedFilesList.Contains(FileName) Then
            Return 1
        Else
            Return 0
        End If
    End Function

#End Region

#Region "Row progress update functions"

    ''' <summary>
    ''' Resets the 3rd column (index 2) in the grid for showing progress bars
    ''' </summary>
    Private Sub ResetProgressColumn()
        ' Reset the grid progress
        ' Add the progress column
        Dim PColumn As New ProgressColumn With {
            .Name = "Progress"
        }

        If dgMain.Columns.Count = 3 Then
            dgMain.Columns.Remove("Progress")
        End If

        dgMain.Columns.Add(PColumn)
        dgMain.Columns(2).Width = 400

    End Sub

    ''' <summary>
    ''' Looks up the row that has the file name and returns the row number
    ''' </summary>
    ''' <param name="FileName">Filename to search in the grid</param>
    ''' <returns>Row number</returns>
    Private Function GetRowLocation(FileName As String) As Integer
        For i = 0 To dgMain.RowCount - 1
            If dgMain.Rows(i).Cells(1).Value = FileName Then
                Return i
            End If
        Next

        Return 0
    End Function

    ''' <summary>
    ''' Initializes the grid row sent
    ''' </summary>
    ''' <param name="Postion">Grid row</param>
    Public Sub InitGridRow(ByVal Postion As Integer)
        dgMain.Rows(Postion).Cells(2).Value = 0
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Updates the grid row as a percentage for the progress bar
    ''' </summary>
    ''' <param name="Postion">Row number to update</param>
    ''' <param name="Count">Current record count</param>
    ''' <param name="TotalRecords">Total records to process</param>
    Public Sub UpdateGridRowProgress(ByVal Postion As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)
        dgMain.Rows(Postion).Cells(2).Value = CInt(Math.Floor(Count / TotalRecords * 100))
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Finalizes the grid row by setting it to 100
    ''' </summary>
    ''' <param name="Postion">Row number</param>
    Public Sub FinalizeGridRow(ByVal Postion As Integer)
        dgMain.Rows(Postion).Cells(2).Value = 100
        Application.DoEvents()
    End Sub

#End Region

#Region "Update Progress Bar on main form"

    ''' <summary>
    ''' Initializes the progress bar on the main form
    ''' </summary>
    ''' <param name="PGMaxCount">Maximum progress bar count</param>
    ''' <param name="UpdateText">Text to display in status label</param>
    Public Sub InitalizeProgress(ByVal PGMaxCount As Long, ByVal UpdateText As String)
        lblStatus.Text = UpdateText

        pgMain.Maximum = PGMaxCount
        pgMain.Value = 0
        If PGMaxCount <> 0 Then
            pgMain.Visible = True
        End If

        Application.DoEvents()

    End Sub

    ''' <summary>
    ''' Resets the progressbar and status label on main form
    ''' </summary>
    Public Sub ClearProgress()
        pgMain.Visible = False
        lblStatus.Text = ""
        Application.DoEvents()

    End Sub

    ''' <summary>
    ''' Increments the progressbar
    ''' </summary>
    ''' <param name="Count">Current count to update on progress bar.</param>
    ''' <param name="UpdateText">Text to display in the status label</param>
    Public Sub UpdateProgress(ByVal Count As Long, ByVal UpdateText As String)
        Count += 1
        If Count < pgMain.Maximum - 1 And Count <> 0 Then
            pgMain.Value = Count
            pgMain.Value = pgMain.Value - 1
            pgMain.Value = Count
        ElseIf Count >= pgMain.Maximum Then
            pgMain.Value = pgMain.Maximum
        Else
            pgMain.Value = Count
        End If

        If UpdateText <> "" Then
            lblStatus.Text = UpdateText
        End If
        Application.DoEvents()

    End Sub

#End Region

#Region "Option Checks processing"

    ''' <summary>
    ''' Processes when a check is clicked on or off in the grid
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub dgMain_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgMain.CellContentClick
        If Not FirstLoad Then
            If e.ColumnIndex = 0 Then
                dgMain.EndEdit() ' make sure it sets the check value correctly
                If Convert.ToBoolean(dgMain.CurrentCell.Value) = True Then
                    ' Checked it - add to the list
                    CheckedFilesList.Add(dgMain.Rows(e.RowIndex).Cells(1).Value)
                Else
                    ' Unchecked it - remove from the list
                    CheckedFilesList.Remove(dgMain.Rows(e.RowIndex).Cells(1).Value)
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Enables or disables check boxes, text boxes, and labels on the main form depending on options sent.
    ''' </summary>
    ''' <param name="Server">Boolean for enabling/disabling the Server Label and Textbox</param>
    ''' <param name="UserName">Boolean for enabling/disabling the User Name Label and Textbox</param>
    ''' <param name="Password">Boolean for enabling/disabling the Password Label and Textbox</param>
    ''' <param name="EUFormatCheck">Boolean for enabling/disabling the EU Format Checkbox</param>
    ''' <param name="Port">Boolean for enabling/disabling the Port Label and Textbox</param>
    ''' <param name="FinalDBFolder">Boolean for enabling/disabling the Final DB Path Label, Button, and Textbox</param>
    Private Sub SetFormObjects(Server As Boolean, UserName As Boolean, Password As Boolean, EUFormatCheck As Boolean, Port As Boolean, FinalDBFolder As Boolean)
        lblServerName.Enabled = Server
        txtServerName.Enabled = Server
        lblUserName.Enabled = UserName
        txtUserName.Enabled = UserName
        lblPassword.Enabled = Password
        txtPassword.Enabled = Password
        chkEUFormat.Visible = EUFormatCheck
        lblPort.Enabled = Port
        txtPort.Enabled = Port

        lblFinalDBPath.Enabled = FinalDBFolder
        lblFinalDBFolder.Enabled = FinalDBFolder
        btnSelectFinalDBPath.Enabled = FinalDBFolder

        If (chkEUFormat.Enabled = True And rbtnCSV.Checked) Or rbtnJSON.Checked Then
            lblDBName.Text = "Folder Name:"
            lblServerName.Visible = False
            txtServerName.Visible = False
        Else
            lblDBName.Text = "Database Name:"
            lblServerName.Visible = True
            txtServerName.Visible = True
        End If

        If rbtnSQLServer.Checked Then
            lblServerName.Text = "Instance Name:"
        Else
            lblServerName.Text = "Server Name:"
        End If

        If Not FirstLoad Then
            Call LoadFormSettings()
        End If

    End Sub

    ''' <summary>
    ''' Loads all the text boxes with settings on the main form depending on what radio button is selected
    ''' </summary>
    Private Sub LoadFormSettings()
        With UserApplicationSettings
            ' Set the variables
            If rbtnAccess.Checked Then
                txtServerName.Text = ""
                txtPassword.Text = .AccessPassword
                txtUserName.Text = ""
                txtPort.Text = ""
                ' Show password and Database Name
                lblDBName.Visible = True
                lblServerName.Visible = False
                lblUserName.Visible = False
                lblPassword.Visible = True
                lblPort.Visible = False
                txtDBName.Visible = True
                txtServerName.Visible = False
                txtUserName.Visible = False
                txtPassword.Visible = True
                txtPort.Visible = False
            ElseIf rbtnSQLiteDB.Checked Then
                txtServerName.Text = ""
                txtPassword.Text = ""
                txtUserName.Text = ""
                txtPort.Text = ""
                lblDBName.Visible = True
                lblServerName.Visible = False
                lblUserName.Visible = False
                lblPassword.Visible = False
                lblPort.Visible = False
                txtDBName.Visible = True
                txtServerName.Visible = False
                txtUserName.Visible = False
                txtPassword.Visible = False
                txtPort.Visible = False
            ElseIf rbtnSQLServer.Checked Then
                txtServerName.Text = .SQLConnectionString
                txtPassword.Text = .SQLPassword
                txtUserName.Text = .SQLUserName
                txtPort.Text = ""
                lblDBName.Visible = True
                lblServerName.Visible = True
                lblUserName.Visible = True
                lblPassword.Visible = True
                lblPort.Visible = False
                txtDBName.Visible = True
                txtServerName.Visible = True
                txtUserName.Visible = True
                txtPassword.Visible = True
                txtPort.Visible = False
            ElseIf rbtnCSV.Checked Then
                chkEUFormat.Checked = .CSVEUCheck
                txtServerName.Text = ""
                txtPassword.Text = ""
                txtUserName.Text = ""
                txtPort.Text = ""
                lblDBName.Visible = True
                lblServerName.Visible = False
                lblUserName.Visible = False
                lblPassword.Visible = False
                lblPort.Visible = False
                txtDBName.Visible = True
                txtServerName.Visible = False
                txtUserName.Visible = False
                txtPassword.Visible = False
                txtPort.Visible = False
            ElseIf rbtnJSON.Checked Then
                txtServerName.Text = ""
                txtPassword.Text = ""
                txtUserName.Text = ""
                txtPort.Text = ""
                lblDBName.Visible = True
                lblServerName.Visible = False
                lblUserName.Visible = False
                lblPassword.Visible = False
                lblPort.Visible = False
                txtDBName.Visible = True
                txtServerName.Visible = False
                txtUserName.Visible = False
                txtPassword.Visible = False
                txtPort.Visible = False
            ElseIf rbtnMySQL.Checked Then
                txtServerName.Text = .MySQLConnectionString
                txtPassword.Text = .MySQLPassword
                txtUserName.Text = .MySQLUserName
                txtPort.Text = .MySQLPort
                lblDBName.Visible = True
                lblServerName.Visible = True
                lblUserName.Visible = True
                lblPassword.Visible = True
                lblPort.Visible = True
                txtDBName.Visible = True
                txtServerName.Visible = True
                txtUserName.Visible = True
                txtPassword.Visible = True
                txtPort.Visible = True
            ElseIf rbtnPostgreSQL.Checked Then
                txtServerName.Text = .PostgreSQLConnectionString
                txtPassword.Text = .PostgreSQLPassword
                txtUserName.Text = .PostgreSQLUserName
                txtPort.Text = .PostgreSQLPort
                lblDBName.Visible = True
                lblServerName.Visible = True
                lblUserName.Visible = True
                lblPassword.Visible = True
                lblPort.Visible = True
                txtDBName.Visible = True
                txtServerName.Visible = True
                txtUserName.Visible = True
                txtPassword.Visible = True
                txtPort.Visible = True
            End If
        End With
    End Sub

    Private Sub rbtnCSV_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnCSV.CheckedChanged
        If rbtnCSV.Checked Then
            Call SetFormObjects(False, False, False, True, False, True)
        End If
    End Sub

    Private Sub rbtnJSON_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnJSON.CheckedChanged
        If rbtnJSON.Checked Then
            Call SetFormObjects(False, False, False, False, False, True)
        End If
    End Sub

    Private Sub rbtnMySQL_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMySQL.CheckedChanged
        If rbtnMySQL.Checked Then
            Call SetFormObjects(True, True, True, False, True, False)
        End If
    End Sub

    Private Sub rbtnAccess_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnAccess.CheckedChanged
        If rbtnAccess.Checked Then
            Call SetFormObjects(False, False, True, False, False, True)
        End If
    End Sub

    Private Sub rbtnSQLiteDB_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnSQLiteDB.CheckedChanged
        If rbtnSQLiteDB.Checked Then
            Call SetFormObjects(False, False, False, False, False, True)
        End If
    End Sub

    Private Sub rbtnSQLServer_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnSQLServer.CheckedChanged
        If rbtnSQLServer.Checked Then
            Call SetFormObjects(True, True, True, False, False, False)
        End If
    End Sub

    Private Sub rbtnPostgreSQL_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnPostgreSQL.CheckedChanged
        If rbtnPostgreSQL.Checked Then
            Call SetFormObjects(True, True, True, False, True, False)
        End If
    End Sub

#End Region

#Region "Click event handlers"

    Private Sub btnSelectSDEPath_Click(sender As Object, e As EventArgs) Handles btnSelectSDEPath.Click
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.SDEDirectory) Then
            FBDialog.SelectedPath = UserApplicationSettings.SDEDirectory
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                lblSDEPath.Text = FBDialog.SelectedPath
                UserApplicationSettings.SDEDirectory = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
            End Try
        End If

        ' Load the file list since they just selected the folder
        Call LoadFileListtoGrid()

    End Sub

    Private Sub btnSelectFinalDBPath_Click(sender As Object, e As EventArgs) Handles btnSelectFinalDBPath.Click
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.FinalDBPath) Then
            FBDialog.SelectedPath = UserApplicationSettings.FinalDBPath
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                lblFinalDBPath.Text = FBDialog.SelectedPath
                UserApplicationSettings.FinalDBPath = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
            End Try
        End If
    End Sub

    Private Sub btnSaveFilePath_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
        Call SaveSettings(False)
    End Sub

    Private Sub btnCheckNoGridItems_Click(sender As Object, e As EventArgs) Handles btnCheckNoGridItems.Click
        For i = 0 To dgMain.RowCount - 1
            dgMain.Rows(i).Cells(0).Value = 0
        Next

        ' Reset all checked files
        CheckedFilesList = New List(Of String)
    End Sub

    Private Sub btnCheckAllGridItems_Click(sender As Object, e As EventArgs) Handles btnCheckAllGridItems.Click
        For i = 0 To dgMain.RowCount - 1
            dgMain.Rows(i).Cells(0).Value = 1
            ' Add all rows
            CheckedFilesList.Add(dgMain.Rows(i).Cells(1).Value)
        Next
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        CancelImport = True
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        End
    End Sub

    Private Sub txtDBName_TextChanged(sender As Object, e As EventArgs) Handles txtDBName.TextChanged
        UserApplicationSettings.DatabaseName = txtDBName.Text
    End Sub

    Private Sub txtDBName_GotFocus(sender As Object, e As EventArgs) Handles txtDBName.GotFocus
        txtDBName.SelectAll()
    End Sub

    Private Sub txtServerName_GotFocus(sender As Object, e As EventArgs) Handles txtServerName.GotFocus
        txtServerName.SelectAll()
    End Sub

    Private Sub txtUserName_GotFocus(sender As Object, e As EventArgs) Handles txtUserName.GotFocus
        txtUserName.SelectAll()
    End Sub

    Private Sub txtPassword_GotFocus(sender As Object, e As EventArgs) Handles txtPassword.GotFocus
        txtPassword.SelectAll()
    End Sub

    Private Sub txtPort_GotFocus(sender As Object, e As EventArgs) Handles txtPort.GotFocus
        txtPort.SelectAll()
    End Sub

    Private Sub AboutToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem1.Click
        Dim f1 = New frmAbout
        f1.ShowDialog()
    End Sub

    Private Sub CheckForUpdatesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckForUpdatesToolStripMenuItem.Click
        Call CheckForUpdates(True)
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        End
    End Sub

#End Region

    ' Predicate for sorting a list of grid file items
    Public Class GridFileItemComparer

        Implements IComparer(Of GridFileItem)

        Public Function Compare(ByVal F1 As GridFileItem, ByVal F2 As GridFileItem) As Integer Implements IComparer(Of GridFileItem).Compare
            ' ascending sort
            Return F1.FileName.CompareTo(F2.FileName)
        End Function

    End Class

    Private Sub txtPassword_TextChanged(sender As Object, e As EventArgs) Handles txtPassword.TextChanged
        If rbtnAccess.Checked Then
            UserApplicationSettings.AccessPassword = txtPassword.Text
        ElseIf rbtnMySQL.Checked Then
            UserApplicationSettings.MySQLPassword = txtPassword.Text
        ElseIf rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLPassword = txtPassword.Text
        ElseIf rbtnSQLServer.Checked Then
            UserApplicationSettings.SQLPassword = txtPassword.Text
        End If
    End Sub

    Private Sub txtUserName_TextChanged(sender As Object, e As EventArgs) Handles txtUserName.TextChanged
        If rbtnMySQL.Checked Then
            UserApplicationSettings.MySQLUserName = txtUserName.Text
        ElseIf rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLUserName = txtUserName.Text
        ElseIf rbtnSQLServer.Checked Then
            UserApplicationSettings.SQLUserName = txtUserName.Text
        End If
    End Sub

    Private Sub txtServerName_TextChanged(sender As Object, e As EventArgs) Handles txtServerName.TextChanged
        If rbtnMySQL.Checked Then
            UserApplicationSettings.MySQLConnectionString = txtServerName.Text
        ElseIf rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLConnectionString = txtServerName.Text
        ElseIf rbtnSQLServer.Checked Then
            UserApplicationSettings.SQLConnectionString = txtServerName.Text
        End If
    End Sub

    Private Sub txtPort_TextChanged(sender As Object, e As EventArgs) Handles txtPort.TextChanged
        If rbtnPostgreSQL.Checked Then
            UserApplicationSettings.PostgreSQLPort = txtPort.Text
        End If
    End Sub

    Private Sub btnSelectDownloadPath_Click(sender As Object, e As EventArgs) Handles btnSelectDownloadPath.Click
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.SDEDirectory) Then
            FBDialog.SelectedPath = UserApplicationSettings.SDEDirectory
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                lblDownloadFolderPath.Text = FBDialog.SelectedPath
                UserApplicationSettings.DownloadFolderPath = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
                Exit Sub
            End Try
        End If

    End Sub

    Private Sub btnSelectOutputFolder_Click(sender As Object, e As EventArgs)
        FBDialog.RootFolder = Environment.SpecialFolder.Desktop

        If Directory.Exists(UserApplicationSettings.SDEDirectory) Then
            FBDialog.SelectedPath = UserApplicationSettings.SDEDirectory
        Else
            FBDialog.SelectedPath = Application.StartupPath
        End If

        If FBDialog.ShowDialog() = DialogResult.OK Then
            Try
                txtDBName.Text = FBDialog.SelectedPath
                UserApplicationSettings.DownloadFolderPath = FBDialog.SelectedPath
            Catch ex As Exception
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
                Exit Sub
            End Try
        End If
    End Sub

    Private Structure SDEData
        Dim BuildNumber As Long
        Dim ReleaseDate As Date
    End Structure

    Private Sub btnDownloadSDE_Click(sender As Object, e As EventArgs) Handles btnDownloadSDE.Click
        Dim LatestBuildFileName As String = UserApplicationSettings.DownloadFolderPath & "\" & "LatestBuild"
        Dim OldLatestBuildFileName As String = UserApplicationSettings.DownloadFolderPath & "\" & "LatestBuild-old"
        Dim NewDownloadDirectory As String ' Folder I'll download into and work with
        Dim NewBuildData As SDEData
        Dim OldBuildData As SDEData
        Dim FileDate As Date ' to save the date of the download (not used anymore)
        Dim FolderName As String

        CancelDownload = False

        If Trim(lblDownloadFolderPath.Text) = "" Then
            Call MsgBox("You must select a SDE download folder path.", vbInformation, Application.ProductName)
            btnSelectDownloadPath.Focus()
            Exit Sub
        End If

        ' Now that we have a good directory, download the build number to make sure we need an update
        If File.Exists(LatestBuildFileName) Then
            ' rename this before downloading the new one
            If File.Exists(OldLatestBuildFileName) Then
                File.Delete(OldLatestBuildFileName)
            End If
            File.Copy(LatestBuildFileName, OldLatestBuildFileName)
            File.Delete(LatestBuildFileName)
            OldBuildData = GetSDEBuildData(OldLatestBuildFileName)
        End If

        ' Get latest build number, and check it against the stored number

        ' Download the latest SDE data first
        Call DownloadFileFromServer("https://developers.eveonline.com/static-data/tranquility/latest.jsonl", LatestBuildFileName, FileDate)

        ' Get the new number
        NewBuildData = GetSDEBuildData(LatestBuildFileName)

        ' Compare the Build Numbers and check if they are different
        If IsNothing(NewBuildData) Then
            MsgBox("Failed to download Build Data. Try again.", vbExclamation, "SDE Database Builder")
            Exit Sub
        End If

        If NewBuildData.BuildNumber <> OldBuildData.BuildNumber Then
            Me.UseWaitCursor = True
            ' Need to download the new SDE
            btnDownloadSDE.Enabled = False
            btnSelectDownloadPath.Enabled = False
            gbSelectDBType.Enabled = False
            btnCancel.Enabled = False
            btnSelectSDEPath.Enabled = False
            btnSelectFinalDBPath.Enabled = False
            btnCheckAllGridItems.Enabled = False
            btnCheckNoGridItems.Enabled = False
            btnBuildDatabase.Enabled = False
            btnSaveSettings.Enabled = False
            btnClose.Enabled = False
            dgMain.Enabled = False
            btnCancelDownload.Enabled = True

            lblStatus.Text = "Preparing files..."
            Application.DoEvents()
            ' Create a folder for today's date and download the SDE into that folder - will overwrite anything there
            FolderName = MonthName(NewBuildData.ReleaseDate.Month) & "_" & CStr(NewBuildData.ReleaseDate.Day) & "_" & Year(NewBuildData.ReleaseDate)
            NewDownloadDirectory = UserApplicationSettings.DownloadFolderPath & "\" & FolderName
            Call CreateNewDirectory(NewDownloadDirectory)

            ' Now download the zip and extract into that folder
            lblStatus.Text = "Downloading SDE..."
            Call DownloadFileFromServer("https://developers.eveonline.com/static-data/tranquility/eve-online-static-data-3241024-jsonl.zip", NewDownloadDirectory & "\SDE.zip", Nothing, pgBar)

            If CancelDownload Then
                ' Delete new BuildData and restore old one
                File.Delete(LatestBuildFileName)
                If File.Exists(OldLatestBuildFileName) Then
                    ' Rename
                    File.Copy(OldLatestBuildFileName, LatestBuildFileName)
                    File.Delete(OldLatestBuildFileName)
                End If
                MsgBox("Cancelled download", vbInformation, Application.ProductName)
                GoTo CancelDownload
            End If

            ' Unzip files and set the new download folder 
            lblStatus.Text = "Extracting files..."
            btnCancelDownload.Enabled = False ' Can't cancel anymore
            Application.DoEvents()
            Call ZipFile.ExtractToDirectory(NewDownloadDirectory & "\SDE.zip", NewDownloadDirectory)

            ' Finally delete old download zip file after extracted to save space
            lblStatus.Text = "Cleaning up files..."
            Application.DoEvents()
            File.Delete(NewDownloadDirectory & "\SDE.zip")

            lblSDEPath.Text = NewDownloadDirectory
            UserApplicationSettings.SDEDirectory = NewDownloadDirectory
            ' Auto set the new folder to process
            txtDBName.Text = FolderName
            ' Save the settings as well
            Call SaveSettings(True)
            lblStatus.Text = ""

            ' Load the file list since they just selected the folder
            Call LoadFileListtoGrid()

            Call MsgBox("SDE Downloaded and saved in SDE File Folder", vbInformation, Application.ProductName)
        Else
            Call MsgBox("You have the latest SDE Version downloaded", vbInformation, Application.ProductName)
        End If

        ' Delete the old check sum file in both cases
        File.Delete(OldLatestBuildFileName)

CancelDownload:
        Me.UseWaitCursor = False

        btnDownloadSDE.Enabled = True
        btnSelectDownloadPath.Enabled = True
        gbSelectDBType.Enabled = True
        btnCancel.Enabled = True
        btnSelectSDEPath.Enabled = True
        btnSelectFinalDBPath.Enabled = True
        btnCheckAllGridItems.Enabled = True
        btnCheckNoGridItems.Enabled = True
        btnBuildDatabase.Enabled = True
        btnSaveSettings.Enabled = True
        btnClose.Enabled = True
        dgMain.Enabled = True

    End Sub
    Public Class LatestVersionData
        <JsonPropertyName("_key")> Public Property key As String
        <JsonPropertyName("buildNumber")> Public Property buildNumber As Long
        <JsonPropertyName("releaseDate")> Public Property releaseDate As String
    End Class

    ' Returns the build number from the latest build file
    Private Function GetSDEBuildData(FileName As String) As SDEData
        Dim Reader As StreamReader
        Dim BuildData As String = ""
        Dim ReturnData As SDEData
        Dim Record As LatestVersionData

        Reader = New StreamReader(FileName)
        While Not Reader.EndOfStream
            BuildData = Reader.ReadLine
        End While

        Call Reader.Dispose() ' Release

        ' Get the value
        If Not IsNothing(BuildData) Then
            Record = JsonSerializer.Deserialize(Of LatestVersionData)(BuildData)
            If Record.key = "sde" Then
                ' Just return the build number
                ReturnData.BuildNumber = Record.buildNumber
                ' Convert the date value
                Dim ReleaseDate As Date = FormatDateTime(Record.releaseDate)
                ' it's in Zulu so need to force it to be consistent since VB formats for local
                ReturnData.ReleaseDate = ReleaseDate.ToUniversalTime()
                Return ReturnData
            End If
        End If

        Return Nothing

    End Function
    Private Sub btnCancelDownload_Click(sender As Object, e As EventArgs) Handles btnCancelDownload.Click
        CancelDownload = True
    End Sub
    Private Sub frmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        'rs.ResizeAllControls(Me)
    End Sub
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' rs.FindAllControls(Me)
    End Sub
    Private Sub ResetSavedBuildNumberToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetSavedBuildNumberToolStripMenuItem.Click
        Call File.Delete(Path.Combine(UserApplicationSettings.DownloadFolderPath, "LatestBuild"))
        Call MsgBox("Database Saved Build Data file deleted.", vbInformation, Application.ProductName)
    End Sub

End Class

' For updating the data grid view
Public Class ProgressColumn
    Inherits DataGridViewColumn

    Public Sub New()
        MyBase.New(New ProgressCell())
    End Sub

    Public Overrides Property CellTemplate() As DataGridViewCell
        Get
            Return MyBase.CellTemplate
        End Get
        Set(ByVal Value As DataGridViewCell)
            ' Ensure that the cell used for the template is a ProgressCell.
            If Value IsNot Nothing And TypeOf (Value) IsNot ProgressCell Then
                Throw New InvalidCastException("Must be a ProgressCell")
            End If
            MyBase.CellTemplate = Value
        End Set
    End Property

End Class

Public Class ProgressCell
    Inherits DataGridViewImageCell
    Protected Overrides Function GetFormattedValue(ByVal value As Object, ByVal rowIndex As Integer, ByRef cellStyle As DataGridViewCellStyle,
                                                   ByVal valueTypeConverter As System.ComponentModel.TypeConverter,
                                                   ByVal formattedValueTypeConverter As System.ComponentModel.TypeConverter,
                                                   ByVal context As DataGridViewDataErrorContexts) As Object
        ' Create bitmap.
        Dim bmp As New Bitmap(Me.Size.Width, Me.Size.Height)

        Using g As Drawing.Graphics = Drawing.Graphics.FromImage(bmp)

            If Not IsNothing(Me.Value) Then
                ' Percentage.
                Dim percentage As Double = 0
                Double.TryParse(Me.Value.ToString(), percentage)
                Dim text As String = percentage.ToString() + " %"

                ' Get width and height of text.
                Dim f As New Font("Segoe UI", 11.25, FontStyle.Regular)
                Dim w As Integer = CType(g.MeasureString(text, f).Width, Integer)
                Dim h As Integer = CType(g.MeasureString(text, f).Height, Integer)

                ' Draw pile - build a white box first to cover the value in the grid so it doesn't overlap
                g.DrawRectangle(Pens.Black, 1, 1, Me.Size.Width - 6, Me.Size.Height - 6)
                g.FillRectangle(Brushes.White, 2, 2, CInt((Me.Size.Width - 7)), CInt(Me.Size.Height - 7))
                ' Draw the green progress rectangle based on the number
                g.DrawRectangle(Pens.Black, 1, 1, Me.Size.Width - 6, Me.Size.Height - 6)
                g.FillRectangle(Brushes.LimeGreen, 2, 2, CInt((Me.Size.Width - 7) * percentage / 100), CInt(Me.Size.Height - 7))

                Dim rect As New RectangleF(0, 3, bmp.Width, bmp.Height)
                Dim sf As New StringFormat With {
                    .Alignment = StringAlignment.Center
                }
                g.DrawString(text, f, Brushes.Black, rect, sf)
            End If
        End Using

        Return bmp
    End Function

End Class
