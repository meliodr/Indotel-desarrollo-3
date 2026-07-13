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
using System.Windows.Forms.VisualStyles;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class Cambiar_Estado : Form
    {
        private readonly int reclamacionId;

        private readonly string expediente;

        private readonly string estadoActual;

        public Cambiar_Estado(int id, string expediente, string estado)
        {
            InitializeComponent();

            reclamacionId = id;

            this.expediente = expediente;

            estadoActual = estado;

        }

        private void Cambiar_Estado_Load(object sender, EventArgs e)
        {
            txtExpediente.Text =expediente;

            txtEstadoActual.Text =estadoActual;

            CargarEstadosDisponibles();

        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbEstadoNuevo.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Seleccione un estado.");

                return;
            }

            CambiarEstadoReclamacionDto dto =
                new CambiarEstadoReclamacionDto
                {
                    EstadoNuevo = cmbEstadoNuevo.Text,

                    Comentario = txtComentario.Text
                };

            ServicioReclamaciones servicio =new ServicioReclamaciones();

            var respuesta =await servicio.CambiarEstado(reclamacionId,dto);

            if (!respuesta.Exitoso)
            {
                MessageBox.Show(
                    respuesta.Mensaje);

                return;
            }

            MessageBox.Show(
                "Estado actualizado correctamente.");

            DialogResult = DialogResult.OK;

            Close();

        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }

        private void CargarEstadosDisponibles()
        {
            cmbEstadoNuevo.Items.Clear();

            switch (estadoActual)
            {
                case "RECIBIDA":
                    cmbEstadoNuevo.Items.Add("VALIDADA");
                    break;

                case "VALIDADA":
                    cmbEstadoNuevo.Items.Add("ENVIADA_A_PRESTADORA");
                    break;

                case "ENVIADA_A_PRESTADORA":
                    cmbEstadoNuevo.Items.Add("RESPONDIDA_POR_PRESTADORA");
                    break;

                case "RESPONDIDA_POR_PRESTADORA":
                    cmbEstadoNuevo.Items.Add("EN_REVISION_INDOTEL");
                    break;

                case "EN_REVISION_INDOTEL":
                    cmbEstadoNuevo.Items.Add("RESUELTA");
                    break;

                case "RESUELTA":
                    cmbEstadoNuevo.Items.Add("CERRADA");
                    break;
            }
        }

    }
}
