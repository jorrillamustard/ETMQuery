Module Classes
    Class EventOutput
        Sub New()

        End Sub

        Overrides Function ToString() As String
            Dim paren = """"
            EventTime = paren & EventTime & paren
            EndTime = paren & EndTime & paren
            EventType = paren & EventType & paren
            FullPath = paren & FullPath & paren
            RegistryPath = paren & RegistryPath & paren
            CurrentPID = paren & CurrentPID & paren
            ParentPID = paren & ParentPID & paren
            PID = paren & PID & paren
            Overflowed = paren & Overflowed & paren
            Hash = paren & Hash & paren
            Username = paren & Username & paren
            CommandLine = paren & CommandLine & paren
            Key = paren & Key & paren
            Data = paren & Data & paren
            AddressFamily = paren & AddressFamily & paren
            Protocol = paren & Protocol & paren
            LocalAddress = paren & LocalAddress & paren
            LocalPort = paren & LocalPort & paren
            RemoteAddress = paren & RemoteAddress & paren
            RemotePort = paren & RemotePort & paren
            URL = paren & URL & paren
            FileAction = paren & FileAction & paren
            ImageBase = paren & ImageBase & paren
            ImageSize = paren & ImageSize & paren

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

    Class ProcThread
        Property AdmonPath
        Property TimeBefore
        Property TimeBeforeGreaterThan0
        Sub Start()
            QueryProcess(AdmonPath, TimeBefore, TimeBeforeGreaterThan0)
        End Sub
    End Class
    Class EventThread
        Property AdmonPath
        Property EventsPath
        Property TimeBefore
        Property TimeBeforeGreaterThan0
        Property EventType
        Sub Start()
            Select Case EventType
                Case 1 'Registry
                    QueryReg(EventsPath, AdmonPath, TimeBefore, TimeBeforeGreaterThan0)
                Case 2 'Network
                    QueryNetwork(EventsPath, AdmonPath, TimeBefore, TimeBeforeGreaterThan0)
                Case 3 'File
                    QueryFile(EventsPath, AdmonPath, TimeBefore, TimeBeforeGreaterThan0)
                Case 4 'Image
                    QueryImage(EventsPath, AdmonPath, TimeBefore, TimeBeforeGreaterThan0)
            End Select

        End Sub
    End Class




End Module
