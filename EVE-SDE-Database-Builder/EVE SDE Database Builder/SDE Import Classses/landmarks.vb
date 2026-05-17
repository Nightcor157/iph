
Public Class landmarks
    Inherits SDEFilesBase
    Implements IImporter(Of landmark)
    Implements IDatabaseImporter(Of landmark)

    Public Const BaseFileName As String = "landmarks"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, landmark) Implements IImporter(Of landmark).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of landmark)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, landmark), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of landmark).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("landmarkID", FieldType.int_type, 0, False, True),
            New DBTableField("description", FieldType.nvarchar_type, -1, True),
            New DBTableField("name", FieldType.nvarchar_type, 100, True),
            New DBTableField("locationID", FieldType.int_type, 0, True),
            New DBTableField("x", FieldType.double_type, 0, True),
            New DBTableField("y", FieldType.double_type, 0, True),
            New DBTableField("z", FieldType.double_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "landmarkID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_LID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("landmarkID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("locationID", NullableField(.locationID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
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

Public Class landmark
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property iconID As Object
    Public Property locationID As Object
    Public Property name As TranslatedNameField
    Public Property position As celestialposition
End Class