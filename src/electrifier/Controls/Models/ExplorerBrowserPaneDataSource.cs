using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.Windows.Shell;

namespace electrifier.Controls.Models;

internal record ExplorerBrowserPaneDataSource(ShellItem ShellItem
, string? ExplicitDisplayName
, IEnumerable<ShellItem> Items
) : INotifyPropertyChanged
{
    public string DisplayName => ExplicitDisplayName ?? ShellItem.Name ?? ToString();
    public string? ExplicitDisplayName
    {
        get;
    } = ExplicitDisplayName;
    public IEnumerable<ShellItem> Items
    {
        get; set;
    } = Items;

    public ExplorerBrowserPaneDataSource(ShellItem shellItem) : this(shellItem, null, []) { }

    public event PropertyChangedEventHandler? PropertyChanged;
}
