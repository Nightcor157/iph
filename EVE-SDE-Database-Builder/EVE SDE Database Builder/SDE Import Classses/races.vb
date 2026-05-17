
Public Class races
    Inherits SDEFilesBase
    Implements IImporter(Of race)
    Implements IDatabaseImporter(Of race)

    Public Const BaseFileName As String = "races"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, race) Implements IImporter(Of race).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of race)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, race), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of race).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("raceID", FieldType.int_type, 0, True),
            New DBTableField("name", FieldType.varchar_type, 100, True),
            New DBTableField("description", FieldType.varchar_type, 1000, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("shipTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim RaceSkillsTableName As String = "raceSkills"
        Table = New List(Of DBTableField) From {
            New DBTableField("raceID", FieldType.int_type, 0, False),
            New DBTableField("skillTypeID", FieldType.int_type, 0, True),
            New DBTableField("level", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(RaceSkillsTableName, Table)

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

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shipTypeID", .shipTypeID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            If Not IsNothing(DataField.Value.skills) Then
                For Each Skill In DataField.Value.skills
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("raceID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("skillTypeID", Skill._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("level", Skill._value, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(RaceSkillsTableName, UpdateDB.BuildOrderedRecord(RaceSkillsTableName, DataFields))
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class race
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property iconID As Integer?
    Public Property name As TranslatedNameField
    Public Property shipTypeID As Integer?
    Public Property skills As List(Of raceSkill) ' typeID and level
End Class

Public Class raceSkill
    Public Property _key As Long
    Public Property _value As Integer
End Class