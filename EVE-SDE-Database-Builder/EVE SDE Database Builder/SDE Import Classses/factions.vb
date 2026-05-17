
Public Class factions
    Inherits SDEFilesBase
    Implements IImporter(Of faction)
    Implements IDatabaseImporter(Of faction)

    Public Const BaseFileName As String = "factions"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, faction) Implements IImporter(Of faction).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of faction)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, faction), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of faction).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("factionID", FieldType.int_type, 0, False, True),
            New DBTableField("factionName", FieldType.varchar_type, 100, True),
            New DBTableField("description", FieldType.varchar_type, 2000, True),
            New DBTableField("shortDescription", FieldType.varchar_type, 500, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("corporationID", FieldType.int_type, 0, True),
            New DBTableField("sizeFactor", FieldType.double_type, 0, True),
            New DBTableField("militiaCorporationID", FieldType.int_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("uniqueName", FieldType.int_type, 0, True),
            New DBTableField("flatLogo", FieldType.varchar_type, 100, True),
            New DBTableField("flatLogoWithName", FieldType.varchar_type, 100, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim MemberRacesTableName As String = "factionsMemberRaces"
        Table = New List(Of DBTableField) From {
            New DBTableField("factionID", FieldType.int_type, 0, False),
            New DBTableField("memberRaceID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(MemberRacesTableName, Table)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("factionID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("factionName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("shortDescription", GetTranslation(.shortDescription, Params.ImportLanguageCode), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", NullableField(.corporationID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("sizeFactor", .sizeFactor, FieldType.double_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("militiaCorporationID", NullableField(.militiaCorporationID), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", BooleanField(.uniqueName), FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("flatLogo", NullableField(.flatLogo), FieldType.nvarchar_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("flatLogoWithName", NullableField(.flatLogoWithName), FieldType.nvarchar_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            For Each MR In DataField.Value.memberRaces
                DataFields = New List(Of DBField) From {
                    UpdateDB.BuildDatabaseField("factionID", DataField.Key, FieldType.int_type),
                    UpdateDB.BuildDatabaseField("memberRaceID", MR, FieldType.int_type)
                }
                Call UpdateDB.InsertRecord(MemberRacesTableName, UpdateDB.BuildOrderedRecord(MemberRacesTableName, DataFields))
            Next

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1
        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class faction
    Public Property _key As Long
    Public Property corporationID As Object
    Public Property description As TranslatedNameField
    Public Property flatLogo As Object
    Public Property flatLogoWithName As Object
    Public Property iconID As Integer
    Public Property memberRaces As List(Of Integer)
    Public Property militiaCorporationID As Object
    Public Property name As TranslatedNameField
    Public Property shortDescription As TranslatedNameField
    Public Property sizeFactor As Double
    Public Property solarSystemID As Integer
    Public Property uniqueName As Boolean

End Class