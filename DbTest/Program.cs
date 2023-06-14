using MySqlConnector;

var connStr = "Server = localhost; Database = test10x; port = 3306; User Id = root; password = "; // строковая перемнная с инфорамцией подключения к БД
MySqlConnection conn = new MySqlConnection(connStr);
conn.Open();

// Создание команды
MySqlCommand cmd = conn.CreateCommand();

cmd.CommandText = "CREATE TABLE IF NOT EXISTS `department` (" +
    "id INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
    "name VARCHAR(200) NOT NULL DEFAULT 'Рока и Копыта'," +
    "location VARCHAR(200) NOT NULL)";


cmd.ExecuteNonQuery();

// Список изменяющихся параметров для занесения в базу
var data = new List<Departments>();
data.Add(new Departments() { Name = "Департамент 1", Location = "Москва" });
data.Add(new Departments() { Location = "Бобруйск" });

foreach (var department in data)
{
    var sqlCmd = $"INSERT INTO `department` ({ (department.Name != null ? "Name, " : "")} location)" +
        $"VALUES({(department.Name != null ? "'" + department.Name + "', ": "")}'{department.Location}')";
    cmd.CommandText = sqlCmd;
    cmd.ExecuteNonQuery();
}


// Создаем параметризованный запрос пользователия
// Для избежания SQL-инъекции
// Работает для любого типа запросов (удаление, вставка и так далее)
try
{
    Console.WriteLine("Введите номер записи для удаления:");
    var delId = Console.ReadLine() ?? "0"; // 0; DELETE FROM `department` - будет два запроса выполняться. Первый удаление 0 строки, второй удаление всего.
    var delCmd = conn.CreateCommand();
    delCmd.CommandText = $"DELETE FROM `department` WHERE `id` = @identity"; // параметр задается через собачку
                                                                             // Создадим объект класса:
    MySqlParameter idParam = new MySqlParameter("@identity", delId);
    delCmd.Parameters.Add(idParam);
    delCmd.ExecuteNonQuery();
}
catch (Exception ex) { Console.WriteLine($"Что-то пошло не так.. \n" +
    $"{ex.Message}"); }

// Запрос на выборку

var selectCmd = conn.CreateCommand();
selectCmd.CommandText = "SELECT * FROM `department`";
var departmentList = new List<Departments>();

using (var reader = selectCmd.ExecuteReader())
{
    // Проверка на то, что мы получили хотя бы 1 строку
    if (reader.HasRows)
    {
        // Пока можем что-то прочитать
        // Чтение идет построчно для .Read()
        // Каждый вызов смещает курсор на след.строку
        while (reader.Read())
        {
            departmentList.Add(new Departments()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Location = reader.GetString(2),
            });
        }
    }
}
departmentList.ForEach((department) => { Console.WriteLine(department); });




// Закрытие соединения
conn.Close();

class Departments
{
    public int Id { get; set; }
    public string? Name { get; set; } = null;
    public string? Location { get; set; } = null;

    public override string ToString()
    {
        return $"{Id}: {Name} -> {Location}";
    }
}