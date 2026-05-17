Imports System.Collections.Concurrent

Public Class LocalDatabase

    Private ReadOnly DataTablesList As New ConcurrentQueue(Of DataTable)
    Private ReadOnly Schemas As New Dictionary(Of String, List(Of DBTableField))(StringComparer.OrdinalIgnoreCase)

    ' Called from CreateTable
    Public Sub RegisterSchema(TableName As String, TableStructure As List(Of DBTableField))
        Schemas(TableName) = TableStructure
    End Sub

    Public Function GetTables() As List(Of DataTable)
        Return DataTablesList.ToList()
    End Function

    Public Sub InsertRecord(TableNameRef As String, SentRecord As List(Of DBField))

        Dim DT As DataTable = GetDataTable(TableNameRef)

        ' Ensure schema exists
        If Not Schemas.ContainsKey(TableNameRef) Then
            Throw New Exception("Schema not registered for table: " & TableNameRef)
        End If

        Dim tableSchema = Schemas(TableNameRef)

        ' Build DataTable schema ONCE
        If DT.Columns.Count = 0 Then
            For Each f In tableSchema
                Dim FT As Type

                Select Case f.FieldType
                    Case FieldType.char_type, FieldType.nchar_type, FieldType.ntext_type, FieldType.nvarchar_type, FieldType.text_type, FieldType.varchar_type
                        FT = GetType(String)
                    Case FieldType.smallint_type, FieldType.tinyint_type
                        FT = GetType(Int16)     ' PostgreSQL smallint = Int16
                    Case FieldType.int_type
                        FT = GetType(Int32)     ' PostgreSQL integer = Int32
                    Case FieldType.bigint_type
                        FT = GetType(Int64)     ' PostgreSQL bigint = Int64
                    Case FieldType.real_type
                        FT = GetType(Single)    ' PostgreSQL real = 4‑byte float
                    Case FieldType.double_type
                        FT = GetType(Double)    ' PostgreSQL double precision = Double 8-byte float
                    Case FieldType.bit_type
                        FT = GetType(Boolean)
                    Case Else
                        FT = GetType(Object)
                End Select

                DT.Columns.Add(New DataColumn(f.FieldName, FT) With {.AllowDBNull = True})
            Next

            DT.TableName = TableNameRef
            DataTablesList.Enqueue(DT)
        End If

        ' Build column map
        Dim Data(DT.Columns.Count - 1) As Object
        Dim columnMap As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For c = 0 To DT.Columns.Count - 1
            columnMap(DT.Columns(c).ColumnName) = c
        Next

        ' Fill row values
        For Each field In SentRecord
            If Not columnMap.ContainsKey(field.FieldName) Then Continue For

            Dim colIndex = columnMap(field.FieldName)
            Dim TempValue As String = field.FieldValue

            If TempValue <> "" AndAlso TempValue.StartsWith("'") AndAlso TempValue.EndsWith("'") Then
                TempValue = TempValue.Substring(1, TempValue.Length - 2).Replace("''", "'")
            End If

            If TempValue = "" OrElse TempValue.ToUpper() = "NULL" Then
                Data(colIndex) = DBNull.Value

            ElseIf field.FieldType = FieldType.bit_type Then
                Data(colIndex) = CBool(TempValue)

            ElseIf DT.Columns(colIndex).DataType Is GetType(String) Then
                Data(colIndex) = CleanString(TempValue)

            Else
                Data(colIndex) = TempValue
            End If
        Next

        ' Add row
        Dim AddRow As DataRow = DT.NewRow()
        AddRow.ItemArray = Data

        SyncLock Lock
            DT.Rows.Add(AddRow)
        End SyncLock

    End Sub

    Public Function GetDataTable(TableNameRef As String) As DataTable
        For Each t In DataTablesList
            If t.TableName = TableNameRef Then Return t
        Next
        Return New DataTable()
    End Function

    Private Function CleanString(input As String) As String
        If input Is Nothing Then Return Nothing

        Dim sb As New System.Text.StringBuilder(input.Length)

        For Each ch As Char In input
            Dim code As Integer = AscW(ch)

            ' Remove null byte
            If code = 0 Then Continue For

            ' Remove ASCII control chars except tab, LF, CR
            If code < 32 AndAlso code <> 9 AndAlso code <> 10 AndAlso code <> 13 Then Continue For

            ' Remove invalid surrogate halves
            If code >= &HD800 AndAlso code <= &HDFFF Then Continue For

            sb.Append(ch)
        Next

        Return sb.ToString()
    End Function



End Class
