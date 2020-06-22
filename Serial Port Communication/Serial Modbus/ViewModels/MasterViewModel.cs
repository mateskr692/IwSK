using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Common;

namespace Serial_Modbus
{
    public class MasterViewModel : BaseViewModel
    {
        private Timer TransactionTimer = new Timer();
        private Stopwatch FrameStopwatch = new Stopwatch();
        private int FailedTransactions = 0;
        private SerialPort Port;
        private string Frame;

        public MasterViewModel()
        {
            this.Port = new SerialPort();
            this.Port.DataReceived += this.OnRecieved;
            this.Port.DiscardNull = false;
            this.Port.DtrEnable = true;
            this.Port.NewLine = "\r\n";

            this.TransactionTimer.AutoReset = false;
            this.TransactionTimer.Elapsed += delegate { this.OnTransactionTimeout(); };

            this.Ports = SerialPort.GetPortNames();
            if ( this.Ports.Count() == 0 )
                MessageBox.Show( "Nie znaleziono portu szeregowego" );
            this.SelectedPort = this?.Ports[ 0 ];

            this.TransmisionSpeeds = new int[] { 150, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200 };
            this.SelectedTransmissionSpeed = 9600;
            this.FrameFormats = new FrameFormat[] { FrameFormat._7E1, FrameFormat._7O1, FrameFormat._7N2 };
            this.SelectedFrameFormat = FrameFormat._7E1;
            this.ControlTypes = new Handshake[] { Handshake.None, Handshake.XOnXOff, Handshake.RequestToSend };
            this.SelectedControlType = Handshake.None;

            this.Commands = new int[] { 1, 2 };
            this.SelectedCommand = 1;
            this.FrameTimeout = 20;
            this.TransactionTimeout = 2000;
            this.Retransmitions = 2;

            this.Adress = 1;

            this.ConnectCommand = new RelayCommand( this.OnConnectCommand );
            this.SendCommand = new RelayCommand( this.OnSendCommand, new Predicate<object>( o => this.IsConnected ) );
        }

        #region Attributes
        public string[] Ports { get; private set; }
        public string SelectedPort { get => this.Port.PortName; set => this.Port.PortName = value; }

        public int[] TransmisionSpeeds { get; private set; }
        public int SelectedTransmissionSpeed { get => this.Port.BaudRate; set => this.Port.BaudRate = value; }

        public FrameFormat[] FrameFormats { get; private set; }
        private FrameFormat _SelectedFrameFormat;
        public FrameFormat SelectedFrameFormat
        {
            get => this._SelectedFrameFormat; set
            {
                this._SelectedFrameFormat = value;
                this.SetFrameFormat( value );
            }
        }

        public Handshake[] ControlTypes { get; private set; }
        public Handshake SelectedControlType { get => this.Port.Handshake; set => this.Port.Handshake = value; }

        private int _TransactionTimeout;
        public int TransactionTimeout
        {
            get => this._TransactionTimeout; set
            {
                value = value < 0 ? 0 : value;
                value = value > 10000 ? 10000 : value;
                value = 100 * (int)Math.Round( value / 100.0 );
                this._TransactionTimeout = value;
                this.TransactionTimer.Interval = value;
            }
        }

        private int _FrameTimeout;
        public int FrameTimeout
        {
            get => this._FrameTimeout; set
            {
                value = value < 0 ? 0 : value;
                value = value > 1000 ? 1000 : value;
                value = 10 * (int)Math.Round( value / 10.0 );
                this._FrameTimeout = value;
            }
        }

        private int _Retransmitions;
        public int Retransmitions
        {
            get => this._Retransmitions; set
            {
                value = value < 0 ? 0 : value;
                value = value > 5 ? 5 : value;
                this._Retransmitions = value;
            }
        }

        public string ConnectionStatus { get => this.IsConnected ? "Rozłącz" : "Połącz"; }

        private string _RecievedData;
        public string RecievedData { get => this._RecievedData; set { this._RecievedData = value; this.OnPropertyChanged(); } }
        private string _RecievedDataHex;
        public string RecievedDataHex { get => this._RecievedDataHex; set { this._RecievedDataHex = value; this.OnPropertyChanged(); } }

        private string _BufforData;
        public string BufforData
        {
            get => this._BufforData; set
            {
                this._BufforData = value;
                this.UpdateBufforedData();
                this.OnPropertyChanged();
            }
        }

        private string _BufforDataHex;
        public string BufforDataHex
        {
            get => this._BufforDataHex; set
            {
                this._BufforDataHex = value;
                this.OnPropertyChanged();
            }
        }

        public int[] Commands { get; set; }
        private int _SelectedCommand;
        public int SelectedCommand
        {
            get => this._SelectedCommand; set
            {
                this._SelectedCommand = value;
                this.OnCommandSelected();
                this.UpdateBufforedData();
            }
        }

