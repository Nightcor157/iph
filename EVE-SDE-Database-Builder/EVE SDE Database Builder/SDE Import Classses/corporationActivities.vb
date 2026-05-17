
Public Class corporationActivities
    Inherits SDEFilesBase
    Implements IImporter(Of corporationActivity)
    Implements IDatabaseImporter(Of corporationActivity)

    Public Const BaseFileName As String = "corporationActivities"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, corporationActivity) Implements IImporter(Of corporationActivity).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of corporationActivity)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, corporationActivity), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of corporationActivity).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        Dim Table As New List(Of DBTableField) From {
            New DBTableField("activityID", FieldType.int_type, 0, False, True),
            New DBTableField("activityName", FieldType.nvarchar_type, 100, True)
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

            ' Build the insert list
            DataFields = New List(Of DBField) From {
                UpdateDB.BuildDatabaseField("activityID", DataField.Key, FieldType.int_type),
                UpdateDB.BuildDatabaseField("activityName", GetTranslation(DataField.Value.name, Params.ImportLanguageCode), FieldType.nvarchar_type)
            }

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class corporationActivity
    Public Property _key As Long
    Public Property name As TranslatedNameField
End Class