using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using electrifier.Controls.Contracts;
using electrifier.Controls.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.PInvoke;
using Vanara.Windows.Shell;
using Windows.Graphics.Imaging;
using static Vanara.PInvoke.Shell32;

namespace electrifier.Controls.Helpers;

public class BrowserItemFactory
{
    public static ShellBrowserItem FromPIDL(Shell32.PIDL pidl, List<ShellBrowserItem>? childItems = null) => new(new ShellItem(pidl), childItems);
    public static ShellBrowserItem FromKnownFolderId(Shell32.KNOWNFOLDERID knownFolderId) => new(new ShellFolder(knownFolderId));
    public static ShellBrowserItem FromShellFolder(ShellFolder shellFolder) => FromPIDL(shellFolder.PIDL);
    public static ShellBrowserItem HomeShellFolder()
    {
        using var homeShellFolder = new ShellItem("shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}");
        return new ShellBrowserItem(homeShellFolder);
    }
}

// TODO: IDisposable
// TODO: IComparable
[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public partial class ShellBrowserItem : AbstractBrowserItem<ShellItem>, INotifyPropertyChanged
{
    public string DisplayName => ShellItem.GetDisplayName(ShellItemDisplayString.NormalDisplay) ?? ShellItem.ToString();
    public Shell32.PIDL PIDL => ShellItem.PIDL;
    public ShellItem ShellItem;
    public SoftwareBitmapSource? SoftwareBitmap;
    public SoftwareBitmapSource? OverlaySoftwareBitmap;
    public ShellItemAttribute Attributes => ShellItem.Attributes;
    private bool _isSelected;

    public readonly new List<ShellBrowserItem> ChildItems;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value == _isSelected)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// HasUnrealizedChildren checks for flag ´SFGAO_HASSUBFOLDER´.
    ///
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/shell/sfgao"/>
    /// The specified folders have subfolders. The SFGAO_HASSUBFOLDER attribute is only advisory and might be returned by Shell folder implementations even if they do not contain subfolders. Note, however, that the converse—failing to return SFGAO_HASSUBFOLDER—definitively states that the folder objects do not have subfolders.
    /// Returning SFGAO_HASSUBFOLDER is recommended whenever a significant amount of time is required to determine whether any subfolders exist. For example, the Shell always returns SFGAO_HASSUBFOLDER when a folder is located on a network drive.
    /// </summary>
    public bool HasUnrealizedChildren => Attributes.HasFlag(ShellItemAttribute.HasSubfolder);
    public new bool IsFolder => ShellItem.IsFolder;
    //public bool IsHidden => Attributes.HasFlag(ShellItemAttribute.Hidden);
    //public bool IsLink => ShellItem.IsLink;

    // TODO: Listen for ShellItem Property changes

    public ShellBrowserItem(ShellItem shItem,
        List<ShellBrowserItem> childItems = null) : base()
    {
        ShellItem = shItem;
        ChildItems = childItems ?? [];

        //ChildItems = childItems ?? []; note: base ctor
        //SoftwareBitmap = ConfiguredTaskAwaitable GetStockIconBitmapSource()


        // if IsHidden... do overlay
        // is IsLink... do overlay
        Shell32.SHSTOCKICONID shStockIconId;
        shStockIconId = IsFolder
            ? Shell32.SHSTOCKICONID.SIID_FOLDER
            : Shell32.SHSTOCKICONID.SIID_DOCASSOC;
        _ = GetStockIconBitmapAsync(shStockIconId);

    }

    private async Task<SoftwareBitmapSource> GetStockIconOverlayBitmapAsync(Shell32.SHSTOCKICONID stockIconId)
    {
        var softwareBitmapSource = await Shel32NamespaceService.GetStockIconBitmapSource(stockIconId);
        SetField(ref OverlaySoftwareBitmap, softwareBitmapSource, nameof(OverlaySoftwareBitmap));

        return softwareBitmapSource;
    }

    private async Task<SoftwareBitmapSource> GetStockIconBitmapAsync(Shell32.SHSTOCKICONID stockIconId)
    {
        var softwareBitmapSource = await Shel32NamespaceService.GetStockIconBitmapSource(stockIconId);
        SetField(ref SoftwareBitmap, softwareBitmapSource, nameof(SoftwareBitmap));

        return softwareBitmapSource;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
