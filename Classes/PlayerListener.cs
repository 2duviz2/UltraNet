using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UltraNet.Classes
{
    public class PlayerListener : MonoBehaviour
    {
        private UdpClient udpClient;
        private Thread receiveThread;
        private string lastReceivedData = "";
        private readonly object lockObject = new object();

        public int port = 41789;

        public void Start()
        {
            return;
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void ReceiveData()
        {
            IPAddress serverIP = IPAddress.Parse("88.9.11.249");
            udpClient = new UdpClient(port); // Bind to local port
            IPEndPoint remoteEndPoint = new IPEndPoint(serverIP, 0);

            while (true)
            {
                try
                {
                    //if (SceneHelper.CurrentScene == "Main Menu" || NewMovement.Instance == null) continue;

                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string text = Encoding.UTF8.GetString(data);

                    // Use a lock to safely pass data to the main thread
                    lock (lockObject)
                    {
                        lastReceivedData = text;
                    }
                }
                catch (Exception e)
                {
                    Plugin.LogError(e.ToString());
                }
            }
        }

        public void Update()
        {
            return;
            // Process data on the main thread (Unity API calls only work here)
            lock (lockObject)
            {
                if (!string.IsNullOrEmpty(lastReceivedData))
                {
                    PlayerFetcher.instance.ParseJson(lastReceivedData);
                    lastReceivedData = ""; // Clear after processing
                }
            }
        }

        void OnDisable() => udpClient?.Close();
        void OnApplicationQuit() => udpClient?.Close();
    }
}