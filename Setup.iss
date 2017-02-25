; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AppId=Edytor JPK
AppName=Edytor JPK
AppVerName=Edytor JPK
AppPublisher=JPK-Edytor
DefaultDirName={localappdata}\JPK
DefaultGroupName=Edytor JPK
OutputDir=C:\Projects\JPKEdytor
OutputBaseFilename=Instalator-EdytorJPK
Compression=lzma
SolidCompression=yes
UsePreviousAppDir=yes
PrivilegesRequired=lowest
;DisableDirPage=no
DisableProgramGroupPage=yes
DisableReadyMemo=yes
ChangesAssociations=yes
;InfoAfterFile = VersionInfo.txt
LicenseFile=LICENSE-PL.txt


[Languages]
Name: "pol"; MessagesFile: "compiler:Languages\Polish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "C:\Projects\JPKEdytor\bin\Release\JPKEdytor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Projects\JPKEdytor\bin\Release\Xceed.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Projects\JPKEdytor\bin\Release\System.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Projects\JPKEdytor\bin\Release\Microsoft.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Projects\JPKEdytor\bin\Release\Schema\*.xsd"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Projects\JPKEdytor\LICENSE-PL.txt"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{userprograms}\Edytor JPK"; Filename: "{app}\JPKEdytor.exe"
Name: "{userdesktop}\Edytor JPK"; Filename: "{app}\JPKEdytor.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\JPKEdytor.exe"; Description: "{cm:LaunchProgram,JPKEdytor}"; Flags: nowait postinstall

[Code]

function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, serviceCount: cardinal;
    success: boolean;
begin
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;
    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;
    // .NET 4.0 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;
    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4\Client', 0) then begin
        MsgBox('Program JPK Edytor wymaga Microsoft .NET Framework 4.'#13#13
            'Prosz�, zainstaluj obs�ug� .NET,'#13
            'a nast�pnie ponownie uruchom instalatora.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;
