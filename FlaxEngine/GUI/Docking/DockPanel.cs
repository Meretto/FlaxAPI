﻿// Flax Engine scripting API

using System;
using System.Collections.Generic;
using FlaxEngine.Assertions;

namespace FlaxEngine.GUI.Docking
{
    /// <summary>
    /// Dockable window mode.
    /// </summary>
    public enum DockState
    {
        Unknown = 0,
        Float = 1,
        //DockTopAutoHide = 2,
        //DockLeftAutoHide = 3,
        //DockBottomAutoHide = 4,
        //DockRightAutoHide = 5,
        DockFill = 6,
        DockTop = 7,
        DockLeft = 8,
        DockBottom = 9,
        DockRight = 10,
        Hidden = 11
    }

    /// <summary>
    /// Dockable panel control.
    /// </summary>
    /// <seealso cref="FlaxEngine.GUI.ContainerControl" />
    public class DockPanel : ContainerControl
    {
        /// <summary>
        /// The default splitters value.
        /// </summary>
        public const float DefaultSplitterValue = 0.3f;

        protected readonly DockPanel _parentPanel;
        protected readonly List<DockPanel> _childPanels = new List<DockPanel>();
        protected readonly List<DockWindow> _tabs = new List<DockWindow>();
        protected int _selectedTabIndex;
        protected Proxy _tabsProxy;

        /// <summary>
        /// Returns true if this panel is a master panel.
        /// </summary>
        /// <returns>True if this panel is a master panel.</returns>
        public virtual bool IsMaster => false;

        /// <summary>
        /// Returns true if this panel is a floating window panel.
        /// </summary>
        /// <returns>True if this panel is a floating window panel.</returns>
        public virtual bool IsFloating => false;

        /// <summary>
        /// Gets docking area bounds (tabs rectangle) in a screen space.
        /// </summary>
        /// <returns>Tabs rectangle area.</returns>
        public Rectangle DockAreaBounds
        {
            get
            {
                var parentWin = ParentWindow;
                if (parentWin == null)
                    throw new InvalidOperationException("Missing parent window.");
                var control = _tabsProxy != null ? (Control)_tabsProxy : this;
                var clientPos = control.PointToWindow(Vector2.Zero);
                return new Rectangle(parentWin.ClientToScreen(clientPos), control.Size);
            }
        }

        /// <summary>
        /// Gets amount of the tabs in a dock panel.
        /// </summary>
        /// <returns>The amount of tabs.</returns>
        public int TabsCount => _tabs.Count;

        /// <summary>
        /// Gets index of the selected tab.
        /// </summary>
        /// <returns>The selected tab index.</returns>
        public int SelectedTabIndex => _selectedTabIndex;

        /// <summary>
        /// Gets the selected tab.
        /// </summary>
        /// <returns>The selected tab.</returns>
        public DockWindow SelectedTab => _selectedTabIndex >= 0 ? _tabs[_selectedTabIndex] : null;

        /// <summary>
        /// Gets the parent panel.
        /// </summary>
        /// <value>
        /// The parent panel.
        /// </value>
        public DockPanel ParentDockPanel => _parentPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockPanel"/> class.
        /// </summary>
        /// <param name="parentPanel">The parent panel.</param>
        public DockPanel(DockPanel parentPanel)
            : base(false, 0, 0, 64, 64)
        {
            _parentPanel = parentPanel;
            _parentPanel?._childPanels.Add(this);
            DockStyle = DockStyle.Fill;
        }

        /// <summary>
        /// Gets tab at the given index.
        /// </summary>
        /// <param name="tabIndex">The index of the tab page.</param>
        /// <returns>The tab.</returns>
        public DockWindow GetTab(int tabIndex)
        {
            return _tabs[tabIndex];
        }

        /// <summary>
        /// Gets tab at the given index.
        /// </summary>
        /// <param name="tab">The tab page.</param>
        /// <returns>The index of the given tab.</returns>
        public int GetTabIndex(DockWindow tab)
        {
            return _tabs.IndexOf(tab);
        }

        /// <summary>
        /// Selects the tab page.
        /// </summary>
        /// <param name="tabIndex">The index of the tab page to select.</param>
        public void SelectTab(int tabIndex)
        {
            if (tabIndex < -1 || tabIndex >= _tabs.Count)
                throw new InvalidOperationException("Cannot select tab.");

            // Check if tab will change
            if (_selectedTabIndex != tabIndex)
            {
                var oldSelected = SelectedTab;
                var newSelected = tabIndex != -1 ? _tabs[tabIndex] : null;

                // Change
                ContainerControl proxy;
                if (oldSelected != null)
                {
                    proxy = oldSelected.Parent;
                    oldSelected.Parent = null;
                }
                else
                {
                    createTabsProxy();
                    proxy = _tabsProxy;
                }
                _selectedTabIndex = tabIndex;
                if (newSelected != null)
                {
                    newSelected.UnlockChildrenRecursive();
                    newSelected.Parent = proxy;
                    newSelected.Focus();
                }
            }
        }

