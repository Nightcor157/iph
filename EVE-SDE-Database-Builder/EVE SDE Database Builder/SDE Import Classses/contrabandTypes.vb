Public Class contrabandTypes
    Inherits SDEFilesBase
    Implements IImporter(Of contrabandType)
    Implements IDatabaseImporter(Of contrabandType)

    Public Const BaseFileName As String = "contrabandTypes"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, contrabandType) Implements IImporter(Of contrabandType).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of contrabandType)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, contrabandType), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of contrabandType).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("attackMinSec", FieldType.double_type, 0, True),
            New DBTableField("confiscateMinSec", FieldType.double_type, 0, True),
            New DBTableField("fineByValue", FieldType.double_type, 0, True),
            New DBTableField("standingLoss", FieldType.double_type, 0, True)
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
                If .factions.Count > 0 Then
                    For Each resource In DataField.Value.factions
                        ' Build the insert list
                        DataFields = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("factionID", resource._key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("attackMinSec", resource.attackMinSec, FieldType.double_type),
                            UpdateDB.BuildDatabaseField("confiscateMinSec", resource.confiscateMinSec, FieldType.double_type),
                            UpdateDB.BuildDatabaseField("fineByValue", resource.fineByValue, FieldType.double_type),
                            UpdateDB.BuildDatabaseField("standingLoss", resource.standingLoss, FieldType.double_type)
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

Public Class contrabandType
    Public Property _key As Long
    Public Property factions As New List(Of contrabandEffectData)
End Class

Public Class contrabandEffectData
    Public Property _key As Long
    Public Property attackMinSec As Double
    Public Property confiscateMinSec As Double
    Public Property fineByValue As Double
    Public Property standingLoss As Double
End Class