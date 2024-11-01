namespace DataVirtualization.WpfApp.Virtualization.DataProvider
{
    using Data;
    using Data.Models;

    public class UserSearchProvider(Database database, string searchTerm) : IVirtualizingDataProvider<User>
    {
        public int FetchCount()
        {
            return database.SearchUsersCount(searchTerm);
        }

        public IList<User> FetchRange(int startIndex, int count)
        {
            return database.SearchUserRange(searchTerm, startIndex, count).ToList();
        }

        public IList<User> FetchNext(int startIndex, int count)
        {
            return database.SearchUserNext(searchTerm, startIndex, count).ToList();
        }

        public IList<User> FetchPrevious(int endIndex, int count)
        {
            return database.SearchUserPrevious(searchTerm, endIndex, count).ToList();
        }
    }
}