        /// <summary>
        /// Selects the tab page.
        /// </summary>
        /// <param name="tab">The tab page to select.</param>
        public void SelectTab(DockWindow tab)
        {
            int tabIndex = GetTabIndex(tab);
            SelectTab(tabIndex);
        }

        /// <summary>
        /// Performs hit test over dock panel
        /// </summary>
        /// <param name="position">Screen space position to test</param>
        /// <returns>Dock panel that has been hitted or null if nothing found</returns>
        public DockPanel HitTest(ref Vector2 position)
        {
            // Get parent window and transform point position into local coordinates system
            var parentWin = ParentWindow;
            if (parentWin == null)
                return null;
            Vector2 clientPos = parentWin.ScreenToClient(position);
            Vector2 localPos = PointFromWindow(clientPos);

            // Early out
            if (localPos.X < 0 || localPos.Y < 0)
                return null;
            if (localPos.X > Width || localPos.Y > Height)
                return null;

            // Test all docked controls (find the smallest one)
            float sizeLengthSquared = float.MaxValue;
            DockPanel result = null;
            for (int i = 0; i < _childPanels.Count; i++)
            {
                var panel = _childPanels[i].HitTest(ref position);
                if (panel != null)
                {
                    var sizeLen = panel.Size.LengthSquared;
                    if (sizeLen < sizeLengthSquared)
                    {
                        sizeLengthSquared = sizeLen;
                        result = panel;
                    }
                }
            }
            if (result != null)
                return result;

            // Itself
            return this;
        }

