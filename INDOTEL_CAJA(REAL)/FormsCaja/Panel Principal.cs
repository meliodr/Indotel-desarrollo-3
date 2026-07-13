using INDOTEL_CAJA_REAL_.Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class Panel_Principal : Form
    {
        public Panel_Principal()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            NUEVA_RECLAMACIÓN reclamacion = new NUEVA_RECLAMACIÓN();

            reclamacion.Show();

            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            AdministrarReclamaciones admireclamaciones = new AdministrarReclamaciones();

            admireclamaciones.Show();

            this.Hide();


        }

        private void label4_Click(object sender, EventArgs e)
        {
            REGISTRAR_CIUDADANO registrar_ciudadano = new REGISTRAR_CIUDADANO();

            registrar_ciudadano.Show();

            this.Hide();

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private async void Panel_Principal_Load(object sender, EventArgs e)
        {
            lblNombre.Text = "Bienvenido" + " " + Sesion.Usuario.NombreCompleto;

            lblNombre2.Text = Sesion.Usuario.NombreCompleto;

            

            await CargarDashboard();
        }

        private async Task CargarDashboard()
        {
            Cursor = Cursors.WaitCursor;

            await CargarResumen();

            await CargarReclamaciones();

            Cursor = Cursors.Default;
        }

        private async Task CargarResumen()
        {
            ServicioReportes servicio =
                new ServicioReportes();

            var respuesta =
                await servicio.ObtenerResumen();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);

                return;
            }

            lblCiudadanos.Text =
                respuesta.Datos.ciudadanos.ToString();

            lblPrestadoras.Text =
                respuesta.Datos.prestadoras.ToString();

            lblServicios.Text =
                respuesta.Datos.servicios.ToString();

            lblReclamaciones.Text =
                respuesta.Datos.reclamaciones.ToString();

            lblabiertas.Text =
                respuesta.Datos.abiertas.ToString();

            lblCerradas.Text =
                respuesta.Datos.cerradas.ToString();

            lblVencidas.Text =
                respuesta.Datos.vencidas.ToString();

            lblRespondidas.Text =
                respuesta.Datos.respondidasPrestadora.ToString();

            lblResueltas.Text =
                respuesta.Datos.resueltas.ToString();

            lblResoluciones.Text =
                respuesta.Datos.resolucionesInstitucionales.ToString();
        }

        private async Task CargarReclamaciones()
        {
            ServicioReclamaciones servicio =
                new ServicioReclamaciones();

            var respuesta =
                await servicio.ObtenerTodas();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);

                return;
            }

            dgResumen.AutoGenerateColumns = true;

            dgResumen.DataSource = respuesta.Datos;
        }

        private void panelResumen_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label8_Click_1(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private async void btnActualizar_Click(object sender, EventArgs e)
        {
            await CargarDashboard();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Buscar_ciudadano buscar_Ciudadano = new Buscar_ciudadano();

            buscar_Ciudadano.Show();

            this.Hide();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Cerrar_Sesión cerrar_sesion = new Cerrar_Sesión();

            cerrar_sesion.Show();

            this.Hide();
        }

        private void dgResumen_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
