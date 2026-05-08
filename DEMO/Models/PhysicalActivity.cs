using System;

namespace DEMO
{
    public class PhysicalActivity : BaseActivity
    {
        public int DurationMinutes { get; set; }

        public PhysicalActivity(string name, string emotionTag, int durationMinutes, string extraNotes = "")
            : base(name, "Physical", emotionTag)
        {
            DurationMinutes = durationMinutes;
            // BINAGO: Tinanggal na ang "Duration: X mins. Notes:" text para malinis ang loob ng Database!
            Details = extraNotes;
        }
    }
}