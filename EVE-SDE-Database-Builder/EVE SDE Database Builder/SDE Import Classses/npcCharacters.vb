
Public Class npcCharacters
    Inherits SDEFilesBase
    Implements IImporter(Of npcCharacter)
    Implements IDatabaseImporter(Of npcCharacter)

    Public Const BaseFileName As String = "npcCharacters"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, npcCharacter) Implements IImporter(Of npcCharacter).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of npcCharacter)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, npcCharacter), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of npcCharacter).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("npcCharacterID", FieldType.int_type, 0, False, True),
            New DBTableField("agentTypeID", FieldType.int_type, 0, True),
            New DBTableField("divisionID", FieldType.int_type, 0, True),
            New DBTableField("isLocator", FieldType.int_type, 0, True),
            New DBTableField("level", FieldType.int_type, 0, True),
            New DBTableField("ancestryID", FieldType.int_type, 0, True),
            New DBTableField("bloodlineID", FieldType.int_type, 0, False),
            New DBTableField("careerID", FieldType.int_type, 0, True),
            New DBTableField("ceo", FieldType.int_type, 0, False),
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("description", FieldType.text_type, 0, True),
            New DBTableField("gender", FieldType.int_type, 0, False),
            New DBTableField("locationID", FieldType.int_type, 0, True),
            New DBTableField("name", FieldType.nvarchar_type, 200, False),
            New DBTableField("raceID", FieldType.int_type, 0, False),
            New DBTableField("schoolID", FieldType.int_type, 0, True),
            New DBTableField("specialityID", FieldType.int_type, 0, True),
            New DBTableField("uniqueName", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "npcCharacterID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        Dim SkillsTable As New List(Of DBTableField) From {
            New DBTableField("npcCharacterID", FieldType.int_type, 0, False),
            New DBTableField("typeID", FieldType.int_type, 0, False)
        }
        Call UpdateDB.CreateTable(TableName & "Skills", SkillsTable)

        IndexFields = New List(Of String)
        IndexFields = New List(Of String) From {
            "npcCharacterID"
        }
        Call UpdateDB.CreateIndex(TableName & "Skills", "IDX_" & TableName & "Skills" & "_CID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)
        Dim TotalRecords As Long = Records.Count

        ' Process Data
        For Each DataField In Records
            DataFields = New List(Of DBField)

            If CancelImport Then Exit Sub

            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("npcCharacterID", DataField.Key, FieldType.int_type))
                If Not IsNothing(.agent) Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("agentTypeID", .agent.agentTypeID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("divisionID", .agent.divisionID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("isLocator", BooleanField(.agent.isLocator), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("level", .agent.level, FieldType.int_type))
                End If
                DataFields.Add(UpdateDB.BuildDatabaseField("ancestryID", .ancestryID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineID", .bloodlineID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("careerID", .careerID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ceo", BooleanField(.ceo), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", .corporationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("gender", BooleanField(.gender), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("locationID", .locationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.text_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("schoolID", .schoolID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("specialityID", .specialityID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", BooleanField(.uniqueName), FieldType.int_type))

                If Not IsNothing(.skills) Then
                    For Each Skill In .skills
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("npcCharacterID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("typeID", Skill.typeID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "Skills", UpdateDB.BuildOrderedRecord(TableName & "Skills", DataFields2))
                    Next
                End If
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class npcCharacter
    Public Property _key As Long
    Public Property agent As AgentInfo
    Public Property ancestryID As Integer?
    Public Property bloodlineID As Integer
    Public Property careerID As Integer?
    Public Property ceo As Boolean
    Public Property corporationID As Integer
    Public Property description As String
    Public Property gender As Boolean
    Public Property locationID As Integer?
    Public Property name As TranslatedNameField
    Public Property raceID As Integer
    Public Property schoolID As Integer?
    Public Property skills As List(Of npcCharacterSkills)
    Public Property specialityID As Integer?
    Public Property startDate As String
    Public Property uniqueName As Boolean
End Class

Public Class AgentInfo
    Public Property agentTypeID As Integer
    Public Property divisionID As Integer
    Public Property isLocator As Boolean
    Public Property level As Integer
End Class

Public Class npcCharacterSkills
    Public Property typeID As Integer
End Class

