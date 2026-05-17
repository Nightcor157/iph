
Public Class metaGroups
    Inherits SDEFilesBase
    Implements IImporter(Of metaGroup)
    Implements IDatabaseImporter(Of metaGroup)

    Public Const BaseFileName As String = "metaGroups"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, metaGroup) Implements IImporter(Of metaGroup).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of metaGroup)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, metaGroup), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of metaGroup).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        Dim Table As New List(Of DBTableField) From {
            New DBTableField("metaGroupID", FieldType.smallint_type, 0, False, True),
            New DBTableField("description", FieldType.nvarchar_type, 1000, True),
            New DBTableField("b_color", FieldType.double_type, 0, True),
            New DBTableField("g_color", FieldType.double_type, 0, True),
            New DBTableField("r_color", FieldType.double_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("iconSuffix", FieldType.nvarchar_type, 30, True),
            New DBTableField("name", FieldType.nvarchar_type, 100, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("metaGroupID", DataField.Key, FieldType.int_type))
                If Not IsNothing(.color) Then
                    DataFields.Add(UpdateDB.BuildDatabaseField("b_color", .color.b, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("g_color", .color.g, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("r_color", .color.r, FieldType.double_type))
                End If
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconSuffix", NullableField(.iconSuffix), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class metaGroup
    Public Property _key As Long
    Public Property color As colorBGR
    Public Property description As TranslatedNameField
    Public Property iconID As Object
    Public Property iconSuffix As Object
    Public Property name As TranslatedNameField
End Class

Public Class colorBGR
    Public Property b As Double
    Public Property g As Double
    Public Property r As Double
End Class