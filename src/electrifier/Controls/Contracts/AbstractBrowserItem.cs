using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace electrifier.Controls.Contracts;

/// <summary>
/// Abstract base class AbstractBrowserItem.
/// 
/// Enclosing Type <typeparam name="T">ShellItem</typeparam> as reference to the underlying Shell Namespace Item reference.
/// </summary>
[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public abstract class AbstractBrowserItem<T> : IEquatable<AbstractBrowserItem<T>?> // TODO: IDisposable
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
    public override bool Equals(object? obj) => Equals(obj as AbstractBrowserItem<T>);
    public bool Equals(AbstractBrowserItem<T>? other) => other is not null && EqualityComparer<T>.Default.Equals(BaseType, other.BaseType) && IsFolder == other.IsFolder;
    public new string ToString() => $"AbstractBrowserItem(<{typeof(T)}>(isFolder {IsFolder}, childItems {ChildItems})";

    public static bool operator ==(AbstractBrowserItem<T>? left, AbstractBrowserItem<T>? right) => EqualityComparer<AbstractBrowserItem<T>>.Default.Equals(left, right);

    public static bool operator !=(AbstractBrowserItem<T>? left, AbstractBrowserItem<T>? right) => !(left == right);

    public override int GetHashCode() => (BaseType.GetHashCode());
}
