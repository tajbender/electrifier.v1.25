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
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.Shell32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

/// <summary>
/// <see href="Microsoft.WindowsAPICodePack.Controls.ExplorerBrowser">ExplorerBrowser</see> like control for WinUI 3 and Windows App SDK.
/// Microsoft.WindowsAPICodePack.Controls.ExplorerBrowser
/// <see href="https://github.com/dahall/Vanara">Vanara</see>.
/// <see href="Microsoft.WindowsAPICodePack.Controls.ExplorerBrowser"/>
/// <see cref="electrifier.Controls.ShellNamespaceTreeControl"/>
/// <see cref="electrifier.Controls.ShellListView"/> 
/// for navigating through the shell namespace.
/// </summary>
public sealed partial class ExplorerBrowser : UserControl
{

    public ObservableCollection<ShellBrowserItem> CurrentItems;
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

    private async void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        PrimaryShellListView.Items.Clear();

        var items = new ShellFolder(e.NewLocation.PIDL).EnumerateChildren(FolderItemFilter.Storage);
        foreach (var item in items)
        {
            var shStockIconId = item.IsFolder
                ? Shell32.SHSTOCKICONID.SIID_FOLDER
                : Shell32.SHSTOCKICONID.SIID_DOCASSOC;

            // TODO: check if item is a link. Will cause exception if not a link
            // SHSTOCKICONID.Link and SHSTOCKICONID.SlowFile have to be used as overlay
            // var softBitmap = await StockIconFactory.GetStockIconBitmapSource(shStockIconId);

            var softBitmap = await Shel32NamespaceService.GetStockIconBitmapSource(shStockIconId);

            var ebItem = new ShellBrowserItem(item.PIDL, item.IsFolder)
            {
                SoftwareBitmap = softBitmap
            };

            // TODO: shNamespaceService.RetrieveChildItemsAsync().select()...;
            PrimaryShellListView.Items.Add(ebItem);
        }
    }

    private async void SecondaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".SecondaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        SecondaryShellListView.Items.Clear();

        var items = new ShellFolder(e.NewLocation.PIDL).EnumerateChildren(FolderItemFilter.Storage);
        foreach (var item in items)
        {
            var shStockIconId = item.IsFolder
                ? Shell32.SHSTOCKICONID.SIID_FOLDER
                : Shell32.SHSTOCKICONID.SIID_DOCASSOC;

            // TODO: check if item is a link. Will cause exception if not a link
            // SHSTOCKICONID.Link and SHSTOCKICONID.SlowFile have to be used as overlay
            // var softBitmap = await StockIconFactory.GetStockIconBitmapSource(shStockIconId);

            var softBitmap = await Shel32NamespaceService.GetStockIconBitmapSource(shStockIconId);

            var ebItem = new ShellBrowserItem(item.PIDL, item.IsFolder)
            {
                SoftwareBitmap = softBitmap
            };

            // TODO: shNamespaceService.RetrieveChildItemsAsync().select()...;
            SecondaryShellListView.Items.Add(ebItem);
        }
    }

    internal async Task<HRESULT> Navigate(ShellBrowserItem target, ShellListView shListView)
    {
        var shTargetItem = target.ShellItem;
        Debug.Assert(shTargetItem is not null);
        // TODO: If no folder, or drive empty, etc... show empty list view with error message

        try
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

        return HRESULT.S_OK;
    }
}
