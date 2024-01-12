using CTC;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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



            
        public LoginWindow(Texture2D backgroundTexture, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            this.backgroundTexture = backgroundTexture;
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;
            // Initialize other UI elements like usernameField, passwordField, loginButton
            usernameField = new TextInputField(graphicsDevice, 520, 380,150,20);
            passwordField = new TextInputField(graphicsDevice, 520, 420,150,20);

            Rectangle ButtonSize = new Rectangle(520, 460, 100, 30);
            loginButton = new UIButton("LOGIN");
            loginButton.Bounds = ButtonSize;

            // Set up event for button click
            loginButton.ButtonPressed += OnLoginButtonClick;


        }

        public void Draw()
        {
            spriteBatch.Begin();
            Rectangle destinationRectangle = new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            spriteBatch.Draw(backgroundTexture, destinationRectangle, Color.White);

            // Assuming Draw method of TextInputField and UIButton requires a bounding box
            Rectangle boundingBox = new Rectangle(20, 20, 30, 10);
            usernameField.Draw(spriteBatch, UIContext.StandardFont); // Assuming Draw only needs spriteBatch
            passwordField.Draw(spriteBatch, UIContext.StandardFont);
            loginButton.Draw(spriteBatch, boundingBox);

            spriteBatch.End();
        }



        private void OnLoginButtonClick(UIButton button, MouseState mouse)
        {
            // Handle login logic here
        }
    }

}
