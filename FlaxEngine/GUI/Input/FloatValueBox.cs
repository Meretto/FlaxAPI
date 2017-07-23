﻿////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2012-2017 Flax Engine. All rights reserved.
////////////////////////////////////////////////////////////////////////////////////

namespace FlaxEngine.GUI
{
    /// <summary>
    /// Floating point value editor.
    /// </summary>
    /// <seealso cref="float" />
    public class FloatValueBox : ValueBox<float>
    {
        /// <inheritdoc />
        public override float Value
        {
            get => _value;
            set
            {
                value = Mathf.Clamp(value, _min, _max);
                if (_value != value)
                {
                    // Set value
                    _value = value;

                    // Update
                    UpdateText();
                    OnValueChanged();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatValueBox"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <param name="width">The width.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="slideSpeed">The slide speed.</param>
        public FloatValueBox(float value, float x = 0, float y = 0, float width = 120, float min = float.MinValue, float max = float.MaxValue, float slideSpeed = 1)
            : base(Mathf.Clamp(value, min, max), x, y, width, min, max, slideSpeed)
        {
            UpdateText();
        }

        /// <inheritdoc />
        protected sealed override void UpdateText()
        {
            // Format
            var text = _value.ToString();

            // Set text
            Text = text;
        }

        /// <inheritdoc />
        protected override void TryGetValue()
        {
            // Try to parse float
            float value;
            if (float.TryParse(Text, out value))
            {
                // Set value
                Value = value;
            }
        }

        /// <inheritdoc />
        protected override void ApplySliding(float delta)
        {
            Value = _startSlideValue + delta;
        }
    }
}
