using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase_Medical
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
}
