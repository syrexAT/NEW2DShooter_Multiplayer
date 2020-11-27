using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace NEWGameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;
        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;

        public Client(int _clientID)
        {
            id = _clientID;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            //this will store the instance we get form the servers connect callback
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;


            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket; //assign the tcpclient thats passed in, to the socket field
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();

                receiveBuffer = new byte[dataBufferSize];

                //byte array that is the location in memeory to store data, read from the stream
                //the location in buffer to begin storing data --> 0
                // the number of bytes to read from the stream
                //async callback that gets executed when beginstream completes
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    //before sending we check that socket field has a value assigned
                    if (socket != null)
                    {
                        //passing it the byte array returned by packet.ToArray
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    //in order to receive data, we call endread, which returns an int representing the number of bytes we read from the stream, we store this in byteLength
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }
                    //if we have received data, we create new array with the length of bytelength
                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength); //copying the received bytes into the new array
                    receivedData.Reset(HandleData(data));

                    //TODO: handle data
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                }
                catch (Exception ex)
                {
                    Console.Write($"Error receiving TCP Data: {ex}");
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                receivedData.SetBytes(data); //setting receivedData's bytes to the bytes we just read from the stream
                                             //checking if receivedData contains more than 4 unread bytes!, if it does we have the start of one of our packet, because int exists of 4 bytes
                                             //and the first data of every packet we send, is an int representing the length of the packet

                if (receivedData.UnreadLength() >= 4)
                {
                    //so we store that length in packet length, if the packet length is less then 1 we return true, because we want to reset receivedData
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                //as long as this is running, it means that receivedData contains another complete packet which we can handle
                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    //we read that packet bytes in a new byte array
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);

                    //the code here wont necesserialy be run on the same thread
                    //we are calling the threadmanagers executeonmainthread function
                    //inside we create a new packet and read out its ID, 
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetID = packet.ReadInt();
                            Server.packetHandlers[packetID](id, packet); //invoke it by passing it the packet instance
                        }
                    });

                    //after that reset packetlength to 0
                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        //so we store that length in packet length, if the packet length is less then 1 we return true, because we want to reset receivedData
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                //if it is greater than 1, we dont want to reset data because theres a partial packet left in there
                return false;

            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        //pretty similar to the clients version
        public class UDP
        {
            public IPEndPoint endPoint;
            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt(); //store packet length in a local variable
                byte[] packetBytes = packetData.ReadBytes(packetLength); //read out number of bytes specified by packet length into a byte array

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetID = packet.ReadInt();
                        Server.packetHandlers[packetID](id, packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public void SendIntoGame(string playerName)
        {
            player = new Player(id, playerName, new Vector2(0, 0));

            //looping through clients dictionary
            foreach (Client client in Server.clients.Values)
            {
                //using this to send the information of all other players that are already connected to our new player
                if (client.player != null)
                {
                    if (client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, client.player);
                    }
                }
            }

            //this will send the new players information to all other players as well as to himself
            foreach (Client client in Server.clients.Values)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.id, player);
                }
            }
        }

        private void Disconnect()
        {
            Console.WriteLine(tcp.socket.Client.RemoteEndPoint + " has disconnected.");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
