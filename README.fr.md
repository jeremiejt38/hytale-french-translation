# Hytale — Pack de traduction française (fr-FR)

Traduction française communautaire pour **Hytale** (accès anticipé).

- **Version du jeu ciblée :** `0.6.8` (build 31)
- **Dernière mise à jour du pack :** 28/09/2026
- **Couverture :** `client.lang` complet — 3 484 clés traduites (menus, paramètres, ATH, outils de construction, social, rapports de bug, etc.)

## Installeur (.exe)

1. Va dans [Releases](https://github.com/jeremiejt38/hytale-french-translation/releases) et télécharge **`HytaleFrenchPatch.exe`** de la dernière release.
2. Exécute-le. Il va :
   - détecter ton installation Hytale,
   - lire la version du jeu dans `env.dat`,
   - récupérer la dernière version du patch compatible depuis ce repo,
   - installer `fr-FR` dans le dossier `Language` du jeu.
3. En jeu : **Settings → General → Language → Français**, puis retourne au menu principal.

L'installeur affiche la bannière officielle Hytale (Wikimedia) et les versions du jeu + patch.

## Installation manuelle

1. Trouve ton dossier Hytale, ex. `D:\Games\Hytale`.
2. Va dans `install\release\package\game\latest\Client\Data\Shared\Language\`.
3. Crée un dossier `fr-FR`.
4. Copie [`lang/fr-FR/client.lang`](lang/fr-FR/client.lang) et [`lang/fr-FR/meta.lang`](lang/fr-FR/meta.lang) dedans.
5. Lance le jeu → **Settings → General → Language → Français**.

## Désinstallation

Supprime le dossier `fr-FR` (ou bouton **Désinstaller** de l'installeur). Les autres langues ne sont pas touchées.

## Limites

- Une mise à jour du jeu peut effacer le dossier — réinstalle le pack après une update.
- Les textes venant des serveurs et les chaînes `[TMP]` restent en anglais.
- Une erreur de traduction ? Ouvre une issue !
