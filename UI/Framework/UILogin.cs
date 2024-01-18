using CTC;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using tibiamonoopengl.Protocol;
using System.Threading.Tasks;
using System.Net.Sockets;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace tibiamonoopengl.UI.Framework
{
    public class LoginWindow 
    {
        private Texture2D backgroundTexture;
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private TextInputField usernameField;
        private TextInputField passwordField;
        private UIButton loginButton;
        private Texture2D whitePixel;
        private string serverAddress;
        private int serverPort;
        private NetworkManager networkManager;
        private GameTime currentGameTime;
        private bool showLoginUI = true;



        public LoginWindow(Texture2D backgroundTexture, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, string serverAddress, int serverPort, ClientViewport clientViewport)
        {
            this.backgroundTexture = backgroundTexture;
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;

            // Create a 1x1 white texture
            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });


            // Initialize other UI elements like usernameField, passwordField, loginButton
            usernameField = new TextInputField(graphicsDevice, 520, 380,150,20);
            passwordField = new TextInputField(graphicsDevice, 520, 430,150,20);
           
            Rectangle ButtonSize = new Rectangle(520, 460, 100, 30); // Ensure these values are correct
            loginButton = new UIButton("LOGIN");
            loginButton.Bounds = ButtonSize;

            loginButton.Bounds = ButtonSize;

            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            networkManager = new NetworkManager(clientViewport,this);

            // Subscribe to the ConnectionStatusChanged event of NetworkManager
            //networkManager.ConnectionStatusChanged += NetworkManager_ConnectionStatusChanged;

            // Set up event for button click
            loginButton.ButtonPressed += async (sender, args) => await OnLoginButtonClick();
        }


        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            currentGameTime = gameTime;
            if (showLoginUI)
            {
                // Check if the user clicked on the usernameField
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (usernameField.Bounds.Contains(mouseState.Position))
                    {
                        usernameField.SetFocus(true);
                        passwordField.SetFocus(false);
                    }
                    else if (passwordField.Bounds.Contains(mouseState.Position))
                    {
                        passwordField.SetFocus(true);
                        usernameField.SetFocus(false);
                    }
                    else
                    {
                        usernameField.SetFocus(false);
                        passwordField.SetFocus(false);
                    }
                }
            }

            // Update text fields
            KeyboardState keyboardState = Keyboard.GetState();
            usernameField.Update(gameTime, keyboardState);
            passwordField.Update(gameTime, keyboardState);
        }


        public void Draw()
        {
            spriteBatch.Begin();

            // Draw background
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            if (showLoginUI)
            {
                // Draw the label for the username field using the bold font
                string usernameLabel = "Username:";
                string PasswordLabel = "Password:";
                Vector2 labelPosition = new Vector2(usernameField.Bounds.X, usernameField.Bounds.Y - 30); // Position above the field
                spriteBatch.DrawString(UIContext.BoldFont, usernameLabel, labelPosition, Color.LightGray);

                Vector2 PasswordLabelPosition = new Vector2(passwordField.Bounds.X, passwordField.Bounds.Y - 30); // Position above the field
                spriteBatch.DrawString(UIContext.BoldFont, PasswordLabel, PasswordLabelPosition, Color.LightGray);

                // Draw text fields
                usernameField.Draw(spriteBatch, UIContext.StandardFont);
                passwordField.Draw(spriteBatch, UIContext.StandardFont);

                // Draw custom button
                Rectangle buttonBounds = new Rectangle(520, 460, 100, 30); // Button position and size

                // Check if the mouse is inside the button bounds
                MouseState mouseState = Mouse.GetState();
                bool isMouseOverButton = buttonBounds.Contains(mouseState.Position);

                // Change the button color based on mouse hover
                Color buttonColor = isMouseOverButton ? Color.LightGreen : Color.Green;

                spriteBatch.Draw(whitePixel, buttonBounds, buttonColor); // Draw button background

                string buttonText = "Login";
                Vector2 buttonTextSize = UIContext.StandardFont.MeasureString(buttonText);
                Vector2 buttonTextPosition = new Vector2(
                    buttonBounds.X + (buttonBounds.Width - buttonTextSize.X) / 2,
                    buttonBounds.Y + (buttonBounds.Height - buttonTextSize.Y) / 2
                );
                spriteBatch.DrawString(UIContext.StandardFont, buttonText, buttonTextPosition, Color.White); // Draw button text

                //// Check for a mouse click on the button
                if (isMouseOverButton && mouseState.LeftButton == ButtonState.Pressed)
                {
                    // Trigger the login action here
                    OnLoginButtonClick();
                }
            }
            spriteBatch.End();
        }



        // Event handler for ConnectionStatusChanged event
        private void NetworkManager_ConnectionStatusChanged(object sender, bool isConnected)
        {
            if (isConnected)
            {
                // Handle the case when the network is connected
                // You can update UI or take other actions here
            }
            else
            {
                // Handle the case when the network is not connected
                // You can update UI or take other actions here
            }
        }

        private bool isAttemptingLogin = false;

        private async Task OnLoginButtonClick()
        {
            if (isAttemptingLogin) return;

            isAttemptingLogin = true;
            string username = usernameField.Text;
            string password = passwordField.Text;

            try
            {
                if (!networkManager.IsConnected)
                {
                    Debug.WriteLine("00000000");

                    await networkManager.ConnectToServerAsync(serverAddress, serverPort, currentGameTime);
                }
                Debug.WriteLine("1111111111111111");

                if (networkManager.IsConnected)
                {
                    Debug.WriteLine("2222222222222");
                    await networkManager.SendLoginRequestAsync(serverAddress, serverPort, username, password, currentGameTime);
                    Debug.WriteLine("Login request sent.");
                }
                else
                {
                    Debug.WriteLine("Failed to connect to the server.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during login: {ex.Message}");
            }
            finally
            {
                isAttemptingLogin = false;
            }
        }



        public void UpdateUIAfterLogin ()
        {
            showLoginUI = false;
        }




    }

}
