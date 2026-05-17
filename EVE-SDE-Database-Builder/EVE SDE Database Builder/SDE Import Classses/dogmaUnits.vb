
Public Class dogmaUnits
    Inherits SDEFilesBase
    Implements IImporter(Of dogmaUnit)
    Implements IDatabaseImporter(Of dogmaUnit)

    Public Const BaseFileName As String = "dogmaUnits"
    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dogmaUnit) Implements IImporter(Of dogmaUnit).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dogmaUnit)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dogmaUnit), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dogmaUnit).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("dogmaUnitID", FieldType.int_type, 0, False, True),
            New DBTableField("description", FieldType.nvarchar_type, 250, True),
            New DBTableField("displayName", FieldType.nvarchar_type, 100, True),
            New DBTableField("name", FieldType.nvarchar_type, 100, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "dogmaUnitID"
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

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("dogmaUnitID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("displayName", GetTranslation(.displayName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", .name, FieldType.nvarchar_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class dogmaUnit
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property displayName As TranslatedNameField
    Public Property name As String
End Class

