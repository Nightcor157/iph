
Public Class planetSchematics
    Inherits SDEFilesBase
    Implements IImporter(Of planetSchematic)
    Implements IDatabaseImporter(Of planetSchematic)

    Public Const BaseFileName As String = "planetSchematics"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, planetSchematic) Implements IImporter(Of planetSchematic).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of planetSchematic)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, planetSchematic), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of planetSchematic).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)

        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("schematicID", FieldType.smallint_type, 0, False, True),
            New DBTableField("name", FieldType.nvarchar_type, 255, True),
            New DBTableField("cycleTime", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Build table - planetSchematicsPinMap
        Table = New List(Of DBTableField) From {
            New DBTableField("schematicID", FieldType.smallint_type, 0, True),
            New DBTableField("pinTypeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "PinMap", Table)

        ' Build table - planetSchematicsTypeMap
        Table = New List(Of DBTableField) From {
            New DBTableField("schematicID", FieldType.smallint_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("quantity", FieldType.smallint_type, 0, True),
            New DBTableField("isInput", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "TypeMap", Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)
        Dim TotalRecords As Long = Records.Count

        ' Process Data
        For Each DataField In Records
            ' Build the insert list
            With DataField.Value
                DataFields = New List(Of DBField)

                If CancelImport Then Exit Sub

                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("schematicID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("cycleTime", .cycleTime, FieldType.int_type))

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' insert into pin map table
                For i = 0 To .pins.Count - 1
                    DataFields2 = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("schematicID", DataField.Key, FieldType.smallint_type),
                        UpdateDB.BuildDatabaseField("pinTypeID", .pins(i), FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(TableName & "PinMap", UpdateDB.BuildOrderedRecord(TableName & "PinMap", DataFields2))
                Next

                ' Insert all data into type map table
                For Each pinType In .types
                    DataFields2 = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("schematicID", DataField.Key, FieldType.smallint_type),
                        UpdateDB.BuildDatabaseField("typeID", pinType._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("quantity", pinType.quantity, FieldType.smallint_type),
                        UpdateDB.BuildDatabaseField("isInput", BooleanField(pinType.isInput), FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(TableName & "TypeMap", UpdateDB.BuildOrderedRecord(TableName & "TypeMap", DataFields2))
                Next
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1 ' Count after so we never get to 100 until finished

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class planetSchematic
    Public Property _key As Long
    Public Property cycleTime As Integer
    Public Property name As TranslatedNameField
    Public Property pins As List(Of Integer)
    Public Property types As List(Of planetSchematicTypeMapValues)
End Class

Public Class planetSchematicTypeMapValues
    Public Property _key As Long
    Public Property isInput As Boolean
    Public Property quantity As Integer
End Class