Public Class mercenaryTacticalOperations
    Inherits SDEFilesBase
    Implements IImporter(Of tacticalOperation)
    Implements IDatabaseImporter(Of tacticalOperation)

    Public Const BaseFileName As String = "mercenaryTacticalOperations"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, tacticalOperation) Implements IImporter(Of tacticalOperation).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of tacticalOperation)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, tacticalOperation), ByVal Params As ImportParameters) _
            Implements IDatabaseImporter(Of tacticalOperation).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("tacticalOperationID", FieldType.int_type, 0, False, True),
            New DBTableField("name", FieldType.nvarchar_type, 100, False),
            New DBTableField("description", FieldType.nvarchar_type, 1000, False),
            New DBTableField("anarchy_impact", FieldType.int_type, 0, False),
            New DBTableField("development_impact", FieldType.int_type, 0, False),
            New DBTableField("infomorph_bonus", FieldType.int_type, 0, False)
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
                DataFields.Add(UpdateDB.BuildDatabaseField("tacticalOperationID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("anarchy_impact", .anarchy_impact, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("development_impact", .development_impact, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("infomorph_bonus", .infomorph_bonus, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class tacticalOperation
    Public Property _key As Long
    Public Property anarchy_impact As Integer
    Public Property description As TranslatedNameField
    Public Property development_impact As Integer
    Public Property infomorph_bonus As Integer
    Public Property name As TranslatedNameField
End Class

