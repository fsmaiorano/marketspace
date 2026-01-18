namespace BuildingBlocks.Abstractions;

public abstract class WatchedList<T>(List<T>? initialItems = null)
{
    public List<T> CurrentItems { get; private set; } = initialItems ?? [];
    
    private readonly List<T> _initial = initialItems ?? [];
    private List<T> _new = [];
    private List<T> _removed = [];

    protected abstract bool CompareItems(T a, T b);

    public List<T> GetItems()
    {
        return CurrentItems;
    }

    public List<T> GetNewItems()
    {
        return _new;
    }

    public List<T> GetRemovedItems()
    {
        return _removed;
    }

    private bool IsCurrentItem(T item)
    {
        return CurrentItems.Any(v => CompareItems(item, v));
    }

    private bool IsNewItem(T item)
    {
        return _new.Any(v => CompareItems(item, v));
    }

    private bool IsRemovedItem(T item)
    {
        return _removed.Any(v => CompareItems(item, v));
    }

    private void RemoveFromNew(T item)
    {
        _new = _new.Where(v => !CompareItems(v, item)).ToList();
    }

    private void RemoveFromCurrent(T item)
    {
        CurrentItems = CurrentItems.Where(v => !CompareItems(item, v)).ToList();
    }

    private void RemoveFromRemoved(T item)
    {
        _removed = _removed.Where(v => !CompareItems(item, v)).ToList();
    }

    private bool WasAddedInitially(T item)
    {
        return _initial.Any(v => CompareItems(item, v));
    }

    public bool Exists(T item)
    {
        return IsCurrentItem(item);
    }

    public void Add(T item)
    {
        if (IsRemovedItem(item))
        {
            RemoveFromRemoved(item);
        }

        if (!IsNewItem(item) && !WasAddedInitially(item))
        {
            _new.Add(item);
        }

        if (!IsCurrentItem(item))
        {
            CurrentItems.Add(item);
        }
    }
    
    public void AddRange(List<T> items) => items.ForEach(Add);

    public void Remove(T item)
    {
        RemoveFromCurrent(item);

        if (IsNewItem(item))
        {
            RemoveFromNew(item);
            return;
        }

        if (!IsRemovedItem(item))
        {
            _removed.Add(item);
        }
    }

    public void Update(List<T> items)
    {
        var newItems = items.Where(a => !GetItems().Any(b => CompareItems(a, b))).ToList();
        var removedItems = GetItems().Where(a => !items.Any(b => CompareItems(a, b))).ToList();

        CurrentItems = items;
        _new = newItems;
        _removed = removedItems;
    }
}