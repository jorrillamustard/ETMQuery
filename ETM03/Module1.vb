

Module Module1

    Sub Main()
        'Extract Support.exe with the needed DLLs
        ExtractDLL()
        'Args List
        '0 - Time, 1 - Process, 2 - Registry, 3 - Network, 4 - File, 5 - Image
        '6 - RegFiter, 7 - NetFilter, 8 - FileFilter, 9 -ImageFilter, 10 - Multi-thread

        Dim eventtype = ""
        Dim TimeBefore As Integer
        Dim DataPath As String = "C:\Program Files\Fidelis\Endpoint\Agent\config\admon.exe\admon.db"
        Dim DataPath2 As String = "C:\Program Files\Fidelis\Endpoint\Agent\config\admon.exe\"

#If DEBUG Then
        DataPath = "..\..\..\SampleDB\admon.db"
        DataPath2 = "..\..\..\SampleDB\"
#End If

        Try

            TimeBefore = CInt(My.Application.CommandLineArgs(0))

            If My.Application.CommandLineArgs(1) = True Then
                If My.Application.CommandLineArgs(10) = True Then
                    Dim ProcThread As New ProcThread
                    ProcThread.DataPath = DataPath
                    ProcThread.TimeBefore = TimeBefore
                    Dim ProcThreading As New Threading.Thread(AddressOf ProcThread.Start)
                    ProcThreading.Start()
                Else
                    QueryETMDate(DataPath, TimeBefore)
                End If


            End If

            If My.Application.CommandLineArgs(2) = True Then
                If My.Application.CommandLineArgs(10) = True Then
                    Dim RegThread As New RegThread
                    RegThread.DataPath = DataPath
                    RegThread.DataPath2 = DataPath2
                    RegThread.TimeBefore = TimeBefore

                    Dim RegThreading As New Threading.Thread(AddressOf RegThread.Start)
                    RegThreading.Start()
                Else
                    DateRegQuery(DataPath2, DataPath, TimeBefore)
                End If

            End If

                If My.Application.CommandLineArgs(3) = True Then
                If My.Application.CommandLineArgs(10) = True Then

                    Dim NetThread As New NetThread
                    NetThread.DataPath = DataPath
                    NetThread.DataPath2 = DataPath2
                    NetThread.TimeBefore = TimeBefore

                    Dim NetThreading As New Threading.Thread(AddressOf NetThread.Start)
                    NetThreading.Start()
                Else
                    DateNetworkQuery(DataPath2, DataPath, TimeBefore)
                End If
            End If

            If My.Application.CommandLineArgs(4) = True Then
                If My.Application.CommandLineArgs(10) = True Then
                    Dim FileThread As New FileThread
                    FileThread.DataPath = DataPath
                    FileThread.DataPath2 = DataPath2
                    FileThread.TimeBefore = TimeBefore

                    Dim FileThreading As New Threading.Thread(AddressOf FileThread.Start)
                    FileThreading.Start()
                Else
                    DateFileQuery(DataPath2, DataPath, TimeBefore)
                End If

            End If

                If My.Application.CommandLineArgs(5) = True Then
                If My.Application.CommandLineArgs(10) = True Then
                    Dim ImageThread As New ImageThread
                    ImageThread.DataPath = DataPath
                    ImageThread.DataPath2 = DataPath2
                    ImageThread.TimeBefore = TimeBefore

                    Dim ImageThreading As New Threading.Thread(AddressOf ImageThread.Start)
                    ImageThreading.Start()
                Else

                    DateImageQuery(DataPath2, DataPath, TimeBefore)
                End If
            End If


        Catch ex As Exception
            Console.Error.WriteLine(ex.Message)
        End Try


    End Sub


End Module
