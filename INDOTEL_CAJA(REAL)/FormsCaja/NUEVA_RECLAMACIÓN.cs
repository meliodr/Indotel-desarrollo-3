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
    public partial class NUEVA_RECLAMACIÓN : Form
    {

        private int ciudadanoId;
        private int prestadoraId;
        private int servicioId;
        private int? tipoReclamacionId;
        private int? motivoReclamacionId;

        public NUEVA_RECLAMACIÓN()
        {
            InitializeComponent();
        }


        private void label9_Click(object sender, EventArgs e)
        {

        }

        private async void NUEVA_RECLAMACIÓN_Load(object sender, EventArgs e)
        {
            txtCaja.Text = "CAJA";

            txtCaja.ReadOnly = true;

            txtCaja.BackColor = Color.WhiteSmoke;

            txtNombre.ReadOnly = true;
            txttelefono.ReadOnly = true;
            txtCorreo.ReadOnly = true;
            txtDireccion.ReadOnly = true;

            await CargarPrestadoras();

            await CargarServicios();

            await CargarTipos();

            await CargarPrioridades();

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cancelar el registro de la reclamación?","Confirmación",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Panel_Principal principal = new Panel_Principal();

                principal.Show();

                this.Hide();
            }

        }

        private async Task CargarPrestadoras()
        {
            ServicioPrestadoras servicio =
                new ServicioPrestadoras();

            var respuesta =
                await servicio.ObtenerTodas();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            cmbPrestadora.DataSource = respuesta.Datos;

            cmbPrestadora.DisplayMember = "NombreComercial";

            cmbPrestadora.ValueMember = "Id";

            cmbPrestadora.SelectedIndex = -1;
        }

        private async Task CargarServicios()
        {
            ServicioTelecom servicios = new ServicioTelecom();

            var respuesta =
                await servicios.ObtenerTodos();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            cmbServicios.DataSource = respuesta.Datos;

            cmbServicios.DisplayMember = "Nombre";

            cmbServicios.ValueMember = "Id";

            cmbServicios.SelectedIndex = -1;
        }

        private async Task CargarTipos()
        {
            ServicioTiposReclamacion servicio =
                new ServicioTiposReclamacion();

            var respuesta =
                await servicio.ObtenerTodos();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            cmbTipoReclamacion.DataSource = respuesta.Datos;

            cmbTipoReclamacion.DisplayMember = "Nombre";

            cmbTipoReclamacion.ValueMember = "Id";

            cmbTipoReclamacion.SelectedIndex = -1;
        }

        private Task CargarPrioridades()
        {
            cmbPrioridad.Items.Clear();

            cmbPrioridad.Items.Add("BAJA");
            cmbPrioridad.Items.Add("MEDIA");
            cmbPrioridad.Items.Add("ALTA");
            cmbPrioridad.Items.Add("URGENTE");

            cmbPrioridad.SelectedItem = "MEDIA";

            return Task.CompletedTask;
        }

        private async Task CargarMotivos(int tipoId)
        {
            ServicioMotivos servicio =
                new ServicioMotivos();

            var respuesta =
                await servicio.ObtenerPorTipo(tipoId);

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            cmbMotivo.DataSource = respuesta.Datos;

            cmbMotivo.DisplayMember = "Nombre";

            cmbMotivo.ValueMember = "Id";

            cmbMotivo.SelectedIndex = -1;
        }

        private async void cmbTipoReclamacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTipoReclamacion.SelectedValue == null)
                return;

            if (!(cmbTipoReclamacion.SelectedValue is int))
                return;

            int tipoId = Convert.ToInt32(cmbTipoReclamacion.SelectedValue);

            await CargarMotivos(tipoId);
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (ciudadanoId == 0)
            {
                MessageBox.Show("Debe seleccionar un ciudadano.");
                return;
            }

            if (cmbPrestadora.SelectedValue == null)
            {
                MessageBox.Show("Seleccione una prestadora.");
                return;
            }

            if (cmbServicios.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un servicio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("Debe ingresar un título.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Debe ingresar una descripción.");
                return;
            }

            ReclamacionCreate request =
                new ReclamacionCreate
                {
                    CiudadanoId = ciudadanoId,
                    PrestadoraId = Convert.ToInt32(cmbPrestadora.SelectedValue),
                    ServicioTelecomId = Convert.ToInt32(cmbServicios.SelectedValue),

                    TipoReclamacionId = cmbTipoReclamacion.SelectedValue == null
                        ? (int?)null
                        : Convert.ToInt32(cmbTipoReclamacion.SelectedValue),

                    MotivoReclamacionId = cmbMotivo.SelectedValue == null
                        ? (int?)null
                        : Convert.ToInt32(cmbMotivo.SelectedValue),

                    CanalRecepcion = "CAJA",

                    Prioridad = cmbPrioridad.Text,

                    Provincia = txtProvincia.Text.Trim(),

                    Municipio = txtMunicipio.Text.Trim(),

                    Titulo = txtTitulo.Text.Trim(),

                    Descripcion = txtDescripcion.Text.Trim()
                };

            ServicioReclamaciones servicio =
                new ServicioReclamaciones();

            var respuesta =
                await servicio.Crear(request);

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            MessageBox.Show(
                "La reclamación fue registrada correctamente.",
                "INDOTEL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;

            Close();
        }

        private async void btnBuscarCedula_Click(object sender, EventArgs e)
        {
             Buscar_ciudadano frm = new Buscar_ciudadano();

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            ciudadanoId = frm.CiudadanoSeleccionado.Id;

            txtCedula.Text = frm.CiudadanoSeleccionado.Cedula;

            txtNombre.Text =
                frm.CiudadanoSeleccionado.Nombres + " " +
                frm.CiudadanoSeleccionado.Apellidos;

            txttelefono.Text =
                frm.CiudadanoSeleccionado.Telefono;

            txtCorreo.Text =
                frm.CiudadanoSeleccionado.Correo;

            txtDireccion.Text =
                frm.CiudadanoSeleccionado.Direccion;

        }

        private void cmbServicios_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

       
    }
}
