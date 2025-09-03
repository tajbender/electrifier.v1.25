using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using electrifier.Controls.Helpers;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace electrifier.Controls;

public sealed partial class ExplorerBrowser : UserControl
{
    public event EventHandler<NavigatedEventArgs> Navigated;
    public event EventHandler<NavigatingEventArgs> Navigating;
    public event EventHandler<NavigationFailedEventArgs> NavigationFailed;

    public readonly ShellNavigationHistory History = new();
    //private Vanara.Windows.Shell.NavigationLogDirection _navigationLogDirection;
    //private Vanara.Windows.Shell.ShellBrowserViewMode _viewMode = ShellBrowserViewMode.Details;
    /// <summary>Fires when the Items collection changes.</summary>
    public event EventHandler? ItemsChanged;


    /// <summary>Fires when the SelectedItems collection changes.</summary>
    public event EventHandler? SelectionChanged;


    /*
     public enum NavigationLogDirection
{
	/// <summary>Navigates forward through the navigation log</summary>
	Forward,

	/// <summary>Navigates backward through the travel log</summary>
	Backward
}

     
     
     *      var e = new CurrentChangingEventArgs();
            OnCurrentChanging(e);
            if (e.Cancel)
            {
                return false;
            }

            CurrentPosition = i;
            OnCurrentChanged(null!);
            return true;
     */

    private Task<HRESULT>? _currentNavigationTask;
    private bool _isLoading;


    public ExplorerBrowser()
    {
        InitializeComponent();
        DataContext = this;

        PrimaryShellTreeView.Navigated += PrimaryShellTreeView_Navigated;
        PrimaryShellListView.Navigated += PrimaryShellTreeView_Navigated;
        SecondaryShellTreeView.Navigated += SecondaryShellTreeView_Navigated;
        SecondaryShellListView.Navigated += SecondaryShellTreeView_Navigated;
    }

    internal async Task<HRESULT> Navigate(ShellBrowserItem target)
    {
        var shTargetItem = target.ShellItem;
        var isRunning = true;

        Debug.WriteLineIf(!shTargetItem.IsFolder, $".WARN: Navigate({target.DisplayName}) => is not a folder!");
        // TODO: If no folder, or drive empty, etc... show empty listview with error message

        // TODO: init ShellNamespaceService
        try
        {
            if (_currentNavigationTask is { IsCompleted: false })
            {
                Debug.Print("ERROR! <_currentNavigationTask> already running");
                // cancel current task
                //CurrentNavigationTask
            }

            PrimaryShellListView.SetItemSource(target.ChildItems);

            if(target.ChildItems.Count > 0)
            {
                Debug.Print($".Navigate({target.DisplayName}) => already has {target.ChildItems.Count} items, skipping reload.");
                return HRESULT.S_OK;
            }

            // WARN: See https://share.google/zWhwyyuhim50ur8Ph     // https://medium.com/@a.lyskawa/the-hitchhiker-guide-to-asynchronous-events-in-c-e9840109fb53


            ShellFolder shFolder = new(shTargetItem);
            var icnExtractor = new ShellIconExtractor(shFolder, bmpSize: 64);  //new[] { shTargetItem }, 32, true);
            icnExtractor.IconExtracted += (sender, args) =>
            {
                var shItem = new ShellItem(args.ItemID);
                var ebItem = new ShellBrowserItem(shItem);
                target.ChildItems?.Add(ebItem);
                //DispatcherQueue.TryEnqueue(() =>
                //{
                //    PrimaryShellListView.AddItem(ebItem);
                //});
            };
            icnExtractor.Complete += (sender, args) =>
            {
                isRunning = false;
                var cnt = target.ChildItems?.Count ?? 0;
                Debug.Print($".Navigate({target.DisplayName}) => .IconExtOnComplete(): {cnt} items");
                //PrimaryShellListView.SetItemSource(target.ChildItems);
                //if (GridViewVisibility == Microsoft.UI.Xaml.Visibility.Visible)
                //{
                //    Debug.Print($".GridViewVisibility = {Microsoft.UI.Xaml.Visibility.Visible}");
                //    ShellGridView.SetItems(CurrentFolderItems);
                //}
            };


            icnExtractor.Start();

            while (isRunning)
            {
                using (PrimaryShellListView.AdvancedCollectionView.DeferRefresh())
                {
                    await Task.Delay(50);
                }
            }
            PrimaryShellListView.SetItemSource(target.ChildItems);
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
            //IsLoading = false;
        }

        return HRESULT.S_OK;
    }


    private async void PrimaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".PrimaryShellTreeView_Navigated() to {e.NewLocation.Name}");

        //var tnode = e.NewLocation;
        var treeNode = PrimaryShellTreeView.SelectedItem;
        var cnt = treeNode?.Content;
        //treeNode.IsSelected = true;



        
        _ = await Navigate(new ShellBrowserItem(e.NewLocation));  // WARN: This is a fire-and-forget call, no await! // WARN: Use existing ShellBrowserItem from TreeView
    }

    private async void SecondaryShellTreeView_Navigated(object sender, NavigatedEventArgs e)
    {
        Debug.Print($".SecondaryShellTreeView_Navigated() to {e.NewLocation.Name}");
        SecondaryShellListView.ClearItems();

        try
        {
            var rootItem = new ShellFolder(e.NewLocation.PIDL);

            var childItems = rootItem?.EnumerateChildren(FolderItemFilter.Folders | FolderItemFilter.NonFolders | FolderItemFilter.IncludeHidden);
            if (childItems == null)
            {
                Debug.Fail($"[Error] Navigate(<{e.NewLocation.Name}>) failed. No items found.");
                return;
            }

            var newBrowserItems = new List<ShellBrowserItem>();
            foreach (var item in childItems)
            {
                newBrowserItems.Add(new ShellBrowserItem(new(item.PIDL)));
            }

            SecondaryShellListView.AddItems(newBrowserItems);
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
