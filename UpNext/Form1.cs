using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Velopack;
using Velopack.Sources;

namespace UpNext
{
    public partial class Form1 : Form
    {
        private TextBox _log;
        private Button _btn;

        public Form1()
        {
            InitializeComponent();

            Text = "Buscador de actualizaciones";
            Width = 640; Height = 400;

            _btn = new Button { Text = "Buscar actualizaciones", Dock = DockStyle.Top, Height = 40 };
            _btn.Click += async (s, e) => await BuscarActualizacionesAsync();

            _log = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };

            Controls.Add(_log);
            Controls.Add(_btn);
        }

        private void Log(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Log(msg)));
                return;
            }
            _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }

        private async Task BuscarActualizacionesAsync()
        {
            try
            {
                _btn.Enabled = false;
                Log("Comprobando actualizaciones...");

                // ⚡ Para repos públicos pasa "null" como token
                var gh = new GithubSource("https://github.com/Gustavo7239/UpNext", null, false, null);

                var mgr = new UpdateManager(gh);

                var info = await mgr.CheckForUpdatesAsync();
                if (info == null)
                {
                    Log("No hay actualizaciones disponibles.");
                    return;
                }

                Log($"Actualización encontrada: {info.TargetFullRelease.Version}");
                Log("Descargando...");

                await mgr.DownloadUpdatesAsync(info, progress =>
                {
                    Log($"Progreso: {progress}%");
                });

                Log("Aplicando actualización y reiniciando...");
                await mgr.WaitExitThenApplyUpdatesAsync(info, restart: true);
            }
            catch (Exception ex)
            {
                Log("Error durante la actualización: " + ex.Message);
            }
            finally
            {
                _btn.Enabled = true;
            }
        }

    }
}
