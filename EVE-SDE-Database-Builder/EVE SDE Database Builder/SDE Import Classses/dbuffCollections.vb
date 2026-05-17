Public Class dbuffCollections
    Inherits SDEFilesBase
    Implements IImporter(Of dbuffCollection)
    Implements IDatabaseImporter(Of dbuffCollection)

    Public Const BaseFileName As String = "dbuffCollections"
    Private Const itemModifiersTable As String = "dbuffItemModifiers"
    Private Const locationGroupModifiersTable As String = "dbuffLocationGroupModifiers"
    Private Const locationModifiersTable As String = "dbuffLocationModifiers"
    Private Const locationRequiredSkillModifiersTable As String = "dbuffLocationRequiredSkillModifiers"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dbuffCollection) Implements IImporter(Of dbuffCollection).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dbuffCollection)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dbuffCollection), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dbuffCollection).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("dbuffCollectionID", FieldType.int_type, 0, True),
            New DBTableField("aggregateMode", FieldType.varchar_type, 7, True),
            New DBTableField("developerDescription", FieldType.varchar_type, 100, True),
            New DBTableField("displayName", FieldType.varchar_type, 200, True),
            New DBTableField("operationName", FieldType.varchar_type, 15, True),
            New DBTableField("showOutputValueInUI", FieldType.varchar_type, 15, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As New List(Of String) From {
            "dbuffCollectionID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID", IndexFields)

        ' Extra tables
        Table = New List(Of DBTableField) From {
            New DBTableField("dbuffCollectionID", FieldType.int_type, 0, True),
            New DBTableField("dogmaAttributeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(itemModifiersTable, Table)

        Table = New List(Of DBTableField) From {
            New DBTableField("dbuffCollectionID", FieldType.int_type, 0, True),
            New DBTableField("dogmaAttributeID", FieldType.int_type, 0, True),
            New DBTableField("groupID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(locationGroupModifiersTable, Table)

        Table = New List(Of DBTableField) From {
            New DBTableField("dbuffCollectionID", FieldType.int_type, 0, True),
            New DBTableField("dogmaAttributeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(locationModifiersTable, Table)

        Table = New List(Of DBTableField) From {
            New DBTableField("dbuffCollectionID", FieldType.int_type, 0, True),
            New DBTableField("dogmaAttributeID", FieldType.int_type, 0, True),
            New DBTableField("skillID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(locationRequiredSkillModifiersTable, Table)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        ' Start processing
        Call InitGridRow(Params.RowLocation)
        Dim TotalRecords As Long = Records.Count

        ' Process Data
        For Each DataField In Records

            ' Build the insert list
            DataFields = New List(Of DBField)

            If CancelImport Then Exit Sub

            With DataField.Value
                ' Build the insert list for certificates
                DataFields.Add(UpdateDB.BuildDatabaseField("dbuffCollectionID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("aggregateMode", .aggregateMode, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("developerDescription", .developerDescription, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("displayName", GetTranslation(.displayName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("operationName", .operationName, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("showOutputValueInUI", .showOutputValueInUI, FieldType.nvarchar_type))

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' Add the other tables too
                If Not IsNothing(.itemModifiers) Then
                    For Each itemMod In .itemModifiers
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("dbuffCollectionID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("dogmaAttributeID", itemMod.dogmaAttributeID, FieldType.int_type)
                         }
                        Call UpdateDB.InsertRecord(itemModifiersTable, UpdateDB.BuildOrderedRecord(itemModifiersTable, DataFields2))
                    Next
                End If

                If Not IsNothing(.locationGroupModifiers) Then
                    For Each groupMod In .locationGroupModifiers
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("dbuffCollectionID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("dogmaAttributeID", groupMod.dogmaAttributeID, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("groupID", groupMod.groupID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(locationGroupModifiersTable, UpdateDB.BuildOrderedRecord(locationGroupModifiersTable, DataFields2))
                    Next
                End If

                If Not IsNothing(.locationModifiers) Then
                    For Each locmod In .locationModifiers
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("dbuffCollectionID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("dogmaAttributeID", locmod.dogmaAttributeID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(locationModifiersTable, UpdateDB.BuildOrderedRecord(locationModifiersTable, DataFields2))
                    Next
                End If

                If Not IsNothing(.locationRequiredSkillModifiers) Then
                    For Each skill In .locationRequiredSkillModifiers
                        DataFields2 = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("dbuffCollectionID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("dogmaAttributeID", skill.dogmaAttributeID, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("skillID", skill.skillID, FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord(locationRequiredSkillModifiersTable, UpdateDB.BuildOrderedRecord(locationRequiredSkillModifiersTable, DataFields2))
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

Public Class dbuffCollection
    Public Property _key As Long
    Public Property aggregateMode As String
    Public Property developerDescription As String
    Public Property displayName As TranslatedNameField
    Public Property itemModifiers As List(Of dbuffItemModifiers)
    Public Property locationGroupModifiers As List(Of dbuffLocationGroupModifiers)
    Public Property locationModifiers As List(Of dbuffLocationModifiers)
    Public Property locationRequiredSkillModifiers As List(Of dbuffLocationRequiredSkillModifiers)
    Public Property operationName As String
    Public Property showOutputValueInUI As String

End Class

Public Class dbuffItemModifiers
    Public Property dogmaAttributeID As Integer
End Class
Public Class dbuffLocationGroupModifiers
    Public Property dogmaAttributeID As Integer
    Public Property groupID As Integer
End Class
Public Class dbuffLocationModifiers
    Public Property dogmaAttributeID As Integer
End Class
Public Class dbuffLocationRequiredSkillModifiers
    Public Property dogmaAttributeID As Integer
    Public Property skillID As Integer
End Class