using INDOTEL_CAJA_REAL_.Clases;
using System;
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
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MostrarSoloLectura();
                return;
            }

            AbrirFormulario(new NUEVA_RECLAMACIÓN());
        }

        private void label3_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new AdministrarReclamaciones());
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MostrarSoloLectura();
                return;
            }

            AbrirFormulario(new REGISTRAR_CIUDADANO());
        }

        private void label8_Click(object sender, EventArgs e)
        {
        }

        private async void Panel_Principal_Load(object sender, EventArgs e)
        {
            if (!Sesion.Logueado || !PermisosCaja.PuedeIngresar(Sesion.Usuario?.Rol))
            {
                MessageBox.Show(
                    "La sesion no es valida para este modulo.",
                    "INDOTEL Caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Close();
                return;
            }

            lblNombre.Text = $"Bienvenido {Sesion.Usuario.NombreCompleto}";
            lblNombre2.Text = $"{Sesion.Usuario.NombreCompleto} ({Sesion.Usuario.Rol})";
            AplicarPermisos();
            await CargarDashboard();
        }

        private void AplicarPermisos()
        {
            var puedeGestionar = PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol);
            label1.Enabled = puedeGestionar;
            label4.Enabled = puedeGestionar;
            label1.Cursor = puedeGestionar ? Cursors.Hand : Cursors.No;
            label4.Cursor = puedeGestionar ? Cursors.Hand : Cursors.No;
        }

        private async Task CargarDashboard()
        {
            btnActualizar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                await CargarResumen();
                await CargarReclamaciones();
            }
            finally
            {
                Cursor = Cursors.Default;
                btnActualizar.Enabled = true;
            }
        }

        private async Task CargarResumen()
        {
            var servicio = new ServicioReportes();
            var respuesta = await servicio.ObtenerResumen();

            if (!respuesta.Exitoso || respuesta.Datos == null)
            {
                MessageBox.Show(
                    respuesta.MensajeConReferencia,
                    "No fue posible cargar el resumen",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            lblCiudadanos.Text = respuesta.Datos.ciudadanos.ToString();
            lblPrestadoras.Text = respuesta.Datos.prestadoras.ToString();
            lblServicios.Text = respuesta.Datos.servicios.ToString();
            lblReclamaciones.Text = respuesta.Datos.reclamaciones.ToString();
            lblabiertas.Text = respuesta.Datos.abiertas.ToString();
            lblCerradas.Text = respuesta.Datos.cerradas.ToString();
            lblVencidas.Text = respuesta.Datos.vencidas.ToString();
            lblRespondidas.Text = respuesta.Datos.respondidasPrestadora.ToString();
            lblResueltas.Text = respuesta.Datos.resueltas.ToString();
            lblResoluciones.Text = respuesta.Datos.resolucionesInstitucionales.ToString();
        }

        private async Task CargarReclamaciones()
        {
            var servicio = new ServicioReclamaciones();
            var respuesta = await servicio.ObtenerTodas();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(
                    respuesta.MensajeConReferencia,
                    "No fue posible cargar las reclamaciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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
            AbrirFormulario(new Buscar_ciudadano());
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            using (var cerrarSesion = new Cerrar_Sesión())
            {
                cerrarSesion.ShowDialog(this);
            }

            if (!Sesion.Logueado)
            {
                Close();
            }
        }

        private void dgResumen_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void AbrirFormulario(Form formulario)
        {
            using (formulario)
            {
                formulario.ShowDialog(this);
            }
        }

        private static void MostrarSoloLectura()
        {
            MessageBox.Show(
                "El perfil Auditor puede consultar informacion, pero no modificarla.",
                "Modo de solo lectura",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
