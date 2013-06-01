using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace Socks5Client
{
    public class Socks5Client
    {
        private string _socksAddr;
        private int _socksPort;
        private string _destAddr;
        private int _destPort;
        private string _username;
        private string _password;
        private Socket _socket;
        private const int SOCKS_VER = 0x05;
        private const int AUTH_METH_SUPPORT = 0x02;
        private const int USER_PASS_AUTH = 0x02;
        private const int NOAUTH = 0x00;
        private const int CMD_CONNECT = 0x01;
        private const int SOCKS_ADDR_TYPE_IPV4 = 0x01;
        private const int SOCKS_ADDR_TYPE_IPV6 = 0x04;
        private const int SOCKS_ADDR_TYPE_DOMAIN_NAME = 0x03;
        private const int AUTH_METHOD_NOT_SUPPORTED = 0xff;
        private const int SOCKS_CMD_SUCCSESS = 0x00;

        private Socks5Client(string socksAddress, int socksPort, string destAddress, int destPort, string username, string password)
        {
            _socksAddr = socksAddress;
            _socksPort = socksPort;
            _destAddr = destAddress;
            _destPort = destPort;
            _username = username;
            _password = password;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public Socket Connect()
        {
            _socket.Connect(_socksAddr, _socksPort);
            byte[] buffer = new byte[4];
            buffer[0] = SOCKS_VER;
            buffer[1] = AUTH_METH_SUPPORT;
            buffer[2] = NOAUTH;
            buffer[3] = USER_PASS_AUTH;
            _socket.Send(buffer);
            _socket.Receive(buffer, 0, 2, SocketFlags.None);
            if (buffer[1] == AUTH_METHOD_NOT_SUPPORTED)
            {
                _socket.Close();
                throw new SocksAuthException();
            }

            if (buffer[1] == USER_PASS_AUTH && (_username == null || _password == null))
            {
                throw new ArgumentException("No username or password provided");
            }

            if (buffer[1] == USER_PASS_AUTH)
            {
                throw new NotImplementedException();
            }
            byte addrType = GetAddressType();
            byte[] address = GetDestAddressBytes(addrType, _destAddr);
            byte[] port = GetDestPortBytes(_destPort);
            buffer = new byte[4 + port.Length + address.Length];
            buffer[0] = SOCKS_VER;
            buffer[1] = CMD_CONNECT;
            buffer[2] = 0x00; //reserved
            buffer[3] = addrType;
            address.CopyTo(buffer, 4);
            port.CopyTo(buffer, 4 + address.Length);
            _socket.Send(buffer);
            buffer = new byte[255];
            _socket.Receive(buffer, buffer.Length, SocketFlags.None);

            if (buffer[1] == SOCKS_CMD_SUCCSESS)
                return _socket;
            throw new SocksRefuseException();
        }


        private byte GetAddressType()
        {
            IPAddress ipAddr;
            bool result = IPAddress.TryParse(_destAddr, out ipAddr);

            if (!result)
                return SOCKS_ADDR_TYPE_DOMAIN_NAME;

            switch (ipAddr.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return SOCKS_ADDR_TYPE_IPV4;
                case AddressFamily.InterNetworkV6:
                    return SOCKS_ADDR_TYPE_IPV6;
                default:
                    throw new BadDistanationAddrException();
            }

        }

        private byte[] GetDestAddressBytes(byte addressType, string host)
        {
            switch (addressType)
            {
                case SOCKS_ADDR_TYPE_IPV4:
                case SOCKS_ADDR_TYPE_IPV6:
                    return IPAddress.Parse(host).GetAddressBytes();
                case SOCKS_ADDR_TYPE_DOMAIN_NAME:
                    byte[] bytes = new byte[host.Length + 1];
                    bytes[0] = Convert.ToByte(host.Length);
                    Encoding.ASCII.GetBytes(host).CopyTo(bytes, 1);
                    return bytes;
                default:
                    return null;
            }
        }

        private byte[] GetDestPortBytes(int value)
        {
            byte[] array = new byte[2];
            array[0] = Convert.ToByte(value / 256);
            array[1] = Convert.ToByte(value % 256);
            return array;
        }

        public static Socket Connect(string socksAddress, int socksPort, string destAddress, int destPort, string username, string password)
        {
            Socks5Client client = new Socks5Client(socksAddress, socksPort, destAddress, destPort, username, password);
            return client.Connect();
        }
    }

    [Serializable]
    public class SocksAuthException : Exception
    {
        public SocksAuthException()
        {
        }

        public SocksAuthException(string message)
            : base(message)
        {
        }

        public SocksAuthException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SocksAuthException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class BadDistanationAddrException : Exception
    {
        public BadDistanationAddrException()
        {
        }

        public BadDistanationAddrException(string message)
            : base(message)
        {
        }

        public BadDistanationAddrException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BadDistanationAddrException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class SocksRefuseException : Exception
    {

        public SocksRefuseException()
        {
        }

        public SocksRefuseException(string message)
            : base(message)
        {
        }

        public SocksRefuseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SocksRefuseException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
