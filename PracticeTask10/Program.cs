using System;
using System.IO;
using System.Linq;

namespace PracticeTask10
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в приложение по стягиванию вершин графа!");

            // Ввод пути к файлу
            Console.WriteLine("Ввод пути к файлу");
            Console.WriteLine("Пример пути к файлу: C:/Users/Aleksandr/source/repos/PracticeTask10/PracticeTask10/Graph.txt");
            Console.WriteLine("\nВведите путь к файлу, в котором записан граф, который Вы хотите обработать:");

            string path = Console.ReadLine();

            // Чтение файла
            Graph graph = Graph.ReadGraph(path);

            // Если граф корректно описан, то выполняем стягивание
            if (graph != null)
            {
                // Ввод значения для стягивания
                Console.WriteLine("Введите значение для стягивания:");
                int value = Input();

                // Стягивание графа
                graph.Contraction(value);

                // Запись графа в файл
                graph.WriteGraph(path);
            }

            Console.WriteLine("\nЗавершение работы в приложении по стягиванию вершин графа");

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        // Ввод целого числа
        private static int Input()
        {
            int number;
            bool check;
            do
            {
                Console.Write("Ввод:");
                check = int.TryParse(Console.ReadLine(), out number);
                if (!check)
                    Console.WriteLine("Ошибка! Введите целое число");
            } while (!check);
            return number;
        }

        // Класс графа
        public class Graph
        {
            byte[,] adjacencyMatrix; // граф задается матрицей смежности
            int[] values;            // массив значений, записанных в вершинах
            int size;                // количество вершин графа

            public Graph(int size, int[] values, byte[,] matrix)
            {
                this.size = size;
                adjacencyMatrix = matrix;
                this.values = values;
            }

            public static Graph ReadGraph(string path)
            {
                try
                {
                    FileStream file = new FileStream(path, FileMode.Open);
                    StreamReader sr = new StreamReader(file);

                    int size; // размер графа
                    bool ok = int.TryParse(sr.ReadLine(), out size); // флаг правильности ввода
                    string vals = sr.ReadLine(); // массив значений

                    int[] values = new int[size];
                    if (vals.Length > size * 2 - 1)
                        vals = vals.Remove(size * 2 - 1);

                    values = vals.Split(' ').Select(n => int.Parse(n)).ToArray();

                    byte[,] matrix = new byte[size, size];
                    for (int i = 0; i < size; i++)
                    {
                        vals = sr.ReadLine();
                        if (vals.Length > size * 2 - 1)
                            vals = vals.Remove(size * 2 - 1);

                        // Чтение строки матрицы
                        byte[] row = vals.Split(' ').Select(n => byte.Parse(n)).ToArray();
                        for (int j = 0; j < size; j++)
                        {
                            if (row[j] != 0 && row[j] != 1)
                            {
                                Console.WriteLine("В файле содержатся некорректные данные.");
                                return null;
                            }
                            matrix[i, j] = row[j];
                        }
                    }

                    sr.Close();
                    file.Close();

                    return new Graph(size, values, matrix);
                }
                catch
                {
                    Console.WriteLine("Не удается открыть файл, проверьте его наличие и правильность пути.");
                    return null;
                }
            }

            // Запись графа в файл
            public void WriteGraph(string path)
            {
                path = Path.GetDirectoryName(path) + Path.GetFileNameWithoutExtension(path) + "output" + Path.GetExtension(path);
                FileStream File;
                try
                {
                    File = new FileStream(path, FileMode.Truncate);
                }
                catch (FileNotFoundException)
                {
                    File = new FileStream(path, FileMode.CreateNew);
                }
                StreamWriter sw = new StreamWriter(File);
                sw.WriteLine(size);
                foreach (int item in values)
                    sw.Write(item + " ");
                sw.WriteLine();
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                        sw.Write(adjacencyMatrix[i, j] + " ");
                    sw.WriteLine();
                }

                Console.WriteLine("Информация об обработанном графе записана в файл " + path);

                sw.Close();
                File.Close();
            }

            // Стягивание графа
            public void Contraction(int Val)
            {
                // Если искомое значение отсутствует в графе
                if (!values.Contains(Val))
                {
                    Console.WriteLine("В графе нет вершины с указанным значением.");
                    return;
                }
                // Номер вершины, где впервые встречается искомое значение
                int FirstVertex = Array.IndexOf(values, Val);

                int i = FirstVertex + 1;
                // Проходим по оставшимся вершинам графа
                while (i < size)
                {
                    // Если нашлась еще одна вершина с искомым значением
                    if (values[i] == Val)
                    {
                        // Переносим ребра
                        // Перенос ребер, исходящих из этой вершины
                        for (int col = 0; col < size; col++)
                            // Если из данной вершины исходит ребро (но не в точку, в которую стягиваем, чтобы не было петлей)
                            if (adjacencyMatrix[i, col] == 1 && col != FirstVertex)
                                // Переносим начало ребра в вершину, куда стягиваем
                                adjacencyMatrix[FirstVertex, col] = 1;

                        // Перенос ребер, входящих в эту вершину
                        for (int row = 0; row < size; row++)
                            // Если в данную вершину входит ребро (но не из точки, в которую стягиваем, чтобы не было петлей)
                            if (adjacencyMatrix[row, i] == 1 && row != FirstVertex)
                                // Переносим конец ребра в вершину, куда стягиваем
                                adjacencyMatrix[row, FirstVertex] = 1;

                        // Удаление вершины из графа
                        RemoveVertex(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            // Удаление вершины из графа
            public void RemoveVertex(int index)
            {
                // Удаление значения
                int[] newValues = new int[size - 1];
                for (int i = 0; i < index; i++)
                    newValues[i] = values[i];
                for (int i = index + 1; i < size; i++)
                    newValues[i - 1] = values[i];
                values = newValues;

                // Удаление вершины из матрицы
                byte[,] newMatrix = new byte[size - 1, size - 1];

                // Копирование незатронутой части
                for (int i = 0; i < index; i++)
                    for (int j = 0; j < index; j++)
                        newMatrix[i, j] = adjacencyMatrix[i, j];

                // Удаление столбца
                for (int i = 0; i < newMatrix.GetLength(0); i++)
                    for (int j = index; j < newMatrix.GetLength(1); j++)
                        newMatrix[i, j] = adjacencyMatrix[i, j + 1];

                // Удаление строки
                for (int i = index; i < newMatrix.GetLength(0); i++)
                    for (int j = 0; j < newMatrix.GetLength(1); j++)
                        newMatrix[i, j] = adjacencyMatrix[i + 1, j];
                adjacencyMatrix = newMatrix;

                // Уменьшаем размер графа
                size--;
            }
        }
    }
}
