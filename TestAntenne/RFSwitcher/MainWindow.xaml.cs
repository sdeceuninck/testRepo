using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpringCardPCSC;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;

namespace RFSwitcher
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SCardReader reader;
        SCardChannel channel;
        BackgroundWorker updateStatus;
        ObservableCollection<string> logs;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string r = SpringCardPCSC.SCARD.GetReaderList()[0];
            if (String.IsNullOrEmpty(r)) textBlockReaderName.Text = "No READER FOUND";
            else
            {
                textBlockReaderName.Text = r;
                reader = new SCardReader(r);
                //channel = new SCardChannel(reader);
            
                //Launch bw for reader status
                updateStatus = new BackgroundWorker();
                updateStatus.DoWork += new DoWorkEventHandler(updateStatus_DoWork);
                updateStatus.WorkerSupportsCancellation = true;
                updateStatus.RunWorkerAsync();
                logs = new ObservableCollection<string>();
                listBoxLog.ItemsSource = logs;
            }


        }

        void UpdateStatusTextBlock(string s)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { textBlockReaderState.Text = s; }));

        }

        void AddLog(string s)
        {
            logs.Add(DateTime.Now.ToString("hh:mm:ss.ffff") + " : " + s);
        }
        void updateStatus_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!updateStatus.CancellationPending)
            {
                UpdateStatusTextBlock(reader.StatusAsString);
                Thread.Sleep(50);
            }
        }

        private CardBuffer SendControl(string cmd)
        {
            channel = new SCardChannel(reader.Name);
            channel.ShareMode = SpringCardPCSC.SCARD.SHARE_DIRECT;
            channel.Protocol = SpringCardPCSC.SCARD.PROTOCOL_NONE;
            if (channel.Connect())
            {
                //AddLog("Channel Connection : OK");
                CardBuffer cb2 = channel.Control(new CardBuffer(cmd));
                //AddLog("Result : " + cb2.GetBytes()[0]);
                channel.DisconnectLeave();
                return cb2;
            }
            else
            {
                throw new ApplicationException("Error on channel connection");
            }
        }

        private void CurrentSlot()
        {
            try
            {
                AddLog("Current Slot name : ");
                CardBuffer cb = SendControl("5821");
                if (cb != null)
                {
                    AddLog(Encoding.ASCII.GetString(cb.GetBytes()));
                }
            }
            catch (Exception ex)
            {
                AddLog("Error reading current slot : " + ex.Message);
            }
        }

        private void StopRf()
        {
            AddLog("Stop RF");
            channel = new SCardChannel(reader.Name);
            channel.ShareMode = SpringCardPCSC.SCARD.SHARE_DIRECT;
            channel.Protocol = SpringCardPCSC.SCARD.PROTOCOL_NONE;
            if (channel.Connect())
            {
                AddLog("Channel Connection : OK");
                
                CardBuffer cb2 = channel.Control(new CardBuffer("582200"));
                AddLog("Result : " + cb2.GetBytes()[0]);
                channel.DisconnectLeave();
            }
            else
            {
                AddLog("Error on channel connection");
            }
        }


        private void StartRf()
        {
            AddLog("Start RF");
            channel = new SCardChannel(reader.Name);
            channel.ShareMode = SpringCardPCSC.SCARD.SHARE_DIRECT;
            channel.Protocol = SpringCardPCSC.SCARD.PROTOCOL_NONE;
            if (channel.Connect())
            {
                AddLog("Channel Connection : OK");
                
                CardBuffer cb2 = channel.Control(new CardBuffer("582300"));
                AddLog("Result : " + cb2.GetBytes()[0]);
                channel.DisconnectLeave();
            }
            else
            {
                AddLog("Error on channel connection");
            }
        }

        private void buttonStopRf_Click(object sender, RoutedEventArgs e)
        {
            StopRf();
        }

        private void buttonStartRf_Click(object sender, RoutedEventArgs e)
        {
            StartRf();
        }

        private void GetSerial()
        {
            channel = new SCardChannel(reader.Name);
            if (!channel.Connected)
            {
                if (!channel.Connect())
                {
                    AddLog("connection to card failed"); return;
                }
            }

            SpringCardPCSC.CAPDU cmd = new SpringCardPCSC.CAPDU(new byte[] { 0xFF, 0xCA, 0x00, 0x00 });
            SpringCardPCSC.RAPDU result = channel.Transmit(cmd);
            if (result.SWString.Equals("Success"))
            {
                AddLog("Serial successfully read");
            }
            else
            {
                AddLog("Erreur de lecture du numéro de série");
            }

        }

        private void buttonReadSerial_Click(object sender, RoutedEventArgs e)
        {
            GetSerial();
        }

        private void buttonReadCurrentSlot_Click(object sender, RoutedEventArgs e)
        {
            CurrentSlot();
        }
    }
}
