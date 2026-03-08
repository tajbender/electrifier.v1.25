using System.Diagnostics;

namespace electrifier.Controls.Contracts;

/// <summary>
/// Abstract base class AbstractBrowserItem.
/// </summary>
[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public abstract class AbstractBrowserItem<T> : IEquatable<AbstractBrowserItem<T>?> // TODO: IDisposable
{
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

    // TODO: Compare PIDL here!
    public override bool Equals(object? obj) => Equals(obj as AbstractBrowserItem<T>);
    // TODO: Compare PIDL here!
    public bool Equals(AbstractBrowserItem<T>? other) => other is not null && other == this;
    // TODO: Compare PIDL here!
    public static bool operator ==(AbstractBrowserItem<T>? left, AbstractBrowserItem<T>? right) => EqualityComparer<AbstractBrowserItem<T>>.Default.Equals(left, right);
    // TODO: Compare PIDL here!
    public static bool operator !=(AbstractBrowserItem<T>? left, AbstractBrowserItem<T>? right) => !(left == right);
    // TODO: Compare PIDL here!
    public new string ToString() => $"AbstractBrowserItem(<{typeof(T)}>(isFolder {IsFolder}, childItems {ChildItems})";
}
