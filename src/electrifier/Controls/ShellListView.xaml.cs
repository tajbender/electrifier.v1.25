using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.WinUI.Collections;
using electrifier.Controls.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Vanara.Windows.Shell;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ShellListView : UserControl
{
    internal ItemsView NativeItemsView => ItemsView;

    public ObservableCollection<ShellBrowserItem> Items
    {
        get;
    }

    public readonly AdvancedCollectionView AdvancedCollectionView;

    public delegate void NavigatedEventHandler(object sender, NavigatedEventArgs e);
    public event NavigatedEventHandler? Navigated;

    public ShellListView()
    {
        InitializeComponent();
        DataContext = this;

        Items = [];
        AdvancedCollectionView = new AdvancedCollectionView(Items, true);
        AdvancedCollectionView.SortDescriptions.Add(new SortDescription(SortDirection.Ascending));
        Debug.Assert(NativeItemsView != null, nameof(NativeItemsView) + " != null");
        NativeItemsView.ItemsSource = AdvancedCollectionView;
    }

    public void AddItem(ShellBrowserItem shellBrowserItem) => Items.Add(shellBrowserItem);

    public void ClearItems()
    {
        using (AdvancedCollectionView.DeferRefresh())
        {
            Items.Clear();
        }
    }

    private void ItemsView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (!e.Handled)
        {
            try
            {
                var shellBrowserItem = (NativeItemsView.SelectedItem) as ShellBrowserItem;
                var shellItem = shellBrowserItem?.ShellItem;
                Debug.Assert(shellItem != null, nameof(shellItem) + " != null");
                if (shellItem.IsFolder)
                {
                    var shFolder = new ShellFolder(shellItem);
                    if (shFolder != null)
                    {
                        Navigated?.Invoke(this, new NavigatedEventArgs(shFolder));
                        //Navigated?.BeginInvoke(this, item, null, null);
                        e.Handled = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Fail(exception.ToString());
                throw;
            }
        }
    }
}
