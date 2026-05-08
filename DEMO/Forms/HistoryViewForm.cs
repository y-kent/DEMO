using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace DEMO
{
    public partial class HistoryViewForm : Form
    {
        public HistoryViewForm(string dateStr, string activityName, string primaryEmo, string secondaryEmo, string dummyNotes, Image dummyPhoto, Image emoji)
        {
            Text = "DEMO - Activity History";
            Size = new Size(950, 560);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // OUTER THICK BORDER 
            BackColor = Color.Black;
            Padding = new Padding(6);

            Panel pnlInner = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            Controls.Add(pnlInner);

            // ==========================================
            // KUNIN ANG DATA MULA SA DATABASE
            // ==========================================
            string fetchedNotes = "No reflections or notes written for this day.";
            string fetchedPhotoPath = "";
            string timeStart = "N/A";
            string timeEnd = "N/A";

            try
            {
                DateTime parsedDate = DateTime.Parse(dateStr);
                string dbDate = parsedDate.ToString("yyyy-MM-dd");

                MariaDbConnector db = new MariaDbConnector();
                db.Connect();

                // 1. KUNIN ANG ACTIVITY DETAILS (Isinama ang 'category' para malaman kung may timer ba ito)
                string sqlAct = "SELECT details, started_at, ended_at, created_at, category FROM activities WHERE name = @act AND DATE(created_at) = @date ORDER BY id DESC LIMIT 1";
                MySqlParameter[] paramAct = {
                    new MySqlParameter("@act", activityName),
                    new MySqlParameter("@date", dbDate)
                };

                var resultAct = db.Query(sqlAct, paramAct) as List<Dictionary<string, object>>;
                if (resultAct != null && resultAct.Count > 0)
                {
                    DateTime createdAt = Convert.ToDateTime(resultAct[0]["created_at"]);

                    DateTime startDt = createdAt;
                    DateTime endDt = createdAt;

                    if (resultAct[0]["started_at"] != DBNull.Value)
                        startDt = Convert.ToDateTime(resultAct[0]["started_at"]);

                    if (resultAct[0]["ended_at"] != DBNull.Value)
                        endDt = Convert.ToDateTime(resultAct[0]["ended_at"]);

                    timeStart = startDt.ToString("hh:mm:ss tt");
                    timeEnd = endDt.ToString("hh:mm:ss tt");

                    string rawDetails = resultAct[0]["details"]?.ToString() ?? "";
                    string actCategory = resultAct[0].ContainsKey("category") && resultAct[0]["category"] != null ? resultAct[0]["category"].ToString() : "";
                    string durationStr = "";

                    // FORMATTING: Gagawa ng "Duration: HH:MM:SS." format KUNG Physical Activity (Timer) lang!
                    if (actCategory == "Physical" && resultAct[0]["started_at"] != DBNull.Value && resultAct[0]["ended_at"] != DBNull.Value)
                    {
                        TimeSpan duration = endDt - startDt;
                        durationStr = $"Duration: {(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}.\r\n\r\n";
                    }

                    // PANG-MALINIS NA CODE
                    if (rawDetails.Contains("Duration: "))
                    {
                        try
                        {
                            int durStart = rawDetails.IndexOf("Duration: ");
                            int durEnd = rawDetails.IndexOf(" mins", durStart);
                            if (durEnd > durStart)
                            {
                                rawDetails = rawDetails.Substring(durEnd + 5).Trim();
                            }
                        }
                        catch { }
                    }

                    if (rawDetails.Contains("[Photo Attached: "))
                    {
                        int pStart = rawDetails.IndexOf("[Photo Attached: ") + 17;
                        int pEnd = rawDetails.IndexOf("]", pStart);
                        if (pEnd > pStart)
                        {
                            fetchedPhotoPath = rawDetails.Substring(pStart, pEnd - pStart).Trim();
                        }
                        fetchedNotes = rawDetails.Substring(0, rawDetails.IndexOf("[Photo Attached: ")).Trim();
                    }
                    else
                    {
                        fetchedNotes = rawDetails;
                    }

                    if (fetchedNotes.Contains("[Playlist:")) fetchedNotes = fetchedNotes.Substring(0, fetchedNotes.IndexOf("[Playlist:")).Trim();
                    if (fetchedNotes.Contains("Notes: ")) fetchedNotes = fetchedNotes.Replace("Notes: ", "").Trim();
                    if (fetchedNotes.StartsWith(".")) fetchedNotes = fetchedNotes.Substring(1).Trim();
                    if (string.IsNullOrWhiteSpace(fetchedNotes)) fetchedNotes = "No reflections or notes written for this day.";

                    // Idadagdag lang ang Duration sa taas ng Notes kapag mayroon (kapag Physical/Timer Activity)
                    if (!string.IsNullOrWhiteSpace(durationStr))
                    {
                        fetchedNotes = durationStr + fetchedNotes;
                    }
                }

                db.Disconnect();
            }
            catch { }


            // ==========================================
            // LEFT SIDE: TEXT DETAILS 
            // ==========================================
            Label lblDate = new Label() { Text = dateStr, Font = new Font("Segoe UI", 24, FontStyle.Regular), Location = new Point(30, 25), AutoSize = true, ForeColor = Color.Black };
            pnlInner.Controls.Add(lblDate);

            Label lblActTitle = new Label() { Text = "Activity that you did:", Font = new Font("Segoe UI", 11, FontStyle.Regular), Location = new Point(35, 80), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 80) };
            pnlInner.Controls.Add(lblActTitle);

            Label lblActivity = new Label() { Text = activityName.ToUpper(), Font = new Font("Segoe UI", 24, FontStyle.Regular), Location = new Point(30, 105), AutoSize = true, ForeColor = Color.Black };
            pnlInner.Controls.Add(lblActivity);

            Label lblTimeStartTitle = new Label() { Text = "Time Started:", Font = new Font("Segoe UI", 11, FontStyle.Regular), Location = new Point(35, 170), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 80) };
            Label lblTimeStart = new Label() { Text = timeStart, Font = new Font("Segoe UI", 18, FontStyle.Regular), Location = new Point(32, 190), AutoSize = true, ForeColor = Color.Black };
            pnlInner.Controls.Add(lblTimeStartTitle);
            pnlInner.Controls.Add(lblTimeStart);

            Label lblTimeEndTitle = new Label() { Text = "Time Ended:", Font = new Font("Segoe UI", 11, FontStyle.Regular), Location = new Point(250, 170), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 80) };
            Label lblTimeEnd = new Label() { Text = timeEnd, Font = new Font("Segoe UI", 18, FontStyle.Regular), Location = new Point(247, 190), AutoSize = true, ForeColor = Color.Black };
            pnlInner.Controls.Add(lblTimeEndTitle);
            pnlInner.Controls.Add(lblTimeEnd);

            // NOTES / REFLECTION BOX
            Panel pnlNotesBorder = new Panel() { Location = new Point(30, 250), Size = new Size(500, 230), BackColor = Color.Black, Padding = new Padding(5) };
            Panel pnlNotesInner = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };

            TextBox txtNotes = new TextBox()
            {
                Text = fetchedNotes,
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.White
            };
            pnlNotesInner.Controls.Add(txtNotes);
            pnlNotesBorder.Controls.Add(pnlNotesInner);
            pnlInner.Controls.Add(pnlNotesBorder);

            // ==========================================
            // MIDDLE: MOOD SECTION
            // ==========================================
            int moodX = 440;
            Label lblMoodTitle = new Label() { Text = "MOOD", Font = new Font("Segoe UI", 18, FontStyle.Regular), Location = new Point(moodX, 40), AutoSize = true, ForeColor = Color.Black };
            pnlInner.Controls.Add(lblMoodTitle);

            PictureBox pbEmoji = new PictureBox() { Image = emoji, SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(90, 90), Location = new Point(moodX - 2, 80), BackColor = Color.Transparent };
            pnlInner.Controls.Add(pbEmoji);

            Label lblPrimary = new Label() { Text = primaryEmo.ToUpper(), Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(moodX - 15, 175), Size = new Size(120, 20), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Black };
            pnlInner.Controls.Add(lblPrimary);

            Label lblSecondary = new Label() { Text = string.IsNullOrWhiteSpace(secondaryEmo) ? "" : secondaryEmo, Font = new Font("Segoe UI", 10, FontStyle.Regular), Location = new Point(moodX - 15, 195), Size = new Size(120, 20), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.FromArgb(80, 80, 80) };
            pnlInner.Controls.Add(lblSecondary);

            // ==========================================
            // RIGHT SIDE: PHOTO BOX 
            // ==========================================
            Panel pnlPhotoBorder = new Panel() { Location = new Point(560, 30), Size = new Size(340, 450), BackColor = Color.Black, Padding = new Padding(5) };
            PictureBox pbUploadedPhoto = new PictureBox()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 245),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            if (!string.IsNullOrEmpty(fetchedPhotoPath) && System.IO.File.Exists(fetchedPhotoPath))
            {
                pbUploadedPhoto.Image = Image.FromFile(fetchedPhotoPath);
            }
            else
            {
                Label lblNoPhoto = new Label()
                {
                    Text = "PHOTO HERE",
                    Font = new Font("Segoe UI", 24, FontStyle.Regular),
                    ForeColor = Color.Silver,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pbUploadedPhoto.Controls.Add(lblNoPhoto);
            }

            pnlPhotoBorder.Controls.Add(pbUploadedPhoto);
            pnlInner.Controls.Add(pnlPhotoBorder);
        }
    }
}