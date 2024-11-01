using System.Collections;

namespace DataVirtualization.WpfApp.Virtualization
{
    public class VirtualizingCollection<T> : IList<T>, IList
    {
        #region VirtualizingList

        public IVirtualizingDataProvider<T> ItemsProvider { get; }
        public int PageSize { get; } = 100;
        public long PageTimeout { get; } = 10_000;

        private readonly Dictionary<int, IList<T>> _pages = new();
        private readonly Dictionary<int, DateTime> _pageLastUpdated = new();

        public VirtualizingCollection(IVirtualizingDataProvider<T> provider, int pageSize, long pageTimeout)
        {
            ItemsProvider = provider;
            PageSize = pageSize;
            PageTimeout = pageTimeout;
        }

        public VirtualizingCollection(IVirtualizingDataProvider<T> provider, int pageSize)
        {
            ItemsProvider = provider;
            PageSize = pageSize;
        }

        public VirtualizingCollection(IVirtualizingDataProvider<T> provider)
        {
            ItemsProvider = provider;
        }

        /// <summary>
        /// Makes a request for the specified page, creating the necessary slots in the dictionary,
        /// and updating the page touch time.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        protected virtual void RequestNextPage(int pageIndex)
        {
            if (_pages.TryAdd(pageIndex, []))
            {
                if (_pages.Count > 1
                    && _pages.TryGetValue(pageIndex - 1, out var page)
                    && TryGetIntIdProperty(page[^1], out var idValue))
                {
                    PopulatePage(pageIndex, ItemsProvider.FetchNext(idValue, PageSize));
                }
                else
                {
                    PopulatePage(pageIndex, ItemsProvider.FetchRange(pageIndex * PageSize, PageSize));
                }

                _pageLastUpdated.Add(pageIndex, DateTime.Now);
            }
            else
            {
                _pageLastUpdated[pageIndex] = DateTime.Now;
            }
        }

        /// <summary>
        /// Makes a request for the specified page, creating the necessary slots in the dictionary,
        /// and updating the page touch time.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        protected virtual void RequestPreviousPage(int pageIndex)
        {
            if (_pages.TryAdd(pageIndex, []))
            {
                if (_pages.Count > 1
                    && _pages.TryGetValue(pageIndex + 1, out var page)
                    && TryGetIntIdProperty(page[0], out var idValue))
                {
                    PopulatePage(pageIndex, ItemsProvider.FetchPrevious(idValue, PageSize));
                }
                else
                {
                    PopulatePage(pageIndex, ItemsProvider.FetchRange(pageIndex * PageSize, PageSize));
                }

                _pageLastUpdated.Add(pageIndex, DateTime.Now);
            }
            else
            {
                _pageLastUpdated[pageIndex] = DateTime.Now;
            }
        }

        /// <summary>
        /// Populates the page within the dictionary.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="page">The page.</param>
        protected virtual void PopulatePage(int pageIndex, IList<T> page)
        {
            if (_pages.ContainsKey(pageIndex))
            {
                _pages[pageIndex] = page;
            }
        }

        /// <summary>
        /// Cleans up any stale pages that have not been accessed in the period dictated by PageTimeout.
        /// </summary>
        public void CleanUpPages()
        {
            var keys = new List<int>(_pageLastUpdated.Keys);

            // page 0 is a special case, since WPF ItemsControl access the first item frequently
            foreach (var key in keys.Where(key => key != 0 && (DateTime.Now - _pageLastUpdated[key]).TotalMilliseconds > PageTimeout))
            {
                _pages.Remove(key);
                _pageLastUpdated.Remove(key);
            }
        }

        /// <summary>
        /// Try to get the value of integer property `Id`. Returns <c>true</c> if success; else <c>false</c>.
        /// </summary>
        /// <param name="item">The object to retrieve `Id` from.</param>
        /// <param name="idValue">The value of `Id`.</param>
        /// <returns></returns>
        private static bool TryGetIntIdProperty(T item, out int idValue)
        {
            idValue = -1;

            if (item == null)
                return false;

            var type = typeof(T);
            var propertyInfo = type.GetProperty("Id");

            if (propertyInfo == null)
                return false;

            // Check if the property type is int
            if (propertyInfo.PropertyType != typeof(int))
                return false;

            // Get the value if it exists
            idValue = (int)propertyInfo.GetValue(item)!;
            return true;
        }

        #endregion

        #region IList/IList<T> implementation

        public T this[int index]
        {
            get
            {
                // determine which page and offset within page
                var pageIndex = index / PageSize;
                var pageOffset = index % PageSize;
                
                // request primary page
                RequestNextPage(pageIndex);

                // if accessing upper 50% then request next page
                if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
                    RequestNextPage(pageIndex + 1);

                // if accessing lower 50% then request prev page
                if (pageOffset < PageSize / 2 && pageIndex > 0)
                    RequestPreviousPage(pageIndex - 1);

                // remove stale pages
                CleanUpPages();

                // defensive check in case of async load
                if (!_pages.TryGetValue(pageIndex, out var page) || !page.Any())
                    return default;

                // return requested item
                return _pages[pageIndex][pageOffset];
            }
            set => throw new NotSupportedException();
        }

        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _pages.Clear();
        }

        private int? _count;
        public int Count => _count ??= ItemsProvider.FetchCount();
        public bool IsSynchronized => false;
        public object SyncRoot => this;
        public bool IsReadOnly => true;
        public bool IsFixedSize => false;

        public int IndexOf(T? item)
        {
            if (item is null)
                return -1;

            foreach (var dict in _pages)
            {
                var index = dict.Value.IndexOf(item);

                if (index >= 0)
                {
                    return dict.Key * PageSize + index;
                }
            }

            return -1;
        }

        public int IndexOf(object? value)
        {
            return IndexOf((T?)value);
        }

        public int Add(object? value)
        {
            throw new NotSupportedException();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(object? value)
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object? value)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void Remove(object? value)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
