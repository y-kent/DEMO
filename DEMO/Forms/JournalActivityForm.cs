using System;
using System.Drawing;
using System.Windows.Forms;

namespace DEMO
{
    public partial class JournalActivityForm : Form
    {
        // ==========================================
        // TEMA AT KULAY (Modern UI)
        // ==========================================
        private Color bgColor = Color.FromArgb(245, 246, 250);
        private Color cardColor = Color.White;
        private Color textColorDark = Color.FromArgb(45, 52, 54);
        private Color textColorLight = Color.FromArgb(100, 110, 115);
        private Color primaryPurple = Color.FromArgb(167, 155, 225);
        private Color borderColor = Color.FromArgb(223, 228, 234);

        // ==========================================
        // MGA CONTROLS AT VARIABLES
        // ==========================================
        private PictureBox pbPhoto;
        private Label lblPhotoHint;
        private TextBox txtJournal;
        private DateTime startTime; // Dito iimbak ang Time Started

        public JournalActivityForm(string activityName = "Journal")
        {
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

            Label lblIcon = new Label() { Text = "📝", Font = new Font("Segoe UI", 24), Location = new Point(30, 20), AutoSize = true };
            Label lblTitle = new Label() { Text = activityName.ToUpper(), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(95, 25), AutoSize = true };
            Label lblDate = new Label() { Text = DateTime.Now.ToString("MMM dd, yyyy | dddd"), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = primaryPurple, Location = new Point(710, 25), AutoSize = true, TextAlign = ContentAlignment.MiddleRight };

            pnlHeader.Controls.Add(lblIcon);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblDate);

            Panel headerBorder = new Panel() { Dock = DockStyle.Bottom, Height = 1, BackColor = borderColor };
            pnlHeader.Controls.Add(headerBorder);
            Controls.Add(pnlHeader);

            // ==========================================
            // LEFT COLUMN: PHOTO UPLOAD
            // ==========================================
            Panel pnlPhotoOuter = new Panel() { Location = new Point(40, 140), Size = new Size(350, 320), BackColor = borderColor, Padding = new Padding(1) };
            pbPhoto = new PictureBox()
            {
                Dock = DockStyle.Fill,
                BackColor = cardColor,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            lblPhotoHint = new Label()
            {
                Text = "📷\n\nCLICK TO ADD\nA MEMORY PHOTO",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = textColorLight,
                AutoSize = false,
                Size = new Size(350, 320),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            pbPhoto.Controls.Add(lblPhotoHint);

            // Logic para makapag-upload ng litrato
            EventHandler uploadImageAction = (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pbPhoto.Image = Image.FromFile(ofd.FileName);
                    lblPhotoHint.Visible = false; // Itatago ang text kapag may litrato na
                    pbPhoto.Tag = ofd.FileName;   // Ise-save ang location ng file para sa Database
                }
            };

            pbPhoto.Click += uploadImageAction;
            lblPhotoHint.Click += uploadImageAction;

            pnlPhotoOuter.Controls.Add(pbPhoto);
            Controls.Add(pnlPhotoOuter);

            // ==========================================
            // RIGHT COLUMN: JOURNAL TEXT AREA
            // ==========================================
            Panel pnlNotesOuter = new Panel() { Location = new Point(430, 140), Size = new Size(460, 320), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlNotesInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor, Padding = new Padding(20) };

            txtJournal = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                ForeColor = textColorLight,
                BorderStyle = BorderStyle.None,
                Text = "Write your thoughts here..."
            };

            // Tatanggalin ang placeholder text kapag nag-click ang user
            txtJournal.Enter += (s, e) => {
                if (txtJournal.Text == "Write your thoughts here...")
                {
                    txtJournal.Text = "";
                    txtJournal.ForeColor = textColorDark;
                }
            };

            pnlNotesInner.Controls.Add(txtJournal);
            pnlNotesOuter.Controls.Add(pnlNotesInner);
            Controls.Add(pnlNotesOuter);

            // ==========================================
            // BOTTOM: MARK AS DONE BUTTON (SAVE LOGIC)
            // ==========================================
            Button btnDone = new Button()
            {
                Text = "MARK AS DONE ✓",
                Location = new Point((ClientSize.Width - 250) / 2, 490),
                Size = new Size(250, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDone.FlatAppearance.BorderSize = 0;

            btnDone.Click += (s, e) => {
                // RECORD END TIME (Kung kailan pinindot ang Done)
                DateTime endTime = DateTime.Now;
                string timeCompleted = endTime.ToString("hh:mm tt");

                // Kunin ang notes at i-format ng malinis
                string userNotes = txtJournal.Text == "Write your thoughts here..." ? "" : txtJournal.Text;
                string attachedPhoto = pbPhoto.Tag != null ? $"\n[Photo Attached: {pbPhoto.Tag.ToString()}]" : "";
                string finalDetails = userNotes + attachedPhoto;

                // GAMITIN ANG OOP BASE CLASS PARA MAG-SAVE
                MentalActivity journalEntry = new MentalActivity(activityName, "Tracked Emotion", finalDetails);

                // IPASA ANG EXACT START AT END TIME
                journalEntry.StartedAt = startTime;
                journalEntry.EndedAt = endTime;

                journalEntry.Save(null);

                MessageBox.Show($"Great job expressing your thoughts! Saved to Database.\nTime Completed: {timeCompleted}", "Activity Recorded", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // IDINAGDAG ITO PARA SABIHAN ANG DASHBOARD NA TAPOS NA ANG ACTIVITY
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            Controls.Add(btnDone);
        }
    }
}