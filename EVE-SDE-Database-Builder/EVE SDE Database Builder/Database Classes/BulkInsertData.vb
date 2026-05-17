Namespace EVE_SDE_Database_Builder

    Public Class BulkInsertData

        ' Name of the table (e.g., "typeEffects")
        Public Property TableName As String

        ' The SQL CREATE TABLE statement (optional but useful)
        Public Property TableSQL As String

        ' The list of fields (schema definition)
        Public Property Fields As List(Of DBField)

        ' Optional: path to schema file or metadata
        Public Property SchemaFile As String

        Public Sub New()
            Fields = New List(Of DBField)
        End Sub

    End Class

End Namespace

