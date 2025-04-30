using System.Collections.ObjectModel;

namespace WpfLb1
{

public class UserData
    {
        public required string Nickname { get; set; }
        public required string Data { get; set; }
    }
    public class DataManager
    {
        public ObservableCollection<UserData> UserEntries { get; private set; } = new ObservableCollection<UserData>();

        public bool Authenticate(string nickname)
        {
            // Проверяем, что ник не пустой
            return !string.IsNullOrWhiteSpace(nickname);
        }

        public void AddEntry(string nickname, string data)
        {
            UserEntries.Add(new UserData { Nickname = nickname, Data = data });
        }
    }
}