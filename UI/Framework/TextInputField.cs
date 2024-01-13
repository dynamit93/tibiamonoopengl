using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tibiamonoopengl.UI.Framework
{
    public class TextInputField
    {
        private StringBuilder text = new StringBuilder();
        private Rectangle bounds;
        private bool isSelected;
        private KeyboardState oldState;
        private Texture2D whitePixel;
        private bool cursorVisible;
        private double cursorBlinkTime;
        private const double BlinkInterval = 0.5;


        public string Text => text.ToString();
        public Rectangle Bounds { get => bounds; set => bounds = value; }

        public TextInputField(GraphicsDevice graphicsDevice, int x, int y, int width, int height)
        {
            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });
            bounds = new Rectangle(x, y, width, height);
            oldState = Keyboard.GetState();
        }

        public void SetFocus(bool focus)
        {
            isSelected = focus;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            var newState = keyboardState;


            if (isSelected)
            {
                foreach (var key in newState.GetPressedKeys())
                {
                    if (!oldState.IsKeyDown(key))
                    {
                        if (key == Keys.Back && text.Length > 0) // Handle backspace
                        {
                            text.Remove(text.Length - 1, 1);
                        }
                        else
                        {
                            char keyChar = GetCharFromKey(key);
                            if (keyChar != '\0')
                            {
                                text.Append(keyChar);
                            }
                        }
                    }
                }
            }
            if (isSelected)
            {
                cursorBlinkTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (cursorBlinkTime >= BlinkInterval)
                {
                    cursorVisible = !cursorVisible;
                    cursorBlinkTime = 0;
                }
            }
            else
            {
                cursorVisible = false;
                cursorBlinkTime = 0;
            }

            oldState = newState;
        }

        private char GetCharFromKey(Keys key)
        {
            // This is a basic implementation and might not handle all cases
            // You might need to expand this method to handle other keys
            if (key >= Keys.A && key <= Keys.Z)
            {
                return (char)key;
            }

            return '\0';
        }





        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Draw the text field border
            spriteBatch.Draw(whitePixel, bounds, Color.LightGray);

            // Draw the text
            spriteBatch.DrawString(font, text.ToString(), new Vector2(bounds.X + 5, bounds.Y + 5), Color.Black);
            if (isSelected && cursorVisible)
            {
                Vector2 cursorPosition = new Vector2(bounds.X + 5 + font.MeasureString(text.ToString()).X, bounds.Y + 5);
                spriteBatch.DrawString(font, "|", cursorPosition, Color.Black);
            }
        }


        // Additional methods for focus, input handling, etc.
    }


}
