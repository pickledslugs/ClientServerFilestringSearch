using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FileStringSearch
{
    class Server
    {
        // Метод Main для запуска сервера
        static void Main()
        {
            // Указание пути к файлу и деталей сервера
            string filePath = "C:\\Users\\Stepan\\Desktop\\file.txt";
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8000;

            // Создание TCP слушателя
            TcpListener listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine($"Сервер запущен, прослушивание на {ipAddress}:{port}");

            // Непрерывное прослушивание входящих подключений клиентов
            while (true)
            {
                // Принятие подключения клиента
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Клиент подключен");

                // Обработка запроса клиента в отдельном потоке
                new Thread(() =>
                {
                    HandleClient(client, filePath);
                }).Start();
            }
        }

        // Метод для обработки запроса клиента
        static void HandleClient(TcpClient client, string filePath)
        {
            try
            {
                // Получение сетевого потока для клиента
                using (NetworkStream stream = client.GetStream())
                {
                    // Чтение строки поиска от клиента
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string searchString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Выполнение поиска в файле
                    (int occurrenceCount, string[] matchingLines) = SearchStringInFile(filePath, searchString);

                    // Отправка количества вхождений клиенту
                    byte[] response = Encoding.UTF8.GetBytes($"{occurrenceCount}");
                    stream.Write(response, 0, response.Length);

                    // Отправка совпадающих строк клиенту
                    foreach (string line in matchingLines)
                    {
                        response = Encoding.UTF8.GetBytes($"{line}\n");
                        stream.Write(response, 0, response.Length);
                    }

                    // Сброс потока для отправки всех данных
                    stream.Flush();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка: {e.Message}");
            }
            finally
            {
                // Закрытие соединения с клиентом
                client.Close();
                Console.WriteLine("Клиент отключен");
            }
        }

        // Метод для поиска строки в файле
        static (int, string[]) SearchStringInFile(string filePath, string searchString)
        {
            int occurrenceCount = 0;
            List<string> matchingLines = new List<string>();

            try
            {
                // Открытие файла для чтения
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    int lineNumber = 1;

                    // Чтение файла построчно
                    while ((line = sr.ReadLine()) != null)
                    {
                        int index = 0;

                        // Поиск строки в строке
                        while ((index = line.IndexOf(searchString, index, StringComparison.OrdinalIgnoreCase)) != -1)
                        {
                            matchingLines.Add($"Строка {lineNumber}, Позиция {index}: {line}");
                            occurrenceCount++;
                            index += searchString.Length;
                        }

                        lineNumber++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }

            // Возврат количества вхождений и совпадающих строк
            return (occurrenceCount, matchingLines.ToArray());
        }
    }
}