﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using electrifier.Controls;
using electrifier.Controls.Helpers;
using electrifier.Controls.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Vanara.PInvoke;
using Vanara.Windows.Shell;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static Vanara.PInvoke.ComCtl32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

/// <summary>
/// A WinUI 3 control that displays a <see cref="electrifier.Controls.ShellNamespaceTreeControl"/>
/// and <see cref="electrifier.Controls.ShellListView"/> for navigating through the shell namespace.
/// 
/// This replaces the <see cref="Microsoft.WindowsAPICodePack.Controls.ExplorerBrowser"/> control.
/// </summary>
public sealed partial class ExplorerBrowser : UserControl
{
    private bool _isLoading = true;

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (value == _isLoading)
            {
                return;
            }

            _isLoading = value;
            // OnPropertyChanged();
        }
    }
    private Shel32NamespaceService Shel32NamespaceService => App.GetService<Shel32NamespaceService>();

    /// <summary>The default text that is displayed when an empty folder is shown</summary>
    [Category("Appearance"), DefaultValue("This folder is empty."), Description("The default text that is displayed when an empty folder is shown.")]
    public string EmptyFolderText { get; set; } = "This folder is empty.";
    /// <summary>The default text that is displayed when an empty group is shown</summary>
    [Category("Appearance"), DefaultValue("This group is empty."), Description("The default text that is displayed when an empty group is shown.")]
    public string EmptyGroupText { get; set; } = "This group is empty.";

    public event EventHandler<NavigatedEventArgs> Navigated;
    public event EventHandler<Vanara.Windows.Shell.NavigationFailedEventArgs> NavigationFailed;

    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        Loading += ExplorerBrowser_Loading;
        Loaded += ExplorerBrowser_Loaded;
        PrimaryShellTreeView.SelectionChanged += NativeTreeView_SelectionChanged;
        SecondaryShellTreeView.SelectionChanged += NativeTreeView_SelectionChanged;

        // Navigate(PrimaryShellTreeView.Items[0] as ShellBrowserItem);
        // . PrimaryShellTreeView.Items.Add(new ShellBrowserItem(ShellFolder.Desktop.PIDL, true));
        // . SecondaryShellTreeView.Items.Add(new ShellBrowserItem(ShellFolder.Desktop.PIDL, true));
    }

    private void ExplorerBrowser_Loading(FrameworkElement sender, object args)
    {
        //var initialNavigationTarget = ShellBrowserItem.HomeShellFolder();
    }
    private void ExplorerBrowser_Loaded(object sender, RoutedEventArgs e)
    {
//        if (PrimaryShellTreeView.Items[0] is ShellBrowserItem initialNavigationTarget)
//        {
//            _ = Navigate(initialNavigationTarget, PrimaryShellTreeView);
//            initialNavigationTarget.TreeViewItemIsSelected = true;  // TODO: Bind property to TreeViewItem.IsSelected
//        }
    }

    private void NativeTreeView_SelectionChanged(object sender, TreeViewSelectionChangedEventArgs e)
    {
        var owner = sender as ShellNamespaceTreeControl;
        var addedItems = e.AddedItems;
        var removedItems = e.RemovedItems;

        Debug.WriteIf((addedItems.Count < 1 || removedItems.Count < 1), "None or less Items added nor removed", ".NativeTreeView_SelectionChanged() parameter mismatch.");

        foreach (var item in addedItems)
        {
            // TODO: Add folders and folder content to ShellListView and group by folder
            Debug.Print($".NativeTreeView_SelectionChanged(`{item?.ToString()}`) added.");
        }

        // Misc Debris:
        var selectedFolder = addedItems[0] as ShellBrowserItem;
        var currentTreeNode = owner?.SelectedItem as TreeViewNode;
        if (currentTreeNode != null)
        {
            Debug.Print($".NativeTreeView_SelectionChanged(`{selectedFolder?.DisplayName}`, treeNode: {currentTreeNode?.ToString()}).");
        }

        // var currentTreeNode = PrimaryShellTreeView.NativeTreeView.SelectedItem;
        // Items.Add(rootItem);

        Debug.Print($".NativeTreeView_SelectionChanged(`{selectedFolder?.DisplayName}`, treeNode: {currentTreeNode?.ToString()}).");

        // check sender!
        // TODO: ShellTreeView.NativeTreeView.SelectedItem = newTreeNode(find TreeNode

        if (selectedFolder?.PIDL is null)
        {
            Debug.Print(".NativeTreeView_SelectionChanged(): selectedFolder.PIDL is null!");
            return;
        }

        // => TODO: currentTreeNode as TreeViewNode ; (owner as ShellNamespaceTreeControl)
        _ = Navigate(selectedFolder, owner);
    }

    /*
    public void Navigate(ShellItem? shellItem,
        IExplorerBrowser.ExplorerBrowser.ExplorerBrowserNavigationItemCategory category =
            IExplorerBrowser.ExplorerBrowser.ExplorerBrowserNavigationItemCategory.Default)
    {
        Debug.Assert(shellItem != null);

        Debug.WriteLineIf(!shellItem.IsFolder, $"Navigate({shellItem.ToString()}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty list view with error message

        // TODO: Find TreeItem here!
        BrowserItem targetItem = new(shellItem.PIDL, null, null);
        _currentNavigationTask = Navigate(targetItem);
    }     
     */

    internal async Task<HRESULT> Navigate(ShellBrowserItem target, ShellNamespaceTreeControl shTreeControl)
    {
        var shTargetItem = target.ShellItem;
        Debug.Assert(shTargetItem is not null);
        // TODO: If no folder, or drive empty, etc... show empty list view with error message

        // TODO: init ShellNamespaceService
        try
        {
            //            if (_currentNavigationTask is { IsCompleted: false })
            //          {
            //              Debug.Print("ERROR! <_currentNavigationTask> already running");
            //              // cancel current task
            //              //CurrentNavigationTask
            //          }

            IsLoading = true;

            if (target.ChildItems.Count <= 0)
            {
                using var shFolder = new ShellFolder(target.ShellItem);

                target.ChildItems.Clear();
                //                ShellListView.Items.Clear();
                foreach (var child in shFolder)
                {
                    var shStockIconId = child.IsFolder
                        ? Shell32.SHSTOCKICONID.SIID_FOLDER
                        : Shell32.SHSTOCKICONID.SIID_DOCASSOC;

                    // TODO: check if item is a link. Will cause exception if not a link
                    // SHSTOCKICONID.Link and SHSTOCKICONID.SlowFile have to be used as overlay
                    // var softBitmap = await StockIconFactory.GetStockIconBitmapSource(shStockIconId);

                    var ebItem = new ShellBrowserItem(child.PIDL, child.IsFolder)
                    {
                        //                        SoftwareBitmap = softBitmap
                    };

                    // TODO: if(child.IsLink) => Add Link-Overlay

                    target.ChildItems.Add(ebItem);
                    // TODO: Update PrimaryShellListView.Items => target.ChildItems with asynchronous loading;
                }


                // TODO: Update PrimaryShellListView.Items => target.ChildItems with asynchronous loading;
            }
            else
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
            }

            //if (shTreeControl == PrimaryShellTreeView)
            //{
            //    var shListView = PrimaryShellListView;
            //    shListView.Items = new();

            //    foreach (var child in target.ChildItems)
            //    {
            //        var ebItem = child as ShellBrowserItem;
            //        if (ebItem is not null)
            //        {
            //            shListView.Items.Add(ebItem);
            //        }
            //    }
            //}
            //else if (shTreeControl == SecondaryShellTreeView)
            //{
            //    var shListView = PrimaryShellListView;
            //    shListView.Items = new();

            //    foreach (var child in target.ChildItems)
            //    {
            //        var ebItem = child as ShellBrowserItem;
            //        if (ebItem is not null)
            //        {
            //            shListView.Items.Add(ebItem);
            //        }
            //    }
            //}
            // TODO: Load folder-open icon and overlays
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{target}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");

            return new HRESULT(comEx.HResult);
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{target}>) failed, reason unknown: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
        }

        return HRESULT.S_OK;
    }
}
