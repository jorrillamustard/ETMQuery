
Imports System.Data.SQLite
Imports System.IO

Module Module1

    Sub Main()
        Dim eventtype = ""
        Dim TimeBefore As Integer
        Dim DataPath As String = "C:\Program Files\Fidelis\Endpoint\Agent\config\admon.exe\admon.db"
        Dim DataPath2 As String = "C:\Program Files\Fidelis\Endpoint\Agent\config\admon.exe\"
        Dim ProcFilter, RegFilter, NetFilter, FileFilter, ImageFilter As String
        ProcFilter = My.Application.CommandLineArgs(6)
#If DEBUG Then
        DataPath = "..\..\..\SampleDB\admon.db"
        DataPath2 = "..\..\..\SampleDB\"
#End If

        Try

            TimeBefore = CInt(My.Application.CommandLineArgs(0))

            If My.Application.CommandLineArgs(1) = True Then
                QueryETMDate(DataPath, TimeBefore, ProcFilter)
            End If

            If My.Application.CommandLineArgs(2) = True Then
                DateRegQuery(DataPath2, DataPath, TimeBefore)
            End If

            If My.Application.CommandLineArgs(3) = True Then
                DateNetworkQuery(DataPath2, DataPath, TimeBefore)

            End If

            If My.Application.CommandLineArgs(4) = True Then
                DateFileQuery(DataPath2, DataPath, TimeBefore)
            End If

            If My.Application.CommandLineArgs(5) = True Then
                DateImageQuery(DataPath2, DataPath, TimeBefore)
            End If


        Catch ex As Exception
            Console.Error.WriteLine(ex.Message)
        End Try


    End Sub




    Private Sub QueryETMDate(DBPath As String, TBefore As Integer, Filter As String)
        Dim eventType = "Process Event"
        Dim conn As New SQLiteConnection("Data Source=" & DBPath)
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\ETM.csv"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False)


        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
        'Dim TickBefore As Long = (TBefore * 1200)
        'TickBefore = (ticktime - TickBefore)


        conn.Open()
        ' Dim Query As String = "select * from ProcessEvent "
        Dim query As String = "select * from ProcessEvent where FullPath LIKE '%" & Filter & "%' OR CurrentProcessID LIKE '%" & Filter & "%' OR ParentId LIKE '%" & Filter & "%' OR ProcessId LIKE '%" & Filter & "%' OR Hash LIKE '%" & Filter & "%' OR UserName LIKE '%" & Filter & "%' OR CommandLine LIKE '%" & Filter & "%'"
        Dim SQLcmd1 As New SQLiteCommand(Query, conn)
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
                    ' Console.WriteLine(Ltemp & ", " & ticktime)

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
                    '  outFile.WriteLine(line)
                Catch ex As Exception
                    ' Console.WriteLine(ex)
                    Continue While
                End Try
            End While
            outFile.Close()
        End If


        conn.Close()

    End Sub

    Private Sub DateRegQuery(DataPath As String, DataPath2 As String, TBefore As Integer)
        Dim eventType = "Registry Event"
        Dim index As Integer = 0
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\REGETM.csv"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False)

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh: mm : ss tt")

        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(DataPath & dbname) Then
                '   Console.WriteLine("File Is Here")
            Else
                Exit While
            End If

            Dim rowCount = 0
            ' Console.WriteLine(DataPath & dbname)
            Dim conn As New SQLiteConnection("Data Source=" & DataPath & dbname)
            Dim conn2 As New SQLiteConnection("Data Source=" & DataPath2)




            conn.Open()
            conn2.Open()
            Dim Query As String = "select * from Events where EventType = '0'"

        Dim SQLcmd1 As New SQLiteCommand(query, conn)

        Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

        If datareader.HasRows Then
            While datareader.Read()
                Try
                    ' Console.WriteLine("Reached RegDateFunction while loop 1")
                    Dim d3 As Date = New DateTime(datareader("Time"))
                    Dim Ltemp As String = d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt")
                    If TBefore > 0 Then
                        If CDate(Ltemp) < CDate(ticktime) Then
                            Continue While
                        End If
                    End If



                    rowCount = datareader("ProcessRow")
                    'Console.WriteLine(rowCount)
                    'Console.WriteLine(d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt"))

                    Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                    Dim ProcessName As String = ""
                    Dim PID As String = ""
                    Dim PPID As String = ""
                    Dim SQLcmd2 As New SQLiteCommand(Query2, conn2)
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


                            outFile.WriteLine(line1)
                        End If
                    End If

                Catch ex As Exception
                    ' Console.WriteLine(ex.ToString)
                End Try

            End While

        End If

        conn2.Close()
        conn.Close()
        Index += 1
        End While
        outFile.Close()
    End Sub

    Private Sub DateNetworkQuery(DataPath As String, DataPath2 As String, TBefore As Integer)
        Dim eventType = "Network Event"
        Dim index As Integer = 0
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\NETETM.csv"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False)

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(DataPath & dbname) Then
                ' Console.WriteLine("File is Here")
            Else
                Exit While
            End If

            Dim rowCount = 0
            ' Console.WriteLine(DataPath & dbname)
            Dim conn As New SQLiteConnection("Data Source=" & DataPath & dbname)
            Dim conn2 As New SQLiteConnection("Data Source=" & DataPath2)




            conn.Open()
            conn2.Open()
            Dim Query As String = "select * from Events where EventType = '1'"

            Dim SQLcmd1 As New SQLiteCommand(Query, conn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
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
                        'Console.WriteLine(rowCount)
                        'Console.WriteLine(d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt"))

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



                        Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, conn2)
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


                            outFile.WriteLine(line1)

                        End If

                    Catch ex As Exception
                        Console.WriteLine(ex.ToString)
                    End Try

                End While

            End If

            conn2.Close()
            conn.Close()
            index += 1
        End While
        outFile.Close()
    End Sub

    Private Sub DateFileQuery(DataPath As String, DataPath2 As String, TBefore As Integer)
        Dim eventType = "File Event"
        Dim index As Integer = 0
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\FILEETM.csv"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False)

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(DataPath & dbname) Then
                ' Console.WriteLine("File is Here")
            Else
                Exit While
            End If

            Dim rowCount = 0
            ' Console.WriteLine(DataPath & dbname)
            Dim conn As New SQLiteConnection("Data Source=" & DataPath & dbname)
            Dim conn2 As New SQLiteConnection("Data Source=" & DataPath2)




            conn.Open()
            conn2.Open()
            Dim Query As String = "select * from Events where EventType = '3'"

            Dim SQLcmd1 As New SQLiteCommand(Query, conn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
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
                        'Console.WriteLine(rowCount)
                        'Console.WriteLine(d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt"))                   

                        Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, conn2)
                        Dim datareader2 As SQLiteDataReader = SQLcmd2.ExecuteReader()


                        If datareader2.Read() Then

                            ProcessName = datareader2("FullPath").ToString
                            PID = datareader2("ProcessID").ToString
                            PPID = datareader2("ParentID").ToString
                            Dim action As String
                            If datareader("EventSubType") = 4 And datareader("isCreate") = 1 Then
                                action = "File Created"
                            ElseIf datareader("EventSubType") = 4 And datareader("isCreate") = 0 Then
                                action = "write"
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




                            outFile.WriteLine(line1)

                        End If

                    Catch ex As Exception
                        Console.WriteLine(ex.ToString)
                    End Try

                End While

            End If

            conn2.Close()
            conn.Close()
            index += 1
        End While
        outFile.Close()
    End Sub

    Private Sub DateImageQuery(DataPath As String, DataPath2 As String, TBefore As Integer)
        Dim eventType = "Image Event"
        Dim index As Integer = 0
        Dim csvFile As String = My.Application.Info.DirectoryPath & "\IMAGEETM.csv"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False)

        Dim ticktime As String = Now.Subtract(New TimeSpan(0, TBefore, 0)).ToString("M/d/yy hh:mm:ss tt")
        While index < 10
            Dim dbname As String = "events_"
            dbname = dbname & index.ToString & ".db"

            If File.Exists(DataPath & dbname) Then
                ' Console.WriteLine("File is Here")
            Else
                Exit While
            End If

            Dim rowCount = 0
            ' Console.WriteLine(DataPath & dbname)
            Dim conn As New SQLiteConnection("Data Source=" & DataPath & dbname)
            Dim conn2 As New SQLiteConnection("Data Source=" & DataPath2)




            conn.Open()
            conn2.Open()
            Dim Query As String = "select * from Events where EventType = '2'"

            Dim SQLcmd1 As New SQLiteCommand(Query, conn)

            Dim datareader As SQLiteDataReader = SQLcmd1.ExecuteReader()

            If datareader.HasRows Then
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
                        'Console.WriteLine(rowCount)
                        'Console.WriteLine(d3.ToLocalTime.ToString("M/d/yy hh:mm:ss tt"))                   

                        Dim Query2 As String = "SELECT * FROM ProcessEvent where rowid = " & rowCount
                        Dim ProcessName As String = ""
                        Dim PID As String = ""
                        Dim PPID As String = ""
                        Dim SQLcmd2 As New SQLiteCommand(Query2, conn2)
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





                            outFile.WriteLine(line1)

                        End If

                    Catch ex As Exception
                        Console.WriteLine(ex.ToString)
                    End Try

                End While

            End If

            conn2.Close()
            conn.Close()
            index += 1
        End While
        outFile.Close()
    End Sub

End Module
