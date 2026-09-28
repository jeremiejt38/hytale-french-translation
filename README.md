# Hytale — French Translation Pack (fr-FR)

Community French translation for **Hytale** (early access).

- **Game version targeted:** `0.6.8` (build 31)
- **Pack last updated:** 2026-09-28
- **Coverage:**
  - `client.lang` (3,484 keys) — all client UI/menus/settings/HUD
  - `server.lang` item names & descriptions (3,791 items + 331 descriptions)
  - avatar customization names (hair, faces, clothes, emotes, etc.)

Hytale does not ship an official French locale yet. This pack adds a `fr-FR` language folder for the client and patches the game `Assets.zip` so that items, workbenches and cosmetic names are translated too.

> French version of this README: [README.fr.md](README.fr.md)

## Installer (.exe)

1. Go to [Releases](https://github.com/jeremiejt38/hytale-french-translation/releases) and download **`HytaleFrenchPatch.exe`** for the latest release.
2. Run it. It will:
   - detect your Hytale installation,
   - read the game version from `env.dat`,
   - fetch the most recent compatible patch from this repository,
   - install the `fr-FR` client files into the game's `Language` folder,
   - patch `Assets.zip` to add French item / avatar names (with an automatic backup).
3. In game: **Settings → General → Language → Français**, then return to the main menu.

The installer uses the official Hytale banner from Wikimedia and shows the matched game/patch versions.

## Manual install

1. Locate your Hytale install, e.g. `D:\Games\Hytale`.
2. **Client language:** copy [`lang/fr-FR/client.lang`](lang/fr-FR/client.lang) and [`lang/fr-FR/meta.lang`](lang/fr-FR/meta.lang) into:
   `install\release\package\game\latest\Client\Data\Shared\Language\fr-FR\`
3. **Items / avatar names:** update `Assets.zip` with the files under [`assets-patch/`](assets-patch/) using 7-Zip or any zip tool. The paths inside the zip must match:
   - `Server/Languages/fr-FR/server.lang`
   - `Common/Languages/fr-FR/avatarCustomization/*.lang`

## Uninstall

Delete the `fr-FR` folder inside `Language` and remove the `fr-FR` entries from `Assets.zip`. The installer also provides a **Désinstaller** button that does this automatically.

## Notes & limitations

- Game updates may wipe custom folders — reinstall the pack after an update.
- Server-provided strings (chat, dynamic item names) and `[TMP]` placeholder texts stay in English.
- Item translations are generated automatically, so a few names may sound awkward. Pull requests to improve them are welcome.
- Untranslated or wrong string? Open an issue — the pack targets game version `0.6.8`; keys missing after a game update will silently fall back to English.

## Contributing

`.lang` files use Java-style `key = value` properties with ICU plural syntax (`{count, plural, one {...} other {...}}`), `{placeholder}` variables and Hytale markup (`<b>`, `<color is="#...">`). Keep keys and placeholders exactly as in `en-US` when editing.

Fan project — not affiliated with Hypixel Studios. `Hytale` and the Hytale logo are trademarks of Hypixel Inc.; the installer banner is loaded from Wikimedia.
