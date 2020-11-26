using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NEWGameServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        //server needs to know which packet to call based on the packet id that it receives
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener; //this will be managing all udp communication for the server

        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            Console.Write($"Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            //pass it an async callback and null for the object state
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(Port); //klein geschrieben?
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.Write($"Server started on {Port}");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            //once a client connects we want to make sure to continue listenting for connections, so we call beginaccepttcpclient again
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");


            //setting an ID for the newly connected client
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    //then we connect
                    clients[i].tcp.Connect(client); //passing in the newly connected tcp client instance
                    return; //return to make  sure client takes only 1 open slot
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint); //this will not only return any bytes we receive, whcih we store in a byte array, it will also set our IPendpoint to the endpoint where the data came from
                //call beginrecieve again to dont miss any incoming data
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data)) //using the byte array we received
                {
                    //read out the client id
                    int clientID = packet.ReadInt();

                    //check if client id is equal to 0
                    if (clientID == 0) //should never be the case, but server would crash
                    {
                        return;
                    }

                    if (clients[clientID].udp.endPoint == null) //this means its a new connection and the packet we receive should be the empty one that opens up the clients port!
                    {
                        clients[clientID].udp.Connect(clientEndPoint);
                        return; //get out of method to prevent it to handle the data
                    }

                    //check if endpoint we stored for the client matches the endpoint where the packet came from, to prevent hackers 
                    if (clients[clientID].udp.endPoint.ToString() == clientEndPoint.ToString()) //tostring because else it would return false even if it matched
                    {
                        clients[clientID].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP Data: {ex}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                //populating clients dictionary, starting at 1
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.playerPosition, ServerHandle.PlayerPosition },
            };

            Console.WriteLine("Initialized packets.");

        }

    }
}
