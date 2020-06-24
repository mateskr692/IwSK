using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using Common;

namespace Serial_RS232
{
    class MainViewModel : BaseViewModel
    {
        private string PingMessage = "ping";
        private Stopwatch Stopwatch = new Stopwatch();

        private string _Terminator;
        public string Terminator { get => this._Terminator; set {
                this._Terminator = value;
                if ( !string.IsNullOrEmpty( value ) )
                    this.Port.NewLine = value;
            } }
        private SelectedTerminator TerminatorType;
        private SerialPort Port;

        public MainViewModel()
        {
            this.Port = new SerialPort();
            this.Port.DataReceived += this.OnRecieved;
            //this.Port.DiscardNull = false;
            //this.Port.DtrEnable = true;

            this.Ports = SerialPort.GetPortNames();
            if ( this.Ports.Count() == 0 )
                MessageBox.Show( "Nie znaleziono portu szeregowego" );
            this.SelectedPort = this?.Ports[ 0 ];

            this.TransmisionSpeeds = new int[] { 150, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
            this.SelectedTransmissionSpeed = 9600;

            this.DataBits = new int[] { 5, 6, 7, 8 };
            this.SelectedDataBit = 7;

            this.ParityBits = new Parity[] { Parity.Even, Parity.Odd, Parity.None };
            this.SelectedParityBit = Parity.Even;

            this.StopBits = new StopBits[] { System.IO.Ports.StopBits.One, System.IO.Ports.StopBits.Two };
            this.SelectedStopBit = System.IO.Ports.StopBits.One;

            this.ControlTypes = new Handshake[] { Handshake.None, Handshake.XOnXOff, Handshake.RequestToSend };
            this.SelectedControlType = Handshake.None;

            this.StandardTerminators = new StandardTerminator[] { StandardTerminator.CR, StandardTerminator.LF, StandardTerminator.CRLF };
            this.SelectedStandardTerminator = StandardTerminator.CR;


            this.NoTerminatorCommand = new RelayCommand( this.OnNoTerminatorCommand );
            this.CustomTerminatorCommand = new RelayCommand( this.OnCustomTerminatorCommand );
            this.StandardTerminatorCommand = new RelayCommand( this.OnStandardTerminatorCommand );

            this.ConnectCommand = new RelayCommand( this.OnConnectCommand );
            this.ClearCommand = new RelayCommand( this.OnClearCommand );
            this.PingCommand = new RelayCommand( this.OnPingCommand, new Predicate<object>( o => this.IsConnected ) );
            this.SendCommand = new RelayCommand( this.OnSendCommand, new Predicate<object>( o => this.IsConnected ) );
        }

        #region Attributes
        public string[] Ports { get; private set; }
        public string SelectedPort { get => this.Port.PortName; set => this.Port.PortName = value; }

        public int[] TransmisionSpeeds { get; private set; }
        public int SelectedTransmissionSpeed { get => this.Port.BaudRate; set => this.Port.BaudRate = value; }

        public int[] DataBits { get; private set; }
        public int SelectedDataBit { get => this.Port.DataBits; set => this.Port.DataBits = value; }

        public Parity[] ParityBits { get; private set; }
        public Parity SelectedParityBit { get => this.Port.Parity; set => this.Port.Parity = value; }

        public StopBits[] StopBits { get; private set; }
        public StopBits SelectedStopBit { get => this.Port.StopBits; set => this.Port.StopBits = value; }

        public Handshake[] ControlTypes { get; private set; }
        public Handshake SelectedControlType { get => this.Port.Handshake; set => this.Port.Handshake = value; }

        public string ConnectionStatus { get => this.IsConnected ? "Rozłącz" : "Połącz"; }

        public StandardTerminator[] StandardTerminators { get; private set; }
        private StandardTerminator _SelectedStandardTerminator;
        public StandardTerminator SelectedStandardTerminator { get => this._SelectedStandardTerminator; set {
                this._SelectedStandardTerminator = value; 
                if( this.TerminatorType == SelectedTerminator.Standard)
                {
                    this.Terminator = this.TerminatorByType( this._SelectedStandardTerminator );
                }
            } }

        public string _CustomTerminator;
        public string CustomTerminator { get => this._CustomTerminator; set {
                this._CustomTerminator = value;
                if(this.TerminatorType == SelectedTerminator.Custom )
                {
                    this.Terminator = this._CustomTerminator;
                }
            } }

        //private bool _TransactionEnabled;
        //public bool TransactionEnabled { get => this._TransactionEnabled; set {
        //        this._TransactionEnabled = value;
        //        this.Port.ReadTimeout = value ? this._TransactionTimeout : SerialPort.InfiniteTimeout;
        //        this.Port.WriteTimeout = value ? this._TransactionTimeout : SerialPort.InfiniteTimeout;
        //        this.OnPropertyChanged();
        //        this.OnPropertyChanged( "Timeout" );
        //    }
        //}

        //private int _TransactionTimeout = 500;
        //public int TransactionTimeout { get => this._TransactionTimeout; set {
        //        this._TransactionTimeout = value > 0 ? value : 1;
        //        this.Port.ReadTimeout = this._TransactionTimeout;
        //        this.Port.WriteTimeout = this._TransactionTimeout;
        //        this.OnPropertyChanged();
        //        this.OnPropertyChanged( "Timeout" );
        //    } }

        //public int Timeout { get => this.Port.WriteTimeout; }

        private string _RecievedData;
        public string RecievedData { get => this._RecievedData; set { this._RecievedData = value; this.OnPropertyChanged(); } }
        public string BufforData { get; set; }
        #endregion


        #region Commands
        public RelayCommand ConnectCommand { get; private set; }
        public void OnConnectCommand(object o)
        {
            if(this.Port.IsOpen)
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
            catch( Exception )
            {
                MessageBox.Show( "Nie udalo sie polączyc do portu" );
            }
        }

        public RelayCommand NoTerminatorCommand { get; private set; }
        public void OnNoTerminatorCommand(object o)
        {
            this.TerminatorType = SelectedTerminator.None;
            this.Terminator = "";
        }

        public RelayCommand CustomTerminatorCommand { get; private set; }
        public void OnCustomTerminatorCommand( object o)
        {
            this.TerminatorType = SelectedTerminator.Custom;
            this.Terminator = this.CustomTerminator;
        }

        public RelayCommand StandardTerminatorCommand { get; private set; }
        public void OnStandardTerminatorCommand(object o)
        {
            this.TerminatorType = SelectedTerminator.Standard;
            this.Terminator = this.TerminatorByType( this.SelectedStandardTerminator );
        }

        public RelayCommand PingCommand { get; private set; }
        public void OnPingCommand(object o)
        {
            this.Stopwatch.Start();
            this.Send( this.PingMessage + this.Terminator );
        }

        public RelayCommand ClearCommand { get; private set; }
        public void OnClearCommand( object o)
        {
            this.RecievedData = "";
        }

        public RelayCommand SendCommand { get; private set; }
        public void OnSendCommand(object o)
        {
            this.Send( this.BufforData + this.Terminator );
        }
        #endregion


        private bool IsConnected  => this.Port.IsOpen;

        private void OnRecieved( object sender, SerialDataReceivedEventArgs e )
        {
            var data = this.Recieve();
            if( data.Equals(this.PingMessage) )
            {
                if( this.Stopwatch.IsRunning)
                {
                    this.Stopwatch.Stop();
                    MessageBox.Show( "Round Trip Delay: " + this.Stopwatch.Elapsed.TotalMilliseconds + " ms" );
                    this.Stopwatch.Reset();
                    return;
                }
                else
                {
                    this.Send( this.PingMessage + this.Terminator );
                    return;
                }
            }

            this.RecievedData += data;
        }

        private string TerminatorByType(StandardTerminator term)
        {
            switch ( term )
            {
                case StandardTerminator.CR: return "\r";
                case StandardTerminator.LF: return "\n";
                case StandardTerminator.CRLF: return "\n\r";
                default: return null;
            }
        }

        private void Send(string data)
        {
            try
            {
                this.Port.Write( data );
            }
            catch( TimeoutException )
            {
                MessageBox.Show( "Timeout przy operacji zapisu" );
            }
            catch ( Exception )
            {

            }
        }

        private string Recieve()
        {
            try
            {
                string data = string.IsNullOrEmpty( this.Terminator ) ? this.Port.ReadExisting() : this.Port.ReadTo( this.Terminator );
                return data;
            }
            catch ( TimeoutException )
            {
                MessageBox.Show( "Timeout przy operacji odczytu" );
            }
            catch( Exception )
            {

            }

            return null;
        }


    }
}
