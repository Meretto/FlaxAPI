////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2012-2017 Flax Engine. All rights reserved.
////////////////////////////////////////////////////////////////////////////////////

using FlaxEditor.CustomEditors.Elements;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEditor.CustomEditors.Editors
{
    /// <summary>
    /// Default implementation of the inspector used to edit Vector3 value type properties.
    /// </summary>
    [CustomEditor(typeof(Vector3)), DefaultEditor]
    public sealed class Vector3Editor : CustomEditor
    {
        private FloatValueElement xElement;
        private FloatValueElement yElement;
        private FloatValueElement zElement;

        /// <inheritdoc />
        public override bool IsInline => true;

        /// <inheritdoc />
        public override void Initialize(LayoutElementsContainer layout)
        {
            if (Values == null)
                return;
            
            var grid = layout.Custom<UniformGridPanel>();
            grid.Custom.Height = TextBox.DefaultHeight;
            grid.Custom.SlotsHorizontally = 3;
            grid.Custom.SlotsVertically = 1;

            xElement = grid.FloatValue();
            xElement.FloatValue.ValueChanged += OnValueChanged;

            yElement = grid.FloatValue();
            yElement.FloatValue.ValueChanged += OnValueChanged;

            zElement = grid.FloatValue();
            zElement.FloatValue.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged()
        {
            SetValue(new Vector3(
                xElement.FloatValue.Value,
                yElement.FloatValue.Value,
                zElement.FloatValue.Value));
        }

        /// <inheritdoc />
        public override void Refresh()
        {
            if (HasDiffrentValues)
            {
                // TOOD: support different values for ValueBox<T>
            }
            else
            {
                var value = (Vector3)Values[0];
                xElement.FloatValue.Value = value.X;
                yElement.FloatValue.Value = value.Y;
                zElement.FloatValue.Value = value.Z;
            }
        }
    }
}