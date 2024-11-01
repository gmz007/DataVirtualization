namespace DataVirtualization.WpfApp.Virtualization
{
    /// <summary>
    /// Represents a provider of collection details.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public interface IVirtualizingDataProvider<T>
    {
        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        int FetchCount();

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        IList<T> FetchRange(int startIndex, int count);

        /// <summary>
        /// Fetches the next range of items, does not include the `startIndex`. To support "Keyset Pagination".
        /// The type T MUST contain an int property named `Id` (and `Id` should be deterministic), defaults to FetchRange() otherwise.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        IList<T> FetchNext(int startIndex, int count);

        /// <summary>
        /// Fetches the previous range of items, does not include `endIndex`. To support "Keyset Pagination".
        /// The type T MUST contain an int property named `Id` (and `Id` should be deterministic), defaults to FetchRange() otherwise.
        /// </summary>
        /// <param name="endIndex">The end index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        IList<T> FetchPrevious(int endIndex, int count);
    }
}
