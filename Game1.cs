using CTC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System;
using System.Net.Sockets;
using tibiamonoopengl.Protocol;
using System.Net;
using System.Linq;
using System.Diagnostics;
using tibiamonoopengl.Rsa;
using Org.BouncyCastle.Crypto;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace tibiamonoopengl
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager Graphics;
        private SpriteBatch _spriteBatch;
        private GameDesktop Desktop;
        private MouseState LastMouseState;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private RsaDecryptor rsaDecryptor;

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Graphics.PreparingDeviceSettings += PrepareDevice;
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 800;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            // Setup the window
            IsFixedTimeStep = false;
            Graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.One;
            //graphics.PreferWaitForVerticalTrace = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            Graphics.ApplyChanges();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            UIContext.Initialize(Window, Graphics, Content);
            UIContext.Load();


            rsaDecryptor = new RsaDecryptor("C:\\Users\\dennis\\source\\repos\\tibiamonoopengl\\Rsa\\key.pem");
            



            Desktop = new GameDesktop();
            Desktop.Load();
            Desktop.CreatePanels();
            Desktop.LayoutSubviews();
            Desktop.NeedsLayout = true;

            // Connect to a Tibia server
            string serverAddress = "127.0.0.1";
            int serverPort = 1300; // Replace with the actual server port
            // Log: Resolving DNS
            Debug.WriteLine($"Resolving DNS for server: {serverAddress}");

            // Resolve the hostname to an IP address (IPv4)
            IPHostEntry hostEntry = Dns.GetHostEntry(serverAddress);
            IPAddress ipAddress = hostEntry.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);



            ConnectToServer(serverAddress, serverPort);


        }


        //private SslStream sslStream; // Add this field to your class

        private async void ConnectToServer(string serverAddress, int port)
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

        //skip first, second \0
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


        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // For testing purposes, accept any certificate
            // In production, check sslPolicyErrors and validate the certificate
            return true;
        }



        private async void StartReceivingData(GameTime gameTime) // Pass GameTime from the game loop
        {
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


        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            StartReceivingData(gameTime);
            // Do input handling

            // First check left mouse button
            if (LastMouseState != null)
            {
                if (LastMouseState.LeftButton != mouse.LeftButton)
                    Desktop.MouseLeftClick(mouse);
            }

            // Send the mouse moved event
            if (LastMouseState.X != mouse.X || LastMouseState.Y != mouse.Y)
                Desktop.MouseMove(mouse);

            // Save the state for next frame so we can see what changed
            LastMouseState = mouse;

            // Update the game state
            Desktop.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            try
            {
                Desktop.Draw(null, Window.ClientBounds);
            }
            catch (Exception e)
            {
                throw;
            }

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            // Clean up network resources
            if (networkStream != null)
                networkStream.Close();
            if (tcpClient != null)
                tcpClient.Close();
        }



        protected void PrepareDevice(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }
}
