
Imports System.Data.SQLite
Imports System.IO
Imports Mysqlx.XDevAPI.Relational

''' <summary>
''' Class to create a SQLite Database and insert data into it.
''' </summary>
Public Class SQLiteDB
    Inherits DBFilesBase

    Public DB As New SQLiteConnection ' keep open for all updates
    Private ReadOnly DBFileNameandPath As String ' For later use if needed

    ''' <summary>
    ''' Constructor class for a SQLite database. Class builds a new database file, 
    ''' or replaces the existing file and inserts all data into it.
    ''' </summary>
    ''' <param name="DatabaseFileNameandPath">Name and path of the database file to create.</param>
    ''' <param name="DBPath">Path where the database file will be</param>
    ''' <param name="Success">True if the database successfully created.</param>
    Public Sub New(ByVal DatabaseFileNameandPath As String, ByVal DBPath As String, ByRef Success As Boolean)
        MyBase.New(DatabaseFileNameandPath, DatabaseType.SQLite)

        Call InitalizeMainProgressBar(0, "Initializing Database..")

        Try

            ' Make sure the folder is there
            If Not Directory.Exists(DBPath) Then
                Directory.CreateDirectory(DBPath)
            End If

            ' Build a new database if it doesn't exist
            If Not File.Exists(DatabaseFileNameandPath) Then
                ' Create new DB
                SQLiteConnection.CreateFile(DatabaseFileNameandPath)
            End If

            ' Open connection
            DB = New SQLiteConnection With {
                .ConnectionString = "Data Source=" & DatabaseFileNameandPath & ";Version=3;"
            }
            DB.Open()

            ' Set DB settings
            Call ExecuteNonQuerySQL("PRAGMA synchronous = OFF; PRAGMA locking_mode = NORMAL; PRAGMA cache_size = 10000")
            Call ExecuteNonQuerySQL("PRAGMA page_size = 4096; PRAGMA temp_store = DEFAULT; PRAGMA journal_mode = TRUNCATE; PRAGMA count_changes = OFF")
            Call ExecuteNonQuerySQL("PRAGMA auto_vacuum = FULL;")

            DBFileNameandPath = DatabaseFileNameandPath

            Success = True

        Catch ex As Exception
            Call CloseDB()
            Call ShowErrorMessage(ex)
            Success = False
        End Try

    End Sub

    ''' <summary>
    ''' Closes and disposes the local database variable.
    ''' </summary>
    Public Sub CloseDB()
        On Error Resume Next
        DB.Close()
        DB.Dispose()
        DB = Nothing
        On Error GoTo 0
    End Sub

    ''' <summary>
    ''' Executes the sent SQL on the main database for the class.
    ''' </summary>
    ''' <param name="SQL">SQL query to execute.</param>
    Public Sub ExecuteNonQuerySQL(ByVal SQL As String)
        Dim DBExecuteCmd As SQLiteCommand = DB.CreateCommand
        DBExecuteCmd.CommandText = SQL
        DBExecuteCmd.ExecuteNonQuery()
        DBExecuteCmd.Dispose()
    End Sub

    ''' <summary>
    ''' Begins an SQL transaction on the server
    ''' </summary>
    Public Sub BeginSQLiteTransaction()
        Call ExecuteNonQuerySQL("BEGIN;")
    End Sub

    ''' <summary>
    ''' Commits an SQL transaction on the server
    ''' </summary>
    Public Sub CommitSQLiteTransaction()
        Call ExecuteNonQuerySQL("END;")
    End Sub

    ''' <summary>
    ''' Rollsback an SQL transaction on the server
    ''' </summary>
    Public Sub RollbackSQLiteTransaction()
        Call ExecuteNonQuerySQL("ROLLBACK;")
    End Sub

    ''' <summary>
    ''' Runs a single select value for the table and where values sent.
    ''' </summary>
    ''' <param name="SelectFieldValues">Fields to select - i.e. SELECT X FROM, field is 'X'</param>
    ''' <param name="SelectTableName">Table name to select from</param>
    ''' <param name="SelectWhereClause">List of where clauses such as WHERE Y = 3</param>
    ''' <returns>List of objects to match the field values</returns>
    Public Function SelectfromTable(ByVal SelectFieldValues As List(Of String), ByVal SelectTableName As String,
                                    Optional ByVal SelectWhereClause As List(Of String) = Nothing,
                                    Optional ByRef RecordsFound As Boolean = False) As List(Of List(Of Object))
        Dim SQLQuery As SQLiteCommand
        Dim SQLReader As SQLiteDataReader
        Dim ReturnValues As New List(Of List(Of Object))
        Dim SelectClause As String = ""
        Dim SelectFieldCount As Integer = SelectFieldValues.Count
        Dim TempRecord As List(Of Object)

        ' Build select query
        For i = 0 To SelectFieldValues.Count - 1
            SelectClause &= SelectFieldValues(i) & ","
        Next

        ' Remove last comma
        SelectClause = SelectClause.Substring(0, Len(SelectClause) - 1)

        Dim SQL As String = String.Format("SELECT {0} FROM {1}", SelectClause, SelectTableName)

        ' Add the WHERE clauses
        If Not IsNothing(SelectWhereClause) Then
            If SelectWhereClause.Count > 0 Then
                SQL &= " WHERE "
                For i = 0 To SelectWhereClause.Count - 1
                    SQL &= String.Format("{0} AND ", SelectWhereClause(i))
                Next

                ' Strip last AND
                SQL = SQL.Substring(0, Len(SQL) - 5)
            End If
        End If

        ' See if the query returns data
        SQLQuery = New SQLiteCommand(SQL, DB)
        SQLReader = SQLQuery.ExecuteReader

        While SQLReader.Read()
            TempRecord = New List(Of Object)
            For i = 0 To SelectFieldCount - 1
                TempRecord.Add(SQLReader.GetValue(i))
            Next
            ReturnValues.Add(TempRecord)
            RecordsFound = True
        End While

        SQLReader.Close()
        SQLQuery.Dispose()

        Return ReturnValues

    End Function

    ''' <summary>
    ''' Drops the tablename sent, if it exists on the database.
    ''' </summary>
    ''' <param name="TableName">Table you want to drop</param>
    Private Sub DropTable(TableName As String)
        Dim SelectValues As New List(Of String) From {"name"}
        Dim WhereValues As New List(Of String) From {
            "type='table'",
            "name='" & TableName & "'"
        }
        Dim ReturnValue As Object = SelectfromTable(SelectValues, "sqlite_master", WhereValues)

        If ReturnValue.count = 0 Then
            ReturnValue = False
        Else
            ReturnValue = True
        End If

        ' See if the table exists and drop if it does
        If ReturnValue Then
            Call ExecuteNonQuerySQL("DROP TABLE " & TableName)
        End If

    End Sub

    ''' <summary>
    ''' Creates a table on the server for the sent table name in the structure sent by table structure
    ''' </summary>
    ''' <param name="TableName">Name of the table to create.</param>
    ''' <param name="TableStructure">List of table fields that define the table.</param>
    Public Sub CreateTable(ByVal TableName As String, ByVal TableStructure As List(Of DBTableField))
        Dim SQL As String
        Dim PKFields As New List(Of String)

        ' Drop the table first
        Call DropTable(TableName)

        ' Get all the PK fields
        For Each F In TableStructure
            If F.IsPrimaryKey Then
                PKFields.Add(F.FieldName)
            End If
        Next

        ' Now build the SQL and execute
        SQL = String.Format("CREATE TABLE {0} (", TableName)

        For Each Record In TableStructure
            With Record
                ' Add field name
                SQL &= .FieldName & SPACE

                Select Case .FieldType
                    ' Format any strings
                    Case FieldType.nchar_type, FieldType.ntext_type, FieldType.nvarchar_type, FieldType.char_type, FieldType.varchar_type, FieldType.text_type
                        ' Add string type
                        If .FieldLength = -1 Then
                            SQL &= "TEXT" ' Leave open
                        Else
                            ' Format length
                            If .FieldType = FieldType.nchar_type Then
                                SQL &= "NCHAR(" & .FieldLength & ")"
                            ElseIf .FieldType = FieldType.nvarchar_type Then
                                SQL &= "NVARCHAR(" & .FieldLength & ")"
                            ElseIf .FieldType = FieldType.ntext_type Then
                                SQL &= "NTEXT(" & .FieldLength & ")"
                            ElseIf .FieldType = FieldType.char_type Then
                                SQL &= "CHARACTER(" & .FieldLength & ")"
                            ElseIf .FieldType = FieldType.varchar_type Then
                                SQL &= "VARCHAR(" & .FieldLength & ")"
                            ElseIf .FieldType = FieldType.text_type Then
                                SQL &= "TEXT " ' This is always open
                            End If

                        End If

                    Case FieldType.double_type
                        SQL &= "DOUBLE"
                    Case FieldType.float_type
                        SQL &= "FLOAT"
                    Case FieldType.real_type
                        SQL &= "REAL"
                    Case FieldType.int_type
                        SQL &= "INTEGER"
                    Case FieldType.int_type
                        SQL &= "TINYINT"
                    Case FieldType.smallint_type
                        SQL &= "SMALLINT"
                    Case FieldType.int_type
                        SQL &= "INTEGER"
                    Case FieldType.bigint_type
                        SQL &= "BIGINT"
                End Select

                ' If there is only one PK, then set it when creating the table
                If PKFields.Count = 1 And .IsPrimaryKey Then
                    SQL &= SPACE & "PRIMARY KEY" & SPACE
                End If

                If Not .IsNull Or .IsPrimaryKey Then ' All PKs are not null anyway
                    SQL &= SPACE & "NOT NULL" & COMMA
                Else
                    SQL &= COMMA
                End If

            End With
        Next

        ' Strip the final comma
        SQL = StripLastCharacter(SQL) & ")"

        Call ExecuteNonQuerySQL(SQL)

        ' Finally build any unique PK index
        If PKFields.Count > 1 Then
            ' Create a unique index now with multiple PK fields for SQLite
            Call CreateIndex(TableName, "IDX_" & TableName & "_PK", PKFields, True)
        End If

    End Sub

    ''' <summary>
    ''' Creates an index on the table sent using the fields sent.
    ''' </summary>
    ''' <param name="TableName">Table the index will apply to.</param>
    ''' <param name="IndexName">Unique name of the index on the database.</param>
    ''' <param name="IndexFields">Fields that make up the index.</param>
    ''' <param name="Unique">If the index is unique.</param>
    ''' <param name="Clustered">Optional value - If the index is clustered or unclustered (not used).</param>
    Public Sub CreateIndex(ByVal TableName As String, ByVal IndexName As String, IndexFields As List(Of String),
                           Optional Unique As Boolean = False, Optional Clustered As Boolean = False)
        Dim SQL As String

        SQL = "CREATE" & SPACE

        ' Unique index
        If Unique Then
            SQL &= "UNIQUE" & SPACE
        End If

        SQL &= String.Format("INDEX {0} ON {1} (", IndexName, TableName)

        ' Build index fields
        For Each Field In IndexFields
            SQL &= Field & COMMA
        Next

        ' Strip the last comman
        SQL = StripLastCharacter(SQL) & ")"

        ' Create index
        Call ExecuteNonQuerySQL(SQL)

    End Sub

    ''' <summary>
    ''' Inserts the list of record values (fields) into the table name provided.
    ''' </summary>
    ''' <param name="TableName">Table to insert records.</param>
    ''' <param name="Record">List of table fields that make up the record.</param>
    Public Sub InsertRecord(ByVal TableName As String, Record As List(Of DBField))
        Dim Fields As String = ""
        Dim FieldValues As String = ""

        For Each Field In Record
            Fields = Fields & Field.FieldName & COMMA
            If Field.FieldValue = "" Then
                FieldValues = FieldValues & NULL & COMMA
            Else
                FieldValues = FieldValues & Field.FieldValue & COMMA
            End If
        Next

        ' Strip the last commas
        Fields = StripLastCharacter(Fields)
        FieldValues = StripLastCharacter(FieldValues)

        SyncLock Lock
            Call ExecuteNonQuerySQL(String.Format("INSERT INTO {0} ({1}) VALUES ({2})", TableName, Fields, FieldValues))
        End SyncLock
    End Sub

    ''' <summary>
    '''  Sets the names of Stars, Planets, Moons, Asteroid Belts, Stations, and Stargates with this schema
    '''  https://developers.eveonline.com/docs/services/static-data/#automation
    ''' </summary>
    Public Sub SetCelestialNames()
        Dim SQL As String

        ' Stars and stargates are set to the solarSystemName, so update first
        SQL = "UPDATE mapStargates SET stargateName = (SELECT solarSystemName FROM mapSolarSystems "
        SQL &= "WHERE mapSolarSystems.solarSystemID = mapStargates.destinationSolarSystemID) "
        SQL &= "WHERE EXISTS (SELECT 1 FROM mapSolarSystems WHERE mapSolarSystems.solarSystemID = mapStargates.solarSystemID)"
        ExecuteNonQuerySQL(SQL)

        SQL = "UPDATE mapStars SET starName = (SELECT solarSystemName FROM mapSolarSystems "
        SQL &= "WHERE mapSolarSystems.solarSystemID = mapStars.solarSystemID) "
        SQL &= "WHERE EXISTS (SELECT 1 FROM mapSolarSystems WHERE mapSolarSystems.solarSystemID = mapStars.solarSystemID)"
        ExecuteNonQuerySQL(SQL)

        ' The rest of the updates concatenates fields but each are different operators, so select here
        Dim ConcatOperator As String = "||"

        ' Planets orbit stars, so it's <starName> <celestialIndex>
        ' First set up the roman look up table
        Call CreateRomanNumberTable()
        SQL = "UPDATE mapPlanets SET planetName = (SELECT starName FROM mapStars "
        SQL &= "WHERE mapStars.starID = mapPlanets.orbitID)" & ConcatOperator & "' '" & ConcatOperator
        SQL &= "(SELECT roman FROM RomanLookup WHERE RomanLookup.value = mapPlanets.celestialIndex)"
        ExecuteNonQuerySQL(SQL)

        ' Moons orbit planets
        SQL = "UPDATE mapMoons SET moonName = (SELECT planetName FROM mapPlanets  "
        SQL &= "WHERE mapPlanets.planetID = mapMoons.orbitID)" & ConcatOperator
        SQL &= "' - Moon '" & ConcatOperator & "mapMoons.orbitIndex"
        ExecuteNonQuerySQL(SQL)

        ' Belts orbit planets
        SQL = "UPDATE mapAsteroidBelts SET asteroidBeltName = (SELECT planetName FROM mapPlanets  "
        SQL &= "WHERE mapPlanets.planetID = mapAsteroidBelts.orbitID)" & ConcatOperator
        SQL &= "' - Asteroid Belt '" & ConcatOperator & "mapAsteroidBelts.orbitIndex"
        ExecuteNonQuerySQL(SQL)

        ' Stations orbit planets and moons, plus they have the corporation name and operation name if useOperationName is true
        SQL = "UPDATE npcStations SET stationName = (SELECT planetName FROM mapPlanets "
        SQL &= "WHERE mapPlanets.planetID = npcStations.orbitID) " & ConcatOperator & "' - '" & ConcatOperator
        SQL &= "(SELECT corporationName FROM npcCorporations WHERE npcCorporations.corporationID = npcStations.ownerID) "
        SQL &= "WHERE useOperationName = 0 AND stationName is NULL"
        ExecuteNonQuerySQL(SQL)

        SQL = "UPDATE npcStations SET stationName = (SELECT planetName FROM mapPlanets "
        SQL &= "WHERE mapPlanets.planetID = npcStations.orbitID) " & ConcatOperator & "' - '" & ConcatOperator
        SQL &= "(SELECT corporationName FROM npcCorporations WHERE npcCorporations.corporationID = npcStations.ownerID)" & ConcatOperator & " ' ' " & ConcatOperator
        SQL &= "(SELECT operationName FROM stationOperations WHERE stationOperations.operationID = npcStations.operationID)"
        SQL &= "WHERE useOperationName = 1 AND stationName is NULL"
        ExecuteNonQuerySQL(SQL)

        ' now moons
        SQL = "UPDATE npcStations SET stationName = (SELECT moonName FROM mapMoons "
        SQL &= "WHERE mapMoons.moonID = npcStations.orbitID) " & ConcatOperator
        SQL &= "(SELECT moonName FROM mapMoons WHERE mapMoons.moonID = npcStations.orbitID)" & ConcatOperator & "' - '" & ConcatOperator
        SQL &= "(SELECT corporationName FROM npcCorporations WHERE npcCorporations.corporationID = npcStations.ownerID) "
        SQL &= "WHERE useOperationName = 0 AND stationName is NULL"
        ExecuteNonQuerySQL(SQL)

        SQL = "UPDATE npcStations SET stationName = (SELECT moonName FROM mapMoons "
        SQL &= "WHERE mapMoons.moonID = npcStations.orbitID) " & ConcatOperator & "' - '" & ConcatOperator
        SQL &= "(SELECT corporationName FROM npcCorporations WHERE npcCorporations.corporationID = npcStations.ownerID) " & ConcatOperator & " ' ' " & ConcatOperator
        SQL &= "(SELECT operationName FROM stationOperations WHERE stationOperations.operationID = npcStations.operationID)"
        SQL &= "WHERE useOperationName = 1 AND stationName is NULL"
        ExecuteNonQuerySQL(SQL)

        ' and one (at the time of this change) orbited a star
        SQL = "UPDATE npcStations SET stationName = (SELECT starName FROM mapStars "
        SQL &= "WHERE mapStars.starID = npcStations.orbitID) " & ConcatOperator & "' - '" & ConcatOperator
        SQL &= "(SELECT corporationName FROM npcCorporations WHERE npcCorporations.corporationID = npcStations.ownerID) "
        SQL &= "WHERE useOperationName = 0 AND stationName is NULL"
        ExecuteNonQuerySQL(SQL)

        SQL = "UPDATE npcStations SET stationName = (SELECT starName FROM mapStars "
        SQL &= "WHERE mapStars.starID = npcStations.orbitID) " & ConcatOperator & "' - '" & ConcatOperator
        SQL &= "(SELECT corporationName FROM npcCorporations WHERE npcCorporations.corporationID = npcStations.ownerID)" & ConcatOperator & " ' ' " & ConcatOperator
        SQL &= "(SELECT operationName FROM stationOperations WHERE stationOperations.operationID = npcStations.operationID)"
        SQL &= "WHERE useOperationName = 1 AND stationName is NULL"
        ExecuteNonQuerySQL(SQL)

        ' don't need this anymore
        Call DropTable("RomanLookup")

    End Sub

    Public Sub CreateRomanNumberTable()
        Dim DataFields As New List(Of DBField)
        Dim Table As New List(Of DBTableField) From {
            New DBTableField("value", FieldType.int_type, 0, False, True),
            New DBTableField("roman", FieldType.nvarchar_type, 10, True)
        }
        Call CreateTable("RomanLookup", Table)

        ' Insert Data for 50 numbers
        For i = 1 To 50
            DataFields = New List(Of DBField) From {
                BuildDatabaseField("value", i, FieldType.int_type),
                BuildDatabaseField("roman", ToRoman(i), FieldType.nvarchar_type)
            }
            Call InsertRecord("RomanLookup", DataFields)
        Next

    End Sub

End Class