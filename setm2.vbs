dim min
dim strCmd
dim Process
dim Reg
dim Net
dim FE
Dim Image
Dim FilterSearch(5)
If (WScript.Arguments.Count > 0) Then
	min = WScript.Arguments(0)
	Process = WScript.Arguments(1)
	Reg = WScript.Arguments(2)
	Net = WScript.Arguments(3)
	FE = WScript.Arguments(4)
	Image = WScript.Arguments(5)
	  strCmd = "ETM03.exe " & min & " " & Process & " " & Reg & " " & Net & " " & FE & " " & Image
		  FilterSearch(0) = WScript.Arguments(10)
		  FilterSearch(1) = WScript.Arguments(6)
		  FilterSearch(2) = WScript.Arguments(7)
		  FilterSearch(3) = WScript.Arguments(8)	
		  FilterSearch(4) = WScript.Arguments(9)
 
		 
End If

ZipFile="ETM03.zip"
ExtractTo="./"

Set fso = CreateObject("Scripting.FileSystemObject")
sourceFile = fso.GetAbsolutePathName(ZipFile)
destFolder = fso.GetAbsolutePathName(ExtractTo)
 
Set objShell = CreateObject("Shell.Application")
Set FilesInZip=objShell.NameSpace(sourceFile).Items()
objShell.NameSpace(destFolder).copyHere FilesInZip, 16
 
Set fso = Nothing
Set objShell = Nothing
Set FilesInZip = Nothing

dim FileName 
FileName = Array("ETM.csv","REGETM.csv","NETETM.csv","FILEETM.csv","IMAGEETM.csv")

Dim Counter
Counter = 0
Dim LineHolder
LineHolder = ""
Dim objShell


Set objShell = CreateObject("WScript.Shell")


Set WshShell = WScript.CreateObject("WScript.Shell") 
WshShell.run strCmd,1,true
WScript.Sleep(10000)

Set fso = CreateObject("Scripting.FileSystemObject")

for each file in FileName
	If fso.FileExists(file)Then		
	
		If FilterSearch(Counter) <> "*" Then
			Set f = fso.OpenTextFile(file)
			Do Until f.AtEndOfStream				
				LineHolder = f.ReadLine
				If InStr(1,LineHolder,FilterSearch(Counter),vbTextCompare) > 0 Then
					WScript.Echo LineHolder

				End If
			Loop
		f.Close
		Else
			Set f = fso.OpenTextFile(file)
			Do Until f.AtEndOfStream				
				WScript.Echo f.ReadLine	
	
			Loop
		f.Close
		End If
	End If
	
Counter = Counter + 1

next

Set fso = nothing