        private int _Adress;
        public int Adress
        {
            get => this._Adress; set
            {
                value = value < 0 ? 0 : value;
                value = value > 247 ? 247 : value;
                this._Adress = value;
                this.OnPropertyChanged();
                this.UpdateBufforedData();
            }
        }

        private bool _AdressEnabled;
        public bool AdressEnabled
        {
            get => this._AdressEnabled; set
            {
                this._AdressEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _Broadcast;
        public bool Broadcast
        {
            get => this._Broadcast; set
            {
                this._Broadcast = value;
                this.OnBroadcastSelected();
                this.OnPropertyChanged();
            }
        }


        private bool _BroadcastEnabled;
        public bool BroadcastEnabled
        {
            get => this._BroadcastEnabled; set
            {
                this._BroadcastEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _BufforDataEnabled;
        public bool BufforDataEnabled
        {
            get => this._BufforDataEnabled; set
            {
                this._BufforDataEnabled = value;
                this.OnPropertyChanged();
            }
        }


        #endregion


        #region Commands
        public RelayCommand ConnectCommand { get; private set; }
        public void OnConnectCommand( object o )
        {
            if ( this.Port.IsOpen )
            {
                this.Port.Close();
                this.OnPropertyChanged( "ConnectionStatus" );
                this.OnPropertyChanged( "IsConnected" );
                return;
            }
            try
            {
                this.Port.Open();
                this.OnPropertyChanged( "ConnectionStatus" );
                this.OnPropertyChanged( "IsConnected" );
                this.Port.ReadExisting();
            }
            catch ( Exception )
            {
                MessageBox.Show( "Nie udalo sie polączyc do portu" );
            }
        }

        public RelayCommand SendCommand { get; private set; }
        public void OnSendCommand( object o )
        {
            this.Port.Write( this.Frame );

            if (this.AwaitReponse())
                this.TransactionTimer.Start();

            this.RecievedData = "";
            this.RecievedDataHex = "";
        }
        #endregion


        private bool IsConnected => this.Port.IsOpen;

        private void SetFrameFormat( FrameFormat format )
        {
            switch ( format )
            {
                case FrameFormat._7E1:
                    this.Port.DataBits = 7;
                    this.Port.Parity = Parity.Even;
                    this.Port.StopBits = StopBits.One;
                    break;

                case FrameFormat._7N2:
                    this.Port.DataBits = 7;
                    this.Port.Parity = Parity.None;
                    this.Port.StopBits = StopBits.Two;
                    break;

                case FrameFormat._7O1:
                    this.Port.DataBits = 7;
                    this.Port.Parity = Parity.Odd;
                    this.Port.StopBits = StopBits.One;
                    break;
            }
        }

        private void OnCommandSelected()
        {
            switch(this.SelectedCommand)
            {
                //wysłanie tekstu ze stacji Master do stacji Slave
                case 1:
                    this.BufforDataEnabled = true;
                    this.BroadcastEnabled = true;
                    this.AdressEnabled = true;
                    break;

                //odczyt tekstu ze stacji Slave
                case 2:
                    this.BufforDataEnabled = false;
                    this.BufforData = "";
                    this.BroadcastEnabled = false;
                    this.Broadcast = false;
                    this.AdressEnabled = true;
                    this.Adress = 1;
                    break;
            }
        }

        private bool AwaitReponse()
        {
            switch(this.SelectedCommand)
            {
                case 1: return false;
                case 2: return true;
            }

            return false;
        }

        private void UpdateBufforedData()
        {
            this.Frame = Converter.ToASCII( (byte)this.SelectedCommand, (byte)this.Adress, this.BufforData );
            this.BufforDataHex = Converter.ToHex( this.Frame );
        }

        private void OnBroadcastSelected()
        {
            if ( this.Broadcast == true)
            {
                this.AdressEnabled = false;
                this.Adress = 0;
            }
            else
            {
                this.AdressEnabled = true;
                this.Adress = 1;
            }
        }

        private void OnTransactionTimeout()
        {
            this.FailedTransactions++;
            if(this.FailedTransactions >= this.Retransmitions)
            {
                this.FailedTransactions = 0;
                MessageBox.Show( "Brakodpowiedzi od stacji Slave" );
                return;
            }
            this.OnSendCommand( null );
        }

        private void OnRecieved( object sender, SerialDataReceivedEventArgs e )
        {
            this.TransactionTimer.Stop();
            this.FrameStopwatch.Restart();
            string data = this.Port.ReadLine() + this.Port.NewLine;
            this.FrameStopwatch.Stop();
            if ( this.FrameStopwatch.ElapsedMilliseconds / data.Length > this.FrameTimeout )
            {
                this.TransactionTimer.Start();
                return;
            }

            this.RecievedDataHex = Converter.ToHex( data );

            var decoded = Converter.FromASCII( data, 0 );
            this.RecievedData = decoded.Item1;
        }

    }
}
