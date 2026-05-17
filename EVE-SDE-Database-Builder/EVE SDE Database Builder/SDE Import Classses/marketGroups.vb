
Public Class marketGroups
    Inherits SDEFilesBase
    Implements IImporter(Of marketGroup)
    Implements IDatabaseImporter(Of marketGroup)

    Public Const BaseFileName As String = "marketGroups"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, marketGroup) Implements IImporter(Of marketGroup).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of marketGroup)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, marketGroup), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of marketGroup).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("marketGroupID", FieldType.int_type, 0, False, True),
            New DBTableField("description", FieldType.nvarchar_type, 300, True),
            New DBTableField("hasTypes", FieldType.int_type, 0, False),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("name", FieldType.nvarchar_type, 100, False),
            New DBTableField("parentGroupID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {"marketGroupID"}
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_MGID", IndexFields)

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
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("marketGroupID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("hasTypes", BooleanField(.hasTypes), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("parentGroupID", NullableField(.parentGroupID), FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class marketGroup
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property hasTypes As Boolean
    Public Property iconID As Object
    Public Property name As TranslatedNameField
    Public Property parentGroupID As Object
End Class