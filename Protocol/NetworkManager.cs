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
namespace tibiamonoopengl.Protocol
{
    public class NetworkManager
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        //private RsaDecryptor rsaDecryptor;
        public bool IsConnected { get; set; }
        public event EventHandler<bool> ConnectionStatusChanged;


        public NetworkManager()
        {

            //rsaDecryptor = new RsaDecryptor("path/to/key.pem");
        }



        public async Task ConnectToServerAsync(string serverAddress, int port, GameTime gameTime)
        {
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

                // Start receiving data
                await StartReceivingDataAsync(gameTime);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
                IsConnected = false;
                OnConnectionStatusChanged(false);
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
                            ProcessReceivedData(message.GetData(), message.GetSize());
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







        private void ProcessReceivedData(byte[] data, int bytesRead)
        {
            // Convert the received data to a readable format
            string receivedString = Encoding.UTF8.GetString(data, 0, bytesRead);

            // Log the received data
            Debug.WriteLine($"Received string: {receivedString}");

            // Split the received string into lines
            string[] playerLines = receivedString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string playerLine in playerLines)
            {
                if (!string.IsNullOrWhiteSpace(playerLine))
                {
                    // Process each line to extract player information
                    // Example format: "Player: John, Level: 5, Balance: 1000"
                    string[] playerDetails = playerLine.Split(',');
                    foreach (string detail in playerDetails)
                    {
                        Debug.WriteLine(detail.Trim()); // Trim to remove leading/trailing whitespaces
                    }
                }
            }

            // Further processing as needed
        }



        public async Task SendLoginRequestAsync(string serverAddress, int port, string username, string password, GameTime gameTime)
        {
            if (!IsConnected)
            {
                // If not connected, attempt to establish a new connection
                await ConnectToServerAsync(serverAddress, port, gameTime);
            }

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
