# Hytale — French Translation Pack (fr-FR)

Community French translation for **Hytale** (early access).

- **Game version targeted:** `0.6.8` (build 31)
- **Pack last updated:** 2026-09-28
- **Coverage:** full `client.lang` — 3,484 keys translated (menus, settings, HUD, builder tools, social, feedback, etc.)

Hytale does not ship an official French locale yet. This pack adds a `fr-FR` language folder next to the official ones (`en-US`, `pt-BR`, …). No original file is modified — the pack is a pure addition and can be removed at any time.

> French version of this README: [README.fr.md](README.fr.md)

## Installer (.exe)

1. Go to [Releases](https://github.com/jeremiejt38/hytale-french-translation/releases) and download **`HytaleFrenchPatch.exe`** for the latest release.
2. Run it. It will:
   - detect your Hytale installation,
   - read the game version from `env.dat`,
   - fetch the most recent compatible patch from this repository,
   - install `fr-FR` into the game's `Language` folder.
3. In game: **Settings → General → Language → Français**, then return to the main menu.

The installer uses the official Hytale banner from Wikimedia and shows the matched game/patch versions.

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

Fan project — not affiliated with Hypixel Studios. `Hytale` and the Hytale logo are trademarks of Hypixel Inc.; the installer banner is loaded from Wikimedia.
