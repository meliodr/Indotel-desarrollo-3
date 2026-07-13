using INDOTEL_CAJA_REAL_.Clases;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INDOTEL_CAJA_REAL_
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.ThreadException += (_, args) =>
            {
                RegistroLocal.Error("Excepcion de interfaz", args.Exception);
                MessageBox.Show(
                    "Ocurrio un error inesperado. La operacion no pudo completarse.",
                    "INDOTEL Caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception exception)
                {
                    RegistroLocal.Error("Excepcion no controlada", exception);
                }
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                RegistroLocal.Error("Tarea no observada", args.Exception);
                args.SetObserved();
            };

            Application.Run(new Login());
        }
    }
}
