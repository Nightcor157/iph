
Public Class groups
    Inherits SDEFilesBase
    Implements IImporter(Of group)
    Implements IDatabaseImporter(Of group)

    Public Const BaseFileName As String = "groups"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, group) Implements IImporter(Of group).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of group)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, group), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of group).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("groupID", FieldType.int_type, 0, False, True),
            New DBTableField("groupName", FieldType.nvarchar_type, 500, True),
            New DBTableField("categoryID", FieldType.int_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("useBasePrice", FieldType.int_type, 0, True),
            New DBTableField("anchored", FieldType.int_type, 0, True),
            New DBTableField("anchorable", FieldType.int_type, 0, True),
            New DBTableField("fittableNonSingleton", FieldType.int_type, 0, True),
            New DBTableField("published", FieldType.int_type, 0, True)
        }

        Dim ShortTableName As String = TableName
        If Params.DatabaseType = DatabaseType.MySQL Then
            ' groups is a reserved word in mysql
            TableName = "`" & TableName & "`"
        End If

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {"groupID"}
        Call UpdateDB.CreateIndex(TableName, "IDX_" & ShortTableName & "_GID", IndexFields)

        IndexFields = New List(Of String) From {"categoryID"}
        Call UpdateDB.CreateIndex(TableName, "IDX_" & ShortTableName & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("groupID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("groupName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("categoryID", .categoryID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("useBasePrice", BooleanField(.useBasePrice), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anchored", BooleanField(.anchored), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anchorable", BooleanField(.anchorable), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fittableNonSingleton", BooleanField(.fittableNonSingleton), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", BooleanField(.published), FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class group
    Public Property _key As Long
    Public Property anchorable As Boolean
    Public Property anchored As Boolean
    Public Property categoryID As Integer
    Public Property fittableNonSingleton As Boolean
    Public Property iconID As Object
    Public Property name As TranslatedNameField
    Public Property published As Boolean
    Public Property useBasePrice As Boolean

End Class