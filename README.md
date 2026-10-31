# RADUSELF PC Check

Free Windows desktop utility by RADUSELF. C# / Windows Forms, .NET 10, Romanian and English. Current version: **0.4.10**. No Microsoft account or Store installation is required for the portable EXE.

## Features

- On-demand RAM and local drive usage snapshot; no hardware diagnosis or antivirus scan.
- Keyboard shortcut and CMD/PowerShell reference with search and copy; catalog commands are not executed automatically.
- Desktop application inventory and links to Windows uninstall settings.
- Temporary-file analysis followed by explicit permanent-deletion confirmation. TEMP files modified within seven days are kept. Prefetch is optional and is not recommended as a routine speed optimization.
- Optional separate administrator window limited to Windows TEMP and Prefetch. No automatic deletion, permission changes, directory deletion or forced unlocking.
- Links to Windows settings, Windows Security, antivirus scan controls, firewall and Windows Update.
- Shutdown, restart and supported UEFI firmware restart requests, each with confirmation. Firmware restart requests administrator permission. No forced application closing.
- Support information; no bank details are embedded and no payment is initiated by the app.

## Build on Windows

Install the .NET 10 SDK, extract or clone this project, and open PowerShell in the directory containing the project file:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Build-Windows.ps1
.\dist\win-x64\RaduSelf.PCCheck.exe
```

The script publishes a self-contained x64 single-file EXE. The PDB is debugging information and is not required for users. Build outputs are excluded from source control. Run the portable EXE by double-clicking it. To remove it, close the app and delete the EXE; the optional language preference is stored under `%LOCALAPPDATA%\RaduSelfPCCheck`.

## Status and limitations

The maintainer has manually built and exercised version 0.4.10 on Windows. Additional device, scaling and regression testing is needed. Shutdown and firmware confirmation dialogs have been checked; actual power transitions have not been verified. No automated Windows test suite is included.

Cleanup button colors: navy means no fully completed cleanup; muted yellow means a complete analysis found no eligible files; green means files were deleted without skipped items. Windows settings buttons can turn green after a user's manual confirmation; this is not independent verification. States reset when the interface is rebuilt or restarted.

This app is not an antivirus or a guarantee of system health. The 90% usage threshold is guidance. Locked and inaccessible files are skipped. Cleaning Prefetch may temporarily slow application launches. Save work before cleanup or power actions.

## Code signing

The current downloadable EXE is unsigned. Windows may display an unknown publisher or SmartScreen warning. There is no SignPath approval or signed release yet. Publishing this repository does not automatically provide signing or SmartScreen reputation. Signing will require a separate provider evaluation and a verifiable build process.

## Privacy and security

See [PRIVACY.md](PRIVACY.md) for local data handling and [SECURITY.md](SECURITY.md) for reporting issues. No credentials, signing keys or build outputs belong in this repository.

## License

MIT — see [LICENSE](LICENSE). Project name and logos identify RADUSELF; do not imply that modified releases are official. Third-party platform names belong to their respective owners. The .NET runtime retains its upstream license notices when distributed.

---

## Română

Utilitar gratuit pentru Windows, în română și engleză. Varianta EXE este portabilă și nu cere cont Microsoft. Pentru compilare este necesar .NET 10 SDK; comenzile sunt mai sus. Curățarea cere confirmare și poate șterge definitiv fișierele eligibile. Nu este antivirus și nu execută automat comenzile din catalog. Sursa este publicată sub licența MIT. Versiunea curentă nu este semnată digital; aprobarea unui serviciu de semnare nu este încă obținută.
