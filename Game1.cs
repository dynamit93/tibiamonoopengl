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
using ImGuiNET;
//using tibiamonoopengl.UI.Framework;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using tibiamonoopengl.UI.Framework;




namespace tibiamonoopengl
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager Graphics;
        private SpriteBatch spriteBatch;
        private GameDesktop Desktop;
        private MouseState LastMouseState;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private RsaDecryptor rsaDecryptor;
        private LoginWindow loginWindow;
        private NetworkManager networkManager;
        private Texture2D backgroundTexture;
        private TextInputField usernameField;
        private TextInputField passwordField;




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

            networkManager = new NetworkManager();
            Debug.WriteLine("NetworkManager initialized in Initialize");
            // Setup the window
            //loginWindow = new LoginWindow();

            IsFixedTimeStep = false;
            Graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.One;
            //graphics.PreferWaitForVerticalTrace = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            Graphics.ApplyChanges();
            base.Initialize();
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //loginWindow = new LoginWindow();
            backgroundTexture = Content.Load<Texture2D>("loginscreenbackground");
            //loginWindow = new LoginWindow(backgroundTexture);
            loginWindow = new LoginWindow(backgroundTexture, spriteBatch, GraphicsDevice);
            UIContext.Initialize(Window, Graphics, Content);
            UIContext.Load();


            rsaDecryptor = new RsaDecryptor("C:\\Users\\dennis\\source\\repos\\tibiamonoopengl\\Rsa\\key.pem");

            


            //loginWindow = new LoginWindow();
            Desktop = new GameDesktop();
            //Desktop.AddSubview(loginWindow);
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



            if (networkManager == null)
            {
                Debug.WriteLine("NetworkManager is null in LoadContent");
            }
            else
            {
                Debug.WriteLine("NetworkManager is not null in LoadContent");
                networkManager.ConnectToServer(serverAddress, serverPort);
            }



        }





        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            

            networkManager.StartReceivingData(gameTime);


            //loginWindow.Update(gameTime);

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
            // Draw login window
            loginWindow.Draw();


 

            //// add game screen in login window
            //try
            //{
            //    Desktop.Draw(null, Window.ClientBounds);
            //}
            //catch (Exception e)
            //{
            //    throw;
            //}


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
