using INDOTEL_CAJA_REAL_.Clases;
using System;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class Cerrar_Sesión : Form
    {
        public Cerrar_Sesión()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            btnCerrarSesion.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                var servicio = new ServicioAuth();
                var respuesta = await servicio.Logout();

                var mensaje = respuesta.Exitoso
                    ? "Sesion cerrada correctamente."
                    : "La sesion local fue cerrada. No fue posible confirmar la revocacion remota.";

                MessageBox.Show(
                    respuesta.Exitoso ? mensaje : $"{mensaje}\n\n{respuesta.MensajeConReferencia}",
                    "Cerrar sesion",
                    MessageBoxButtons.OK,
                    respuesta.Exitoso ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                Sesion.Limpiar();
                Cursor = Cursors.Default;
                btnCerrarSesion.Enabled = true;
            }
        }

        private void Cerrar_Sesión_Load(object sender, EventArgs e)
        {
            txtUsuario.Text = Sesion.Usuario?.Correo ?? "Sesion no disponible";
            txtRol.Text = Sesion.Usuario?.Rol ?? string.Empty;
        }
    }
}
