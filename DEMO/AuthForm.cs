using System;
using System.Drawing;
using System.Windows.Forms;

namespace DEMO
{
    public partial class AuthForm : Form
    {
        private Panel pnlLogin = new Panel();
        private Panel pnlSignup = new Panel();

        // Custom Colors 
        private Color primaryPurple = Color.FromArgb(167, 155, 225);
        private Color inputBgColor = Color.FromArgb(235, 237, 242);
        private Color textColorDark = Color.FromArgb(30, 30, 30);
        private Color textColorLight = Color.FromArgb(100, 100, 100);

        public AuthForm()
        {
            InitializeComponent();

            // Form Setup - Split-Screen Layout
            Text = "DEMO - Authentication";
            Size = new Size(850, 550);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 252);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // ==========================================
            // LEFT SIDE: BRANDING (Logo, Title, Slogan)
            // ==========================================
            int leftCenterX = 240;

            PictureBox pbLogo = new PictureBox();
            pbLogo.Size = new Size(140, 140);
            pbLogo.Location = new Point(leftCenterX - (pbLogo.Width / 2), 110);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.BackColor = Color.Transparent;
            pbLogo.Image = Properties.Resources.img_DEMO;
            Controls.Add(pbLogo);

            Label lblTitle = new Label() { Text = "DEMO", Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = textColorDark, AutoSize = true };
            lblTitle.Location = new Point(leftCenterX - (lblTitle.PreferredWidth / 2), pbLogo.Bottom + 5);
            Controls.Add(lblTitle);

            Label lblSlogan = new Label() { Text = "One emotion at a time", Font = new Font("Segoe UI", 11, FontStyle.Italic), ForeColor = textColorLight, AutoSize = true };
            lblSlogan.Location = new Point(leftCenterX - (lblSlogan.PreferredWidth / 2), lblTitle.Bottom + 5);
            Controls.Add(lblSlogan);


            // ==========================================
            // RIGHT SIDE: AUTHENTICATION CARDS
            // ==========================================
            int rightPanelX = 430;
            // Niliitan ang height ng panels para mas compact matapos tanggalin ang Forgot Password
            int loginPanelY = (ClientSize.Height - 400) / 2;
            int signupPanelY = (ClientSize.Height - 480) / 2;

            // ==========================================
            // SETUP NG LOGIN PANEL
            // ==========================================
            pnlLogin.Size = new Size(360, 400); // Mula 440, ginawang 400
            pnlLogin.Location = new Point(rightPanelX, loginPanelY);
            pnlLogin.BackColor = Color.White;
            pnlLogin.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(pnlLogin);

            Label lblLoginHeader = new Label() { Text = "Welcome Back", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = textColorDark, AutoSize = true };
            lblLoginHeader.Location = new Point((pnlLogin.Width - lblLoginHeader.PreferredWidth) / 2, 30);
            pnlLogin.Controls.Add(lblLoginHeader);

            Label lblSubHeader = new Label() { Text = "Log in to continue tracking your mood", Font = new Font("Segoe UI", 9), ForeColor = textColorLight, AutoSize = true };
            lblSubHeader.Location = new Point((pnlLogin.Width - lblSubHeader.PreferredWidth) / 2, 70);
            pnlLogin.Controls.Add(lblSubHeader);

            // Textboxes
            AddModernInput(pnlLogin, "Email", 120, false);
            AddModernInput(pnlLogin, "Password", 180, true);

