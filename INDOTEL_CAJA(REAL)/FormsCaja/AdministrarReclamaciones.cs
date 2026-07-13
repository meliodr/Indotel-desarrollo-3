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
    public partial class AdministrarReclamaciones : Form
    {
        public AdministrarReclamaciones()
        {
            InitializeComponent();
        }

        private async void AdministrarReclamaciones_Load(object sender, EventArgs e)
        {
            await CargarTodas();
        }

        private async void btnBuscarCedula_Click(object sender, EventArgs e)
        {
            ServicioReclamaciones servicio =
         new ServicioReclamaciones();

            /*if (!string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                var respuesta =
                    await servicio.BuscarPorCedula(txtCedula.Text);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(respuesta.Mensaje);
                    return;
                }

                dgvReclamaciones.DataSource =
                    respuesta.Datos;

                return;
            }*/

            if (!string.IsNullOrWhiteSpace(txtExpediente.Text))
            {
                var respuesta =
                    await servicio.BuscarExpediente(
                        txtExpediente.Text);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(respuesta.Mensaje);
                    return;
                }

                List<Reclamacion> lista =
                    new List<Reclamacion>();

                lista.Add(respuesta.Datos);

                dgvReclamaciones.DataSource =
                    lista;

                return;
            }

            MessageBox.Show(
                "Debe un número de expediente.");

        }

       

        private async void btnActualizar_Click(object sender, EventArgs e)
        {
            //txtCedula.Clear();

            txtExpediente.Clear();

            await CargarTodas();

        }

        private void btnVer_Click(object sender, EventArgs e)
        {
            if (dgvReclamaciones.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reclamación.");
                return;
            }

            int id = Convert.ToInt32(
                dgvReclamaciones.CurrentRow.Cells["Id"].Value);

           Detalle_Reclamaciones frm =
                new Detalle_Reclamaciones(id);

            frm.ShowDialog();

        }

        private async Task CargarTodas()
        {
            ServicioReclamaciones servicio =
                new ServicioReclamaciones();

            var respuesta =
                await servicio.ObtenerTodas();

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(respuesta.Mensaje);
                return;
            }

            dgvReclamaciones.DataSource =
                respuesta.Datos;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtExpediente_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
