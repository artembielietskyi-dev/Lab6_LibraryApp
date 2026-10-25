using System;
using System.Globalization;

namespace LibraryApp
{
	internal class Program
	{
		const int Capacity = 100;   // максимум книг
		const int OverdueDays = 14; // скільки днів можна тримати книгу

		// сім масивів, елементи з однаковим індексом це одна книга
		static int[] ids = new int[Capacity];                  // номер книги
		static string[] titles = new string[Capacity];         // назва
		static string[] authors = new string[Capacity];        // автор
		static string[] genres = new string[Capacity];         // жанр
		static int[] copies = new int[Capacity];               // кількість примірників
		static DateTime[] issueDates = new DateTime[Capacity]; // дата видачі
		static bool[] isIssued = new bool[Capacity];           // чи видана зараз

		static int count = 0;   // скільки книг зараз у списку
		static int nextId = 1;  // номер для наступної книги

		static void Main(string[] args)
		{
			// щоб українські літери нормально вводились і виводились
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			Console.InputEncoding = System.Text.Encoding.UTF8;

			Seed(); // додаємо три книги для прикладу

			// меню працює, поки не оберемо 0
			bool running = true;
			while (running)
			{
				Console.WriteLine();
				Console.WriteLine("=== Бібліотека: облік книг ===");
				Console.WriteLine("1 — додати книгу");
				Console.WriteLine("2 — показати всі книги");
				Console.WriteLine("3 — знайти книгу за номером");
				Console.WriteLine("4 — видалити книгу за номером");
				Console.WriteLine("0 — вихід");
				Console.Write("Ваш вибір: ");

				string choice = Console.ReadLine();

				switch (choice)
				{
					case "1": AddBook(); break;
					case "2": PrintAll(); break;
					case "3": FindById(); break;
					case "4": DeleteById(); break;
					case "0": running = false; break;
					default: Console.WriteLine("Немає такого пункту меню."); break;
				}
			}

			Console.WriteLine("Роботу завершено.");
		}

		// записує книгу в першу вільну комірку всіх масивів
		static void Store(string title, string author, string genre,
						  int copyCount, DateTime issueDate, bool issued)
		{
			ids[count] = nextId;
			titles[count] = title;
			authors[count] = author;
			genres[count] = genre;
			copies[count] = copyCount;
			issueDates[count] = issueDate;
			isIssued[count] = issued;

			count++;    // книг стало на одну більше
			nextId++;   // наступна книга отримає інший номер
		}

		// три книги для прикладу
		static void Seed()
		{
			Store("Кобзар", "Тарас Шевченко", "Поезія", 3, DateTime.Today.AddDays(-20), true);
			Store("Захар Беркут", "Іван Франко", "Повість", 2, new DateTime(), false);
			Store("Маруся Чурай", "Ліна Костенко", "Поезія", 1, DateTime.Today.AddDays(-5), true);
		}

		// шукає книгу за номером, повертає індекс або -1, якщо не знайшла
		static int IndexOfId(int id)
		{
			for (int i = 0; i < count; i++) // йдемо тільки до count
			{
				if (ids[i] == id)
				{
					return i;
				}
			}
			return -1;
		}

		// 1 - додати книгу
		static void AddBook()
		{
			// перевіряємо, чи є вільне місце
			if (count >= Capacity)
			{
				Console.WriteLine("Сховище заповнене: більше книг не вміщується.");
				return;
			}

			Console.WriteLine("--- Нова книга ---");

			Console.Write("Назва: ");
			string title = Console.ReadLine();
			if (title == "")
			{
				Console.WriteLine("Назва не може бути порожньою. Книгу не додано.");
				return;
			}

			Console.Write("Автор: ");
			string author = Console.ReadLine();
			if (author == "")
			{
				Console.WriteLine("Автор не може бути порожнім. Книгу не додано.");
				return;
			}

			Console.Write("Жанр: ");
			string genre = Console.ReadLine();
			if (genre == "")
			{
				Console.WriteLine("Жанр не може бути порожнім. Книгу не додано.");
				return;
			}

			// кількість має бути числом більше нуля
			Console.Write("Кількість примірників: ");
			int copyCount;
			if (!int.TryParse(Console.ReadLine(), out copyCount) || copyCount <= 0)
			{
				Console.WriteLine("Кількість має бути цілим числом більше нуля. Книгу не додано.");
				return;
			}
			Console.Write("Чи видана зараз (1 — так, 0 — ні): ");
			string answer = Console.ReadLine();
			bool issued;
			if (answer == "1")
			{
				issued = true;
			}
			else if (answer == "0")
			{
				issued = false;
			}
			else
			{
				Console.WriteLine("Потрібна відповідь 1 або 0. Книгу не додано.");
				return;
			}

			// якщо книга видана, питаємо дату видачі
			DateTime issueDate = new DateTime();
			if (issued)
			{
				Console.Write("Дата видачі (дд.мм.рррр): ");
				if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy",
											CultureInfo.InvariantCulture,
											DateTimeStyles.None, out issueDate))
				{
					Console.WriteLine("Дату не розпізнано. Потрібен формат 05.11.2025. Книгу не додано.");
					return;
				}
			}

