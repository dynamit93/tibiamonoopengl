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
            //Content.RootDirectory = "Content";
            //IsMouseVisible = true;
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
            string serverAddress = "192.168.1.107";
            int serverPort = 7171; // Replace with the actual server port

            // Log: Resolving DNS
            Debug.WriteLine($"Resolving DNS for server: {serverAddress}");

            // Resolve the hostname to an IP address (IPv4)
            IPHostEntry hostEntry = Dns.GetHostEntry(serverAddress);
            IPAddress ipAddress = hostEntry.AddressList.FirstOrDefault(addr => addr.AddressFamily == AddressFamily.InterNetwork);



            ConnectToServer(serverAddress, 7171);


        }



        private async void ConnectToServer(string serverAddress, int port)
        {
            tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(serverAddress, port);
                networkStream = tcpClient.GetStream();

                // Prepare the initial message
                byte[] initialMessage = PrepareInitialMessage();

                // Send the initial message
                await networkStream.WriteAsync(initialMessage, 0, initialMessage.Length);

                // Start listening to the server
                StartReceivingData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        private byte[] PrepareInitialMessage()
        {
            // Example: Let's say the initial message is just the protocol version (2 bytes)
            ushort protocolVersion = 1100; // Example protocol version
            byte[] message = BitConverter.GetBytes(protocolVersion);

            // If BitConverter.IsLittleEndian is true, reverse the array to get Big Endian format
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(message);
            }

            return message;
        }




        private async void StartReceivingData()
        {
            try
            {
                byte[] buffer = new byte[1024]; // Adjust buffer size as needed

                while (tcpClient.Connected)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        // Process received data
                        ProcessReceivedData(buffer, bytesRead);
                    }
                    Debug.WriteLine($"Received data: {bytesRead}");
                }
            }
            catch (Exception ex)
            {
                // Handle data receiving errors
                Debug.WriteLine($"Error receiving data: {ex.Message}");
            }
        }

        private void ProcessReceivedData(byte[] data, int bytesRead)
        {
            // Convert the received data to a readable format
            // If the data is expected to be a UTF-8 string:
            string receivedString = Encoding.UTF8.GetString(data, 0, bytesRead);

            // Log the received data
            Debug.WriteLine($"Received data: {receivedString}");

            // If you have decryption and further processing:
            // byte[] decryptedData = rsaDecryptor.Decrypt(data, bytesRead);
            // ParseProtocolData(decryptedData);
        }


        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

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
