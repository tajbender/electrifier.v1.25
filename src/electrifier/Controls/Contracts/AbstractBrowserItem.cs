using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace electrifier.Controls.Contracts;

/// <summary>Abstract base class ShellBrowserItem.
/// Enclosing Type <typeparam name="T">ShellItem</typeparam> as reference to the underlying Shell Namespace Item reference.
/// </summary>
[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public abstract class AbstractBrowserItem<T> // TODO: IDisposable
{
    public T BaseType;
    public readonly IEnumerable<AbstractBrowserItem<T>>? ChildItems;
    public readonly bool? IsFolder;

    protected AbstractBrowserItem()
    {
        //todo: var propertyBag = new ArrayList<object owner, string key, object value>();
        //todo: var pb = new PropertyBag();
    }

    //internal void async IconUpdate(int Index, SoftwareBitmapSource bmpSrc);
    //internal void async StockIconUpdate(STOCKICONID id, SoftwareBitmapSource bmpSrc);
    //internal void async ChildItemsIconUpdate();
    public new string ToString() => $"AbstractBrowserItem(<{typeof(T)}>(isFolder {IsFolder}, childItems {ChildItems})";
}
