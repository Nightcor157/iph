
Public Class graphics
    Inherits SDEFilesBase
    Implements IImporter(Of graphic)
    Implements IDatabaseImporter(Of graphic)

    Public Const BaseFileName As String = "graphics"
    Private Const graphicssofLayouts As String = "graphicsofLayouts"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, graphic) Implements IImporter(Of graphic).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of graphic)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, graphic), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of graphic).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("graphicID", FieldType.int_type, 0, False, True),
            New DBTableField("graphicFile", FieldType.varchar_type, 150, True),
            New DBTableField("iconFolder", FieldType.varchar_type, 150, True),
            New DBTableField("sofFactionName", FieldType.varchar_type, 100, True),
            New DBTableField("sofHullName", FieldType.varchar_type, 100, True),
            New DBTableField("sofMaterialSetID", FieldType.int_type, 0, True),
            New DBTableField("sofRaceName", FieldType.varchar_type, 100, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Add minor table for sofLayout change in uprising
        Table = New List(Of DBTableField) From {
            New DBTableField("graphicID", FieldType.int_type, 0, False, False),
            New DBTableField("sofLayout", FieldType.varchar_type, 100, True)
        }

        Call UpdateDB.CreateTable(graphicssofLayouts, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "graphicID"
        }
        Call UpdateDB.CreateIndex(graphicssofLayouts, "IDX_" & graphicssofLayouts & "_GID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicFile", NullableField(.graphicFile), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconFolder", NullableField(.iconFolder), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofFactionName", NullableField(.sofFactionName), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofHullName", NullableField(.sofHullName), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofMaterialSetID", NullableField(.sofMaterialSetID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sofRaceName", NullableField(.sofRaceName), FieldType.nvarchar_type))

                If Not IsNothing(.sofLayout) Then
                    For Each sofLayout In .sofLayout
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("graphicID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("sofLayout", NullableField(sofLayout), FieldType.varchar_type)
                        }
                        Call UpdateDB.InsertRecord(graphicssofLayouts, UpdateDB.BuildOrderedRecord(graphicssofLayouts, DataFields2))
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

Public Class graphic
    Public Property _key As Long
    Public Property graphicFile As Object
    Public Property iconFolder As Object
    Public Property sofFactionName As Object
    Public Property sofHullName As Object
    Public Property sofLayout As List(Of String)
    Public Property sofMaterialSetID As Object
    Public Property sofRaceName As Object
End Class
