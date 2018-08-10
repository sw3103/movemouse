SET gacutil="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\gacutil.exe"
%gacutil% /u ComponentFactory.Krypton.Design
%gacutil% /u ComponentFactory.Krypton.Docking
%gacutil% /u ComponentFactory.Krypton.Navigator
%gacutil% /u ComponentFactory.Krypton.Ribbon
%gacutil% /u ComponentFactory.Krypton.Toolkit
%gacutil% /u ComponentFactory.Krypton.Workspace
%gacutil% /i ComponentFactory.Krypton.Design.dll
%gacutil% /i ComponentFactory.Krypton.Docking.dll
%gacutil% /i ComponentFactory.Krypton.Navigator.dll
%gacutil% /i ComponentFactory.Krypton.Ribbon.dll
%gacutil% /i ComponentFactory.Krypton.Toolkit.dll
%gacutil% /i ComponentFactory.Krypton.Workspace.dll
pause
