using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.PInvoke;
using Vanara.Windows.Shell;
using electrifier.Controls.Contracts;
using static Vanara.PInvoke.Shell32;

namespace electrifier.Controls.Helpers;

public class BrowserItemFactory
{
    public static ShellBrowserItem FromPIDL(Shell32.PIDL pidl, bool? isFolder, List<AbstractBrowserItem<ShellItem>>? childItems = null) => new(pidl, isFolder, childItems);
    public static ShellBrowserItem FromKnownFolderId(Shell32.KNOWNFOLDERID knownFolderId) => new(new ShellFolder(knownFolderId).PIDL, isFolder: true);
    public static ShellBrowserItem FromShellFolder(ShellFolder shellFolder) => FromPIDL(shellFolder.PIDL, isFolder: true);
    public static ShellBrowserItem HomeShellFolder()
    {
        using var homeShellFolder = new ShellItem("shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}");
        return new ShellBrowserItem(homeShellFolder.PIDL, isFolder: true);
    }
}

    // TODO: IDisposable
    // TODO: IComparable
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public partial class ShellBrowserItem : AbstractBrowserItem<ShellItem>, INotifyPropertyChanged
    {
        public string DisplayName => ShellItem.GetDisplayName(ShellItemDisplayString.NormalDisplay) ?? ShellItem.ToString();
        public readonly Shell32.PIDL PIDL;
        public ShellItem ShellItem;
        public SoftwareBitmapSource? SoftwareBitmap;
        private bool _isSelected;

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
        public bool HasUnrealizedChildren => ShellItem.Attributes.HasFlag(ShellItemAttribute.HasSubfolder);

        // TODO: Listen for ShellItem Property changes
        public ShellBrowserItem(Shell32.PIDL pidl, bool? isFolder,
            List<AbstractBrowserItem<ShellItem>>? childItems = null) : base(isFolder, childItems ?? [])
        {
            PIDL = new Shell32.PIDL(pidl);
            ShellItem = new ShellItem(pidl);
            //ChildItems = childItems ?? []; note: base ctor
            //SoftwareBitmap = ConfiguredTaskAwaitable GetStockIconBitmapSource()
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
