
Public Class blueprints
    Inherits SDEFilesBase
    Implements IImporter(Of Blueprint)
    Implements IDatabaseImporter(Of Blueprint)

    Public Const BaseFileName As String = "blueprints"

    ' These are hard coded here but can be looked up in ramActivities
    Private Const ActivityManufacturing As Integer = 1
    Private Const ActivityResearchTime As Integer = 3
    Private Const ActivityResearchMaterial As Integer = 4
    Private Const ActivityCopying As Integer = 5
    Private Const ActivityReverseEngineering As Integer = 7
    Private Const ActivityInvention As Integer = 8
    Private Const ActivityReaction As Integer = 11

    Private Const blueprintsBlueprints_Table As String = "blueprints"
    Private Const blueprintsActivities_Table As String = "blueprintsActivities"
    Private Const blueprintsActivityMaterials_Table As String = "blueprintsActivityMaterials"
    Private Const blueprintsActivityProducts_Table As String = "blueprintsActivityProducts"
    Private Const blueprintsActivitySkills_Table As String = "blueprintsActivitySkills"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, Blueprint) Implements IImporter(Of Blueprint).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of blueprint)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, Blueprint), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of Blueprint).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build all the tables to insert blueprint data into. This includes the following tables:
        ' - blueprints
        ' - blueprintsActivities
        ' - blueprintsActivityMaterials
        ' - blueprintsActivityProducts (includes probability for invention)
        ' - blueprintsActivitySkills
        Call BuildBlueprintsTable()
        Call BuildBlueprintsActivitiesTable()
        Call BuildBlueprintsActivityMaterialsTable()
        Call BuildBlueprintsActivityProductsTable()
        Call BuildBlueprintsActivitySkillsTable()

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
                ' Insert blueprint record first
                DataFields.Add(UpdateDB.BuildDatabaseField("blueprintTypeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("maxProductionLimit", .maxProductionLimit, FieldType.int_type))
                Call UpdateDB.InsertRecord(blueprintsBlueprints_Table, UpdateDB.BuildOrderedRecord(blueprintsBlueprints_Table, DataFields))

                ' Now upload activities
                If Not IsNothing(.activities.copying) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityCopying, .activities.copying.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.copying.materials, ActivityCopying)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.copying.skills, ActivityCopying)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.copying.products, ActivityCopying)
                End If

                If Not IsNothing(.activities.invention) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityInvention, .activities.invention.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.invention.materials, ActivityInvention)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.invention.skills, ActivityInvention)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.invention.products, ActivityInvention)
                End If

                If Not IsNothing(.activities.manufacturing) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityManufacturing, .activities.manufacturing.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.manufacturing.materials, ActivityManufacturing)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.manufacturing.skills, ActivityManufacturing)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.manufacturing.products, ActivityManufacturing)
                End If

                If Not IsNothing(.activities.research_material) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityResearchMaterial, .activities.research_material.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.research_material.materials, ActivityResearchMaterial)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.research_material.skills, ActivityResearchMaterial)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.research_material.products, ActivityResearchMaterial)
                End If

                If Not IsNothing(.activities.research_time) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityResearchTime, .activities.research_time.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.research_time.materials, ActivityResearchTime)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.research_time.skills, ActivityResearchTime)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.research_time.products, ActivityResearchTime)
                End If

                If Not IsNothing(.activities.reaction) Then
                    Call InsertBlueprintActivity(.blueprintTypeID, ActivityReaction, .activities.reaction.time)
                    Call InsertBlueprintMaterials(.blueprintTypeID, .activities.reaction.materials, ActivityReaction)
                    Call InsertBlueprintSkills(.blueprintTypeID, .activities.reaction.skills, ActivityReaction)
                    Call InsertBlueprintProducts(.blueprintTypeID, .activities.reaction.products, ActivityReaction)
                End If
            End With

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Sub InsertBlueprintActivity(ByVal BPID As Integer, ByVal ActivityID As Integer, ByVal ActivityTime As Integer)
        Dim DataFields As New List(Of DBField) From {
            UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type),
            UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.int_type),
            UpdateDB.BuildDatabaseField("time", ActivityTime, FieldType.int_type)
        }

        Call UpdateDB.InsertRecord(blueprintsActivities_Table, UpdateDB.BuildOrderedRecord(blueprintsActivities_Table, DataFields))

    End Sub

    Private Sub InsertBlueprintMaterials(ByVal BPID As Integer, ByVal Materials As List(Of blueprint.Material), ByVal ActivityID As Integer)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(Materials) Then
            For Each mat In Materials
                ' Insert material record into blueprintsActivityMaterials
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("materialTypeID", mat.typeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("quantity", mat.quantity, FieldType.int_type)
                }

                Call UpdateDB.InsertRecord(blueprintsActivityMaterials_Table, UpdateDB.BuildOrderedRecord(blueprintsActivityMaterials_Table, DataFields))
            Next
        End If

    End Sub

    Private Sub InsertBlueprintSkills(ByVal BPID As Integer, ByVal Skills As List(Of blueprint.Skill), ByVal ActivityID As Integer)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(Skills) Then
            For Each Skill In Skills
                ' Insert material record into blueprintsActivityMaterials
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("skillID", Skill.typeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("level", Skill.level, FieldType.int_type)
                }

                Call UpdateDB.InsertRecord(blueprintsActivitySkills_Table, UpdateDB.BuildOrderedRecord(blueprintsActivitySkills_Table, DataFields))
            Next
        End If

    End Sub

    Private Sub InsertBlueprintProducts(ByVal BPID As Integer, ByVal Products As List(Of blueprint.Product), ByVal ActivityID As Integer)
        Dim DataFields As List(Of DBField)

        If Not IsNothing(Products) Then
            For Each Product In Products
                ' Insert material record into blueprintsActivityMaterials
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("blueprintTypeID", BPID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("activityID", ActivityID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("productTypeID", Product.typeID, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("quantity", Product.quantity, FieldType.int_type)
                }
                If IsNothing(Product.probability) Then
                    ' Set it to 1 for 100% probability
                    Product.probability = 1
                End If
                DataFields.Add(UpdateDB.BuildDatabaseField("probability", NullableField(Product.probability), FieldType.double_type))

                Call UpdateDB.InsertRecord(blueprintsActivityProducts_Table, UpdateDB.BuildOrderedRecord(blueprintsActivityProducts_Table, DataFields))
            Next
        End If

    End Sub

    Private Sub BuildBlueprintsTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("blueprintTypeID", FieldType.int_type, 0, False, True),
            New DBTableField("maxProductionLimit", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(blueprintsBlueprints_Table, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "blueprintTypeID"
        }
        Call UpdateDB.CreateIndex(blueprintsBlueprints_Table, "IDX_" & blueprintsBlueprints_Table & "_BPID", IndexFields)

    End Sub

    Private Sub BuildBlueprintsActivitiesTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("blueprintTypeID", FieldType.int_type, 0, False, True),
            New DBTableField("activityID", FieldType.int_type, 0, False, True),
            New DBTableField("time", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(blueprintsActivities_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "activityID"
        }
        Call UpdateDB.CreateIndex(blueprintsActivities_Table, "IDX_" & blueprintsActivities_Table & "_AID", IndexFields)

    End Sub

    Private Sub BuildBlueprintsActivityMaterialsTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("blueprintTypeID", FieldType.int_type, 0, True),
            New DBTableField("activityID", FieldType.int_type, 0, True),
            New DBTableField("materialTypeID", FieldType.int_type, 0, True),
            New DBTableField("quantity", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(blueprintsActivityMaterials_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "blueprintTypeID",
            "activityID"
        }
        Call UpdateDB.CreateIndex(blueprintsActivityMaterials_Table, "IDX_" & blueprintsActivityMaterials_Table & "_TID_AID", IndexFields)

    End Sub

    Private Sub BuildBlueprintsActivitySkillsTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("blueprintTypeID", FieldType.int_type, 0, True),
            New DBTableField("activityID", FieldType.int_type, 0, True),
            New DBTableField("skillID", FieldType.int_type, 0, True),
            New DBTableField("level", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(blueprintsActivitySkills_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "blueprintTypeID",
            "activityID"
        }
        Call UpdateDB.CreateIndex(blueprintsActivitySkills_Table, "IDX_" & blueprintsActivitySkills_Table & "_TID_AID", IndexFields)

    End Sub

    Private Sub BuildBlueprintsActivityProductsTable()
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("blueprintTypeID", FieldType.int_type, 0, True),
            New DBTableField("activityID", FieldType.int_type, 0, True),
            New DBTableField("productTypeID", FieldType.int_type, 0, True),
            New DBTableField("quantity", FieldType.int_type, 0, True),
            New DBTableField("probability", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(blueprintsActivityProducts_Table, Table)

        ' Create index
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "blueprintTypeID",
            "activityID"
        }
        Call UpdateDB.CreateIndex(blueprintsActivityProducts_Table, "IDX_" & blueprintsActivityProducts_Table & "_TID_AID", IndexFields)

        IndexFields = New List(Of String) From {
            "productTypeID"
        }
        Call UpdateDB.CreateIndex(blueprintsActivityProducts_Table, "IDX_" & blueprintsActivityProducts_Table & "_PTID", IndexFields)

    End Sub
End Class

' Class to parse the blueprints file
Public Class blueprint
    Public Property _key As Long
    Public Property activities As BlueprintActivities
    Public Property blueprintTypeID As Integer
    Public Property maxProductionLimit As Integer

    Public Class BlueprintActivities
        Public Property copying As CopyingActivity
        Public Property invention As InventionActivity
        Public Property manufacturing As ManufacturingActivity
        Public Property research_material As ResearchMaterialActivity
        Public Property research_time As ResearchTimeActivity
        Public Property reaction As ReactionActivity
    End Class

    Public Class CopyingActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Integer
    End Class

    Public Class InventionActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Integer
    End Class

    Public Class ManufacturingActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Integer
    End Class

    Public Class ResearchMaterialActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Integer
    End Class

    Public Class ResearchTimeActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Integer
    End Class

    Public Class ReactionActivity
        Public Property materials As List(Of Material)
        Public Property products As List(Of Product)
        Public Property skills As List(Of Skill)
        Public Property time As Integer
    End Class

    Public Class Material
        Public Property quantity As Integer
        Public Property typeID As Integer
    End Class

    Public Class Skill
        Public Property level As Integer
        Public Property typeID As Integer
    End Class

    Public Class Product
        Public Property probability As Object
        Public Property quantity As Integer
        Public Property typeID As Integer
    End Class

End Class