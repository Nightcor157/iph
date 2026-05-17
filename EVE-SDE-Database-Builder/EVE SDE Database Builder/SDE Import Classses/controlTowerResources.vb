
Public Class controlTowerResources
    Inherits SDEFilesBase
    Implements IImporter(Of _controlTowerResources)
    Implements IDatabaseImporter(Of _controlTowerResources)

    Public Const BaseFileName As String = "controlTowerResources"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, _controlTowerResources) Implements IImporter(Of _controlTowerResources).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of _controlTowerResources)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, _controlTowerResources), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of _controlTowerResources).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("minSecurityLevel", FieldType.double_type, 0, True),
            New DBTableField("quantity", FieldType.int_type, 0, True),
            New DBTableField("purpose", FieldType.int_type, 0, True),
            New DBTableField("resourceTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)
        Dim TotalRecords As Long = Records.Count

        ' Process Data
        For Each DataField In Records

            If CancelImport Then Exit Sub

            With DataField.Value
                If .resources.Count > 0 Then
                    For Each resource In DataField.Value.resources
                        ' Build the insert list
                        DataFields = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("factionID", NullableField(resource.factionID), FieldType.int_type),
                            UpdateDB.BuildDatabaseField("minSecurityLevel", NullableField(resource.minSecurityLevel), FieldType.double_type),
                            UpdateDB.BuildDatabaseField("quantity", resource.quantity, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("purpose", resource.purpose, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("resourceTypeID", resource.resourceTypeID, FieldType.int_type)
                        }

                        Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))
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

Public Class _controlTowerResources
    Public Property _key As Long
    Public Property resources As List(Of controlTowerResource)
End Class

Public Class controlTowerResource
    Public Property purpose As Integer
    Public Property factionID As Object
    Public Property minSecurityLevel As Object
    Public Property quantity As Integer
    Public Property resourceTypeID As Integer
End Class