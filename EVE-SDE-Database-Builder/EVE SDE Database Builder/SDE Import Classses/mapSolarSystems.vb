
Public Class mapSolarSystems
    Inherits SDEFilesBase
    Implements IImporter(Of solarSystem)
    Implements IDatabaseImporter(Of solarSystem)

    Public Const BaseFileName As String = "mapSolarSystems"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, solarSystem) Implements IImporter(Of solarSystem).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of solarSystem)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, solarSystem), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of solarSystem).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("solarSystemID", FieldType.int_type, 0, False, True),
            New DBTableField("solarSystemName", FieldType.varchar_type, 255, False),
            New DBTableField("constellationID", FieldType.int_type, 0, False),
            New DBTableField("border", FieldType.int_type, 0, True),
            New DBTableField("corridor", FieldType.int_type, 0, True),
            New DBTableField("fringe", FieldType.int_type, 0, True),
            New DBTableField("hub", FieldType.int_type, 0, True),
            New DBTableField("international", FieldType.int_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("luminosity", FieldType.double_type, 0, True),
            New DBTableField("x", FieldType.double_type, 0, False),
            New DBTableField("y", FieldType.double_type, 0, False),
            New DBTableField("z", FieldType.double_type, 0, False),
            New DBTableField("x2D", FieldType.double_type, 0, True),
            New DBTableField("y2D", FieldType.double_type, 0, True),
            New DBTableField("radius", FieldType.double_type, 0, False),
            New DBTableField("regionID", FieldType.int_type, 0, False),
            New DBTableField("regional", FieldType.int_type, 0, True),
            New DBTableField("securityClass", FieldType.varchar_type, 255, True),
            New DBTableField("securityStatus", FieldType.double_type, 0, False),
            New DBTableField("starID", FieldType.int_type, 0, True),
            New DBTableField("visualEffect", FieldType.varchar_type, 255, True),
            New DBTableField("wormholeClassID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
                "solarSystemID"
            }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_ID", IndexFields)

        ' disallowedAnchorCategories table
        Table = New List(Of DBTableField) From {
            New DBTableField("solarSystemID", FieldType.int_type, 0, False, False),
            New DBTableField("disallowedAnchorCategory", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "disallowedAnchorCategories", Table)

        IndexFields = New List(Of String) From {
                "solarSystemID"
            }
        Call UpdateDB.CreateIndex(TableName & "disallowedAnchorCategories", "IDX_" & TableName & "disallowedAnchorCategories" & "_CID", IndexFields)

        ' disallowedAnchorGroups table
        Table = New List(Of DBTableField) From {
            New DBTableField("solarSystemID", FieldType.int_type, 0, False, False),
            New DBTableField("disallowedAnchorGroup", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "disallowedAnchorGroups", Table)

        IndexFields = New List(Of String) From {
                "solarSystemID"
            }
        Call UpdateDB.CreateIndex(TableName & "disallowedAnchorGroups", "IDX_" & TableName & "disallowedAnchorGroups" & "_CID", IndexFields)

        ' planetIDs table
        Table = New List(Of DBTableField) From {
            New DBTableField("solarSystemID", FieldType.int_type, 0, False, False),
            New DBTableField("planetID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "planetIDs", Table)

        IndexFields = New List(Of String) From {
                "solarSystemID"
            }
        Call UpdateDB.CreateIndex(TableName & "planetIDs", "IDX_" & TableName & "planetIDs" & "_CID", IndexFields)

        ' stargateIDs table
        Table = New List(Of DBTableField) From {
            New DBTableField("solarSystemID", FieldType.int_type, 0, False, False),
            New DBTableField("stargateID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "stargateIDs", Table)

        IndexFields = New List(Of String) From {
                "solarSystemID"
            }
        Call UpdateDB.CreateIndex(TableName & "stargateIDs", "IDX_" & TableName & "stargateIDs" & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("border", NullableField(.border), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("constellationID", .constellationID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corridor", NullableField(.corridor), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", NullableField(.factionID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fringe", NullableField(.fringe), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("hub", NullableField(.hub), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("international", NullableField(.international), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("luminosity", NullableField(.luminosity), FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("x", .position.x, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("y", .position.y, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("z", .position.z, FieldType.double_type))
                If Not IsNothing(.position2D) Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("x2D", .position2D.x, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("y2D", .position2D.y, FieldType.double_type))
                End If
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("regionID", .regionID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("regional", NullableField(.regional), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("securityClass", NullableField(.securityClass), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("securityStatus", .securityStatus, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("starID", NullableField(.starID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("visualEffect", NullableField(.visualEffect), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("wormholeClassID", NullableField(.wormholeClassID), FieldType.int_type))

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' disallowedAnchorCategories
                If .disallowedAnchorCategories IsNot Nothing Then
                    For Each disallowedAnchorCategory In DataField.Value.disallowedAnchorCategories
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("solarSystemID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("disallowedAnchorCategory", disallowedAnchorCategory, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "disallowedAnchorCategories", UpdateDB.BuildOrderedRecord(TableName & "disallowedAnchorCategories", DataFields2))
                    Next
                End If

                ' disallowedAnchorGroups
                If .disallowedAnchorGroups IsNot Nothing Then
                    For Each disallowedAnchorGroup In DataField.Value.disallowedAnchorGroups
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("solarSystemID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("disallowedAnchorGroup", disallowedAnchorGroup, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "disallowedAnchorGroups", UpdateDB.BuildOrderedRecord(TableName & "disallowedAnchorGroups", DataFields2))
                    Next
                End If

                ' planetIDs 
                If .planetIDs IsNot Nothing Then
                    For Each planetID In DataField.Value.planetIDs
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("solarSystemID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("planetID", planetID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "planetIDs", UpdateDB.BuildOrderedRecord(TableName & "planetIDs", DataFields2))
                    Next
                End If

                ' stargateIDs
                If .stargateIDs IsNot Nothing Then
                    For Each stargateID In DataField.Value.stargateIDs
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("solarSystemID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("stargateID", stargateID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(TableName & "stargateIDs", UpdateDB.BuildOrderedRecord(TableName & "stargateIDs", DataFields2))
                    Next
                End If

            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class solarSystem
    Public Property _key As Long
    Public Property border As Object
    Public Property constellationID As Integer
    Public Property corridor As Object
    Public Property disallowedAnchorCategories As List(Of Integer)
    Public Property disallowedAnchorGroups As List(Of Integer)
    Public Property factionID As Object
    Public Property fringe As Object
    Public Property hub As Object
    Public Property international As Object
    Public Property luminosity As Object
    Public Property name As TranslatedNameField
    Public Property planetIDs As List(Of Integer)
    Public Property position As celestialposition
    Public Property position2D As celestialposition2D
    Public Property radius As Double
    Public Property regionID As Integer
    Public Property regional As Object
    Public Property securityClass As String
    Public Property securityStatus As Double
    Public Property starID As Object
    Public Property stargateIDs As List(Of Integer)
    Public Property visualEffect As Object
    Public Property wormholeClassID As Object
End Class

