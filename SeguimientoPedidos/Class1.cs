using MySqlConnector;
using System;
using System.ComponentModel;
using System.Windows;

namespace TFG.ViewModels
{
    public class PedidoViewModel : INotifyPropertyChanged
    {
        private int id;
        private int clienteWebId;
        private string numeroPedido;
        private string nombreCliente;
        private string apellidoCliente;
        private string direccionCliente;
        private string ciudadCliente;
        private decimal precioTotal;
        private DateTime fechaPedido;
        private string estado;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public int ClienteWebId
        {
            get { return clienteWebId; }
            set
            {
                clienteWebId = value;
                OnPropertyChanged(nameof(ClienteWebId));
                CargarDatosCliente();
            }
        }

        public string NumeroPedido
        {
            get { return numeroPedido; }
            set
            {
                numeroPedido = value;
                OnPropertyChanged(nameof(NumeroPedido));
            }
        }


        public string NombreCliente
        {
            get { return nombreCliente; }
            set
            {
                nombreCliente = value;
                OnPropertyChanged(nameof(NombreCliente));
            }
        }

        public string ApellidoCliente
        {
            get { return apellidoCliente; }
            set
            {
                apellidoCliente = value;
                OnPropertyChanged(nameof(ApellidoCliente));
            }
        }

        public string DireccionCliente
        {
            get { return direccionCliente; }
            set
            {
                direccionCliente = value;
                OnPropertyChanged(nameof(DireccionCliente));
            }
        }

        public string CiudadCliente
        {
            get { return ciudadCliente; }
            set
            {
                ciudadCliente = value;
                OnPropertyChanged(nameof(CiudadCliente));
            }
        }

        public decimal PrecioTotal
        {
            get { return precioTotal; }
            set
            {
                precioTotal = value;
                OnPropertyChanged(nameof(PrecioTotal));
            }
        }

        public DateTime FechaPedido
        {
            get { return fechaPedido; }
            set
            {
                fechaPedido = value;
                OnPropertyChanged(nameof(FechaPedido));
            }
        }

        public string Estado
        {
            get { return estado; }
            set
            {
                estado = value;
                OnPropertyChanged(nameof(Estado));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CargarDatosCliente()
        {
            if (ClienteWebId <= 0)
            {
                MessageBox.Show("ClienteWebId no puede ser cero o negativo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = "SELECT Nombre, Apellido, Direccion, Ciudad FROM clienteweb WHERE Id = @ClienteWebId";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@ClienteWebId", ClienteWebId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                NombreCliente = reader["Nombre"].ToString();
                                ApellidoCliente = reader["Apellido"].ToString();
                                DireccionCliente = reader["Direccion"].ToString();
                                CiudadCliente = reader["Ciudad"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Cliente no encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los datos del cliente: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
