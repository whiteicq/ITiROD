using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите имя:");
            string? name = Console.ReadLine();
            Console.WriteLine("Введите порт для отправки сообщений:");
            if (!int.TryParse(Console.ReadLine(), out int remotePort)) return;
            Console.WriteLine("Введите порт для получения сообщений:");
            if (!int.TryParse(Console.ReadLine(), out int localPort)) return;
            Console.WriteLine();

            UdpChat chat = new UdpChat(name, localPort, remotePort);
            chat.TryLoadHistory();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(chat.CurrentDomain_ProcessExit);
            Task.Run(chat.ReceiveMessangeAsync);
            await chat.SendMessageAsync();

        }
    }
}