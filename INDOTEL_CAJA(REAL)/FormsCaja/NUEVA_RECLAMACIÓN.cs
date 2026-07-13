using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.Clases.Servicios;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class NUEVA_RECLAMACIÓN : Form
    {
        private int ciudadanoId;

        public NUEVA_RECLAMACIÓN()
        {
            InitializeComponent();
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private async void NUEVA_RECLAMACIÓN_Load(object sender, EventArgs e)
        {
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MessageBox.Show("No tiene permiso para registrar reclamaciones.");
                Close();
                return;
            }

            txtCaja.Text = "CAJA";
            txtCaja.ReadOnly = true;
            txtCaja.BackColor = Color.WhiteSmoke;
            txtNombre.ReadOnly = true;
            txttelefono.ReadOnly = true;
            txtCorreo.ReadOnly = true;
            txtDireccion.ReadOnly = true;

            Cursor = Cursors.WaitCursor;
            try
            {
                await CargarPrestadoras();
                await CargarServicios();
                await CargarTipos();
                await CargarPrioridades();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "Desea cancelar el registro de la reclamacion?",
                    "Confirmacion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private async Task CargarPrestadoras()
        {
            var servicio = new ServicioPrestadoras();
            var respuesta = await servicio.ObtenerTodas();
            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.MensajeConReferencia);
                return;
            }

            cmbPrestadora.DataSource = respuesta.Datos;
            cmbPrestadora.DisplayMember = "NombreComercial";
            cmbPrestadora.ValueMember = "Id";
            cmbPrestadora.SelectedIndex = -1;
        }

        private async Task CargarServicios()
        {
            var servicio = new ServicioTelecom();
            var respuesta = await servicio.ObtenerTodos();
            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.MensajeConReferencia);
                return;
            }

            cmbServicios.DataSource = respuesta.Datos;
            cmbServicios.DisplayMember = "Nombre";
            cmbServicios.ValueMember = "Id";
            cmbServicios.SelectedIndex = -1;
        }

        private async Task CargarTipos()
        {
            var servicio = new ServicioTiposReclamacion();
            var respuesta = await servicio.ObtenerTodos();
            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.MensajeConReferencia);
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
            var servicio = new ServicioMotivos();
            var respuesta = await servicio.ObtenerPorTipo(tipoId);
            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.MensajeConReferencia);
                return;
            }

            cmbMotivo.DataSource = respuesta.Datos;
            cmbMotivo.DisplayMember = "Nombre";
            cmbMotivo.ValueMember = "Id";
            cmbMotivo.SelectedIndex = -1;
        }

        private async void cmbTipoReclamacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTipoReclamacion.SelectedValue is int tipoId)
            {
                await CargarMotivos(tipoId);
            }
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (ciudadanoId <= 0)
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

            if (string.IsNullOrWhiteSpace(txtProvincia.Text))
            {
                MessageBox.Show("Debe indicar la provincia.");
                txtProvincia.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMunicipio.Text))
            {
                MessageBox.Show("Debe indicar el municipio.");
                txtMunicipio.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MessageBox.Show("Debe ingresar un titulo.");
                txtTitulo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Debe ingresar una descripcion.");
                txtDescripcion.Focus();
                return;
            }

            btnGuardar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var request = new ReclamacionCreate
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

                var servicio = new ServicioReclamaciones();
                var respuesta = await servicio.Crear(request);
                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible registrar la reclamacion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    $"La reclamacion fue registrada correctamente.\nExpediente: {respuesta.Datos?.NumeroExpediente}",
                    "INDOTEL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                Cursor = Cursors.Default;
                btnGuardar.Enabled = true;
            }
        }

        private void btnBuscarCedula_Click(object sender, EventArgs e)
        {
            using (var formulario = new Buscar_ciudadano())
            {
                if (formulario.ShowDialog(this) != DialogResult.OK ||
                    formulario.CiudadanoSeleccionado == null)
                {
                    return;
                }

                ciudadanoId = formulario.CiudadanoSeleccionado.Id;
                txtCedula.Text = formulario.CiudadanoSeleccionado.Cedula;
                txtNombre.Text = formulario.CiudadanoSeleccionado.Nombres + " " +
                                 formulario.CiudadanoSeleccionado.Apellidos;
                txttelefono.Text = formulario.CiudadanoSeleccionado.Telefono;
                txtCorreo.Text = formulario.CiudadanoSeleccionado.Correo;
                txtDireccion.Text = formulario.CiudadanoSeleccionado.Direccion;
            }
        }

        private void cmbServicios_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
