
Public Class typeBonus
    Inherits SDEFilesBase
    Implements IImporter(Of bonus)
    Implements IDatabaseImporter(Of bonus)

    Public Const BaseFileName As String = "typeBonus"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, bonus) Implements IImporter(Of bonus).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of bonus)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, bonus), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of bonus).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False, True),
            New DBTableField("iconID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable("typeBonus", Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & "typeBonus" & "_TID", IndexFields)

        Table = New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("bonus", FieldType.double_type, 0, True),
            New DBTableField("bonusText", FieldType.nvarchar_type, 1000, True),
            New DBTableField("importance", FieldType.int_type, 0, True),
            New DBTableField("isPositive", FieldType.int_type, 0, True),
            New DBTableField("unitID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable("typeBonusMisc", Table)

        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & "typeBonusMisc" & "_TID", IndexFields)

        Table = New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("bonus", FieldType.double_type, 0, True),
            New DBTableField("bonusText", FieldType.nvarchar_type, 1000, True),
            New DBTableField("importance", FieldType.int_type, 0, True),
            New DBTableField("unitID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable("typeBonusRole", Table)

        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & "typeBonusRole" & "_TID", IndexFields)

        Table = New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("typesID", FieldType.int_type, 0, False),
            New DBTableField("bonus", FieldType.double_type, 0, True),
            New DBTableField("bonusText", FieldType.nvarchar_type, 1000, True),
            New DBTableField("importance", FieldType.int_type, 0, True),
            New DBTableField("unitID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable("typeBonusTypes", Table)

        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & "typeBonusTypes" & "_TID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
            End With
            Call UpdateDB.InsertRecord("typeBonus", UpdateDB.BuildOrderedRecord("typeBonus", DataFields))

            If DataField.Value.miscBonuses IsNot Nothing Then
                For Each bonus In DataField.Value.miscBonuses
                    DataFields = New List(Of DBField)
                    With bonus
                        ' Build the insert list
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonus", .bonus, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonusText", GetTranslation(.bonusText, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("importance", .importance, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("isPositive", BooleanField(.isPositive), FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("unitID", .unitID, FieldType.int_type))
                    End With
                    Call UpdateDB.InsertRecord("typeBonusMisc", UpdateDB.BuildOrderedRecord("typeBonusMisc", DataFields))
                Next
            End If

            If DataField.Value.roleBonuses IsNot Nothing Then
                For Each bonus In DataField.Value.roleBonuses
                    DataFields = New List(Of DBField)
                    With bonus
                        ' Build the insert list
                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonus", .bonus, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonusText", GetTranslation(.bonusText, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("importance", .importance, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("unitID", .unitID, FieldType.int_type))
                    End With
                    Call UpdateDB.InsertRecord("typeBonusRole", UpdateDB.BuildOrderedRecord("typeBonusRole", DataFields))
                Next
            End If

            If DataField.Value.types IsNot Nothing Then
                For Each kvp In DataField.Value.types
                    For Each b In kvp._value
                        DataFields = New List(Of DBField)

                        DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("typesID", kvp._key, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonus", b.bonus, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("bonusText", GetTranslation(b.bonusText, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("importance", b.importance, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("unitID", b.unitID, FieldType.int_type))

                        Call UpdateDB.InsertRecord("typeBonusTypes", UpdateDB.BuildOrderedRecord("typeBonusTypes", DataFields))
                    Next
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class bonus
    Public Property _key As Long
    Public Property iconID As Integer?
    Public Property miscBonuses As List(Of MiscBonus)
    Public Property roleBonuses As List(Of RoleBonus)
    Public Property types As List(Of TypeBonusEntry)
End Class

Public Class TypeBonusEntry
    Public Property _key As Integer
    Public Property _value As List(Of bonusType)
End Class

Public Class bonusType
    Public Property bonus As Double?
    Public Property bonusText As TranslatedNameField
    Public Property importance As Integer
    Public Property unitID As Integer?
End Class

Public Class MiscBonus
    Public Property bonus As Double?
    Public Property bonusText As TranslatedNameField
    Public Property importance As Integer
    Public Property isPositive As Boolean
    Public Property unitID As Integer?
End Class

Public Class RoleBonus
    Public Property bonus As Double?
    Public Property bonusText As TranslatedNameField
    Public Property importance As Integer
    Public Property unitID As Integer?
End Class


