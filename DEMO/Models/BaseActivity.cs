using System;
using MySql.Data.MySqlClient;

namespace DEMO
{
    public abstract class BaseActivity : IDatabaseAccess
    {
        public string ActivityName { get; set; }
        public string Category { get; set; }
        public string EmotionTag { get; set; }
        public string Details { get; set; }

        // BAGONG FIELDS PARA SA ORAS
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public BaseActivity(string name, string category, string emotionTag)
        {
            ActivityName = name;
            Category = category;
            EmotionTag = emotionTag;
        }

        public virtual void Save(object data)
        {
            MariaDbConnector db = new MariaDbConnector();
            db.Connect();

            // BINAGO: Isinama na ang started_at at ended_at sa pag-save!
            string sql = "INSERT INTO activities (name, category, emotionTag, details, started_at, ended_at) VALUES (@name, @category, @emotion, @details, @start, @end)";

            MySqlParameter[] parameters = {
                new MySqlParameter("@name", ActivityName),
                new MySqlParameter("@category", Category),
                new MySqlParameter("@emotion", EmotionTag),
                new MySqlParameter("@details", Details ?? ""),
                new MySqlParameter("@start", StartedAt.HasValue ? (object)StartedAt.Value : DBNull.Value),
                new MySqlParameter("@end", EndedAt.HasValue ? (object)EndedAt.Value : DBNull.Value)
            };

            db.Query(sql, parameters);
            db.Disconnect();
            Console.WriteLine($"[DB] Saved {Category} Activity: {ActivityName}");
        }

        public virtual object Fetch(string id) { return null; }
        public virtual void Delete(string id) { }
    }
}