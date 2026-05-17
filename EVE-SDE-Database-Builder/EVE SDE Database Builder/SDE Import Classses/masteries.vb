
Imports System.IO
Imports System.Text.Json

Public Class masteries
    Inherits SDEFilesBase
    Implements IDatabaseImporter(Of Dictionary(Of Integer, List(Of Integer)))

    Public Const BaseFileName As String = "masteries"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, Dictionary(Of Integer, List(Of Integer)))
        Dim options As New JsonSerializerOptions With {
        .PropertyNameCaseInsensitive = True
    }

        Dim results As New Dictionary(Of Long, Dictionary(Of Integer, List(Of Integer)))()

        Try
            Using reader As New StreamReader(SDEFilePath)
                While Not reader.EndOfStream
                    Dim line As String = reader.ReadLine().Trim()
                    If String.IsNullOrWhiteSpace(line) Then Continue While

                    Dim record As mastery = JsonSerializer.Deserialize(Of mastery)(line, options)

                    If record IsNot Nothing Then
                        Dim levels As New Dictionary(Of Integer, List(Of Integer))()

                        For Each entry In record._value
                            levels(entry._key) = entry._value
                        Next

                        results(record._key) = levels
                    End If
                End While
            End Using

            Return results

        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, Dictionary(Of Integer, List(Of Integer))), params As ImportParameters) _
        Implements IDatabaseImporter(Of Dictionary(Of Integer, List(Of Integer))).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("masteryID", FieldType.int_type, 0, False),
            New DBTableField("masteryLevel", FieldType.int_type, 0, False),
            New DBTableField("skillID", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {"masteryID"}
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_MID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(params.RowLocation)
        Dim TotalRecords As Long = Records.Count

        ' Process Data
        For Each DataField In Records
            DataFields = New List(Of DBField)

            If CancelImport Then Exit Sub

            With DataField.Value
                ' Build the insert list
                For Each Level In DataField.Value
                    For Each SkillID In Level.Value
                        DataFields.Add(UpdateDB.BuildDatabaseField("masteryID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("masteryLevel", Level.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("skillID", SkillID, FieldType.int_type))
                        Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))
                        DataFields = New List(Of DBField)
                    Next
                Next
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(params.RowLocation)

    End Sub
End Class

Public Class mastery
    Public Property _key As Integer
    Public Property _value As List(Of masteryEntry)
End Class
Public Class masteryEntry
    Public Property _key As Integer
    Public Property _value As List(Of Integer)
End Class


