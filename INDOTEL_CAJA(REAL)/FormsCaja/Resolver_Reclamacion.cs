using INDOTEL_CAJA_REAL_.Clases;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_.FormsCaja
{
    public partial class Resolver_Reclamacion : Form
    {
        private readonly int reclamacionId;
        private readonly string expediente;

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

            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                btnResolver.Enabled = false;
                MessageBox.Show(
                    "Su perfil solo tiene permiso de consulta.",
                    "Acceso de solo lectura",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private async void btnResolver_Click(object sender, EventArgs e)
        {
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MessageBox.Show("No tiene permiso para resolver reclamaciones.");
                return;
            }

            if (cmbResultado.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un resultado.");
                return;
            }

            var comentario = txtComentarioResolucion.Text?.Trim();
            if (string.IsNullOrWhiteSpace(comentario))
            {
                MessageBox.Show("Ingrese un comentario.");
                return;
            }

            decimal? monto = null;
            if (!string.IsNullOrWhiteSpace(txtMontoAjuste.Text))
            {
                if (!decimal.TryParse(
                        txtMontoAjuste.Text,
                        NumberStyles.Number,
                        CultureInfo.CurrentCulture,
                        out var valor) || valor < 0)
                {
                    MessageBox.Show("El monto debe ser un numero mayor o igual a cero.");
                    return;
                }

                monto = valor;
            }

            btnResolver.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var dto = new ResolverReclamacionDto
                {
                    ResultadoResolucion = cmbResultado.Text,
                    ComentarioResolucion = comentario,
                    FundamentoResolucion = txtFundamentoResolucion.Text?.Trim() ?? string.Empty,
                    AccionOrdenada = txtAccionOrdenada.Text?.Trim() ?? string.Empty,
                    MontoAjuste = monto
                };

                var servicio = new ServicioReclamaciones();
                var respuesta = await servicio.Resolver(reclamacionId, dto);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible resolver la reclamacion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Reclamacion resuelta correctamente.");
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                Cursor = Cursors.Default;
                btnResolver.Enabled = PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
