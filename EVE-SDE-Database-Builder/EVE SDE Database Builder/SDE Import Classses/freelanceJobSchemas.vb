
Public Class freelanceJobSchemas
    Inherits SDEFilesBase
    Implements IImporter(Of freelanceJob)
    Implements IDatabaseImporter(Of freelanceJob)

    Public Const BaseFileName As String = "freelanceJobSchemas"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, freelanceJob) Implements IImporter(Of freelanceJob).ImportFile

        FileNameErrorTracker = BaseFileName
        Return ImportJSONlFile(Of freelanceJob)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, freelanceJob), Params As ImportParameters) _
        Implements IDatabaseImporter(Of freelanceJob).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0
        Dim ValueString As String = ""

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("jobName", FieldType.nvarchar_type, 50, False, True),
            New DBTableField("iconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("title", FieldType.nvarchar_type, 100, True),
            New DBTableField("description", FieldType.nvarchar_type, 500, True),
            New DBTableField("targetDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("progressDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("rewardDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("contributionMult_defaultValue", FieldType.int_type, 0, True),
            New DBTableField("contributionMult_description", FieldType.nvarchar_type, 500, True),
            New DBTableField("contributionMult_iconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("contributionMult_maxValue", FieldType.int_type, 0, True),
            New DBTableField("contributionMult_minValue", FieldType.double_type, 0, True),
            New DBTableField("contributionMult_title", FieldType.nvarchar_type, 500, True),
            New DBTableField("contributionMult_unsetDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("maxContribPerParticipant_title", FieldType.nvarchar_type, 500, True),
            New DBTableField("maxContribPerParticipant_iconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("maxContribPerParticipant_description", FieldType.nvarchar_type, 500, True),
            New DBTableField("maxContribPerParticipant_unsetDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("maxProgressPerContribution_title", FieldType.nvarchar_type, 500, True),
            New DBTableField("maxProgressPerContribution_iconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("maxProgressPerContribution_description", FieldType.nvarchar_type, 500, True),
            New DBTableField("maxProgressPerContribution_unsetDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("schemaID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "jobName"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        ' Content tags table
        Dim freelanceJobSchemasContentTagsTable As String = "freelanceJovSchemasContentTags"

        Table = New List(Of DBTableField) From {
            New DBTableField("jobName", FieldType.nvarchar_type, 50, False, False),
            New DBTableField("contentTag", FieldType.nvarchar_type, 1000, False)
        }

        Call UpdateDB.CreateTable(freelanceJobSchemasContentTagsTable, Table)

        IndexFields = New List(Of String) From {
            "jobName"
        }
        Call UpdateDB.CreateIndex(freelanceJobSchemasContentTagsTable, "IDX_" & freelanceJobSchemasContentTagsTable & "_CID", IndexFields)

        ' Parameters table
        Dim freelanceJobSchemasParametersTable As String = "freelanceJobSchemasParameters"

        Table = New List(Of DBTableField) From {
            New DBTableField("jobName", FieldType.nvarchar_type, 50, False, False),
            New DBTableField("parameterName", FieldType.nvarchar_type, 50, True),
            New DBTableField("parameterType", FieldType.nvarchar_type, 50, True),
            New DBTableField("boolChoiceLabel", FieldType.nvarchar_type, 50, True),
            New DBTableField("boolDefault", FieldType.int_type, 0, True),
            New DBTableField("boolDescription", FieldType.nvarchar_type, 500, True),
            New DBTableField("boolIconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("boolOptionFalseDescription", FieldType.nvarchar_type, 255, True),
            New DBTableField("boolOptionFalseTitle", FieldType.nvarchar_type, 255, True),
            New DBTableField("boolOPtionTrueDescription", FieldType.nvarchar_type, 255, True),
            New DBTableField("boolOptionTrueTitle", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryDescription", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryIconID", FieldType.nchar_type, 50, True),
            New DBTableField("itemDeliveryTitle", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryLocationAcceptedValueTypes", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryLocationDescription", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryLocationIconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("itemDeliveryLocationMaxEntries", FieldType.int_type, 0, True),
            New DBTableField("itemDeliveryLocationTitle", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryLocationUnsetDescription", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryItemTypeAcceptedValueTypes", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryItemTypeDescription", FieldType.nchar_type, 255, True),
            New DBTableField("itemDeliveryItemTypeIconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("itemDeliveryItemTypeTitle", FieldType.nvarchar_type, 255, True),
            New DBTableField("itemDeliveryItemTypeUnsetDescription", FieldType.nvarchar_type, 255, True),
            New DBTableField("matcherAcceptedValueTypes", FieldType.nvarchar_type, 255, True),
            New DBTableField("matcherDescription", FieldType.nchar_type, 255, True),
            New DBTableField("matcherIconID", FieldType.nvarchar_type, 50, True),
            New DBTableField("matcherMaxEntries", FieldType.int_type, 0, True),
            New DBTableField("matcherOptional", FieldType.int_type, 0, True),
            New DBTableField("matcherTitle", FieldType.nvarchar_type, 255, True),
            New DBTableField("matcherType", FieldType.nvarchar_type, 100, True),
            New DBTableField("matcherUnsetDescription", FieldType.nvarchar_type, 255, True)
        }

        Call UpdateDB.CreateTable(freelanceJobSchemasParametersTable, Table)

        IndexFields = New List(Of String) From {
            "jobName"
        }
        Call UpdateDB.CreateIndex(freelanceJobSchemasParametersTable, "IDX_" & freelanceJobSchemasParametersTable & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("schemaID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("jobName", ._key, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("title", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("targetDescription", GetTranslation(.targetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("progressDescription", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("rewardDescription", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))

                If Not IsNothing(.contributionMultiplier) Then
                    With .contributionMultiplier
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_defaultValue", .defaultValue, FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_iconID", .iconID, FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_maxValue", .maxValue, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_minValue", .minValue, FieldType.double_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_title", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("contributionMult_unsetDescription", GetTranslation(.unsetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                    End With
                End If

                If Not IsNothing(.maxContributionsPerParticipant) Then
                    With .maxContributionsPerParticipant
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxContribPerParticipant_title", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxContribPerParticipant_iconID", .iconID, FieldType.nchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxContribPerParticipant_description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxContribPerParticipant_unsetDescription", GetTranslation(.unsetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                    End With
                End If

                If Not IsNothing(.maxProgressPerContribution) Then
                    With .maxProgressPerContribution
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxProgressPerContribution_title", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxProgressPerContribution_iconID", .iconID, FieldType.nchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxProgressPerContribution_description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("maxProgressPerContribution_unsetDescription", GetTranslation(.unsetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                    End With
                End If

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' Enter content tags
                If Not IsNothing(.contentTags) Then
                    For Each tag In .contentTags
                        DataFields = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("jobName", ._key, FieldType.nvarchar_type),
                            UpdateDB.BuildDatabaseField("contentTag", tag, FieldType.nvarchar_type)
                        }
                        Call UpdateDB.InsertRecord(freelanceJobSchemasContentTagsTable, UpdateDB.BuildOrderedRecord(freelanceJobSchemasContentTagsTable, DataFields))
                    Next
                End If

                ' Now do parameters in separate table
                If Not IsNothing(.parameters) Then
                    For Each ParameterObject In .parameters
                        With ParameterObject
                            DataFields = New List(Of DBField)
                            ValueString = ""
                            DataFields.Add(UpdateDB.BuildDatabaseField("jobName", DataField.Value._key, FieldType.nvarchar_type))
                            DataFields.Add(UpdateDB.BuildDatabaseField("parameterName", .Key, FieldType.nvarchar_type))
                            If Not IsNothing(.Value.boolean) Then
                                DataFields.Add(UpdateDB.BuildDatabaseField("parameterType", "boolean", FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolChoiceLabel", GetTranslation(.Value.boolean.choiceLabel, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolDefault", BooleanField(.Value.boolean.default), FieldType.int_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolDescription", GetTranslation(.Value.boolean.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolIconID", .Value.boolean.iconID, FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolOptionFalseDescription", GetTranslation(.Value.boolean.optionFalse.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolOptionFalseTitle", GetTranslation(.Value.boolean.optionFalse.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolOPtionTrueDescription", GetTranslation(.Value.boolean.optionTrue.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("boolOptionTrueTitle", GetTranslation(.Value.boolean.optionTrue.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                            ElseIf Not IsNothing(.Value.itemDelivery) Then
                                With .Value.itemDelivery
                                    For Each acceptedValue In .deliveryLocation.acceptedValueTypes
                                        ValueString &= acceptedValue & ";"
                                    Next
                                    DataFields.Add(UpdateDB.BuildDatabaseField("parameterType", "itemDelivery", FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryDescription", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryIconID", .iconID, FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryTitle", GetTranslation(.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    ' deliveryLocation
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryLocationAcceptedValueTypes", ValueString, FieldType.nchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryLocationDescription", GetTranslation(.deliveryLocation.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryLocationIconID", .deliveryLocation.iconID, FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryLocationMaxEntries", .deliveryLocation.maxEntries, FieldType.int_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryLocationTitle", GetTranslation(.deliveryLocation.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryLocationUnsetDescription", GetTranslation(.deliveryLocation.unsetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    ' inventoryType
                                    For Each acceptedValue In .inventoryType.acceptedValueTypes
                                        ValueString &= acceptedValue & ";"
                                    Next
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryItemTypeAcceptedValueTypes", ValueString, FieldType.nchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryItemTypeDescription", GetTranslation(.deliveryLocation.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryItemTypeIconID", .deliveryLocation.iconID, FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryItemTypeTitle", GetTranslation(.deliveryLocation.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                    DataFields.Add(UpdateDB.BuildDatabaseField("itemDeliveryItemTypeUnsetDescription", GetTranslation(.deliveryLocation.unsetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                End With
                            ElseIf Not IsNothing(.Value.matcher) Then
                                DataFields.Add(UpdateDB.BuildDatabaseField("parameterType", "matcher", FieldType.nvarchar_type))
                                For Each acceptedValue In .Value.matcher.acceptedValueTypes
                                    ValueString &= acceptedValue & ";"
                                Next
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherAcceptedValueTypes", ValueString, FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherDescription", GetTranslation(.Value.matcher.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherIconID", .Value.matcher.iconID, FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherMaxEntries", .Value.matcher.maxEntries, FieldType.int_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherOptional", BooleanField(.Value.matcher.optional), FieldType.int_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherTitle", GetTranslation(.Value.matcher.title, Params.ImportLanguageCode), FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherType", .Value.matcher.type, FieldType.nvarchar_type))
                                DataFields.Add(UpdateDB.BuildDatabaseField("matcherUnsetDescription", GetTranslation(.Value.matcher.unsetDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                            End If

                            Call UpdateDB.InsertRecord(freelanceJobSchemasParametersTable, UpdateDB.BuildOrderedRecord(freelanceJobSchemasParametersTable, DataFields))

                        End With
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

Public Class freelanceJob
    Public Property _key As Long
    Public Property contentTags As List(Of String)
    Public Property contributionMultiplier As Multiplier
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property maxContributionsPerParticipant As Contribution
    Public Property maxProgressPerContribution As Contribution
    Public Property parameters As Dictionary(Of String, schemaParameter)
    Public Property progressDescription As TranslatedNameField
    Public Property rewardDescription As TranslatedNameField
    Public Property targetDescription As TranslatedNameField
    Public Property title As TranslatedNameField

End Class

Public Class Multiplier
    Public Property defaultValue As Integer
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property maxValue As Integer
    Public Property minValue As Double
    Public Property title As TranslatedNameField
    Public Property unsetDescription As TranslatedNameField
End Class

Public Class Contribution
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property title As TranslatedNameField
    Public Property unsetDescription As TranslatedNameField
End Class
' boolean, itemDelivery, matcher - key is what the thing is, then the type is one of these three, which all need to be objects
Public Class schemaParameter
    Public Property [boolean] As booleanParameter
    Public Property itemDelivery As itemDelveryParameter
    Public Property matcher As matcherParameter
End Class

Public Class booleanParameter
    Public Property choiceLabel As TranslatedNameField
    Public Property [default] As Object
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property optionFalse As OptionClass
    Public Property optionTrue As OptionClass

End Class
Public Class OptionClass
    Public Property description As TranslatedNameField
    Public Property title As TranslatedNameField
End Class

Public Class itemDelveryParameter
    Public Property deliveryLocation As deliveryLoc
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property inventoryType As inventoryTypeParam
    Public Property title As TranslatedNameField
    Public Property unsetdescription As TranslatedNameField
End Class

Public Class deliveryLoc
    Public Property acceptedValueTypes As List(Of String)
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property maxEntries As Integer
    Public Property title As TranslatedNameField
    Public Property unsetDescription As TranslatedNameField
End Class

Public Class inventoryTypeParam
    Public Property acceptedValueTypes As List(Of String)
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property title As TranslatedNameField
    Public Property unsetDescription As TranslatedNameField

End Class

Public Class matcherParameter
    Public Property acceptedValueTypes As List(Of String)
    Public Property description As TranslatedNameField
    Public Property iconID As String
    Public Property maxEntries As Integer
    Public Property [optional] As String
    Public Property title As TranslatedNameField
    Public Property [type] As String
    Public Property unsetDescription As TranslatedNameField
End Class