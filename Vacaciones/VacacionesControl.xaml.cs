using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;
using TFG;

namespace Vacaciones
{
    public partial class VacacionesControl : UserControl
    {
        public ObservableCollection<Vacacion> Vacaciones { get; set; }

        public VacacionesControl()
        {
            InitializeComponent();
            Vacaciones = new ObservableCollection<Vacacion>();
            DataContext = this;
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar.SelectedDates.Count == 2)
            {
                DateTime fechaInicio = calendar.SelectedDates[0];
                DateTime fechaFin = calendar.SelectedDates[1];
                int diasTotales = (fechaFin - fechaInicio).Days + 1;

                
            }
            else if (calendar.SelectedDates.Count > 2)
            {
                // Limpiar selecciones si se seleccionan más de dos fechas
                calendar.SelectedDates.Clear();
            }
        }

        private void AñadirVacaciones_Click(object sender, RoutedEventArgs e)
        {
            if (Vacaciones.Count > 0)
            {
                var vacacion = Vacaciones[Vacaciones.Count - 1]; // Obtener la última vacación añadida

                // Guardar en la base de datos
                GuardarVacacionEnBD(vacacion);
            }
        }

        private void GuardarVacacionEnBD(Vacacion vacacion)
        {
            try
            {
                using (Conexion conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    string query = "INSERT INTO Vacaciones (TrabajadorId, FechaInicio, FechaFin, DiasTotales) VALUES (@TrabajadorId, @FechaInicio, @FechaFin, @DiasTotales)";

                    using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@TrabajadorId", 1); // Aquí deberías obtener el ID del trabajador
                        command.Parameters.AddWithValue("@FechaInicio", vacacion.FechaInicio);
                        command.Parameters.AddWithValue("@FechaFin", vacacion.FechaFin);
                        command.Parameters.AddWithValue("@DiasTotales", vacacion.DiasTotales);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Vacaciones añadidas correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar las vacaciones: " + ex.Message);
            }
        }
    }

    public class Vacacion
    {
        private DateTime _fechaInicio;
        private DateTime _fechaFin;

        public string Nombre { get; set; }
        public string DNI { get; set; }

        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set
            {
                _fechaInicio = value;
                CalcularDiasTotales();
            }
        }

        public DateTime FechaFin
        {
            get => _fechaFin;
            set
            {
                _fechaFin = value;
                CalcularDiasTotales();
            }
        }

        public int DiasTotales { get; private set; }

        private void CalcularDiasTotales()
        {
            if (_fechaInicio != default && _fechaFin != default)
            {
                DiasTotales = (_fechaFin - _fechaInicio).Days + 1;
            }
        }
    }
}