            // Login Button (Ini-angat natin sa 250 mula 280)
            Button btnLogin = new Button() { Text = "Login", Location = new Point(30, 250), Size = new Size(300, 45), BackColor = primaryPurple, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;

            // TEMPORARY LOGIC: Pag-click sa Login, lilipat sa EmotionForm
            btnLogin.Click += (s, e) =>
            {
                this.Hide(); // Itatago muna ang AuthForm
                EmotionForm emotionForm = new EmotionForm();
                emotionForm.FormClosed += (senderForm, args) => this.Close(); // Para isara ang buong app kapag in-exit ang EmotionForm
                emotionForm.Show();
            };

            pnlLogin.Controls.Add(btnLogin);

            // Bottom Text (Ini-angat natin sa 315 mula 345)
            Label lblNoAccount = new Label() { Text = "Don't have an account?", Font = new Font("Segoe UI", 9), ForeColor = textColorDark, AutoSize = true };
            LinkLabel linkSignUp = new LinkLabel() { Text = "Sign up", Font = new Font("Segoe UI", 9), LinkColor = primaryPurple, ActiveLinkColor = Color.Blue, AutoSize = true, LinkBehavior = LinkBehavior.NeverUnderline };
            linkSignUp.LinkClicked += (s, e) => { pnlLogin.Visible = false; pnlSignup.Visible = true; };

            pnlLogin.Controls.Add(lblNoAccount);
            pnlLogin.Controls.Add(linkSignUp);

            int loginSpacing = 3;
            int totalLoginWidth = lblNoAccount.PreferredWidth + linkSignUp.PreferredWidth + loginSpacing;
            int startXLogin = (pnlLogin.Width - totalLoginWidth) / 2;
            lblNoAccount.Location = new Point(startXLogin, 315);
            linkSignUp.Location = new Point(startXLogin + lblNoAccount.PreferredWidth + loginSpacing, 315);


            // ==========================================
            // SETUP NG SIGN-UP PANEL
            // ==========================================
            pnlSignup.Size = new Size(360, 480); // Mula 500, ginawang 480
            pnlSignup.Location = new Point(rightPanelX, signupPanelY);
            pnlSignup.BackColor = Color.White;
            pnlSignup.BorderStyle = BorderStyle.FixedSingle;
            pnlSignup.Visible = false; // Nakatago by default
            Controls.Add(pnlSignup);

            Label lblSignUpHeader = new Label() { Text = "Create an Account", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = textColorDark, AutoSize = true };
            lblSignUpHeader.Location = new Point((pnlSignup.Width - lblSignUpHeader.PreferredWidth) / 2, 30);
            pnlSignup.Controls.Add(lblSignUpHeader);

            // Textboxes
            AddModernInput(pnlSignup, "Full Name", 90, false);
            AddModernInput(pnlSignup, "Email", 150, false);
            AddModernInput(pnlSignup, "Password", 210, true);
            AddModernInput(pnlSignup, "Confirm Password", 270, true);

            // Sign Up Button (Ini-angat natin sa 340)
            Button btnSignup = new Button() { Text = "Sign Up", Location = new Point(30, 340), Size = new Size(300, 45), BackColor = primaryPurple, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSignup.FlatAppearance.BorderSize = 0;
            pnlSignup.Controls.Add(btnSignup);

            // Bottom Text (Ini-angat natin sa 405)
            Label lblHasAccount = new Label() { Text = "Already have an account?", Font = new Font("Segoe UI", 9), ForeColor = textColorDark, AutoSize = true };
            LinkLabel linkLogin = new LinkLabel() { Text = "Log in", Font = new Font("Segoe UI", 9), LinkColor = primaryPurple, ActiveLinkColor = Color.Blue, AutoSize = true, LinkBehavior = LinkBehavior.NeverUnderline };
            linkLogin.LinkClicked += (s, e) => { pnlSignup.Visible = false; pnlLogin.Visible = true; };

            pnlSignup.Controls.Add(lblHasAccount);
            pnlSignup.Controls.Add(linkLogin);

            int signupSpacing = 3;
            int totalSignupWidth = lblHasAccount.PreferredWidth + linkLogin.PreferredWidth + signupSpacing;
            int startXSignup = (pnlSignup.Width - totalSignupWidth) / 2;
            lblHasAccount.Location = new Point(startXSignup, 405);
            linkLogin.Location = new Point(startXSignup + lblHasAccount.PreferredWidth + signupSpacing, 405);
        }

        // ==========================================
        // HELPER METHOD: Para magmukhang modern yung Input Boxes
        // ==========================================
        private void AddModernInput(Panel parent, string placeholder, int yPosition, bool isPassword)
        {
            Panel pnlBg = new Panel()
            {
                Location = new Point(30, yPosition),
                Size = new Size(300, 45),
                BackColor = inputBgColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            TextBox txtInput = new TextBox()
            {
                Text = placeholder,
                ForeColor = textColorLight,
                BackColor = inputBgColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                Location = new Point(15, 12),
                Size = new Size(270, 20)
            };

            txtInput.Enter += (s, e) =>
            {
                if (txtInput.Text == placeholder)
                {
                    txtInput.Text = "";
                    txtInput.ForeColor = textColorDark;
                    if (isPassword) txtInput.UseSystemPasswordChar = true;
                }
            };

            txtInput.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtInput.Text))
                {
                    txtInput.Text = placeholder;
                    txtInput.ForeColor = textColorLight;
                    if (isPassword) txtInput.UseSystemPasswordChar = false;
                }
            };

            pnlBg.Controls.Add(txtInput);
            parent.Controls.Add(pnlBg);
        }
    }
}