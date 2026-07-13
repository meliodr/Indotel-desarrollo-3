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
    public partial class REGISTRAR_CIUDADANO : Form
    {
        public REGISTRAR_CIUDADANO()
        {
            InitializeComponent();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }

        private bool ValidarCedula()
        {
            string cedula = txtCedula.Text.Replace("-", "").Trim();

            if (cedula.Length != 11)
            {
                MessageBox.Show("La cédula debe contener 11 dígitos.");
                txtCedula.Focus();
                return false;
            }

            return true;
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                MessageBox.Show("Debe ingresar la cédula.");
                txtCedula.Focus();
                return;
            }

            if (!ValidarCedula()) 
            {
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

            btnGuardar.Enabled = false;

            try
            {
                CiudadanoCreate ciudadano = new CiudadanoCreate
                {
                    Cedula = txtCedula.Text.Trim(),
                    Nombres = txtNombre.Text.Trim(),
                    Apellidos = txtApellido.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Correo = txtCorreo.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim()
                };

                ServicioCiudadanos servicio =
                    new ServicioCiudadanos();

                var respuesta =
                    await servicio.Crear(ciudadano);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(respuesta.Mensaje);
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
                btnGuardar.Enabled = true;
            }
        }

       
    }
}
