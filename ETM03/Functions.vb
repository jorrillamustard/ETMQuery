Imports System.Data.SQLite
Imports System.IO

Module Functions

    Class EventOutput
        Sub New()

        End Sub

        Overrides Function ToString() As String
            Return Join({EventTime, EndTime, EventType, FullPath, RegistryPath, CurrentPID, ParentPID, PID, Overflowed,
            Hash, Username, CommandLine, Key, Data, AddressFamily, Protocol, LocalAddress, LocalPort, RemoteAddress, RemotePort, URL, FileAction, ImageBase, ImageSize}, ";"c)
        End Function
        Property EventTime As String
        Property EndTime As String
        Property EventType As String
        Property FullPath As String
        Property RegistryPath As String
        Property CurrentPID As String
        Property ParentPID As String
        Property PID As String
        Property Overflowed As String
        Property Hash As String
        Property Username As String
        Property CommandLine As String
        Property Key As String
        Property Data As String
        Property AddressFamily As String
        Property Protocol As String
        Property LocalAddress As String
        Property LocalPort As String
        Property RemoteAddress As String
        Property RemotePort As String
        Property URL As String
        Property FileAction As String
        Property ImageBase As String
        Property ImageSize As String

    End Class

    Class ParentProcessInfo
        Property Path As String
        Property PID As String
        Property PPID As String
        Sub New()

        End Sub
    End Class

    Function EventProcessInfo(ByVal admonpath As String, rowid As String, imagefilter As String) As ParentProcessInfo
        Dim rtninfo As New ParentProcessInfo
        Dim procconn As New SQLiteConnection("Data Source=" & admonpath)
        procconn.Open()
        Dim procquery As String
        Select Case imagefilter
            Case "*"
                procquery = "select * from ProcessEvent where rowid = '" & rowid & "'"

            Case Else
                procquery = "select * from ProcessEvent where rowid = '" & rowid & "' AND (FullPath LIKE '%" & imagefilter & "%' OR CommandLine LIKE '%" & imagefilter & "%')"
        End Select
        Dim proccmd As New SQLiteCommand(procquery, procconn)
        Dim procreader As SQLiteDataReader = proccmd.ExecuteReader()
        procreader.Read()
        rtninfo.Path = procreader("FullPath")
        rtninfo.PID = procreader("ProcessID")
        rtninfo.PPID = procreader("ParentID")
        procreader.Close()
        proccmd.Dispose()
        procconn.Close()

        Return rtninfo
    End Function

    Sub ExtractDLL()
        If IO.File.Exists("Support.exe") Then
            Dim psinfo As New ProcessStartInfo
            psinfo.FileName = "Support.exe"
            psinfo.Arguments = "-o .\ -y"
            psinfo.CreateNoWindow = True
            psinfo.WindowStyle = ProcessWindowStyle.Hidden

            Dim ext = Process.Start(psinfo)
            ext.WaitForExit()
        End If
    End Sub

    Function Get_TimeBeforeNow(ByVal TBefore As Integer)
        Try
            'Subtract 1600 years for ETM time, then the minutes in TBefore
            Dim rtn
            If TBefore > 0 Then
                rtn = Now.AddYears(-1600).Subtract(New TimeSpan(0, TBefore, 0))
            Else
                rtn = Now.AddYears(-1600)
            End If

            Debug.WriteLine("TimeBeforeNow = " & rtn.ToString("M/d/yy hh:mm:ss tt"))
            Return rtn.ToString("M/d/yy hh:mm:ss tt")
        Catch ex As Exception
            Return Now.AddYears(-1600).ToString("M/d/yy hh:mm:ss tt")
        End Try
    End Function

    Function Convert_TimeToTick(ByVal dt As DateTime)
        Try
            Debug.WriteLine("Time To Ticks = " & dt.Ticks)
            Return dt.Ticks
        Catch ex As Exception
            Return Now.Ticks
        End Try
    End Function

    Sub QueryETMDate(admonpath As String, TBefore As Integer)
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        Dim eventType = "Process Event"
        Dim conn As New SQLiteConnection("Data Source=" & admonpath)

        conn.Open()

        ' Dim query As String = "select * from ProcessEvent where FullPath LIKE '%" & ImageFilter & "%' OR CurrentProcessID LIKE '%" & Filter() & "%' OR ParentId LIKE '%" & Filter() & "%' OR ProcessId LIKE '%" & Filter() & "%' OR Hash LIKE '%" & Filter() & "%' OR UserName LIKE '%" & Filter() & "%' OR CommandLine LIKE '%" & Filter() & "%'"
        Dim query As String = "select * from ProcessEvent"
        If TBefore > 0 Then
            query = query & " where StartTime >= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
            Debug.WriteLine(query)
        Else
            query = query & " where StartTime <= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
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
        Dim SQLcmd1 As New SQLiteCommand(query, conn)
        Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()
        If datareader.HasRows Then
            While datareader.Read()

                Try
                    Dim d3 As Date = New DateTime(datareader("StartTime"))
                    Dim d4 As Date = New DateTime(datareader("EndTime"))
                    Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")

                    Dim eventout As New EventOutput
                    eventout.EventTime = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                    eventout.EndTime = d4.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                    eventout.EventType = eventType
                    eventout.FullPath = datareader("FullPath").ToString()
                    eventout.CurrentPID = datareader("CurrentProcessID")
                    eventout.ParentPID = datareader("ParentID").ToString()
                    eventout.PID = datareader("ProcessID").ToString()
                    eventout.Overflowed = datareader("OverFlowed").ToString()
                    eventout.Hash = datareader("Hash").ToString()
                    eventout.Username = datareader("UserName").ToString()
                    eventout.CommandLine = datareader("CommandLine").ToString()
                    Debug.WriteLine(eventout)
                    Console.WriteLine(eventout)


                Catch ex As Exception
                    Console.Error.WriteLine(ex.Message)
                    Continue While
                End Try
            End While

        End If
        conn.Close()

    End Sub
    Sub DateRegQuery(eventspath As String, admonpath As String, TBefore As Integer)
        Dim RegFilter = My.Application.CommandLineArgs(6)
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        Dim eventType = "Registry Event"
        Dim index As Integer = 0

        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(eventspath & dbname) Then
                'File here
            Else
                Exit While
            End If

            Dim rowCount = 0

            Dim regconn As New SQLiteConnection("Data Source=" & eventspath & dbname)
            regconn.Open()
            'Dim Query As String = "select * from Events where EventType = '0'"
            Dim Query As String = "select * from Events where EventType = '0'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            End If
            Select Case RegFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (Path LIKE '%" & RegFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)
            Dim SQLcmd1 As New SQLiteCommand(Query, regconn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try
                        ' Reached RegDateFunction while loop 1
                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")

                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)

                        If datareader("Path").ToString.Contains("\REGISTRY\") Then

                            Dim eventout As New EventOutput
                            eventout.EventTime = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                            eventout.EventType = eventType
                            eventout.FullPath = procinfo.Path
                            eventout.RegistryPath = datareader("Path").ToString
                            eventout.ParentPID = procinfo.PPID
                            eventout.PID = procinfo.PID
                            eventout.Key = datareader("Key").ToString
                            eventout.Data = datareader("Data").ToString

                            Debug.WriteLine(eventout)
                            Console.WriteLine(eventout)

                        End If
                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While

            End If

            regconn.Close()
            index += 1
        End While

    End Sub

    Sub DateNetworkQuery(eventspath As String, admonpath As String, TBefore As Integer)
        Dim eventType = "Network Event"
        Dim index As Integer = 0
        Dim NetFilter = My.Application.CommandLineArgs(7)
        Dim ImageFilter = My.Application.CommandLineArgs(9)

        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(eventspath & dbname) Then
                ' File is Here
            Else
                Exit While
            End If

            Dim rowCount = 0

            Dim netconn As New SQLiteConnection("Data Source=" & eventspath & dbname)

            netconn.Open()
            'Dim Query As String = "select * from Events where EventType = '1'"
            Dim Query As String = "select * from Events where EventType = '1'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            End If
            Select Case NetFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (LocalAddress LIKE '%" & NetFilter & "%' OR LocalPort LIKE '%" & NetFilter & "%' OR RemoteAddress LIKE '%" & NetFilter & "%' OR RemotePort LIKE '%" & NetFilter & "%' OR URL LIKE '%" & NetFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)

            Dim SQLcmd1 As New SQLiteCommand(Query, netconn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")

                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)

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
                            eventout.EventTime = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
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

                            Debug.WriteLine(eventout)
                            Console.WriteLine(eventout)

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While

            End If

            netconn.Close()
            index += 1
        End While

    End Sub

    Sub DateFileQuery(eventspath As String, admonpath As String, TBefore As Integer)
        Dim eventType = "File Event"
        Dim index As Integer = 0
        Dim FileFilter = My.Application.CommandLineArgs(8)
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(eventspath & dbname) Then
                ' Console.WriteLine("File is Here")
            Else
                Exit While
            End If

            Dim rowCount = 0

            Dim fileconn As New SQLiteConnection("Data Source=" & eventspath & dbname)

            fileconn.Open()
            'Dim Query As String = "select * from Events where EventType = '3'"
            Dim Query As String = "select * from Events where EventType = '3'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            End If
            Select Case FileFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (Path like '%" & FileFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)

            Dim SQLcmd1 As New SQLiteCommand(Query, fileconn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")


                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)

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
                        eventout.EventTime = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                        eventout.EventType = eventType
                        eventout.FullPath = procinfo.Path
                        eventout.ParentPID = procinfo.PPID
                        eventout.PID = procinfo.PID
                        eventout.FileAction = action
                        Debug.WriteLine(eventout)

                        Console.WriteLine(eventout)

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While

            End If

            fileconn.Close()
            index += 1
        End While

    End Sub

    Sub DateImageQuery(eventspath As String, admonpath As String, TBefore As Integer)
        Dim eventType = "Image Event"
        Dim index As Integer = 0
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(eventspath & dbname) Then
                ' Console.WriteLine("File is Here")
            Else
                Exit While
            End If

            Dim rowCount = 0
            ' Console.WriteLine(DataPath & dbname)
            Dim ImageConn As New SQLiteConnection("Data Source=" & eventspath & dbname)

            ImageConn.Open()

            ' Dim Query As String = "select * from Events where EventType = '2'"
            Dim Query As String = "select * from Events where EventType = '2'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & Convert_TimeToTick(Get_TimeBeforeNow(TBefore)) & "'"
                Debug.WriteLine(Query)
            End If
            Select Case ImageFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (Path like '%" & ImageFilter & "%' OR Hash like '%" & ImageFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)
            Dim SQLcmd1 As New SQLiteCommand(Query, ImageConn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")

                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)

                        Dim eventout As New EventOutput
                            eventout.EventTime = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                            eventout.EventType = eventType
                        eventout.FullPath = procinfo.Path
                        eventout.ParentPID = procinfo.PPID
                        eventout.PID = procinfo.PID
                        eventout.Hash = datareader("Hash").ToString
                            eventout.ImageBase = datareader("ImageBase").ToString
                            eventout.ImageSize = datareader("ImageSize").ToString

                            Debug.WriteLine(eventout)

                            Console.WriteLine(eventout)

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While

            End If

            ImageConn.Close()
            index += 1
        End While

    End Sub
End Module
