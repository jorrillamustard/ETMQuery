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

        'Subtract 1600 years for ETM time, then the minutes in TBefore
        Dim rtn As DateTime
            If TBefore > 0 Then
                rtn = Now.AddYears(-1600).Subtract(New TimeSpan(0, TBefore, 0))
            Else
                rtn = Now.AddYears(-1600)
            End If

        Debug.WriteLine("TimeBeforeNow = " & rtn.ToString("M/d/yyyy HH:mm:ss"))
        Return rtn.ToString("M/d/yyyy HH:mm:ss")

    End Function

    Function Convert_TimeToTick(ByVal dt As DateTime)

        Debug.WriteLine("Time To Ticks = " & dt.Ticks)
        Return dt.Ticks

    End Function

    Sub QueryETMDate(admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        Dim eventType = "Process Event"
        Dim conn As New SQLiteConnection("Data Source=" & admonpath & ";Read Only=True;")

        conn.Open()

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
        Dim conncmd As New SQLiteCommand(query, conn)
        Dim datareader As SQLiteDataReader = conncmd.ExecuteReader()
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
            datareader.Close()
            conncmd.Dispose()
        End If
        conn.Close()
        conn.Dispose()

    End Sub
    Sub DateRegQuery(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
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

            Dim regconn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")
            regconn.Open()
            'Dim Query As String = "select * from Events where EventType = '0'"
            Dim Query As String = "select * from Events where EventType = '0'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & TBefore & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & TBefore & "'"
                Debug.WriteLine(Query)
            End If
            Select Case RegFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (Path LIKE '%" & RegFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)
            Dim regcmd As New SQLiteCommand(Query, regconn)

            Dim datareader As SQLiteDataReader = regcmd.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try
                        ' Reached RegDateFunction while loop 1
                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")

                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)
                        Dim matchimage As Boolean = True
                        If Not ImageFilter = "*" Then
                            If procinfo.Path Is vbNullString Then matchimage = False
                        End If

                        If matchimage = True Then

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
                regcmd.Dispose()
                datareader.Close()

            End If

            regconn.Close()
            regconn.Dispose()
            index += 1
        End While

    End Sub

    Sub DateNetworkQuery(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
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

            Dim netconn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")

            netconn.Open()
            'Dim Query As String = "select * from Events where EventType = '1'"
            Dim Query As String = "select * from Events where EventType = '1'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & TBefore & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & TBefore & "'"
                Debug.WriteLine(Query)
            End If
            Select Case NetFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (LocalAddress LIKE '%" & NetFilter & "%' OR LocalPort LIKE '%" & NetFilter & "%' OR RemoteAddress LIKE '%" & NetFilter & "%' OR RemotePort LIKE '%" & NetFilter & "%' OR URL LIKE '%" & NetFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)

            Dim netcmd As New SQLiteCommand(Query, netconn)

            Dim datareader As SQLiteDataReader = netcmd.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try

                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)
                        Dim matchimage As Boolean = True
                        If Not ImageFilter = "*" Then
                            If procinfo.Path Is vbNullString Then matchimage = False
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

                            eventout.EventTime = New DateTime(datareader("Time")).ToString("M/d/yy hh:mm:ss tt")
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
                        End If
                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                netcmd.Dispose()
                datareader.Close()

            End If

            netconn.Close()
            netconn.Dispose()
            index += 1
        End While

    End Sub

    Sub DateFileQuery(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
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

            Dim fileconn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")

            fileconn.Open()
            'Dim Query As String = "select * from Events where EventType = '3'"
            Dim Query As String = "select * from Events where EventType = '3'"
            If TBefore > 0 Then
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

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")


                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)
                        Dim matchimage As Boolean = True
                        If Not ImageFilter = "*" Then
                            If procinfo.Path Is vbNullString Then matchimage = False
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
                            eventout.EventTime = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                            eventout.EventType = eventType
                            eventout.FullPath = procinfo.Path
                            eventout.ParentPID = procinfo.PPID
                            eventout.PID = procinfo.PID
                            eventout.FileAction = action
                            Debug.WriteLine(eventout)

                            Console.WriteLine(eventout)
                        End If
                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                filecmd.Dispose()
                datareader.Close()
            End If

            fileconn.Close()
            fileconn.Dispose()
            index += 1
        End While

    End Sub

    Sub DateImageQuery(eventspath As String, admonpath As String, TBefore As Long, TBeforeGreaterThan0 As Boolean)
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
            Dim ImageConn As New SQLiteConnection("Data Source=" & eventspath & dbname & ";Read Only=True;")

            ImageConn.Open()

            ' Dim Query As String = "select * from Events where EventType = '2'"
            Dim Query As String = "select * from Events where EventType = '2'"
            If TBefore > 0 Then
                Query = Query & " AND Time >= '" & TBefore & "'"
                Debug.WriteLine(Query)
            Else
                Query = Query & " AND Time <= '" & TBefore & "'"
                Debug.WriteLine(Query)
            End If
            Select Case ImageFilter
                Case "*"
                    'Do nothing

                Case Else
                    Query = Query & " AND (Path like '%" & ImageFilter & "%' OR Hash like '%" & ImageFilter & "%')"

            End Select
            Debug.WriteLine("Full Query = " & Query)
            Dim imgcmd As New SQLiteCommand(Query, ImageConn)

            Dim datareader As SQLiteDataReader = imgcmd.ExecuteReader()

            If datareader.HasRows Then

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")

                        rowCount = datareader("ProcessRow")
                        'Get parent process info
                        Dim procinfo As ParentProcessInfo = EventProcessInfo(admonpath, rowCount, ImageFilter)
                        Dim matchimage As Boolean = True
                        If Not ImageFilter = "*" Then
                            If procinfo.Path Is vbNullString Then matchimage = False
                        End If

                        If matchimage = True Then
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
                        End If
                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                imgcmd.Dispose()
                datareader.Close()
            End If

            ImageConn.Close()
            ImageConn.Dispose()
            index += 1
        End While

    End Sub
End Module
