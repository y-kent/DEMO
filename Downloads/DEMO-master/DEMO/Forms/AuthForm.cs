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
            // LEFT SIDE: BRANDING
            // ==========================================
            int leftCenterX = 240;

            PictureBox pbLogo = new PictureBox();
            pbLogo.Size = new Size(140, 140);
            pbLogo.Location = new Point(leftCenterX - (pbLogo.Width / 2), 110);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.BackColor = Color.Transparent;

            // IBINALIK ANG LOGO: Pinalitan ng try-catch para iwas-error kung iba ang pangalan ng Logo ninyo sa Resources
            try
            {
                // PARA KAY ALWYN: Kung iba ang pangalan ng inyong logo, palitan ang "LogoIcon" dito
                pbLogo.Image = Properties.Resources.img_DEMO;
            }
            catch { }

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
            int loginPanelY = (ClientSize.Height - 400) / 2;
            int signupPanelY = (ClientSize.Height - 480) / 2;

            // ==========================================
            // SETUP NG LOGIN PANEL
            // ==========================================
            pnlLogin.Size = new Size(360, 400);
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

            AddModernInput(pnlLogin, "txtLoginEmail", "Email", 120, false);
            AddModernInput(pnlLogin, "txtLoginPass", "Password", 180, true);

            Button btnLogin = new Button() { Text = "Login", Location = new Point(30, 250), Size = new Size(300, 45), BackColor = primaryPurple, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;

            // ==========================================
            // BACKEND LOGIC: LOGIN
            // ==========================================
            btnLogin.Click += (s, e) =>
            {
                // Gumamit ng .Trim() para mawala ang mga accidental spaces sa dulo ng email
                string emailInput = GetInputValue(pnlLogin, "txtLoginEmail", "Email").Trim();
                string passInput = GetInputValue(pnlLogin, "txtLoginPass", "Password");

                if (string.IsNullOrWhiteSpace(emailInput) || emailInput == "Email")
                {
                    MessageBox.Show("Please enter your email.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                User loginUser = new User();
                bool isSuccess = loginUser.Login(emailInput, passInput);

                if (isSuccess)
                {
                    // 1. Kukunin natin ang Full Name sa Database
                    string fullName = emailInput; // Fallback kung sakaling mag-error
                    try
                    {
                        string connStr = "server=localhost;user=root;password=;database=emotiondb;";
                        using (MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connStr))
                        {
                            conn.Open();
                            // PAALALA KAY ALWYN: Siguraduhing tama itong "fullname" na column base sa users table ninyo! 
                            // (Baka "full_name" o "name" ang ginamit niya)
                            using (MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT fullname FROM users WHERE email=@email", conn))
                            {
                                cmd.Parameters.AddWithValue("@email", emailInput);
                                var result = cmd.ExecuteScalar();
                                if (result != null) fullName = result.ToString();
                            }
                        }
                    }
                    catch { }

                    MessageBox.Show("Login successful! Welcome back.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();

                    // 2. IPAPASA NA NATIN ANG EMAIL (Para sa DB) AT FULLNAME (Para sa UI)
                    EmotionForm emotionForm = new EmotionForm(emailInput, fullName);
                    emotionForm.FormClosed += (senderForm, args) => this.Close();
                    emotionForm.Show();
                }
                else
                {
                    MessageBox.Show("Invalid email or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            pnlLogin.Controls.Add(btnLogin);

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
            pnlSignup.Size = new Size(360, 480);
            pnlSignup.Location = new Point(rightPanelX, signupPanelY);
            pnlSignup.BackColor = Color.White;
            pnlSignup.BorderStyle = BorderStyle.FixedSingle;
            pnlSignup.Visible = false;
            Controls.Add(pnlSignup);

            Label lblSignUpHeader = new Label() { Text = "Create an Account", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = textColorDark, AutoSize = true };
            lblSignUpHeader.Location = new Point((pnlSignup.Width - lblSignUpHeader.PreferredWidth) / 2, 30);
            pnlSignup.Controls.Add(lblSignUpHeader);

            AddModernInput(pnlSignup, "txtRegName", "Full Name", 90, false);
            AddModernInput(pnlSignup, "txtRegEmail", "Email", 150, false);
            AddModernInput(pnlSignup, "txtRegPass", "Password", 210, true);
            AddModernInput(pnlSignup, "txtRegConfirm", "Confirm Password", 270, true);

            Button btnSignup = new Button() { Text = "Sign Up", Location = new Point(30, 340), Size = new Size(300, 45), BackColor = primaryPurple, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSignup.FlatAppearance.BorderSize = 0;

            // ==========================================
            // BACKEND LOGIC: SIGN UP
            // ==========================================
            btnSignup.Click += (s, e) =>
            {
                // Gumamit din ng .Trim() para sa signup
                string nameInput = GetInputValue(pnlSignup, "txtRegName", "Full Name").Trim();
                string emailInput = GetInputValue(pnlSignup, "txtRegEmail", "Email").Trim();
                string passInput = GetInputValue(pnlSignup, "txtRegPass", "Password");
                string confirmInput = GetInputValue(pnlSignup, "txtRegConfirm", "Confirm Password");

                // STRICT VALIDATION
                if (string.IsNullOrWhiteSpace(nameInput) || nameInput == "Full Name" ||
                    string.IsNullOrWhiteSpace(emailInput) || emailInput == "Email" ||
                    string.IsNullOrWhiteSpace(passInput) || passInput == "Password")
                {
                    MessageBox.Show("Please fill in all fields correctly.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Pipigilan na pumunta sa database
                }

                if (passInput != confirmInput)
                {
                    MessageBox.Show("Passwords do not match!", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Pipigilan na pumunta sa database
                }

                User newUser = new User(nameInput, emailInput, passInput);
                newUser.Save(null);

                MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                pnlSignup.Visible = false;
                pnlLogin.Visible = true;
            };

            pnlSignup.Controls.Add(btnSignup);

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
        // HELPER METHOD: ADD MODERN INPUT
        // ==========================================
        private void AddModernInput(Panel parent, string controlName, string placeholder, int yPosition, bool isPassword)
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
                Name = controlName,
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

        // ==========================================
        // HELPER METHOD: KUNIN ANG VALUE NG TEXTBOX
        // ==========================================
        private string GetInputValue(Panel parentPanel, string controlName, string placeholder)
        {
            var controls = parentPanel.Controls.Find(controlName, true);
            if (controls.Length > 0 && controls[0] is TextBox txt)
            {
                if (txt.Text == placeholder && txt.ForeColor == textColorLight) return "";
                return txt.Text;
            }
            return "";
        }
    }
}