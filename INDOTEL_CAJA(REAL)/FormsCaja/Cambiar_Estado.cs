using INDOTEL_CAJA_REAL_.Clases;
using System;
using System.Linq;
using System.Windows.Forms;

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

        private async void Cambiar_Estado_Load(object sender, EventArgs e)
        {
            txtExpediente.Text = expediente;
            txtEstadoActual.Text = estadoActual;

            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                btnGuardar.Enabled = false;
                MessageBox.Show(
                    "Su perfil solo tiene permiso de consulta.",
                    "Acceso de solo lectura",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            await CargarEstadosDisponibles();
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!PermisosCaja.PuedeGestionar(Sesion.Usuario?.Rol))
            {
                MessageBox.Show("No tiene permiso para cambiar estados.");
                return;
            }

            if (cmbEstadoNuevo.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un estado.");
                return;
            }

            var comentario = txtComentario.Text?.Trim();
            if (string.IsNullOrWhiteSpace(comentario))
            {
                MessageBox.Show("Indique el motivo o comentario del cambio.");
                txtComentario.Focus();
                return;
            }

            btnGuardar.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var dto = new CambiarEstadoReclamacionDto
                {
                    EstadoNuevo = cmbEstadoNuevo.SelectedItem.ToString(),
                    Comentario = comentario
                };

                var servicio = new ServicioReclamaciones();
                var respuesta = await servicio.CambiarEstado(reclamacionId, dto);

                if (!respuesta.Exitoso)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible cambiar el estado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Estado actualizado correctamente.");
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                Cursor = Cursors.Default;
                btnGuardar.Enabled = cmbEstadoNuevo.Items.Count > 0;
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async System.Threading.Tasks.Task CargarEstadosDisponibles()
        {
            cmbEstadoNuevo.Items.Clear();
            btnGuardar.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                var servicio = new ServicioReclamaciones();
                var respuesta = await servicio.ObtenerTransiciones(reclamacionId);

                if (!respuesta.Exitoso || respuesta.Datos == null)
                {
                    MessageBox.Show(
                        respuesta.MensajeConReferencia,
                        "No fue posible consultar las transiciones",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                txtEstadoActual.Text = respuesta.Datos.EstadoActual;
                foreach (var estado in respuesta.Datos.TransicionesPermitidas.Distinct())
                {
                    cmbEstadoNuevo.Items.Add(estado);
                }

                if (cmbEstadoNuevo.Items.Count > 0)
                {
                    cmbEstadoNuevo.SelectedIndex = 0;
                    btnGuardar.Enabled = respuesta.Datos.PuedeCambiarEstado;
                }
                else
                {
                    MessageBox.Show(
                        "No existen transiciones disponibles para el estado y rol actuales.",
                        "Sin acciones disponibles",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
