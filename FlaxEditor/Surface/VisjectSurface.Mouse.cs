// Copyright (c) 2012-2018 Wojciech Figat. All rights reserved.

using FlaxEngine;

namespace FlaxEditor.Surface
{
    public partial class VisjectSurface
    {
        /// <summary>
        /// Gets the node under the mouse location.
        /// </summary>
        /// <returns>The node or null if no intersection.</returns>
        private SurfaceNode GetNodeUnderMouse()
        {
            var pos = _surface.PointFromParent(_mousePos);
            if (_surface.GetChildAt(pos) is SurfaceNode node)
                return node;
            return null;
        }

        private void UpdateSelectionRectangle()
        {
            var p1 = _surface.PointFromParent(_leftMouseDownPos);
            var p2 = _surface.PointFromParent(_mousePos);
            var selectionRect = Rectangle.FromPoints(p1, p2);

            // Find nodes to select
            for (int i = 0; i < _nodes.Count; i++)
            {
                _nodes[i].IsSelected = _nodes[i].Bounds.Intersects(ref selectionRect);
            }
        }

        /// <inheritdoc />
        public override void OnMouseEnter(Vector2 location)
        {
            _lastBoxUnderMouse = null;

            // Cache mouse location
            _mousePos = location;

            base.OnMouseEnter(location);
        }

        /// <inheritdoc />
        public override void OnMouseMove(Vector2 location)
        {
            _lastBoxUnderMouse = null;

            // Cache mouse location
            _mousePos = location;

            // Moving around surface with mouse
            if (_rightMouseDown)
            {
                // Calculate delta
                Vector2 delta = location - _rightMouseDownPos;
                if (delta.LengthSquared > 0.01f)
                {
                    // Move view
                    _mouseMoveAmount += delta.Length;
                    _surface.Location += delta;
                    _rightMouseDownPos = location;
                    Cursor = CursorType.SizeAll;
                }

                // Handled
                return;
            }

            // Check if user is selecting or moving node(s)
            if (_leftMouseDown)
            {
                if (_startBox != null) // Connecting
                {
                }
                else if (_isMovingSelection) // Moving
                {
                    // Calculate delta (apply view offset)
                    Vector2 viewDelta = _surface.Location - _movingSelectionViewPos;
                    _movingSelectionViewPos = _surface.Location;
                    Vector2 delta = location - _leftMouseDownPos - viewDelta;
                    if (delta.LengthSquared > 0.01f)
                    {
                        // Move selected nodes
                        delta /= _targeScale;
                        for (int i = 0; i < _nodes.Count; i++)
                        {
                            if (_nodes[i].IsSelected)
                            {
                                _nodes[i].Location += delta;
                            }
                        }
                        _leftMouseDownPos = location;
                        Cursor = CursorType.SizeAll;
                        MarkAsEdited(false);
                    }

                    // Handled
                    return;
                }
                else // Selecting
                {
                    UpdateSelectionRectangle();

                    // Handled
                    return;
                }
            }

            base.OnMouseMove(location);
        }

        /// <inheritdoc />
        public override void OnLostFocus()
        {
            // Clear flags and state
            if (_leftMouseDown)
            {
                _leftMouseDown = false;
            }
            if (_rightMouseDown)
            {
                _rightMouseDown = false;
                Cursor = CursorType.Default;
            }
            _isMovingSelection = false;
            ConnectingEnd(null);

            base.OnLostFocus();
        }

