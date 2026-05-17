
Public Class agentTypes
    Inherits SDEFilesBase
    Implements IImporter(Of agentType)
    Implements IDatabaseImporter(Of agentType)

    Public Const BaseFileName As String = "agentTypes"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, agentType) Implements IImporter(Of agentType).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of agentType)(SDEFilePath, Function(c) c._key)

    End Function

    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, agentType), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of agentType).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("agentTypeID", FieldType.int_type, 0, False, True),
            New DBTableField("agentType", FieldType.varchar_type, 50, False) ' Enum values
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
                DataFields.Add(UpdateDB.BuildDatabaseField("agentTypeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("agentType", .name, FieldType.text_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class agentType
    Public Property _key As Long
    Public Property name As String
End Class