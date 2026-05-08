using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace DEMO
{
    public partial class DashboardForm : Form
    {
        // Modern Theme Colors 
        private Color bgColor = Color.FromArgb(245, 246, 250);
        private Color cardColor = Color.White;
        private Color textColorDark = Color.FromArgb(45, 52, 54);
        private Color textColorLight = Color.FromArgb(100, 110, 115);
        private Color primaryPurple = Color.FromArgb(167, 155, 225);
        private Color borderColor = Color.FromArgb(223, 228, 234);

        // Data from Emotion Form
        private string currentUserEmail = "";
        private string currentUserName = "";
        private string currentActivityName = "";
        private Image currentActivityImage;
        private Image currentEmotionImage;
        private Color currentEmotionColor;
        private bool isActivityCompleted = false;

        private Label lblClock;

        // ==========================================
        // AI CHAT VARIABLES
        // ==========================================
        private AIChatbot chatbot;
        private FlowLayoutPanel aiChatArea;
        private TextBox txtAi;

        // Custom Class para hawakan ang data ng bawat araw
        private class DayRecord
        {
            public string Primary { get; set; }
            public string Secondary { get; set; }
            public string Activity { get; set; }
            public string DateLogged { get; set; }
        }

        public DashboardForm(string email, string fullName, string activityName, Image actImg, Image emoImg, Color themeColor, bool isCompleted)
        {
            currentUserEmail = email;
            currentUserName = fullName;
            currentActivityName = activityName;
            currentActivityImage = actImg;
            currentEmotionImage = emoImg;
            currentEmotionColor = themeColor;
            isActivityCompleted = isCompleted;

            // !! PAALALA: PALITAN ITO NG TOTOO NIYONG API KEY !!
            chatbot = new AIChatbot("?");

            Text = "DEMO - Dashboard";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = bgColor;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI();

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => { lblClock.Text = DateTime.Now.ToString("hh:mm:ss tt"); };
            timer.Start();
        }

        private void InitializeUI()
        {
            // ==========================================
            // TOP HEADER
            // ==========================================
            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 80, BackColor = cardColor };

            Label lblGreeting = new Label() { Text = $"Hi, {currentUserName}! How are you today?", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = textColorDark, AutoSize = true, Location = new Point(40, 25) };
            pnlHeader.Controls.Add(lblGreeting);

            lblClock = new Label() { Text = DateTime.Now.ToString("hh:mm:ss tt"), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = primaryPurple, AutoSize = true, Location = new Point(730, 25) };
            pnlHeader.Controls.Add(lblClock);

            Label lblLogout = new Label() { Text = "🚪 Log Out", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.IndianRed, AutoSize = true, Location = new Point(910, 30), Cursor = Cursors.Hand };
            lblLogout.Click += (s, e) => {
                if (MessageBox.Show("Are you sure you want to log out?", "Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Application.Restart();
            };
            pnlHeader.Controls.Add(lblLogout);

            PictureBox pbProfile = new PictureBox()
            {
                // Palitan ang "img_DEMO" kung iba ang eksaktong pangalan ng logo niyo sa Resources
                Image = Properties.Resources.img_DEMO,
                SizeMode = PictureBoxSizeMode.Zoom, // Para hindi ma-stretch at sakto lang sa box
                Size = new Size(45, 45), // Pinalaki nang kaunti para mas visible ang logo
                Location = new Point(1000, 18), // In-adjust ang pwesto para pantay sa header
                BackColor = Color.Transparent
            };
            
            // Pwede mo rin itong lagyan ng click event kung gusto mong may bumukas na profile page!
            // pbProfile.Click += (s, e) => { MessageBox.Show("Profile clicked!"); };
            
            pnlHeader.Controls.Add(pbProfile);

            Panel headerBorder = new Panel() { Dock = DockStyle.Bottom, Height = 1, BackColor = borderColor };
            pnlHeader.Controls.Add(headerBorder);
            Controls.Add(pnlHeader);

            // ==========================================
            // LEFT SIDE: CALENDAR
            // ==========================================
            Panel pnlCalendarOuter = new Panel() { Location = new Point(40, 110), Size = new Size(620, 380), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlCalendarInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor };

            Label lblCalTitle = new Label() { Text = "Calendar View", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(20, 20), AutoSize = true };
            Label lblMonth = new Label() { Text = DateTime.Now.ToString("MMM yyyy"), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = primaryPurple, Location = new Point(500, 20), AutoSize = true };
            pnlCalendarInner.Controls.Add(lblCalTitle);
            pnlCalendarInner.Controls.Add(lblMonth);

            TableLayoutPanel calGrid = new TableLayoutPanel()
            {
                Location = new Point(20, 60),
                Size = new Size(580, 300),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = borderColor
            };
            calGrid.ColumnCount = 7;
            calGrid.RowCount = 7;

            calGrid.ColumnStyles.Clear();
            for (int i = 0; i < 7; i++) { calGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f)); }

            calGrid.RowStyles.Clear();
            calGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));
            for (int i = 0; i < 6; i++) { calGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66f)); }

            string[] days = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
            for (int i = 0; i < 7; i++)
            {
                Label lblDay = new Label() { Text = days[i], TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textColorLight, BackColor = cardColor, Margin = new Padding(0) };
                calGrid.Controls.Add(lblDay, i, 0);
            }

            // ==============================================================
            // KUNIN ANG MGA NAKARAANG LOGS SA DATABASE (May Secondary Emotion at Activity na)
            // ==============================================================
            Dictionary<int, DayRecord> monthlyMoods = new Dictionary<int, DayRecord>();
            try
            {
                MariaDbConnector db = new MariaDbConnector();
                db.Connect();

                string sql = "SELECT date_logged, primary_emotion, secondary_emotion, selected_activity FROM mood_entries WHERE email = @email AND MONTH(date_logged) = @month AND YEAR(date_logged) = @year";
                MySqlParameter[] parameters = {
                    new MySqlParameter("@email", currentUserEmail),
                    new MySqlParameter("@month", DateTime.Now.Month),
                    new MySqlParameter("@year", DateTime.Now.Year)
                };

                var results = db.Query(sql, parameters) as List<Dictionary<string, object>>;
                if (results != null)
                {
                    foreach (var row in results)
                    {
                        if (row["date_logged"] != null && row["primary_emotion"] != null)
                        {
                            DateTime date = Convert.ToDateTime(row["date_logged"]);
                            monthlyMoods[date.Day] = new DayRecord()
                            {
                                Primary = row["primary_emotion"].ToString(),
                                Secondary = row.ContainsKey("secondary_emotion") ? row["secondary_emotion"].ToString() : "",
                                Activity = row.ContainsKey("selected_activity") ? row["selected_activity"].ToString() : "N/A",
                                DateLogged = date.ToString("MMMM dd, yyyy")
                            };
                        }
                    }
                }
                db.Disconnect();
            }
            catch { }

            int currentDay = DateTime.Now.Day;
            int dayCounter = 1;

            for (int row = 1; row <= 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if (row == 1 && col < 5)
                    {
                        Panel emptyPanel = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };
                        calGrid.Controls.Add(emptyPanel, col, row);
                        continue;
                    }
                    if (dayCounter > 31)
                    {
                        Panel emptyPanel = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };
                        calGrid.Controls.Add(emptyPanel, col, row);
                        continue;
                    }

                    Panel cellPanel = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0) };
                    Label lblDayNum = new Label() { Text = dayCounter.ToString(), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(5, 5), AutoSize = true };
                    cellPanel.Controls.Add(lblDayNum);

                    // ==============================================================
                    // LOGIC KUNG MAY EMOJI ANG ARAW (Ngayon o Nakaraan)
                    // ==============================================================
                    bool hasData = false;
                    string dayPrimary = "";
                    string daySecondary = "";
                    string dayActivity = "";
                    string dayDateStr = DateTime.Now.ToString("MMMM ") + dayCounter + ", " + DateTime.Now.Year;
                    Color boxColor = Color.White;
                    Image boxImg = null;

                    // BINAGO: Inuna nating i-check ang DATABASE (monthlyMoods) bago ang "currentDay"
                    // Para siguradong kung ano ang nasa DB, iyon ang masusunod na Emotion at Emoji!
                    if (monthlyMoods.ContainsKey(dayCounter))
                    {
                        hasData = true;
                        DayRecord rec = monthlyMoods[dayCounter];
                        dayPrimary = rec.Primary;
                        daySecondary = rec.Secondary;
                        dayActivity = rec.Activity;
                        dayDateStr = rec.DateLogged;

                        string pMood = rec.Primary.ToUpper();
                        if (pMood.Contains("HAPP")) { boxColor = Color.FromArgb(254, 235, 142); boxImg = Properties.Resources.Happy; }
                        else if (pMood.Contains("SAD")) { boxColor = Color.FromArgb(174, 214, 241); boxImg = Properties.Resources.Sad; }
                        else if (pMood.Contains("ANGR")) { boxColor = Color.FromArgb(245, 183, 177); boxImg = Properties.Resources.Angry; }
                        else if (pMood.Contains("FEAR")) { boxColor = Color.FromArgb(210, 180, 222); boxImg = Properties.Resources.Fear; }
                        else if (pMood.Contains("SURPRISE")) { boxColor = Color.FromArgb(169, 223, 191); boxImg = Properties.Resources.Surprise; }
                    }
                    else if (dayCounter == currentDay)
                    {
                        hasData = true;
                        // Fallback kung hindi pa nagsa-save sa DB ngayong araw
                        if (currentEmotionColor == Color.FromArgb(254, 235, 142)) dayPrimary = "HAPPY";
                        else if (currentEmotionColor == Color.FromArgb(174, 214, 241)) dayPrimary = "SAD";
                        else if (currentEmotionColor == Color.FromArgb(245, 183, 177)) dayPrimary = "ANGRY";
                        else if (currentEmotionColor == Color.FromArgb(210, 180, 222)) dayPrimary = "FEAR";
                        else if (currentEmotionColor == Color.FromArgb(169, 223, 191)) dayPrimary = "SURPRISE";
                        else dayPrimary = "MOOD";

                        dayActivity = currentActivityName;
                        boxColor = currentEmotionColor;
                        boxImg = currentEmotionImage;
                    }

                    if (hasData)
                    {
                        cellPanel.BackColor = boxColor;
                        if (boxImg != null)
                        {
                            PictureBox pbEmo = new PictureBox()
                            {
                                Image = boxImg,
                                SizeMode = PictureBoxSizeMode.Zoom,
                                Size = new Size(26, 26),
                                Location = new Point(46, 8),
                                BackColor = Color.Transparent
                            };
                            cellPanel.Controls.Add(pbEmo);
                            pbEmo.BringToFront();

                            // GAWING CLICKABLE ANG PICTUREBOX
                            pbEmo.Cursor = Cursors.Hand;
                            pbEmo.Click += (s, e) => {
                                HistoryViewForm historyForm = new HistoryViewForm(dayDateStr, dayActivity, dayPrimary, daySecondary, "", null, boxImg);
                                historyForm.ShowDialog();
                            };
                        }

                        // GAWING CLICKABLE ANG BUONG BOX
                        cellPanel.Cursor = Cursors.Hand;
                        Image finalImg = boxImg;
                        cellPanel.Click += (s, e) => {
                            HistoryViewForm historyForm = new HistoryViewForm(dayDateStr, dayActivity, dayPrimary, daySecondary, "", null, finalImg);
                            historyForm.ShowDialog();
                        };
                    }
                    // ==============================================================

                    calGrid.Controls.Add(cellPanel, col, row);
                    dayCounter++;
                }
            }

            pnlCalendarInner.Controls.Add(calGrid);
            pnlCalendarOuter.Controls.Add(pnlCalendarInner);
            Controls.Add(pnlCalendarOuter);

            // ==========================================
            // LEFT BOTTOM: ACTIVITY TODAY
            // ==========================================
            Panel pnlActivityOuter = new Panel() { Location = new Point(40, 510), Size = new Size(620, 120), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlActivityInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor };

            Label lblActTitle = new Label() { Text = "Activity Today", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = textColorLight, Location = new Point(20, 15), AutoSize = true };
            pnlActivityInner.Controls.Add(lblActTitle);

            PictureBox pbAct = new PictureBox() { Image = currentActivityImage, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(60, 60), Location = new Point(20, 40) };
            pnlActivityInner.Controls.Add(pbAct);

            Label lblActName = new Label() { Text = currentActivityName.ToUpper(), Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(95, 55), AutoSize = true };
            pnlActivityInner.Controls.Add(lblActName);

            Button btnView = new Button()
            {
                Location = new Point(470, 45),
                Size = new Size(130, 45),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnView.FlatAppearance.BorderSize = 0;

            if (isActivityCompleted)
            {
                btnView.Text = "COMPLETED ✓";
                btnView.BackColor = Color.FromArgb(46, 204, 113);
                btnView.ForeColor = Color.White;
                btnView.Enabled = false;
            }
            else
            {
                btnView.Text = "VIEW";
                btnView.BackColor = primaryPurple;
                btnView.ForeColor = Color.White;
                btnView.Enabled = true;
            }

            btnView.Click += (s, e) => {
                string actCheck = currentActivityName.ToUpper();
                string currentMood = "HAPPY";
                if (currentEmotionColor == Color.FromArgb(174, 214, 241)) currentMood = "SAD";

                Form activityForm;
                if (actCheck.Contains("JOURNAL")) activityForm = new JournalActivityForm();
                else if (actCheck.Contains("MUSIC")) activityForm = new MusicActivityForm(currentActivityName, currentMood);
                else activityForm = new TimerActivityForm(currentActivityName, 5, "⏳");

                if (activityForm.ShowDialog() == DialogResult.OK)
                {
                    btnView.Text = "COMPLETED ✓";
                    btnView.BackColor = Color.FromArgb(46, 204, 113);
                    btnView.ForeColor = Color.White;
                    btnView.Enabled = false;
                    isActivityCompleted = true;

                    try
                    {
                        MariaDbConnector db = new MariaDbConnector();
                        db.Connect();
                        string sql = "UPDATE mood_entries SET is_completed = 1 WHERE email = @email AND date_logged = @date";
                        MySqlParameter[] parameters = {
                            new MySqlParameter("@email", currentUserEmail),
                            new MySqlParameter("@date", DateTime.Now.ToString("yyyy-MM-dd"))
                        };
                        db.Query(sql, parameters);
                        db.Disconnect();
                    }
                    catch { }
                }
            };

            pnlActivityInner.Controls.Add(btnView);
            pnlActivityOuter.Controls.Add(pnlActivityInner);
            Controls.Add(pnlActivityOuter);

            // ==========================================
            // RIGHT SIDE: AI CHAT PANEL
            // ==========================================
            Panel pnlAiOuter = new Panel() { Location = new Point(680, 110), Size = new Size(380, 520), BackColor = borderColor, Padding = new Padding(1) };
            Panel pnlAiInner = new Panel() { Dock = DockStyle.Fill, BackColor = cardColor };

            Panel aiHeader = new Panel() { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(250, 250, 252) };
            Label lblAiTitle = new Label() { Text = "🤖 DEMO. AI", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = textColorDark, Location = new Point(15, 15), AutoSize = true };
            aiHeader.Controls.Add(lblAiTitle);
            Panel aiHeaderBorder = new Panel() { Dock = DockStyle.Bottom, Height = 1, BackColor = borderColor };
            aiHeader.Controls.Add(aiHeaderBorder);

            Panel aiInputArea = new Panel() { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            Panel aiInputBorder = new Panel() { Dock = DockStyle.Top, Height = 1, BackColor = borderColor };
            aiInputArea.Controls.Add(aiInputBorder);

            txtAi = new TextBox() { Text = "Type your message here...", Font = new Font("Segoe UI", 10), ForeColor = textColorLight, BorderStyle = BorderStyle.None, Location = new Point(20, 20), Size = new Size(300, 20) };
            txtAi.Enter += (s, e) => { if (txtAi.Text == "Type your message here...") { txtAi.Text = ""; txtAi.ForeColor = textColorDark; } };

            txtAi.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await SendMessageToAI();
                }
            };
            aiInputArea.Controls.Add(txtAi);

            Label lblSend = new Label() { Text = "➤", Font = new Font("Segoe UI", 14), ForeColor = primaryPurple, Location = new Point(340, 15), AutoSize = true, Cursor = Cursors.Hand };
            lblSend.Click += async (s, e) => await SendMessageToAI();
            aiInputArea.Controls.Add(lblSend);

            aiChatArea = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = cardColor,
                Padding = new Padding(15, 20, 15, 20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            pnlAiInner.Controls.Add(aiHeader);
            pnlAiInner.Controls.Add(aiInputArea);
            pnlAiInner.Controls.Add(aiChatArea);
            aiChatArea.BringToFront();

            pnlAiOuter.Controls.Add(pnlAiInner);
            Controls.Add(pnlAiOuter);

            string cleanActName = currentActivityName.Replace("\n", " ");
            AddChatMessage("DEMO AI", $"Hi {currentUserName}! I see you're feeling a bit of that emotion today. How can I assist you with your '{cleanActName}'?", true);
        }

        private void AddChatMessage(string sender, string message, bool isAI)
        {
            Label lblMsg = new Label();
            lblMsg.Text = $"{sender}:\n{message}\n\n";
            lblMsg.Font = new Font("Segoe UI", 10);
            lblMsg.ForeColor = isAI ? textColorDark : primaryPurple;
            lblMsg.BackColor = isAI ? bgColor : Color.FromArgb(235, 245, 255);
            lblMsg.AutoSize = true;
            lblMsg.MaximumSize = new Size(310, 0);
            lblMsg.Padding = new Padding(10);
            lblMsg.Margin = new Padding(0, 0, 0, 20);

            aiChatArea.Controls.Add(lblMsg);

            aiChatArea.PerformLayout();
            aiChatArea.ScrollControlIntoView(lblMsg);
            try { aiChatArea.VerticalScroll.Value = aiChatArea.VerticalScroll.Maximum; } catch { }
        }

        private async Task SendMessageToAI()
        {
            string userText = txtAi.Text.Trim();
            if (string.IsNullOrEmpty(userText) || userText == "Type your message here...") return;

            AddChatMessage("You", userText, false);
            txtAi.Text = "";

            txtAi.Enabled = false;
            Label lblTyping = new Label() { Text = "AI is typing...", ForeColor = textColorLight, Font = new Font("Segoe UI", 9, FontStyle.Italic), AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            aiChatArea.Controls.Add(lblTyping);
            aiChatArea.ScrollControlIntoView(lblTyping);

            string aiReply = await chatbot.ChatAsync(userText);

            aiChatArea.Controls.Remove(lblTyping);
            AddChatMessage("DEMO AI", aiReply, true);

            txtAi.Enabled = true;
            txtAi.Focus();
        }
    }
}
