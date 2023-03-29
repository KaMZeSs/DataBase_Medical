using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase_Medical.Windows
{
    public class HospitalStaff // Класс для примера интерфейса
    {
        //public int id;
        public string Имя { get; set; }
        public string Фамилия { get; set; }
        public string Отчество { get; set; }
        public string Отделение { get; set; }
        public string Категория { get; set; }
        public string Дата_принятия_на_работу { get; set; }
        public string Зарплата { get; set; }
    }

    public class HospitalStaff_Small // Класс для примера интерфейса
    {
        //public int id;
        public string ФИО { get; set; }
        public string Отделение { get; set; }
        public string Категория { get; set; }
    }

    public class Ill // Класс для примера интерфейса
    {
        //public int id;
        public string Имя { get; set; }
        public string Фамилия { get; set; }
        public string Отчество { get; set; }
        public string Дата_рождения { get; set; }
        public string Социальное_положение { get; set; }
        public string Лечащий_врач { get; set; }
    }

    public class Categories // Класс для примера интерфейса
    {
        //public int id;
        public string Название_категории { get; set; }
    }

}
