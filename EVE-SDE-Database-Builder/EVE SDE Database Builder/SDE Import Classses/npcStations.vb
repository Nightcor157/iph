
Public Class npcStations
    Inherits SDEFilesBase
    Implements IImporter(Of npcStation)
    Implements IDatabaseImporter(Of npcStation)

    Public Const BaseFileName As String = "npcStations"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, npcStation) Implements IImporter(Of npcStation).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of npcStation)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, npcStation), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of npcStation).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("stationID", FieldType.int_type, 0, False, True),
            New DBTableField("stationName", FieldType.nvarchar_type, 250, True), ' set later
            New DBTableField("celestialIndex", FieldType.int_type, 0, True),
            New DBTableField("operationID", FieldType.int_type, 0, False),
            New DBTableField("orbitID", FieldType.int_type, 0, False),
            New DBTableField("orbitIndex", FieldType.int_type, 0, True),
            New DBTableField("ownerID", FieldType.int_type, 0, False),
            New DBTableField("x", FieldType.double_type, 0, False),
            New DBTableField("y", FieldType.double_type, 0, False),
            New DBTableField("z", FieldType.double_type, 0, False),
            New DBTableField("reprocessingEfficiency", FieldType.double_type, 0, False),
            New DBTableField("reprocessingHangarFlag", FieldType.int_type, 0, False),
            New DBTableField("reprocessingStationsTake", FieldType.double_type, 0, False),
            New DBTableField("solarSystemID", FieldType.int_type, 0, False),
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("useOperationName", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "solarSystemID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SSID", IndexFields)

        IndexFields = New List(Of String) From {
            "operationID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_OID", IndexFields)

        IndexFields = New List(Of String) From {
            "stationID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_STID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("stationID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("celestialIndex", .celestialIndex, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("operationID", .operationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("orbitID", .orbitID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("orbitIndex", .orbitIndex, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ownerID", .ownerID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingEfficiency", .reprocessingEfficiency, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingHangarFlag", .reprocessingHangarFlag, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("reprocessingStationsTake", .reprocessingStationsTake, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("useOperationName", BooleanField(.useOperationName), FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class npcStation
    Public Property _key As Long
    Public Property celestialIndex As Integer
    Public Property operationID As Integer
    Public Property orbitID As Integer
    Public Property orbitIndex As Integer
    Public Property ownerID As Integer
    Public Property position As celestialposition
    Public Property reprocessingEfficiency As Double
    Public Property reprocessingHangarFlag As Integer
    Public Property reprocessingStationsTake As Double
    Public Property solarSystemID As Integer
    Public Property typeID As Integer
    Public Property useOperationName As Boolean
End Class


