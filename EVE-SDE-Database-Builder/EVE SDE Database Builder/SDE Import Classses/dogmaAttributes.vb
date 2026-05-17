
Public Class dogmaAttributes
    Inherits SDEFilesBase
    Implements IImporter(Of dogmaAttribute)
    Implements IDatabaseImporter(Of dogmaAttribute)

    Public Const BaseFileName As String = "dogmaAttributes"
    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dogmaAttribute) Implements IImporter(Of dogmaAttribute).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dogmaAttribute)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dogmaAttribute), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dogmaAttribute).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("attributeID", FieldType.smallint_type, 0, False), ' Dupes ok in uprising expansion
            New DBTableField("attributeName", FieldType.varchar_type, 500, True),
            New DBTableField("attributeCategoryID", FieldType.int_type, 0, True),
            New DBTableField("description", FieldType.varchar_type, 1000, True),
            New DBTableField("displayName", FieldType.varchar_type, 1000, True),
            New DBTableField("dataType", FieldType.varchar_type, 100, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("defaultValue", FieldType.double_type, 0, True),
            New DBTableField("published", FieldType.smallint_type, 0, True),
            New DBTableField("stackable", FieldType.smallint_type, 0, True),
            New DBTableField("unitID", FieldType.int_type, 0, True),
            New DBTableField("highIsGood", FieldType.smallint_type, 0, True),
            New DBTableField("tooltipDescription", FieldType.varchar_type, 1000, True),
            New DBTableField("tooltipTitle", FieldType.varchar_type, 1000, True),
            New DBTableField("chargeRechargeTimeID", FieldType.smallint_type, 0, True),
            New DBTableField("displayWhenZero", FieldType.smallint_type, 0, True),
            New DBTableField("maxAttributeID", FieldType.int_type, 0, True),
            New DBTableField("minAttributeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "attributeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_AID", IndexFields)

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
                ' Build the insert list
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("attributeID", DataField.Key, FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("attributeName", .name, FieldType.varchar_type),
                    UpdateDB.BuildDatabaseField("attributeCategoryID", NullableField(.attributeCategoryID), FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("description", NullableField(.description), FieldType.varchar_type),
                    UpdateDB.BuildDatabaseField("displayName", GetTranslation(.displayName, Params.ImportLanguageCode), FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("dataType", .dataType, FieldType.varchar_type),
                    UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type),
                    UpdateDB.BuildDatabaseField("defaultValue", .defaultValue, FieldType.double_type),
                    UpdateDB.BuildDatabaseField("published", BooleanField(.published), FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("stackable", BooleanField(.stackable), FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("unitID", NullableField(.unitID), FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("highIsGood", BooleanField(.highIsGood), FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("tooltipDescription", GetTranslation(.tooltipDescription, Params.ImportLanguageCode), FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("tooltipTitle", GetTranslation(.tooltipTitle, Params.ImportLanguageCode), FieldType.nvarchar_type),
                    UpdateDB.BuildDatabaseField("chargeRechargeTimeID", NullableField(.chargeRechargeTimeID), FieldType.int_type),
                    UpdateDB.BuildDatabaseField("displayWhenZero", BooleanField(.displayWhenZero), FieldType.smallint_type),
                    UpdateDB.BuildDatabaseField("maxAttributeID", NullableField(.maxAttributeID), FieldType.int_type),
                    UpdateDB.BuildDatabaseField("minAttributeID", NullableField(.minAttributeID), FieldType.int_type)
                }

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' Update grid progress
                Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
                Count += 1
            End With
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class dogmaAttribute
    Public Property _key As Long
    Public Property attributeCategoryID As Object
    Public Property chargeRechargeTimeID As Object
    Public Property dataType As Integer
    Public Property defaultValue As Double
    Public Property description As Object
    Public Property displayName As TranslatedNameField
    Public Property displayWhenZero As Boolean
    Public Property highIsGood As Boolean
    Public Property iconID As Object
    Public Property maxAttributeID As Object
    Public Property minAttributeID As Object
    Public Property name As String
    Public Property published As Boolean
    Public Property stackable As Boolean
    Public Property tooltipDescription As TranslatedNameField
    Public Property tooltipTitle As TranslatedNameField
    Public Property unitID As Object

End Class