			Store(title, author, genre, copyCount, issueDate, issued);
			Console.WriteLine($"Додано книгу №{ids[count - 1]}. Зайнято комірок: {count} зі {Capacity}.");
		}

		// 2 - показати всі книги
		static void PrintAll()
		{
			if (count == 0)
			{
				Console.WriteLine("Книг поки немає.");
				return;
			}

			// шапка таблиці
			Console.WriteLine();
			Console.WriteLine($"{"№",3} | {"Назва",-20} | {"Автор",-16} | {"Жанр",-8} | {"Прим.",5} | {"Видано",-10} | Стан");
			Console.WriteLine("---------------------------------------------------------------------------------------");

			int totalCopies = 0; // всього примірників
			int overdue = 0;     // скільки книг видано понад OverdueDays днів

			for (int i = 0; i < count; i++)
			{
				// якщо книга видана, показуємо дату
				string dateText = "—";
				string state = "в наявності";
				if (isIssued[i])
				{
					dateText = issueDates[i].ToString("dd.MM.yyyy");
					state = "видана";
				}

				Console.WriteLine($"{ids[i],3} | {titles[i],-20} | {authors[i],-16} | {genres[i],-8} | " +
								  $"{copies[i],5} | {dateText,-10} | {state}");

				totalCopies += copies[i];

				// дні в масиві не зберігаємо, рахуємо тут
				if (isIssued[i] && (DateTime.Today - issueDates[i]).Days > OverdueDays)
				{
					overdue++;
				}
			}

			Console.WriteLine($"Усього записів: {count}, примірників: {totalCopies}, " +
							  $"видано понад {OverdueDays} днів: {overdue}.");
		}

		// 3 - знайти книгу за номером
		static void FindById()
		{
			Console.Write("Номер книги для пошуку: ");
			int id;
			if (!int.TryParse(Console.ReadLine(), out id))
			{
				Console.WriteLine("Номер має бути цілим числом.");
				return;
			}

			// шукаємо індекс книги
			int index = IndexOfId(id);
			if (index == -1)
			{
				Console.WriteLine($"Книги №{id} у списку немає.");
				return;
			}

			Console.WriteLine($"Знайдено в позиції {index}:");
			Console.WriteLine($"  номер:       {ids[index]}");
			Console.WriteLine($"  назва:       {titles[index]}");
			Console.WriteLine($"  автор:       {authors[index]}");
			Console.WriteLine($"  жанр:        {genres[index]}");
			Console.WriteLine($"  примірників: {copies[index]}");
			if (isIssued[index])
			{
				Console.WriteLine($"  видана:      так, з {issueDates[index]:dd.MM.yyyy}");
			}
			else
			{
				Console.WriteLine("  видана:      ні");
			}
		}

		// 4 - видалити книгу за номером
		static void DeleteById()
		{
			Console.Write("Номер книги для видалення: ");
			int id;
			if (!int.TryParse(Console.ReadLine(), out id))
			{
				Console.WriteLine("Номер має бути цілим числом.");
				return;
			}

			int index = IndexOfId(id);
			if (index == -1)
			{
				Console.WriteLine($"Книги №{id} у списку немає.");
				return;
			}

			Console.WriteLine($"Видалено книгу №{ids[index]} ({authors[index]} — «{titles[index]}»).");

			// зсуваємо всі книги після неї вліво, в усіх масивах
			for (int i = index; i < count - 1; i++)
			{
				ids[i] = ids[i + 1];
				titles[i] = titles[i + 1];
				authors[i] = authors[i + 1];
				genres[i] = genres[i + 1];
				copies[i] = copies[i + 1];
				issueDates[i] = issueDates[i + 1];
				isIssued[i] = isIssued[i + 1];
			}

			count--; // книг стало менше

			// очищаємо останню комірку, щоб там не лишилось старих даних
			ids[count] = 0;
			titles[count] = "";
			authors[count] = "";
			genres[count] = "";
			copies[count] = 0;
			issueDates[count] = new DateTime();
			isIssued[count] = false;

			Console.WriteLine($"Зайнято комірок: {count} зі {Capacity}.");
		}
	}
}