
Public Class npcCorporations
    Inherits SDEFilesBase
    Implements IImporter(Of npcCorporation)
    Implements IDatabaseImporter(Of npcCorporation)

    Public Const BaseFileName As String = "npcCorporations"

    Public Sub New(ByVal FileName As String, ByVal FilePath As String, ImportDatabase As Object)
        MyBase.New(FileName, FilePath, ImportDatabase)
    End Sub

    ''' <summary>
    ''' Imports the sde file into the database set in the constructor
    ''' </summary>
    Public Function ImportFile() As Dictionary(Of Long, npcCorporation) Implements IImporter(Of npcCorporation).ImportFile

        FileNameErrorTracker = BaseFileName

        Return ImportJsonlFile(Of npcCorporation)(SDEFilePath, Function(c) c._key)

    End Function

    ''' <summary>
    ''' Imports the SDE file into the database set in the constructor
    ''' </summary>
    ''' <param name="Params">What the row location is and whether to insert the data or not (for bulk import)</param>
    Public Sub InsertDatatoDatabase(Records As Dictionary(Of Long, npcCorporation), ByVal Params As ImportParameters) _
        Implements IDatabaseImporter(Of npcCorporation).InsertDatatoDatabase
        FileNameErrorTracker = BaseFileName
        If Records.Count = 0 And Params.InsertRecords Then Exit Sub

        Dim DataFields As List(Of DBField)
        Dim Count As Long = 0

        ' Build table
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False, True),
            New DBTableField("corporationName", FieldType.nvarchar_type, 200, False),
            New DBTableField("description", FieldType.nvarchar_type, 1500, True),
            New DBTableField("ceoID", FieldType.int_type, 0, True),
            New DBTableField("enemyID", FieldType.int_type, 0, True),
            New DBTableField("factionID", FieldType.int_type, 0, True),
            New DBTableField("friendID", FieldType.int_type, 0, True),
            New DBTableField("iconID", FieldType.int_type, 0, True),
            New DBTableField("initialPrice", FieldType.int_type, 0, False),
            New DBTableField("mainActivityID", FieldType.int_type, 0, True),
            New DBTableField("memberLimit", FieldType.int_type, 0, False),
            New DBTableField("minimumJoinStanding", FieldType.int_type, 0, False),
            New DBTableField("raceID", FieldType.int_type, 0, True),
            New DBTableField("secondaryActivityID", FieldType.int_type, 0, True),
            New DBTableField("shares", FieldType.double_type, 0, False),
            New DBTableField("solarSystemID", FieldType.int_type, 0, True),
            New DBTableField("stationID", FieldType.int_type, 0, True),
            New DBTableField("deleted", FieldType.int_type, 0, False),
            New DBTableField("hasPlayerPersonnelManager", FieldType.int_type, 0, False),
            New DBTableField("sendCharTerminationMessage", FieldType.int_type, 0, False),
            New DBTableField("uniqueName", FieldType.int_type, 0, False),
            New DBTableField("extent", FieldType.nvarchar_type, 10, False),
            New DBTableField("size", FieldType.nvarchar_type, 10, False),
            New DBTableField("minSecurity", FieldType.double_type, 0, False),
            New DBTableField("sizeFactor", FieldType.double_type, 0, False),
            New DBTableField("taxRate", FieldType.double_type, 0, False),
            New DBTableField("tickerName", FieldType.nvarchar_type, 50, False)
        }

        Call UpdateDB.CreateTable(TableName, Table)

        Dim CorporationAllowedMemberRacesTableName As String = TableName & "AllowedMemberRaces"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("memberRace", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationAllowedMemberRacesTableName, Table)

        Dim CorporationTradesTableName As String = TableName & "Trades"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("typeID", FieldType.int_type, 0, True),
            New DBTableField("value", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationTradesTableName, Table)

        Dim CorporationDivisionsTableName As String = TableName & "Divisions"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("divisionID", FieldType.int_type, 0, False),
            New DBTableField("divisionNumber", FieldType.int_type, 0, True),
            New DBTableField("leaderID", FieldType.int_type, 0, True),
            New DBTableField("size", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationDivisionsTableName, Table)

        Dim CorporationExchangeRatesTableName As String = TableName & "ExchangeRates"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("exchangeID", FieldType.int_type, 0, True),
            New DBTableField("exchangeRate", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationExchangeRatesTableName, Table)

        Dim CorporationInvestorsTableName As String = TableName & "Investors"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("investorID", FieldType.int_type, 0, True),
            New DBTableField("shares", FieldType.double_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationInvestorsTableName, Table)

        Dim CorporationLPOffersTableName As String = TableName & "LPOffers"
        Table = New List(Of DBTableField) From {
            New DBTableField("corporationID", FieldType.int_type, 0, False),
            New DBTableField("lpOfferTableID", FieldType.int_type, 0, True)
        }

        Call UpdateDB.CreateTable(CorporationLPOffersTableName, Table)

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
                With DataField.Value
                    DataFields.Add(UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("ceoID", .ceoID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("enemyID", .enemyID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("factionID", .factionID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("friendID", .friendID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("iconID", .iconID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("initialPrice", .initialPrice, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("mainActivityID", .mainActivityID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("memberLimit", .memberLimit, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("minimumJoinStanding", .minimumJoinStanding, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("raceID", .raceID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("secondaryActivityID", .secondaryActivityID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("shares", .shares, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("solarSystemID", .solarSystemID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("stationID", .stationID, FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("deleted", BooleanField(.deleted), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("hasPlayerPersonnelManager", BooleanField(.hasPlayerPersonnelManager), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("sendCharTerminationMessage", BooleanField(.sendCharTerminationMessage), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("uniqueName", BooleanField(.uniqueName), FieldType.int_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("extent", .extent, FieldType.nvarchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("size", .size, FieldType.nvarchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("minSecurity", .minSecurity, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("sizeFactor", .sizeFactor, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("taxRate", .taxRate, FieldType.double_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("tickerName", .tickerName, FieldType.nvarchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("description", GetTranslation(.description, Params.ImportLanguageCode), FieldType.nvarchar_type))
                    DataFields.Add(UpdateDB.BuildDatabaseField("corporationName", GetTranslation(.name, Params.ImportLanguageCode), FieldType.nvarchar_type))
                End With

            End With

            Call UpdateDB.InsertRecord(TableName, UpdateDB.BuildOrderedRecord(TableName, DataFields))

            ' Add other tables now
            If Not IsNothing(DataField.Value.allowedMemberRaces) Then
                For Each AMR In DataField.Value.allowedMemberRaces
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("memberRace", AMR, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationAllowedMemberRacesTableName, UpdateDB.BuildOrderedRecord(CorporationAllowedMemberRacesTableName, DataFields))
                Next
            End If

            If Not IsNothing(DataField.Value.corporationTrades) Then
                For Each CT In DataField.Value.corporationTrades
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("typeID", CT._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("value", CT._value, FieldType.double_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationTradesTableName, UpdateDB.BuildOrderedRecord(CorporationTradesTableName, DataFields))
                Next
            End If

            If Not IsNothing(DataField.Value.divisions) Then
                For Each CD In DataField.Value.divisions
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("divisionID", CD._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("divisionNumber", CD.divisionNumber, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("leaderID", CD.leaderID, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("size", CD.size, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationDivisionsTableName, UpdateDB.BuildOrderedRecord(CorporationDivisionsTableName, DataFields))
                Next
            End If

            If Not IsNothing(DataField.Value.exchangeRates) Then
                For Each ER In DataField.Value.exchangeRates
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("exchangeID", ER._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("exchangeRate", ER._value, FieldType.double_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationExchangeRatesTableName, UpdateDB.BuildOrderedRecord(CorporationExchangeRatesTableName, DataFields))
                Next
            End If

            If Not IsNothing(DataField.Value.investors) Then
                For Each CI In DataField.Value.investors
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("investorID", CI._key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("shares", CI._value, FieldType.double_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationInvestorsTableName, UpdateDB.BuildOrderedRecord(CorporationInvestorsTableName, DataFields))
                Next
            End If

            If Not IsNothing(DataField.Value.lpOfferTables) Then
                For Each LPO In DataField.Value.lpOfferTables
                    DataFields = New List(Of DBField) From {
                        UpdateDB.BuildDatabaseField("corporationID", DataField.Key, FieldType.int_type),
                        UpdateDB.BuildDatabaseField("lpOfferTableID", LPO, FieldType.int_type)
                    }
                    Call UpdateDB.InsertRecord(CorporationLPOffersTableName, UpdateDB.BuildOrderedRecord(CorporationLPOffersTableName, DataFields))
                Next
            End If

            ' Update grid progress
            Call UpdateGridRowProgress(Params.RowLocation, Count, TotalRecords)
            Count += 1

        Next

        Call FinalizeGridRow(Params.RowLocation)

    End Sub
End Class

' Root object
Public Class npcCorporation
    Public Property _key As Long
    Public Property allowedMemberRaces As List(Of Integer)
    Public Property ceoID As Integer
    Public Property corporationTrades As List(Of CorporationTrade)
    Public Property deleted As Boolean
    Public Property description As TranslatedNameField
    Public Property divisions As List(Of CorpDivision)
    Public Property enemyID As Integer?
    Public Property exchangeRates As List(Of ExchangeRate)
    Public Property extent As String   ' enum: L, G, R, N, C
    Public Property factionID As Integer?
    Public Property friendID As Integer?
    Public Property hasPlayerPersonnelManager As Boolean
    Public Property iconID As Integer?
    Public Property initialPrice As Integer
    Public Property investors As List(Of Investor)
    Public Property lpOfferTables As List(Of Integer)
    Public Property mainActivityID As Integer?
    Public Property memberLimit As Integer
    Public Property minSecurity As Double
    Public Property minimumJoinStanding As Integer
    Public Property name As TranslatedNameField
    Public Property raceID As Integer?
    Public Property secondaryActivityID As Integer?
    Public Property sendCharTerminationMessage As Boolean
    Public Property shares As Long
    Public Property size As String     ' enum: T, H, M, L, S
    Public Property sizeFactor As Double
    Public Property solarSystemID As Integer?
    Public Property stationID As Integer?
    Public Property taxRate As Double
    Public Property tickerName As String
    Public Property uniqueName As Boolean
End Class

Public Class CorpDivision
    Public Property _key As Integer
    Public Property divisionNumber As Integer
    Public Property leaderID As Integer
    Public Property size As Integer
End Class

Public Class CorporationTrade
    Public Property _key As Integer
    Public Property _value As Double
End Class

Public Class ExchangeRate
    Public Property _key As Integer
    Public Property _value As Double
End Class

Public Class Investor
    Public Property _key As Integer
    Public Property _value As Integer
End Class



