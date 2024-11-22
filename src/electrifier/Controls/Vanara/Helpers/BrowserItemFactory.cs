using electrifier.Controls.Vanara.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.PInvoke;

namespace electrifier.Controls.Vanara.Helpers;
internal class BrowserItemFactory
{
    public static async Task<SoftwareBitmapSource> GetStockIconBitmapSource(Shell32.SHSTOCKICONID shStockIconId)
    {
        Dictionary<Shell32.SHSTOCKICONID, SoftwareBitmapSource> stockIconDictionary = [];

        try
        {
            if (stockIconDictionary.TryGetValue(shStockIconId, out var source))
            {
                return source;
            }

            var siFlags = Shell32.SHGSI.SHGSI_LARGEICON | Shell32.SHGSI.SHGSI_ICON;
            var icninfo = Shell32.SHSTOCKICONINFO.Default;
            Shell32.SHGetStockIconInfo(shStockIconId, siFlags, ref icninfo).ThrowIfFailed($"SHGetStockIconInfo({shStockIconId})");

            var hIcon = icninfo.hIcon;
            var icnHandle = hIcon.ToIcon();
            var bmpSource = ShellNamespaceService.GetWinUi3BitmapSourceFromIcon(icnHandle);
            await bmpSource;
            var softBitmap = bmpSource.Result;

            if (softBitmap != null)
            {
                _ = stockIconDictionary.TryAdd(shStockIconId, softBitmap);
                return softBitmap;
            }

            throw new ArgumentOutOfRangeException($".GetStockIconBitmapSource(): Can't get StockIcon for SHSTOCKICONID: {shStockIconId.ToString()}");
        }
        catch (Exception)
        {
            throw; // TODO handle exception
        }
    }

}
