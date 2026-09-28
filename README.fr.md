# Hytale — Pack de traduction française (fr-FR)

Traduction française communautaire pour **Hytale** (accès anticipé).

- **Version du jeu ciblée :** `0.6.8` (build 31)
- **Dernière mise à jour du pack :** 28/09/2026
- **Couverture :** `client.lang` complet — 3 484 clés traduites (menus, paramètres, ATH, outils de construction, social, rapports de bug, etc.)

## Installation facile (fenêtre graphique)

1. Téléchargez [`installer/Install-HytaleFrench.ps1`](installer/Install-HytaleFrench.ps1).
2. Clic droit → **Exécuter avec PowerShell**.
3. L'installeur détecte automatiquement le dossier Hytale — vérifiez-le ou parcourez, puis cliquez sur **Installer**.
4. En jeu : **Settings → General → Language → Français**, puis retournez au menu principal.

Ou en une ligne dans PowerShell :

```powershell
iwr https://raw.githubusercontent.com/jeremiejt38/hytale-french-translation/main/installer/Install-HytaleFrench.ps1 -OutFile "$env:TEMP\Install-HytaleFrench.ps1"; powershell -ExecutionPolicy Bypass -File "$env:TEMP\Install-HytaleFrench.ps1"
```

## Installation manuelle

1. Trouvez votre dossier Hytale, ex. `D:\Games\Hytale`.
2. Allez dans `install\release\package\game\latest\Client\Data\Shared\Language\`.
3. Créez un dossier `fr-FR`.
4. Copiez [`lang/fr-FR/client.lang`](lang/fr-FR/client.lang) et [`lang/fr-FR/meta.lang`](lang/fr-FR/meta.lang) dedans.
5. Lancez le jeu → **Settings → General → Language → Français**.

## Désinstallation

Supprimez le dossier `fr-FR` (ou bouton **Désinstaller** de l'installeur). Les autres langues ne sont pas touchées.

## Limites

- Une mise à jour du jeu peut effacer le dossier — réinstallez le pack après une update.
- Les textes venant des serveurs et les chaînes `[TMP]` restent en anglais.
- Une erreur de traduction ? Ouvrez une issue !
