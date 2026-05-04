using System;
using System.Windows.Forms; // Idinagdag para makita natin ang error sa screen
using MySql.Data.MySqlClient;

public class MariaDbConnector
{
    public string connStr { get; private set; }
    public bool isConnected { get; private set; }
    private MySqlConnection _connection;

    public MariaDbConnector()
    {
        // ==========================================
        // HARDCODED SETTINGS PARA IWAS ERROR
        // Siguraduhing ganito ang default ng XAMPP ninyo
        // ==========================================
        string host = "localhost";
        string port = "3306";
        string user = "root";
        string password = ""; // Blanko dapat ito by default sa XAMPP
        string database = "emotiondb"; // Tama na ang pangalan base sa .txt ng kaklase mo

        connStr = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
        isConnected = false;
        _connection = null;
    }

    public void Connect()
    {
        try
        {
            _connection = new MySqlConnection(connStr);
            _connection.Open();
            isConnected = true;
        }
        catch (MySqlException e)
        {
            isConnected = false;
            // PAPAKITA NA SA SCREEN ANG ERROR IMPES NA NAKATAGO!
            MessageBox.Show($"[DB Connection Failed]\n\n{e.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void Disconnect()
    {
        if (_connection != null)
        {
            _connection.Close();
            _connection = null;
        }
        isConnected = false;
    }

    public object Query(string sql, params MySqlParameter[] parameters)
    {
        if (!isConnected || _connection == null)
        {
            MessageBox.Show("Hindi maka-connect sa database. Siguraduhing naka-start ang MySQL sa XAMPP.", "Database Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        try
        {
            // C# 7.3 Compatible Syntax (Para walang error sa luma ninyong Visual Studio)
            using (MySqlCommand command = new MySqlCommand(sql, _connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    var results = new System.Collections.Generic.List<
                                  System.Collections.Generic.Dictionary<string, object>>();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new System.Collections.Generic.Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            results.Add(row);
                        }
                    }
                    return results;
                }

                return command.ExecuteNonQuery();
            }
        }
        catch (MySqlException e)
        {
            // PAPAKITA ANG ERROR KUNG MAY MALI SA PAG-INSERT O SELECT
            MessageBox.Show($"[SQL Query Error]\n\n{e.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }
}