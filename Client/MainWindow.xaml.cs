using System;
using System.Net.Sockets;
using System.Windows;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Timers;

namespace Client
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
#pragma warning disable 0618 // per disabilitare il warning CS0618

    public partial class MainWindow : MetroWindow
    {
        private static TcpClient client ;        
        Thread threadAscolto;
        List<byte[]> lMessaggiDaInviareAldevice = new List<byte[]>();
        bool active = false;

        Thread threadInvioRisposta;
        enum STATE_MACHINE_STATE_POLL_LOOP { S_IDLE, S_CHECK_NUM_MESSAGE_WRITE, S_WRITE_MESSAGE, S_WAIT_WRITE, S_WAIT_READ, S_READ_MESSAGE };
        private STATE_MACHINE_STATE_POLL_LOOP state;
        private List<byte[]> internalMessaggiDaInviare;
        private byte[] tempMessaggio;
        int iLenghtMessaggio;
        SerialPort tempSerialPort;
        System.Timers.Timer timeMaxWaitResponse = new System.Timers.Timer();
        int iTime = 500; //ms

        public List<byte[]> MessaggiDaInviare { get { lock (lMessaggiDaInviareAldevice) { return lMessaggiDaInviareAldevice; } } }
            

        public MainWindow()
        {
            InitializeComponent();
            BT_OpenClient.IsEnabled = true;
            BT_CloseClient.IsEnabled = false;
        }

        static bool Connect(string serverIP)
        {
            bool bRet = false;
            string output = "";

            try
            {
                // Create a TcpClient.
                // The client requires a TcpServer that is connected
                // to the same address specified by the server and port
                // combination.
                Int32 port = 7000;
                client = new TcpClient(serverIP, port);
                bRet = true;                

                // Translate the passed message into ASCII and store it as a byte array.
                //Byte[] data = new Byte[256];
                //data = System.Text.Encoding.ASCII.GetBytes(message);

                //// Get a client stream for reading and writing.
                //// Stream stream = client.GetStream();
                //NetworkStream stream = client.GetStream();

                //// Send the message to the connected TcpServer. 
                //stream.Write(data, 0, data.Length);

                //output = "Sent: " + message;
                //MessageBox.Show(output);

                //// Buffer to store the response bytes.
                //data = new Byte[256];

                //// String to store the response ASCII representation.
                //String responseData = String.Empty;

                //// Read the first batch of the TcpServer response bytes.
                //Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = Encoding.ASCII.GetString(data, 0, bytes);
                //output = "Received: " + responseData;
                //MessageBox.Show(output);

                //// Close everything.
                //stream.Close();
                //client.Close();
            }
            catch (Exception e)
            {
                output = "ArgumentNullException: " + e;
                MessageBox.Show(output);
            }
            return bRet;
        }

        private void BT_OpenClient_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddress.Text;
            if (Connect(ipAddress) == true)
            {
                BT_OpenClient.IsEnabled = false;
                BT_CloseClient.IsEnabled = true;
                //serial port
                               
                if (tempSerialPort == null)
                {
                    tempSerialPort = new SerialPort("COM3", 38400,Parity.None, 8, StopBits.Two);                    
                }

                //avvio il thread di ascolto dati
                if (threadAscolto == null)
                {
                    threadAscolto = new Thread(Ascolto);
                    threadAscolto.Start();
                }
                else
                {
                    threadAscolto.Resume();
                }

                //avvio il thread  che invia i messaggi alla porta e prendo anche la risposta
                if (threadInvioRisposta == null)
                {
                    threadInvioRisposta = new Thread(InvioRisposta);
                    threadInvioRisposta.Start();
                    //timer timeout attesa risposta
                    timeMaxWaitResponse.Interval = iTime;
                    timeMaxWaitResponse.Elapsed += OnTimerEventTimeMaxWaitResponse;
                    timeMaxWaitResponse.AutoReset = false;
                }
                else
                {
                    threadInvioRisposta.Resume();
                }
            }
        }
        private void BT_CloseClient_Click(object sender, RoutedEventArgs e)
        {
            //stoppo il thread di ascolto
            threadAscolto.Suspend();
            threadInvioRisposta.Suspend();

            BT_OpenClient.IsEnabled = true;
            BT_CloseClient.IsEnabled = false;

            client.Close();            
        }
        private void Ascolto()
        {
            active = true;
            while (active)
            {           
                if(client.Connected == true)
                { 
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] data = new byte[8];
                        stream.Read(data, 0, data.Length);
                        lMessaggiDaInviareAldevice.Add(data);                        
                        Thread.Sleep(2000);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }                    
                }
            }
        }

        //////////////////////////////////////////////////////////
        private void OnTimerEventTimeMaxWaitResponse(object sender, ElapsedEventArgs e)
        {
            lock (internalMessaggiDaInviare)
            {
                //MainWindow.logger.Info("Timer: numeroSlave: {0}", tempMessage.DeviceInfo.slaveAddress);
                if (internalMessaggiDaInviare.Count > 0)
                {
                    internalMessaggiDaInviare.RemoveAt(0);                    
                    //MainWindow.logger.Info("Slave Address {0}: message Timeout", tempMessage.DeviceInfo.sensor_slaveAddress);
                }

                Console.WriteLine("Timeout: slave address {0}", tempMessaggio[0]);

                tempSerialPort.Dispose();

                state = STATE_MACHINE_STATE_POLL_LOOP.S_IDLE;
                timeMaxWaitResponse.Stop();
            }
        }
        
        private void InvioRisposta()
        {
            while (active)
            {
                switch (state)
                {
                    case STATE_MACHINE_STATE_POLL_LOOP.S_IDLE:
                        {
                            Console.WriteLine("S_IDLE");

                            internalMessaggiDaInviare = MessaggiDaInviare;
                            if (internalMessaggiDaInviare != null)
                            {
                                if (internalMessaggiDaInviare.Count > 0)
                                {
                                    //MainWindow.logger.Debug("Num Message To send: {0}", internalMessageToSend.Count);
                                    state = STATE_MACHINE_STATE_POLL_LOOP.S_WRITE_MESSAGE;
                                }
                                else
                                {
                                    state = STATE_MACHINE_STATE_POLL_LOOP.S_IDLE;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                state = STATE_MACHINE_STATE_POLL_LOOP.S_IDLE;
                                Thread.Sleep(1000);
                            }
                        }
                        break;

                    case STATE_MACHINE_STATE_POLL_LOOP.S_WRITE_MESSAGE:
                        {
                            Console.WriteLine("S_WRITE_MESSAGE");

                            //MainWindow.logger.Debug("STATE_MACHINE_STATE.S_WRITE_MESSAGE");
                            tempMessaggio = internalMessaggiDaInviare[0];

                            if (tempMessaggio.Length != 0)
                            {
                                if (WriteMessageRS485() == true)
                                {
                                    state = STATE_MACHINE_STATE_POLL_LOOP.S_WAIT_WRITE;
                                }
                                else
                                {
                                    state = STATE_MACHINE_STATE_POLL_LOOP.S_IDLE;
                                }
                            }
                        }
                        break;
                    case STATE_MACHINE_STATE_POLL_LOOP.S_WAIT_WRITE:
                        {
                            Console.WriteLine("S_WAIT_WRITE");

                            //MainWindow.logger.Debug("STATE_MACHINE_STATE.S_WAIT_WRITE");
                                                        
                            if (tempSerialPort != null && tempSerialPort.BytesToWrite == 0)
                            {
                                state = STATE_MACHINE_STATE_POLL_LOOP.S_WAIT_READ;
                                timeMaxWaitResponse.Start();
                            }
                        }
                        break;
                    case STATE_MACHINE_STATE_POLL_LOOP.S_WAIT_READ:
                        {
                            Console.WriteLine("S_WAIT_READ SlaveAddress: {0}", tempMessaggio[0]);

                            //MainWindow.logger.Debug("STATE_MACHINE_STATE.S_WAIT_READ");

                            if (tempSerialPort != null && tempSerialPort.IsOpen == true)
                            {
                                if(tempSerialPort.BytesToRead > 0)
                                {
                                    state = STATE_MACHINE_STATE_POLL_LOOP.S_READ_MESSAGE;
                                }                                
                            }                            
                        }
                        break;
                    case STATE_MACHINE_STATE_POLL_LOOP.S_READ_MESSAGE:
                        {
                            Console.WriteLine("S_READ_MESSAGE");

                            //MainWindow.logger.Debug("STATE_MACHINE_STATE.S_WAIT_READ");
                            if (ReadMessageRS485() == true)
                            {
                                state = STATE_MACHINE_STATE_POLL_LOOP.S_IDLE;
                            }
                            else
                            {
                                state = STATE_MACHINE_STATE_POLL_LOOP.S_READ_MESSAGE;
                            }                          
                        }
                        break;
                }
            }
        }

        private bool WriteMessageRS485()
        {
            bool bRet = false;                        
            try
            {
                if (tempSerialPort != null)
                {
                    if (tempSerialPort.IsOpen)
                    {
                        tempSerialPort.DiscardInBuffer();
                        tempSerialPort.Write(tempMessaggio, 0, tempMessaggio.Length);

                        //ricavo quanto deve essere lungo in messaggio di risposta
                        iLenghtMessaggio = (int)(tempMessaggio[4] << 8);
                        iLenghtMessaggio += (int)(tempMessaggio[5]);
                        iLenghtMessaggio = iLenghtMessaggio * 2 + 5;

                        bRet = true;
                    }
                    else
                    {
                        tempSerialPort.Close();
                        tempSerialPort.Open();
                    }
                }
                else
                {
                    lock (internalMessaggiDaInviare)
                    {
                        internalMessaggiDaInviare.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            return bRet;
        }
        private bool ReadMessageRS485()
        {
            bool bRet = false; 
                    
            if (tempSerialPort != null && tempSerialPort.BytesToRead == iLenghtMessaggio)
            {
                timeMaxWaitResponse.Stop();
                byte[] tempMessaggioRisposta  = new byte[iLenghtMessaggio];
                
                tempSerialPort.Read(tempMessaggioRisposta, 0, iLenghtMessaggio);
                Console.WriteLine("Slave Address {0}: Messaggio Letto", tempMessaggioRisposta[0]);
                NetworkStream stream = client.GetStream();
                
                stream.Write(tempMessaggioRisposta, 0, tempMessaggioRisposta.Length);
                

                //rimuovo il messaggio che ho gestito dalla lista
                internalMessaggiDaInviare.RemoveAt(0);

                tempSerialPort.Close();

                bRet = true;
            }

            return bRet;           
        }
        //////////////////////////////////////////////////////////
    }
}
