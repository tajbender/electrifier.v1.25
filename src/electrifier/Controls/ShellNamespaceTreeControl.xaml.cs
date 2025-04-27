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

    public bool AutoExpandAfterSelection
    {
        get;
        set;
    }
    public TreeViewNode? SelectedItem => NativeTreeView.SelectedNode as TreeViewNode;

    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);
    public event NavigatedEventHandler? Navigated;

    // todo: public event TypedEventHandler<ShellNamespaceTreeControl, TreeViewNode> SelectionChanged
    // todo: public event EventHandler FolderItemsChanged

    public ShellNamespaceTreeControl()
    {
        InitializeComponent();
        DataContext = this;
        Items = [];

        Loading += OnLoading;
        NativeTreeView.SelectionChanged += OnSelectionChanged;
    }

    private void OnLoading(FrameworkElement sender, object args)
    {
        // TODO: Items.Add(new ShellBrowserItem(/* Home */, isFolder: true));
        // TODO: Items.Add(new ShellBrowserItem(/* Gallery */, isFolder: true));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_SkyDrive));
        // TODO: Add separator and add this as child items of the rootItem as second view option
        Items.Add(new ShellBrowserItem(ShellFolder.Desktop.PIDL, isFolder: true));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Downloads));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Documents));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Pictures));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Music));
        Items.Add(BrowserItemFactory.FromKnownFolderId(Shell32.KNOWNFOLDERID.FOLDERID_Videos));

        Navigated?.Invoke(this, new NavigatedEventArgs(new ShellFolder(Items[0].ShellItem)));
    }

    private void OnSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
    {
        var addedItems = e.AddedItems;
        var removedItems = e.RemovedItems;

        Debug.WriteIf((addedItems.Count < 1 && removedItems.Count < 1), "None or less Items added nor removed", ".OnSelectionChanged() parameter mismatch.");
        if (addedItems[0] is null)
        {
            Debug.Fail(".OnSelectionChanged(): selectedNode is null!");
            return;
        }
        if (addedItems[0] is not ShellBrowserItem shellBrowserItem)
        {
            Debug.Fail(".OnSelectionChanged(): shellBrowserItem is null!");
            return;
        }
        Navigated?.Invoke(this, new NavigatedEventArgs(new ShellFolder(shellBrowserItem.ShellItem)));
        //Navigated?.BeginInvoke(this, shellBrowserItem, null, null);
    }
}
