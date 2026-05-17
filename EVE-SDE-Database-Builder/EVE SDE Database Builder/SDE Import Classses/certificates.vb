
Public Class certificates
    Inherits SDEFilesBase
    Implements IImporter(Of certificate)
    Implements IDatabaseImporter(Of certificate)

    Public Const BaseFileName As String = "certificates"

    Private Const certificateSkills_Table As String = "certificateSkills"
    Private Const certificatesRecommendedTypes_Table As String = "certificateRecommendedTypes"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, certificate) Implements IImporter(Of certificate).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of certificate)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, certificate), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of certificate).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build all the tables to insert certificate data into. This includes the following tables:
        ' - certificates
        ' - certificateSkills
        ' - certificatesRecommendedTypes
        Call BuildCertificatesTable()
        Call BuildCertificateSkillsTable()
        Call BuildCertificateRecTypesTable()

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
                ' Build the insert list for certificates
                DataFields.Add(UpdateDB.BuildDatabaseField("certificateID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("groupID", .groupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("name", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))

                Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

                ' Insert skills and required types
                Call InsertCertificateSkills(DataField.Key, .skillTypes)
                Call InsertCertificateRecommendedTypes(DataField.Key, .recommendedFor)

            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub BuildCertificatesTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("certificateID", FieldType.int_type, 0, False, True),
            New DBTableField("groupID", FieldType.int_type, 0, True),
            New DBTableField("name", FieldType.varchar_type, 100, True),
            New DBTableField("description", FieldType.text_type, MaxFieldLen, True)
        }

        Call UpdateDB.CreateTable(BaseFileName, Table)

    End Sub

    Private Sub BuildCertificateSkillsTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("certificateID", FieldType.int_type, 0, True),
            New DBTableField("masteryLevel", FieldType.int_type, 0, True),
            New DBTableField("masteryText", FieldType.varchar_type, 10, True),
            New DBTableField("skillTypeID", FieldType.int_type, 0, True),
            New DBTableField("requiredSkillLevel", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(certificateSkills_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "certificateID"
        }
        Call UpdateDB.CreateIndex(certificateSkills_Table, "IDX_" & certificateSkills_Table & "_CID", IndexFields)

        IndexFields = New List(Of String) From {
            "skillTypeID"
        }
        Call UpdateDB.CreateIndex(certificateSkills_Table, "IDX_" & certificateSkills_Table & "_SID", IndexFields)

    End Sub

    Private Sub BuildCertificateRecTypesTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("certificateID", FieldType.int_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(certificatesRecommendedTypes_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "typeID",
            "certificateID"
        }
        Call UpdateDB.CreateIndex(certificatesRecommendedTypes_Table, "IDX_" & certificatesRecommendedTypes_Table & "_TID_CID", IndexFields)

    End Sub

    Private Sub InsertCertificateSkills(ByVal CertID As Integer, ByVal Skills As List(Of certificate.certificateMastery))
        Dim DataFields As List(Of DBField)

        Dim SkillTypeID As Integer
        Dim MasteryLevel As Integer
        Dim MasteryText As String
        Dim ReqSkillLevel As Integer

        If Not IsNothing(Skills) Then
            For Each Skill In Skills
                ' Build the insert list for certificate skills
                For i = 0 To 4 ' 5 loops for the masteries
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("certificateID", CertID, FieldType.int_type)
                    }
                    SkillTypeID = Skill._key
                    MasteryLevel = i + 1
                    MasteryText = ""
                    ReqSkillLevel = 0
                    ' Just select these based off of the loop
                    Select Case i
                        Case 0 ' basic
                            MasteryText = "Basic"
                            ReqSkillLevel = Skill.basic
                        Case 1 ' basic
                            MasteryText = "Standard"
                            ReqSkillLevel = Skill.standard
                        Case 2 ' basic
                            MasteryText = "Improved"
                            ReqSkillLevel = Skill.improved
                        Case 3 ' basic
                            MasteryText = "Advanced"
                            ReqSkillLevel = Skill.advanced
                        Case 4 ' basic
                            MasteryText = "Elite"
                            ReqSkillLevel = Skill.elite
                    End Select

                    If MasteryText <> "" Then
                        DataFields.Add(UpdateDB.BuildDatabaseField("masteryLevel", MasteryLevel, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("masteryText", MasteryText, FieldType.varchar_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("skillTypeID", SkillTypeID, FieldType.int_type))
                        DataFields.Add(UpdateDB.BuildDatabaseField("requiredSkillLevel", ReqSkillLevel, FieldType.int_type))
                    End If

                    Call UpdateDB.InsertRecord(certificateSkills_Table, UpdateDB.BuildOrderedRecord(certificateSkills_Table, DataFields))

                Next
            Next
        End If
    End Sub

    Private Sub InsertCertificateRecommendedTypes(ByVal CertID As Integer, ByVal TypeList As List(Of Integer))
        Dim DataFields As List(Of DBField)

        If Not IsNothing(TypeList) Then
            For Each recType In TypeList
                ' Build the insert list for recomended types
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("certificateID", CertID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("typeID", recType, FieldType.int_type)
                }

                Call UpdateDB.InsertRecord(certificatesRecommendedTypes_Table, UpdateDB.BuildOrderedRecord(certificatesRecommendedTypes_Table, DataFields))
            Next
        End If
    End Sub
End Class

Public Class certificate
    Public Property _key As Long
    Public Property description As TranslatedNameField
    Public Property groupID As Integer
    Public Property name As TranslatedNameField
    Public Property recommendedFor As List(Of Integer)
    ' key will be the skill ID and each mastery after that
    Public Property skillTypes As List(Of certificateMastery)

    Public Class certificateMastery
        Public Property _key As Long
        Public Property advanced As Integer
        Public Property basic As Integer
        Public Property elite As Integer
        Public Property improved As Integer
        Public Property standard As Integer
    End Class

End Class

