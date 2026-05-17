
Public Class mapAsteroidBelts
    Inherits SDEFilesBase
    Implements IImporter(Of asteroidBelt)
    Implements IDatabaseImporter(Of asteroidBelt)

    Public Const BaseFileName As String = "mapAsteroidBelts"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, asteroidBelt) Implements IImporter(Of asteroidBelt).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of asteroidBelt)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, asteroidBelt), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of asteroidBelt).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("asteroidBeltID", FieldType.int_type, 0, False, True),
            New DBTableField("asteroidBeltName", FieldType.nvarchar_type, 250, True), ' Set later
            New DBTableField("celestialIndex", FieldType.int_type, 0, True),
            New DBTableField("orbitID", FieldType.int_type, 0, True),
            New DBTableField("orbitIndex", FieldType.int_type, 0, True),
            New DBTableField("x", FieldType.double_type, 0, True),
            New DBTableField("y", FieldType.double_type, 0, True),
            New DBTableField("z", FieldType.double_type, 0, True),
            New DBTableField("radius", FieldType.double_type, 0, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("uniqueName", FieldType.nvarchar_type, 100, True),
            New DBTableField("density", FieldType.double_type, 0, True),
            New DBTableField("eccentricity", FieldType.double_type, 0, True),
            New DBTableField("escapeVelocity", FieldType.double_type, 0, True),
            New DBTableField("locked", FieldType.int_type, 0, True),
            New DBTableField("massDust", FieldType.double_type, 0, True),
            New DBTableField("massGas", FieldType.double_type, 0, True),
            New DBTableField("orbitPeriod", FieldType.double_type, 0, True),
            New DBTableField("orbitRadius", FieldType.double_type, 0, True),
            New DBTableField("rotationRate", FieldType.double_type, 0, True),
            New DBTableField("spectralClass", FieldType.varchar_type, 10, True),
            New DBTableField("surfaceGravity", FieldType.double_type, 0, True),
            New DBTableField("temperature", FieldType.double_type, 0, True)
            }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
                "asteroidBeltID"
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
                DataFields.Add(UpdateDB.BuildDatabaseField("asteroidBeltID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("celestialIndex", .celestialIndex, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("orbitID", .orbitID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("orbitIndex", .orbitIndex, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", NullableField(.radius), FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", GetTranslation(.uniqueName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                If Not IsNothing(.statistics) Then
                    With .statistics
                        DataFields.Add(UpdateDB.BuildDatabaseField("density", .density, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("eccentricity", .eccentricity, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("escapeVelocity", .escapeVelocity, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("locked", CInt(CBool(.locked)), FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("massDust", .massDust, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("massGas", NullableField(.massGas), FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("orbitPeriod", .orbitPeriod, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("orbitRadius", .orbitRadius, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("rotationRate", .rotationRate, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("spectralClass", .spectralClass, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("surfaceGravity", .surfaceGravity, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("temperature", .temperature, FieldType.double_type))
                    End With
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

Public Class asteroidBelt
    Public Property _key As Long
    Public Property celestialIndex As Integer
    Public Property orbitID As Integer
    Public Property orbitIndex As Integer
    Public Property position As asteroidPosition
    Public Property radius As Object
    Public Property solarSystemID As Integer
    Public Property statistics As beltStatistic
    Public Property typeID As Integer
    Public Property uniqueName As TranslatedNameField
End Class

Public Class beltStatistic
    Public Property density As Double
    Public Property eccentricity As Double
    Public Property escapeVelocity As Double
    Public Property locked As Boolean
    Public Property massDust As Double
    Public Property massGas As Object
    Public Property orbitPeriod As Double
    Public Property orbitRadius As Double
    Public Property rotationRate As Double
    Public Property spectralClass As String
    Public Property surfaceGravity As Double
    Public Property temperature As Double

End Class

Public Class asteroidPosition
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double
End Class