        /// <summary>
        /// Try get panel dock state
        /// </summary>
        /// <param name="splitterValue">Splitter value</param>
        /// <returns>Dock State</returns>
        public virtual DockState TryGetDockState(ref float splitterValue)
        {
            DockState result = DockState.Unknown;
            splitterValue = DefaultSplitterValue;

            if (HasParent)
            {
                if (Parent.Parent is SplitPanel splitter)
                {
                    splitterValue = splitter.SplitterValue;
                    if (Parent == splitter.Panel1)
                    {
                        if (splitter.Orientation == Orientation.Horizontal)
                            result = DockState.DockLeft;
                        else
                            result = DockState.DockTop;
                    }
                    else
                    {
                        if (splitter.Orientation == Orientation.Horizontal)
                            result = DockState.DockRight;
                        else
                            result = DockState.DockBottom;
                        splitterValue = 1 - splitterValue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Create child dock panel
        /// </summary>
        /// <param name="state">Dock panel state</param>
        /// <param name="splitterValue">Initial splitter value</param>
        /// <returns>Child panel</returns>
        public DockPanel CreateChildPanel(DockState state, float splitterValue)
        {
            createTabsProxy();

            // Create child dock panel
            var dockPanel = new DockPanel(this);

            // Switch dock mode
            Control c1;
            Control c2;
            Orientation o;
            switch (state)
            {
                case DockState.DockTop:
                {
                    o = Orientation.Vertical;
                    c1 = dockPanel;
                    c2 = _tabsProxy;
                    break;
                }


                case DockState.DockBottom:
                {
                    splitterValue = 1 - splitterValue;
                    o = Orientation.Vertical;
                    c1 = _tabsProxy;
                    c2 = dockPanel;
                    break;
                }


                case DockState.DockLeft:
                {
                    o = Orientation.Horizontal;
                    c1 = dockPanel;
                    c2 = _tabsProxy;
                    break;
                }


                case DockState.DockRight:
                {
                    splitterValue = 1 - splitterValue;
                    o = Orientation.Horizontal;
                    c1 = _tabsProxy;
                    c2 = dockPanel;
                    break;
                }

                default: throw new ArgumentOutOfRangeException();
            }

            // Create splitter and link controls
            var parent = _tabsProxy.Parent;
            SplitPanel splitter = new SplitPanel(o, ScrollBars.None, ScrollBars.None);
            splitter.SplitterValue = splitterValue;
            splitter.Panel1.AddChild(c1);
            splitter.Panel2.AddChild(c2);
            parent.AddChild(splitter);

            // Update
            splitter.UnlockChildrenRecursive();
            splitter.PerformLayout();

            return dockPanel;
        }

        protected virtual void OnLastTabRemoved()
        {
            // Check if dock panel is linked to the split panel
            if (HasParent)
            {
                if (Parent.Parent is SplitPanel splitter)
                {
                    // Check if has any child panels
                    var childPanel = new List<DockPanel>(_childPanels);
                    for (int i = 0; i < childPanel.Count; i++)
                    {
                        // Undock all tabs
                        var panel = childPanel[0];
                        int count = panel.TabsCount;
                        while (count-- > 0)
                        {
                            panel.GetTab(0).Close();
                        }
                    }

                    // Unlink splitter
                    var splitterParent = splitter.Parent;
                    Assert.IsNotNull(splitterParent);
                    splitter.Parent = null;

                    // Move controls from second split panel to the split panel parent
                    var scrPanel = Parent == splitter.Panel2 ? splitter.Panel1 : splitter.Panel2;
                    var srcPanelChildrenCount = scrPanel.ChildrenCount;
                    for (int i = srcPanelChildrenCount - 1; i >= 0 && scrPanel.ChildrenCount > 0; i--)
                    {
                        scrPanel.GetChild(i).Parent = splitterParent;
                    }
                    Assert.IsTrue(scrPanel.ChildrenCount == 0);
                    Assert.IsTrue(splitterParent.ChildrenCount == srcPanelChildrenCount);

                    // Delete
                    splitter.Dispose();
                }
                else if (!IsMaster)
                {
                    throw new InvalidOperationException();
                }
            }
            else if (!IsMaster)
            {
                throw new InvalidOperationException();
            }
        }

        internal virtual void DockWindowInternal(DockState state, DockWindow window)
        {
            DockWindow(state, window);
        }

        protected virtual void DockWindow(DockState state, DockWindow window)
        {
            createTabsProxy();

            // Check if dock like a tab or not
            if (state == DockState.DockFill)
            {
                // Add tab
                addTab(window);
            }
            else
            {
                // Create child panel
                var dockPanel = CreateChildPanel(state, DefaultSplitterValue);

                // Dock window as a tab in a child panel
                dockPanel.DockWindow(DockState.DockFill, window);
            }
        }

        internal void UndockWindowInternal(DockWindow window)
        {
            UndockWindow(window);
        }

        protected virtual void UndockWindow(DockWindow window)
        {
            // Undock
            var index = GetTabIndex(window);
            if (index == -1)
                throw new IndexOutOfRangeException();
            _tabs.RemoveAt(index);
            window.ParentDockPanel = null;

            // Check if tab was selected
            if (window == SelectedTab)
            {
                // Change selection
                SelectTab(index - 1);
            }

            // Check if has no more tabs
            if (_tabs.Count == 0)
            {
                OnLastTabRemoved();
            }
            else
            {
                // Update
                PerformLayout();
            }
        }

        protected virtual void addTab(DockWindow window)
        {
            // Dock
            _tabs.Add(window);
            window.ParentDockPanel = this;

            // Select tab
            SelectTab(window);
        }

        private void createTabsProxy()
        {
            // Check if has no tabs proxy created
            if (_tabsProxy == null)
            {
                // Create proxy and make set simple full dock
                _tabsProxy = new Proxy(this);
                _tabsProxy.Parent = this;
                _tabsProxy.IsLayoutLocked = false;
            }
        }

        /// <inheritdoc />
        public override void OnDestroy()
        {
            _parentPanel?._childPanels.Remove(this);
            
            base.OnDestroy();
        }

        protected class Proxy : ContainerControl
        {
            public DockPanel Panel;
            public bool IsMouseDown;
            public bool IsMouseDownOverCross;
            public DockWindow MouseDownWindow;
            public Vector2 MousePosition;
            public DockWindow StartDragAsyncWindow;

            public Proxy(DockPanel panel)
                : base(false, 0, 0, 0, 0)
            {
            }


            /*   private DockWindow getTabAtPos(Vector2 position, out bool closeButton) const;
               private void getTabRect(DockWindow win, out Rectangle bounds) const;
               private void startDrag(DockWindow win);
               private void startDragAsync(float dt);
   
           public:
   
           // [ContainerControl]
           String ToString() const override;
           void Draw(Render2D* render, const Vector2& root) override;
           void OnLostFocus() override;
           void OnMouseEnter(const Vector2& location) override;
           void OnMouseMove(const Vector2& location) override;
           bool OnMouseDown(MouseButtons buttons, const Vector2& location) override;
           bool OnMouseUp(MouseButtons buttons, const Vector2& location) override;
           void OnMouseLeave() override;
   
           protected:
   
           // [ContainerControl]
           void getDesireClientArea(Rectangle rect) const override;*/
        }
    }
}