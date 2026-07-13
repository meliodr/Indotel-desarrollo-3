using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.Clases.Servicios;
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
            await CargarReclamacion();

        }

        private async Task CargarReclamacion()
        {
            ServicioReclamaciones servicio =
                new ServicioReclamaciones();

            var respuesta =
                await servicio.ObtenerPorId(reclamacionId);

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);

                Close();

                return;
            }

            reclamacion = respuesta.Datos;

            MostrarDatosBasicos();

            await CargarDatosRelacionados();
        }

        private void MostrarDatosBasicos()
        {
            lblTituloExpediente.Text = reclamacion.NumeroExpediente;

            lblTituloEstado.Text = reclamacion.Estado;

            lblTituloFecha.Text = reclamacion.FechaCreacion.ToShortDateString();

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
            ServicioCiudadanos servicio =
                new ServicioCiudadanos();

            var respuesta =
                await servicio.ObtenerPorId(reclamacion.CiudadanoId);

            if (!respuesta.Exitoso)
                return;

            Ciudadano ciudadano = respuesta.Datos;

            txtCedula.Text = ciudadano.Cedula;

            txtNombre.Text =
                ciudadano.Nombres + " " + ciudadano.Apellidos;

            txtTelefono.Text =
                ciudadano.Telefono;

            txtCorreo.Text =
                ciudadano.Correo;

            txtDireccion.Text =
                ciudadano.Direccion;
        }

        private async Task CargarPrestadora()
        {
            ServicioPrestadoras servicio =
                new ServicioPrestadoras();

            var respuesta =
                await servicio.ObtenerPorId(reclamacion.PrestadoraId);

            if (!respuesta.Exitoso)
                return;

            txtPrestadora.Text =
                respuesta.Datos.NombreComercial;
        }

        private async Task CargarServicio()
        {
            ServicioServicios servicio =
                new ServicioServicios();

            var respuesta =
                await servicio.ObtenerPorId(
                    reclamacion.ServicioTelecomId);

            if (!respuesta.Exitoso)
                return;

            txtServicio.Text =
                respuesta.Datos.Nombre;
        }

        private async Task CargarTipo()
        {
            if (!reclamacion.TipoReclamacionId.HasValue)
                return;

            ServicioCatalogos servicio =
                new ServicioCatalogos();

            var respuesta =
                await servicio.ObtenerTipos();

            if (!respuesta.Exitoso)
                return;

            var tipo = respuesta.Datos.FirstOrDefault(x =>
                x.Id == reclamacion.TipoReclamacionId.Value);

            if (tipo != null)
                txtTipo.Text = tipo.Nombre;
        }

        private async Task CargarMotivo()
        {
            if (!reclamacion.MotivoReclamacionId.HasValue)
                return;

            ServicioCatalogos servicio =
                new ServicioCatalogos();

            var respuesta =
                await servicio.ObtenerMotivos();

            if (!respuesta.Exitoso)
                return;

            var motivo = respuesta.Datos.FirstOrDefault(x =>
                x.Id == reclamacion.MotivoReclamacionId.Value);

            if (motivo != null)
                txtMotivo.Text = motivo.Nombre;
        }

        private void btnRespuestaPrestadora_Click(object sender, EventArgs e)
        {

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }

        private void btnCambiarEstado_Click(object sender, EventArgs e)
        {
            Cambiar_Estado frm = new Cambiar_Estado(reclamacion.Id,reclamacion.NumeroExpediente,reclamacion.Estado);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                CargarReclamacion();
            }
        }

        private void btnResolver_Click(object sender, EventArgs e)
        {
            Resolver_Reclamacion frm =new Resolver_Reclamacion(reclamacion.Id,reclamacion.NumeroExpediente);


            if (frm.ShowDialog() == DialogResult.OK)
            {
                CargarReclamacion();
            }

            this.Hide();
        }
    }
}
