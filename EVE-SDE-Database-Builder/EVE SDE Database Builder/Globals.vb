Imports System.IO
Imports System.Net
Imports System.Net.Http

' Common types and variables for the program
Public Module Globals

    Public CancelImport As Boolean = False
    Public CancelDownload As Boolean = False
    Public frmErrorText As String = ""
    Public Lock As New Object
    Public Lock2 As New Object
    Public AllSettings As New ProgramSettings
    Public UserApplicationSettings As ApplicationSettings

    Public FileNameErrorTracker As String = ""

    Public EXEFileFolder As String = ""

    Public RetryCall As Boolean = False

    ''' <summary>
    ''' Displays error message from Try/Catch Exceptions
    ''' </summary>
    ''' <param name="ex">Exepction variable for displaying exception text</param>
    Public Sub ShowErrorMessage(ex As Exception)

        If CancelImport Then Exit Sub

        Dim msg As String = ex.Message

        If ex.InnerException IsNot Nothing Then
            msg &= vbCrLf & vbCrLf & "Inner Exception: " & ex.InnerException.ToString()
        End If

        MessageBox.Show(
        msg,
        Application.ProductName,
        MessageBoxButtons.OK,
        MessageBoxIcon.Exclamation,
        MessageBoxDefaultButton.Button1,
        MessageBoxOptions.ServiceNotification)

    End Sub

    ''' <summary>
    ''' Writes a sent message to the Errors.log file
    ''' </summary>
    ''' <param name="ErrorMsg">Message to write to log file</param>
    Public Sub WriteMsgToErrorLog(ByVal ErrorMsg As String)
        Call OutputToFile("Errors.log", ErrorMsg)
    End Sub

    ''' <summary>
    ''' Writes a sent message to the OutputLog.log file
    ''' </summary>
    ''' <param name="OutputMessage">Message to write to log file</param>
    Public Sub OutputMsgtoLog(ByVal OutputMessage As String)
        Call OutputToFile("OutputLog.log", OutputMessage)
    End Sub

    ''' <summary>
    ''' Outputs sent output text to the file path
    ''' </summary>
    ''' <param name="FilePath">Path of outputfile with name</param>
    ''' <param name="OutputText">Text to output to file</param>
    Private Sub OutputToFile(FilePath As String, OutputText As String)
        Dim AllText() As String

        If Not IO.File.Exists(FilePath) Then
            Dim sw As IO.StreamWriter = IO.File.CreateText(FilePath)
            sw.Close()
        End If

        ' This is an easier way to get all of the strings in the file.
        AllText = IO.File.ReadAllLines(FilePath)
        ' This will append the string to the end of the file.
        My.Computer.FileSystem.WriteAllText(FilePath, CStr(Now) & ", " & OutputText & Environment.NewLine, True)

    End Sub

    ' Initializes the main form grid 
    Private Delegate Sub InitRow(ByVal Position As Integer)
    Public Sub InitGridRow(ByVal Postion As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New InitRow(AddressOf f1.InitGridRow), Postion)
        Application.DoEvents()
    End Sub

    ' Updates the main form grid
    Private Delegate Sub UpdateRowProgress(ByVal Position As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)
    Public Sub UpdateGridRowProgress(ByVal Postion As Integer, ByVal Count As Integer, ByVal TotalRecords As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New UpdateRowProgress(AddressOf f1.UpdateGridRowProgress), Postion, Count, TotalRecords)
        Application.DoEvents()
    End Sub

    ' Just checks for null field value and returns nothing for ease of coding in SDE files
    Public Function GetTranslation(ByVal FieldValue As TranslatedNameField, LanguageCode As String) As String
        Dim ReturnTranslation As String = ""
        Try
            If Not IsNothing(FieldValue) Then
                Select Case LanguageCode
                    Case "en"
                        ReturnTranslation = FieldValue.en
                    Case "de"
                        ReturnTranslation = FieldValue.de
                    Case "fr"
                        ReturnTranslation = FieldValue.fr
                    Case "ja"
                        ReturnTranslation = FieldValue.ja
                    Case "ru"
                        ReturnTranslation = FieldValue.ru
                    Case "zh"
                        ReturnTranslation = FieldValue.zh
                    Case Else
                        ReturnTranslation = FieldValue.en ' default to english if an unknown language code is sent
                End Select

                If ReturnTranslation = "" Then
                    ' English is required, so return that if it's there
                    ReturnTranslation = FieldValue.en
                End If

                Return ReturnTranslation
            Else
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try

    End Function
    Public Class TranslatedNameField
        Public Property de As String
        Public Property en As String
        Public Property fr As String
        Public Property ja As String
        Public Property ko As String
        Public Property ru As String
        Public Property zh As String
    End Class
    ' Process a nullable field from JSON for data inserts correctly
    Public Function NullableField(ByVal FieldValue As Object) As Object
        If IsNothing(FieldValue) Then
            Return DBNull.Value
        Else
            ' parse the value
            Dim parsedvalue As String = CStr(FieldValue.ToString).Trim()
            If parsedvalue = "" Then
                Return DBNull.Value
            Else
                Return ConvertStringToTypedValue(parsedvalue)
            End If
        End If
    End Function

    Public Function ConvertStringToTypedValue(input As String) As Object
        If input Is Nothing Then Return DBNull.Value

        Dim s As String = input.Trim()

        ' Null-like values
        If s = "" OrElse s.ToLower() = "null" Then
            Return DBNull.Value
        End If

        ' Boolean
        If s.Equals("true", StringComparison.OrdinalIgnoreCase) Then Return 1
        If s.Equals("false", StringComparison.OrdinalIgnoreCase) Then Return 0

        ' Integer (Int32)
        Dim i32 As Integer
        If Integer.TryParse(s, i32) Then Return i32

        ' Integer (Int64)
        Dim i64 As Long
        If Long.TryParse(s, i64) Then Return i64

        ' Floating point (Double)
        Dim dbl As Double
        If Double.TryParse(s, Globalization.NumberStyles.Any,
                       Globalization.CultureInfo.InvariantCulture, dbl) Then
            Return dbl
        End If

        ' Fallback: return original string
        Return s
    End Function

    Public Function BooleanField(ByVal FieldValue As Object) As Object
        If FieldValue Is Nothing Then Return DBNull.Value
        Dim s = FieldValue.ToString().Trim().ToLower()
        If s = "" Then Return DBNull.Value
        Return If(CBool(s), 1, 0)
    End Function

    ''' <summary>
    ''' Downloads the sent file from server and saves it to the root directory as the sent file name
    ''' </summary>
    ''' <param name="DownloadURL">URL to download the file</param>
    ''' <param name="FileName">File name of downloaded file</param>
    ''' <returns>File Name of where the downloaded file was saved.</returns>
    Public Function DownloadFileFromServer(ByVal DownloadURL As String, ByVal FileName As String, Optional ByRef FileDate As Date = Nothing, Optional PGBar As ProgressBar = Nothing) As String
        Dim FileSize As Double
        Dim nRead As Long
        Dim buffer(4095) As Byte

        Try
            If Not Directory.Exists(Path.GetDirectoryName(FileName)) Then
                Directory.CreateDirectory(Path.GetDirectoryName(FileName))
            End If
        Catch ex As Exception
            MsgBox("An error downloading the file occured: " & ex.Message, vbExclamation, Application.ProductName)
            Return ""
        End Try

        If Not IsNothing(PGBar) Then
            PGBar.Minimum = 0
            PGBar.Value = 0
            PGBar.Visible = True
            Application.DoEvents()
        End If

        Try
            Using handler As New HttpClientHandler()
                ' Configure TLS and proxy/credentials per-client instead of using ServicePointManager
                handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                handler.UseProxy = False
                handler.Credentials = CredentialCache.DefaultCredentials

                Using client As New HttpClient(handler, disposeHandler:=True)
                    client.DefaultRequestHeaders.ExpectContinue = True

                    Dim response As HttpResponseMessage = client.GetAsync(DownloadURL, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult()
                    response.EnsureSuccessStatusCode()

                    FileSize = response.Content.Headers.ContentLength.GetValueOrDefault()
                    If response.Content.Headers.LastModified.HasValue Then
                        FileDate = response.Content.Headers.LastModified.Value.DateTime
                    End If

                    Using stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                        Using writeStream As New FileStream(FileName, FileMode.Create)
                            Do
                                Application.DoEvents()
                                If CancelDownload Then Exit Do

                                Dim bytesRead As Integer = stream.Read(buffer, 0, buffer.Length)
                                If bytesRead = 0 Then Exit Do

                                nRead += bytesRead
                                If Not IsNothing(PGBar) AndAlso FileSize > 0 Then
                                    PGBar.Value = CInt((nRead * 100) / FileSize)
                                End If

                                writeStream.Write(buffer, 0, bytesRead)
                            Loop
                        End Using
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' Optionally log the exception here
            Return ""
        End Try

        If Not IsNothing(PGBar) Then
            PGBar.Value = 0
            PGBar.Visible = False
        End If

        Return FileName
    End Function

    ' Finalizes the main form grid
    Private Delegate Sub FinalizeRow(ByVal Position As Integer)
    Public Sub FinalizeGridRow(ByVal Postion As Integer)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New FinalizeRow(AddressOf f1.FinalizeGridRow), Postion)
        Application.DoEvents()
    End Sub

    ' Initializes the progressbar on the main form
    Private Delegate Sub InitMainProgress(MaxCount As Long, UpdateText As String)
    Public Sub InitalizeMainProgressBar(MaxCount As Long, UpdateText As String)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New InitMainProgress(AddressOf f1.InitalizeProgress), MaxCount, UpdateText)
        Application.DoEvents()
    End Sub

    ' Clears the progress bar and label on the main form
    Private Delegate Sub ClearMainProgress()
    Public Sub ClearMainProgressBar()

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New ClearMainProgress(AddressOf f1.ClearProgress))
        Application.DoEvents()
    End Sub

    ' Updates the main progress bar and label on the main form
    Private Delegate Sub UpdateMainProgress(ByVal Count As Long, ByVal UpdateText As String)
    Public Sub UpdateMainProgressBar(Count As Long, UpdateText As String)

        Dim f1 As frmMain = DirectCast(My.Application.OpenForms.Item("frmMain"), frmMain)
        f1.Invoke(New UpdateMainProgress(AddressOf f1.UpdateProgress), Count, UpdateText)
        Application.DoEvents()
    End Sub
    ''' <summary>
    ''' This will create a new directory. If the directory already exists it will delete it first.
    ''' </summary>
    ''' <param name="DirectoryPath"></param>
    Public Sub CreateNewDirectory(DirectoryPath As String)
        DeleteMyDirectory(DirectoryPath)
        Directory.CreateDirectory(DirectoryPath)
    End Sub

    Public Sub DeleteMyDirectory(DirectoryPath As String)
        If Directory.Exists(DirectoryPath) Then
            Dim di As New DirectoryInfo(DirectoryPath)
            di.Attributes = FileAttributes.Normal
            Directory.Delete(DirectoryPath, True)
        End If
    End Sub


    ''' <summary>
    ''' Calculates the MD5 hash for the sent file.
    ''' </summary>
    ''' <param name="filepath">File to calculate an MD5 for</param>
    ''' <returns>The formatted hash as a string</returns>
    Public Function MD5CalcFile(ByVal filepath As String) As String

        ' Open file (as read-only) - If it's not there, return ""
        If IO.File.Exists(filepath) Then
            Using reader As New System.IO.FileStream(filepath, IO.FileMode.Open, IO.FileAccess.Read)
                Using md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create()
                    ' hash contents of this stream
                    Dim hash() As Byte = md5.ComputeHash(reader)

                    ' return formatted hash
                    Return ByteArrayToString(hash)

                End Using
            End Using
        End If

        ' Something went wrong
        Return ""

    End Function

    ''' <summary>
    ''' Converts byte array to a hex string for MD5 hash
    ''' </summary>
    ''' <param name="arrInput">Array of bytes</param>
    ''' <returns>Hex string of bytes input</returns>
    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String

        Dim sb As New System.Text.StringBuilder(arrInput.Length * 2)

        For i As Integer = 0 To arrInput.Length - 1
            sb.Append(arrInput(i).ToString("X2"))
        Next

        Return sb.ToString().ToLower

    End Function

End Module

Public Class celestialposition
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double
End Class

Public Class celestialposition2D
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double
End Class