
Public Class skinLicenses
    Inherits SDEFilesBase
    Implements IImporter(Of skinLicense)
    Implements IDatabaseImporter(Of skinLicense)

    Public Const BaseFileName As String = "skinLicenses"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, skinLicense) Implements IImporter(Of skinLicense).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of skinLicense)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, skinLicense), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of skinLicense).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("skinLicenseID", FieldType.int_type, 0, False, True),
            New DBTableField("duration", FieldType.int_type, 0, False),
            New DBTableField("isSingleUse", FieldType.int_type, 0, True),
            New DBTableField("licenseTypeID", FieldType.int_type, 0, False),
            New DBTableField("skinID", FieldType.int_type, 0, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "skinLicenseID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_SID", IndexFields)

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
                DataFields.Add(UpdateDB.BuildDatabaseField("skinLicenseID", DataField.Key, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("licenseTypeID", .licenseTypeID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("duration", .duration, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("skinID", .skinID, FieldType.int_type))
                DataFields.Add(UpdateDB.BuildDatabaseField("isSingleUse", BooleanField(.isSingleUse), FieldType.int_type))
            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

Public Class skinLicense
    Public Property _key As Long
    Public Property licenseTypeID As Integer
    Public Property duration As Integer
    Public Property isSingleUse As Boolean
    Public Property skinID As Integer
End Class