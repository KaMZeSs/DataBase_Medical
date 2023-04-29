using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    internal class StaticMethods<T>
    {
        public static T[] GetRandomElements(T[] objects, int count)
        {
            if (objects.Length < count)
                throw new ArgumentException();

            var selected = new List<T>(); // выбранные элементы

            var random = new Random();

            while (selected.Count < count)
            {
                int index = random.Next(objects.Length); // получаем случайный индекс в массиве
                T value = objects[index];

                if (!selected.Contains(value))
                {
                    selected.Add(value); // добавляем выбранный элемент в список
                }
            }

            return selected.ToArray();
        }

        public static DateTime GetRandomDateInRange(DateTime startDate, DateTime endDate)
        {
            Random random = new Random();
            int range = (endDate - startDate).Days;
            DateTime randomDate = startDate.AddDays(random.Next(range));
            return randomDate;
        }

        public static DateTime GetRandomDateTimeInRange(DateTime startDate, DateTime endDate)
        {
            //Random random = new Random();
            //int range = (endDate - startDate).Milliseconds;
            //DateTime randomDate = startDate.AddMilliseconds(random.Next(range));
            //return randomDate;

            Random random = new Random();
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan randomSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime randomDate = startDate + randomSpan;

            return randomDate;
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var login = new char[length];
            for (int i = 0; i < length; i++)
            {
                login[i] = chars[random.Next(chars.Length)];
            }
            return new string(login);
        }
    }
}
