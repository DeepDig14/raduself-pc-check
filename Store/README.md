# Microsoft Store submission candidate

Run `./Build-Store.ps1` on Windows with .NET 10 SDK and Windows SDK installed. Output: `dist/store/RADUSELF-PC-Check-Store-x64.msix`.

The package is unsigned for Partner Center submission. Microsoft signs accepted MSIX submissions. Do not offer this unsigned package as a sideload download. The Store identity must match Partner Center exactly before upload.

This edition defines STORE_BUILD: administrator cleanup and direct UEFI restart are unavailable. User TEMP cleanup retains confirmation and age/access checks; packaged TEMP may be redirected to package-local storage. Windows storage and recovery settings remain available. The portable build is unchanged.

The manifest requests runFullTrust for the existing WinForms desktop application, with no allowElevation capability. Explain this desktop-app capability in Partner Center if requested. Certification and packaged runtime testing are still required. Test language persistence, Windows links, cleanup boundaries, resource/process inventory, and uninstall using a separately signed local test package; never distribute a test certificate as a public trust solution.

Confirm the privacy URL, age ratings, screenshots, descriptions and restricted-capability explanation in Partner Center. No Store certification approval is implied by a successful package build.
