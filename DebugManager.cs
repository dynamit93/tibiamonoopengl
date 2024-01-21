using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Text;
using CTC; // Using the CTC namespace

namespace tibiamonoopengl
{
    public class DebugManager
    {
        private StringBuilder debugText = new StringBuilder();

        // Change the delegate to match the Log.LogMessageHandler
        public event Log.LogMessageHandler OnLogMessage;

        public void Initialize()
        {
            // Subscribe to the log events using the Log.LogMessageHandler
            Log.Instance.OnLogMessage += OnLogMessage;
        }

        public void Update(GameTime gameTime)
        {
            // Handle update logic here
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw debug information here using spriteBatch
        }

        public void LogMessage(Log.Level level, string text)
        {
            // Create a Log.Message object compatible with the CTC.Log class
            Log.Message message = new Log.Message
            {
                time = DateTime.Now,
                level = level,
                text = text,
                sender = this // Add 'sender' property
            };

            // Invoke the event with the correct delegate type
            OnLogMessage?.Invoke(this, message);
        }
    }
}
