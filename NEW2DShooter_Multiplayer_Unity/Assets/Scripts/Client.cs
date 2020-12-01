using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

/// <summary>
/// Client network code
/// Creates the TCP and UDP classes with all methods like sending data, receiving data, handling partial packets and disconnecting
/// Also fills the Dictionary "packetHandlers" by passing the enum and the associated method, this needs to be manually filled everytime we make a new method which should get sent to the server
/// </summary>
public class Client : MonoBehaviour
{
    public static Client instance; //singelton instance
    public static int dataBufferSize = 4096;

    //IP and Port where the client will connect to
    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myID = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Destroy(this);
        }
    }

    public void Start()
    {
        tcp = new TCP();
        udp = new UDP();

    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        //same as in server tcp setup
        public TcpClient socket; 

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient //initilaizing the tcp client buffer sizes
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket); //starts connecting
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);
            //checking if we are in fact connected
            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            //same as in server!
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {

                Debug.Log($"Error sending data to server via TCP: {ex}");
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
                    instance.Disconnect();
                    return;
                }
                //if we have received data, we create new array with the length of bytelength
                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength); //copying the received bytes into the new array

                receivedData.Reset(HandleData(data));
                ///<summary> Partial packets, resetting received bytes explanation
                ///takes in the bool returned by HandleData method
                ///wether or not the receivedData packet gets reset, depends on the value returned by HandleData
                ///our server and client are communicating through TCP, this protocol is stream based!
                ///meaning it sends an continues stream of information, assuring that all packets sending are delivered and in correct order
                ///while the chunks of data being sent over are garanteed to arrive, they aren't garanteed to be delivered in one piece!
                ///when we send a packet it will be added to a larger list of bytes, once enough bytes are accumalated, they are sent in 1 bigger delivery
                ///TCP leaves it up to us handling cases where a packet is split between 2 deliveries, which why we dont always reset received bytes
                ///there could still be a piece of a packet in there that we havent handled yet, because the rest hasnt arrived
                ///if we would reset received bytes then, we would throw away data, which result in us throwing away packets
                ///</summary>


                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            }
            catch (Exception ex)
            {
                Disconnect();
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
                        packetHandlers[packetID](packet); //invoke it by passing it the packet instance
                    }
                });

                //after that reset packetlength to 0
                packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    //so we store that length in packet length, if the packet length is less than 1 we return true, because we want to reset receivedData
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

        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int localPort) //the port on which the client is communicating, this is differnet from the servers port number!
        {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            //creating packet and immediatly sending it
            //purpose of this packet is to initiate the connection with the server and open up the local port so that the client can receive messages!
            //since the SendData method writes the clients ID to the packet we dont need to do it manually
            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                //Inserting client id into the packet, because we use this value on the server to determine who sent it
                //because of the way udp works, we cant give every client an own udp instance on the server, because of issues with ports being closed
                //typically only 1 udp client is used on the server, so all udp communication is handled by a single udp client instance! 
                packet.InsertInt(instance.myID);
                if (socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null); //same as tcp / server etc.
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via UDP: {ex}");
                
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4) //making sure an actual packet is to handle
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception)
            {
                Disconnect();   
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet packet = new Packet(data)) //creating new packet and passing it the bytes we received
            {
                //read the specified amount of bytes back into the data variable
                //this just removes the first 4 bytes from the array which represent the length of the packet
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetID = packet.ReadInt();
                    packetHandlers[packetID](packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }

    //filling in the dictionary by passing the num and the associated ClientHandle function
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome },
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            {(int)ServerPackets.spawnProjectile, ClientHandle.SpawnProjectile },
            //{(int)ServerPackets.projectilePosition, ClientHandle.ProjectilePosition }
        };
        Debug.Log("Initialized packets");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from Server");
        }
    }

}
