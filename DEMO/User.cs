using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace DEMO
{
    // Ini-implement ang IDatabaseAccess ng kaklase mo
    public class User : IDatabaseAccess
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // Constructor para sa Sign-Up
        public User(string fullName, string email, string password)
        {
            UserId = Guid.NewGuid().ToString(); // Gagawa ng random ID
            FullName = fullName;
            Email = email;
            Password = password;
        }

        // Empty constructor para sa Login
        public User() { }

        // ==========================================
        // IDatabaseAccess Implementation
        // ==========================================
        public void Save(object data)
        {
            MariaDbConnector db = new MariaDbConnector();
            db.Connect();

            // BINAGO: Isinama na ang fullname dahil maayos na ang ating SQL script!
            string sql = "INSERT INTO users (userId, fullname, email, password) VALUES (@id, @fname, @email, @pass)";

            MySqlParameter[] parameters = {
                new MySqlParameter("@id", UserId),
                new MySqlParameter("@fname", FullName),
                new MySqlParameter("@email", Email),
                new MySqlParameter("@pass", Password) // Sa totoong app, dapat naka-encrypt/hash ito
            };

            db.Query(sql, parameters);
            db.Disconnect();
            Console.WriteLine($"[DB] User {Email} registered successfully.");
        }

        public object Fetch(string email)
        {
            MariaDbConnector db = new MariaDbConnector();
            db.Connect();

            // Hahanapin natin yung user gamit ang kanyang email
            string sql = "SELECT * FROM users WHERE email = @email LIMIT 1";
            MySqlParameter[] parameters = { new MySqlParameter("@email", email) };

            var result = db.Query(sql, parameters) as List<Dictionary<string, object>>;
            db.Disconnect();

            return result;
        }

        public void Delete(string id)
        {
            // Logic kung gustong i-delete ng user ang account niya
        }

        // ==========================================
        // Custom Login Method
        // ==========================================
        public bool Login(string email, string password)
        {
            // Kukunin ang data ng user base sa email
            var result = Fetch(email) as List<Dictionary<string, object>>;

            // Kung may nahanap at hindi empty
            if (result != null && result.Count > 0)
            {
                // I-check natin kung tumugma ang password
                string dbPassword = result[0]["password"].ToString();
                if (dbPassword == password)
                {
                    return true; // Tama ang email at password! Access Granted.
                }
            }

            return false; // Mali ang email o password. Access Denied.
        }
    }
}