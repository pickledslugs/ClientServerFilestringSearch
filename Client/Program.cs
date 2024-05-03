using System;
using System.Net.Sockets;
using System.Text;

namespace FileStringSearch
{
    class Client
    {
        static void Main()
        {
            // Адрес и порт сервера
            string serverAddress = "127.0.0.1";
            int serverPort = 8000;

            // Основной цикл приложения
            while (true)
            {
                // Создание нового экземпляра TcpClient
                using (TcpClient client = new TcpClient())
                {
                    try
                    {
                        // Подключение к серверу
                        client.Connect(serverAddress, serverPort);

                        // Запрос пользователю на ввод строки для поиска
                        Console.Write("Введите строку для поиска ('/q' для выхода): ");
                        string searchString = Console.ReadLine();

                        // Проверка на команду выхода
                        if (searchString.ToLower() == "/q")
                            break;

                        // Отправка строки поиска на сервер
                        byte[] buffer = Encoding.UTF8.GetBytes(searchString);
                        client.GetStream().Write(buffer, 0, buffer.Length);

                        // Получение ответа от сервера
                        buffer = new byte[1024];
                        int bytesRead = client.GetStream().Read(buffer, 0, buffer.Length);
                        int occurrenceCount = int.Parse(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                        // Вывод количества найденных вхождений
                        Console.WriteLine($"{occurrenceCount} вхождений найдено.");

                        // Вывод строк, содержащих искомую строку
                        while ((bytesRead = client.GetStream().Read(buffer, 0, buffer.Length)) > 0)
                        {
                            string line = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim(); // Удаление лишних пробелов
                            if (!string.IsNullOrWhiteSpace(line)) // Проверка на пустые или содержащие только пробел строки
                            {
                                Console.WriteLine(line);
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        // Обработка ошибок, связанных с сокетами
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Обработка общих исключений
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                }
            }
        }
    }
}