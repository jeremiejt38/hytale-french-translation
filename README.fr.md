# Hytale — Pack de traduction française (fr-FR)

Traduction française communautaire pour **Hytale** (accès anticipé).

- **Version du jeu ciblée :** `0.6.8` (build 31)
- **Dernière mise à jour du pack :** 28/09/2026
- **Couverture :**
  - `client.lang` (3 484 clés) — toute l'interface, menus, paramètres, ATH
  - `server.lang` : noms et descriptions des objets (3 791 objets + 331 descriptions)
  - noms des cosmétiques / personnalisation de l'avatar

## Installeur (.exe)

1. Va dans [Releases](https://github.com/jeremiejt38/hytale-french-translation/releases) et télécharge **`HytaleFrenchPatch.exe`** de la dernière release.
2. Exécute-le. Il va :
   - détecter ton installation Hytale,
   - lire la version du jeu dans `env.dat`,
   - récupérer la dernière version du patch compatible depuis ce repo,
   - installer les fichiers `fr-FR` dans le dossier `Language`,
   - patcher `Assets.zip` pour ajouter les noms d'objets et d'avatar (avec sauvegarde automatique).
3. En jeu : **Settings → General → Language → Français**, puis retourne au menu principal.

L'installeur affiche la bannière officielle Hytale (Wikimedia) et les versions du jeu + patch.

## Installation manuelle

1. Trouve ton dossier Hytale, ex. `D:\Games\Hytale`.
2. **Langue client :** copie [`lang/fr-FR/client.lang`](lang/fr-FR/client.lang) et [`lang/fr-FR/meta.lang`](lang/fr-FR/meta.lang) dans :
   `install\release\package\game\latest\Client\Data\Shared\Language\fr-FR\`
3. **Objets / avatar :** mets à jour `Assets.zip` avec les fichiers sous [`assets-patch/`](assets-patch/). Les chemins à l'intérieur du zip doivent être :
   - `Server/Languages/fr-FR/server.lang`
   - `Common/Languages/fr-FR/avatarCustomization/*.lang`

## Désinstallation

Supprime le dossier `fr-FR` dans `Language` et retire les entrées `fr-FR` de `Assets.zip`. L'installeur propose aussi un bouton **Désinstaller**.

## Limites

- Une mise à jour du jeu peut effacer le dossier — réinstalle le pack après une update.
- Les textes venant des serveurs et les chaînes `[TMP]` restent en anglais.
- Les noms d'objets sont traduits automatiquement : certains peuvent être un peu bizarres. Les PR pour améliorer sont les bienvenues.
- Une erreur de traduction ? Ouvre une issue !
