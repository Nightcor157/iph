
Public Class skinMaterials
    Inherits SDEFilesBase
    Implements IImporter(Of skinMaterial)
    Implements IDatabaseImporter(Of skinMaterial)

    Public Const BaseFileName As String = "skinMaterials"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, skinMaterial) Implements IImporter(Of skinMaterial).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of skinMaterial)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, skinMaterial), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of skinMaterial).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("skinMaterialID", FieldType.int_type, 0, False, True),
            New DBTableField("displayName", FieldType.nvarchar_type, 100, True),
            New DBTableField("materialSetID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "skinMaterialID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        Dim TotalRecords As Long = Records.Count

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        ' Process Data
        For Each DataField In Records
            DataFields = New List(Of DBField)

            If CancelImport Then Exit Sub

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("skinMaterialID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("displayName", GetTranslation(.displayName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("materialSetID", .materialSetID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class skinMaterial
    Public Property _key As Long
    Public Property displayName As TranslatedNameField
    Public Property materialSetID As Integer
End Class