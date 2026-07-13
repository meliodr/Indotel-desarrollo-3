using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.Clases.Servicios;
using System;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class REGISTRAR_CIUDADANO : Form
    {
        public REGISTRAR_CIUDADANO()
        {
            InitializeComponent();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MessageBox.Show("No tiene permiso para registrar ciudadanos.");
                return;
            }

            if (!ValidacionesCaja.CedulaValida(txtCedula.Text))
            {
                MessageBox.Show("La cedula debe contener exactamente 11 digitos.");
                txtCedula.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Debe ingresar los nombres.");
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Debe ingresar los apellidos.");
                txtApellido.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtCorreo.Text) &&
                !ValidacionesCaja.CorreoValido(txtCorreo.Text))
            {
                MessageBox.Show("Ingrese un correo valido.");
                txtCorreo.Focus();
                return;
            }

            btnGuardar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var ciudadano = new CiudadanoCreate
                {
                    Cedula = ValidacionesCaja.NormalizarCedula(txtCedula.Text),
                    Nombres = txtNombre.Text.Trim(),
                    Apellidos = txtApellido.Text.Trim(),
                    Telefono = txtTelefono.Text?.Trim() ?? string.Empty,
                    Correo = txtCorreo.Text?.Trim() ?? string.Empty,
                    Direccion = txtDireccion.Text?.Trim() ?? string.Empty
                };

                var servicio = new ServicioCiudadanos();
                var respuesta = await servicio.Crear(ciudadano);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible registrar al ciudadano",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    "Ciudadano registrado correctamente.",
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
    }
}
