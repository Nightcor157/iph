
Public Class dogmaEffects
    Inherits SDEFilesBase
    Implements IImporter(Of dogmaEffect)
    Implements IDatabaseImporter(Of dogmaEffect)

    Public Const BaseFileName As String = "dogmaEffects"
    Public Const dogmaEffectsModifierInfoTable As String = "dogmaEffectsModifierInfo"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dogmaEffect) Implements IImporter(Of dogmaEffect).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dogmaEffect)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dogmaEffect), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dogmaEffect).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build main table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("effectID", FieldType.smallint_type, 0, False, True),
            New DBTableField("name", FieldType.varchar_type, 400, True),
            New DBTableField("description", FieldType.varchar_type, 1000, True),
            New DBTableField("disallowAutoRepeat", FieldType.int_type, 0, True),
            New DBTableField("displayName", FieldType.varchar_type, 1000, True),
            New DBTableField("dischargeAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("distribution", FieldType.varchar_type, 100, True),
            New DBTableField("durationAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("effectCategoryID", FieldType.int_type, 0, True),
            New DBTableField("electronicChance", FieldType.int_type, 0, True),
            New DBTableField("fittingUsageChanceAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("falloffAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("guid", FieldType.varchar_type, 60, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("isAssistance", FieldType.int_type, 0, True),
            New DBTableField("isOffensive", FieldType.int_type, 0, True),
            New DBTableField("isWarpSafe", FieldType.int_type, 0, True),
            New DBTableField("npcUsageChanceAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("npcActivationChanceAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("propulsionChance", FieldType.int_type, 0, True),
            New DBTableField("published", FieldType.int_type, 0, True),
            New DBTableField("rangeAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("resistanceAttributeID", FieldType.smallint_type, 0, True),
            New DBTableField("rangeChance", FieldType.int_type, 0, True),
            New DBTableField("trackingSpeedAttributeID", FieldType.smallint_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Make modifier info table
        Table = New List(Of DBTableField) From {
            New DBTableField("effectID", FieldType.smallint_type, 0, True),
            New DBTableField("domain", FieldType.varchar_type, 50, True),
            New DBTableField("func", FieldType.varchar_type, 50, True),
            New DBTableField("groupID", FieldType.int_type, 0, True),
            New DBTableField("modifiedAttributeID", FieldType.int_type, 0, True),
            New DBTableField("modifyingAttributeID", FieldType.int_type, 0, True),
            New DBTableField("operation", FieldType.varchar_type, 50, True),
            New DBTableField("skillTypeID", FieldType.int_type, 0, True),
            New DBTableField("modifierEffectID", FieldType.smallint_type, 0, True)
        }

        Call UpdateDB.CreateTable(dogmaEffectsModifierInfoTable, Table)

        ' Create unique index on modifier info table
        'Dim IndexFields As New List(Of String) From {
        '    "effectID",
        '    "domain",
        '    "func",
        '    "groupID",
        '    "modifiedAttributeID",
        '    "modifyingAttributeID",
        '    "operation",
        '    "skillTypeID",
        '    "modifierEffectID"
        '}
        'Call UpdateDB.CreateIndex(dogmaEffectsModifierInfoTable, "IDX_" & dogmaEffectsModifierInfoTable & "_EID", IndexFields, True)

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

            ' Build the insert list
            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("effectID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("disallowAutoRepeat", BooleanField(.disallowAutoRepeat), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("dischargeAttributeID", NullableField(.dischargeAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("displayName", GetTranslation(.displayName, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("distribution", NullableField(.distribution), FieldType.varchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("durationAttributeID", NullableField(.durationAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("effectCategoryID", .effectCategoryID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("electronicChance", BooleanField(.electronicChance), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("falloffAttributeID", NullableField(.falloffAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("fittingUsageChanceAttributeID", NullableField(.fittingUsageChanceAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("guid", NullableField(.guid), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", NullableField(.iconID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("isAssistance", BooleanField(.isAssistance), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("isOffensive", BooleanField(.isOffensive), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("isWarpSafe", BooleanField(.isWarpSafe), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", .name, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("npcUsageChanceAttributeID", NullableField(.npcUsageChanceAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("npcActivationChanceAttributeID", NullableField(.npcActivationChanceAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("propulsionChance", BooleanField(.propulsionChance), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", BooleanField(.published), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("rangeAttributeID", NullableField(.rangeAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("rangeChance", BooleanField(.rangeChance), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("resistanceAttributeID", NullableField(.resistanceAttributeID), FieldType.smallint_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("trackingSpeedAttributeID", NullableField(.trackingSpeedAttributeID), FieldType.smallint_type))

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                If Not IsNothing(.modifierInfo) Then
                    ' Store modifier string data into the new table
                    Dim DataFields2 As New List(Of DBField)

                    If .modifierInfo.Count > 0 Then
                        For Each Record In .modifierInfo
                            DataFields2 = New List(Of DBField) From {
                                UpdateDB.BuildDatabaseField("effectID", DataField.Key, FieldType.smallint_type),
                                UpdateDB.BuildDatabaseField("domain", Record.domain, FieldType.varchar_type),
                                UpdateDB.BuildDatabaseField("func", Record.func, FieldType.varchar_type),
                                UpdateDB.BuildDatabaseField("groupID", NullableField(Record.groupID), FieldType.int_type),
                                UpdateDB.BuildDatabaseField("modifiedAttributeID", NullableField(Record.modifiedAttributeID), FieldType.int_type),
                                UpdateDB.BuildDatabaseField("modifyingAttributeID", NullableField(Record.modifyingAttributeID), FieldType.int_type),
                                UpdateDB.BuildDatabaseField("operation", NullableField(Record.operation), FieldType.varchar_type),
                                UpdateDB.BuildDatabaseField("skillTypeID", NullableField(Record.skillTypeID), FieldType.int_type),
                                UpdateDB.BuildDatabaseField("modifierEffectID", NullableField(Record.effectID), FieldType.int_type)
                            }

                            Call UpdateDB.InsertRecord(dogmaEffectsModifierInfoTable, UpdateDB.BuildOrderedRecord(dogmaEffectsModifierInfoTable, DataFields2))
                        Next
                    End If
                End If
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class dogmaEffect
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property disallowAutoRepeat As Boolean
    Public Property dischargeAttributeID As Object
    Public Property displayName As TranslatedNameField
    Public Property distribution As Object
    Public Property durationAttributeID As Object
    Public Property effectCategoryID As Integer
    Public Property electronicChance As Boolean
    Public Property falloffAttributeID As Object
    Public Property fittingUsageChanceAttributeID As Object
    Public Property guid As Object
    Public Property iconID As Object
    Public Property isAssistance As Boolean
    Public Property isOffensive As Boolean
    Public Property isWarpSafe As Boolean
    Public Property name As String
    Public Property npcActivationChanceAttributeID As Object
    Public Property npcUsageChanceAttributeID As Object
    Public Property propulsionChance As Boolean
    Public Property published As Boolean
    Public Property rangeAttributeID As Object
    Public Property rangeChance As Boolean
    Public Property resistanceAttributeID As Object
    Public Property trackingSpeedAttributeID As Object
    Public Property modifierInfo As List(Of dogmaEffectModifierInfo)
End Class

Public Class dogmaEffectModifierInfo
    Public Property domain As String
    Public Property effectID As Object
    Public Property func As String
    Public Property groupID As Object
    Public Property modifiedAttributeID As Object
    Public Property modifyingAttributeID As Object
    Public Property operation As Object
    Public Property skillTypeID As Object
End Class
