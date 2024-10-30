using System.Data;
using Microsoft.Data.Sqlite;

namespace lemmikkiAPI_esimerkki
{
    public record Owner(int id, string name, string number);
    public record Pet(int id, string name, string type, string omistaja_name);
    public class DBManager
    {
        private SqliteConnection dbConnection;

        public DBManager(string dbName)
        {
            dbConnection = new SqliteConnection("Data Source=" + dbName + ".db");
            NewTable("Omistajat", new List<(string, string)>() { ("Id", "INTEGER PRIMARY KEY"), ("Name", "STRING NOT NULL"), ("Number", "STRING") });
            NewTable("Lemmikit", new List<(string, string)> { ("Id", "INTEGER PRIMARY KEY"), ("Name", "STRING NOT NULL"), ("Type", "STRING"), ("Omistaja_Id", "INTEGER") });
        }

        public bool NewTable(string tableName, List<(string, string)> columns)
        {
            if (string.IsNullOrEmpty(tableName)) return false;

            string command = "CREATE TABLE IF NOT EXISTS " + tableName + " (";
            command += string.Join(",", columns.Select(column => $"{column.Item1} {column.Item2}")) + ")";

            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                dbConnection.Open();
                sqlcommand.ExecuteNonQuery();
                dbConnection.Close();
            }

            return true;
        }

        public void PostOmistaja(string name, string number)
        {
            string command = @"INSERT INTO Omistajat (Name, Number) VALUES ($Name, $Number)";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$Name", name);
                sqlcommand.Parameters.AddWithValue("$Number", number);

                dbConnection.Open();
                sqlcommand.ExecuteNonQuery();
                dbConnection.Close();
            }
        }

        public bool PostLemmikki(string name, string type, string omistaja_Name)
        {
            string command = @"INSERT INTO Lemmikit (Name, Type, Omistaja_Id) VALUES ($Name, $Type, $Omistaja_Id)";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$Name", name);
                sqlcommand.Parameters.AddWithValue("$Type", type);

                // Get the owner ID
                int? omistaja_Id = GetOwnerId(omistaja_Name);
                if (omistaja_Id == null)
                {
                    return false; // Owner not found
                }

                sqlcommand.Parameters.AddWithValue("$Omistaja_Id", omistaja_Id);
                dbConnection.Open();
                sqlcommand.ExecuteNonQuery();
                dbConnection.Close();
                return true; // Successfully added pet
            }
        }

        private int? GetOwnerId(string ownerName)
        {
            string command = "SELECT Id FROM Omistajat WHERE Name = $Name";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$Name", ownerName);
                dbConnection.Open();
                object result = sqlcommand.ExecuteScalar();
                dbConnection.Close();
                return result != null ? (int?)Convert.ToInt32(result) : null;
            }
        }

        public void UpdateOmistajaNumber(int ownerId, string newNumber)
        {
            if (!OwnerExists(ownerId))
            {
                throw new Exception("ID not found.");
            }

            string command = @"UPDATE Omistajat SET Number = $newNumber WHERE Id = $ownerId";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$ownerId", ownerId);
                sqlcommand.Parameters.AddWithValue("$newNumber", newNumber);

                dbConnection.Open();
                sqlcommand.ExecuteNonQuery();
                dbConnection.Close();
            }
        }

        public object? GetOmistajaNumberFromLemmikkiName(string lemmikkiName)
        {
            string command = @"SELECT Omistajat.Number FROM Omistajat LEFT JOIN Lemmikit ON Omistajat.Id=Lemmikit.Omistaja_Id WHERE Lemmikit.Name=$lemmikkiName AND lemmikit.Omistaja_Id=Omistajat.Id";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$lemmikkiName", lemmikkiName);
                dbConnection.Open();
                object? result = sqlcommand.ExecuteScalar();
                dbConnection.Close();
                return result;
            }
        }

        public List<Owner> GetAllOwners()
        {
            var owners = new List<Owner>();
            string command = "SELECT * FROM Omistajat";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                dbConnection.Open();
                using (var reader = sqlcommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        owners.Add(new Owner(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                    }
                }
                dbConnection.Close();
            }
            return owners;
        }

        public List<Pet> GetAllPets()
        {
            var pets = new List<Pet>();
            string command = "SELECT * FROM Lemmikit";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                dbConnection.Open();
                using (var reader = sqlcommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pets.Add(new Pet(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                    }
                }
                dbConnection.Close();
            }
            return pets;
        }

        public void DeleteOwner(int ownerId)
        {
            if (!OwnerExists(ownerId))
            {
                throw new Exception("ID not found.");
            }

            string command = @"DELETE FROM Omistajat WHERE Id = $ownerId";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$ownerId", ownerId);
                dbConnection.Open();
                sqlcommand.ExecuteNonQuery();
                dbConnection.Close();
            }
        }

        public void DeletePet(int petId)
        {
            if (!PetExists(petId))
            {
                throw new Exception("ID not found.");
            }

            string command = @"DELETE FROM Lemmikit WHERE Id = $petId";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$petId", petId);
                dbConnection.Open();
                sqlcommand.ExecuteNonQuery();
                dbConnection.Close();
            }
        }

        private bool OwnerExists(int ownerId)
        {
            string command = "SELECT COUNT(*) FROM Omistajat WHERE Id = $ownerId";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$ownerId", ownerId);
                dbConnection.Open();
                int count = Convert.ToInt32(sqlcommand.ExecuteScalar());
                dbConnection.Close();
                return count > 0;
            }
        }

        private bool PetExists(int petId)
        {
            string command = "SELECT COUNT(*) FROM Lemmikit WHERE Id = $petId";
            using (var sqlcommand = dbConnection.CreateCommand())
            {
                sqlcommand.CommandText = command;
                sqlcommand.Parameters.AddWithValue("$petId", petId);
                dbConnection.Open();
                int count = Convert.ToInt32(sqlcommand.ExecuteScalar());
                dbConnection.Close();
                return count > 0;
            }
        }
    }
}