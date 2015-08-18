using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AplicacionNotificacionArriendo.Resources;

namespace AplicacionNotificacionArriendo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GrowlNotifiactions _growlNotifications = new GrowlNotifiactions();
        private int _tipo = 1;
        private int _visto = 1;
        private NotifyIcon _icon = new NotifyIcon();
        private MainWindow.ConfiguraModel modeloConfig = new MainWindow.ConfiguraModel();
        private const double TopOffset = 20.0;
        private const double LeftOffset = 480.0;
        private DateTime _ultimafecha;

        private bool cierra = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += new EventHandler(this.Window1_Closed);
            this.Deactivated += new EventHandler(this.Window1_Deactivated);
            this._growlNotifications.Top = SystemParameters.WorkArea.Top + 20.0;
            GrowlNotifiactions growlNotifiactions = this._growlNotifications;
            Rect workArea = SystemParameters.WorkArea;
            double left = workArea.Left;
            workArea = SystemParameters.WorkArea;
            double width = workArea.Width;
            double num = left + width - 480.0;
            growlNotifiactions.Left = num;

            this._icon.Visible = true;
            this._icon.Icon = Resource1.iconoSistema;
            this._icon.ContextMenu = new System.Windows.Forms.ContextMenu();
            this._icon.Text = @"ARRIENDO ;)";
            this._icon.ContextMenu.MenuItems.Add("Maximizar Ventana");
            this._icon.ContextMenu.MenuItems.Add("Minimizar Ventana");
            this._icon.ContextMenu.MenuItems.Add("Salir");
            this._icon.ContextMenu.MenuItems[1].Enabled = false;
            this._icon.ContextMenu.MenuItems[0].Click += new EventHandler(this.icon_DoubleClick);
            this._icon.ContextMenu.MenuItems[1].Click += new EventHandler(this.icon_Minimize);
            this._icon.ContextMenu.MenuItems[2].Click += new EventHandler(this.icon_Salir);
            this._icon.DoubleClick += new EventHandler(this.icon_DoubleClick);
            this.Hide();
            var iniFile = new IniFile("Resources/config.ini");
            this.modeloConfig.HoraAlarma = iniFile.Read("HoraAlarma", "General");
            this.modeloConfig.Intervalo = int.Parse(iniFile.Read("Intervalo", "General"));
            this.modeloConfig.HoraFin = iniFile.Read("HoraFinAlarma", "General");
            var backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };
            backgroundWorker.DoWork += new DoWorkEventHandler(this.worker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }

        private void icon_Minimize(object sender, EventArgs e)
        {
            this._icon.ContextMenu.MenuItems[1].Enabled = false;
            this._icon.ContextMenu.MenuItems[0].Enabled = true;
            this.Hide();
        }

        private void icon_Salir(object sender, EventArgs e)
        {
            string repeatPassword = MyDialog.Prompt("Ingrese Contraseña", "Confirmar para salir", inputType: MyDialog.InputType.Password);
            if (repeatPassword != null)
            {
                if (repeatPassword == "A1b1c1d1")
                {
                    cierra = true;
                    this.Close();
                    this._icon.Dispose();
                }
               
            }
           

        }

        private void icon_DoubleClick(object sender, EventArgs e)
        {
            this._icon.ContextMenu.MenuItems[0].Enabled = false;
            this._icon.ContextMenu.MenuItems[1].Enabled = true;
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void Window1_Deactivated(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
                return;
            this.Hide();
        }

        private void Window1_Closed(object sender, EventArgs e)
        {
            this.Hide();
            this.WindowState = WindowState.Minimized;
        }



        protected override void OnClosed(EventArgs e)
        {  
            this._growlNotifications.Close();
            base.OnClosed(e);
        }

        private void WindowLoaded1(object sender, RoutedEventArgs e)
        {
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            

            while (true)
            {
                DateTime dateTime1 = new DateTime();
                DateTime dateTime2;
                if (this._tipo == 1)
                {
                    dateTime2 = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + this.modeloConfig.HoraAlarma);
                }
                else
                {
                    this._visto = 1;
                    dateTime2 = this._ultimafecha.AddMinutes(this.modeloConfig.Intervalo);
                }

                string date = string.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now);

                if (Convert.ToDateTime(date) == dateTime2 && this._visto == 1)
                {
                    _tipo = 2;
                    _ultimafecha = Convert.ToDateTime(date);
                    this.Dispatcher.Invoke(
                      // Anonymous delegate that calls SetValue on the Window thread
                      (Action)delegate () {
                          _growlNotifications.AddNotification(new Notification()
                          {
                              Title = "Informativo",
                              ImageUrl = "pack://application:,,,/Resources/notification-icon.png",
                              Message = "RECUERDE. Todos los Contratos de Arriendo con FLETE que se encuentren en estado POR DESPACHAR después de las 15:00 hrs, serán programados para la ruta del día subsiguiente."
                          });
                      }, null);
                }
                Thread.Sleep(1000);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this._growlNotifications.AddNotification(new Notification()
            {
                Title = "Informativo",
                ImageUrl = "pack://application:,,,/Resources/notification-icon.png",
                Message = "RECUERDE. Todos los Contratos de Arriendo con FLETE que se encuentren en estado POR DESPACHAR después de las 15:00 hrs, serán programados para la ruta del día subsiguiente."
            });
        }

      

        public class ConfiguraModel
        {
            public string HoraAlarma { get; set; }

            public int Intervalo { get; set; }

            public string HoraFin { get; set; }
        }
       

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (cierra == false)
            {
                this._icon.ContextMenu.MenuItems[1].Enabled = false;
                this._icon.ContextMenu.MenuItems[0].Enabled = true;
                e.Cancel = true;

                this.ShowInTaskbar = false;

                this.WindowState = WindowState.Minimized;
            }
            
        }
    }
}