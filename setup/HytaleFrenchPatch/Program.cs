using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
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

        private readonly HttpClient http = new HttpClient();

        private PictureBox bannerBox;
        private Label lblTitle, lblGameVer, lblPatchVer, lblPath, lblStatus;
        private TextBox txtPath;
        private Button btnBrowse, btnInstall, btnUninstall;
        private ProgressBar progress;

        private string detectedVersion = null;
        private string detectedBuild = null;
        private string detectedLangDir = null;
        private PatchInfo selectedPatch = null;

        public MainForm()
        {
            Text = "Hytale — Patch de traduction française";
            Size = new Size(700, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            BackColor = Color.FromArgb(14, 21, 38);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.FromArgb(220, 226, 242);

            var margin = 24;
            bannerBox = new PictureBox
            {
                Location = new Point(margin, margin),
                Size = new Size(640, 220),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(19, 27, 48),
            };

            lblTitle = new Label
            {
                Text = "Pack de traduction française (fr-FR) pour Hytale",
                Location = new Point(margin, bannerBox.Bottom + 14),
                Size = new Size(640, 22),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            };

            lblGameVer = new Label
            {
                Text = "Détection du jeu...",
                Location = new Point(margin, lblTitle.Bottom + 8),
                Size = new Size(640, 18),
                ForeColor = Color.FromArgb(159, 180, 216),
            };

            lblPatchVer = new Label
            {
                Text = "Recherche du patch compatible...",
                Location = new Point(margin, lblGameVer.Bottom + 6),
                Size = new Size(640, 18),
                ForeColor = Color.FromArgb(111, 207, 151),
            };

            lblPath = new Label
            {
                Text = "Dossier d'installation :",
                Location = new Point(margin, lblPatchVer.Bottom + 18),
                Size = new Size(640, 18),
            };

            txtPath = new TextBox
            {
                Location = new Point(margin, lblPath.Bottom + 6),
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
                Location = new Point(margin, txtPath.Bottom + 18),
                Size = new Size(640, 14),
                Style = ProgressBarStyle.Continuous,
                Visible = false,
            };

            btnInstall = new Button
            {
                Text = "Installer",
                Location = new Point(margin, progress.Bottom + 18),
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
                Location = new Point(margin, btnInstall.Bottom + 18),
                Size = new Size(640, 40),
                ForeColor = Color.FromArgb(159, 180, 216),
            };

            Controls.AddRange(new Control[] { bannerBox, lblTitle, lblGameVer, lblPatchVer, lblPath, txtPath, btnBrowse, progress, btnInstall, btnUninstall, lblStatus });

            Load += async (_, __) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                bannerBox.Image = await LoadImageAsync(WikiBanner);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Impossible de charger la bannière : {ex.Message}";
            }

            DetectGame();

            if (!string.IsNullOrEmpty(detectedLangDir))
            {
                txtPath.Text = Directory.GetParent(Directory.GetParent(detectedLangDir)?.FullName)?.FullName ?? detectedLangDir;
                lblGameVer.Text = $"Jeu détecté : Hytale {detectedVersion ?? "?"} (build {detectedBuild ?? "?"}) — {detectedLangDir}";
                btnUninstall.Enabled = Directory.Exists(Path.Combine(detectedLangDir, "fr-FR"));
            }
            else
            {
                lblGameVer.Text = "Jeu Hytale non détecté automatiquement. Sélectionnez le dossier Hytale.";
                lblGameVer.ForeColor = Color.FromArgb(224, 179, 106);
            }

            await FetchCompatiblePatchAsync();
        }

        private async Task<Image> LoadImageAsync(string url)
        {
            var bytes = await http.GetByteArrayAsync(url);
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
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
                    TryRegisterGamePath(root);
                    if (detectedLangDir != null) return;
                }
            }

            var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            TryRegisterGamePath(Path.Combine(localApp, "Programs", "Hypixel Studios", "Hytale"));
        }

        private void TryRegisterGamePath(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
            var envDat = Path.Combine(root, "install", "release", "env.dat");
            var langDir = Path.Combine(root, "install", "release", "package", "game", "latest", "Client", "Data", "Shared", "Language");
            if (File.Exists(envDat))
            {
                ReadVersionFromEnv(envDat);
            }
            if (Directory.Exists(langDir))
            {
                detectedLangDir = langDir;
            }
        }

        private void ReadVersionFromEnv(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                using (var doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.TryGetProperty("dependency_versions", out var deps))
                    {
                        if (deps.TryGetProperty("game", out var game))
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
                    lblPatchVer.Text = $"Aucun patch exact trouvé. Dernier patch disponible : {versions.Latest}";
                }
                else
                {
                    lblPatchVer.Text = "Impossible de récupérer les informations du patch.";
                    lblPatchVer.ForeColor = Color.FromArgb(224, 179, 106);
                }
            }
            catch (Exception ex)
            {
                lblPatchVer.Text = $"Erreur de récupération du patch : {ex.Message}";
                lblPatchVer.ForeColor = Color.FromArgb(224, 138, 138);
            }

            btnInstall.Enabled = selectedPatch != null && !string.IsNullOrEmpty(detectedLangDir);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog { Description = "Sélectionnez le dossier Hytale" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                    detectedLangDir = null;
                    detectedVersion = null; detectedBuild = null;
                    TryRegisterGamePath(fbd.SelectedPath);
                    if (detectedLangDir != null)
                    {
                        lblGameVer.Text = $"Dossier valide : Hytale {detectedVersion ?? "?"} (build {detectedBuild ?? "?"})";
                        lblGameVer.ForeColor = Color.FromArgb(159, 180, 216);
                        btnUninstall.Enabled = Directory.Exists(Path.Combine(detectedLangDir, "fr-FR"));
                    }
                    else
                    {
                        lblGameVer.Text = "Dossier invalide : env.dat et/ou Language introuvable.";
                        lblGameVer.ForeColor = Color.FromArgb(224, 138, 138);
                    }
                    FetchCompatiblePatchAsync();
                }
            }
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            if (selectedPatch == null || detectedLangDir == null) return;

            SetBusy(true);
            progress.Visible = true;
            lblStatus.Text = "Téléchargement du patch...";
            progress.Value = 0;

            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "hytale-fr-patch");
                Directory.CreateDirectory(tempDir);

                var clientPath = Path.Combine(tempDir, "client.lang");
                var metaPath = Path.Combine(tempDir, "meta.lang");

                await DownloadWithProgressAsync(selectedPatch.ClientLang, clientPath, 50);
                await DownloadWithProgressAsync(selectedPatch.MetaLang, metaPath, 100);

                lblStatus.Text = "Installation dans le dossier Language...";
                var frDir = Path.Combine(detectedLangDir, "fr-FR");
                Directory.CreateDirectory(frDir);
                File.Copy(clientPath, Path.Combine(frDir, "client.lang"), true);
                File.Copy(metaPath, Path.Combine(frDir, "meta.lang"), true);

                lblStatus.Text = "Installation terminée !";
                lblStatus.ForeColor = Color.FromArgb(111, 207, 151);
                btnUninstall.Enabled = true;
                MessageBox.Show(
                    "Le pack français est installé.\n\nDans Hytale : Settings > General > Language > Français, puis retournez au menu principal.",
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

        private async Task DownloadWithProgressAsync(string url, string dest, int targetPercent)
        {
            using (var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                var total = response.Content.Headers.ContentLength ?? -1L;
                using (var s = await response.Content.ReadAsStreamAsync())
                using (var fs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[8192];
                    long read = 0;
                    int n;
                    while ((n = await s.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, n);
                        read += n;
                        if (total > 0)
                        {
                            int value = (int)((double)read / total * targetPercent);
                            if (value > progress.Value) progress.Value = Math.Min(value, 100);
                        }
                    }
                }
            }
        }

        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            var frDir = Path.Combine(detectedLangDir, "fr-FR");
            if (Directory.Exists(frDir))
            {
                Directory.Delete(frDir, true);
                lblStatus.Text = "Pack français désinstallé.";
                lblStatus.ForeColor = Color.FromArgb(159, 180, 216);
                btnUninstall.Enabled = false;
            }
        }

        private void SetBusy(bool busy)
        {
            btnInstall.Enabled = !busy;
            btnBrowse.Enabled = !busy;
            btnUninstall.Enabled = !busy && Directory.Exists(Path.Combine(detectedLangDir ?? "", "fr-FR"));
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        public class PatchInfo
        {
            public int Build { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("patch_version")]
            public string PatchVersion { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("patch_date")]
            public string PatchDate { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("client_lang")]
            public string ClientLang { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("meta_lang")]
            public string MetaLang { get; set; }
        }

        public class VersionManifest
        {
            public string Latest { get; set; }
            public Dictionary<string, PatchInfo> Patches { get; set; }
        }
    }
}
