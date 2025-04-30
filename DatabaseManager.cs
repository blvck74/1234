using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static WpfLb1.MainWindow;

namespace WpfLb1
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager()
        {
            _connectionString = ConfigurationManager.ConnectionString;
        }

        public bool AddUser(string nickname)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand("INSERT INTO Users (Nickname) VALUES (@nickname) ON CONFLICT DO NOTHING", connection);
            command.Parameters.AddWithValue("nickname", nickname);

            return command.ExecuteNonQuery() > 0;
        }
        
        // Добавить в класс DatabaseManager
        public MainWindow.Work? GetWorkById(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = new NpgsqlCommand("SELECT * FROM Works WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", workId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadWork(reader) : null;
        }

        public List<string> GetSections()
        {
            var sections = new List<string>();
            using var connection = new NpgsqlConnection(_connectionString);
            try
            {
                connection.Open();
                using var command = new NpgsqlCommand("SELECT Name FROM Sections", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sections.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении секций: {ex.Message}");
            }
            return sections;
        }


        public void AddSection(string sectionName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand("INSERT INTO Sections (Name) VALUES (@name) ON CONFLICT DO NOTHING", connection);
            command.Parameters.AddWithValue("name", sectionName);
            command.ExecuteNonQuery();
        }

        public void DeleteSection(string sectionName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand("DELETE FROM Sections WHERE Name = @name", connection);
            command.Parameters.AddWithValue("name", sectionName);
            command.ExecuteNonQuery();
        }
        // Обновить методы работы с темами:
        public void SaveTopic(Topic topic)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Проверяем существование темы
                var topicCommand = new NpgsqlCommand(@"
            INSERT INTO Topics (Name, SectionId) 
            SELECT @name, Id 
            FROM Sections 
            WHERE Name = @section
            ON CONFLICT (Name) DO UPDATE 
            SET Name = EXCLUDED.Name
            RETURNING Id",
                    connection, transaction);

                topicCommand.Parameters.AddWithValue("name", topic.Name);
                topicCommand.Parameters.AddWithValue("section", topic.Section);
                var topicId = topicCommand.ExecuteScalar();

                if (topicId == null)
                    throw new Exception("Не удалось создать или найти тему");

                // Обновляем или добавляем контент
                var contentCommand = new NpgsqlCommand(@"
            INSERT INTO Information (TopicId, Content) 
            VALUES (@topicId, @content)
            ON CONFLICT (TopicId) DO UPDATE 
            SET Content = EXCLUDED.Content",
                    connection, transaction);

                contentCommand.Parameters.AddWithValue("topicId", topicId);
                contentCommand.Parameters.AddWithValue("content", topic.Content);
                contentCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public List<string> GetTopics(string sectionName)
        {
            var topics = new List<string>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand(@"
        SELECT t.Name 
        FROM Topics t
        JOIN Sections s ON t.SectionId = s.Id
        WHERE s.Name = @sectionName
        ORDER BY t.Name",  // Добавляем сортировку для удобства
                connection);

            command.Parameters.AddWithValue("sectionName", sectionName);

            try
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    topics.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении тем: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }

            return topics;
        }
        public Topic GetTopic(string name)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand(@"
                SELECT t.Name, i.Content, s.Name as Section
                FROM Topics t
                JOIN Sections s ON t.SectionId = s.Id
                LEFT JOIN Information i ON t.Id = i.TopicId
                WHERE t.Name = @name",
                connection);

            command.Parameters.AddWithValue("name", name);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Topic
                {
                    Name = reader.GetString(0),
                    Content = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), // Получаем текст как есть
                    Section = reader.GetString(2)
                };
            }
            return null;
        }

    // Удалить устаревшие методы
    // - GetInformation (заменен на GetTopic)
    // - UpdateTopic (заменен на SaveTopic)
  
        public void AddInformation(string section, string topic, string content)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Получаем ID раздела
                var sectionCommand = new NpgsqlCommand(
                    "SELECT Id FROM Sections WHERE Name = @name",
                    connection,
                    transaction);
                sectionCommand.Parameters.AddWithValue("name", section);
                var sectionId = sectionCommand.ExecuteScalar();

                if (sectionId == null) throw new Exception("Раздел не найден.");

                // Добавляем тему
                var topicCommand = new NpgsqlCommand(
                    "INSERT INTO Topics (Name, SectionId) VALUES (@name, @sectionId) RETURNING Id",
                    connection,
                    transaction);
                topicCommand.Parameters.AddWithValue("name", topic);
                topicCommand.Parameters.AddWithValue("sectionId", sectionId);
                var topicId = topicCommand.ExecuteScalar();

                if (topicId == null) throw new Exception("Не удалось добавить тему.");

                // Добавляем информацию как простой текст, а не как RTF или байты
                var infoCommand = new NpgsqlCommand(
                    "INSERT INTO Information (TopicId, Content) VALUES (@topicId, @content)",
                    connection,
                    transaction);
                infoCommand.Parameters.AddWithValue("topicId", topicId);
                infoCommand.Parameters.AddWithValue("content", content); // Сохраняем как есть, без конвертации

                infoCommand.ExecuteNonQuery();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteTopic(string topicName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Сначала получаем ID темы
                var getTopicIdCmd = new NpgsqlCommand(
                    "SELECT Id FROM Topics WHERE Name = @name",
                    connection,
                    transaction);
                getTopicIdCmd.Parameters.AddWithValue("name", topicName);
                var topicId = getTopicIdCmd.ExecuteScalar();

                if (topicId == null)
                {
                    throw new Exception($"Тема '{topicName}' не найдена");
                }

                // Удаляем связанную информацию
                var deleteInfoCmd = new NpgsqlCommand(
                    "DELETE FROM Information WHERE TopicId = @topicId",
                    connection,
                    transaction);
                deleteInfoCmd.Parameters.AddWithValue("topicId", topicId);
                deleteInfoCmd.ExecuteNonQuery();

                // Удаляем саму тему
                var deleteTopicCmd = new NpgsqlCommand(
                    "DELETE FROM Topics WHERE Id = @topicId",
                    connection,
                    transaction);
                deleteTopicCmd.Parameters.AddWithValue("topicId", topicId);
                deleteTopicCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
       


        public bool CreateUser(string nickname, string password)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand(
                "INSERT INTO Users (Nickname, Password) VALUES (@nickname, @password) ON CONFLICT (Nickname) DO NOTHING",
                connection
            );
            command.Parameters.AddWithValue("nickname", nickname);
            command.Parameters.AddWithValue("password", password);

            return command.ExecuteNonQuery() > 0;
        }

        public bool AuthenticateUser(string nickname, string password)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Nickname = @nickname AND Password = @password",
                connection
            );
            command.Parameters.AddWithValue("nickname", nickname);
            command.Parameters.AddWithValue("password", password);

            return (command.ExecuteScalar() as long?) > 0;
        }

        // Методы для работы с сервисами
        public void AddService(string serviceName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO Services (ServiceName) VALUES (@name)", connection);
            cmd.Parameters.AddWithValue("name", serviceName);
            cmd.ExecuteNonQuery();
        }

        public void DeleteService(string serviceName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM Services WHERE ServiceName = @name", connection);
            cmd.Parameters.AddWithValue("name", serviceName);
            cmd.ExecuteNonQuery();
        }

        public List<string> GetServices()
        {
            var services = new List<string>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand("SELECT ServiceName FROM Services", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                services.Add(reader.GetString(0));
            }
            return services;
        }

        // Методы для работы с работами
        public void AddWork(Work work)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(@"
            INSERT INTO Works (ServiceName, IncidentNumber, WorkName, StartDate, EndDate, WorkLink, HasDowntime, Status)
            VALUES (@service, @incident, @name, @start, @end, @link, @hasDowntime, 'Active')", connection);

            cmd.Parameters.AddWithValue("service", work.ServiceName);
            cmd.Parameters.AddWithValue("incident", work.IncidentNumber);
            cmd.Parameters.AddWithValue("name", work.WorkName);
            cmd.Parameters.AddWithValue("start", work.StartDate);
            cmd.Parameters.AddWithValue("end", work.EndDate);
            cmd.Parameters.AddWithValue("link", work.WorkLink ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("hasDowntime", work.HasDowntime);

            cmd.ExecuteNonQuery();
        }

        public void DeleteWork(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM Works WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("id", workId);
            cmd.ExecuteNonQuery();
        }

        public List<Work> GetWorksByService(string serviceName)
        {
            var works = new List<Work>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM Works WHERE ServiceName = @service AND Status != 'Completed'", connection);
            cmd.Parameters.AddWithValue("service", serviceName);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                works.Add(ReadWork(reader));
            }
            return works;
        }


        public void UpdateWorkStatus(int workId, string status)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE Works SET Status = @status WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("id", workId);
            cmd.ExecuteNonQuery();

            // Добавляем запись в лог
            LogWorkAction(workId, CurrentUser, $"Status changed to {status}");
        }

        public void PauseWork(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO WorkStatus (WorkId, Status, StartTime) 
                VALUES (@workId, 'Paused', @startTime)", connection);
            cmd.Parameters.AddWithValue("workId", workId);
            cmd.Parameters.AddWithValue("startTime", DateTime.Now);
            cmd.ExecuteNonQuery();

            UpdateWorkStatus(workId, "Paused");
        }

        public void ResumeWork(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Завершаем текущую паузу
            using var cmd = new NpgsqlCommand(@"
                UPDATE WorkStatus 
                SET EndTime = @endTime 
                WHERE WorkId = @workId AND Status = 'Paused' AND EndTime IS NULL",
                connection);

            var endTime = DateTime.Now;
            cmd.Parameters.AddWithValue("endTime", endTime);
            cmd.Parameters.AddWithValue("workId", workId);
            cmd.ExecuteNonQuery();

            // Обновляем общую длительность пауз
            UpdateTotalPauseDuration(workId);
            UpdateWorkStatus(workId, "Active");
        }

        public void ExtendWork(int workId, DateTime newEndTime)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE Works SET EndDate = @endDate WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("endDate", newEndTime);
            cmd.Parameters.AddWithValue("id", workId);
            cmd.ExecuteNonQuery();

            LogWorkAction(workId, CurrentUser, $"Extended to {newEndTime}");
        }

        public void CompleteWork(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE Works SET Status = 'Completed' WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("id", workId);
            cmd.ExecuteNonQuery();

            LogWorkAction(workId, CurrentUser, "Status changed to Completed");
        }


        private void LogWorkAction(int workId, string userId, string action)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(@"
            INSERT INTO WorkLogs (WorkId, UserId, Action, ActionTime)
            VALUES (@workId, @userId, @action, @actionTime)", connection);

            cmd.Parameters.AddWithValue("workId", workId);
            cmd.Parameters.AddWithValue("userId", string.IsNullOrEmpty(userId) ? "System" : userId);
            cmd.Parameters.AddWithValue("action", action);
            cmd.Parameters.AddWithValue("actionTime", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        public List<Work> GetCompletedWorks(string searchTerm = "")
        {
            var works = new List<Work>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(@"
                SELECT * FROM Works 
                WHERE Status = 'Completed' 
                AND (LOWER(WorkName) LIKE LOWER(@search) 
                    OR LOWER(IncidentNumber) LIKE LOWER(@search))", connection);
            cmd.Parameters.AddWithValue("search", $"%{searchTerm}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                works.Add(ReadWork(reader));
            }
            return works;
        }

        public List<WorkLogEntry> GetWorkLogs(int workId)
        {
            var logs = new List<WorkLogEntry>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM WorkLogs WHERE WorkId = @workId ORDER BY ActionTime DESC",
                connection);
            cmd.Parameters.AddWithValue("workId", workId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new WorkLogEntry
                {
                    UserId = reader.GetString(reader.GetOrdinal("UserId")),
                    Action = reader.GetString(reader.GetOrdinal("Action")),
                    ActionTime = reader.GetDateTime(reader.GetOrdinal("ActionTime")),
                    Details = reader.IsDBNull(reader.GetOrdinal("Details")) ? string.Empty : reader.GetString(reader.GetOrdinal("Details"))
                });
            }
            return logs;
        }
        // Метод для обновления общей длительности пауз
        private void UpdateTotalPauseDuration(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Рассчитываем общую длительность пауз
            var cmd = new NpgsqlCommand(@"
            UPDATE Works
            SET TotalPauseDuration = COALESCE(TotalPauseDuration, INTERVAL '0') + (
                SELECT SUM(EXTRACT(EPOCH FROM (EndTime - StartTime))) * INTERVAL '1 second'
                FROM WorkStatus
                WHERE WorkId = @workId AND Status = 'Paused' AND EndTime IS NOT NULL
            )
            WHERE Id = @workId", connection);

            cmd.Parameters.AddWithValue("workId", workId);
            cmd.ExecuteNonQuery();
        }

        // Метод для чтения работы по ID
        private Work ReadWork(NpgsqlDataReader reader)
        {
            return new Work
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ServiceName = reader.GetString(reader.GetOrdinal("ServiceName")),
                IncidentNumber = reader.GetString(reader.GetOrdinal("IncidentNumber")),
                WorkName = reader.GetString(reader.GetOrdinal("WorkName")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                WorkLink = reader.IsDBNull(reader.GetOrdinal("WorkLink")) ? null : reader.GetString(reader.GetOrdinal("WorkLink")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                HasDowntime = reader.GetBoolean(reader.GetOrdinal("HasDowntime")),
                TotalPauseDuration = reader.GetTimeSpan(reader.GetOrdinal("TotalPauseDuration")),
                PauseStartTime = reader.IsDBNull(reader.GetOrdinal("PauseStartTime")) ? null : reader.GetDateTime(reader.GetOrdinal("PauseStartTime"))
            };
        }
        public static string CurrentUser { get; set; } = "DefaultUser"; // Replace "DefaultUser" with actual logic to retrieve the current user
        public string GetWorkStatus(int workId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand("SELECT Status FROM Works WHERE Id = @id", connection);
            command.Parameters.AddWithValue("id", workId);

            return command.ExecuteScalar()?.ToString() ?? "Unknown";
        }
        public void UpdateWorkDowntime(int workId, bool hasDowntime)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(
                "UPDATE Works SET HasDowntime = @hasDowntime WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("hasDowntime", hasDowntime);
            cmd.Parameters.AddWithValue("id", workId);
            cmd.ExecuteNonQuery();

            // Логируем изменение
            LogWorkAction(workId, CurrentUser, $"Downtime status changed to: {hasDowntime}");
        }

    }

    public class WorkLogEntry
    {
        public required string UserId { get; set; }
        public required string Action { get; set; }
        public DateTime ActionTime { get; set; }
        public string Details { get; set; } = string.Empty;
    }



}
