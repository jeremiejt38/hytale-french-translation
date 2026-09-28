# Hytale — French Translation Pack (fr-FR)

Community French translation for **Hytale** (early access).

- **Game version targeted:** `0.6.8` (build 31)
- **Pack last updated:** 2026-09-28
- **Coverage:** full `client.lang` — 3,484 keys translated (menus, settings, HUD, builder tools, social, feedback, etc.)

Hytale does not ship an official French locale yet. This pack adds a `fr-FR` language folder next to the official ones (`en-US`, `pt-BR`, …). No original file is modified — the pack is a pure addition and can be removed at any time.

> French version of this README: [README.fr.md](README.fr.md)

## Easy install (Windows GUI)

1. Download [`installer/Install-HytaleFrench.ps1`](installer/Install-HytaleFrench.ps1) (and `assets/logo.png` next to it if you want the banner image, otherwise it is fetched automatically).
2. Right-click → **Run with PowerShell** (or run it from a terminal).
3. The installer auto-detects your Hytale folder — confirm or browse to it, then click **Installer**.
4. In game: **Settings → General → Language → Français**, then return to the main menu.

One-liner from an elevated-optional PowerShell (downloads and runs the GUI):

```powershell
iwr https://raw.githubusercontent.com/jeremiejt38/hytale-french-translation/main/installer/Install-HytaleFrench.ps1 -OutFile "$env:TEMP\Install-HytaleFrench.ps1"; powershell -ExecutionPolicy Bypass -File "$env:TEMP\Install-HytaleFrench.ps1"
```

## Manual install

1. Locate your Hytale install, e.g. `D:\Games\Hytale`.
2. Go to `install\release\package\game\latest\Client\Data\Shared\Language\`.
3. Create a folder named `fr-FR`.
4. Copy [`lang/fr-FR/client.lang`](lang/fr-FR/client.lang) and [`lang/fr-FR/meta.lang`](lang/fr-FR/meta.lang) into it.
5. Launch the game → **Settings → General → Language → Français**.

## Uninstall

Delete the `fr-FR` folder (or use the **Désinstaller** button in the installer). Your other languages are untouched.

## Notes & limitations

- Game updates may wipe custom folders — reinstall the pack after an update.
- Server-provided strings (chat, dynamic item names) and `[TMP]` placeholder texts stay in English.
- Untranslated or wrong string? Open an issue — the pack targets game version `0.6.8`; keys missing after a game update will silently fall back to English.

## Contributing

`client.lang` uses Java-style `key = value` properties with ICU plural syntax (`{count, plural, one {...} other {...}}`), `{placeholder}` variables and Hytale markup (`<b>`, `<color is="#...">`). Keep keys and placeholders exactly as in `en-US` when editing.

Fan project — not affiliated with Hypixel Studios. `Hytale` and the Hytale logo are trademarks of Hypixel Inc.; `assets/` images are extracted from the game for the installer UI only.
