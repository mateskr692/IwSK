using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public class Converter
    {
        public static string ToASCII(byte functionCode, byte destinationAdress, string data)
        {
            byte lrc = 0;
            string message = ":";

            lrc += destinationAdress;
            message += string.Format( "{0:X2}", destinationAdress );

            lrc += functionCode;
            message += string.Format( "{0:X2}", functionCode );

            if ( !string.IsNullOrEmpty(data))
            {
                foreach ( var c in data )
                {
                    byte b = (byte)c;
                    lrc += b;
                    message += string.Format( "{0:X2}", b );
                }
            }

            lrc = (byte)((lrc ^ 0xFF) + 1);
            message += string.Format( "{0:X2}", lrc );
            message += "\r\n";

            return message;
        }

        public static Tuple<string,int> FromASCII(string message, byte stationAdress )
        {
            if(string.IsNullOrEmpty(message) || message[0] != ':')
                return null;

            byte adress = byte.Parse( message.Substring( 1, 2 ), NumberStyles.HexNumber );
            if ( stationAdress > 0 && (adress != 0 && adress != stationAdress) )
                return null;

            byte code = byte.Parse( message.Substring( 3, 2 ), NumberStyles.HexNumber );
            byte lrc = byte.Parse( message.Substring( message.Length - 4, 2 ), NumberStyles.HexNumber );
            string data = "";

            if(message.Length > 9)
            {
                var hexdata = message.Substring( 5, message.Length - 9 );
                for ( int i = 0; i < hexdata.Length; i += 2 )
                    data += Convert.ToChar( byte.Parse( hexdata.Substring( i, 2 ), NumberStyles.HexNumber ) );
            }

            return new Tuple<string, int>( data, code );
        }

        public static string ToHex(string s)
        {
            var hex = "";
            foreach ( char c in s )
            {
                hex += string.Format( "{0:X2}", (byte)c );
            }

            return Regex.Replace( hex, ".{2}", "$0 " );
        }
    }
}
