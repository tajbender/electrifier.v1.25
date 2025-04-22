using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using electrifier.Controls.Helpers;
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
using static Vanara.PInvoke.Shell32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ShellNamespaceTreeControl : UserControl
{
    private TreeView NativeTreeView => TreeView;
    internal ObservableCollection<ShellBrowserItem> Items;
    //    internal readonly AdvancedCollectionView AdvancedCollectionView;
    //    public static ShellNamespaceService NamespaceService => App.GetService<ShellNamespaceService>();

    public bool AutoExpandAfterSelection
    {
        get;
        set;
    }
    public TreeViewNode? SelectedItem => NativeTreeView.SelectedNode as TreeViewNode;


    //    public event TypedEventHandler<ShellNamespaceTreeControl, TreeViewNode> SelectionChanged
    //    {
    //        add => NativeTreeView.SelectionChanged += value;
    //        remove => NativeTreeView.SelectionChanged -= value;
    //    }
    //public event EventHandler FolderItemsChanged
    //{
    //    add => ;
    //    remove => ;
    //}

    public ShellNamespaceTreeControl()
    {
        InitializeComponent();
        DataContext = this;
        Items = [];


        /*  // SelectionChanged = (sender, e) =>
            //{
            //    if (e.AddedItems.Count > 0)
            //    {
            //        if (e.AddedItems[0] is ShellBrowserItem item)
            //        {
            //            var args = new SelectionChangedEventArgs(Array.Empty<object>(), Array.Empty<object>());
            //            SelectionChanged(this, args);
            //        }
            //    }
            //}; */

        //AdvancedCollectionView = new AdvancedCollectionView(Items, true);
        //NativeTreeView.ItemsSource = AdvancedCollectionView;

        //rootItem.TreeViewItemIsSelected = true; TODO: Concatenate with reference to rootItem of TreeView... rootItem.TreeViewItemIsExpanded = true;
        //SelectedItem = rootItem;
        //NativeTreeView.SelectedItem = rootItem;
        //rootItem.EnumChildItems();
        //rootItem.TreeViewItemIsSelected = true;

        Loading += OnShellNamespaceTreeControlLoading;
        NativeTreeView.SelectionChanged += OnNativeTreeViewSelectionChanged;
    }


    private void OnShellNamespaceTreeControlLoading(FrameworkElement sender, object args)
    {
        Items.Add(new ShellBrowserItem(ShellFolder.Desktop.PIDL, isFolder: true));
        // TODO: Add separator and add this as child items of the rootItem as second view option
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_SkyDrive));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Downloads));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Documents));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Pictures));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Music));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Videos));
    }


    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);
    public event NavigatedEventHandler Navigated;

    private void OnNativeTreeViewSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
    {
        var addedItems = e.AddedItems;
        var removedItems = e.RemovedItems;

        Debug.WriteIf((addedItems.Count < 1 && removedItems.Count < 1), "None or less Items added nor removed", ".OnNativeTreeViewSelectionChanged() parameter mismatch.");

        foreach (var item in addedItems)
        {
            // TODO: Add folders and folder content to ShellListView and group by folder
            Debug.Print($".OnNativeTreeViewSelectionChanged(Item `{item?.ToString()}`) has been added to TreeView' selected items.");
        }
        foreach (var item in removedItems)
        {
            // TODO: Add folders and folder content to ShellListView and group by folder
            Debug.Print($".OnNativeTreeViewSelectionChanged(Item `{item?.ToString()}`) has been deselected.");
        }

        var selectedNode = addedItems[0];
        if (selectedNode is null)
        {
            Debug.Print(".OnNativeTreeViewSelectionChanged(): selectedNode is null!");
            return;
        }
        if (selectedNode is not ShellBrowserItem shellBrowserItem)
        {
            Debug.Print(".OnNativeTreeViewSelectionChanged(): shellBrowserItem is null!");
            return;
        }
        var selectedFolder = new ShellFolder(shellBrowserItem.ShellItem);
        Navigated?.Invoke(this, new NavigatedEventArgs(selectedFolder));
        //Navigated?.BeginInvoke(this, shellBrowserItem, null, null);
    }
}
