
Public Class typeDogma
    Inherits SDEFilesBase
    Implements IImporter(Of dogmatype)
    Implements IDatabaseImporter(Of dogmatype)

    Public Const BaseFileName As String = "typeDogma"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, dogmatype) Implements IImporter(Of dogmatype).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of dogmatype)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, dogmatype), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of dogmatype).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        TableName = "typeDogmaAttributes"
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False, True),
            New DBTableField("attributeID", FieldType.smallint_type, 0, False, True),
            New DBTableField("value", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim IndexFields As List(Of String)
        IndexFields = New List(Of String) From {
            "typeID"
        }
        Call UpdateDB.CreateIndex(TableName, "IDX_" & TableName & "_CID", IndexFields)

        ' Build table
        TableName = "typeDogmaEffects"
        Table = New List(Of DBTableField) From {
            New DBTableField("typeID", FieldType.int_type, 0, False, True),
            New DBTableField("effectID", FieldType.smallint_type, 0, False, True),
            New DBTableField("isDefault", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        IndexFields = New List(Of String) From {
            "typeID"
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

            If CancelImport Then Exit Sub

            With DataField.Value
                ' For each record, insert the attribute values and the effect values
                If Not IsNothing(.dogmaAttributes) Then
                    For Each attribute In .dogmaAttributes
                        DataFields = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("attributeID", attribute.attributeID, FieldType.smallint_type),
                            UpdateDB.BuildDatabaseField("value", attribute.value, FieldType.double_type)
                        }
                        Call UpdateDB.InsertRecord("typeDogmaAttributes", UpdateDB.BuildOrderedRecord("typeDogmaAttributes", DataFields))
                    Next
                End If

                If Not IsNothing(.dogmaEffects) Then
                    For Each effect In .dogmaEffects
                        DataFields = New List(Of DBField) From {
                            UpdateDB.BuildDatabaseField("typeID", DataField.Key, FieldType.int_type),
                            UpdateDB.BuildDatabaseField("effectID", effect.effectID, FieldType.smallint_type),
                            UpdateDB.BuildDatabaseField("isDefault", BooleanField(effect.isDefault), FieldType.int_type)
                        }
                        Call UpdateDB.InsertRecord("typeDogmaEffects", UpdateDB.BuildOrderedRecord("typeDogmaEffects", DataFields))
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

Public Class dogmatype
    Public Property _key As Long
    Public Property dogmaAttributes As List(Of dogmatypeAttribute)
    Public Property dogmaEffects As List(Of dogmatypeEffect)
End Class

Public Class dogmatypeAttribute
    Public Property attributeID As Integer
    Public Property value As Double
End Class

Public Class dogmatypeEffect
    Public Property effectID As Integer
    Public Property isDefault As Boolean
End Class