
Public Class planetResources
    Inherits SDEFilesBase
    Implements IImporter(Of planetResource)
    Implements IDatabaseImporter(Of planetResource)

    Public Const BaseFileName As String = "planetResources"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, planetResource) Implements IImporter(Of planetResource).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of planetResource)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, planetResource), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of planetResource).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("planetID", FieldType.bigint_type, 0, True, True),
            New DBTableField("power", FieldType.int_type, 0, True),
            New DBTableField("workforce", FieldType.bigint_type, 0, True),
            New DBTableField("amount_per_cycle", FieldType.int_type, 0, True),
            New DBTableField("cycle_period", FieldType.int_type, 0, True),
            New DBTableField("secured_capacity", FieldType.int_type, 0, True),
            New DBTableField("type_id", FieldType.int_type, 0, True),
            New DBTableField("unsecured_capacity", FieldType.double_type, 0, True)
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
                DataFields.Add(UpdateDB.BuildDatabaseField("planetID", DataField.Key, FieldType.bigint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("power", .power, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("workforce", .workforce, FieldType.bigint_type))
                If Not IsNothing(.reagent) Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("amount_per_cycle", .reagent.amount_per_cycle, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("cycle_period", .reagent.cycle_period, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("secured_capacity", .reagent.secured_capacity, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("type_id", .reagent.type_id, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("unsecured_capacity", .reagent.unsecured_capacity, FieldType.int_type))
                End If
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class planetResource
    Public Property _key As Long
    Public Property power As Integer?
    Public Property reagent As planetReagent
    Public Property workforce As Integer?
End Class

Public Class planetReagent
    Public Property amount_per_cycle As Integer
    Public Property cycle_period As Integer
    Public Property secured_capacity As Integer
    Public Property type_id As Integer
    Public Property unsecured_capacity As Integer
End Class