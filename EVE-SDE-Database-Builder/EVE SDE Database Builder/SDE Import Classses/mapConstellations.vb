
Public Class mapConstellations
    Inherits SDEFilesBase
    Implements IImporter(Of constellation)
    Implements IDatabaseImporter(Of constellation)

    Public Const BaseFileName As String = "mapConstellations"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, constellation) Implements IImporter(Of constellation).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of constellation)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, constellation), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of constellation).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("constellationID", FieldType.int_type, 0, False, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("constellationName", FieldType.varchar_type, 100, True),
            New DBTableField("x", FieldType.double_type, 0, True),
            New DBTableField("y", FieldType.double_type, 0, True),
            New DBTableField("z", FieldType.double_type, 0, True),
            New DBTableField("regionID", FieldType.int_type, 0, True),
            New DBTableField("wormholeClassID", FieldType.int_type, 0, True)
            }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
                "constellationID"
            }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        Table = New List(Of DBTableField) From {
            New DBTableField("constellationID", FieldType.int_type, 0, False, False),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "SolarSystems", Table)

        IndexFields = New List(Of String) From {
                "constellationID"
            }
        Call UpdateDB.CreateIndex(TableName & "SolarSystems", "IDX_" & TableName & "SolarSystems" & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("constellationID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", NullableField(.factionID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("constellationName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("regionID", .regionID, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("wormholeClassID", NullableField(.wormholeClassID), FieldType.int_type))
                If Not IsNothing(.solarSystemIDs) Then
                    For Each solarSystem In .solarSystemIDs
                        DataFields2 = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("constellationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("solarSystemID", solarSystem, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "SolarSystems", UpdateDB.BuildOrderedRecord(TableName & "SolarSystems", DataFields2))
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

Public Class constellation
    Public Property _key As Long
    Public Property factionID As Object
    Public Property name As TranslatedNameField
    Public Property position As celestialposition
    Public Property regionID As Integer
    Public Property solarSystemIDs As List(Of Integer)
    Public Property wormholeClassID As Object
End Class

