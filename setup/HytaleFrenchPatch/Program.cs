using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HytaleFrenchPatch
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private const string RepoRaw = "https://raw.githubusercontent.com/jeremiejt38/hytale-french-translation/main";
        private const string WikiBanner = "https://upload.wikimedia.org/wikipedia/fr/4/42/Hytale_Logo.png";
        private const string VersionsUrl = RepoRaw + "/versions.json";
        private const string DefaultLocale = "fr-FR";

        private readonly HttpClient http = new HttpClient();

        private PictureBox bannerBox;
        private Label lblTitle, lblGameVer, lblPatchVer, lblPath, lblStatus;
        private TextBox txtPath;
        private Button btnBrowse, btnInstall, btnUninstall;
        private ProgressBar progress;

        private string detectedVersion = null;
        private string detectedBuild = null;
        private string detectedGameRoot = null;
        private string detectedLangDir = null;
        private string detectedAssetsZip = null;
        private PatchInfo selectedPatch = null;

        public MainForm()
        {
            Text = "Hytale — Patch de traduction française";
            Size = new Size(700, 580);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Color.FromArgb(14, 21, 38);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.FromArgb(220, 226, 242);

            int m = 24;
            bannerBox = new PictureBox
            {
                Location = new Point(m, m),
                Size = new Size(640, 220),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(19, 27, 48),
            };

            lblTitle = new Label
            {
                Text = "Pack de traduction française (fr-FR) pour Hytale",
                Location = new Point(m, bannerBox.Bottom + 14),
                Size = new Size(640, 22),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            };

            lblGameVer = new Label
            {
                Text = "Détection du jeu...",
                Location = new Point(m, lblTitle.Bottom + 8),
                Size = new Size(640, 18),
                ForeColor = Color.FromArgb(159, 180, 216),
            };

            lblPatchVer = new Label
            {
                Text = "Recherche du patch compatible...",
                Location = new Point(m, lblGameVer.Bottom + 6),
                Size = new Size(640, 18),
                ForeColor = Color.FromArgb(111, 207, 151),
            };

            lblPath = new Label
            {
                Text = "Dossier d'installation de Hytale :",
                Location = new Point(m, lblPatchVer.Bottom + 18),
                Size = new Size(640, 18),
            };

            txtPath = new TextBox
            {
                Location = new Point(m, lblPath.Bottom + 6),
                Size = new Size(520, 26),
                BackColor = Color.FromArgb(26, 36, 56),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };

            btnBrowse = new Button
            {
                Text = "Parcourir...",
                Location = new Point(txtPath.Right + 8, txtPath.Top - 1),
                Size = new Size(112, 28),
                BackColor = Color.FromArgb(46, 61, 92),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnBrowse_Click;

            progress = new ProgressBar
            {
                Location = new Point(m, txtPath.Bottom + 18),
                Size = new Size(640, 14),
                Style = ProgressBarStyle.Continuous,
                Visible = false,
            };

            btnInstall = new Button
            {
                Text = "Installer",
                Location = new Point(m, progress.Bottom + 18),
                Size = new Size(160, 38),
                BackColor = Color.FromArgb(61, 123, 239),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;

            btnUninstall = new Button
            {
                Text = "Désinstaller",
                Location = new Point(btnInstall.Right + 12, btnInstall.Top),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(58, 36, 48),
                ForeColor = Color.FromArgb(224, 138, 138),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false,
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += BtnUninstall_Click;

            lblStatus = new Label
            {
                Text = "Prêt",
                Location = new Point(m, btnInstall.Bottom + 18),
                Size = new Size(640, 50),
                ForeColor = Color.FromArgb(159, 180, 216),
            };

            Controls.AddRange(new Control[] { bannerBox, lblTitle, lblGameVer, lblPatchVer, lblPath, txtPath, btnBrowse, progress, btnInstall, btnUninstall, lblStatus });

            Load += async (_, __) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try { bannerBox.Image = await LoadImageAsync(WikiBanner); } catch { }
            DetectGame();
            await FetchCompatiblePatchAsync();
        }

        private async Task<Image> LoadImageAsync(string url)
        {
            var bytes = await http.GetByteArrayAsync(url);
            using (var ms = new MemoryStream(bytes))
                return Image.FromStream(ms);
        }

        private void DetectGame()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;
                foreach (var root in new[] {
                    Path.Combine(drive.RootDirectory.FullName, "Games", "Hytale"),
                    Path.Combine(drive.RootDirectory.FullName, "Hytale"),
                    Path.Combine(drive.RootDirectory.FullName, "Program Files", "Hytale"),
                })
                {
                    if (TrySetGamePath(root)) return;
                }
            }
            var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            TrySetGamePath(Path.Combine(localApp, "Programs", "Hypixel Studios", "Hytale"));
        }

        private bool TrySetGamePath(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return false;
            var langDir = Path.Combine(root, "install", "release", "package", "game", "latest", "Client", "Data", "Shared", "Language");
            var assetsZip = Path.Combine(root, "install", "release", "package", "game", "latest", "Assets.zip");
            var envDat = Path.Combine(root, "install", "release", "env.dat");
            if (File.Exists(envDat)) ReadVersionFromEnv(envDat);
            if (Directory.Exists(langDir) && File.Exists(assetsZip))
            {
                detectedGameRoot = root;
                detectedLangDir = langDir;
                detectedAssetsZip = assetsZip;
                txtPath.Text = root;
                lblGameVer.Text = $"Jeu détecté : Hytale {detectedVersion ?? "?"} (build {detectedBuild ?? "?"})";
                lblGameVer.ForeColor = Color.FromArgb(159, 180, 216);
                btnUninstall.Enabled = Directory.Exists(Path.Combine(langDir, DefaultLocale));
                return true;
            }
            return false;
        }

        private void ReadVersionFromEnv(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                using (var doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.TryGetProperty("dependency_versions", out var deps) &&
                        deps.TryGetProperty("game", out var game))
                    {
                        foreach (var kvp in game.EnumerateObject())
                        {
                            detectedVersion = kvp.Name;
                            if (kvp.Value.TryGetProperty("build", out var b)) detectedBuild = b.GetInt32().ToString();
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        private async Task FetchCompatiblePatchAsync()
        {
            try
            {
                var json = await http.GetStringAsync(VersionsUrl);
                var versions = JsonSerializer.Deserialize<VersionManifest>(json);
                if (versions?.Patches != null && detectedVersion != null &&
                    versions.Patches.TryGetValue(detectedVersion, out var patch))
                {
                    selectedPatch = patch;
                    lblPatchVer.Text = $"Patch compatible : {patch.PatchVersion} du {patch.PatchDate} (build {patch.Build})";
                }
                else if (versions?.Latest != null)
                {
                    selectedPatch = versions.Patches?.Values.FirstOrDefault(p => p.PatchVersion == versions.Latest)
                        ?? new PatchInfo { PatchVersion = versions.Latest };
                    lblPatchVer.Text = $"Aucun patch exact. Dernier disponible : {versions.Latest}";
                }
                else
                {
                    lblPatchVer.Text = "Impossible de récupérer les informations du patch.";
                    lblPatchVer.ForeColor = Color.FromArgb(224, 179, 106);
                }
            }
            catch (Exception ex)
            {
                lblPatchVer.Text = $"Erreur récupération patch : {ex.Message}";
                lblPatchVer.ForeColor = Color.FromArgb(224, 138, 138);
            }
            btnInstall.Enabled = selectedPatch != null && !string.IsNullOrEmpty(detectedGameRoot) && !string.IsNullOrEmpty(selectedPatch.PatchZip);
        }

        private async void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog { Description = "Sélectionnez le dossier Hytale" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                    detectedGameRoot = null; detectedLangDir = null; detectedAssetsZip = null; detectedVersion = null; detectedBuild = null;
                    selectedPatch = null;
                    btnInstall.Enabled = false;
                    if (TrySetGamePath(fbd.SelectedPath))
                    {
                        lblGameVer.Text = $"Dossier valide : Hytale {detectedVersion ?? "?"} (build {detectedBuild ?? "?"})";
                    }
                    else
                    {
                        lblGameVer.Text = "Dossier invalide : Language et/ou Assets.zip introuvable.";
                        lblGameVer.ForeColor = Color.FromArgb(224, 138, 138);
                    }
                    await FetchCompatiblePatchAsync();
                }
            }
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            if (selectedPatch == null || detectedGameRoot == null) return;
            SetBusy(true);
            progress.Visible = true; progress.Value = 0;
            try
            {
                lblStatus.Text = "Téléchargement du patch...";
                var tmp = Path.Combine(Path.GetTempPath(), "hytale-fr-patch-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tmp);
                var patchZip = Path.Combine(tmp, "patch.zip");
                await DownloadWithProgressAsync(selectedPatch.PatchZip, patchZip, 50);
                progress.Value = 50;

                lblStatus.Text = "Extraction du patch...";
                ZipFile.ExtractToDirectory(patchZip, tmp);
                progress.Value = 60;

                var extractedLang = Path.Combine(tmp, "Language", DefaultLocale);
                var extractedAssets = Path.Combine(tmp, "Assets");

                lblStatus.Text = "Installation des fichiers de langue...";
                var destLangDir = Path.Combine(detectedLangDir, DefaultLocale);
                Directory.CreateDirectory(destLangDir);
                foreach (var f in Directory.GetFiles(extractedLang))
                    File.Copy(f, Path.Combine(destLangDir, Path.GetFileName(f)), true);
                progress.Value = 70;

                lblStatus.Text = "Mise à jour d'Assets.zip (sauvegarde incluse)...";
                await Task.Run(() => UpdateAssetsZip(extractedAssets, detectedAssetsZip));
                progress.Value = 100;

                lblStatus.Text = "Installation terminée !";
                lblStatus.ForeColor = Color.FromArgb(111, 207, 151);
                MessageBox.Show(
                    "Traduction française installée.\n\nDans Hytale : Settings > General > Language > Français, puis retournez au menu principal.",
                    "Installation réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Erreur : {ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(224, 138, 138);
            }
            finally
            {
                SetBusy(false);
                progress.Visible = false;
            }
        }

        private async void BtnUninstall_Click(object sender, EventArgs e)
        {
            if (detectedGameRoot == null) return;
            SetBusy(true);
            try
            {
                var frDir = Path.Combine(detectedLangDir, DefaultLocale);
                if (Directory.Exists(frDir))
                {
                    Directory.Delete(frDir, true);
                    lblStatus.Text = "Pack fr-FR supprimé du dossier Language.";
                }
                else
                {
                    lblStatus.Text = "Le pack fr-FR n'est pas installé.";
                    lblStatus.ForeColor = Color.FromArgb(224, 179, 106);
                }

                if (File.Exists(detectedAssetsZip))
                {
                    lblStatus.Text += "\nNettoyage d'Assets.zip...";
                    await Task.Run(() => RemoveAssetsLocale(DefaultLocale));
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Erreur désinstallation : {ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(224, 138, 138);
            }
            finally { SetBusy(false); }
        }

        private void UpdateAssetsZip(string extractedAssetsRoot, string assetsZipPath)
        {
            var backup = assetsZipPath + ".bak.fr";
            if (!File.Exists(backup))
                File.Copy(assetsZipPath, backup, false);

            var assetFiles = Directory.GetFiles(extractedAssetsRoot, "*.lang", SearchOption.AllDirectories)
                .Select(f => (fullPath: f, entryName: GetZipEntryName(f, extractedAssetsRoot)))
                .ToList();
            var newNames = new HashSet<string>(assetFiles.Select(f => f.entryName));

            // ZipArchiveMode.Update corrupts large (ZIP64) archives like Assets.zip:
            // rebuild a clean contiguous archive instead, then swap it in.
            var tmpPath = assetsZipPath + ".new";
            try
            {
                using (var input = ZipFile.OpenRead(assetsZipPath))
                using (var output = ZipFile.Open(tmpPath, ZipArchiveMode.Create))
                {
                    foreach (var entry in input.Entries)
                    {
                        if (newNames.Contains(entry.FullName))
                            continue; // replaced by the French version below
                        var newEntry = output.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        if (entry.Length > 0 && !entry.FullName.EndsWith("/"))
                        {
                            using (var si = entry.Open())
                            using (var so = newEntry.Open())
                                si.CopyTo(so);
                        }
                    }
                    foreach (var (fullPath, entryName) in assetFiles)
                        output.CreateEntryFromFile(fullPath, entryName, CompressionLevel.Optimal);
                }
                File.Move(tmpPath, assetsZipPath, true);
            }
            finally
            {
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
            }
        }

        private void RemoveAssetsLocale(string locale)
        {
            var prefix1 = $"Server/Languages/{locale}/";
            var prefix2 = $"Common/Languages/{locale}/";
            var tmpPath = detectedAssetsZip + ".new";
            try
            {
                using (var input = ZipFile.OpenRead(detectedAssetsZip))
                using (var output = ZipFile.Open(tmpPath, ZipArchiveMode.Create))
                {
                    foreach (var entry in input.Entries)
                    {
                        if (entry.FullName.StartsWith(prefix1) || entry.FullName.StartsWith(prefix2))
                            continue;
                        var newEntry = output.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        if (entry.Length > 0 && !entry.FullName.EndsWith("/"))
                        {
                            using (var si = entry.Open())
                            using (var so = newEntry.Open())
                                si.CopyTo(so);
                        }
                    }
                }
                File.Move(tmpPath, detectedAssetsZip, true);
            }
            finally
            {
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
            }
        }

        private string GetZipEntryName(string fullPath, string root)
        {
            var rel = fullPath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return rel.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }

        private async Task DownloadWithProgressAsync(string url, string dest, int targetPercent)
        {
            using (var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                resp.EnsureSuccessStatusCode();
                var total = resp.Content.Headers.ContentLength ?? -1L;
                using (var s = await resp.Content.ReadAsStreamAsync())
                using (var fs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buf = new byte[8192];
                    long read = 0; int n;
                    while ((n = await s.ReadAsync(buf, 0, buf.Length)) > 0)
                    {
                        await fs.WriteAsync(buf, 0, n);
                        read += n;
                        if (total > 0)
                        {
                            int v = (int)((double)read / total * targetPercent);
                            if (v > progress.Value) progress.Value = Math.Min(v, 100);
                        }
                    }
                }
            }
        }

        private void SetBusy(bool busy)
        {
            btnInstall.Enabled = !busy;
            btnBrowse.Enabled = !busy;
            btnUninstall.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        public class PatchInfo
        {
            public int Build { get; set; }
            [JsonPropertyName("patch_version")] public string PatchVersion { get; set; }
            [JsonPropertyName("patch_date")] public string PatchDate { get; set; }
            [JsonPropertyName("patch_zip")] public string PatchZip { get; set; }
        }

        public class VersionManifest
        {
            [JsonPropertyName("latest")] public string Latest { get; set; }
            [JsonPropertyName("patches")] public Dictionary<string, PatchInfo> Patches { get; set; }
        }
    }
}
