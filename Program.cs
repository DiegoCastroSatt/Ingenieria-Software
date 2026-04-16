using MySql.Data.MySqlClient;
using System;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "server=localhost;database=Gym_Software;user=gymuser;password=4321;";

        Console.Write("Nombre: ");
        string user = Console.ReadLine();

        Console.Write("Password: ");
        string pass = Console.ReadLine();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            string queryLogin = "SELECT * FROM usuarios WHERE nombre=@user AND contrasena=@pass";

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

            Console.WriteLine("Usuario no existe. Creando cuenta...");

            Console.Write("Ingrese RUT: ");
            string rut = Console.ReadLine();

            Console.Write("Ingrese Correo: ");
            string correo = Console.ReadLine();

            string queryInsert = "INSERT INTO usuarios (nombre, rut, correo, contrasena) VALUES (@user, @rut, @correo, @pass)";

            using (MySqlCommand cmd = new MySqlCommand(queryInsert, conn))
            {
                cmd.Parameters.AddWithValue("@user", user);
                cmd.Parameters.AddWithValue("@rut", rut);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@pass", pass);

                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Cuenta creada correctamente");
        }
    }
}