
Public Class mapStars
    Inherits SDEFilesBase
    Implements IImporter(Of star)
    Implements IDatabaseImporter(Of star)

    Public Const BaseFileName As String = "mapStars"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, star) Implements IImporter(Of star).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of star)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, star), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of star).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("starID", FieldType.int_type, 0, False, True),
            New DBTableField("starName", FieldType.nvarchar_type, 250, True), ' Set later
            New DBTableField("radius", FieldType.double_type, 0, True),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("age", FieldType.double_type, 0, True),
            New DBTableField("life", FieldType.double_type, 0, True),
            New DBTableField("luminosity", FieldType.double_type, 0, True),
            New DBTableField("spectralClass", FieldType.varchar_type, 10, True),
            New DBTableField("temperature", FieldType.double_type, 0, True),
            New DBTableField("typeID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
                "starID"
            }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("starID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("radius", .radius, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("typeID", .typeID, FieldType.int_type))

                With .statistics
                    DataFields.Add(UpdateDB.BuildDatabaseField("age", .age, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("life", .life, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("luminosity", .luminosity, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("spectralClass", .spectralClass, FieldType.nvarchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("temperature", .temperature, FieldType.double_type))
                End With
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class star
    Public Property _key As Long
    Public Property radius As Long
    Public Property solarSystemID As Integer
    Public Property statistics As starStatistic
    Public Property typeID As Integer
End Class

Public Class starStatistic
    Public Property age As Double
    Public Property life As Double
    Public Property luminosity As Double
    Public Property spectralClass As String
    Public Property temperature As Double

End Class

