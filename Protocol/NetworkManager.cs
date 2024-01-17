using System;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using tibiamonoopengl.Protocol;
using tibiamonoopengl.Rsa;
using Microsoft.Xna.Framework;
using CTC;
using tibiamonoopengl.UI.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
namespace tibiamonoopengl.Protocol
{
    public class NetworkManager
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        //private RsaDecryptor rsaDecryptor;
        public bool IsConnected { get; set; }
        public event EventHandler<bool> ConnectionStatusChanged;
        private LoginWindow loginWindow;
        private ClientViewport clientViewport;

        public NetworkManager(ClientViewport viewport, LoginWindow loginWindow)
        {
            this.clientViewport = viewport;
            this.loginWindow = loginWindow;

            //rsaDecryptor = new RsaDecryptor("path/to/key.pem");
        }

        private bool isConnecting = false;

        public async Task ConnectToServerAsync(string serverAddress, int port, GameTime gameTime)
        {
            if (isConnecting) return;

            isConnecting = true;
            tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(serverAddress, port);
                    networkStream = tcpClient.GetStream();
                    IsConnected = true;
                    OnConnectionStatusChanged(true);

                    // Send the initial authentication token or any other setup data
                    string authToken = "ExpectedAuthToken";
                    byte[] authBytes = Encoding.UTF8.GetBytes(authToken);
                    await networkStream.WriteAsync(authBytes, 0, authBytes.Length);

                // Start receiving data in a separate task
                await StartReceivingDataAsync(gameTime);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
                IsConnected = false;
                OnConnectionStatusChanged(false);
            }
            finally
            {
                isConnecting = false;
            }
        }

        public async Task StartReceivingDataAsync(GameTime gameTime)
        {
            if (!IsConnected)
            {
                Debug.WriteLine("Cannot start receiving data: not connected to the server.");
                return;
            }

            TibiaNetworkStream tibiaNetworkStream = new TibiaNetworkStream(networkStream);

            while (IsConnected)
            {
                try
                {
                    if (networkStream.DataAvailable)
                    {
                        NetworkMessage message = tibiaNetworkStream.Read(gameTime);
                        if (message != null)
                        {
                            ReceiveDataFromServer(message.GetData(), message.GetSize());
                        }
                    }
                    else
                    {
                        Debug.WriteLine("No data available to read.");
                    }
                    await Task.Delay(100); // Reduced delay
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in data receiving loop: {ex.Message}");
                    if (ex.Message.Contains("Connection closed"))
                    {
                        Debug.WriteLine("Connection to server lost.");
                        IsConnected = false;
                        OnConnectionStatusChanged(false);
                        break;
                    }
                }
            }
        }







        //private void ProcessReceivedData(byte[] data, int bytesRead)
        //{
        //    string receivedString = Encoding.UTF8.GetString(data, 0, bytesRead);
        //    Debug.WriteLine($"Received data: {receivedString}");

        //    try
        //    {
        //        // Assuming the JSON structure is {"player": { ... }}
        //        var container = JsonConvert.DeserializeObject<Dictionary<string, ClientPlayer>>(receivedString);
        //        ClientPlayer player = container["player"];
        //        // Process the player data
        //    }
        //    catch (JsonException ex)
        //    {
        //        Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
        //    }
        //}

        // Example client-side method to receive data
        private void ReceiveDataFromServer(byte[] data, int size)
        {
            // Convert the received byte array to a string
            string receivedJson = Encoding.UTF8.GetString(data, 0, size);
            receivedJson = receivedJson.TrimStart('\0');
            Debug.WriteLine($"Received JSON: {receivedJson}");

            try
            {
                // Deserialize the JSON string to the appropriate object
                var container = JsonConvert.DeserializeObject<Dictionary<string, ClientPlayer>>(receivedJson);
                if (container != null && container.ContainsKey("player"))
                {
                    ClientPlayer player = container["player"];
                    // Process the player data
                    // Here you can update the UI or game state based on the received player data
                    // ...

                    UpdatePlayerState(player);
                }
                else
                {
                    Debug.WriteLine("Deserialization returned null or missing 'player' key.");
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Deserialization error: {ex.Message}");
            }
        }


        private void UpdatePlayerState(ClientPlayer player)
        {
            // Assuming 'clientViewport' is an instance of ClientViewport
            // and it's accessible in this context

            // Update the player's state in the viewport
            clientViewport.Player = player;

            //// Update other game states or UI elements as needed
            //RefreshUI();
            //UpdateMap();
            //// ... other update methods ...

            //// Trigger any events or notifications necessary
            //OnPlayerStateUpdated(); // This is a hypothetical event handler
        }


        public void SetClientViewport(ClientViewport viewport)
        {
            this.clientViewport = viewport;
        }

        public async Task SendLoginRequestAsync(string serverAddress, int port, string username, string password, GameTime gameTime)
        {
            //if (!IsConnected)
            //{
            //    // If not connected, attempt to establish a new connection
            //    await ConnectToServerAsync(serverAddress, port, gameTime);
            //}

            if (IsConnected)
            {
                // Connected or reconnected successfully, proceed with login request
                string message = $"LOGIN {username} {password}";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            else
            {
                throw new InvalidOperationException("Not connected to the server.");
            }
        }



        public void Cleanup()
        {
            if (networkStream != null)
                networkStream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }

        protected virtual void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }
        // Other methods like ProcessReceivedData, etc.
    }
}
