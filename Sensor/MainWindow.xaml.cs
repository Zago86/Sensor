using System;
using System.Windows;
using MahApps.Metro.Controls;
using System.Reflection;
using NLog;
using System.Windows.Threading;
using System.Windows.Media;

namespace Sensor
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>  
    public partial class MainWindow : MetroWindow
    {
        #region VARIABILI
        public static Logger logger = LogManager.GetCurrentClassLogger();
               
        DispatcherTimer timerConnectionStatus;
        public static ConnectedDevices connectedDevices;

        TickData varTickData;

        public static DB_MYSQL DataBase;
        #endregion VARIABILI


        public MainWindow()
        {
            InitializeComponent();

            this.Title = "Sensor " + Assembly.GetEntryAssembly().GetName().Version.ToString();
            logger.Info("Avvio Applicazione: {0}", this.Title);            
        }

        #region BOTTONI 
        private void BT_StartComunication_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetAndStartTimerConnectionStatus();
                connectedDevices = new ConnectedDevices();
                //creo il database e la tabella se non esiste
                DataBase = new DB_MYSQL(connectedDevices);

                connectedDevices.OpenConnectionDevice();
                connectedDevices.StartTimerPeriodicMessage();               

                varTickData = new TickData();                                                      
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString);
            }
        }
        private void BT_StopComunication_Click(object sender, RoutedEventArgs e)
        {
            if(connectedDevices != null)
            { 
                if(varTickData != null)
                {
                    varTickData.StopThreds();
                }
                ConnectionSetAndOpen.CloseComunication(ref ConnectedDevices.TotaldeviceConnected);
                BT_StartComunication.IsEnabled = true;
                BT_StopComunication.IsEnabled = false;
            }           

            tbTX_System_1.Background = Brushes.Gray;
            tbRX_System_1.Background = Brushes.Gray;
            tbTX_System_2.Background = Brushes.Gray;
            tbRX_System_2.Background = Brushes.Gray;
            tbTX_System_3.Background = Brushes.Gray;
            tbRX_System_3.Background = Brushes.Gray;

        }
        #endregion BOTTONI
        private void SetAndStartTimerConnectionStatus()
        {
            timerConnectionStatus = new DispatcherTimer();
            timerConnectionStatus.Tick += new EventHandler(OnTimerEventConnectionStatus);
            timerConnectionStatus.Interval = new TimeSpan(0, 0, 3);
            timerConnectionStatus.Start();
        }

        private void OnTimerEventConnectionStatus(object sender, EventArgs e)
        {
            #region Grafica Bottoni Start/Stop
            if(connectedDevices != null)
            { 
                if(ConnectionSetAndOpen.GetConnectionStatus())
                {
                    BT_StartComunication.IsEnabled = false;
                    BT_StopComunication.IsEnabled = true;
                }
                else
                {
                    BT_StartComunication.IsEnabled = true;
                    BT_StopComunication.IsEnabled = false;
                }
            }
            #endregion Grafica Bottoni Start/Stop
            #region Grafica invio ricezione messaggio al device
            if (varTickData != null)
            {
                if (varTickData.Getstate()[0] == 0) //messaggio trasmesso
                {
                    if(varTickData.Getstate()[1] == 1)
                    {
                        //system1
                        Dispatcher.Invoke(new Action(() => { tbTX_System_1.Background = Brushes.Green; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_1.Background = Brushes.Gray; }));
                        //system2
                        Dispatcher.Invoke(new Action(() => { tbTX_System_2.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_2.Background = Brushes.Gray; }));
                        //system3
                        Dispatcher.Invoke(new Action(() => { tbTX_System_3.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_3.Background = Brushes.Gray; }));
                    }
                    else if(varTickData.Getstate()[1] == 2) 
                    {
                        //system1
                        Dispatcher.Invoke(new Action(() => { tbTX_System_1.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_1.Background = Brushes.Gray; }));
                        //system2
                        Dispatcher.Invoke(new Action(() => { tbTX_System_2.Background = Brushes.Green; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_2.Background = Brushes.Gray; }));
                        //system3
                        Dispatcher.Invoke(new Action(() => { tbTX_System_3.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_3.Background = Brushes.Gray; }));
                                                
                    }
                    else if (varTickData.Getstate()[1] == 3)
                    {
                        //system1
                        Dispatcher.Invoke(new Action(() => { tbTX_System_1.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_1.Background = Brushes.Gray; }));
                        //system2
                        Dispatcher.Invoke(new Action(() => { tbTX_System_2.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_2.Background = Brushes.Gray; }));
                        //system3
                        Dispatcher.Invoke(new Action(() => { tbTX_System_3.Background = Brushes.Green; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_3.Background = Brushes.Gray; }));
                    }

                }
                else if (varTickData.Getstate()[0] == 1)//messaggio ricevuto
                {
                    if (varTickData.Getstate()[1] == 1)
                    {
                        //system1
                        Dispatcher.Invoke(new Action(() => { tbTX_System_1.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_1.Background = Brushes.Green; }));
                        //system2
                        Dispatcher.Invoke(new Action(() => { tbTX_System_2.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_2.Background = Brushes.Gray; }));
                        //system3
                        Dispatcher.Invoke(new Action(() => { tbTX_System_3.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_3.Background = Brushes.Gray; }));                       
                    }
                    else if (varTickData.Getstate()[1] == 2)
                    {
                        //system1
                        Dispatcher.Invoke(new Action(() => { tbTX_System_1.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_1.Background = Brushes.Gray; }));
                        //system2
                        Dispatcher.Invoke(new Action(() => { tbTX_System_2.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_2.Background = Brushes.Green; }));
                        //system3
                        Dispatcher.Invoke(new Action(() => { tbTX_System_3.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_3.Background = Brushes.Gray; }));                       
                    }
                    else if (varTickData.Getstate()[1] == 3)
                    {
                        //system1
                        Dispatcher.Invoke(new Action(() => { tbTX_System_1.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_1.Background = Brushes.Gray; }));
                        //system2
                        Dispatcher.Invoke(new Action(() => { tbTX_System_2.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_2.Background = Brushes.Gray; }));
                        //system3
                        Dispatcher.Invoke(new Action(() => { tbTX_System_3.Background = Brushes.Gray; }));
                        Dispatcher.Invoke(new Action(() => { tbRX_System_3.Background = Brushes.Green; }));
                    }
                }
            }
            #endregion Grafica invio ricezione messaggio al device

            #region Grafica Database comunication
            if (DataBase != null)
            {
                lock (ConnectedDevices.TotaldeviceConnected)
                {
                    foreach (ConnectedDevices.ModelAndNameDevice device in ConnectedDevices.TotaldeviceConnected)
                    {
                        DataBase.SendMessageStatus(device);
                    }
                }                

                if (DataBase.DB_ConnectionState != System.Data.ConnectionState.Closed)
                {
                    Dispatcher.Invoke(new Action(() => { txDB_Connection.Background = Brushes.Green; }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() => { txDB_Connection.Background = Brushes.Red; }));                    
                }                 
            }
            #endregion Grafica Database comunication
        }
    }         
}
