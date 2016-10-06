﻿using System;
using System.Collections.Generic;
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;

namespace MegaApp.Classes
{
    /// <summary>
    /// Class to provide easy access to useful information of links.
    /// </summary>
    public class LinkInformation
    {
        /// <summary>
        /// Class constructor.
        /// </summary>
        public LinkInformation()
        {
            this.SelectedNodes = new List<IMegaNode>();
            this.FoldersToImport = new Dictionary<string, List<MNode>>();
            this.FolderPaths = new Dictionary<string, string>();

            this.Reset();
        }

        /// <summary>
        /// Method to reset all the class properties to the default values.
        /// </summary>
        /// <param name="clearDictionaries">
        /// Value which indicates if clear the dictionaries used to import folder links.
        /// </param>
        public void Reset(bool clearDictionaries = true)
        {
            ActiveLink = null;
            UriLink = UriLinkType.None;
            LinkAction = LinkAction.None;
            PublicNode = null;
            DownloadPath = null;
            SelectedNodes.Clear();

            HasFetchedNodesFolderLink = false;

            if (clearDictionaries)
            {
                FoldersToImport.Clear();
                FolderPaths.Clear();
            }
        }

        #region Properties

        /// <summary>
        /// Link which is being processed or will be processed by the app.
        /// </summary>
        public string ActiveLink { get; set; }

        /// <summary>
        /// Type of the current link.
        /// </summary>
        public UriLinkType UriLink { get; set; }

        /// <summary>
        /// Operation to realize with the current active link.
        /// </summary>
        public LinkAction LinkAction { get; set; }

        /// <summary>
        /// Node obtained from a file link.
        /// </summary>
        public MNode PublicNode { get; set; }

        /// <summary>
        /// Selected nodes to process from a folder link.
        /// </summary>
        public List<IMegaNode> SelectedNodes { get; set; }

        /// <summary>
        /// The download path for the selected nodes in case of download operation.
        /// </summary>
        public string DownloadPath { get; set; }

        /// <summary>
        /// Indicates if the app has already fetched nodes of the folder link.
        /// </summary>
        public bool HasFetchedNodesFolderLink { get; set; }

        /// <summary>
        /// Dictionary to store the subfolders to import from a folder link.
        /// <para>- Key: Base64Handle of the parent folder.</para>
        /// <para>- Value: Path of the folder node to import.</para>
        /// </summary>
        public Dictionary<string, List<MNode>> FoldersToImport;

        /// <summary>
        /// Dictionary to store the subfolder node paths to import from a folder link.
        /// <para>- Key: Base64Handle of folder node to import.</para>
        /// <para>- Value: Path of the folder node to import.</para>
        /// </summary>
        public Dictionary<string, string> FolderPaths;

        #endregion
    }
}
