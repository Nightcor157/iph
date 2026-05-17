
Public Class dynamicItemAttributes
    Inherits SDEFilesBase
    Implements IImporter(Of dynamicItemAttribute)
    Implements IDatabaseImporter(Of dynamicItemAttribute)

    Public Const BaseFileName As String = "dynamicItemAttributes"
    Private Const dynamicItemAttributeIDsTable As String = "dynamicItemAttributeIDs"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dynamicItemAttribute) Implements IImporter(Of dynamicItemAttribute).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dynamicItemAttribute)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dynamicItemAttribute), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dynamicItemAttribute).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("dynamicItemAttributeID", FieldType.int_type, 0, False),
            New DBTableField("applicableType", FieldType.int_type, 0, False),
            New DBTableField("resultingType", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "dynamicItemAttributeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        ' Build the attributeIDs table too
        Table = New List(Of DBTableField) From {
            New DBTableField("dynamicItemAttributeID", FieldType.int_type, 0, False),
            New DBTableField("attributeID", FieldType.int_type, 0, False),
            New DBTableField("highIsGood", FieldType.int_type, 0, True),
            New DBTableField("max", FieldType.double_type, 0, False),
            New DBTableField("min", FieldType.double_type, 0, False)
        }

        Call UpdateDB.CreateTable(dynamicItemAttributeIDsTable, Table)

        IndexFields = New List(Of String)
        IndexFields = New List(Of String) From {
            "dynamicItemAttributeID"
        }
        Call UpdateDB.CreateIndex(dynamicItemAttributeIDsTable, "IDX_" & dynamicItemAttributeIDsTable & "_CID", IndexFields)

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
                For Each mapping In .inputOutputMapping
                    For Each applicableType In mapping.applicableTypes
                        DataFields.Add(UpdateDB.BuildDatabaseField("dynamicItemAttributeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("applicableType", applicableType, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("resultingType", mapping.resultingType, FieldType.int_type))

                        ' Update here each record 
                        Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))
                        DataFields = New List(Of DBField)
                    Next
                Next

                ' Insert other data in separate table
                For Each applicableType In .attributeIDs
                    DataFields2 = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("dynamicItemAttributeID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("attributeID", ._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("highIsGood", BooleanField(applicableType.highIsGood), FieldType.int_type),
                        UpdateDB.BuildDatabaseField("max", NullableField(applicableType.max), FieldType.double_type),
                        UpdateDB.BuildDatabaseField("min", NullableField(applicableType.min), FieldType.double_type)
                    }

                    Call UpdateDB.InsertRecord(dynamicItemAttributeIDsTable, UpdateDB.BuildOrderedRecord(dynamicItemAttributeIDsTable, DataFields2))
                Next
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class dynamicItemAttribute
    Public Property _key As Long
    Public Property attributeIDs As List(Of dynamicAttributeID)
    Public Property inputOutputMapping As List(Of inputOutputMapping)
End Class

Public Class dynamicAttributeID
    Public Property _key As Long
    Public Property highIsGood As Boolean
    Public Property max As Double
    Public Property min As Double
End Class

Public Class inputOutputMapping
    Public Property applicableTypes As List(Of Integer)
    Public Property resultingType As Integer
End Class