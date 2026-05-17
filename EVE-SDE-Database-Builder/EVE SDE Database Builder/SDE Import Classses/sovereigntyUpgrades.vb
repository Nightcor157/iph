
Public Class sovereigntyUpgrades
    Inherits SDEFilesBase
    Implements IImporter(Of sovereigntyUpgrade)
    Implements IDatabaseImporter(Of sovereigntyUpgrade)

    Public Const BaseFileName As String = "sovereigntyUpgrades"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, sovereigntyUpgrade) Implements IImporter(Of sovereigntyUpgrade).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of sovereigntyUpgrade)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, sovereigntyUpgrade), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of sovereigntyUpgrade).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("upgradeTypeID", FieldType.bigint_type, 0, True, True),
            New DBTableField("fuel_hourly_upkeep", FieldType.bigint_type, 0, True),
            New DBTableField("fuel_startup_cost", FieldType.bigint_type, 0, True),
            New DBTableField("fuel_type_id", FieldType.bigint_type, 0, True),
            New DBTableField("mutually_exclusive_group", FieldType.varchar_type, 100, True),
            New DBTableField("power_allocation", FieldType.int_type, 0, True),
            New DBTableField("power_production", FieldType.int_type, 0, True),
            New DBTableField("workforce_allocation", FieldType.int_type, 0, True),
            New DBTableField("workforce_production", FieldType.int_type, 0, True)
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
                DataFields.Add(UpdateDB.BuildDatabaseField("upgradeTypeID", DataField.Key, FieldType.bigint_type))
                If .fuel IsNot Nothing Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("fuel_hourly_upkeep", .fuel.hourly_upkeep, FieldType.bigint_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("fuel_startup_cost", .fuel.startup_cost, FieldType.bigint_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("fuel_type_id", .fuel.type_id, FieldType.bigint_type))
                End If
                DataFields.Add(UpdateDB.BuildDatabaseField("mutually_exclusive_group", .mutually_exclusive_group, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("power_allocation", .power_allocation, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("power_production", .power_production, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("workforce_allocation", .workforce_allocation, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("workforce_production", .workforce_production, FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class sovereigntyUpgrade
    Public Property _key As Long
    Public Property fuel As FuelStats
    Public Property mutually_exclusive_group As String
    Public Property power_allocation As Integer?
    Public Property power_production As Integer?
    Public Property workforce_allocation As Integer?
    Public Property workforce_production As Integer?
End Class

Public Class FuelStats
    Public Property hourly_upkeep As Integer
    Public Property startup_cost As Integer
    Public Property type_id As Integer
End Class