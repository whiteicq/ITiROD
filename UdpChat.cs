using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class UdpChat
    {
        private string _localAddress;
        private string _username;
        private int _localPort;
        private int _remotePort;
        private Socket _sender;
        private Socket _receiver;
        private List<string> _logs;

        public UdpChat(string username, int localPort, int remotePort, string localAddress = "127.0.0.1")
        {
            _username = username;
            _localAddress = localAddress;
            _localPort = localPort;
            _remotePort = remotePort;
            _logs = new List<string>();
        }

        public async Task ReceiveMessangeAsync() // мб добавить проверку на то, есть ли подключение, // а то мб в этом и проблема
        {
            byte[] data = new byte[65535];
            using (_receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                _receiver.Bind(new IPEndPoint(IPAddress.Parse(_localAddress), _localPort));
                while (true)
                {
                    var result = await _receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
                    var message = Encoding.UTF8.GetString(data, 0, result.ReceivedBytes);
                    Console.WriteLine(message);
                    _logs.Add(message);
                }
            }
        }
        public async Task SendMessageAsync()
        {
            using (_sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                Console.WriteLine("Введите сообщение или нажмите Enter для выхода:");

                while (true)
                {
                    string? message = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Exit();
                    }
                    message = $"{_username}: {message}";
                    var data = Encoding.UTF8.GetBytes(message);
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        await _sender.SendToAsync(data, new IPEndPoint(IPAddress.Parse(_localAddress), _remotePort));
                        _logs.Add(message);
                    }
                    else continue;
                }
            }
        }

        public void Exit()
        {
            Console.Clear();
            _sender.Close();
            string? req = Console.ReadLine();
            if (req == "reconnect")
            {
                _sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _sender.Connect(new IPEndPoint(IPAddress.Parse(_localAddress), _remotePort));
                foreach (var i in _logs)
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
}
