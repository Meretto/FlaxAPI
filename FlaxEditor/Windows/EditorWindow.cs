﻿// Flax Engine scripting API

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.GUI.Docking;

namespace FlaxEditor.Windows
{
    /// <summary>
    ///  Base class for all windows in Editor.
    /// </summary>
    /// <seealso cref="FlaxEngine.GUI.Docking.DockWindow" />
    public abstract class EditorWindow : DockWindow
    {
        /// <summary>
        /// Gets the editor object.
        /// </summary>
        public readonly Editor Editor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorWindow"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="hideOnClose">True if hide window on closing, otherwise it will be destroyed.</param>
        /// <param name="scrollBars">The scroll bars.</param>
        protected EditorWindow(Editor editor, bool hideOnClose, ScrollBars scrollBars)
            : base(editor.UI.MasterPanel, hideOnClose, scrollBars)
        {
            Editor = editor;

            // Register
            Editor.Windows.Windows.Add(this);
        }

        #region Window Events

        /// <summary>
        /// Fired when scene starts saving
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneSaving(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene gets saved
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneSaved(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene gets saving error
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneSaveError(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene starts loading
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneLoading(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene gets loaded
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneLoaded(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene cannot be loaded
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneLoadError(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene gets unloading
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneUnloading(Scene scene, Guid sceneId)
        {
        }

        /// <summary>
        /// Fired when scene gets unloaded
        /// </summary>
        /// <param name="scene">The scene object. It may be null!</param>
        /// <param name="sceneId">The scene ID.</param>
        public virtual void OnSceneUnloaded(Scene scene, Guid sceneId)
        {
        }

        #endregion

        /// <inheritdoc />
        public override void OnDestroy()
        {
            // Unregister
            Editor.Windows.Windows.Remove(this);

            base.OnDestroy();
        }
    }
}