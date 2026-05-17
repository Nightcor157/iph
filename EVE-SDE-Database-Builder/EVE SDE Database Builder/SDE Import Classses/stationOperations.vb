
Public Class stationOperations
    Inherits SDEFilesBase
    Implements IImporter(Of stationOperation)
    Implements IDatabaseImporter(Of stationOperation)

    Public Const BaseFileName As String = "stationOperations"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, stationOperation) Implements IImporter(Of stationOperation).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of stationOperation)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, stationOperation), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of stationOperation).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("operationID", FieldType.int_type, 0, False, True),
            New DBTableField("operationName", FieldType.nvarchar_type, 1000, True),
            New DBTableField("activityID", FieldType.int_type, 0, True),
            New DBTableField("border", FieldType.double_type, 0, True),
            New DBTableField("corridor", FieldType.double_type, 0, True),
            New DBTableField("description", FieldType.nvarchar_type, 1500, True),
            New DBTableField("fringe", FieldType.double_type, 0, True),
            New DBTableField("hub", FieldType.double_type, 0, True),
            New DBTableField("manufacturingFactor", FieldType.double_type, 0, True),
            New DBTableField("ratio", FieldType.double_type, 0, True),
            New DBTableField("researchFactor", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim StationOperationServicesTableName As String = "stationOperationsServices"
        Table = New List(Of DBTableField) From {
            New DBTableField("operationID", FieldType.int_type, 0, False),
            New DBTableField("serviceID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(StationOperationServicesTableName, Table)

        Dim StationOperationTypesTableName As String = "stationOperationsTypes"
        Table = New List(Of DBTableField) From {
            New DBTableField("operationID", FieldType.int_type, 0, False),
            New DBTableField("raceID", FieldType.int_type, 0, True),
            New DBTableField("stationTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(StationOperationTypesTableName, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        Dim TotalRecords As Long = Records.Count

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        ' Process Data
        For Each DataField In Records
            DataFields = New List(Of DBField)

            If CancelImport Then Exit Sub

            ' Build the insert list
            With DataField.Value
                DataFields.Add(UpdateDB.BuildDatabaseField("operationID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("operationName", GetTranslation(.operationName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("activityID", .activityID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("border", .border, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corridor", .corridor, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fringe", .fringe, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("hub", .hub, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("manufacturingFactor", .manufacturingFactor, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("ratio", .ratio, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("researchFactor", .researchFactor, FieldType.double_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            If Not IsNothing(DataField.Value.services) Then
                For Each Service In DataField.Value.services
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("operationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("serviceID", Service, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(StationOperationServicesTableName, UpdateDB.BuildOrderedRecord(StationOperationServicesTableName, DataFields))
                Next
            End If

            If Not IsNothing(DataField.Value.stationTypes) Then
                For Each StationType In DataField.Value.stationTypes
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("operationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("raceID", StationType._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("stationTypeID", StationType._Value, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(StationOperationTypesTableName, UpdateDB.BuildOrderedRecord(StationOperationTypesTableName, DataFields))
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class stationOperation
    Public Property _key As Long
    Public Property activityID As Integer
    Public Property border As Double
    Public Property corridor As Double
    Public Property description As TranslatedNameField
    Public Property fringe As Double
    Public Property hub As Double
    Public Property manufacturingFactor As Double
    Public Property operationName As TranslatedNameField
    Public Property ratio As Double
    Public Property researchFactor As Double
    Public Property services As List(Of Integer) ' new table
    Public Property stationTypes As List(Of stationType) ' new table
End Class

Public Class stationType
    Public Property _key As Long
    Public Property _Value As Integer
End Class