
Public Class skins
    Inherits SDEFilesBase
    Implements IImporter(Of skin)
    Implements IDatabaseImporter(Of skin)

    Public Const BaseFileName As String = "skins"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, skin) Implements IImporter(Of skin).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of skin)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, skin), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of skin).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("skinID", FieldType.int_type, 0, True),
            New DBTableField("skinDescription", FieldType.text_type, MaxFieldLen, True),
            New DBTableField("internalName", FieldType.varchar_type, 100, True),
            New DBTableField("skinMaterialID", FieldType.int_type, 0, True),
            New DBTableField("isStructureSkin", FieldType.int_type, 0, True),
            New DBTableField("allowCCPDevs", FieldType.int_type, 0, True),
            New DBTableField("visibleSerenity", FieldType.int_type, 0, True),
            New DBTableField("visibleTranquility", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "skinID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SID", IndexFields)

        Table = New List(Of DBTableField) From {
            New DBTableField("skinID", FieldType.int_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName & "Types", Table)

        ' Create indexes
        IndexFields = New List(Of String)
        IndexFields = New List(Of String) From {
            "skinID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "Types" & "_SID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)
        Dim TotalRecords As Long = Records.Count

        ' Process Data
        For Each DataField In Records
            For Each DF In DataField.Value.types
                DataFields = New List(Of DBField)

                If CancelImport Then Exit Sub

                With DataField.Value
                    ' Build the insert list
                    DataFields.Add(UpdateDB.BuildDatabaseField("skinID", DataField.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skinDescription", GetTranslation(.skinDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("internalName", .internalName, FieldType.varchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("skinMaterialID", .skinMaterialID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("isStructureSkin", BooleanField(.isStructureSkin), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("allowCCPDevs", BooleanField(.allowCCPDevs), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("visibleSerenity", BooleanField(.visibleSerenity), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("visibleTranquility", BooleanField(.visibleTranquility), FieldType.int_type))

                    If Not IsNothing(.types) Then
                        For Each typeID In .types
                            DataFields2 = New List(Of DBField) From {
                                UpdateDB.BuildDatabaseField("skinID", DataField.Key, FieldType.int_type),
                                UpdateDB.BuildDatabaseField("typeID", typeID, FieldType.int_type)
                            }
                            Call UpdateDB.InsertRecord(TableName & "Types", UpdateDB.BuildOrderedRecord(TableName & "Types", DataFields2))
                        Next
                    End If
                End With

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))
            Next

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class skin
    Public Property _key As Long
    Public Property allowCCPDevs As Boolean
    Public Property internalName As String
    Public Property isStructureSkin As Boolean
    Public Property skinDescription As TranslatedNameField
    Public Property skinMaterialID As Integer
    Public Property types As List(Of Integer)
    Public Property visibleSerenity As Boolean
    Public Property visibleTranquility As Boolean
End Class