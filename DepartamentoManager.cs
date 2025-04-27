using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG
{
    public class DepartamentoManager
    {
        public string ObtenerDepartamento(string usuario, string contrasena)
        {
            string departamento = null;
            string connectionString = "Server=localhost;Database=optistock;Uid=root;Pwd=root;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT Departamento FROM Trabajadores WHERE Usuario = @usuario AND Contraseña = @contrasena";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@usuario", usuario);
                    command.Parameters.AddWithValue("@contrasena", contrasena);

                    try
                    {
                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                departamento = reader["Departamento"].ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error al obtener el departamento: {ex.Message}", ex);
                    }
                }
            }

            return departamento;
        }

    }
}
