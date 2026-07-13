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
    public partial class Resolver_Reclamacion : Form
    {
        private int reclamacionId;

        private string expediente;
        public Resolver_Reclamacion(int id, string numeroExpediente)
        {
            InitializeComponent();

            reclamacionId = id;

            expediente = numeroExpediente;

        }

        private void Resolver_Reclamacion_Load(object sender, EventArgs e)
        {
            txtExpediente.Text = expediente;

            cmbResultado.SelectedIndex = -1;

        }

        private async void btnResolver_Click(object sender, EventArgs e)
        {
            if (cmbResultado.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Seleccione un resultado.");

                return;
            }


            if (string.IsNullOrWhiteSpace(txtComentarioResolucion.Text))
            {
                MessageBox.Show(
                    "Ingrese un comentario.");

                return;
            }


            decimal? monto = null;


            if (!string.IsNullOrWhiteSpace(txtMontoAjuste.Text))
            {
                if (decimal.TryParse(txtMontoAjuste.Text,out decimal valor))
                {
                    monto = valor;
                }
                else
                {
                    MessageBox.Show("Monto inválido.");

                    return;
                }
            }



            ResolverReclamacionDto dto =
                new ResolverReclamacionDto
                {
                    ResultadoResolucion =cmbResultado.Text,

                    ComentarioResolucion =txtComentarioResolucion.Text,

                    FundamentoResolucion =txtFundamentoResolucion.Text,

                    AccionOrdenada =txtAccionOrdenada.Text,

                    MontoAjuste =monto
                };


            ServicioReclamaciones servicio =new ServicioReclamaciones();


            var respuesta =await servicio.Resolver(reclamacionId,dto);



            if (!respuesta.Exitoso)
            {
                MessageBox.Show(
                    respuesta.Mensaje);

                return;
            }


            MessageBox.Show("Reclamación resuelta correctamente.");


            DialogResult =DialogResult.OK;


            Close();

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Panel_Principal principal = new Panel_Principal();

            principal.Show();

            this.Hide();
        }

       
    }
}
