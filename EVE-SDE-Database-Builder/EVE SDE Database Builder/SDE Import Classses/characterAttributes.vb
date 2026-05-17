
Public Class characterAttributes
    Inherits SDEFilesBase
    Implements IImporter(Of characterAttribute)
    Implements IDatabaseImporter(Of characterAttribute)

    Public Const BaseFileName As String = "characterAttributes"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, characterAttribute) Implements IImporter(Of characterAttribute).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of characterAttribute)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, characterAttribute), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of characterAttribute).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("attributeID", FieldType.int_type, 0, False, True),
            New DBTableField("attributeName", FieldType.varchar_type, 100, True),
            New DBTableField("description", FieldType.varchar_type, 1000, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("shortDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("notes", FieldType.nvarchar_type, 500, True)
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
                DataFields.Add(UpdateDB.BuildDatabaseField("attributeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("attributeName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", .description, FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", .shortDescription, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("notes", .notes, FieldType.nvarchar_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class characterAttribute
    Public Property _key As Long
    Public Property description As String
    Public Property iconID As Integer
    Public Property name As TranslatedNameField
    Public Property notes As String
    Public Property shortDescription As String
End Class