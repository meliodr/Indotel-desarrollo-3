using INDOTEL_CAJA_REAL_.Clases;
using System;
using System.Collections.Generic;
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
            btnBuscarCedula.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                var servicio = new ServicioReclamaciones();

                if (!string.IsNullOrWhiteSpace(txtCedula.Text))
                {
                    if (!ValidacionesCaja.CedulaValida(txtCedula.Text))
                    {
                        MessageBox.Show("La cedula debe contener 11 digitos.");
                        return;
                    }

                    var porCedula = await servicio.BuscarPorCedula(txtCedula.Text);
                    MostrarResultado(porCedula);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(txtExpediente.Text))
                {
                    var respuesta = await servicio.Buscar(
                        numeroExpediente: txtExpediente.Text,
                        page: 1,
                        pageSize: 100);

                    if (!respuesta.Exitoso)
                    {
                        MessageBox.Show(
                            respuesta.MensajeConReferencia,
                            "Busqueda no completada",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    dgvReclamaciones.DataSource = respuesta.Datos?.Data ?? new List<Reclamacion>();
                    return;
                }

                MessageBox.Show("Indique una cedula o un numero de expediente.");
            }
            finally
            {
                Cursor = Cursors.Default;
                btnBuscarCedula.Enabled = true;
            }
        }

        private async void btnActualizar_Click(object sender, EventArgs e)
        {
            txtCedula.Clear();
            txtExpediente.Clear();
            await CargarTodas();
        }

        private void btnVer_Click(object sender, EventArgs e)
        {
            if (dgvReclamaciones.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una reclamacion.");
                return;
            }

            var id = Convert.ToInt32(dgvReclamaciones.CurrentRow.Cells["Id"].Value);
            using (var detalle = new Detalle_Reclamaciones(id))
            {
                detalle.ShowDialog(this);
            }
        }

        private async Task CargarTodas()
        {
            btnActualizar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var servicio = new ServicioReclamaciones();
                var respuesta = await servicio.Buscar(page: 1, pageSize: 100);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible cargar las reclamaciones",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                dgvReclamaciones.DataSource = respuesta.Datos?.Data ?? new List<Reclamacion>();
            }
            finally
            {
                Cursor = Cursors.Default;
                btnActualizar.Enabled = true;
            }
        }

        private void MostrarResultado(ApiResponse<List<Reclamacion>> respuesta)
        {
            if (!respuesta.Exitoso)
            {
                MessageBox.Show(
                    respuesta.MensajeConReferencia,
                    "Busqueda no completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            dgvReclamaciones.DataSource = respuesta.Datos ?? new List<Reclamacion>();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void txtExpediente_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
