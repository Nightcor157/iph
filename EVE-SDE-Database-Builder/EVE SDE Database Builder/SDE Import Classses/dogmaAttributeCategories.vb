
Public Class dogmaAttributeCategories
    Inherits SDEFilesBase
    Implements IImporter(Of dogmaAttributeCategory)
    Implements IDatabaseImporter(Of dogmaAttributeCategory)

    Public Const BaseFileName As String = "dogmaAttributeCategories"
    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dogmaAttributeCategory) Implements IImporter(Of dogmaAttributeCategory).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dogmaAttributeCategory)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dogmaAttributeCategory), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dogmaAttributeCategory).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("categoryID", FieldType.int_type, 0, False, True),
            New DBTableField("name", FieldType.nvarchar_type, 50, True),
            New DBTableField("description", FieldType.nvarchar_type, 200, True)
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

            If CancelImport Then Exit Sub

            With DataField.Value
                ' Build the insert list
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("categoryID", DataField.Key, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("name", .name, FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("description", NullableField(.description), FieldType.nvarchar_type)
                }

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' Update grid progress
                Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
                Count += 1
            End With
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class dogmaAttributeCategory
    Public Property _key As Long
    Public Property description As Object
    Public Property name As String
End Class