using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Common;

namespace Serial_Modbus
{
    public class SlaveViewModel : BaseViewModel
    {
        private SerialPort Port;
        private Stopwatch FrameStopwatch = new Stopwatch();

        public SlaveViewModel()
        {
            this.Port = new SerialPort();
            this.Port.DataReceived += this.OnRecieved;
            this.Port.DiscardNull = false;
            this.Port.DtrEnable = true;
            this.Port.NewLine = "\r\n";

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

            this.Adress = 1;
            this.FrameTimeout = 100;

            this.ConnectCommand = new RelayCommand( this.OnConnectCommand );
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

        private int _Adress;
        public int Adress
        {
            get => this._Adress; set
            {
                value = value < 1 ? 1 : value;
                value = value > 247 ? 247 : value;
                this._Adress = value;
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

        public string ConnectionStatus { get => this.IsConnected ? "Rozłącz" : "Połącz"; }
        public string SendBackText { get; set; }

        private string _RecievedData;
        public string RecievedData { get => this._RecievedData; set { this._RecievedData = value; this.OnPropertyChanged(); } }
        private string _RecievedDataHex;
        public string RecievedDataHex { get => this._RecievedDataHex; set { this._RecievedDataHex = value; this.OnPropertyChanged(); } }

        
        private string _SendData;
        public string SendData { get => this._SendData; set { this._SendData = value; this.OnPropertyChanged(); } }
        private string _SendDataHex;
        public string SendDataHex { get => this._SendDataHex; set { this._SendDataHex = value; this.OnPropertyChanged(); } }

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


        private bool SendReponse(int code)
        {
            switch ( code )
            {
                case 1: return false;
                case 2: return true;
            }

            return false;
        }

        private void OnRecieved( object sender, SerialDataReceivedEventArgs e )
        {
            this.FrameStopwatch.Restart();
            string data = this.Port.ReadLine() + this.Port.NewLine;
            this.FrameStopwatch.Stop();
            var elapsed = this.FrameStopwatch.ElapsedMilliseconds;
            var ellapsedPerFrame = elapsed / data.Length;
            if ( ellapsedPerFrame > this.FrameTimeout )
                return;

            var decoded = Converter.FromASCII( data, (byte)this.Adress );
            if( decoded == null)
                return;

            if(this.SendReponse(decoded.Item2))
            {
                this.SendData = Converter.ToASCII( (byte)decoded.Item2, (byte)this.Adress, this.SendBackText );
                this.SendDataHex = Converter.ToHex( this.SendData );
                this.RecievedData = "";
                this.RecievedDataHex = "";

                this.Port.Write( this.SendData );
                this.SendData = this.SendBackText;

            }
            else
            {
                this.RecievedDataHex = Converter.ToHex( data );
                this.RecievedData = decoded.Item1;

                this.SendData = "";
                this.SendDataHex = "";
            }
        }
    }
}
