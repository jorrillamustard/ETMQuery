Imports System.Data.SQLite
Imports System.IO

Module Functions
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

    Sub QueryETMDate(admonpath As String, TBefore As Integer)
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        Dim eventType = "Process Event"
        Dim conn As New SQLiteConnection("Data Source=" & admonpath)

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")

        conn.Open()

        ' Dim query As String = "select * from ProcessEvent where FullPath LIKE '%" & ImageFilter & "%' OR CurrentProcessID LIKE '%" & Filter() & "%' OR ParentId LIKE '%" & Filter() & "%' OR ProcessId LIKE '%" & Filter() & "%' OR Hash LIKE '%" & Filter() & "%' OR UserName LIKE '%" & Filter() & "%' OR CommandLine LIKE '%" & Filter() & "%'"
        Dim query As String = "select * from ProcessEvent where FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%'"
        Dim SQLcmd1 As New SQLiteCommand(query, conn)
        Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()
        If datareader.HasRows Then
            While datareader.Read()

                Try
                    Dim d3 As Date = New DateTime(datareader("StartTime"))
                    Dim d4 As Date = New DateTime(datareader("EndTime"))
                    Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hhmm : ss tt")

                    If TBefore > 0 Then
                        If CDate(Ltemp) < CDate(ticktime) Then
                            Continue While
                        End If
                    End If

                    Dim line As String = (d3.ToLocalTime.ToString("M/d/yy hh: mm : ss tt") & ";" &
                              d4.ToLocalTime.ToString("M/d/yy hh: mm : ss tt") & ";" &
                              eventType & ";" &
                              datareader("FullPath").ToString() & ";" &
                              "" & ";" &
                              datareader("CurrentProcessID") & ";" &
                              datareader("ParentID").ToString() & ";" &
                              datareader("ProcessID").ToString() & ";" &
                              datareader("OverFlowed").ToString() & ";" &
                              datareader("Hash").ToString() & ";" &
                              datareader("UserName").ToString() & ";" &
                              datareader("CommandLine").ToString() & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "" & ";" &
                              "")
                    Console.WriteLine(line)
                    Debug.WriteLine(line)

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

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh: mm : ss tt")

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
            Dim procconn As New SQLiteConnection("Data Source=" & admonpath)


            regconn.Open()
            'Dim Query As String = "select * from Events where EventType = '0'"
            Dim Query As String = "select * from Events where EventType = '0' AND Path LIKE '%" & RegFilter & "%'"

            Dim SQLcmd1 As New SQLiteCommand(Query, regconn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
                procconn.Open()
                While datareader.Read()
                    Try
                        ' Reached RegDateFunction while loop 1
                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                        If TBefore > 0 Then
                            If CDate(Ltemp) < CDate(ticktime) Then
                                Continue While
                            End If
                        End If

                        rowCount = datareader("ProcessRow")

                        'Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim Query2 As String = "select * from ProcessEvent where rowid = '" & rowCount & "' AND FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%'"

                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, procconn)
                        Dim datareader2 As SQLiteDataReader = SQLcmd2.ExecuteReader()

                        If datareader2.Read() Then
                            If datareader("Path").ToString.Contains("\REGISTRY\") Then

                                ProcessName = datareader2("FullPath").ToString
                                PID = datareader2("ProcessID").ToString
                                PPID = datareader2("ParentID").ToString


                                Dim line1 As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt") & ";" & 'Start Time
                                      "" & ";" &                                                            'End Time
                                      eventType & ";" &
                                      ProcessName & ";" &                                                   'Path
                                      datareader("Path").ToString & ";" &                                   'RegPath    
                                      "" & ";" &                                                            'Current PID
                                      PPID & ";" &                                                          'Parent PID
                                      PID & ";" &                                                           'PID
                                      "" & ";" &                                                            'Overflowed
                                      "" & ";" &                                                            'Hash
                                      "" & ";" &                                                            'UserName
                                      "" & ";" &                                                            'Command
                                      datareader("Key").ToString & ";" &                                    'key
                                      datareader("data").ToString & ";" &                                           'data
                                      "" & ";" &
                                      "" & ";" &
                                      "" & ";" &
                                      "" & ";" &
                                      "" & ";" &
                                      "" & ";" &
                                      "" & ";" &
                                      "" & ";" &
                                      ""

                                Console.WriteLine(line1)

                            End If
                        End If

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                procconn.Close()
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

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
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
            Dim procconn As New SQLiteConnection("Data Source=" & admonpath)

            netconn.Open()
            'Dim Query As String = "select * from Events where EventType = '1'"
            Dim Query As String = "select * from Events where EventType = '1' AND LocalAddress = '%" & NetFilter & "%' OR LocalPort = '%" & NetFilter & "%' OR RemoteAddress = '%" & NetFilter & "%' OR RemotePort = '%" & NetFilter & "%'"

            Dim SQLcmd1 As New SQLiteCommand(Query, netconn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
                procconn.Open()

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                        If TBefore > 0 Then
                            If CDate(Ltemp) < CDate(ticktime) Then
                                Continue While
                            End If
                        End If


                        rowCount = datareader("ProcessRow")

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

                        ' Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim Query2 As String = "select * from ProcessEvent where rowid = '" & rowCount & "' AND FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%'"
                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, procconn)
                        Dim datareader2 As SQLiteDataReader = SQLcmd2.ExecuteReader()

                        If datareader2.Read() Then

                            ProcessName = datareader2("FullPath").ToString
                            PID = datareader2("ProcessID").ToString
                            PPID = datareader2("ParentID").ToString


                            Dim line1 As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt") & ";" & 'Start Time
                                  "" & ";" &                                                            'End Time
                                  eventType & ";" &
                                  ProcessName & ";" &                                                   'Path
                                  datareader("Path").ToString & ";" &                                   'RegPath    
                                  "" & ";" &                                                            'Current PID
                                  PPID & ";" &                                                          'Parent PID
                                  PID & ";" &                                                           'PID
                                  "" & ";" &                                                            'Overflowed
                                  "" & ";" &                                                            'Hash
                                  "" & ";" &                                                            'UserName
                                  "" & ";" &                                                            'Command
                                  "" & ";" &                                                            'key
                                  "" & ";" &                                                            'data
                                  AddressFamily & ";" &                                                 'AddressFamily
                                  protocol & ";" &                                                      'Protocol
                                  datareader("LocalAddress").ToString & ";" &                           'Local Address
                                  datareader("LocalPort").ToString & ";" &                              'Local Port
                                  datareader("RemoteAddress").ToString & ";" &                          'Remote Address
                                  datareader("RemotePort").ToString & ";" &                                    'Remote Port
                                  "" & ";" &
                                  ""
                            Console.WriteLine(line1)

                        End If

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                procconn.Close()
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
        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
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
            Dim procconn As New SQLiteConnection("Data Source=" & admonpath)

            fileconn.Open()
            'Dim Query As String = "select * from Events where EventType = '3'"
            Dim Query As String = "select * from Events where EventType = '3' AND Path like '%" & FileFilter & "%'"

            Dim SQLcmd1 As New SQLiteCommand(Query, fileconn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
                procconn.Open()

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                        If TBefore > 0 Then
                            If CDate(Ltemp) < CDate(ticktime) Then
                                Continue While
                            End If
                        End If

                        rowCount = datareader("ProcessRow")

                        '  Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim Query2 As String = "select * from ProcessEvent where rowid = '" & rowCount & "' AND FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%'"
                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, procconn)
                        Dim datareader2 As SQLiteDataReader = SQLcmd2.ExecuteReader()


                        If datareader2.Read() Then

                            ProcessName = datareader2("FullPath").ToString
                            PID = datareader2("ProcessID").ToString
                            PPID = datareader2("ParentID").ToString
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


                            Dim line1 As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt") & ";" & 'Start Time
                                  "" & ";" &                                                            'End Time
                                  eventType & ";" &
                            ProcessName & ";" &                                                   'Path
                                  datareader("Path").ToString & ";" &                                   'RegPath    
                                  "" & ";" &                                                            'Current PID
                                  PPID & ";" &                                                          'Parent PID
                                  PID & ";" &                                                           'PID
                                  "" & ";" &                                                            'Overflowed
                                  "" & ";" &                                                            'Hash
                                  "" & ";" &                                                            'UserName
                                  "" & ";" &                                                            'Command
                                  "" & ";" &                                                            'key
                                  "" & ";" &                                                            'data
                                  "" & ";" &                                                            'AddressFamily
                                  "" & ";" &                                                            'Protocol
                                  "" & ";" &                                                            'Local Address
                                  "" & ";" &                                                            'Local Port
                                  "" & ";" &                                                            'Remote Address
                                  "" & ";" &                                                            'Remote Port
                                  action & ";" &                                                                'File Action'
                                  "" & ";" &
                                  ""

                            Console.WriteLine(line1)

                        End If

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                procconn.Close()
            End If

            fileconn.Close()
            index += 1
        End While

    End Sub

    Sub DateImageQuery(eventspath As String, admonpath As String, TBefore As Integer)
        Dim eventType = "Image Event"
        Dim index As Integer = 0
        Dim ImageFilter = My.Application.CommandLineArgs(9)
        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
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
            Dim ProcConn As New SQLiteConnection("Data Source=" & admonpath)

            ImageConn.Open()

            ' Dim Query As String = "select * from Events where EventType = '2'"
            Dim Query As String = "select * from Events where EventType = '2' AND Path like '%" & ImageFilter & "%' OR Hash like '%" & ImageFilter & "%'"
            Dim SQLcmd1 As New SQLiteCommand(Query, ImageConn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
                ProcConn.Open()

                While datareader.Read()
                    Try

                        Dim d3 As Date = New DateTime(datareader("Time"))
                        Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                        If TBefore > 0 Then
                            If CDate(Ltemp) < CDate(ticktime) Then
                                Continue While
                            End If
                        End If

                        rowCount = datareader("ProcessRow")

                        'Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim Query2 As String = "select * from ProcessEvent where rowid = '" & rowCount & "' AND FullPath LIKE '%" & ImageFilter & "%' OR CommandLine LIKE '%" & ImageFilter & "%'"
                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, ProcConn)
                        Dim datareader2 As SQLiteDataReader = SQLcmd2.ExecuteReader()


                        If datareader2.Read() Then

                            ProcessName = datareader2("FullPath").ToString
                            PID = datareader2("ProcessID").ToString
                            PPID = datareader2("ParentID").ToString


                            Dim line1 As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt") & ";" & 'Start Time
                                  "" & ";" &                                                            'End Time
                                  eventType & ";" &
                            ProcessName & ";" &                                                   'Path
                                  datareader("Path").ToString & ";" &                                   'RegPath    
                                  "" & ";" &                                                            'Current PID
                                  PPID & ";" &                                                          'Parent PID
                                  PID & ";" &                                                           'PID
                                  "" & ";" &                                                            'Overflowed
                                  datareader("Hash").ToString & ";" &                                   'Hash
                                  "" & ";" &                                                            'UserName
                                  "" & ";" &                                                            'Command
                                  "" & ";" &                                                            'key
                                  "" & ";" &                                                            'data
                                  "" & ";" &                                                            'AddressFamily
                                  "" & ";" &                                                            'Protocol
                                  "" & ";" &                                                            'Local Address
                                  "" & ";" &                                                            'Local Port
                                  "" & ";" &                                                            'Remote Address
                                  "" & ";" &                                                            'Remote Port
                                  "" & ";" &                                                            'File Action'
                                  datareader("ImageBase").ToString & ";" &                              'Image Base'
                                  datareader("ImageSize").ToString                                      'Image Size'

                            Console.WriteLine(line1)

                        End If

                    Catch ex As Exception
                        Console.Error.WriteLine(ex.Message)
                    End Try

                End While
                ProcConn.Close()
            End If

            ImageConn.Close()
            index += 1
        End While

    End Sub
End Module
