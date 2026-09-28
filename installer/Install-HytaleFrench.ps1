#requires -Version 5.1
<#
.SYNOPSIS
    Hytale French Translation Pack - GUI Installer
.DESCRIPTION
    Installs the community French (fr-FR) language pack into Hytale.
    Auto-detects the game installation, downloads the latest translation
    from GitHub and copies it into the game's Language folder.
.NOTES
    Translation built for Hytale 0.6.8 (build 31) - early access.
#>

Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Windows.Forms

$Script:RepoRaw      = 'https://raw.githubusercontent.com/jeremiejt38/hytale-french-translation/main'
$Script:PackVersion  = '0.6.8 (build 31)'
$Script:PackDate     = '2026-09-28'
$Script:GameLanguage = 'fr-FR'

# ---------------------------------------------------------------------------
# XAML UI
# ---------------------------------------------------------------------------
$xaml = @'
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Hytale - Traduction Francaise" Height="560" Width="640"
        WindowStartupLocation="CenterScreen" ResizeMode="NoResize"
        Background="#FF0E1526" FontFamily="Segoe UI">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="170"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="70"/>
        </Grid.RowDefinitions>

        <!-- Banner -->
        <Border Grid.Row="0" Background="#FF131B30">
            <Grid>
                <Image x:Name="BannerImage" Stretch="Uniform" Margin="20,10"/>
                <StackPanel VerticalAlignment="Bottom" HorizontalAlignment="Right" Margin="12">
                    <TextBlock x:Name="PackVersionLabel" Text="" Foreground="#FF9FB4D8"
                               FontSize="12" HorizontalAlignment="Right"/>
                    <TextBlock x:Name="PackDateLabel" Text="" Foreground="#FF5E6E8F"
                               FontSize="11" HorizontalAlignment="Right"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Body -->
        <StackPanel Grid.Row="1" Margin="24,14">
            <TextBlock Text="Traduction francaise communautaire pour Hytale"
                       Foreground="White" FontSize="18" FontWeight="SemiBold"/>
            <TextBlock Text="Ce patch installe le pack de langue fr-FR dans le dossier Language du jeu."
                       Foreground="#FFB8C4DC" FontSize="12" Margin="0,6,0,14" TextWrapping="Wrap"/>

            <TextBlock Text="Dossier d'installation de Hytale :" Foreground="#FF9FB4D8" FontSize="12"/>
            <Grid Margin="0,6,0,0">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <TextBox x:Name="PathBox" Grid.Column="0" Height="30" VerticalContentAlignment="Center"
                         Background="#FF1A2438" Foreground="White" BorderBrush="#FF2E3D5C"
                         Padding="8,0" FontSize="12"/>
                <Button x:Name="BrowseButton" Grid.Column="1" Content="Parcourir..." Width="100"
                        Height="30" Margin="8,0,0,0" Background="#FF2E3D5C" Foreground="White"
                        BorderThickness="0" FontSize="12" Cursor="Hand"/>
            </Grid>
            <TextBlock x:Name="PathStatus" Text="" Foreground="#FF6FCF97" FontSize="11" Margin="0,6,0,0" TextWrapping="Wrap"/>

            <TextBlock x:Name="InfoText" Margin="0,14,0,0" TextWrapping="Wrap" Foreground="#FF8A97B5" FontSize="11"
                       Text="Apres l'installation : lancez Hytale, ouvrez Settings, puis General, puis Language et selectionnez Francais. Retournez au menu principal pour appliquer."/>
        </StackPanel>

        <!-- Footer -->
        <Border Grid.Row="2" Background="#FF0A0F1C">
            <Grid Margin="24,0">
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" VerticalAlignment="Center">
                    <TextBlock x:Name="StatusText" Text="Pret." Foreground="#FF9FB4D8" FontSize="12"/>
                </StackPanel>
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Center">
                    <Button x:Name="UninstallButton" Content="Desinstaller" Width="110" Height="34"
                            Margin="0,0,10,0" Background="#FF3A2430" Foreground="#FFE08A8A"
                            BorderThickness="0" FontSize="12" Cursor="Hand"/>
                    <Button x:Name="InstallButton" Content="Installer" Width="140" Height="34"
                            Background="#FF3D7BEF" Foreground="White" BorderThickness="0"
                            FontSize="13" FontWeight="SemiBold" Cursor="Hand"/>
                </StackPanel>
            </Grid>
        </Border>
    </Grid>
</Window>
'@

