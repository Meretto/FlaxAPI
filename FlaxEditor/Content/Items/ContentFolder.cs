// Copyright (c) 2012-2018 Wojciech Figat. All rights reserved.

using System;
using System.Collections.Generic;
using FlaxEditor.GUI.Drag;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEditor.Content
{
    /// <summary>
    /// Types of content directories.
    /// </summary>
    public enum ContentFolderType
    {
        /// <summary>
        /// The directory with assets.
        /// </summary>
        Content,

        /// <summary>
        /// The directory with script source files.
        /// </summary>
        Source,

        /// <summary>
        /// The directory with Editor private files.
        /// </summary>
        Editor,

        /// <summary>
        /// The directory with Engine private files.
        /// </summary>
        Engine,

        /// <summary>
        /// The other type of directory.
        /// </summary>
        Other
    }

    /// <summary>
    /// Represents workspace directory item.
    /// </summary>
    public class ContentFolder : ContentItem
    {
        private DragItems _dragOverItems;
        private bool _validDragOver;

        /// <summary>
        /// Gets the type of the folder.
        /// </summary>
        public ContentFolderType FolderType { get; }

        /// <summary>
        /// Returns true if that folder can import/manage scripts.
        /// </summary>
        public bool CanHaveScripts => FolderType == ContentFolderType.Source;

        /// <summary>
        /// Returns true if that folder can import/manage assets.
        /// </summary>
        public bool CanHaveAssets => FolderType == ContentFolderType.Content || FolderType == ContentFolderType.Editor || FolderType == ContentFolderType.Engine;

        /// <summary>
        /// Returns true if that folder belongs to the project workspace.
        /// </summary>
        public bool IsProjectOnly => FolderType == ContentFolderType.Content || FolderType == ContentFolderType.Source;

        /// <summary>
        /// Returns true if that folder belongs to the Engine or Editor private files.
        /// </summary>
        public bool IsEnginePrivate => FolderType == ContentFolderType.Editor || FolderType == ContentFolderType.Engine;

        /// <summary>
        /// Gets the content node.
        /// </summary>
        public ContentTreeNode Node { get; }

        /// <summary>
        /// The subitems of this folder.
        /// </summary>
        public readonly List<ContentItem> Children = new List<ContentItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFolder"/> class.
        /// </summary>
        /// <param name="type">The folder type.</param>
        /// <param name="path">The path to the item.</param>
        /// <param name="node">The folder parent node.</param>
        internal ContentFolder(ContentFolderType type, string path, ContentTreeNode node)
        : base(path)
        {
            FolderType = type;
            Node = node;
        }

        /// <summary>
        /// Tries to find child element with given path
        /// </summary>
        /// <param name="path">Element path to find</param>
        /// <returns>Found element of null</returns>
        public ContentItem FindChild(string path)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Path == path)
                    return Children[i];
            }

            return null;
        }

        /// <summary>
        /// Check if folder contains child element with given path
        /// </summary>
        /// <param name="path">Element path to find</param>
        /// <returns>True if contains that element, otherwise false</returns>
        public bool ContainsChild(string path)
        {
            return FindChild(path) != null;
        }

        /// <inheritdoc />
        public override ContentItemType ItemType => ContentItemType.Folder;

        /// <inheritdoc />
        public override bool CanRename => ParentFolder != null; // Deny rename action for root folders

        /// <inheritdoc />
        public override bool CanDrag => ParentFolder != null; // Deny rename action for root folders

        /// <inheritdoc />
        public override bool Exists => System.IO.Directory.Exists(Path);

        /// <inheritdoc />
        public override string DefaultThumbnailName => "Folder64";

        /// <inheritdoc />
        internal override void UpdatePath(string value)
        {
            base.UpdatePath(value);

            // Update node text
            Node.Text = ShortName;
        }

        /// <inheritdoc />
        protected override void OnParentFolderChanged()
        {
            // Update tree nodes structure
            Node.Parent = ParentFolder?.Node;

            base.OnParentFolderChanged();
        }

        /// <inheritdoc />
        public override ContentItem Find(string path)
        {
            // TODO: split name into parts and check each going tree sructure level down - make it faster

            if (Path == path)
                return this;

            for (int i = 0; i < Children.Count; i++)
            {
                var result = Children[i].Find(path);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <inheritdoc />
        public override bool Find(ContentItem item)
        {
            if (item == this)
                return true;

            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Find(item))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override ContentItem Find(Guid id)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var result = Children[i].Find(id);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <inheritdoc />
        public override ScriptItem FindScriptWitScriptName(string scriptName)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var result = Children[i].FindScriptWitScriptName(scriptName);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <inheritdoc />
        public override int Compare(Control other)
        {
            if (other is ContentItem otherItem)
            {
                if (!otherItem.IsFolder)
                    return -1;
                return string.Compare(ShortName, otherItem.ShortName, StringComparison.InvariantCulture);
            }

            return base.Compare(other);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            base.Draw();

            // Check if drag is over
            if (IsDragOver && _validDragOver)
                Render2D.FillRectangle(new Rectangle(Vector2.Zero, Size), Style.Current.BackgroundSelected * 0.6f, true);
        }

        private bool ValidateDragItem(ContentItem item)
        {
            // Reject itself and any parent
            return item != this && !item.Find(this);
        }

        /// <inheritdoc />
        public override DragDropEffect OnDragEnter(ref Vector2 location, DragData data)
        {
            base.OnDragEnter(ref location, data);

            // Check if drop file(s)
            if (data is DragDataFiles)
            {
                _validDragOver = true;
                return DragDropEffect.Copy;
            }

            // Check if drop asset(s)
            if (_dragOverItems == null)
                _dragOverItems = new DragItems();
            _dragOverItems.OnDragEnter(data, ValidateDragItem);
            _validDragOver = _dragOverItems.HasValidDrag;
            return _dragOverItems.Effect;
        }

        /// <inheritdoc />
        public override DragDropEffect OnDragMove(ref Vector2 location, DragData data)
        {
            base.OnDragMove(ref location, data);

            if (data is DragDataFiles)
                return DragDropEffect.Copy;
            return _dragOverItems.Effect;
        }

        /// <inheritdoc />
        public override DragDropEffect OnDragDrop(ref Vector2 location, DragData data)
        {
            var result = base.OnDragDrop(ref location, data);

            // Check if drop file(s)
            if (data is DragDataFiles files)
            {
                // Import files
                Editor.Instance.ContentImporting.Import(files.Files, this);
                result = DragDropEffect.Copy;
            }
            else if (_dragOverItems.HasValidDrag)
            {
                // Move items
                Editor.Instance.ContentDatabase.Move(_dragOverItems.Objects, this);
                result = DragDropEffect.Move;
            }

            // Clear cache
            _dragOverItems?.OnDragDrop();
            _validDragOver = false;

            return result;
        }

        /// <inheritdoc />
        public override void OnDragLeave()
        {
            _dragOverItems?.OnDragLeave();
            _validDragOver = false;

            base.OnDragLeave();
        }
    }
}
