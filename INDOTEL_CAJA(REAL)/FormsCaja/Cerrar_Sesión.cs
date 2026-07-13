using INDOTEL_CAJA_REAL_.Clases;
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
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }

        private async void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            ServicioAuth servicio = new ServicioAuth();

            var respuesta =
                await servicio.Logout();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            Sesion.Token = "";
            Sesion.RefreshToken = "";
            Sesion.Usuario = null;

            MessageBox.Show("Sesión cerrada correctamente.");

            Hide();

            Login login = new Login();

            login.Show();

            Close();

        }

        private void Cerrar_Sesión_Load(object sender, EventArgs e)
        {
            txtUsuario.Text = Sesion.Usuario.Correo;

            txtRol.Text = Sesion.Usuario.Rol;


        }
    }
}
