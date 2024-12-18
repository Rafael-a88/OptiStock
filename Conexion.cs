
using MySqlConnector;
using System;
using System.Data.SqlClient;

namespace TFG
{
    public class Conexion : IDisposable // Implementar IDisposable
    {
        private MySqlConnection connection;

        // Constructor
        public Conexion()
        {
            string connectionString = "Server=localhost;Database=optistock;Uid=root;Pwd=;";
            connection = new MySqlConnection(connectionString);
        }

        // Método para abrir la conexión
        public void AbrirConexion()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                    Console.WriteLine("Conexión abierta exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al abrir la conexión: " + ex.Message);
            }
        }

        // Método para cerrar la conexión
        public void CerrarConexion()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                    Console.WriteLine("Conexión cerrada exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cerrar la conexión: " + ex.Message);
            }
        }

        // Método para obtener la conexión
        public MySqlConnection ObtenerConexion()
        {
            return connection; // Se devuelve la conexión, pero asegúrate de que esté abierta antes de usarla
        }

        // Implementación de IDisposable
        public void Dispose()
        {
            CerrarConexion();
            connection.Dispose();
        }
    }
}