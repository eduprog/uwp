﻿using System.Collections.ObjectModel;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class FolderNodeViewModel: NodeViewModel
    {
        public FolderNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, FolderViewModel parent,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parent, parentCollection, childCollection)
        {
            SetFolderInfo();
            Transfer = new TransferObjectModel(this, MTransferType.TYPE_DOWNLOAD, LocalDownloadPath);

            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_default");

            if (!megaNode.getName().ToLower().Equals("camera uploads")) return;
            this.DefaultImagePathData = ResourceService.VisualResources.GetString("VR_FolderTypePath_photo");
        }

        #region Public Methods

        public void SetFolderInfo()
        {
            this.ChildFolders = this.MegaSdk.getNumChildFolders(this.OriginalMNode);
            this.ChildFiles = this.MegaSdk.getNumChildFiles(this.OriginalMNode);

            OnUiThread(() =>
            {
                this.Information = string.Format("{0} {1} | {2} {3}",
                    this.ChildFolders, this.ChildFolders == 1 ? ResourceService.UiResources.GetString("UI_SingleFolder").ToLower() : ResourceService.UiResources.GetString("UI_MultipleFolders").ToLower(),
                    this.ChildFiles, this.ChildFiles == 1 ? ResourceService.UiResources.GetString("UI_SingleFile").ToLower() : ResourceService.UiResources.GetString("UI_MultipleFiles").ToLower());
            });
        }

        #endregion
    }
}
