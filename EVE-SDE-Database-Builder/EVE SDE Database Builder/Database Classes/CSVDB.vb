
Imports System.IO
Imports System.Security.AccessControl
Imports System.Text

''' <summary>
''' Class to create a CSV "database" and insert data into it.
''' </summary>
Public Class CSVDB
    Inherits DBFilesBase

    Private ReadOnly DB As String ' Folder Path for all 'tables' as files

    Private ReadOnly DELIMITER As String = ""
    Private Const CSVExtention As String = ".csv"
    Private ReadOnly UseNullforBlanks As Boolean = False

    Private CSVColumnOrder As New Dictionary(Of String, List(Of String))

    ''' <summary>
    ''' Constructor class for a CSV "database". 
    ''' </summary>
    ''' <param name="DatabaseFileNameandPath">Name of the database to create.</param>
    ''' <param name="Success">True if the database successfully created.</param>
    ''' <param name="AllowDirectoryFullAccess">Optional boolean to allow access to the full directory to allow other DB classes access to the folder</param>
    ''' <param name="ExportasSSV">Export the data in Semi-colon separated values for EU users</param>
    Public Sub New(ByVal DatabaseFileNameandPath As String, ByRef Success As Boolean, ByVal InsertNullforBlankValues As Boolean,
                   Optional ByVal AllowDirectoryFullAccess As Boolean = False, Optional ByVal ExportasSSV As Boolean = False,
                   Optional ByVal CreateDirectory As Boolean = True)
        MyBase.New(DatabaseFileNameandPath, DatabaseType.CSV)

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try
            ' Build a new folder for the 'database' 
            If CreateDirectory Then
                Call CreateNewDirectory(DatabaseFileNameandPath)
            End If

            ' Set the folder access if needed (mainly for postgresql bulk import)
            If AllowDirectoryFullAccess Then
                Dim UserAccount As String = "EVERYONE" 'Specify the user here, to allow stuff like postgresql to have access to the folder
                Dim FolderInfo As New DirectoryInfo(DatabaseFileNameandPath)
                Dim FolderSecurity As New DirectorySecurity
                FolderSecurity.AddAccessRule(New FileSystemAccessRule(UserAccount, FileSystemRights.Read, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
                FolderInfo.SetAccessControl(FolderSecurity)
            End If

            If Right(DatabaseFileNameandPath, 1) <> "\" Then
                DB = DatabaseFileNameandPath & "\"
            Else
                DB = DatabaseFileNameandPath
            End If

            If ExportasSSV Then
                DELIMITER = SEMICOLON
            Else
                DELIMITER = COMMA
            End If

            UseNullforBlanks = InsertNullforBlankValues

            Success = True

        Catch ex As Exception
            Call ShowErrorMessage(ex)
            Success = False
        End Try

    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub CloseDB()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub BeginSQLTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub CommitSQLTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Creates a table on the server for the sent table name in the structure sent by table structure
    ''' </summary>
    ''' <param name="TableName">Name of the table to create.</param>
    ''' <param name="TableStructure">List of table fields that define the table.</param>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Dim TableStream As StreamWriter
        Dim OutputText As String = ""
        Dim colOrder As New List(Of String)

        ' "drop table" if it exists
        If File.Exists(DB & TableName & CSVExtention) Then
            File.Delete(DB & TableName & CSVExtention)
        End If

        ' Build the file
        Try
            TableStream = File.CreateText(DB & TableName & CSVExtention)
        Catch ex As Exception
            ShowErrorMessage(ex)
            Exit Sub
        End Try

        ' Add fields - cacheing each
        For Each Field In TableStructure
            With Field
                OutputText = OutputText & Field.FieldName & DELIMITER
            End With
            colOrder.Add(Field.FieldName)
        Next

        ' Save this for future inserts
        CSVColumnOrder(TableName) = colOrder

        TableStream.WriteLine(String.Join(DELIMITER, StripLastCharacter(OutputText)))
        TableStream.Flush()
        TableStream.Close()
        TableStream.Dispose()

    End Sub

    ''' <summary>
    ''' Does nothing for CSV
    ''' </summary>
    Public Sub CreateIndex(ByVal TableName As String, ByVal IndexName As String, IndexFields As List(Of String),
                           Optional Unique As Boolean = False, Optional Clustered As Boolean = False)
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Converts a US formatted number (10,000.00) to a EU formatted number (10.000,00)
    ''' </summary>
    ''' <param name="USFormattedValue">The US formatted number</param>
    ''' <returns></returns>
    Private Function ConvertUStoEUDecimal(ByVal USFormattedValue As String) As String
        Dim TempString As String

        TempString = USFormattedValue

        ' First replace any periods with pipes
        TempString = TempString.Replace(".", "|")

        ' Now change the commas to periods
        TempString = TempString.Replace(",", ".")

        ' Now change the pipes to commas
        TempString = TempString.Replace("|", ",")

        ' Last update, re-set the names for R.A.M.s and R.Dbs back
        TempString = TempString.Replace("R,A,M,", "R.A.M.")
        TempString = TempString.Replace("R,Db", "R.Db")

        Return TempString

    End Function

    ''' <summary>
    ''' Inserts the list of record values (fields) into the table name provided.
    ''' </summary>
    ''' <param name="TableName">Table to insert records.</param>
    ''' <param name="Record">List of table fields that make up the record.</param>
    Public Sub InsertRecord(ByVal TableName As String, Record As List(Of DBField))
        Dim filePath As String = DB & TableName & CSVExtention
        Dim TableStream As StreamWriter = File.AppendText(filePath)
        Dim OutputText As New StringBuilder()

        ' Get the correct column order
        Dim order = CSVColumnOrder(TableName)

        ' Record is assumed to be in the same order as the schema
        ' If not, reorder it ONCE when building the DBField list
        For i = 0 To order.Count - 1
            Dim field = Record(i)

            Dim value = field.FieldValue

            ' Decimal formatting
            If (field.FieldType = FieldType.double_type Or
            field.FieldType = FieldType.float_type Or
            field.FieldType = FieldType.real_type) AndAlso DELIMITER = SEMICOLON Then

                value = ConvertUStoEUDecimal(value)
            End If

            ' NULL handling
            If UseNullforBlanks AndAlso (value Is Nothing OrElse value.ToString() = "") Then
                value = NULL
            End If

            OutputText.Append(value)
            OutputText.Append(DELIMITER)
        Next

        ' Remove last delimiter
        OutputText.Length -= 1

        TableStream.WriteLine(OutputText.ToString())
        TableStream.Flush()
        TableStream.Close()
    End Sub

    Public Overrides Function BuildOrderedRecord(TableName As String, fields As List(Of DBField)) As List(Of DBField)
        Dim ordered As New List(Of DBField)
        Dim map As New Dictionary(Of String, DBField)(StringComparer.OrdinalIgnoreCase)

        ' Build lookup once
        For Each f In fields
            map(f.FieldName) = f
        Next

        ' Build full ordered list
        For Each colName In CSVColumnOrder(TableName)
            If map.ContainsKey(colName) Then
                ordered.Add(map(colName))
            Else
                ' Missing field → insert NULL
                ordered.Add(New DBField With {
                .FieldName = colName,
                .FieldValue = "",
                .FieldType = FieldType.text_type   ' or whatever default makes sense
            })
            End If
        Next

        Return ordered
    End Function


End Class