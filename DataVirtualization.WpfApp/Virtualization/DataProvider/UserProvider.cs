namespace DataVirtualization.WpfApp.Virtualization.DataProvider
{
    using Data;
    using Data.Models;

    public class UserProvider(Database database) : IVirtualizingDataProvider<User>
    {
        public int FetchCount()
        {
            return database.GetUsersCount();
        }

        public IList<User> FetchRange(int startIndex, int count)
        {
            return database.GetUserRange(startIndex, count).ToList();
        }

        public IList<User> FetchNext(int startIndex, int count)
        {
            return database.GetUserRange(startIndex, count).ToList();
        }

        public IList<User> FetchPrevious(int endIndex, int count)
        {
            return database.GetUserPrevious(endIndex, count).ToList();
        }
    }
}
