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

        public NetworkManager()
        {
            //rsaDecryptor = new RsaDecryptor("path/to/key.pem");
        }

        public async void ConnectToServer(string serverAddress, int port)
        {
            tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(serverAddress, port);
                networkStream = tcpClient.GetStream();
                //sslStream = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

                // Authenticate as the client
                //sslStream.AuthenticateAsClient("ServerName");

                // Now use sslStream for all subsequent read/write operations
                // Example: Sending initial message
                string authToken = "ExpectedAuthToken";
                byte[] authBytes = Encoding.UTF8.GetBytes(authToken);
                await networkStream.WriteAsync(authBytes, 0, authBytes.Length);


                byte[] initialMessage = PrepareInitialMessage();
                //await sslStream.WriteAsync(initialMessage, 0, initialMessage.Length);
                await networkStream.WriteAsync(initialMessage, 0, initialMessage.Length);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        private byte[] PrepareInitialMessage()
        {
            // Assuming msg.buffer is a byte array
            byte[] msgBuffer = new byte[] { 0x00, 0x00 }; // Example byte array with values 10 and 27

            // Combine the two bytes into a ushort
            // The first byte (msgBuffer[0]) is the high-order byte, and the second byte (msgBuffer[1]) is the low-order byte
            ushort protocolVersion = (ushort)((msgBuffer[0] << 8) | msgBuffer[1]);

            // Convert ushort to byte array
            byte[] message = BitConverter.GetBytes(protocolVersion);

            // If BitConverter.IsLittleEndian is true, reverse the array to get Big Endian format
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(message);
            }

            return message;
        }

        public async void StartReceivingData(GameTime gameTime) // Pass GameTime from the game loop
        {

            if (tcpClient == null || !tcpClient.Connected)
            {
                Debug.WriteLine("Cannot start receiving data: tcpClient is not connected.");
                return;
            }


            try
            {
                TibiaNetworkStream tibiaNetworkStream = new TibiaNetworkStream(networkStream);

                while (tcpClient.Connected)
                {
                    NetworkMessage message = tibiaNetworkStream.Read(gameTime); // Pass the gameTime instance
                    if (message != null)
                    {
                        // Process received data
                        ProcessReceivedData(message.GetData(), message.GetSize());
                    }
                    Debug.WriteLine($"Received data: {message?.GetSize() ?? 0}");
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error receiving data: {ex.Message}");
            }
        }



        private void ProcessReceivedData(byte[] data, int bytesRead)
        {
            // Convert the received data to a readable format
            // If the data is expected to be a UTF-8 string:
            string receivedString = Encoding.UTF8.GetString(data, 0, bytesRead);

            // Log the received data
            Debug.WriteLine($"Received string: {receivedString}");

            // If you have decryption and further processing:
            // byte[] decryptedData = rsaDecryptor.Decrypt(data, bytesRead);
            // ParseProtocolData(decryptedData);
        }


        public async Task SendLoginRequest(string username, string password)
        {
            if (tcpClient == null || !tcpClient.Connected)
            {
                throw new InvalidOperationException("Not connected to server.");
            }

            // Example: creating a simple message string
            string message = $"LOGIN {username} {password}";

            // Convert the string message to a byte array
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Send the message
            await networkStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }


        public void Cleanup()
        {
            if (networkStream != null)
                networkStream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }

        // Other methods like ProcessReceivedData, etc.
    }
}
