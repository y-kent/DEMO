using System;
using System.Drawing;
using System.Windows.Forms;

namespace DEMO
{
    public partial class TimerActivityForm : Form
    {
        // Theme colors
        private Color bgColor = Color.FromArgb(245, 246, 250);
        private Color cardColor = Color.White;
        private Color textColorDark = Color.FromArgb(45, 52, 54);
        private Color textColorLight = Color.FromArgb(100, 110, 115);
        private Color primaryPurple = Color.FromArgb(167, 155, 225);
        private Color borderColor = Color.FromArgb(223, 228, 234);

        private PictureBox pbPhoto;
        private Label lblPhotoHint;
        private TextBox txtNotes;
        private Label lblTimer;
        private Button btnStartPause;

        private Timer activityTimer;
        private int secondsElapsed = 0;
        private bool isRunning = false;

        public TimerActivityForm(string activityName, int recommendedMinutes, string icon)
        {
            Text = "DEMO - " + activityName;
            Size = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bgColor;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI(activityName, icon);

            activityTimer = new Timer();
            activityTimer.Interval = 1000; // 1 second
            activityTimer.Tick += (s, e) => {
                secondsElapsed++;
                TimeSpan time = TimeSpan.FromSeconds(secondsElapsed);
                lblTimer.Text = time.ToString(@"hh\:mm\:ss");
            };
        }

        private void InitializeUI(string activityName, string icon)
        {
            // ==========================================
            // TOP HEADER
            // ==========================================
            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 80, BackColor = cardColor };
            Label lblIcon = new Label() { Text = icon, Font = new Font("Segoe UI", 24), Location = new Point(30, 20), AutoSize = true };
            Label lblTitle = new Label() { Text = activityName.ToUpper(), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(95, 25), AutoSize = true };
            Label lblDate = new Label() { Text = DateTime.Now.ToString("MMM dd, yyyy | dddd"), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = primaryPurple, Location = new Point(710, 25), AutoSize = true, TextAlign = ContentAlignment.MiddleRight };
            pnlHeader.Controls.Add(lblIcon); pnlHeader.Controls.Add(lblTitle); pnlHeader.Controls.Add(lblDate);
            Panel headerBorder = new Panel() { Dock = DockStyle.Bottom, Height = 1, BackColor = borderColor };
            pnlHeader.Controls.Add(headerBorder); Controls.Add(pnlHeader);

            // ==========================================
            // LEFT COLUMN: TIMER CARD
            // ==========================================
            Panel pnlTimerOuter = new Panel() { Location = new Point(50, 140), Size = new Size(220, 320), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlTimerInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor };

            lblTimer = new Label()
            {
                Text = "00:00:00",
                Font = new Font("Consolas", 32, FontStyle.Bold),
                ForeColor = textColorDark,
                AutoSize = false,
                Size = new Size(220, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 50)
            };
            pnlTimerInner.Controls.Add(lblTimer);

            btnStartPause = new Button()
            {
                Text = "▶ START",
                Location = new Point(30, 170),
                Size = new Size(160, 45),
                BackColor = primaryPurple,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStartPause.FlatAppearance.BorderSize = 0;
            btnStartPause.Click += (s, e) => {
                if (!isRunning)
                {
                    activityTimer.Start();
                    btnStartPause.Text = "⏸ PAUSE";
                    btnStartPause.BackColor = Color.FromArgb(241, 196, 15); // Yellow
                    isRunning = true;
                }
                else
                {
                    activityTimer.Stop();
                    btnStartPause.Text = "▶ RESUME";
                    btnStartPause.BackColor = primaryPurple;
                    isRunning = false;
                }
            };

            pnlTimerInner.Controls.Add(btnStartPause);
            pnlTimerOuter.Controls.Add(pnlTimerInner);
            Controls.Add(pnlTimerOuter);

            // ==========================================
            // MIDDLE COLUMN: PHOTO UPLOAD
            // ==========================================
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
            Panel pnlNotesOuter = new Panel() { Location = new Point(610, 140), Size = new Size(290, 320), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlNotesInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor, Padding = new Padding(15) };

            txtNotes = new TextBox() { Multiline = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11), ForeColor = textColorLight, BorderStyle = BorderStyle.None, Text = "Write your reflections or notes here..." };
            txtNotes.Enter += (s, e) => { if (txtNotes.Text == "Write your reflections or notes here...") { txtNotes.Text = ""; txtNotes.ForeColor = textColorDark; } };
            pnlNotesInner.Controls.Add(txtNotes); pnlNotesOuter.Controls.Add(pnlNotesInner); Controls.Add(pnlNotesOuter);

            // ==========================================
            // BOTTOM: MARK AS DONE BUTTON (Save Logic)
            // ==========================================
            Button btnDone = new Button() { Text = "MARK AS DONE ✓", Location = new Point((ClientSize.Width - 250) / 2, 490), Size = new Size(250, 45), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDone.FlatAppearance.BorderSize = 0;

            btnDone.Click += (s, e) => {
                activityTimer.Stop();

                // BACKEND COMPUTATION: Kukunin mula sa totoong tracked time
                DateTime endTime = DateTime.Now;
                DateTime startTime = endTime.AddSeconds(-secondsElapsed);
                string timeCompleted = endTime.ToString("hh:mm tt");

                string userNotes = txtNotes.Text == "Write your reflections or notes here..." ? "" : txtNotes.Text;
                string attachedPhoto = pbPhoto.Tag != null ? $"\n[Photo Attached: {pbPhoto.Tag.ToString()}]" : "";
                string finalDetails = userNotes + attachedPhoto;

                int minutesSpent = secondsElapsed / 60;

                // PhysicalActivity category para maglabas ng "Duration" sa Scrapbook!
                PhysicalActivity physicalEntry = new PhysicalActivity(activityName, "Tracked Emotion", minutesSpent, finalDetails);

                physicalEntry.StartedAt = startTime;
                physicalEntry.EndedAt = endTime;

                physicalEntry.Save(null);

                MessageBox.Show($"Great job on your {activityName}! Saved to Database.\nTime Completed: {timeCompleted}\nDuration Tracked: {minutesSpent}m {secondsElapsed % 60}s", "Activity Recorded", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // IDINAGDAG ITO PARA SABIHAN ANG DASHBOARD NA TAPOS NA ANG ACTIVITY
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            Controls.Add(btnDone);
        }
    }
}