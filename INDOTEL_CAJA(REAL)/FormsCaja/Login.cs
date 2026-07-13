using INDOTEL_CAJA_REAL_.Clases;
using INDOTEL_CAJA_REAL_.FormsCaja;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            /*Panel_Principal dashboard = new Panel_Principal();

            dashboard.Show();*/
        }

        private async void btnInicioSesion_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("Ingrese el correo.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtContraseña.Text))
            {
                MessageBox.Show("Ingrese la contraseña.");
                return;
            }

            btnInicioSesion.Enabled = false;

            try
            {
                ServicioAuth servicio = new ServicioAuth();

                LoginRequest request = new LoginRequest
                {
                    Correo = txtCorreo.Text,
                    Password = txtContraseña.Text
                };

                var respuesta = await servicio.Login(request);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(respuesta.Mensaje);
                    return;
                }

                Sesion.Token = respuesta.Datos.Token;

                Sesion.RefreshToken = respuesta.Datos.RefreshToken;

                Sesion.Usuario = respuesta.Datos.Usuario;

               Panel_Principal dashboard = new Panel_Principal();

                dashboard.Show();

                this.Hide();
            }
            finally
            {
                btnInicioSesion.Enabled = true;
            }


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

      

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
