
Public Class cloneGrades
    Inherits SDEFilesBase
    Implements IImporter(Of cloneGrade)
    Implements IDatabaseImporter(Of cloneGrade)

    Public Const BaseFileName As String = "cloneGrades"
    Private Const cloneGradeSkillsTable As String = "cloneGradesSkills"
    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, cloneGrade) Implements IImporter(Of cloneGrade).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of cloneGrade)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, cloneGrade), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of cloneGrade).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim DataFields2 As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
                New DBTableField("cloneGradeID", FieldType.int_type, 0, False, True),
                New DBTableField("cloneGradeName", FieldType.varchar_type, 100, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "cloneGradeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        ' Build the skills table too
        Table = New List(Of DBTableField) From {
            New DBTableField("cloneGradeID", FieldType.int_type, 0, False),
            New DBTableField("typeID", FieldType.int_type, 0, False),
            New DBTableField("skillLevel", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(cloneGradeSkillsTable, Table)

        ' Create index
        IndexFields = New List(Of String) From {
            "cloneGradeID"
        }
        Call UpdateDB.CreateIndex(cloneGradeSkillsTable, "IDX_" & cloneGradeSkillsTable & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("cloneGradeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("cloneGradeName", .name, FieldType.nvarchar_type))

                ' Add the skill data now
                For Each skill In .skills
                    DataFields2 = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("cloneGradeID", DataField.Key, FieldType.smallint_type),
                        UpdateDB.BuildDatabaseField("typeID", skill.typeID, FieldType.varchar_type),
                        UpdateDB.BuildDatabaseField("skillLevel", skill.level, FieldType.varchar_type)
                    }
                    Call UpdateDB.InsertRecord(cloneGradeSkillsTable, UpdateDB.BuildOrderedRecord(cloneGradeSkillsTable, DataFields2))
                Next

            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class cloneGrade
    Public Property _key As Long
    Public Property name As String
    Public Property skills As List(Of cloneSkills)

End Class

Public Class cloneSkills
    Public Property level As Integer
    Public Property typeID As Integer
End Class