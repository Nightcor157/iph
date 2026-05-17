
Public Class types
    Inherits SDEFilesBase
    Implements IImporter(Of typeID)
    Implements IDatabaseImporter(Of typeID)

    Public Const BaseFileName As String = "types"

    Private ReadOnly PackagedItems As List(Of PackagedItem)
    Private PackagedItemtoFind As PackagedItem
    Private ReadOnly PackagedGroups As List(Of PackagedGroup)
    Private PackagedGrouptoFind As PackagedGroup

    Private Class PackagedItem
        Public TypeID As Integer
        Public PackagedVolume As Double
    End Class

    Private Class PackagedGroup
        Public GroupID As Integer
        Public PackagedVolume As Double
    End Class

    ' Predicate for finding a packaged category record
    Private Function FindPackagedVolumebyTypeID(ByVal Item As PackagedItem) As Boolean
        If Item.TypeID = PackagedItemtoFind.TypeID Then
            Return True
        Else
            Return False
        End If
    End Function

    ' Predicate for finding a packaged category record
    Private Function FindPackagedVolumebyGroup(ByVal Item As PackagedGroup) As Boolean
        If Item.GroupID = PackagedGrouptoFind.GroupID Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)

        ' Load up the packaged groups for ships
        PackagedGroups = New List(Of PackagedGroup) From {
            New PackagedGroup With {.GroupID = 25, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 26, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 27, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 28, .PackagedVolume = 20000},
            New PackagedGroup With {.GroupID = 29, .PackagedVolume = 500},
            New PackagedGroup With {.GroupID = 30, .PackagedVolume = 10000000},
            New PackagedGroup With {.GroupID = 31, .PackagedVolume = 500},
            New PackagedGroup With {.GroupID = 237, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 324, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 358, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 380, .PackagedVolume = 20000},
            New PackagedGroup With {.GroupID = 381, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 419, .PackagedVolume = 15000},
            New PackagedGroup With {.GroupID = 420, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 463, .PackagedVolume = 3750},
            New PackagedGroup With {.GroupID = 485, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 513, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 540, .PackagedVolume = 15000},
            New PackagedGroup With {.GroupID = 541, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 543, .PackagedVolume = 3750},
            New PackagedGroup With {.GroupID = 547, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 659, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 830, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 831, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 832, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 833, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 834, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 883, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 893, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 894, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 898, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 900, .PackagedVolume = 50000},
            New PackagedGroup With {.GroupID = 902, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 906, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 941, .PackagedVolume = 500000},
            New PackagedGroup With {.GroupID = 963, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 1022, .PackagedVolume = 500},
            New PackagedGroup With {.GroupID = 1201, .PackagedVolume = 15000},
            New PackagedGroup With {.GroupID = 1202, .PackagedVolume = 20000},
            New PackagedGroup With {.GroupID = 1283, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 1305, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 1527, .PackagedVolume = 2500},
            New PackagedGroup With {.GroupID = 1534, .PackagedVolume = 5000},
            New PackagedGroup With {.GroupID = 1538, .PackagedVolume = 1300000},
            New PackagedGroup With {.GroupID = 1972, .PackagedVolume = 10000},
            New PackagedGroup With {.GroupID = 2001, .PackagedVolume = 2500}
        }

        ' Now add the container data
        PackagedItems = New List(Of PackagedItem) From {
            New PackagedItem With {.TypeID = 3293, .PackagedVolume = 33},
            New PackagedItem With {.TypeID = 3296, .PackagedVolume = 65},
            New PackagedItem With {.TypeID = 3297, .PackagedVolume = 10},
            New PackagedItem With {.TypeID = 3465, .PackagedVolume = 65},
            New PackagedItem With {.TypeID = 3466, .PackagedVolume = 33},
            New PackagedItem With {.TypeID = 3467, .PackagedVolume = 10},
            New PackagedItem With {.TypeID = 11488, .PackagedVolume = 150},
            New PackagedItem With {.TypeID = 11489, .PackagedVolume = 300},
            New PackagedItem With {.TypeID = 17363, .PackagedVolume = 10},
            New PackagedItem With {.TypeID = 17364, .PackagedVolume = 33},
            New PackagedItem With {.TypeID = 17365, .PackagedVolume = 65},
            New PackagedItem With {.TypeID = 17366, .PackagedVolume = 10000},
            New PackagedItem With {.TypeID = 17367, .PackagedVolume = 50000},
            New PackagedItem With {.TypeID = 17368, .PackagedVolume = 100000},
            New PackagedItem With {.TypeID = 24445, .PackagedVolume = 1200},
            New PackagedItem With {.TypeID = 33003, .PackagedVolume = 2500},
            New PackagedItem With {.TypeID = 33005, .PackagedVolume = 5000},
            New PackagedItem With {.TypeID = 33007, .PackagedVolume = 1000},
            New PackagedItem With {.TypeID = 33009, .PackagedVolume = 500},
            New PackagedItem With {.TypeID = 33011, .PackagedVolume = 100}
        }
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, typeID) Implements IImporter(Of typeID).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of typeID)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, typeID), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of typeID).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False, True),
            New DBTableField("groupID", FieldType.int_type, 0, True),
            New DBTableField("typeName", FieldType.nvarchar_type, 500, True),
            New DBTableField("description", FieldType.text_type, MaxFieldLen, True),
            New DBTableField("basePrice", FieldType.double_type, 0, True),
            New DBTableField("capacity", FieldType.double_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("graphicID", FieldType.int_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("marketGroupID", FieldType.int_type, 0, True),
            New DBTableField("mass", FieldType.double_type, 0, True),
            New DBTableField("metaGroupID", FieldType.int_type, 0, True),
            New DBTableField("metaLevel", FieldType.int_type, 0, True),
            New DBTableField("packagedVolume", FieldType.double_type, 0, True),
            New DBTableField("portionSize", FieldType.int_type, 0, True),
            New DBTableField("published", FieldType.int_type, 0, True),
            New DBTableField("raceID", FieldType.int_type, 0, True),
            New DBTableField("radius", FieldType.double_type, 0, True),
            New DBTableField("soundID", FieldType.int_type, 0, True),
            New DBTableField("variationparentTypeID", FieldType.int_type, 0, True),
            New DBTableField("volume", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        ' Create indexes
        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_TID", IndexFields)

        IndexFields = New List(Of String) From {
            "groupID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_GID", IndexFields)

        IndexFields = New List(Of String) From {
            "marketGroupID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_MGID", IndexFields)

        ' See if we only want to build the table and indexes
        If Not Params.InsertRecords Then
            Exit Sub
        End If

        Dim TotalRecords As Long = Records.Count

        ' Start processing
        Call InitGridRow(Params.RowLocation)

        ' Process Data
        For Each DataField In Records
            DataFields = New List(Of DBField)

            If CancelImport Then Exit Sub

            With DataField.Value
                ' Build the insert list
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("basePrice", .basePrice, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("capacity", .capacity, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("graphicID", .graphicID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("groupID", .groupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("marketGroupID", .marketGroupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("mass", .mass, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("metaGroupID", .metaGroupID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("metaLevel", .metaLevel, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("packagedVolume", GetPackagedVolume(DataField.Key, .groupID, .volume), FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("portionSize", .portionSize, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("published", BooleanField(.published), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("soundID", .soundID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("variationparentTypeID", .variationParentTypeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("volume", .volume, FieldType.double_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub

    Private Function GetPackagedVolume(ByVal TypeID As Integer, ByVal GroupID As Integer, ByVal DefaultVolume As Object) As Object
        Dim FoundItem As PackagedItem = Nothing
        Dim FoundGroup As New PackagedGroup

        If IsNothing(DefaultVolume) Then
            Return Nothing
        End If

        ' Look up typeID first
        PackagedItemtoFind = New PackagedItem With {.TypeID = TypeID, .PackagedVolume = 0}
        FoundItem = PackagedItems.Find(AddressOf FindPackagedVolumebyTypeID)

        If FoundItem IsNot Nothing Then
            Return FoundItem.PackagedVolume
        Else
            ' Look up group
            PackagedGrouptoFind = New PackagedGroup With {.GroupID = GroupID, .PackagedVolume = 0}
            FoundGroup = PackagedGroups.Find(AddressOf FindPackagedVolumebyGroup)

            If FoundGroup IsNot Nothing Then
                Return FoundGroup.PackagedVolume
            Else
                Return DefaultVolume
            End If
        End If

    End Function
End Class

Public Class typeID
    Public Property _key As Long
    Public Property basePrice As Double?
    Public Property capacity As Double?
    Public Property description As TranslatedNameField
    Public Property factionID As Integer?
    Public Property graphicID As Integer?
    Public Property groupID As Integer
    Public Property iconID As Integer?
    Public Property marketGroupID As Integer?
    Public Property mass As Double?
    Public Property metaGroupID As Integer?
    Public Property metaLevel As Integer?
    Public Property name As TranslatedNameField
    Public Property portionSize As Integer
    Public Property published As Boolean
    Public Property raceID As Integer?
    Public Property radius As Double?
    Public Property soundID As Integer?
    Public Property variationParentTypeID As Integer?
    Public Property volume As Double?
End Class



