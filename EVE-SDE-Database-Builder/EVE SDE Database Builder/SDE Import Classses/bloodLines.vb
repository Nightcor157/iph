
Public Class bloodLines
    Inherits SDEFilesBase
    Implements IImporter(Of bloodline)
    Implements IDatabaseImporter(Of bloodline)

    Public Const BaseFileName As String = "bloodlines"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, bloodline) Implements IImporter(Of bloodline).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of bloodline)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, bloodline), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of bloodline).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("bloodlineID", FieldType.int_type, 0, False, True),
            New DBTableField("bloodlineName", FieldType.nvarchar_type, 100, False),
            New DBTableField("raceID", FieldType.int_type, 0, False),
            New DBTableField("description", FieldType.nvarchar_type, 1000, False),
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("perception", FieldType.int_type, 0, False),
            New DBTableField("willpower", FieldType.int_type, 0, False),
            New DBTableField("charisma", FieldType.int_type, 0, False),
            New DBTableField("memory", FieldType.int_type, 0, False),
            New DBTableField("intelligence", FieldType.int_type, 0, False),
            New DBTableField("iconID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", .corporationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("perception", .perception, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("willpower", .willpower, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("charisma", .charisma, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("memory", .memory, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("intelligence", .intelligence, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class bloodline
    Public Property _key As Long
    Public Property charisma As Integer
    Public Property corporationID As Long
    Public Property description As TranslatedNameField
    Public Property iconID As Object
    Public Property intelligence As Integer
    Public Property memory As Integer
    Public Property name As TranslatedNameField
    Public Property perception As Integer
    Public Property raceID As Integer
    Public Property willpower As Integer
End Class