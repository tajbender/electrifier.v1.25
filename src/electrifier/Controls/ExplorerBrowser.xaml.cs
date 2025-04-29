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

public sealed partial class ExplorerBrowser : UserControl
{
    public ObservableCollection<ShellBrowserItem> CurrentItems;
    public event EventHandler<Vanara.Windows.Shell.NavigatedEventArgs> Navigated;
    public event EventHandler<Vanara.Windows.Shell.NavigationFailedEventArgs> NavigationFailed;
    
    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        PrimaryShellTreeView.Navigated += PrimaryShellTreeView_Navigated;
        PrimaryShellListView.Navigated += PrimaryShellTreeView_Navigated;
        SecondaryShellTreeView.Navigated += SecondaryShellTreeView_Navigated;
        SecondaryShellListView.Navigated += SecondaryShellTreeView_Navigated;
    }

    private async void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        PrimaryShellListView.ClearItems();

        try
        {
            var items = new ShellFolder(e.NewLocation.PIDL).EnumerateChildren(FolderItemFilter.Storage);
            foreach (var item in items)
            {
                PrimaryShellListView.AddItem(new ShellBrowserItem(item.PIDL, item.IsFolder));
            }
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{e.NewLocation.Name}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");
            //NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(comEx));
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed, reason unknown: {ex.Message}");
            throw;
        }
    }

    private async void SecondaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".SecondaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        SecondaryShellListView.ClearItems();

        try
        {
            var items = new ShellFolder(e.NewLocation.PIDL).EnumerateChildren(FolderItemFilter.Storage);
            foreach (var item in items)
            {
                SecondaryShellListView.AddItem(new ShellBrowserItem(item.PIDL, item.IsFolder));
            }
        }
        catch (COMException comEx)
        {
            Debug.Fail(
                $"[Error] Navigate(<{e.NewLocation.Name}>) failed. COMException: <HResult: {comEx.HResult}>: `{comEx.Message}`");
            //NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(comEx));
        }
        catch (Exception ex)
        {
            Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed, reason unknown: {ex.Message}");
            throw;
        }
    }
}
