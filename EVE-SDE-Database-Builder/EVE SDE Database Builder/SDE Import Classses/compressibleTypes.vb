
Public Class compressibleTypes
    Inherits SDEFilesBase
    Implements IImporter(Of compressibleType)
    Implements IDatabaseImporter(Of compressibleType)

    Public Const BaseFileName As String = "compressibleTypes"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, compressibleType) Implements IImporter(Of compressibleType).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of compressibleType)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, compressibleType), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of compressibleType).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("compressibleTypeID", FieldType.int_type, 0, False, True),
            New DBTableField("compressedTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "compressibleTypeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("compressibleTypeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("compressedTypeID", .compressedTypeID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class compressibleType
    Public Property _key As Long
    Public Property compressedTypeID As Integer

End Class
