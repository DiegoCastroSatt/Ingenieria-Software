using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "server=localhost;database=Gym_Software;user=gymuser;password=1234;";

        Console.Write("Usuario: ");
        string user = Console.ReadLine();

        Console.Write("Password: ");
        string pass = Console.ReadLine();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string queryLogin = "SELECT * FROM usuarios WHERE nombre=@user AND password=@pass";

            using (MySqlCommand cmd = new MySqlCommand(queryLogin, conn))
            {
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@pass", pass);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("Login correcto");
                        return;
                    }
                }
            }

            Console.WriteLine("❌ Usuario no existe. Creando cuenta...");

            string queryInsert = "INSERT INTO usuarios (nombre, password) VALUES (@user, @pass)";

            using (MySqlCommand cmd = new MySqlCommand(queryInsert, conn))
            {
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@pass", pass);

                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Cuenta creada y ");
        }
    }
}