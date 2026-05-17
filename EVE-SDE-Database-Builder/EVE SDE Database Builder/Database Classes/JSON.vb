''' <summary>
''' Class to create a JSON "database" and insert data into it.
''' </summary>
Public Class JSONDB
    Inherits DBFilesBase
    Public ReadOnly DatabasePath As String
    ''' <summary>
    ''' Constructor class for a JSON "database". 
    ''' </summary>
    ''' <param name="DatabaseFileNameandPath">Name of the database to create.</param>
    ''' <param name="Success">True if the database successfully created.</param>
    Public Sub New(ByVal DatabaseFileNameandPath As String, ByRef Success As Boolean)
        MyBase.New(DatabaseFileNameandPath, DatabaseType.JSON)

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try
            ' Build a new folder for the 'database' 
            Call CreateNewDirectory(DatabaseFileNameandPath)
            DatabasePath = DatabaseFileNameandPath
            Success = True

        Catch ex As Exception
            Call ShowErrorMessage(ex)
            Success = False
        End Try

    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub CloseDB()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub BeginSQLTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub CommitSQLTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Copies each file set to the JSON "database" folder
    ''' </summary>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub CreateIndex(ByVal TableName As String, ByVal IndexName As String, IndexFields As List(Of String),
                           Optional Unique As Boolean = False, Optional Clustered As Boolean = False)
        Application.DoEvents()
    End Sub

    ''' <summary>
    ''' Does nothing for JSON
    ''' </summary>
    Public Sub InsertRecord(ByVal TableName As String, Record As List(Of DBField))
        Application.DoEvents()
    End Sub

End Class
