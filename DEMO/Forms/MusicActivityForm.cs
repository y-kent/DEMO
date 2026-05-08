using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace DEMO
{
    public partial class MusicActivityForm : Form
    {
        // Modern Theme Colors 
        private Color bgColor = Color.FromArgb(245, 246, 250);
        private Color cardColor = Color.White;
        private Color textColorDark = Color.FromArgb(45, 52, 54);
        private Color textColorLight = Color.FromArgb(100, 110, 115);
        private Color primaryPurple = Color.FromArgb(167, 155, 225);
        private Color spotifyGreen = Color.FromArgb(30, 215, 96);
        private Color borderColor = Color.FromArgb(223, 228, 234);

        // Controls
        private Button btnOpenSpotify;
        private PictureBox pbPhoto;
        private Label lblPhotoHint;
        private TextBox txtNotes;

        // Totoong Spotify Playlist Links
        private string happyPlaylistUrl = "https://open.spotify.com/playlist/37i9dQZF1DXdPec7aLTmlC";
        private string sadPlaylistUrl = "https://open.spotify.com/playlist/37i9dQZF1DX7qK8ma5wgG1";

        private string currentEmotion = "";

        // BAGONG VARIABLE: Para i-record kung anong oras binuksan ang music form
        private DateTime startTime;

        public MusicActivityForm(string activityName = "Listen to Music", string emotionTag = "HAPPY")
        {
            currentEmotion = emotionTag.ToUpper();

            // RECORD START TIME KAGAD PAGKABUKAS NG FORM
            startTime = DateTime.Now;

            Text = "DEMO - " + activityName;
            Size = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bgColor;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI(activityName);
        }

        private void InitializeUI(string activityName)
        {
            // ==========================================
            // TOP HEADER
            // ==========================================
            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 80, BackColor = cardColor };
            Label lblIcon = new Label() { Text = "🎧", Font = new Font("Segoe UI", 24), Location = new Point(30, 20), AutoSize = true };
            Label lblTitle = new Label() { Text = activityName.ToUpper(), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(95, 25), AutoSize = true };
            Label lblDate = new Label() { Text = DateTime.Now.ToString("MMM dd, yyyy | dddd"), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = primaryPurple, Location = new Point(710, 25), AutoSize = true, TextAlign = ContentAlignment.MiddleRight };
            pnlHeader.Controls.Add(lblIcon); pnlHeader.Controls.Add(lblTitle); pnlHeader.Controls.Add(lblDate);
            Panel headerBorder = new Panel() { Dock = DockStyle.Bottom, Height = 1, BackColor = borderColor };
            pnlHeader.Controls.Add(headerBorder); Controls.Add(pnlHeader);

            // ==========================================
            // LEFT COLUMN: SIMPLIFIED SPOTIFY CARD
            // ==========================================
            Panel pnlPlayerOuter = new Panel() { Location = new Point(50, 140), Size = new Size(220, 320), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlPlayerInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor };

            Label lblSpotifyIcon = new Label()
            {
                Text = "🟢", // Spotify Icon
                Font = new Font("Segoe UI", 48),
                ForeColor = spotifyGreen,
                AutoSize = false,
                Size = new Size(220, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 40)
            };
            pnlPlayerInner.Controls.Add(lblSpotifyIcon);

            Label lblSpotifyDesc = new Label()
            {
                Text = "Ready to relax?\nOpen Spotify to play\nyour recommended music.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = textColorLight,
                AutoSize = false,
                Size = new Size(220, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 130)
            };
            pnlPlayerInner.Controls.Add(lblSpotifyDesc);

            btnOpenSpotify = new Button()
            {
                Text = "OPEN SPOTIFY",
                Location = new Point(30, 230),
                Size = new Size(160, 45),
                BackColor = spotifyGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOpenSpotify.FlatAppearance.BorderSize = 0;

            // SIMPLE LOGIC: Bubuksan lang ang Happy o Sad playlist
            btnOpenSpotify.Click += (s, e) => {
                string targetUrl = currentEmotion.Contains("HAPP") ? happyPlaylistUrl : sadPlaylistUrl;
                try { Process.Start(new ProcessStartInfo(targetUrl) { UseShellExecute = true }); }
                catch { try { Process.Start(targetUrl); } catch { MessageBox.Show("Failed to open Spotify link."); } }
            };

            pnlPlayerInner.Controls.Add(btnOpenSpotify);
            pnlPlayerOuter.Controls.Add(pnlPlayerInner);
            Controls.Add(pnlPlayerOuter);

            // ==========================================
            // MIDDLE COLUMN: PHOTO UPLOAD
            // ==========================================
            // Inayos ang spacing: Location X ay 300
            Panel pnlPhotoOuter = new Panel() { Location = new Point(300, 140), Size = new Size(280, 320), BackColor = borderColor, Padding = new Padding(1) };
            pbPhoto = new PictureBox() { Dock = DockStyle.Fill, BackColor = cardColor, SizeMode = PictureBoxSizeMode.Zoom, Cursor = Cursors.Hand };

            lblPhotoHint = new Label() { Text = "📷\n\nCLICK TO UPLOAD\nA PHOTO HERE", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = textColorLight, AutoSize = false, Size = new Size(280, 320), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            pbPhoto.Controls.Add(lblPhotoHint);

            EventHandler uploadImageAction = (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pbPhoto.Image = Image.FromFile(ofd.FileName);
                    lblPhotoHint.Visible = false; pbPhoto.Tag = ofd.FileName;
                }
            };

            pbPhoto.Click += uploadImageAction; lblPhotoHint.Click += uploadImageAction;
            pnlPhotoOuter.Controls.Add(pbPhoto); Controls.Add(pnlPhotoOuter);

            // ==========================================
            // RIGHT COLUMN: NOTES
            // ==========================================
            // Inayos ang spacing: Location X ay 610
            Panel pnlNotesOuter = new Panel() { Location = new Point(610, 140), Size = new Size(290, 320), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlNotesInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor, Padding = new Padding(15) };

            txtNotes = new TextBox() { Multiline = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11), ForeColor = textColorLight, BorderStyle = BorderStyle.None, Text = "Write your reflections or notes here..." };
            txtNotes.Enter += (s, e) => { if (txtNotes.Text == "Write your reflections or notes here...") { txtNotes.Text = ""; txtNotes.ForeColor = textColorDark; } };
            pnlNotesInner.Controls.Add(txtNotes); pnlNotesOuter.Controls.Add(pnlNotesInner); Controls.Add(pnlNotesOuter);

            // ==========================================
            // BOTTOM: MARK AS DONE BUTTON
            // ==========================================
            Button btnDone = new Button() { Text = "MARK AS DONE ✓", Location = new Point((ClientSize.Width - 250) / 2, 490), Size = new Size(250, 45), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDone.FlatAppearance.BorderSize = 0;

            btnDone.Click += (s, e) => {
                // RECORD END TIME (Kung kailan pinindot ang Done)
                DateTime endTime = DateTime.Now;
                string timeCompleted = endTime.ToString("hh:mm tt");

                string userNotes = txtNotes.Text == "Write your reflections or notes here..." ? "" : txtNotes.Text;

                string playlistName = currentEmotion.Contains("HAPP") ? "Happy Hits" : "Sad Songs";
                string playlistInfo = $"\n[Playlist: {playlistName}]";
                string attachedPhoto = pbPhoto.Tag != null ? $"\n[Photo Attached: {pbPhoto.Tag.ToString()}]" : "";
                string finalDetails = userNotes + playlistInfo + attachedPhoto;

                MentalActivity musicEntry = new MentalActivity(activityName, currentEmotion, finalDetails);

                // IPASA ANG EXACT START AT END TIME PARA HINDI MAGING NULL SA DATABASE
                musicEntry.StartedAt = startTime;
                musicEntry.EndedAt = endTime;

                musicEntry.Save(null);

                MessageBox.Show($"Great job relaxing with music! Saved to Database.\nTime Completed: {timeCompleted}", "Activity Recorded", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // IDINAGDAG ITO PARA SABIHAN ANG DASHBOARD NA TAPOS NA ANG ACTIVITY
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            Controls.Add(btnDone);
        }
    }
}