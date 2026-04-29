using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DEMO
{
    public partial class EmotionForm : Form
    {
        private Panel pnlPrimary = new Panel();
        private Panel pnlSecondary = new Panel();
        private Panel pnlActivity = new Panel();
        private Panel pnlPopup = new Panel();

        // Custom Theme Colors (Background & Text)
        private Color bgColor = Color.FromArgb(250, 250, 252);
        private Color textColorDark = Color.FromArgb(40, 40, 40);
        private Color textColorLight = Color.FromArgb(100, 100, 100);

        // Primary Emotion Colors (Pastel)
        private Color colHappy = Color.FromArgb(254, 235, 142);    // Yellow
        private Color colSad = Color.FromArgb(174, 214, 241);      // Pastel Blue
        private Color colAngry = Color.FromArgb(245, 183, 177);    // Coral Red
        private Color colFear = Color.FromArgb(210, 180, 222);     // Lavender Purple
        private Color colSurprise = Color.FromArgb(169, 223, 191); // Mint Green

        // Secondary Emotion Colors (Shades of Primary)
        private Color shadeHappy = Color.FromArgb(255, 246, 191);    // Lighter Yellow
        private Color shadeSad = Color.FromArgb(206, 230, 248);      // Lighter Pastel Blue
        private Color shadeAngry = Color.FromArgb(250, 209, 205);    // Lighter Coral Red
        private Color shadeFear = Color.FromArgb(228, 208, 237);     // Lighter Lavender
        private Color shadeSurprise = Color.FromArgb(203, 237, 217); // Lighter Mint Green

        // Current State
        private string selectedPrimary = "";
        private Color currentThemeColor = Color.White;
        private Color currentShadeColor = Color.White;

        private Label lblSelectedPrimary;
        private PictureBox pbSelectedPrimaryIcon;
        private Button[] btnSecondaries = new Button[4];

        // Activity UI Elements
        private Panel[] pnlActivities = new Panel[3];
        private Label[] lblActTitles = new Label[3];
        private Label[] lblActTags = new Label[3];
        private PictureBox[] pbActIcons = new PictureBox[3]; // Bagong array para sa icons ng activities

        public EmotionForm()
        {
            InitializeComponent();

            Text = "DEMO - Emotion Check-in";
            Size = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bgColor;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // ==========================================
            // 1. PRIMARY EMOTION SCREEN
            // ==========================================
            pnlPrimary.Dock = DockStyle.Fill;
            pnlPrimary.BackColor = bgColor;
            Controls.Add(pnlPrimary);

            // Header Elements
            Label lblTopLeft = CreateLabel("↺ BACK", 30, 20, 10, FontStyle.Bold);
            lblTopLeft.ForeColor = textColorLight;
            lblTopLeft.Cursor = Cursors.Hand;
            pnlPrimary.Controls.Add(lblTopLeft);

            Label lblTopRight = CreateLabel("👤 Welcome, User", ClientSize.Width - 160, 20, 10, FontStyle.Regular);
            lblTopRight.ForeColor = textColorDark;
            pnlPrimary.Controls.Add(lblTopRight);

            // Main Titles
            Label lblQuestion = CreateLabel("What are you feeling today?", 0, 90, 24, FontStyle.Bold);
            lblQuestion.Location = new Point((ClientSize.Width - lblQuestion.PreferredWidth) / 2, 90);
            pnlPrimary.Controls.Add(lblQuestion);

            Label lblSubTitle = CreateLabel("Choose the emotion that best matches your current mood.", 0, 140, 11, FontStyle.Regular);
            lblSubTitle.ForeColor = textColorLight;
            lblSubTitle.Location = new Point((ClientSize.Width - lblSubTitle.PreferredWidth) / 2, 140);
            pnlPrimary.Controls.Add(lblSubTitle);

            // 5 Colored Primary Buttons
            string[] primaries = { "Happy", "Sad", "Angry", "Fear", "Surprise" };
            Color[] pColors = { colHappy, colSad, colAngry, colFear, colSurprise };

            int boxWidth = 140;
            int boxHeight = 160;
            int spacing = 20;
            int totalWidth = (boxWidth * 5) + (spacing * 4);
            int startX = (ClientSize.Width - totalWidth) / 2;

            for (int i = 0; i < 5; i++)
            {
                Button btn = new Button();
                btn.Text = primaries[i];

                // Ibalik ang text sa ibaba
                btn.TextAlign = ContentAlignment.BottomCenter;
                btn.Padding = new Padding(0, 0, 0, 15);

                btn.Size = new Size(boxWidth, boxHeight);
                btn.Location = new Point(startX + (i * (boxWidth + spacing)), 200);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = pColors[i];
                btn.ForeColor = textColorDark;
                btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                btn.Cursor = Cursors.Hand;
                btn.Tag = pColors[i];

                // IBABALIK NATIN ANG PICTUREBOX PARA SA "ZOOM" FEATURE
                PictureBox pbSlot = new PictureBox();
                pbSlot.Size = new Size(90, 90);
                pbSlot.Location = new Point((boxWidth - 90) / 2, 20);
                pbSlot.BackColor = Color.Transparent;
                pbSlot.SizeMode = PictureBoxSizeMode.Zoom; // Ito ang pipigil sa pag-zoom ng sobra (Magpi-fit na ang image)
                pbSlot.BorderStyle = BorderStyle.None;

                // PARA KAY ALWYN: I-uncomment at i-tama ang mga pangalan base sa nasa Resources ninyo
                if (primaries[i] == "Happy") pbSlot.Image = Properties.Resources.Happy;
                else if (primaries[i] == "Sad") pbSlot.Image = Properties.Resources.Sad;
                else if (primaries[i] == "Angry") pbSlot.Image = Properties.Resources.Angry;
                else if (primaries[i] == "Fear") pbSlot.Image = Properties.Resources.Fear;
                else if (primaries[i] == "Surprise") pbSlot.Image = Properties.Resources.Surprised;

                // Ipasa ang click ng picture sa mismong button
                pbSlot.Click += (s, ev) => btn.PerformClick();

                btn.Controls.Add(pbSlot); // Ipasok ang picturebox sa button
                btn.Click += PrimaryEmotion_Click;
                pnlPrimary.Controls.Add(btn);
            }

            Label lblBottomText = CreateLabel("Your mood helps us recommend the best activities for today.", 0, 400, 10, FontStyle.Regular);
            lblBottomText.ForeColor = textColorLight;
            lblBottomText.Location = new Point((ClientSize.Width - lblBottomText.PreferredWidth) / 2, 400);
            pnlPrimary.Controls.Add(lblBottomText);

            Label lblSkip = CreateLabel("Skip", ClientSize.Width - 70, ClientSize.Height - 60, 10, FontStyle.Regular);
            lblSkip.ForeColor = textColorLight;
            lblSkip.Cursor = Cursors.Hand;
            pnlPrimary.Controls.Add(lblSkip);


            // ==========================================
            // 2. SECONDARY EMOTION SCREEN
            // ==========================================
            pnlSecondary.Dock = DockStyle.Fill;
            pnlSecondary.BackColor = bgColor;
            pnlSecondary.Visible = false;
            Controls.Add(pnlSecondary);

            Label lblBack2 = CreateLabel("↺ BACK", 30, 20, 10, FontStyle.Bold);
            lblBack2.Cursor = Cursors.Hand;
            lblBack2.ForeColor = textColorLight;
            lblBack2.Click += (s, e) => { pnlSecondary.Visible = false; pnlPrimary.Visible = true; };
            pnlSecondary.Controls.Add(lblBack2);

            Label lblSecQuestion = CreateLabel("Can you be more specific?", 0, 70, 20, FontStyle.Bold);
            lblSecQuestion.Location = new Point((ClientSize.Width - lblSecQuestion.PreferredWidth) / 2, 70);
            pnlSecondary.Controls.Add(lblSecQuestion);

            // Left Side: Selected Primary Display (Malaking Box)
            Panel boxPrimary = new Panel() { Size = new Size(240, 280), Location = new Point(160, 140) };

            pbSelectedPrimaryIcon = new PictureBox();
            pbSelectedPrimaryIcon.Size = new Size(140, 140); // Saktong-sakto na laki
            pbSelectedPrimaryIcon.Location = new Point(50, 45); // Perfectly centered
            pbSelectedPrimaryIcon.BackColor = Color.Transparent;
            pbSelectedPrimaryIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pbSelectedPrimaryIcon.BorderStyle = BorderStyle.None;
            boxPrimary.Controls.Add(pbSelectedPrimaryIcon);

            lblSelectedPrimary = CreateLabel("EMOTION", 0, 195, 20, FontStyle.Bold);
            lblSelectedPrimary.AutoSize = false;
            lblSelectedPrimary.Size = new Size(240, 40);
            lblSelectedPrimary.TextAlign = ContentAlignment.MiddleCenter;
            boxPrimary.Controls.Add(lblSelectedPrimary);
            pnlSecondary.Controls.Add(boxPrimary);

            // Right Side: 4 Secondary Buttons (Shaded Colors)
            for (int i = 0; i < 4; i++)
            {
                btnSecondaries[i] = new Button();
                btnSecondaries[i].Size = new Size(350, 55);
                btnSecondaries[i].Location = new Point(440, 140 + (i * 75));
                btnSecondaries[i].FlatStyle = FlatStyle.Flat;
                btnSecondaries[i].FlatAppearance.BorderSize = 0;
                btnSecondaries[i].Font = new Font("Segoe UI", 12, FontStyle.Bold);
                btnSecondaries[i].ForeColor = textColorDark;
                btnSecondaries[i].Cursor = Cursors.Hand;
                btnSecondaries[i].Click += SecondaryEmotion_Click;
                pnlSecondary.Controls.Add(btnSecondaries[i]);
            }


            // ==========================================
            // 3. ACTIVITY SCREEN (RANKED + WITH ICONS)
            // ==========================================
            pnlActivity.Dock = DockStyle.Fill;
            pnlActivity.BackColor = bgColor;
            pnlActivity.Visible = false;
            Controls.Add(pnlActivity);

            Label lblBack3 = CreateLabel("↺ BACK", 30, 20, 10, FontStyle.Bold);
            lblBack3.Cursor = Cursors.Hand;
            lblBack3.ForeColor = textColorLight;
            lblBack3.Click += (s, e) => { pnlActivity.Visible = false; pnlSecondary.Visible = true; };
            pnlActivity.Controls.Add(lblBack3);

            Label lblActHeader = CreateLabel("Recommended Activities", 0, 80, 22, FontStyle.Bold);
            lblActHeader.Location = new Point((ClientSize.Width - lblActHeader.PreferredWidth) / 2, 80);
            pnlActivity.Controls.Add(lblActHeader);

            // 3 Activity Cards
            int actBoxWidth = 240;
            int actBoxHeight = 220; // Tinaasan nang konti para may extra room
            int actSpacing = 30;
            int actTotalWidth = (actBoxWidth * 3) + (actSpacing * 2);
            int actStartX = (ClientSize.Width - actTotalWidth) / 2;

            for (int i = 0; i < 3; i++)
            {
                pnlActivities[i] = new Panel();
                pnlActivities[i].Size = new Size(actBoxWidth, actBoxHeight);
                pnlActivities[i].Location = new Point(actStartX + (i * (actBoxWidth + actSpacing)), 150);
                pnlActivities[i].BackColor = Color.White;
                pnlActivities[i].BorderStyle = BorderStyle.FixedSingle;
                pnlActivities[i].Cursor = Cursors.Hand;
                pnlActivities[i].Click += Activity_Click;

                // Tag
                lblActTags[i] = CreateLabel("", 0, 15, 9, FontStyle.Bold);
                lblActTags[i].AutoSize = false;
                lblActTags[i].Size = new Size(actBoxWidth, 20);
                lblActTags[i].TextAlign = ContentAlignment.MiddleCenter;
                if (i == 0)
                {
                    lblActTags[i].Text = "★ MOST RECOMMENDED";
                    lblActTags[i].ForeColor = Color.FromArgb(230, 126, 34);
                }
                lblActTags[i].Click += Activity_Click;

                // BAGONG SLOT: Activity Icon (Nasa gitna)
                pbActIcons[i] = new PictureBox();
                pbActIcons[i].Size = new Size(90, 90);
                pbActIcons[i].Location = new Point((actBoxWidth - 90) / 2, 50); // Naka-center horizontally
                pbActIcons[i].BackColor = Color.Transparent;
                pbActIcons[i].SizeMode = PictureBoxSizeMode.Zoom;
                pbActIcons[i].BorderStyle = BorderStyle.None;
                pbActIcons[i].Click += Activity_Click;

                // Title (Ibinaba sa ilalim ng icon)
                lblActTitles[i] = CreateLabel("ACTIVITY NAME", 0, 150, 12, FontStyle.Bold);
                lblActTitles[i].AutoSize = false;
                lblActTitles[i].Size = new Size(actBoxWidth, 60);
                lblActTitles[i].TextAlign = ContentAlignment.MiddleCenter;
                lblActTitles[i].Click += Activity_Click;

                pnlActivities[i].Controls.Add(lblActTags[i]);
                pnlActivities[i].Controls.Add(pbActIcons[i]); // Inilagay na sa panel
                pnlActivities[i].Controls.Add(lblActTitles[i]);
                pnlActivity.Controls.Add(pnlActivities[i]);
            }


            // ==========================================
            // 4. POPUP OVERLAY
            // ==========================================
            pnlPopup.Size = new Size(400, 200);
            pnlPopup.Location = new Point((ClientSize.Width - 400) / 2, (ClientSize.Height - 200) / 2);
            pnlPopup.BackColor = Color.White;
            pnlPopup.BorderStyle = BorderStyle.FixedSingle;
            pnlPopup.Visible = false;
            Controls.Add(pnlPopup);
            pnlPopup.BringToFront();

            Label lblPopupMsg = CreateLabel("Activity Selected successfully!", 0, 50, 12, FontStyle.Regular);
            lblPopupMsg.AutoSize = false;
            lblPopupMsg.Size = new Size(400, 30);
            lblPopupMsg.TextAlign = ContentAlignment.MiddleCenter;
            pnlPopup.Controls.Add(lblPopupMsg);

            Button btnOkay = new Button() { Text = "PROCEED TO DASHBOARD ►", Size = new Size(260, 45), Location = new Point(70, 100) };
            btnOkay.FlatStyle = FlatStyle.Flat;
            btnOkay.BackColor = Color.FromArgb(167, 155, 225);
            btnOkay.ForeColor = Color.White;
            btnOkay.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnOkay.FlatAppearance.BorderSize = 0;
            btnOkay.Cursor = Cursors.Hand;
            btnOkay.Click += (s, e) => { MessageBox.Show("Welcome sa inyong Dashboard!", "DEMO App", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            pnlPopup.Controls.Add(btnOkay);
        }

        // ==========================================
        // EVENTS & LOGIC
        // ==========================================
        private void PrimaryEmotion_Click(object sender, EventArgs e)
        {
            Button clickedBtn = (Button)sender;
            selectedPrimary = clickedBtn.Text.Trim();
            currentThemeColor = (Color)clickedBtn.Tag;

            // 1. I-set up ang solid color
            lblSelectedPrimary.Text = selectedPrimary.ToUpper();
            lblSelectedPrimary.Parent.BackColor = currentThemeColor;

            // Hanapin ang PictureBox sa loob ng button para kopyahin ang mukha
            pbSelectedPrimaryIcon.Image = null; // i-clear muna
            foreach (Control c in clickedBtn.Controls)
            {
                if (c is PictureBox)
                {
                    pbSelectedPrimaryIcon.Image = ((PictureBox)c).Image;
                    if (pbSelectedPrimaryIcon.Image != null) pbSelectedPrimaryIcon.BorderStyle = BorderStyle.None;
                    break;
                }
            }

            // 2. Alamin ang mga Secondary Emotions at ang Shade Color
            string[] secondaries = new string[4];
            switch (selectedPrimary.ToUpper())
            {
                case "HAPPY":
                    secondaries = new string[] { "Hopeful", "Proud", "Excited", "Delighted" };
                    currentShadeColor = shadeHappy;
                    break;
                case "SAD":
                    secondaries = new string[] { "Shame", "Neglectful", "Guilty", "Isolated" };
                    currentShadeColor = shadeSad;
                    break;
                case "ANGRY":
                    secondaries = new string[] { "Hate", "Envy", "Jealous", "Annoyed" };
                    currentShadeColor = shadeAngry;
                    break;
                case "FEAR":
                    secondaries = new string[] { "Anxious", "Insecure", "Inferior", "Panic" };
                    currentShadeColor = shadeFear;
                    break;
                case "SURPRISE":
                    secondaries = new string[] { "Shocked", "Dismayed", "Confused", "Perplexed" };
                    currentShadeColor = shadeSurprise;
                    break;
            }

            // 3. I-apply ang Shades
            for (int i = 0; i < 4; i++)
            {
                btnSecondaries[i].Text = secondaries[i];
                btnSecondaries[i].BackColor = currentShadeColor;
            }

            pnlPrimary.Visible = false;
            pnlSecondary.Visible = true;
        }

        private void SecondaryEmotion_Click(object sender, EventArgs e)
        {
            Button clickedBtn = (Button)sender;
            string selectedSecondary = clickedBtn.Text.ToLower();

            List<string> acts = new List<string>();

            // RANKING LOGIC 
            if (selectedPrimary.ToUpper() == "HAPPY")
            {
                if (selectedSecondary == "hopeful") { acts.AddRange(new string[] { "WALK OUTSIDE\n(5 mins)", "JOURNAL", "LISTEN TO MUSIC" }); }
                else if (selectedSecondary == "delighted") { acts.AddRange(new string[] { "LISTEN TO MUSIC", "WALK OUTSIDE\n(5 mins)", "JOURNAL" }); }
                else { acts.AddRange(new string[] { "WALK OUTSIDE\n(5 mins)", "LISTEN TO MUSIC", "JOURNAL" }); }
            }
            else if (selectedPrimary.ToUpper() == "SAD")
            {
                if (selectedSecondary == "shame" || selectedSecondary == "guilty") { acts.AddRange(new string[] { "SLOW WALK\nIN NATURE", "LISTEN TO MUSIC", "JOURNAL" }); }
                else if (selectedSecondary == "neglectful") { acts.AddRange(new string[] { "JOURNAL", "SLOW WALK\nIN NATURE", "LISTEN TO MUSIC" }); }
                else { acts.AddRange(new string[] { "LISTEN TO MUSIC", "JOURNAL", "SLOW WALK\nIN NATURE" }); }
            }
            else if (selectedPrimary.ToUpper() == "FEAR")
            {
                if (selectedSecondary == "anxious" || selectedSecondary == "panic") { acts.AddRange(new string[] { "MEDITATE\n(Breathing)", "GENTLE STRETCHING", "JOURNAL" }); }
                else if (selectedSecondary == "insecure") { acts.AddRange(new string[] { "JOURNAL", "GENTLE STRETCHING", "MEDITATE" }); }
                else { acts.AddRange(new string[] { "GENTLE STRETCHING", "MEDITATE", "JOURNAL" }); }
            }
            else if (selectedPrimary.ToUpper() == "ANGRY")
            {
                if (selectedSecondary == "hate") { acts.AddRange(new string[] { "JOG", "DANCE IT OUT", "JOURNAL" }); }
                else if (selectedSecondary == "jealous") { acts.AddRange(new string[] { "JOURNAL", "JOG", "DANCE IT OUT" }); }
                else if (selectedSecondary == "annoyed") { acts.AddRange(new string[] { "DANCE IT OUT", "JOG", "JOURNAL" }); }
                else { acts.AddRange(new string[] { "JOG", "JOURNAL", "DANCE IT OUT" }); }
            }
            else if (selectedPrimary.ToUpper() == "SURPRISE")
            {
                if (selectedSecondary == "shocked") { acts.AddRange(new string[] { "BREATHING EXERCISE", "SHORT WALK", "JOURNAL" }); }
                else if (selectedSecondary == "confused") { acts.AddRange(new string[] { "JOURNAL", "SHORT WALK", "BREATHING EXERCISE" }); }
                else if (selectedSecondary == "dismayed") { acts.AddRange(new string[] { "SHORT WALK", "BREATHING EXERCISE", "JOURNAL" }); }
                else { acts.AddRange(new string[] { "SHORT WALK", "JOURNAL", "BREATHING EXERCISE" }); }
            }

            // Populate Top 3 Activities and Assign Icons
            for (int i = 0; i < 3; i++)
            {
                lblActTitles[i].Text = acts[i];
                pnlActivities[i].BackColor = currentShadeColor;

                // PARA KAY ALWYN: Automatic na pipili ang system ng Image base sa text ng activity
                // I-uncomment ito kapag na-import na ang mga icons para sa activities:
                string actName = acts[i].ToUpper();
                if (actName.Contains("WALK") || actName.Contains("JOG")) pbActIcons[i].Image = Properties.Resources.Walk;
                else if (actName.Contains("JOURNAL")) pbActIcons[i].Image = Properties.Resources.Journal;
                else if (actName.Contains("MUSIC")) pbActIcons[i].Image = Properties.Resources.Music;
                else if (actName.Contains("MEDITATE") || actName.Contains("BREATHING")) pbActIcons[i].Image = Properties.Resources.Meditate;
                else if (actName.Contains("DANCE")) pbActIcons[i].Image = Properties.Resources.Dance;
                else if (actName.Contains("STRETCHING")) pbActIcons[i].Image = Properties.Resources.Stretch;
            }

            pnlSecondary.Visible = false;
            pnlActivity.Visible = true;
        }

        private void Activity_Click(object sender, EventArgs e)
        {
            pnlPopup.Visible = true;
        }

        // ==========================================
        // UI HELPERS
        // ==========================================
        private Label CreateLabel(string text, int x, int y, float size, FontStyle style)
        {
            return new Label()
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", size, style),
                ForeColor = textColorDark,
                AutoSize = true
            };
        }
    }
}