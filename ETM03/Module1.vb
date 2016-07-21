

Module Module1

    Sub Main()
        'Extract Support.exe with the needed DLLs
        ExtractDLL()
        'Args List
        '0 - Time, 1 - Process, 2 - Registry, 3 - Network, 4 - File, 5 - Image
        '6 - RegFiter, 7 - NetFilter, 8 - FileFilter, 9 -ImageFilter

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
                QueryETMDate(DataPath, TimeBefore)
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
