using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using electrifier.Controls.Helpers;
using Vanara.Windows.Shell;

namespace electrifier.Controls.Services;

// TODO:
// WARN: Add Shell Log-Writer class, for logging Shell32 operations

/// <summary>
/// A service for interacting with the Shell32 namespace.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class Shel32NamespaceService
{
    public ObservableCollection<ShellBrowserItem> Enumerate(ShellItem shellItem)
    {
        var result = new ObservableCollection<ShellBrowserItem>();
        // Enumerate child items
        var shFolder = new ShellFolder(shellItem);

        foreach (var item in shFolder.EnumerateChildren(FolderItemFilter.Storage))
        {
            // Create a new ShellBrowserItem for each child item
            var browserItem = BrowserItemFactory.FromPIDL(item.PIDL, item.IsFolder);
            result.Add(browserItem);
        }
        return result;
        //return new ObservableCollection<ShellBrowserItem>(shellItem.EnumChildItems()
        //    .Select(item => BrowserItemFactory.FromPIDL(item, item.IsFolder, null))
        //    .ToList());
    }

    private string GetDebuggerDisplay() => $"{nameof(Shel32NamespaceService)}>";
}