$reader = New-Object System.Xml.XmlNodeReader ([xml]$xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

$BannerImage      = $window.FindName('BannerImage')
$PackVersionLabel = $window.FindName('PackVersionLabel')
$PackDateLabel    = $window.FindName('PackDateLabel')
$PathBox          = $window.FindName('PathBox')
$BrowseButton     = $window.FindName('BrowseButton')
$PathStatus       = $window.FindName('PathStatus')
$StatusText       = $window.FindName('StatusText')
$InstallButton    = $window.FindName('InstallButton')
$UninstallButton  = $window.FindName('UninstallButton')

$PackVersionLabel.Text = "Pack pour Hytale $Script:PackVersion"
$PackDateLabel.Text    = "Mise a jour du pack : $Script:PackDate"

# ---------------------------------------------------------------------------
# Banner image: local assets\logo.png, otherwise downloaded once to temp
# ---------------------------------------------------------------------------
function Set-Banner {
    $candidates = @(
        (Join-Path $PSScriptRoot '..\assets\logo.png'),
        (Join-Path $PSScriptRoot 'logo.png'),
        (Join-Path $env:TEMP 'hytale-fr-logo.png')
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { $BannerImage.Source = $c; return }
    }
    try {
        $tmp = Join-Path $env:TEMP 'hytale-fr-logo.png'
        Invoke-WebRequest "$Script:RepoRaw/assets/logo.png" -OutFile $tmp -UseBasicParsing -TimeoutSec 15
        $BannerImage.Source = $tmp
    } catch { }
}
Set-Banner

# ---------------------------------------------------------------------------
# Install path detection
# ---------------------------------------------------------------------------
function Test-GamePath($root) {
    if ([string]::IsNullOrWhiteSpace($root)) { return $null }
    $lang = Join-Path $root 'install\release\package\game\latest\Client\Data\Shared\Language'
    if (Test-Path $lang) { return $lang }
    # also accept a path already pointing inside the package
    $lang2 = Join-Path $root 'Client\Data\Shared\Language'
    if (Test-Path $lang2) { return $lang2 }
    return $null
}

function Find-Hytale {
    $candidates = @()
    foreach ($d in [IO.DriveInfo]::GetDrives() | Where-Object { $_.IsReady -and $_.DriveType -eq 'Fixed' }) {
        $candidates += Join-Path $d.RootDirectory 'Games\Hytale'
        $candidates += Join-Path $d.RootDirectory 'Hytale'
        $candidates += Join-Path $d.RootDirectory 'Program Files\Hytale'
    }
    $candidates += Join-Path $env:LOCALAPPDATA 'Programs\Hypixel Studios\Hytale'
    foreach ($c in $candidates) {
        $lang = Test-GamePath $c
        if ($lang) { return @{ Root = $c; Lang = $lang } }
    }
    return $null
}

$found = Find-Hytale
if ($found) {
    $PathBox.Text = $found.Root
    $PathStatus.Text = "Installation detectee : $($found.Lang)"
    $PathStatus.Foreground = '#FF6FCF97'
} else {
    $PathStatus.Text = "Installation non detectee automatiquement : selectionnez le dossier Hytale."
    $PathStatus.Foreground = '#FFE0B36A'
}

$BrowseButton.Add_Click({
    $dlg = New-Object System.Windows.Forms.FolderBrowserDialog
    $dlg.Description = 'Selectionnez le dossier Hytale'
    if ($dlg.ShowDialog() -eq 'OK') {
        $PathBox.Text = $dlg.SelectedPath
        $lang = Test-GamePath $dlg.SelectedPath
        if ($lang) {
            $PathStatus.Text = "Dossier valide : $lang"
            $PathStatus.Foreground = '#FF6FCF97'
        } else {
            $PathStatus.Text = "Attention : dossier Language introuvable ici. Verifiez le chemin."
            $PathStatus.Foreground = '#FFE0B36A'
        }
    }
})

# ---------------------------------------------------------------------------
# Install / uninstall
# ---------------------------------------------------------------------------
function Get-LangFiles {
    # Prefer downloading the latest files from GitHub; fall back to bundled files.
    $tmp = Join-Path $env:TEMP 'hytale-fr-lang'
    New-Item -ItemType Directory -Force $tmp | Out-Null
    try {
        Invoke-WebRequest "$Script:RepoRaw/lang/fr-FR/client.lang" -OutFile "$tmp\client.lang" -UseBasicParsing -TimeoutSec 30
        Invoke-WebRequest "$Script:RepoRaw/lang/fr-FR/meta.lang"   -OutFile "$tmp\meta.lang"   -UseBasicParsing -TimeoutSec 30
        return $tmp
    } catch {
        $bundled = Join-Path $PSScriptRoot '..\lang\fr-FR'
        if (Test-Path "$bundled\client.lang") { return $bundled }
        throw "Telechargement impossible et aucun fichier local trouve : $_"
    }
}

$InstallButton.Add_Click({
    $langDir = Test-GamePath $PathBox.Text
    if (-not $langDir) {
        $StatusText.Text = 'Erreur : dossier Language introuvable.'
        $StatusText.Foreground = '#FFE08A8A'
        return
    }
    $InstallButton.IsEnabled = $false
    $StatusText.Foreground = '#FF9FB4D8'
    $StatusText.Text = 'Installation en cours...'
    $window.Dispatcher.Invoke([Action]{}, 'Render')
    try {
        $src = Get-LangFiles
        $dest = Join-Path $langDir $Script:GameLanguage
        New-Item -ItemType Directory -Force $dest | Out-Null
        Copy-Item "$src\client.lang" "$dest\client.lang" -Force
        Copy-Item "$src\meta.lang"   "$dest\meta.lang"   -Force
        $StatusText.Text = "Installe ! Activez 'Francais' dans Settings - Language."
        $StatusText.Foreground = '#FF6FCF97'
        [System.Windows.MessageBox]::Show(
            "Traduction francaise installee dans :`n$dest`n`nDans le jeu : Settings > General > Language > Francais.`nRetournez au menu principal pour appliquer.",
            'Installation terminee', 'OK', 'Information') | Out-Null
    } catch {
        $StatusText.Text = "Echec : $_"
        $StatusText.Foreground = '#FFE08A8A'
    }
    $InstallButton.IsEnabled = $true
})

$UninstallButton.Add_Click({
    $langDir = Test-GamePath $PathBox.Text
    if (-not $langDir) { return }
    $dest = Join-Path $langDir $Script:GameLanguage
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        $StatusText.Text = 'Pack fr-FR desinstalle.'
        $StatusText.Foreground = '#FF9FB4D8'
    } else {
        $StatusText.Text = "Le pack fr-FR n'est pas installe."
        $StatusText.Foreground = '#FFE0B36A'
    }
})

$window.ShowDialog() | Out-Null
