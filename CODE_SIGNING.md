# Code signing policy

## Current status

RADUSELF PC Check is not approved for SignPath Foundation signing. Current builds are unsigned. No certificate or signing credentials are configured. A successful GitHub Actions build is not a signed release and does not imply security certification.

## Build origin

The official repository is https://github.com/DeepDig14/raduself-pc-check. The `Build Windows portable EXE` workflow builds from this repository on GitHub-hosted Windows runners, publishes an unsigned portable EXE and records its source commit and SHA-256 checksum. Workflow artifacts expire after 30 days and are not permanent releases.

## Planned signing scope

If a signing provider approves the project, only reviewed official builds of RADUSELF PC Check produced from this repository will be submitted. Signing credentials must remain in protected secrets, never in source code. Production signing requests require explicit maintainer approval. Pull requests and untrusted forks must not receive signing credentials.

The maintainer is the GitHub account DeepDig14. Two-factor authentication must be enabled for all accounts with signing authority before signing is configured. No claim is made here that account security has been verified.

Provider-specific configuration and required acknowledgements will be added only after acceptance. The published signed EXE must be verified after signing and before distribution; its checksum must be generated after signing.

## Privacy and contact

See [PRIVACY.md](PRIVACY.md) and [SECURITY.md](SECURITY.md). No private signing keys, personal bank details or identity documents belong in the public repository.
