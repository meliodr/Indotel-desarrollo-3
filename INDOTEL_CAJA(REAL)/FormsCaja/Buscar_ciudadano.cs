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
    public partial class Buscar_ciudadano : Form
    {
        private Ciudadano ciudadanoSeleccionado;
        public Buscar_ciudadano()
        {
            InitializeComponent();
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

            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                MessageBox.Show("Digite una cédula.");
                txtCedula.Focus();
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                ServicioCiudadanos servicio =
                    new ServicioCiudadanos();

                var respuesta =
                    await servicio.BuscarPorCedula(txtCedula.Text.Trim());

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(
                        "No existe un ciudadano con esa cédula.");

                    Limpiar();

                    return;
                }

                ciudadanoSeleccionado =
                    respuesta.Datos;

                txtNombre.Text =
                    ciudadanoSeleccionado.Nombres;

                txtApellido.Text =
                    ciudadanoSeleccionado.Apellidos;

                txtTelefono.Text =
                    ciudadanoSeleccionado.Telefono;

                txtCorreo.Text =
                    ciudadanoSeleccionado.Correo;

                txtDireccion.Text =
                    ciudadanoSeleccionado.Direccion;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

        }

        public Ciudadano CiudadanoSeleccionado
        {
            get
            {
                return ciudadanoSeleccionado;
            }
        }

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
            REGISTRAR_CIUDADANO registro_ciudadano = new REGISTRAR_CIUDADANO();

            registro_ciudadano.Show();

            this.Hide();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }
    }
}
