Public Class npcCorporationDivisions
    Inherits SDEFilesBase
    Implements IImporter(Of npcCorporationDivision)
    Implements IDatabaseImporter(Of npcCorporationDivision)

    Public Const BaseFileName As String = "npcCorporationDivisions"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, npcCorporationDivision) Implements IImporter(Of npcCorporationDivision).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of npcCorporationDivision)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, npcCorporationDivision), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of npcCorporationDivision).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("divisionID", FieldType.int_type, 0, False, True),
            New DBTableField("description", FieldType.varchar_type, 500, True),
            New DBTableField("displayName", FieldType.varchar_type, 100, True),
            New DBTableField("internalName", FieldType.varchar_type, 1000, False),
            New DBTableField("leaderTypeName", FieldType.varchar_type, 100, False),
            New DBTableField("name", FieldType.varchar_type, 100, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "divisionID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_DID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("divisionID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("displayName", .displayName, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("internalName", .internalName, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("leaderTypeName", GetTranslation(.leaderTypeName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class npcCorporationDivision
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property displayName As String
    Public Property internalName As String
    Public Property leaderTypeName As TranslatedNameField
    Public Property name As TranslatedNameField
End Class