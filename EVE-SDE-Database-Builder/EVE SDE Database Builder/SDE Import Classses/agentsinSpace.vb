
Public Class agentsinSpace
    Inherits SDEFilesBase
    Implements IImporter(Of agentInSpace)
    Implements IDatabaseImporter(Of agentInSpace)

    Public Const BaseFileName As String = "agentsInSpace"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the JSON file into an object and returns the records for processing
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, agentInSpace) Implements IImporter(Of agentInSpace).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of agentInSpace)(SDEFilePath, Function(c) c._key)

    End Function

    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, agentInSpace), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of agentInSpace).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim IndexFields As List(Of String)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("agentID", FieldType.int_type, 0, False, True),
            New DBTableField("dungeonID", FieldType.int_type, 0, False),
            New DBTableField("solarSystemID", FieldType.int_type, 0, False),
            New DBTableField("spawnPointID", FieldType.int_type, 0, False),
            New DBTableField("typeID", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        IndexFields = New List(Of String) From {
            "agentID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_AID", IndexFields)

        IndexFields = New List(Of String) From {
            "solarSystemID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SSID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("agentID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("dungeonID", .dungeonID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("spawnPointID", .spawnPointID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class agentInSpace
    Public Property _key As Long
    Public Property dungeonID As Long
    Public Property solarSystemID As Long
    Public Property spawnPointID As Integer
    Public Property typeID As Integer
End Class