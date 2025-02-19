﻿using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TestVSCode
{
    class Program
    {

        static string infoMessage = "infoMessage";
        static string wariningMessage = "wariningMessage";
        static string urgentMessage = "urgentMessage";
        static Dictionary<Socket, List<MessageType>> clientSubscriptions = new Dictionary<Socket, List<MessageType>>();
        enum MessageType 
        {
            Information,
            Warning,
            Urgent
        }
        static List<Socket> clients = new List<Socket>();
        static int interval = 5000;
        static void Main(string[] args)
        {
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 4567);
            listenerSocket.Bind(localEndPoint);
            Console.WriteLine("server started");
            Thread clienListenerThread = new Thread (() =>
            {
                while (true)
                {
                    Socket clientSocket = listenerSocket.Accept();
                    List<MessageType> subscription = new List<MessageType> { MessageType.Information, MessageType.Warning};
                    SubscribeClient(clientSocket, subscription);
                    Console.WriteLine("client subscribed");
                    SendMessageToClient(clientSocket, infoMessage);
                    SendMessageToClient(clientSocket, wariningMessage);
                    SendUrgentMessage(urgentMessage);
                }
            });
            clienListenerThread.Start();
            while (true)
            {
                Console.WriteLine("clientSocket to remove or exit");
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }
                if (RemoveClintFromSubscription(input))
                {
                    Console.WriteLine("client removed");
                }
                else{
                    Console.WriteLine("client not found");
                }

            }
        
        }
        static bool RemoveClintFromSubscription(string clientAddress)
        {
            foreach (var client in clientSubscriptions.Keys)
            {
                IPEndPoint clientEndPoint = (IPEndPoint)client.RemoteEndPoint;
                if (clientEndPoint.Address.ToString() == clientAddress)
                {
                    clientSubscriptions.Remove(client);
                    client.Close();
                    return true;
                }
            }
            return false;
        }
        
        static void SendMessageToClient(Socket clientSocket, string message)
        {
            List<MessageType> subsctiptions = clientSubscriptions[clientSocket];
            if (message == "urgentMessage" || subsctiptions.Contains(MessageType.Information))
            {
                byte[] messageBytes = Encoding.Default.GetBytes(message);
                clientSocket.Send(messageBytes);
                Console.WriteLine($"sent message {message} to client");
            }
        }

        static void SubscribeClient(Socket clientSocket, List<MessageType> subsctiptionTypes)
        {
            clientSubscriptions[clientSocket] = subsctiptionTypes;
        }

        static void SendUrgentMessage(string urgentMessage)
        {
            byte[] messageBytes = Encoding.Default.GetBytes(urgentMessage);

            foreach (var clint in clientSubscriptions.Keys)
            {
                clint.Send(messageBytes);
            }
            Console.WriteLine("Sent urgent message for all");
        }
            
    }
}
