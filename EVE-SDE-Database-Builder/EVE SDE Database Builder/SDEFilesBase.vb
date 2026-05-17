Imports System.IO
Imports System.Text.Json
Imports EVE_SDE_Database_Builder.SDEFilesBase

' Base class for initializing and closing import file process - all other classes will inherit this so put common functions here
Public Class SDEFilesBase

    Protected SDEFilePath As String ' full path of the file we are processing
    Protected TableName As String ' Name of the table (based on file name) we will insert data into
    Protected UpdateDB As Object ' The database object for the class used to insert data into the database

    Public NullValue As String = "NULL"

    Public Const MaxFieldLen As Integer = -1

    ' DatabaseLocation is the file path or the instance name
    Public Sub New(ByVal SentFileName As String, ByVal SentFilePath As String, ByRef DBConnectionRef As Object)

        ' Save the location and name of the JSON file we are processing 
        SDEFilePath = SentFilePath & "\" & SentFileName & ".jsonl"

        ' Table name will be the file name in almost all cases - update if needed
        If SentFileName <> "" Then
            TableName = SentFileName
        End If

        UpdateDB = DBConnectionRef

    End Sub

    ''' <summary>
    ''' This is a reusalbe JSONL loader for each SDE file
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <param name="keySelector"></param>
    ''' <returns></returns>
    Protected Function ImportJSONlFile(Of T As Class)(path As String, keySelector As Func(Of T, Long)) As Dictionary(Of Long, T)

        Dim result As New Dictionary(Of Long, T)

        For Each line In File.ReadLines(path)
            If String.IsNullOrWhiteSpace(line) Then Continue For

            Dim obj As T = JsonSerializer.Deserialize(Of T)(line)

            Dim key As Long = keySelector(obj)
            result(key) = obj
        Next

        Return result
    End Function

    ' Parameters to import a SDE file
    Public Structure ImportParameters
        Dim RowLocation As Integer
        Dim InsertRecords As Boolean ' For skipping inserts and just building the tables
        Dim ImportLanguageCode As String ' Language code for importing translated text
        Dim DatabaseType As DatabaseType
    End Structure

    Public Structure SDEImportTaskObject
        Dim SDERecordSet As Dictionary(Of Long, Object) ' The records imported from the SDE file
        Dim ClassName As String ' The class name of the records
    End Structure

End Class
Public Interface IImporter(Of T)
    Function ImportFile() As Dictionary(Of Long, T)
End Interface

Public Interface IDatabaseImporter(Of T)
    Sub InsertDatatoDatabase(
        records As Dictionary(Of Long, T),
        params As ImportParameters)
End Interface

Public Class ImportCoordinator
    Public Sub RunDatabaseImport(Of T)(importer As IDatabaseImporter(Of T), data As Dictionary(Of Long, T), params As ImportParameters)
        importer.InsertDatatoDatabase(data, params)
    End Sub

End Class

