﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Search;

namespace electrifier.Services;

internal static class DosShellItemHelpers
{
    /// <summary>
    /// <br>
    /// <see href="DefaultUnknownFileIcon"/> for the default unknown file icon.
    /// </br>
    /// <br>
    /// <see cref="ImageIcon"/> for the default unknown file icon.
    /// </br>
    /// </summary>
#pragma warning disable CS8601 // Possible null reference assignment.
    public static readonly ImageIcon DefaultFolderIcon = defaultFolderImageIcon;
    public static readonly ImageIcon DefaultUnknownFileIcon = defaultUnknownFileImageIcon;
#pragma warning restore CS8601 // Possible null reference assignment.

    // TODO: Convert to array and use constants as index
    public const string shell32DefaultFolderIcon = "ms-appx:///Assets/Views/Workbench/Shell32 Default Folder.ico";
    public const string shell32FolderContainingFileIcon = "ms-appx:///Assets/Views/Workbench/Shell32 Folder containing File.ico";
    public const string shell32NetworkIcon = "ms-appx:///Assets/Views/Workbench/Shell32 Network.ico";
    //public const string shell32NetworkFolderIcon = "ms-appx:///Assets/Views/Workbench/Shell32 Network Folder.ico";
    //public const string shell32NetworkFolderOpenIcon = "ms-appx:///Assets/Views/Workbench/Shell32 Network Folder Open.ico";
    //public const string shell32NetworkOfflineIcon = "ms-appx:///Assets/Views/Workbench/Shell32 Network Offline.ico";

    public static QueryOptions DefaultQueryOptionsCommonFile = new(CommonFileQuery.OrderByName, null);

    private static readonly ImageIcon defaultUnknownFileImageIcon = new()
    {
        Source = new BitmapImage(new System.Uri("ms-appx:///Assets/Views/Workbench/Shell32 Default unknown File.ico"))
    };

    private static readonly ImageIcon defaultFolderImageIcon = new()
    {
        Source = new BitmapImage(new System.Uri("ms-appx:///Assets/Views/Workbench/Shell32 Default Folder.ico"))
    };
}
