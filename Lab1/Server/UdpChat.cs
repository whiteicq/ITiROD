using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Client
{
    internal class UdpChat
    {
        private string _username;
        private string _localAddress;
        private int _localPort;
        private int _remotePort;
        private Socket _sender;
        private Socket _receiver;
        private List<string> messages = new List<string>();
        private XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
        public UdpChat(string username, int localPort, int remotePort, string localAddress = "127.0.0.1")
        {
            _username = username;
            _localAddress = localAddress;
            _localPort = localPort;
            _remotePort = remotePort;
        }

        public async Task ReceiveMessangeAsync()
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
                    messages.Add(message);
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
                        using FileStream fs = new FileStream($"{_username}_s logs.xml", FileMode.OpenOrCreate);
                        serializer.Serialize(fs, message);
                        Console.WriteLine("Соединение потеряно. История сообщений сохранена");
                        break;
                    }

                    message = $"{_username}: {message}";
                    var data = Encoding.UTF8.GetBytes(message);
                    await _sender.SendToAsync(data, new IPEndPoint(IPAddress.Parse(_localAddress), _remotePort));
                    messages.Add(message);
                }
            }
        }

        public void TryLoadHistory()
        {
            if (!File.Exists($"{_username}_s logs.xml"))
            {
                return;
            }
            using FileStream fs = new FileStream($"{_username}_s logs.xml", FileMode.Open);
            List<string>? messages = serializer.Deserialize(fs) as List<string>;
            Console.WriteLine("Сообщения с последней сессии:");
            foreach (string mes in messages)
            {
                Console.WriteLine(mes);
            }
        }

        public void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            using FileStream fs = new FileStream($"{_username}_s logs.xml", FileMode.OpenOrCreate);
            serializer.Serialize(fs, messages);
        }
    }
}
