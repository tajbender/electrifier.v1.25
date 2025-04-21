using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public ObservableCollection<ShellBrowserItem> CurrentItems;


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

        //var initialNavigationTarget = ShellBrowserItem.HomeShellFolder();

        Loading += ExplorerBrowser_Loading;
        Loaded += ExplorerBrowser_Loaded;

        PrimaryShellTreeView.Navigated += PrimaryShellTreeView_Navigated;
        SecondaryShellTreeView.Navigated += SecondaryShellTreeView_Navigated;
        //PrimaryShellTreeView.SelectedItemChangedEventHandler += OnPrimaryShellTreeViewSelectedItemChanged;
        //        PrimaryShellTreeView.SelectionChanged += PrimaryTreeViewSelectionChanged;
        //        SecondaryShellTreeView.SelectionChanged += SecondaryTreeViewSelectionChanged;
        //
        // Navigate(PrimaryShellTreeView.Items[0] as ShellBrowserItem);
        // . PrimaryShellTreeView.Items.Add(new ShellBrowserItem(ShellFolder.Desktop.PIDL, true));
        // . SecondaryShellTreeView.Items.Add(new ShellBrowserItem(ShellFolder.Desktop.PIDL, true));
    }

    private void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        PrimaryShellListView.Items.Clear();

        var items = new ShellFolder(e.NewLocation.PIDL).EnumerateChildren(FolderItemFilter.Storage);
        foreach (var item in items)
        {
            // TODO: shNamespaceService.RetrieveChildItemsAsync().select()...;
            PrimaryShellListView.Items.Add(new ShellBrowserItem(item.PIDL, null));
        }
    }
    private void SecondaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".SecondaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        SecondaryShellListView.Items.Clear();

        var items = new ShellFolder(e.NewLocation.PIDL).EnumerateChildren(FolderItemFilter.Storage);
        foreach (var item in items)
        {
            // TODO: shNamespaceService.RetrieveChildItemsAsync().select()...;
            SecondaryShellListView.Items.Add(new ShellBrowserItem(item.PIDL, null));
        }
    }

    private void ExplorerBrowser_Loading(FrameworkElement sender, object args)
    {
        // Enumerate the root items of the shell namespace
    }
    private void ExplorerBrowser_Loaded(object sender, RoutedEventArgs e)
    {
//        if (PrimaryShellTreeView.Items[0] is ShellBrowserItem initialNavigationTarget)
//        {
//            _ = Navigate(initialNavigationTarget, PrimaryShellTreeView);
//            initialNavigationTarget.TreeViewItemIsSelected = true;  // TODO: Bind property to TreeViewItem.IsSelected
//        }
    }

    //private void PrimaryTreeViewSelectionChanged(object sender, TreeViewSelectionChangedEventArgs e)
    //{
    //    NativeTreeView_SelectionChanged(PrimaryShellTreeView, PrimaryShellListView, e);
    //}

    //private void SecondaryTreeViewSelectionChanged(object sender, TreeViewSelectionChangedEventArgs e)
    //{
    //    NativeTreeView_SelectionChanged(SecondaryShellTreeView, SecondaryShellListView, e);
    //}
    private void OnPrimaryShellTreeViewSelectedItemChanged(ShellNamespaceTreeControl sender, ShellBrowserItem args)
    {
    }

    private void NativeTreeView_SelectionChanged(ShellNamespaceTreeControl senderTreeView, ShellListView shListView, TreeViewSelectionChangedEventArgs e)
    {
        var addedItems = e.AddedItems;
        var removedItems = e.RemovedItems;

        Debug.WriteIf((addedItems.Count < 1 && removedItems.Count < 1), "None or less Items added nor removed", ".NativeTreeView_SelectionChanged() parameter mismatch.");

        foreach (var item in addedItems)
        {
            // TODO: Add folders and folder content to ShellListView and group by folder
            Debug.Print($".NativeTreeView_SelectionChanged(Item `{item?.ToString()}`) has been added to TreeView' selected items.");
        }
        foreach (var item in removedItems)
        {
            // TODO: Add folders and folder content to ShellListView and group by folder
            Debug.Print($".NativeTreeView_SelectionChanged(Item `{item?.ToString()}`) has been deselected.");
        }

        var selectedNode = addedItems[0];
        if (selectedNode is null)
        {
            Debug.Print(".NativeTreeView_SelectionChanged(): selectedNode is null!");
            shListView.Items.Clear();
            return;
        }
        if (selectedNode is not ShellBrowserItem shellBrowserItem)
        {
            Debug.Print(".NativeTreeView_SelectionChanged(): shellBrowserItem is null!");
            shListView.Items.Clear();
            return;
        }

        _ = Navigate(shellBrowserItem, shListView);
    }

    internal async Task<HRESULT> Navigate(ShellBrowserItem target, ShellListView shListView)
    {
        var shTargetItem = target.ShellItem;
        Debug.Assert(shTargetItem is not null);
        // TODO: If no folder, or drive empty, etc... show empty list view with error message

        try
        {
            IsLoading = true;

            if (target.ChildItems.Count > 0)
            {
                Debug.WriteLine(".Navigate() => Cache hit!");
                shListView.Items.Clear();
                foreach (var child in target.ChildItems)
                {
                    var ebItem = child as ShellBrowserItem;
                    if (ebItem is not null)
                    {
                        shListView.Items.Add(ebItem);
                    }
                }
            }
            else
            {
                using var shFolder = new ShellFolder(shTargetItem);

                target.ChildItems.Clear();
                shListView.Items.Clear();
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
                    shListView.Items.Add(ebItem);
                    // TODO: Update PrimaryShellListView.Items => target.ChildItems with asynchronous loading;
                }
                // TODO: Update PrimaryShellListView.Items => target.ChildItems with asynchronous loading;
            }
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
