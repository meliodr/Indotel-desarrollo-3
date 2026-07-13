using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.Clases.Servicios;
using System;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class Buscar_ciudadano : Form
    {
        private Ciudadano ciudadanoSeleccionado;

        public Buscar_ciudadano()
        {
            InitializeComponent();
            btnNuevo.Enabled = PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol);
        }

        private void Limpiar()
        {
            ciudadanoSeleccionado = null;
            txtNombre.Clear();
            txtApellido.Clear();
            txtTelefono.Clear();
            txtCorreo.Clear();
            txtDireccion.Clear();
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            if (!ValidacionesCaja.CedulaValida(txtCedula.Text))
            {
                MessageBox.Show("La cedula debe contener exactamente 11 digitos.");
                txtCedula.Focus();
                return;
            }

            btnBuscar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var servicio = new ServicioCiudadanos();
                var respuesta = await servicio.BuscarPorCedula(
                    ValidacionesCaja.NormalizarCedula(txtCedula.Text));

                if (!respuesta.Exitoso || respuesta.Datos == null)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "Ciudadano no encontrado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Limpiar();
                    return;
                }

                ciudadanoSeleccionado = respuesta.Datos;
                txtNombre.Text = ciudadanoSeleccionado.Nombres;
                txtApellido.Text = ciudadanoSeleccionado.Apellidos;
                txtTelefono.Text = ciudadanoSeleccionado.Telefono;
                txtCorreo.Text = ciudadanoSeleccionado.Correo;
                txtDireccion.Text = ciudadanoSeleccionado.Direccion;
            }
            finally
            {
                Cursor = Cursors.Default;
                btnBuscar.Enabled = true;
            }
        }

        public Ciudadano CiudadanoSeleccionado => ciudadanoSeleccionado;

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            if (ciudadanoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un ciudadano.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MessageBox.Show("Su perfil solo tiene permiso de consulta.");
                return;
            }

            using (var registro = new REGISTRAR_CIUDADANO())
            {
                registro.ShowDialog(this);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
