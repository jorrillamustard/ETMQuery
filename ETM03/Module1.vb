
Imports System.Data.SQLite
Imports System.IO

Module Module1


    Sub Main()
        'Extract Support.exe with the needed DLLs
        ExtractDLL()

        Dim eventtype = ""
        Dim TimeBefore As Integer
        Dim DataPath As String = "C:\Program Files\Fidelis\Endpoint\Agent\config\admon.exe\admon.db"
        Dim DataPath2 As String = "C:\Program Files\Fidelis\Endpoint\Agent\config\admon.exe\"
        Dim ProcFilter, RegFilter, NetFilter, FileFilter, ImageFilter As String
        ProcFilter = My.Application.CommandLineArgs(6)
        '#If DEBUG Then
        '        DataPath = "..\..\..\SampleDB\admon.db"
        '        DataPath2 = "..\..\..\SampleDB\"
        '#End If

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


End Module
