Imports System.Data.SQLite
Imports System.IO

Module Functions



    Function EventProcessInfo(ByVal admonpath As String, rowid As String, imagefilter As String) As ParentProcessInfo
        Dim rtninfo As New ParentProcessInfo
        Dim procconn As New SQLiteConnection("Data Source=" & admonpath & ";Read Only=True;")

        procconn.Open()
        Dim procquery As String
        Select Case imagefilter
            Case "*"
                procquery = "Select * from ProcessEvent where rowid = '" & rowid & "'"

        Case Else
                procquery = "select * from ProcessEvent where rowid = '" & rowid & "' AND (FullPath LIKE '%" & imagefilter & "%' OR CommandLine LIKE '%" & imagefilter & "%')"
        End Select
        Dim proccmd As New SQLiteCommand(procquery, procconn)
        Dim procreader As SQLiteDataReader = proccmd.ExecuteReader()
        If procreader.Read() Then
            rtninfo.Path = procreader("FullPath")
            rtninfo.PID = procreader("ProcessID")
            rtninfo.PPID = procreader("ParentID")
            procreader.Close()
            proccmd.Dispose()
            procconn.Close()
            procconn.Dispose()
        End If
        Return rtninfo
    End Function

    Sub ExtractDLL()
        Try
            If IO.File.Exists("Support.exe") Then
                Dim psinfo As New ProcessStartInfo
                psinfo.FileName = "Support.exe"
                psinfo.Arguments = "-o .\ -y"
                psinfo.CreateNoWindow = True
                psinfo.WindowStyle = ProcessWindowStyle.Hidden

                Dim ext = Process.Start(psinfo)
                ext.WaitForExit()
            End If
        Catch ex As Exception
            Console.Error.WriteLine("0 - " & ex.Message)
            Environment.Exit(0)
        End Try

    End Sub

    Function Get_TimeBeforeNow(ByVal TBefore As Integer)
        Try

            'Subtract 1600 years for ETM time, then the minutes in TBefore
            Dim rtn As New DateTime
            If TBefore > 0 Then
                rtn = DateTime.UtcNow
                rtn = rtn.AddYears(-1600).AddMinutes(Math.Abs(TBefore) * -1)
                'rtn = Now.AddYears(-1600).Subtract(New TimeSpan(0, TBefore, 0))
            Else
                rtn = Now.AddYears(-1600)
            End If

            Debug.WriteLine("TimeBeforeNow = " & rtn.ToString("o"))
            Return rtn
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Console.Error.WriteLine("2 - " & ex.Message)
            Return Now.AddYears(-1600)
        End Try


    End Function

    Function Convert_TimeToTick(ByVal dt As DateTime)
        Try
            Debug.WriteLine("Time To Ticks = " & dt.Ticks)
            Return dt.Ticks

        Catch ex As Exception
            Console.Error.WriteLine("3 - " & ex.Message)
            Return Now.AddYears(-1600).Ticks
        End Try

    End Function

    Sub QueryProcess(admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
        Dim ImageFilter As String
        Dim eventType = "Process Event"
        Dim conn As New SQLiteConnection("Data Source=" & admonpath & ";Read Only=True;")

        Dim filterarg As New List(Of String)(My.Application.CommandLineArgs(9).Split(","c))



        For Each arg In filterarg
            Debug.WriteLine(arg)
            ImageFilter = arg
            ' Dim query As String = "select * from ProcessEvent where FullPath LIKE '%" & ImageFilter & "%' OR CurrentProcessID LIKE '%" & Filter() & "%' OR ParentId LIKE '%" & Filter() & "%' OR ProcessId LIKE '%" & Filter() & "%' OR Hash LIKE '%" & Filter() & "%' OR UserName LIKE '%" & Filter() & "%' OR CommandLine LIKE '%" & Filter() & "%'"
            Dim query As String = "select * from ProcessEvent"

            If TBeforeGreaterThan0 = True Then
                query = query & " where StartTime >= '" & TBefore & "'"
                Debug.WriteLine(query)
            Else
                query = query & " where StartTime <= '" & TBefore & "'"
                Debug.WriteLine(query)
            End If



        Select Case ImageFilter
                Case "*"
                    'Do nothing
                Case Else
                    'query = "select * from ProcessEvent where FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%'"
                    query = query & " AND (FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%')"
            End Select
            Debug.WriteLine("Full Query = " & query)
            Try
                conn.Open()

            Catch ex As Exception
                Console.Error.WriteLine("9 - " & ex.Message)

            End Try

            Dim conncmd As New SQLiteCommand(query, conn)
            Dim datareader As SQLiteDataReader = conncmd.ExecuteReader()
            If datareader.HasRows Then
                While datareader.Read()

                    Try

                        Dim eventout As New EventOutput
                        If datareader("StartTime") > DateTime.MinValue.Ticks And datareader("StartTime") < DateTime.MaxValue.Ticks Then
                            eventout.EventTime = New DateTime(datareader("StartTime")).ToLocalTime.AddYears(1600).ToString("yyyy-MM-dd hh:mm:ss tt")
                        Else
                            eventout.EventTime = "N/A"
                        End If
                        If datareader("EndTime") > DateTime.MinValue.Ticks And datareader("EndTime") < DateTime.MaxValue.Ticks Then

                            eventout.EndTime = New DateTime(datareader("EndTime")).ToLocalTime.AddYears(1600).ToString("yyyy-MM-dd hh:mm:ss tt")
                        Else
                            eventout.EndTime = "N/A"
                        End If
                        If eventout.EventTime = "1601-01-01 12:00:00 AM" Then
                            eventout.EventTime = "N/A"
                        End If
                        If eventout.EndTime = "1601-01-01 12:00:00 AM" Then
                            eventout.EndTime = "N/A"
                        End If
                        eventout.EventType = eventType
                        eventout.FullPath = datareader("FullPath").ToString()
                        eventout.CurrentPID = datareader("CurrentProcessID")
                        eventout.ParentPID = datareader("ParentID").ToString()
                        eventout.PID = datareader("ProcessID").ToString()
                        eventout.Overflowed = datareader("OverFlowed").ToString()
                        eventout.Hash = datareader("Hash").ToString()
                        eventout.Username = datareader("UserName").ToString()
                        eventout.CommandLine = datareader("CommandLine").ToString()
                        ' Debug.WriteLine(eventout)
                        Console.WriteLine(eventout)


                    Catch ex As Exception
                        Console.Error.WriteLine("4 - " & ex.Message)
                        Continue While
                    End Try
                End While
                datareader.Close()
                conncmd.Dispose()
            End If
            'Run Next Filterarg
        Next
        conn.Close()
            conn.Dispose()

    End Sub
    Sub QueryReg(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
        Dim RegArg As New List(Of String)(My.Application.CommandLineArgs(6).Split(","c))
        Dim ImageArg As New List(Of String)(My.Application.CommandLineArgs(9).Split(","c))
        Dim eventType = "Registry Event"
        Dim index As Integer = 10
nextindex:

        While index >= 0

            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"
            Debug.WriteLine("Current Network Database: " & dbname)

            If File.Exists(eventspath & dbname) Then
                ' File is Here
            Else
                index -= 1
                GoTo nextindex
            End If

            Dim rowCount = 0

            Dim regconn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")
            regconn.Open()

            For Each arg In RegArg
                'Dim Query As String = "select * from Events where EventType = '0'"
                Dim Query As String = "select * from Events where EventType = '0'"
                If TBeforeGreaterThan0 = True Then
                    Query = Query & " AND Time >= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                Else
                    Query = Query & " AND Time <= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                End If
                Select Case arg
                    Case "*"
                        'Do nothing

                    Case Else
                        Query = Query & " AND (Path LIKE '%" & arg & "%')"

                End Select
                Debug.WriteLine("Full Query = " & Query)
                Dim regcmd As New SQLiteCommand(Query, regconn)

                Dim datareader As SQLiteDataReader = regcmd.ExecuteReader()

                If datareader.HasRows Then

                    While datareader.Read()
                        Try
                            ' Reached RegDateFunction while loop 1

                            rowCount = datareader("ProcessRow")
                            'Get parent process info x imgarg
                            For Each arg1 In ImageArg
                                Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, arg1)
                                Dim matchimage As Boolean = True
                                If Not arg1 = "*" Then
                                    If procinfo.Path = vbNullString Then matchimage = False
                                End If

                                If matchimage = True Then

                                    Dim eventout As New EventOutput
                                    eventout.EventTime = New DateTime(datareader("Time")).ToLocalTime.AddYears(1600).ToString("yyyy-MM-dd hh:mm:ss tt")
                                    eventout.EventType = eventType
                                    eventout.FullPath = procinfo.Path
                                    eventout.RegistryPath = datareader("Path").ToString
                                    eventout.ParentPID = procinfo.PPID
                                    eventout.PID = procinfo.PID
                                    eventout.Key = datareader("Key").ToString
                                    eventout.Data = datareader("Data").ToString

                                    ' Debug.WriteLine(eventout)
                                    Console.WriteLine(eventout)

                                End If
                            Next
                        Catch ex As Exception
                            Console.Error.WriteLine("5 - " & ex.Message)
                        End Try

                    End While
                    regcmd.Dispose()
                    datareader.Close()

                End If
            Next
            regconn.Close()
            regconn.Dispose()
            index -= 1
        End While

    End Sub

    Sub QueryNetwork(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
        Dim eventType = "Network Event"
        Dim index As Integer = 10
        Dim NetArg As New List(Of String)(My.Application.CommandLineArgs(7).Split(","c))
        Dim ImageArg As New List(Of String)(My.Application.CommandLineArgs(9).Split(","c))

nextindex:

        While index >= 0

            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"
            Debug.WriteLine("Current Network Database: " & dbname)

            If File.Exists(eventspath & dbname) Then
                ' File is Here
            Else
                index -= 1
                GoTo nextindex
            End If

            Dim rowCount = 0

            Dim netconn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")

            netconn.Open()

            For Each arg In NetArg
                'Dim Query As String = "select * from Events where EventType = '1'"
                Dim Query As String = "select * from Events where EventType = '1'"
                If TBeforeGreaterThan0 = True Then
                    Query = Query & " AND Time >= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                Else
                    Query = Query & " AND Time <= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                End If
                Select Case arg
                    Case "*"
                        'Do nothing

                    Case Else
                        Query = Query & " AND (LocalAddress LIKE '%" & arg & "%' OR LocalPort LIKE '%" & arg & "%' OR RemoteAddress LIKE '%" & arg & "%' OR RemotePort LIKE '%" & arg & "%' OR URL LIKE '%" & arg & "%')"

                End Select
                Debug.WriteLine("Full Query = " & Query)

                Dim netcmd As New SQLiteCommand(Query, netconn)

                Dim datareader As SQLiteDataReader = netcmd.ExecuteReader()

                If datareader.HasRows Then

                    While datareader.Read()

                        Try

                            rowCount = datareader("ProcessRow")
                            'Get parent process info x imgarg
                            For Each arg1 In ImageArg
                                Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, arg1)
                                Dim matchimage As Boolean = true
                                If Not arg1 = "*" Then
                                    If procinfo.Path = vbNullString Then matchimage = False
                                End If

                                If matchimage = True Then

                                    Dim AddressFamily As String
                                    Dim protocol As String

                                    If datareader("AddressFamily") = 2 Then
                                        AddressFamily = "IPV4"
                                    Else
                                        AddressFamily = "IPV6"
                                    End If

                                    If datareader("Protocol") = 6 Then
                                        protocol = "TCP"
                                    Else
                                        protocol = "UDP"
                                    End If


                                    Dim eventout As New EventOutput
                                    eventout.EventTime = New DateTime(datareader("Time")).ToLocalTime.AddYears(1600).ToString("yyyy-MM-dd hh:mm:ss tt")
                                    eventout.EventType = eventType
                                    eventout.FullPath = procinfo.Path
                                    eventout.ParentPID = procinfo.PPID
                                    eventout.PID = procinfo.PID
                                    eventout.AddressFamily = AddressFamily
                                    eventout.Protocol = protocol
                                    eventout.LocalAddress = datareader("LocalAddress").ToString
                                    eventout.LocalPort = datareader("LocalPort").ToString
                                    eventout.RemoteAddress = datareader("RemoteAddress").ToString
                                    eventout.RemotePort = datareader("RemotePort").ToString
                                    eventout.URL = datareader("URL").ToString

                                    '  Debug.WriteLine(eventout)
                                    Console.WriteLine(eventout)
                                End If
                            Next
                        Catch ex As Exception
                            Console.Error.WriteLine("6 - " & ex.Message)
                        End Try

                    End While
                    netcmd.Dispose()
                    datareader.Close()

                End If
            Next
            netconn.Close()
            netconn.Dispose()
            index -= 1
        End While

    End Sub

    Sub QueryFile(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
        Dim eventType = "File Event"
        Dim index As Integer = 10
        Dim FileFilter As String
        'Dim ImageFilter = My.Application.CommandLineArgs(9)
        Dim filterarg As New List(Of String)(My.Application.CommandLineArgs(8).Split(","c))
        Dim imagearg As New List(Of String)(My.Application.CommandLineArgs(9).Split(","c))

nextindex:

            While index >= 0

                Dim dbname As String = "events_"
                dbname = dbname & index.ToString & ".db"
                Debug.WriteLine("Current File Event Database: " & dbname)

                If File.Exists(eventspath & dbname) Then
                    ' File is Here
                Else
                    index -= 1
                    GoTo nextindex
                End If

                Dim rowCount = 0

            Dim fileconn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")

            fileconn.Open()

            For Each arg In filterarg
                Debug.WriteLine(arg)
                FileFilter = arg
                'Dim Query As String = "select * from Events where EventType = '3'"
                Dim Query As String = "select * from Events where EventType = '3'"
                If TBeforeGreaterThan0 = True Then
                    Query = Query & " AND Time >= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                Else
                    Query = Query & " AND Time <= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                End If
                Select Case FileFilter
                    Case "*"
                        'Do nothing

                    Case Else
                        Query = Query & " AND (Path like '%" & FileFilter & "%')"

                End Select
                Debug.WriteLine("Full Query = " & Query)

                Dim filecmd As New SQLiteCommand(Query, fileconn)

                Dim datareader As SQLiteDataReader = filecmd.ExecuteReader()

                If datareader.HasRows Then

                    While datareader.Read()
                        Try


                            rowCount = datareader("ProcessRow")
                            'Get parent process info x image filters
                            For Each argimg In imagearg
                                Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, argimg)
                                Dim matchimage As Boolean = True
                                If Not argimg = "*" Then
                                    If procinfo.Path = vbNullString Then matchimage = False
                                End If

                                If matchimage = True Then
                                    Dim action As String
                                    If datareader("EventSubType") = 4 And datareader("isCreate") = 1 Then
                                        action = "File Created"
                                    ElseIf datareader("EventSubType") = 4 And datareader("isCreate") = 0 Then
                                        action = "Write"
                                    ElseIf datareader("EventSubType") = 0 Then
                                        action = "Read"
                                    Else
                                        action = "N/A"
                                    End If

                                    Dim eventout As New EventOutput

                                    eventout.EventTime = New DateTime(datareader("Time")).ToLocalTime.AddYears(1600).ToString("yyyy-MM-dd hh:mm:ss tt")
                                    eventout.EventType = eventType
                                    eventout.FullPath = procinfo.Path
                                    eventout.CommandLine = datareader("Path")
                                    eventout.ParentPID = procinfo.PPID
                                    eventout.PID = procinfo.PID
                                    eventout.FileAction = action
                                    '  Debug.WriteLine(eventout)

                                    Console.WriteLine(eventout)
                                End If
                            Next
                        Catch ex As Exception
                            Console.Error.WriteLine("7 - " & ex.Message)
                        End Try

                    End While
                    filecmd.Dispose()
                    datareader.Close()
                End If
            Next
            fileconn.Close()
                fileconn.Dispose()
            index -= 1
        End While

    End Sub

    Sub QueryImage(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
        Dim eventType = "Image Event"
        Dim index As Integer = 10
        Dim imagearg As New List(Of String)(My.Application.CommandLineArgs(9).Split(","c))
nextindex:

        While index >= 0

            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"
            Debug.WriteLine("Current Image Event Database: " & dbname)

            If File.Exists(eventspath & dbname) Then
                ' File is Here
            Else
                index -= 1
                GoTo nextindex
            End If

            Dim rowCount = 0
            ' Console.WriteLine(DataPath & dbname)
            Dim ImageConn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")

            ImageConn.Open()
            For Each arg In imagearg
                ' Dim Query As String = "select * from Events where EventType = '2'"
                Dim Query As String = "select * from Events where EventType = '2'"
                If TBeforeGreaterThan0 = True Then
                    Query = Query & " AND Time >= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                Else
                    Query = Query & " AND Time <= '" & TBefore & "'"
                    Debug.WriteLine(Query)
                End If
                Select Case arg
                    Case "*"
                        'Do nothing

                    Case Else
                        Query = Query & " AND (Path like '%" & arg & "%' OR Hash like '%" & arg & "%')"

                End Select
                Debug.WriteLine("Full Query = " & Query)
                Dim imgcmd As New SQLiteCommand(Query, ImageConn)

                Dim datareader As SQLiteDataReader = imgcmd.ExecuteReader()

                If datareader.HasRows Then

                    While datareader.Read()
                        Try


                            rowCount = datareader("ProcessRow")
                            'Get parent process info x imgfilter
                            For Each imgarg1 In imagearg
                                Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, imgarg1)
                                Dim matchimage As Boolean = True
                                If Not imgarg1 = "*" Then
                                    If procinfo.Path = vbNullString Then matchimage = False
                                End If

                                If matchimage = True Then
                                    Dim eventout As New EventOutput
                                    eventout.EventTime = New DateTime(datareader("Time")).ToLocalTime.AddYears(1600).ToString("yyyy-MM-dd hh:mm:ss tt")
                                    eventout.EventType = eventType
                                    eventout.FullPath = procinfo.Path
                                    eventout.ParentPID = procinfo.PPID
                                    eventout.PID = procinfo.PID
                                    eventout.Hash = datareader("Hash").ToString
                                    eventout.ImageBase = datareader("ImageBase").ToString
                                    eventout.ImageSize = datareader("ImageSize").ToString

                                    ' Debug.WriteLine(eventout)

                                    Console.WriteLine(eventout)
                                End If
                            Next
                        Catch ex As Exception
                            Console.Error.WriteLine("8 - " & ex.Message)
                        End Try

                    End While
                    imgcmd.Dispose()
                    datareader.Close()
                End If
            Next
            ImageConn.Close()
            ImageConn.Dispose()
            index -= 1
        End While

    End Sub
End Module
