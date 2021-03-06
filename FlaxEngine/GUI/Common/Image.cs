// Copyright (c) 2012-2018 Wojciech Figat. All rights reserved.

using System;

namespace FlaxEngine.GUI
{
    /// <summary>
    /// The basic GUI image control. Shows texture, sprite or render target.
    /// </summary>
    /// <seealso cref="FlaxEngine.GUI.ContainerControl" />
    public class Image : ContainerControl
    {
        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        [EditorOrder(10), Tooltip("The image to draw.")]
        public IImageSource ImageSource { get; set; }

        /// <summary>
        /// Gets or sets the margin for the image.
        /// </summary>
        [EditorOrder(20), Tooltip("Margins of the image area.")]
        public Margin Margin { get; set; }

        /// <summary>
        /// Gets or sets the color used to multiply the image pixels.
        /// </summary>
        [EditorDisplay("Style"), EditorOrder(2000)]
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the color used to multiply the image pixels when mouse is over the image.
        /// </summary>
        [EditorDisplay("Style"), EditorOrder(2000)]
        public Color MouseOverColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets a value indicating whether render image with alpha blending.
        /// </summary>
        [EditorOrder(50), Tooltip("If checked, image will be rendered with alpha blending, otherwise it will be opaque.")]
        public bool WithAlpha { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether keep aspect ratio whend rawing the image.
        /// </summary>
        [EditorOrder(60), Tooltip("If checked, control will keep aspect ratio of the image.")]
        public bool KeepAspectRatio { get; set; } = true;

        /// <summary>
        /// Occurs when mouse clicks on the image.
        /// </summary>
        public event Action<Image, MouseButton> Clicked;

        /// <inheritdoc />
        public Image()
        : base(0, 0, 64, 64)
        {
            CanFocus = false;
        }

        /// <inheritdoc />
        public Image(float x, float y, float width, float height)
        : base(x, y, width, height)
        {
            CanFocus = false;
        }

        /// <inheritdoc />
        public Image(Vector2 location, Vector2 size)
        : base(location, size)
        {
            CanFocus = false;
        }

        /// <inheritdoc />
        public Image(Rectangle bounds)
        : base(bounds)
        {
            CanFocus = false;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            base.Draw();

            if (ImageSource == null)
                return;

            Rectangle rect;
            if (KeepAspectRatio)
            {
                // Figure out the ratio
                var size = Size;
                var imageSize = ImageSource.Size;
                if (imageSize.LengthSquared < 1)
                    return;
                var ratio = size / imageSize;
                var aspectRatio = ratio.MinValue;

                // Get the new height and width
                var newSize = imageSize * aspectRatio;

                // Calculate the X,Y position of the upper-left corner 
                // (one of these will always be zero)
                var newPos = (size - newSize) / 2;

                rect = new Rectangle(newPos, newSize);
            }
            else
            {
                rect = new Rectangle(Vector2.Zero, Size);
            }

            Margin.ShrinkRectangle(ref rect);

            ImageSource.Draw(rect, IsMouseOver ? MouseOverColor : Color, WithAlpha);
        }

        /// <inheritdoc />
        public override bool OnMouseUp(Vector2 location, MouseButton buttons)
        {
            if (base.OnMouseUp(location, buttons))
                return true;

            if (Clicked != null)
            {
                Clicked.Invoke(this, buttons);
                return true;
            }

            return false;
        }
    }
}
