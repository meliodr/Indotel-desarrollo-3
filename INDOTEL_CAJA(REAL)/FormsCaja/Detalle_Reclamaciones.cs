using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.Clases.Servicios;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class Detalle_Reclamaciones : Form
    {
        private readonly int reclamacionId;
        private Reclamacion reclamacion;

        public Detalle_Reclamaciones(int id)
        {
            InitializeComponent();
            reclamacionId = id;
        }

        private async void Detalle_Reclamaciones_Load(object sender, EventArgs e)
        {
            AplicarPermisos();
            await CargarReclamacion();
        }

        private void AplicarPermisos()
        {
            var puedeGestionar = PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol);
            btnCambiarEstado.Enabled = puedeGestionar;
            btnResolver.Enabled = puedeGestionar;

            // La respuesta de la prestadora se gestiona fuera de Caja.
            // El botón fue retirado del formulario para evitar exponer una acción no autorizada.
        }

        private async Task CargarReclamacion()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var servicio = new ServicioReclamaciones();
                var respuesta = await servicio.ObtenerPorId(reclamacionId);

                if (!respuesta.Exitoso || respuesta.Datos == null)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible cargar la reclamacion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                reclamacion = respuesta.Datos;
                MostrarDatosBasicos();
                await CargarDatosRelacionados();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MostrarDatosBasicos()
        {
            lblTituloExpediente.Text = reclamacion.NumeroExpediente;
            lblTituloEstado.Text = reclamacion.Estado;
            lblTituloFecha.Text = reclamacion.FechaCreacion.ToLocalTime().ToShortDateString();
            txtPrioridad.Text = reclamacion.Prioridad;
            txtCanal.Text = reclamacion.CanalRecepcion;
            txtProvincia.Text = reclamacion.Provincia;
            txtMunicipio.Text = reclamacion.Municipio;
            Txttitulo.Text = reclamacion.Titulo;
            TxtDescripcion.Text = reclamacion.Descripcion;
        }

        private async Task CargarDatosRelacionados()
        {
            await CargarCiudadano();
            await CargarPrestadora();
            await CargarServicio();
            await CargarTipo();
            await CargarMotivo();
        }

        private async Task CargarCiudadano()
        {
            var servicio = new ServicioCiudadanos();
            var respuesta = await servicio.ObtenerPorId(reclamacion.CiudadanoId);
            if (!respuesta.Exitoso || respuesta.Datos == null) return;

            var ciudadano = respuesta.Datos;
            txtCedula.Text = ciudadano.Cedula;
            txtNombre.Text = ciudadano.Nombres + " " + ciudadano.Apellidos;
            txtTelefono.Text = ciudadano.Telefono;
            txtCorreo.Text = ciudadano.Correo;
            txtDireccion.Text = ciudadano.Direccion;
        }

        private async Task CargarPrestadora()
        {
            var servicio = new ServicioPrestadoras();
            var respuesta = await servicio.ObtenerPorId(reclamacion.PrestadoraId);
            if (respuesta.Exitoso && respuesta.Datos != null)
            {
                txtPrestadora.Text = respuesta.Datos.NombreComercial;
            }
        }

        private async Task CargarServicio()
        {
            var servicio = new ServicioServicios();
            var respuesta = await servicio.ObtenerPorId(reclamacion.ServicioTelecomId);
            if (respuesta.Exitoso && respuesta.Datos != null)
            {
                txtServicio.Text = respuesta.Datos.Nombre;
            }
        }

        private async Task CargarTipo()
        {
            if (!reclamacion.TipoReclamacionId.HasValue) return;
            var servicio = new ServicioCatalogos();
            var respuesta = await servicio.ObtenerTipos();
            if (!respuesta.Exitoso || respuesta.Datos == null) return;

            var tipo = respuesta.Datos.FirstOrDefault(x => x.Id == reclamacion.TipoReclamacionId.Value);
            if (tipo != null) txtTipo.Text = tipo.Nombre;
        }

        private async Task CargarMotivo()
        {
            if (!reclamacion.MotivoReclamacionId.HasValue) return;
            var servicio = new ServicioCatalogos();
            var respuesta = await servicio.ObtenerMotivos();
            if (!respuesta.Exitoso || respuesta.Datos == null) return;

            var motivo = respuesta.Datos.FirstOrDefault(x => x.Id == reclamacion.MotivoReclamacionId.Value);
            if (motivo != null) txtMotivo.Text = motivo.Nombre;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void btnCambiarEstado_Click(object sender, EventArgs e)
        {
            if (reclamacion == null) return;
            using (var formulario = new Cambiar_Estado(
                       reclamacion.Id,
                       reclamacion.NumeroExpediente,
                       reclamacion.Estado))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    await CargarReclamacion();
                }
            }
        }

        private async void btnResolver_Click(object sender, EventArgs e)
        {
            if (reclamacion == null) return;
            using (var formulario = new Resolver_Reclamacion(
                       reclamacion.Id,
                       reclamacion.NumeroExpediente))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    await CargarReclamacion();
                }
            }
        }
    }
}
