using System;

namespace DEMO
{
    public class MentalActivity : BaseActivity
    {
        public string JournalNotes { get; set; }

        public MentalActivity(string name, string emotionTag, string notes)
            : base(name, "Mental", emotionTag)
        {
            JournalNotes = notes;
            // BINAGO: Tinanggal ang "Notes: " na nakasulat para malinis din sa Database!
            Details = JournalNotes;
        }
    }
}