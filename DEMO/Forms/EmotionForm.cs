using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

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
        private Color colHappy = Color.FromArgb(254, 235, 142);
        private Color colSad = Color.FromArgb(174, 214, 241);
        private Color colAngry = Color.FromArgb(245, 183, 177);
        private Color colFear = Color.FromArgb(210, 180, 222);
        private Color colSurprise = Color.FromArgb(169, 223, 191);

        // Secondary Emotion Colors (Shades of Primary)
        private Color shadeHappy = Color.FromArgb(255, 246, 191);
        private Color shadeSad = Color.FromArgb(206, 230, 248);
        private Color shadeAngry = Color.FromArgb(250, 209, 205);
        private Color shadeFear = Color.FromArgb(228, 208, 237);
        private Color shadeSurprise = Color.FromArgb(203, 237, 217);

        // Current State
        private string selectedPrimary = "";
        private Color currentThemeColor = Color.White;
        private Color currentShadeColor = Color.White;

        private string currentUserEmail = "";
        private string currentFullName = "";

        private string finalSelectedActivity = "Journal";
        private string finalSelectedSecondary = "";

        // ==========================================
        // BAGONG VARIABLES PARA SA "ONE EMOTION PER DAY"
        // ==========================================
        private bool hasLoggedToday = false;
        private string savedPrimary = "";
        private string savedSecondary = "";
        private string savedActivity = "";
        private bool savedIsCompleted = false; // IDINAGDAG: Para matandaan kung Completed na ba

        private Label lblSelectedPrimary;
        private PictureBox pbSelectedPrimaryIcon;
        private Button[] btnSecondaries = new Button[4];

        // Activity UI Elements
        private Panel[] pnlActivities = new Panel[3];
        private Label[] lblActTitles = new Label[3];
        private Label[] lblActTags = new Label[3];
        private PictureBox[] pbActIcons = new PictureBox[3];

        public EmotionForm(string email = "test@email.com", string fullName = "User")
        {
            currentUserEmail = email;
            currentFullName = fullName;

            InitializeComponent();

            // 1. I-CHECK AGAD SA DATABASE KUNG MAY LOG NA NGAYONG ARAW BAGO MAG-LOAD ANG UI
            CheckIfLoggedToday();

            Text = "DEMO - Emotion Check-in";
            Size = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bgColor;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI();
        }

        // ==========================================
        // DATABASE CHECK LOGIC (1 DAY = 1 EMOTION)
        // ==========================================
        private void CheckIfLoggedToday()
        {
            try
            {
                MariaDbConnector db = new MariaDbConnector();
                db.Connect();

                // BINAGO: Isinama na ang is_completed sa kukunin sa Database
                string sql = "SELECT primary_emotion, secondary_emotion, selected_activity, is_completed FROM mood_entries WHERE email = @email AND date_logged = @date LIMIT 1";
                MySqlParameter[] parameters = {
                    new MySqlParameter("@email", currentUserEmail),
                    new MySqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd"))
                };

                var result = db.Query(sql, parameters) as List<Dictionary<string, object>>;

                if (result != null && result.Count > 0)
                {
                    hasLoggedToday = true; // May nahanap! I-lock ang system.
                    savedPrimary = result[0]["primary_emotion"].ToString();
                    savedSecondary = result[0]["secondary_emotion"].ToString();
                    savedActivity = result[0]["selected_activity"].ToString();

                    // Ligtas na pagkuha ng true/false mula sa Database
                    if (result[0].ContainsKey("is_completed") && result[0]["is_completed"] != null)
                    {
                        string val = result[0]["is_completed"].ToString();
                        savedIsCompleted = (val == "1" || val.ToLower() == "true");
                    }
                }
                db.Disconnect();
            }
            catch { }
        }

        // ==========================================
        // KAPAG LUMABAS NA ANG SCREEN, IDIDIRETso KUNG MAY LOG NA
        // ==========================================
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (hasLoggedToday)
            {
                this.Hide();

                // Kapag maglo-login ulit, tahimik na lang papasok sa dashboard nang walang nakakairitang message box

                selectedPrimary = savedPrimary;
                finalSelectedSecondary = savedSecondary;

                string checkP = selectedPrimary.ToUpper();
                if (checkP.Contains("HAPP")) currentThemeColor = colHappy;
                else if (checkP.Contains("SAD")) currentThemeColor = colSad;
                else if (checkP.Contains("ANGR")) currentThemeColor = colAngry;
                else if (checkP.Contains("FEAR")) currentThemeColor = colFear;
                else if (checkP.Contains("SURPRISE")) currentThemeColor = colSurprise;

                finalSelectedActivity = savedActivity;

                ProceedToDashboard(false);
            }
        }

        private void InitializeUI()
        {
            // ==========================================
            // 1. PRIMARY EMOTION SCREEN
            // ==========================================
            pnlPrimary.Dock = DockStyle.Fill;
            pnlPrimary.BackColor = bgColor;
            Controls.Add(pnlPrimary);

            Label lblTopLeft = CreateLabel("↺ BACK", 30, 20, 10, FontStyle.Bold);
            lblTopLeft.ForeColor = textColorLight;
            lblTopLeft.Cursor = Cursors.Hand;
            pnlPrimary.Controls.Add(lblTopLeft);

            Label lblTopRight = CreateLabel($"👤 Welcome, {currentFullName}", ClientSize.Width - 160, 20, 10, FontStyle.Regular);
            lblTopRight.ForeColor = textColorDark;
            lblTopRight.Location = new Point(ClientSize.Width - lblTopRight.PreferredWidth - 30, 20);
            pnlPrimary.Controls.Add(lblTopRight);

            Label lblQuestion = CreateLabel("What are you feeling today?", 0, 90, 24, FontStyle.Bold);
            lblQuestion.Location = new Point((ClientSize.Width - lblQuestion.PreferredWidth) / 2, 90);
            pnlPrimary.Controls.Add(lblQuestion);

            Label lblSubTitle = CreateLabel("Choose the emotion that best matches your current mood.", 0, 140, 11, FontStyle.Regular);
            lblSubTitle.ForeColor = textColorLight;
            lblSubTitle.Location = new Point((ClientSize.Width - lblSubTitle.PreferredWidth) / 2, 140);
            pnlPrimary.Controls.Add(lblSubTitle);

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

                PictureBox pbSlot = new PictureBox();
                pbSlot.Size = new Size(90, 90);
                pbSlot.Location = new Point((boxWidth - 90) / 2, 20);
                pbSlot.BackColor = Color.Transparent;
                pbSlot.SizeMode = PictureBoxSizeMode.Zoom;
                pbSlot.BorderStyle = BorderStyle.None;

                try
                {
                    if (primaries[i] == "Happy") pbSlot.Image = Properties.Resources.Happy;
                    else if (primaries[i] == "Sad") pbSlot.Image = Properties.Resources.Sad;
                    else if (primaries[i] == "Angry") pbSlot.Image = Properties.Resources.Angry;
                    else if (primaries[i] == "Fear") pbSlot.Image = Properties.Resources.Fear;
                    else if (primaries[i] == "Surprise") pbSlot.Image = Properties.Resources.Surprise;
                }
                catch { }

                pbSlot.Click += (s, ev) => btn.PerformClick();

                btn.Controls.Add(pbSlot);
                btn.Click += PrimaryEmotion_Click;
                pnlPrimary.Controls.Add(btn);
            }

            Label lblBottomText = CreateLabel("Your mood helps us recommend the best activities for today.", 0, 400, 10, FontStyle.Regular);
            lblBottomText.ForeColor = textColorLight;
            lblBottomText.Location = new Point((ClientSize.Width - lblBottomText.PreferredWidth) / 2, 400);
            pnlPrimary.Controls.Add(lblBottomText);

            Label lblBottomBack = CreateLabel("Back", 30, ClientSize.Height - 60, 10, FontStyle.Regular);
            lblBottomBack.ForeColor = textColorLight;
            pnlPrimary.Controls.Add(lblBottomBack);

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

            Panel boxPrimary = new Panel() { Size = new Size(240, 280), Location = new Point(160, 140) };

            pbSelectedPrimaryIcon = new PictureBox();
            pbSelectedPrimaryIcon.Size = new Size(140, 140);
            pbSelectedPrimaryIcon.Location = new Point(50, 45);
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
            // 3. ACTIVITY SCREEN
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

            int actBoxWidth = 240;
            int actBoxHeight = 220;
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

                pbActIcons[i] = new PictureBox();
                pbActIcons[i].Size = new Size(90, 90);
                pbActIcons[i].Location = new Point((actBoxWidth - 90) / 2, 50);
                pbActIcons[i].BackColor = Color.Transparent;
                pbActIcons[i].SizeMode = PictureBoxSizeMode.Zoom;
                pbActIcons[i].BorderStyle = BorderStyle.None;
                pbActIcons[i].Click += Activity_Click;

                lblActTitles[i] = CreateLabel("ACTIVITY NAME", 0, 150, 12, FontStyle.Bold);
                lblActTitles[i].AutoSize = false;
                lblActTitles[i].Size = new Size(actBoxWidth, 60);
                lblActTitles[i].TextAlign = ContentAlignment.MiddleCenter;
                lblActTitles[i].Click += Activity_Click;

                pnlActivities[i].Controls.Add(lblActTags[i]);
                pnlActivities[i].Controls.Add(pbActIcons[i]);
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

            // Tinatawag ang ProceedToDashboard method kung saan naka-set sa "true" ang pag-save
            btnOkay.Click += (s, e) => ProceedToDashboard(true);

            pnlPopup.Controls.Add(btnOkay);
        }

        // ==========================================
        // METHOD PARA SA DASHBOARD TRANSITION AT SAVING
        // ==========================================
        private void ProceedToDashboard(bool saveToDb)
        {
            if (saveToDb)
            {
                try
                {
                    MariaDbConnector db = new MariaDbConnector();
                    db.Connect();

                    string sql = "INSERT INTO mood_entries (email, primary_emotion, secondary_emotion, selected_activity, is_completed, date_logged) VALUES (@email, @primary, @secondary, @activity, @is_completed, @date)";
                    MySqlParameter[] parameters = {
                        new MySqlParameter("@email", currentUserEmail),
                        new MySqlParameter("@primary", selectedPrimary),
                        new MySqlParameter("@secondary", finalSelectedSecondary),
                        new MySqlParameter("@activity", finalSelectedActivity),
                        new MySqlParameter("@is_completed", 0), // Default value ay 0 (False) dahil kaka-select pa lang
                        new MySqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd"))
                    };

                    db.Query(sql, parameters);
                    db.Disconnect();
                }
                catch { }
            }

            this.Hide();

            string selectedActivity = finalSelectedActivity;
            Image actImg = null;
            string checkAct = selectedActivity.ToUpper();
            try
            {
                if (checkAct.Contains("JOURNAL")) actImg = Properties.Resources.Journal;
                else if (checkAct.Contains("WALK OUTSIDE") || checkAct.Contains("SLOW WALK IN NATURE") || checkAct.Contains("JOG") || checkAct.Contains("SHORT WALK")) actImg = Properties.Resources.Walk;
                else if (checkAct.Contains("LISTEN TO MUSIC") || checkAct.Contains("LIESTEN TO MUSIC")) actImg = Properties.Resources.Music;
                else if (checkAct.Contains("DANCE IT OUT")) actImg = Properties.Resources.Dance;
                else if (checkAct.Contains("MEDITATE") || checkAct.Contains("BREATHING EXERCISE")) actImg = Properties.Resources.Meditate;
                else if (checkAct.Contains("GENTLE STRETCHING") || checkAct.Contains("STRETCH")) actImg = Properties.Resources.Stretch;
            }
            catch { }

            Image emoImg = null;
            string checkEmo = selectedPrimary.ToUpper();
            try
            {
                if (checkEmo.Contains("HAPPY")) emoImg = Properties.Resources.Happy;
                else if (checkEmo.Contains("SAD")) emoImg = Properties.Resources.Sad;
                else if (checkEmo.Contains("ANGRY") || checkEmo.Contains("ANGER")) emoImg = Properties.Resources.Angry;
                else if (checkEmo.Contains("FEAR")) emoImg = Properties.Resources.Fear;
                else if (checkEmo.Contains("SURPRISE")) emoImg = Properties.Resources.Surprise;
            }
            catch { }

            // BINAGO: IPINAPASA NA ANG currentUserEmail at savedIsCompleted sa DashboardForm
            DashboardForm dashboard = new DashboardForm(currentUserEmail, currentFullName, selectedActivity, actImg, emoImg, currentThemeColor, savedIsCompleted);
            dashboard.FormClosed += (senderForm, args) => this.Close();
            dashboard.Show();
        }

        // ==========================================
        // EVENTS & LOGIC
        // ==========================================
        private void PrimaryEmotion_Click(object sender, EventArgs e)
        {
            Button clickedBtn = (Button)sender;
            selectedPrimary = clickedBtn.Text.Trim();
            currentThemeColor = (Color)clickedBtn.Tag;

            lblSelectedPrimary.Text = selectedPrimary.ToUpper();
            lblSelectedPrimary.Parent.BackColor = currentThemeColor;

            pbSelectedPrimaryIcon.Image = null;
            foreach (Control c in clickedBtn.Controls)
            {
                if (c is PictureBox)
                {
                    pbSelectedPrimaryIcon.Image = ((PictureBox)c).Image;
                    if (pbSelectedPrimaryIcon.Image != null) pbSelectedPrimaryIcon.BorderStyle = BorderStyle.None;
                    break;
                }
            }

            string[] secondaries = new string[4];
            switch (selectedPrimary.ToUpper())
            {
                case "HAPPY": secondaries = new string[] { "Hopeful", "Proud", "Excited", "Delighted" }; currentShadeColor = shadeHappy; break;
                case "SAD": secondaries = new string[] { "Shame", "Neglectful", "Guilty", "Isolated" }; currentShadeColor = shadeSad; break;
                case "ANGRY": secondaries = new string[] { "Hate", "Envy", "Jealous", "Annoyed" }; currentShadeColor = shadeAngry; break;
                case "FEAR": secondaries = new string[] { "Anxious", "Insecure", "Inferior", "Panic" }; currentShadeColor = shadeFear; break;
                case "SURPRISE": secondaries = new string[] { "Shocked", "Dismayed", "Confused", "Perplexed" }; currentShadeColor = shadeSurprise; break;
            }

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
            finalSelectedSecondary = clickedBtn.Text;

            // Para makakuha ng listahan ng activities
            List<string> acts = GetActivityList(selectedPrimary, selectedSecondary);

            for (int i = 0; i < 3; i++)
            {
                lblActTitles[i].Text = acts[i];
                pnlActivities[i].BackColor = currentShadeColor;

                string actName = acts[i].ToUpper();
                try
                {
                    if (actName.Contains("WALK") || actName.Contains("JOG")) pbActIcons[i].Image = Properties.Resources.Walk;
                    else if (actName.Contains("JOURNAL")) pbActIcons[i].Image = Properties.Resources.Journal;
                    else if (actName.Contains("MUSIC")) pbActIcons[i].Image = Properties.Resources.Music;
                    else if (actName.Contains("MEDITATE") || actName.Contains("BREATHING")) pbActIcons[i].Image = Properties.Resources.Meditate;
                    else if (actName.Contains("DANCE")) pbActIcons[i].Image = Properties.Resources.Dance;
                    else if (actName.Contains("STRETCHING")) pbActIcons[i].Image = Properties.Resources.Stretch;
                }
                catch { }
            }

            pnlSecondary.Visible = false;
            pnlActivity.Visible = true;
        }

        private void Activity_Click(object sender, EventArgs e)
        {
            Control clickedCtrl = (Control)sender;
            Panel parentBox = clickedCtrl as Panel;

            if (parentBox == null) parentBox = clickedCtrl.Parent as Panel;

            if (parentBox != null)
            {
                foreach (Control ctrl in parentBox.Controls)
                {
                    if (ctrl is Label lbl && !lbl.Text.Contains("RECOMMENDED") && !string.IsNullOrWhiteSpace(lbl.Text))
                    {
                        finalSelectedActivity = lbl.Text.Trim();
                        break;
                    }
                }
            }
            pnlPopup.Visible = true;
        }

        // ==========================================
        // HELPER METHODS PARA SA BYPASS / RECOMMENDATION LOGIC
        // ==========================================
        private string GetTopActivity(string primary, string secondary)
        {
            List<string> acts = GetActivityList(primary, secondary);
            if (acts.Count > 0) return acts[0].Replace("\n", " ");
            return "Journal";
        }

        private List<string> GetActivityList(string primary, string secondary)
        {
            string p = primary.ToUpper();
            string s = secondary.ToLower();
            List<string> acts = new List<string>();

            if (p == "HAPPY")
            {
                if (s == "hopeful") acts.AddRange(new string[] { "WALK OUTSIDE", "JOURNAL", "LISTEN TO MUSIC" });
                else if (s == "delighted") acts.AddRange(new string[] { "LISTEN TO MUSIC", "WALK OUTSIDE", "JOURNAL" });
                else acts.AddRange(new string[] { "WALK OUTSIDE", "LISTEN TO MUSIC", "JOURNAL" });
            }
            else if (p == "SAD")
            {
                if (s == "shame" || s == "guilty") acts.AddRange(new string[] { "SHORT WALK", "LISTEN TO MUSIC", "JOURNAL" });
                else if (s == "neglectful") acts.AddRange(new string[] { "JOURNAL", "SHORT WALK", "LISTEN TO MUSIC" });
                else acts.AddRange(new string[] { "LISTEN TO MUSIC", "JOURNAL", "SHORT WALK" });
            }
            else if (p == "FEAR")
            {
                if (s == "anxious" || s == "panic") acts.AddRange(new string[] { "MEDITATE", "GENTLE STRETCHING", "JOURNAL" });
                else if (s == "insecure") acts.AddRange(new string[] { "JOURNAL", "GENTLE STRETCHING", "MEDITATE" });
                else acts.AddRange(new string[] { "GENTLE STRETCHING", "MEDITATE", "JOURNAL" });
            }
            else if (p == "ANGRY")
            {
                if (s == "hate") acts.AddRange(new string[] { "JOG", "DANCE IT OUT", "JOURNAL" });
                else if (s == "jealous") acts.AddRange(new string[] { "JOURNAL", "JOG", "DANCE IT OUT" });
                else if (s == "annoyed") acts.AddRange(new string[] { "DANCE IT OUT", "JOG", "JOURNAL" });
                else acts.AddRange(new string[] { "JOG", "JOURNAL", "DANCE IT OUT" });
            }
            else if (p == "SURPRISE")
            {
                if (s == "shocked") acts.AddRange(new string[] { "BREATHING EXERCISE", "SHORT WALK", "JOURNAL" });
                else if (s == "confused") acts.AddRange(new string[] { "JOURNAL", "SHORT WALK", "BREATHING EXERCISE" });
                else if (s == "dismayed") acts.AddRange(new string[] { "SHORT WALK", "BREATHING EXERCISE", "JOURNAL" });
                else acts.AddRange(new string[] { "SHORT WALK", "JOURNAL", "BREATHING EXERCISE" });
            }

            return acts;
        }

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