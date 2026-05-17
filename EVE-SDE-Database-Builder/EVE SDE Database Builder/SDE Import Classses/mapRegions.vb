
Public Class mapRegions
    Inherits SDEFilesBase
    Implements IImporter(Of region)
    Implements IDatabaseImporter(Of region)

    Public Const BaseFileName As String = "mapRegions"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, region) Implements IImporter(Of region).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of region)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, region), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of region).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("regionID", FieldType.int_type, 0, False, True),
            New DBTableField("regionName", FieldType.varchar_type, 255, True),
            New DBTableField("description", FieldType.varchar_type, 3000, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("nebulaID", FieldType.int_type, 0, True),
            New DBTableField("x", FieldType.double_type, 0, True),
            New DBTableField("y", FieldType.double_type, 0, True),
            New DBTableField("z", FieldType.double_type, 0, True),
            New DBTableField("wormholeClassID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
                "regionID"
            }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_ID", IndexFields)

        ' Constellations that are in region
        Table = New List(Of DBTableField) From {
            New DBTableField("regionID", FieldType.int_type, 0, False, False),
            New DBTableField("constellationID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable("mapRegionsConstellations", Table)

        IndexFields = New List(Of String) From {
                "regionID"
            }
        Call UpdateDB.CreateIndex("mapRegionsConstellations", "IDX_" & "mapRegionsConstellations" & "_ID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("regionID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("regionName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", NullableField(.factionID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("nebulaID", .nebulaID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("wormholeClassID", NullableField(.wormholeClassID), FieldType.int_type))

                If Not IsNothing(.constellationIDs) Then
                    For Each constellationID In .constellationIDs
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("regionID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("constellationID", constellationID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord("mapRegionsConstellations", UpdateDB.BuildOrderedRecord("mapRegionsConstellations", DataFields2))
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

Public Class region
    Public Property _key As Long
    Public Property constellationIDs As List(Of Integer)
    Public Property description As TranslatedNameField
    Public Property factionID As Object
    Public Property name As TranslatedNameField
    Public Property nebulaID As Integer
    Public Property position As celestialposition
    Public Property wormholeClassID As Object
End Class