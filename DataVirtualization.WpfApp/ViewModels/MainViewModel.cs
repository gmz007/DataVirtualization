using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataVirtualization.WpfApp.Virtualization;
using DataVirtualization.WpfApp.Virtualization.DataProvider;

namespace DataVirtualization.WpfApp.ViewModels
{
    using Data;
    using Data.Models;

    public partial class MainViewModel : ObservableObject
    {
        private const int PageSize = 200;
        private readonly Database _database;

        public MainViewModel(Database database)
        {
            _database = database;

            var queryWatch = System.Diagnostics.Stopwatch.StartNew();

            _users = new VirtualizingCollection<User>(new UserProvider(_database), PageSize);
            _count = Users.Count;

            queryWatch.Stop();
            QueryTime = queryWatch.ElapsedMilliseconds;
        }

        [ObservableProperty]
        private int _count;

        [ObservableProperty]
        private long _queryTime;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty] 
        private VirtualizingCollection<User> _users;

        [RelayCommand]
        private void SearchUser()
        {
            var queryWatch = System.Diagnostics.Stopwatch.StartNew();

            Users.Clear();
            Users = new VirtualizingCollection<User>(new UserSearchProvider(_database, SearchText), PageSize);
            Count = Users.Count;

            queryWatch.Stop();
            QueryTime = queryWatch.ElapsedMilliseconds;
        }

        [RelayCommand]
        private void Refresh()
        {
            var queryWatch = System.Diagnostics.Stopwatch.StartNew();

            Users.Clear();
            Users = new VirtualizingCollection<User>(new UserProvider(_database), PageSize);
            Count = Users.Count;

            queryWatch.Stop();
            QueryTime = queryWatch.ElapsedMilliseconds;
        }
    }
}
