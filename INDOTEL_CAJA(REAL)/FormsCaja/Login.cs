using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.FormsCaja;
using System;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void btnInicioSesion_Click(object sender, EventArgs e)
        {
            var correo = txtCorreo.Text?.Trim();
            var password = txtContraseña.Text;

            if (!ValidacionesCaja.CorreoValido(correo))
            {
                MessageBox.Show("Ingrese un correo valido.", "Inicio de sesion");
                txtCorreo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese la contrasena.", "Inicio de sesion");
                txtContraseña.Focus();
                return;
            }

            btnInicioSesion.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                var servicio = new ServicioAuth();
                var respuesta = await servicio.Login(new LoginRequest
                {
                    Correo = correo,
                    Password = password
                });

                if (!respuesta.Exitoso || respuesta.Datos?.Usuario == null)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible iniciar sesion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Sesion.Token = respuesta.Datos.Token;
                Sesion.RefreshToken = respuesta.Datos.RefreshToken;
                Sesion.Usuario = respuesta.Datos.Usuario;

                if (!PermisosCaja.PuedeIngresar(Sesion.Usuario.Rol))
                {
                    await servicio.Logout();
                    MessageBox.Show(
                        "Este perfil no tiene acceso al modulo interno de Caja.",
                        "Acceso denegado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                txtContraseña.Clear();
                Hide();
                using (var dashboard = new Panel_Principal())
                {
                    dashboard.ShowDialog(this);
                }

                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                btnInicioSesion.Enabled = true;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void Login_Load(object sender, EventArgs e)
        {
            Sesion.Limpiar();
            txtCorreo.Focus();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
