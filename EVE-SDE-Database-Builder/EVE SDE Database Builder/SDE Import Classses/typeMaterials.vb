
Public Class typeMaterials
    Inherits SDEFilesBase
    Implements IImporter(Of typeMaterial)
    Implements IDatabaseImporter(Of typeMaterial)

    Public Const BaseFileName As String = "typeMaterials"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, typeMaterial) Implements IImporter(Of typeMaterial).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of typeMaterial)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, typeMaterial), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of typeMaterial).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("materialTypeID", FieldType.int_type, 0, False),
            New DBTableField("quantity", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID", IndexFields)

        Table = New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("materialTypeID", FieldType.int_type, 0, False),
            New DBTableField("quantityMax", FieldType.int_type, 0, False),
            New DBTableField("quantityMin", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable("typeRandomizedMaterials", Table)

        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & "typeRandomizedMaterials" & "_TID", IndexFields)

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

            ' Build the insert list
            Dim typeID As Long = DataField.Key
            If DataField.Value.materials IsNot Nothing Then
                For Each record In DataField.Value.materials
                    DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("typeID", typeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("materialTypeID", record.materialTypeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("quantity", record.quantity, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))
                Next
            End If

            If DataField.Value.randomizedMaterials IsNot Nothing Then
                For Each record In DataField.Value.randomizedMaterials
                    DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("typeID", typeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("materialTypeID", record.materialTypeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("quantityMax", record.quantityMax, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("quantityMin", record.quantityMin, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord("typeRandomizedMaterials", UpdateDB.BuildOrderedRecord("typeRandomizedMaterials", DataFields))
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class typeMaterial
    Public Property _key As Long
    Public Property materials As List(Of material)
    Public Property randomizedMaterials As List(Of randomizedMaterial)
End Class

Public Class material
    Public Property materialTypeID As Integer
    Public Property quantity As Integer
End Class

Public Class randomizedMaterial
    Public Property materialTypeID As Integer
    Public Property quantityMax As Integer
    Public Property quantityMin As Integer
End Class