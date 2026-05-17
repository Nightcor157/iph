
Public Class mapStargates
    Inherits SDEFilesBase
    Implements IImporter(Of stargate)
    Implements IDatabaseImporter(Of stargate)

    Public Const BaseFileName As String = "mapStargates"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, stargate) Implements IImporter(Of stargate).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of stargate)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, stargate), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of stargate).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("stargateID", FieldType.int_type, 0, False, True),
            New DBTableField("stargateName", FieldType.nvarchar_type, 250, True), ' Set later
            New DBTableField("destinationSolarSystemID", FieldType.int_type, 0, True),
            New DBTableField("destinationStargateID", FieldType.int_type, 0, True),
            New DBTableField("x", FieldType.double_type, 0, True),
            New DBTableField("y", FieldType.double_type, 0, True),
            New DBTableField("z", FieldType.double_type, 0, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True)
            }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
                "stargateID"
            }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("stargateID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("destinationSolarSystemID", .destination.solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("destinationStargateID", .destination.stargateID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class stargate
    Public Property _key As Long
    Public Property destination As gatedestination
    Public Property stargateID As Integer
    Public Property position As celestialposition
    Public Property solarSystemID As Integer
    Public Property typeID As Integer
End Class

Public Class gatedestination
    Public Property stargateID As Integer
    Public Property solarSystemID As Integer
End Class


