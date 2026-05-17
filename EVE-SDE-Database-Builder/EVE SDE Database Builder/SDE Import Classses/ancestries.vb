
Public Class ancestries
    Inherits SDEFilesBase
    Implements IImporter(Of ancestry)
    Implements IDatabaseImporter(Of ancestry)

    Public Const BaseFileName As String = "ancestries"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, ancestry) Implements IImporter(Of ancestry).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of ancestry)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, ancestry), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of ancestry).InsertDatatoDatabase

        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("ancestryID", FieldType.int_type, 0, False, True),
            New DBTableField("name", FieldType.nvarchar_type, 100, False),
            New DBTableField("bloodlineID", FieldType.int_type, 0, False),
            New DBTableField("description", FieldType.nvarchar_type, 1000, False),
            New DBTableField("perception", FieldType.int_type, 0, False),
            New DBTableField("willpower", FieldType.int_type, 0, False),
            New DBTableField("charisma", FieldType.int_type, 0, False),
            New DBTableField("memory", FieldType.int_type, 0, False),
            New DBTableField("intelligence", FieldType.int_type, 0, False),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("shortDescription", FieldType.nvarchar_type, 500, True)
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
                DataFields.Add(UpdateDB.BuildDatabaseField("ancestryID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("bloodlineID", .bloodlineID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("perception", .perception, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("willpower", .willpower, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("charisma", .charisma, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("memory", .memory, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("intelligence", .intelligence, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", .shortDescription, FieldType.nvarchar_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class ancestry
    Public Property _key As Long
    Public Property bloodlineID As Integer
    Public Property charisma As Integer
    Public Property description As TranslatedNameField
    Public Property iconID As Object
    Public Property intelligence As Integer
    Public Property memory As Integer
    Public Property name As TranslatedNameField
    Public Property perception As Integer
    Public Property shortDescription As String
    Public Property willpower As Integer
End Class