        /// <inheritdoc />
        public override bool OnMouseWheel(Vector2 location, float delta)
        {
            if (base.OnMouseWheel(location, delta))
                return true;

            // Change scale (disable scalig during selecting nodes)
            if (IsMouseOver && !_leftMouseDown)
            {
                var viewCenter = ViewCenterPosition;
                ViewScale += delta * 0.1f;
                ViewCenterPosition = viewCenter;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool OnMouseDown(Vector2 location, MouseButton buttons)
        {
            // Check if user is connecting boxes
            if (_startBox != null)
                return true;

            // Cache data
            _isMovingSelection = false;
            _mousePos = location;
            if (buttons == MouseButton.Left)
            {
                _leftMouseDown = true;
                _leftMouseDownPos = location;
            }
            if (buttons == MouseButton.Right)
            {
                _rightMouseDown = true;
                _rightMouseDownPos = location;
            }

            // Check if any node is under the mouse
            SurfaceNode nodeAtMouse = GetNodeUnderMouse();
            Vector2 cLocation = _surface.PointFromParent(location);
            if (nodeAtMouse != null)
            {
                // Check if mouse is over header and user is pressing mouse left button
                if (_leftMouseDown && nodeAtMouse.HitsHeader(ref cLocation))
                {
                    // Check if user is pressing control
                    if (Root.GetKey(Keys.Control))
                    {
                        // Add to selection
                        AddToSelection(nodeAtMouse);
                    }
                    // Check if node isn't selected
                    else if (!nodeAtMouse.IsSelected)
                    {
                        // Select node
                        Select(nodeAtMouse);
                    }

                    // Start moving selected nodes
                    StartMouseCapture();
                    _isMovingSelection = true;
                    _movingSelectionViewPos = _surface.Location;
                    Focus();
                    return true;
                }
            }
            else
            {
                // Cache flags and state
                if (_leftMouseDown)
                {
                    // Start selecting
                    StartMouseCapture();
                    ClearSelection();
                    Focus();
                    return true;
                }
                if (_rightMouseDown)
                {
                    // Start navigating
                    StartMouseCapture();
                    Focus();
                    return true;
                }
            }

            // Base
            if (base.OnMouseDown(location, buttons))
            {
                // Clear flags to disable handling mouse events by itself (children should do)
                _leftMouseDown = _rightMouseDown = false;
                return true;
            }

            Focus();
            return true;
        }

        /// <inheritdoc />
        public override bool OnMouseUp(Vector2 location, MouseButton buttons)
        {
            // Cache mouse location
            _mousePos = location;

            // Check if any node is under the mouse
            SurfaceNode nodeAtMouse = GetNodeUnderMouse();
            if (nodeAtMouse == null)
            {
                // Check if no move has been made at all
                if (_mouseMoveAmount < 5.0f)
                {
                    ClearSelection();
                }
            }

            // Cache flags and state
            if (_leftMouseDown && buttons == MouseButton.Left)
            {
                _leftMouseDown = false;
                EndMouseCapture();
                Cursor = CursorType.Default;

                if (!_isMovingSelection && _startBox == null)
                {
                    UpdateSelectionRectangle();
                }
            }
            if (_rightMouseDown && buttons == MouseButton.Right)
            {
                _rightMouseDown = false;
                EndMouseCapture();
                Cursor = CursorType.Default;

                // Check if no move has been made at all
                if (_mouseMoveAmount < 3.0f)
                {
                    // Check if any node is under the mouse
                    _cmStartPos = location;
                    if (nodeAtMouse != null)
                    {
                        if (!HasSelection)
                            Select(nodeAtMouse);

                        // Show secondary context menu
                        ShowSecondaryCM(_cmStartPos);
                    }
                    else
                    {
                        // Show primary context menu
                        ShowPrimaryMenu(_cmStartPos);
                    }
                }
                _mouseMoveAmount = 0;
            }

            // Base
            if (base.OnMouseUp(location, buttons))
                return true;

            if (buttons == MouseButton.Left)
            {
                ConnectingEnd(null);
                EndMouseCapture();
            }

            return true;
        }

        /// <inheritdoc />
        public override bool OnKeyDown(Keys key)
        {
            if (base.OnKeyDown(key))
                return true;

            if (key == Keys.Delete)
            {
                Delete();
                return true;
            }

            if (Root.GetKey(Keys.Control))
            {
                switch (key)
                {
                case Keys.A:
                    SelectAll();
                    return true;
                case Keys.C:
                    Copy();
                    return true;
                case Keys.V:
                    Paste();
                    return true;
                case Keys.X:
                    Cut();
                    return true;
                case Keys.D:
                    Duplicate();
                    return true;
                }
            }

            return false;
        }
    }
}
