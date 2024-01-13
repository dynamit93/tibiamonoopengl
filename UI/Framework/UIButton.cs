using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CTC
{
    public class UIButton : UIView
    {


        /// <summary>
        /// The button type that will be used when the button ain't do anything.
        /// </summary>
        public UIElementType NormalType
        {
            get { return _NormalType; }
            set
            {
                if (!Highlighted)
                    ElementType = value;
                _NormalType = value;
            }
        }
        private UIElementType _NormalType;

        /// <summary>
        /// The Element type that will be used when the button is highlighted.
        /// </summary>
        public UIElementType HighlightType
        {
            get { return _HighlightType; }
            set
            {
                if (Highlighted)
                    ElementType = value;
                _HighlightType = value;
            }
        }
        private UIElementType _HighlightType;

        /// <summary>
        /// The button is Highlighted when the user has started pressing it
        /// but not released the mouse button yet.
        /// </summary>
        public virtual bool Highlighted
        {
            get
            {
                return _Highlighted;
            }
            set
            {
                if (value)
                    ElementType = HighlightType;
                else
                    ElementType = NormalType;
                _Highlighted = value;
            }
        }
        private bool _Highlighted = false;

        /// <summary>
        /// The name of the button, will be displayed centered on it.
        /// </summary>
        // TODO: Replace this with an UILabel
        public String Label;

        public delegate void ButtonPressedEvent(UIButton Button, MouseState mouse);


        public event ButtonPressedEvent ButtonPressed;
        public event ButtonPressedEvent ButtonDragged;
        public event ButtonPressedEvent ButtonReleased;
        public event ButtonPressedEvent ButtonReleasedInside;
        public event ButtonPressedEvent ButtonReleasedOutside;

        public UIButton(String Label = "")
        {
            this.Label = Label;
            ElementType = UIElementType.Button;
            NormalType = UIElementType.Button;
            HighlightType = UIElementType.ButtonHighlight;

            Bounds = new Rectangle(520, 460, 100, 30);
        }

        public override bool MouseLeftClick(MouseState mouse)
        {
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (CaptureMouse())
                {
                    if (ButtonPressed != null)
                        ButtonPressed(this, mouse);
                    Highlighted = true;
                }
            }
            else if (mouse.LeftButton == ButtonState.Released && Highlighted)
            {
                ReleaseMouse();
                
                // Fire some events
                if (ButtonReleased != null)
                    ButtonReleased(this, mouse);
                if (ScreenBounds.Contains(new Point(mouse.X, mouse.Y)))
                {
                    if (ButtonReleasedInside != null)
                        ButtonReleasedInside(this, mouse);
                }
                else
                {
                    if (ButtonReleasedOutside != null)
                        ButtonReleasedOutside(this, mouse);
                }
            }
            return true;
        }

        public override bool MouseMove(MouseState mouse)
        {
            if (ButtonDragged != null)
                ButtonDragged(this, mouse);
            return base.MouseMove(mouse);
        }

        public override bool MouseLost()
        {
            Highlighted = false;
            return true;
        }

        protected override void DrawContent(SpriteBatch CurrentBatch)
        {

            // Draw the button background
            Color backgroundColor = Highlighted ? Color.Gray : Color.DarkGray; // Example colors
            
            Debug.WriteLine($"Bounds: {Bounds}");
            CurrentBatch.Draw(UIContext.WhitePixel, Bounds, Color.Blue);


            if (Label != null)
            {
                Vector2 labelSize = UIContext.StandardFont.MeasureString(Label);
                Vector2 labelPosition = new Vector2(
                    ClientBounds.X + (ClientBounds.Width - labelSize.X) / 2,
                    ClientBounds.Y + (ClientBounds.Height - labelSize.Y) / 2
                );

                CurrentBatch.DrawString(
                    UIContext.StandardFont, Label, labelPosition,
                    Color.White
                );
            }
        }
    }
}
