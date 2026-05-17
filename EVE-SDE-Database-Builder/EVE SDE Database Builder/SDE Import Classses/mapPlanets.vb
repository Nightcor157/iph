
Public Class mapPlanets
    Inherits SDEFilesBase
    Implements IImporter(Of planet)
    Implements IDatabaseImporter(Of planet)

    Public Const BaseFileName As String = "mapPlanets"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, planet) Implements IImporter(Of planet).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of planet)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, planet), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of planet).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("planetID", FieldType.int_type, 0, False, True),
            New DBTableField("planetName", FieldType.nvarchar_type, 250, True), ' Set later
            New DBTableField("heightMap1", FieldType.int_type, 0, True),
            New DBTableField("heightMap2", FieldType.int_type, 0, True),
            New DBTableField("shaderPreset", FieldType.int_type, 0, True),
            New DBTableField("population", FieldType.int_type, 0, True),
            New DBTableField("celestialIndex", FieldType.double_type, 0, True),
            New DBTableField("orbitID", FieldType.int_type, 0, True),
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
                "planetID"
            }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_ID", IndexFields)

        ' NPC stations linked to planet
        Table = New List(Of DBTableField) From {
            New DBTableField("planetID", FieldType.int_type, 0, False, False),
            New DBTableField("npcStationID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "NPCStationIDs", Table)

        IndexFields = New List(Of String) From {
                "planetID"
            }
        Call UpdateDB.CreateIndex(TableName & "NPCStationIDs", "IDX_" & TableName & "NPCStationIDs" & "_ID", IndexFields)

        ' Asteroid Belts linked to planet
        Table = New List(Of DBTableField) From {
            New DBTableField("planetID", FieldType.int_type, 0, False, False),
            New DBTableField("asteroidBeltID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "AsteroidBeltIDs", Table)

        IndexFields = New List(Of String) From {
                "planetID"
            }
        Call UpdateDB.CreateIndex(TableName & "AsteroidBeltIDs", "IDX_" & TableName & "AsteroidBeltIDs" & "_ID", IndexFields)

        ' Moons linked to planet
        Table = New List(Of DBTableField) From {
            New DBTableField("planetID", FieldType.int_type, 0, False, False),
            New DBTableField("moonID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "MoonIDs", Table)

        IndexFields = New List(Of String) From {
                "planetID"
            }
        Call UpdateDB.CreateIndex(TableName & "MoonIDs", "IDX_" & TableName & "MoonIDs" & "_ID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("planetID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("heightMap1", .attributes.heightMap1, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("heightMap2", .attributes.heightMap2, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shaderPreset", .attributes.shaderPreset, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("population", BooleanField(.attributes.population), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("celestialIndex", .celestialIndex, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("orbitID", .orbitID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", GetTranslation(.uniqueName, Params.ImportLanguageCode), FieldType.nvarchar_type))

                If Not IsNothing(.statistics) Then
                    With .statistics
                        DataFields.Add(UpdateDB.BuildDatabaseField("density", .density, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("eccentricity", .eccentricity, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("escapeVelocity", .escapeVelocity, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("locked", BooleanField(.locked), FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("massDust", .massDust, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("massGas", .massGas, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("orbitPeriod", .orbitPeriod, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("orbitRadius", .orbitRadius, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("rotationRate", .rotationRate, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("spectralClass", .spectralClass, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("surfaceGravity", NullableField(.surfaceGravity), FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("temperature", .temperature, FieldType.double_type))
                    End With
                End If

                If Not IsNothing(.asteroidBeltIDs) Then
                    For Each station In .asteroidBeltIDs
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("planetID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("asteroidBeltID", station, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "AsteroidBeltIDs", UpdateDB.BuildOrderedRecord(TableName & "AsteroidBeltIDs", DataFields2))
                    Next
                End If

                If Not IsNothing(.moonIDs) Then
                    For Each station In .moonIDs
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("planetID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("moonID", station, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "MoonIDs", UpdateDB.BuildOrderedRecord(TableName & "MoonIDs", DataFields2))
                    Next
                End If

                If Not IsNothing(.npcStationIDs) Then
                    For Each station In .npcStationIDs
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("planetID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("npcStationID", station, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "NPCStationIDs", UpdateDB.BuildOrderedRecord(TableName & "NPCStationIDs", DataFields2))
                    Next
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

Public Class planet
    Public Property _key As Long
    Public Property asteroidBeltIDs As List(Of Integer)
    Public Property attributes As planetAttribute
    Public Property celestialIndex As Integer
    Public Property moonIDs As List(Of Integer)
    Public Property npcStationIDs As List(Of Integer)
    Public Property orbitID As Integer
    Public Property position As celestialposition
    Public Property radius As Integer
    Public Property solarSystemID As Integer
    Public Property statistics As planetStatistic
    Public Property typeID As Integer
    Public Property uniqueName As TranslatedNameField
End Class

Public Class planetAttribute
    Public Property heightMap1 As Integer
    Public Property heightMap2 As Integer
    Public Property population As Boolean
    Public Property shaderPreset As Integer
End Class

Public Class planetStatistic
    Public Property density As Double
    Public Property eccentricity As Double
    Public Property escapeVelocity As Double
    Public Property locked As Boolean
    Public Property massDust As Double
    Public Property massGas As Double
    Public Property orbitPeriod As Double
    Public Property orbitRadius As Double
    Public Property pressure As Double
    Public Property rotationRate As Double
    Public Property spectralClass As String
    Public Property surfaceGravity As Object
    Public Property temperature As Double

End Class