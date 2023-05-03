using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.VisualBasic;

using Npgsql;

using Randomize;

using static Generator.DataBase;

namespace Generator
{
    public class DataBase
    {
        #region Classes

        public class Category
        {
            public int category_id;
            public string category_name;

            public static Category[] GetCategories(String[] strings)
            {
                var categories = new List<Category>();
                int i = 0;
                foreach (var c in strings)
                {
                    categories.Add(new Category
                    {
                        category_id = i,
                        category_name = c
                    });
                    i++;
                }

                return categories.ToArray();
            }

            public override string ToString()
            {
                return $"({category_id + 1}, \'{category_name}\')";
            }
        }

        public class Disease
        {
            public int disease_id;
            public string disease_name;

            public static Disease[] GetDiseases(String[] strings)
            {
                var diseases = new List<Disease>();
                int i = 0;
                foreach (var c in strings)
                {
                    diseases.Add(new Disease
                    {
                        disease_id = i,
                        disease_name = c
                    });
                    i++;
                }

                return diseases.ToArray();
            }

            public override string ToString()
            {
                return $"({disease_id + 1}, \'{disease_name}\')";
            }
        }

        public class JobTitle
        {
            public int jobTitle_id;
            public string jobTitle_name;

            public static JobTitle[] GetJobTitles(String[] strings)
            {
                var jobTitles = new List<JobTitle>();
                int i = 0;
                foreach (var c in strings)
                {
                    jobTitles.Add(new JobTitle
                    {
                        jobTitle_id = i,
                        jobTitle_name = c
                    });
                    i++;
                }

                return jobTitles.ToArray();
            }

            public override string ToString()
            {
                return $"({jobTitle_id + 1}, \'{jobTitle_name}\')";
            }
        }

        public class Procedure
        {
            public int procedure_id;
            public string procedure_name;

            public static Procedure[] GetProcedures(String[] strings)
            {
                var procedures = new List<Procedure>();
                int i = 0;
                foreach (var c in strings)
                {
                    procedures.Add(new Procedure
                    {
                        procedure_id = i,
                        procedure_name = c
                    });
                    i++;
                }

                return procedures.ToArray();
            }

            public override string ToString()
            {
                return $"({procedure_id + 1}, \'{procedure_name}\')";
            }
        }

        public class SocialStatus
        {
            public int socialStatus_id;
            public string socialStatus_name;

            public static SocialStatus[] GetSocialStatuses(String[] strings)
            {
                var socialStatuses = new List<SocialStatus>();
                int i = 0;
                foreach (var c in strings)
                {
                    socialStatuses.Add(new SocialStatus
                    {
                        socialStatus_id = i,
                        socialStatus_name = c
                    });
                    i++;
                }

                return socialStatuses.ToArray();
            }

            public override string ToString()
            {
                return $"({socialStatus_id + 1}, \'{socialStatus_name}\')";
            }
        }

        public class Department
        {
            public int department_id;
            public string department_name;
            public int department_beds;
            public string department_phone;
            public bool department_exists;

            public static Department[] GenerateDepartments(int count, String[] department_names,
                int lowest_bed_count_boundary, int highest_bed_count_boundary)
            {
                var rnd = new Random();
                var departments = new List<Department>();

                var selected_departments = StaticMethods<String>.GetRandomElements(department_names, count);
                
                for (int i = 0; i < count; i++)
                {
                    departments.Add(new Department
                    {
                        department_id = i,
                        department_name = selected_departments[i],
                        department_beds = rnd.Next(lowest_bed_count_boundary, highest_bed_count_boundary),
                        department_phone = String.Join("", 
                        "+7 (", rnd.Next(0, 9).ToString(), rnd.Next(0, 9).ToString(), rnd.Next(0, 9).ToString(), ") ", 
                        rnd.Next(0, 9).ToString(), rnd.Next(0, 9).ToString(), rnd.Next(0, 9).ToString(), "-",
                        rnd.Next(0, 9).ToString(), rnd.Next(0, 9).ToString(), "-",
                        rnd.Next(0, 9).ToString(), rnd.Next(0, 9).ToString()),
                        department_exists = true
                    });
                }

                return departments.ToArray();
            }


            public override string ToString()
            {
                var is_exists = department_exists ? "true" : "false";
                return $"({department_id + 1}, \'{department_name}\', {department_beds}, \'{department_phone}\', \'{is_exists}\')";
            }
        }

        public class Staff
        {
            public int staff_id;
            public string staff_Name;
            public string staff_Surname;
            public string staff_Patronymic;
            public int staff_Department_id;
            public int staff_Category_id;
            public DateOnly staff_EmploymentDate;
            public int staff_Salary;
            public int staff_JobTitle_id;
            public string staff_login;

            public static Staff[] GenerateStaffList(int count, Department[] departments,
                Category[] categories, JobTitle[] jobTitles, PartOfName[] firstNames, 
                PartOfName[] midNames, PartOfName[] lastNames)
            {
                var rnd = new Random();
                var staff_list = new List<Staff>();

                int count_per_department = count / departments.Length;
                if (count_per_department is 0)
                    throw new Exception();

                int index = 0;

                // Глав врач
                var rnd_date = DateOnly.FromDateTime(StaticMethods<object>
                        .GetRandomDateInRange(new DateTime(1960, 1, 1), DateTime.Now));
                var fio = PartOfName.GenerateFIO(firstNames, midNames, lastNames);
                var login = StaticMethods<object>.GenerateRandomString(12);
                while (staff_list.Where(x => x.staff_login == login).Any())
                {
                    login = StaticMethods<object>.GenerateRandomString(12);
                }
                staff_list.Add(new Staff
                {
                    staff_id = index,
                    staff_Department_id = -1,
                    staff_Category_id = categories[rnd.Next(categories.Length)].category_id,
                    staff_JobTitle_id = jobTitles.Where(x => x.jobTitle_name == "Главный врач").First().jobTitle_id,
                    staff_EmploymentDate = rnd_date,
                    staff_Salary = rnd.Next(15000, 50000),
                    staff_Name = fio.Item2,
                    staff_Surname = fio.Item1,
                    staff_Patronymic = fio.Item3,
                    staff_login = login
                });
                index++;

                // Администратор
                rnd_date = DateOnly.FromDateTime(StaticMethods<object>
                        .GetRandomDateInRange(new DateTime(1960, 1, 1), DateTime.Now));
                fio = PartOfName.GenerateFIO(firstNames, midNames, lastNames);
                login = StaticMethods<object>.GenerateRandomString(12);
                while (staff_list.Where(x => x.staff_login == login).Any())
                {
                    login = StaticMethods<object>.GenerateRandomString(12);
                }
                staff_list.Add(new Staff
                {
                    staff_id = index,
                    staff_Department_id = -1,
                    staff_Category_id = categories.Where(x => x.category_name == "Нет").First().category_id,
                    staff_JobTitle_id = jobTitles.Where(x => x.jobTitle_name == "Администратор").First().jobTitle_id,
                    staff_EmploymentDate = rnd_date,
                    staff_Salary = rnd.Next(15000, 50000),
                    staff_Name = fio.Item2,
                    staff_Surname = fio.Item1,
                    staff_Patronymic = fio.Item3,
                    staff_login = login
                });
                index++;

                foreach (var department in departments)
                {
                    for (int i = 0; i < count_per_department; i++)
                    {
                        rnd_date = DateOnly.FromDateTime(StaticMethods<object>
                            .GetRandomDateInRange(new DateTime(1960, 1, 1), DateTime.Now));
                        fio = PartOfName.GenerateFIO(firstNames, midNames, lastNames);
                        login = StaticMethods<object>.GenerateRandomString(12);
                        while (staff_list.Where(x => x.staff_login == login).Any())
                        {
                            login = StaticMethods<object>.GenerateRandomString(12);
                        }
                        staff_list.Add(new Staff
                        {
                            staff_id = index,
                            staff_Department_id = department.department_id,
                            staff_Category_id = categories[rnd.Next(categories.Length)].category_id,
                            staff_JobTitle_id = jobTitles.Where(x => x.jobTitle_name == "Врач").First().jobTitle_id,
                            staff_EmploymentDate = rnd_date,
                            staff_Salary = rnd.Next(15000, 50000),
                            staff_Name = fio.Item2,
                            staff_Surname = fio.Item1,
                            staff_Patronymic = fio.Item3,
                            staff_login = login
                        });
                        index++;
                    }
                    // Зав отделения
                    rnd_date = DateOnly.FromDateTime(StaticMethods<object>
                            .GetRandomDateInRange(new DateTime(1960, 1, 1), DateTime.Now));
                    fio = PartOfName.GenerateFIO(firstNames, midNames, lastNames);
                    login = StaticMethods<object>.GenerateRandomString(12);
                    while (staff_list.Where(x => x.staff_login == login).Any())
                    {
                        login = StaticMethods<object>.GenerateRandomString(12);
                    }
                    staff_list.Add(new Staff
                    {
                        staff_id = index,
                        staff_Department_id = department.department_id,
                        staff_Category_id = categories[rnd.Next(categories.Length)].category_id,
                        staff_JobTitle_id = jobTitles.Where(x => x.jobTitle_name == "Заведующий отделением").First().jobTitle_id,
                        staff_EmploymentDate = rnd_date,
                        staff_Salary = rnd.Next(15000, 50000),
                        staff_Name = fio.Item2,
                        staff_Surname = fio.Item1,
                        staff_Patronymic = fio.Item3,
                        staff_login = login
                    });
                    index++;
                }

                return staff_list.ToArray();
            }

            public override string ToString()
            {
                if (staff_Department_id is -1)
                {
                    return $"({staff_id + 1}, \'{staff_Name}\', '{staff_Surname}', '{staff_Patronymic}', NULL, {staff_Category_id + 1}, \'{staff_EmploymentDate:dd-MM-yyyy}\', {staff_Salary}, {staff_JobTitle_id + 1}, \'{staff_login}\')";
                }
                else
                {
                    return $"({staff_id + 1}, \'{staff_Name}\', '{staff_Surname}', '{staff_Patronymic}', {staff_Department_id + 1}, {staff_Category_id + 1}, \'{staff_EmploymentDate:dd-MM-yyyy}\', {staff_Salary}, {staff_JobTitle_id + 1}, \'{staff_login}\')";
                }
            }
        }

        public class Patient
        {
            public int patient_id;
            public string patient_Name;
            public string patient_Surname;
            public string patient_Patronymic;
            public DateOnly patient_Birthday;
            public int patient_SocialStatus_id;
            public int patient_CurrentDoctor_id;

            public static Patient[] GeneratePatients(int count, SocialStatus[] socialStatuses, Staff[] staff,
                PartOfName[] firstNames, PartOfName[] midNames, PartOfName[] lastNames, JobTitle[] jobTitles)
            {
                var rnd = new Random();
                var patients = new List<Patient>();

                var job_id = jobTitles.Where(x => x.jobTitle_name == "Врач").First().jobTitle_id;

                var doctors = staff.Where(x => x.staff_JobTitle_id == job_id).ToArray();

                for (int i = 0; i < count; i++)
                {
                    var rnd_date = DateOnly.FromDateTime(StaticMethods<object>
                            .GetRandomDateInRange(new DateTime(1960, 1, 1), DateTime.Now));
                    var fio = PartOfName.GenerateFIO(firstNames, midNames, lastNames);

                    patients.Add(new Patient
                    {
                        patient_id = i,
                        patient_Birthday = rnd_date,
                        patient_SocialStatus_id = socialStatuses[rnd.Next(socialStatuses.Length)].socialStatus_id,
                        patient_CurrentDoctor_id = doctors[rnd.Next(doctors.Length)].staff_id,
                        patient_Name = fio.Item2,
                        patient_Surname = fio.Item1,
                        patient_Patronymic = fio.Item3
                    });
                }

                return patients.ToArray();
            }

            public override string ToString()
            {
                return $"({patient_id + 1}, \'{patient_Name}\', '{patient_Surname}', '{patient_Patronymic}', \'{patient_Birthday:dd-MM-yyyy}\', {patient_SocialStatus_id + 1}, {patient_CurrentDoctor_id + 1})";
            }
        }

        public class PatientDiseases
        {
            public int patientDiseases_id;
            public int patientDiseases_patient_id;
            public int patientDiseases_decease_id;
            public bool patientDiseases_isCured;
            public DateOnly patientDiseases_Start_Date;
            public DateOnly patientDiseases_End_Date;

            public static PatientDiseases[] GeneratePatientDiseases(int count_per_patient,
                Patient[] patients, Disease[] diseases)
            {
                var rnd = new Random();
                var patient_diseases = new List<PatientDiseases>();

                foreach (var patient in patients)
                {
                    for (int i = 0; i < count_per_patient; i++)
                    {
                        bool isCured = rnd.Next(4) is not 0;
                        var start = StaticMethods<int>.GetRandomDateInRange(patient.patient_Birthday.ToDateTime(TimeOnly.MinValue), DateTime.Now);
                        patient_diseases.Add(new PatientDiseases()
                        {
                            patientDiseases_id = patient_diseases.Count,
                            patientDiseases_patient_id = patient.patient_id,
                            patientDiseases_decease_id = diseases[rnd.Next(diseases.Length)].disease_id,
                            patientDiseases_isCured = isCured,
                            patientDiseases_Start_Date = DateOnly.FromDateTime(start),
                            patientDiseases_End_Date = DateOnly.FromDateTime(
                                StaticMethods<int>.GetRandomDateInRange(start, DateTime.Now))
                        });
                    }
                }

                return patient_diseases.ToArray();
            }

            public override string ToString()
            {
                if (patientDiseases_isCured)
                {
                    return $"({patientDiseases_id + 1}, {patientDiseases_patient_id + 1}, {patientDiseases_decease_id + 1}, '{patientDiseases_Start_Date:dd-MM-yyyy}', '{patientDiseases_End_Date:dd-MM-yyyy}')";
                }
                else
                {
                    return $"({patientDiseases_id + 1}, {patientDiseases_patient_id + 1}, {patientDiseases_decease_id + 1}, '{patientDiseases_Start_Date:dd-MM-yyyy}', NULL)";
                }
                
            }
        }

        public class HospitalStay
        {
            public int hospitalStay_id;
            public int hospitalStay_Patient_id;
            public DateOnly hospitalStay_Start_Date;
            public DateOnly hospitalStay_End_Date;
            public int hospitalStay_Cost;
            public int hospitalStay_Department_id;

            public static HospitalStay[] GenerateHospitalStays(int count_per_patient,
                Patient[] patients, Department[] departments)
            {
                var rnd = new Random();
                var hospitalStays = new List<HospitalStay>();

                foreach (var patient in patients)
                {
                    for (int i = 0; i < count_per_patient; i++)
                    {
                        var dep = departments[rnd.Next(departments.Length)];
                        var start = StaticMethods<int>.GetRandomDateInRange(patient.patient_Birthday.ToDateTime(TimeOnly.MinValue), DateTime.Now);
                        var end = DateOnly.FromDateTime(
                                StaticMethods<int>.GetRandomDateInRange(start, DateTime.Now));
                        var c = hospitalStays.Where(x => x.hospitalStay_Department_id == dep.department_id).Count();
                        hospitalStays.Add(new HospitalStay()
                        {
                            hospitalStay_id = hospitalStays.Count,
                            hospitalStay_Patient_id = patient.patient_id,
                            hospitalStay_Department_id = dep.department_id,
                            hospitalStay_Cost = rnd.Next(10000),
                            hospitalStay_Start_Date = DateOnly.FromDateTime(start),
                            hospitalStay_End_Date = c < dep.department_beds ? end : DateOnly.MinValue
                        });
                    }
                }

                return hospitalStays.ToArray();
            }

            public override string ToString()
            {
                var str_end_date_isNull = hospitalStay_End_Date == DateOnly.MinValue;

                if (str_end_date_isNull)
                {
                    return $"({hospitalStay_id + 1}, {hospitalStay_Patient_id + 1}, '{hospitalStay_Start_Date:dd-MM-yyyy}', NULL, {hospitalStay_Cost}, {hospitalStay_Department_id + 1})";
                }
                else
                {
                    return $"({hospitalStay_id + 1}, {hospitalStay_Patient_id + 1}, '{hospitalStay_Start_Date:dd-MM-yyyy}', '{hospitalStay_End_Date:dd-MM-yyyy}', {hospitalStay_Cost}, {hospitalStay_Department_id + 1})";
                }
            }
        }

        public class DoctorAppointment
        {
            public int doctorAppointment_id;
            public int doctorAppointment_procedure_id;
            public int doctorAppointment_patient_id;
            public int doctorAppointment_doctor_id;
            public TimeSpan doctorAppointment_interval;
            public DateTime doctorAppointment_Start_Date;
            public int doctorAppointment_Count;

            public static DoctorAppointment[] GenerateDoctorAppointments(int count_per_patient,
                Patient[] patients, Staff[] staff, Procedure[] procedures, JobTitle[] jobTitles)
            {
                var rnd = new Random();
                var doctorAppointments = new List<DoctorAppointment>();
                
                var job_id = jobTitles.Where(x => x.jobTitle_name == "Врач").First().jobTitle_id;

                var doctors = staff.Where(x => x.staff_JobTitle_id == job_id).ToArray();

                foreach (var patient in patients)
                {
                    for (int i = 0; i < count_per_patient; i++)
                    {
                        var start = StaticMethods<int>.GetRandomDateTimeInRange(patient.patient_Birthday.ToDateTime(TimeOnly.MinValue), DateTime.Now);
                        var interval = TimeSpan.FromHours(rnd.Next(1, 100));
                        doctorAppointments.Add(new DoctorAppointment()
                        {
                            doctorAppointment_id = doctorAppointments.Count,
                            doctorAppointment_patient_id = patient.patient_id,
                            doctorAppointment_doctor_id = doctors[rnd.Next(doctors.Length)].staff_id,
                            doctorAppointment_procedure_id = procedures[rnd.Next(procedures.Length)].procedure_id,
                            doctorAppointment_Start_Date = start,
                            doctorAppointment_interval = interval,
                            doctorAppointment_Count = rnd.Next(1, 100)
                        });
                    }
                }

                return doctorAppointments.ToArray();
            }

            public override string ToString()
            {
                return $"({doctorAppointment_id + 1}, {doctorAppointment_procedure_id + 1}, {doctorAppointment_patient_id + 1}, {doctorAppointment_doctor_id + 1}, \'{doctorAppointment_interval}\', \'{doctorAppointment_Start_Date:dd-MM-yyyy HH:mm:ss}\', {doctorAppointment_Count})";
            }
        }

        #endregion

        public Category[] Categories;
        public Disease[] Diseases;
        public JobTitle[] JobTitles;
        public Procedure[] Procedures;
        public SocialStatus[] SocialStatuses;
        public Department[] Departments;
        public Staff[] Staff_list;
        public Patient[] Patients;
        public PatientDiseases[] PatientDiseases_list;
        public HospitalStay[] HospitalStays;
        public DoctorAppointment[] DoctorAppointments;

        public static DataBase GenerateDataBase(int count)
        {
            var db = new DataBase();

            var path = Environment.CurrentDirectory;

            Console.WriteLine($"{DateTime.Now}: Загрузка данных для генерации");
            var first_names = PartOfName.DeserializePartsOfName(path + "\\Data\\Firstnames.xml");
            var second_names = PartOfName.DeserializePartsOfName(path + "\\Data\\Midnames.xml");
            var last_names = PartOfName.DeserializePartsOfName(path + "\\Data\\Lastnames.xml");

            Console.WriteLine($"{DateTime.Now}: Генерация данных");
            Console.WriteLine($"{DateTime.Now}: Генерация категорий");
            db.Categories = Category.GetCategories(File.ReadAllLines(path + "\\Data\\Category.txt"));
            Console.WriteLine($"{DateTime.Now}: Генерация болезней");
            db.Diseases = Disease.GetDiseases(File.ReadAllLines(path + "\\Data\\Diseases.txt"));
            Console.WriteLine($"{DateTime.Now}: Генерация должностей");
            db.JobTitles = JobTitle.GetJobTitles(File.ReadAllLines(path + "\\Data\\JobTitle.txt"));
            Console.WriteLine($"{DateTime.Now}: Генерация процедур");
            db.Procedures = Procedure.GetProcedures(File.ReadAllLines(path + "\\Data\\Procedures.txt"));
            Console.WriteLine($"{DateTime.Now}: Генерация социальных статусов");
            db.SocialStatuses = SocialStatus.GetSocialStatuses(File.ReadAllLines(path + "\\Data\\SocialStatus.txt"));
            Console.WriteLine($"{DateTime.Now}: Генерация отделений");
            db.Departments = Department.GenerateDepartments(10, File.ReadAllLines(path + "\\Data\\Department.txt"), 15, 50);
            Console.WriteLine($"{DateTime.Now}: Генерация сотрудников");
            db.Staff_list = Staff.GenerateStaffList(150, db.Departments, db.Categories, db.JobTitles, first_names, second_names, last_names);
            Console.WriteLine($"{DateTime.Now}: Генерация пациентов");
            db.Patients = Patient.GeneratePatients(500, db.SocialStatuses, db.Staff_list, first_names, second_names, last_names, db.JobTitles);
            Console.WriteLine($"{DateTime.Now}: Генерация болезней пациентов");
            db.PatientDiseases_list = PatientDiseases.GeneratePatientDiseases(5, db.Patients, db.Diseases);
            Console.WriteLine($"{DateTime.Now}: Генерация нахождений пациентов");
            db.HospitalStays = HospitalStay.GenerateHospitalStays(5, db.Patients, db.Departments);
            Console.WriteLine($"{DateTime.Now}: Генерация назначений врачей");
            db.DoctorAppointments = DoctorAppointment.GenerateDoctorAppointments(10, db.Patients, db.Staff_list, db.Procedures, db.JobTitles);
            Console.WriteLine($"{DateTime.Now}: Генерация данных завершена");

            return db;
        }

        public void Send(string connectString = "Host=localhost;Port=5432;User Id=postgres;Password=1310;Database=postgres;Timeout=300;CommandTimeout=300;")
        {
            var conn = new NpgsqlConnection(connectString);
            string sql;
            NpgsqlCommand cmd;

            string[] toClear = 
            { 
                "DELETE FROM \"DoctorAppointment\"",
                "DELETE FROM \"HospitalStay\"",
                "DELETE FROM \"PatientDiseases\"",
                "DELETE FROM \"Patient\"",
                "DELETE FROM \"Staff\"",
                "DELETE FROM \"Department\"",
                "DELETE FROM \"SocialStatus\"",
                "DELETE FROM \"Procedure\"",
                "DELETE FROM \"JobTitle\"",
                "DELETE FROM \"Disease\"",
                "DELETE FROM \"Category\""
            };

            var firstParts = new Dictionary<string, string>()
            {
                { "Category", "INSERT INTO \"Category\" (\"Category_Id\", \"Category_Name\") VALUES " },
                { "Disease", "INSERT INTO \"Disease\" (\"Disease_Id\", \"Disease_Name\") VALUES " },
                { "JobTitle", "INSERT INTO \"JobTitle\" (\"JobTitle_Id\", \"JobTitle_Name\") VALUES " },
                { "Procedure", "INSERT INTO \"Procedure\" (\"Procedure_Id\", \"Procedure_Name\") VALUES " },
                { "SocialStatus", "INSERT INTO \"SocialStatus\" (\"SocialStatus_Id\", \"SocialStatus_Name\") VALUES " },
                { "Department", "INSERT INTO \"Department\" (\"Department_Id\", \"Department_Name\", \"Department_Beds\", \"Department_Phone\", \"Department_Exists\") VALUES " },
                { "Staff", "INSERT INTO \"Staff\" (\"Staff_Id\", \"Staff_Name\", \"Staff_Surname\", \"Staff_Patronymic\", \"Staff_Department_Id\", \"Staff_Category_Id\", \"Staff_EmploymentDate\", \"Staff_Salary\", \"Staff_JobTitle_Id\", \"Staff_Login\") VALUES " },
                { "Patient", "INSERT INTO \"Patient\" (\"Patient_Id\", \"Patient_Name\", \"Patient_Surname\", \"Patient_Patronymic\", \"Patient_BirthDay\", \"Patient_SocialStatus_Id\", \"Patient_CurrentDoctor_Id\") VALUES " },
                { "PatientDiseases", "INSERT INTO \"PatientDiseases\" (\"PatientDiseases_Id\", \"PatientDiseases_Patient_Id\", \"PatientDiseases_Disease_Id\", \"PatientDiseases_Start_Date\", \"PatientDiseases_End_Date\") VALUES " },
                { "HospitalStay", "INSERT INTO \"HospitalStay\" (\"HospitalStay_Id\", \"HospitalStay_Patient_Id\", \"HospitalStay_Start_Date\", \"HospitalStay_End_Date\", \"HospitalStay_Cost\", \"HospitalStay_Department_Id\") VALUES " },
                { "DoctorAppointment", "INSERT INTO \"DoctorAppointment\" (\"DoctorAppointment_Id\", \"DoctorAppointment_Procedure_Id\", \"DoctorAppointment_Patient_id\", \"DoctorAppointment_Doctor_Id\", \"DoctorAppointment_Interval\", \"DoctorAppointment_Start_Date\", \"DoctorAppointment_Count\") VALUES " }
            };

            var Sequences = new Dictionary<string, string>()
            {
                { "Category", $"ALTER SEQUENCE \"Category_Category_Id_seq\" RESTART WITH {this.Categories.Length}" },
                { "Disease", $"ALTER SEQUENCE \"Disease_Disease_Id_seq\" RESTART WITH {this.Diseases.Length}" },
                { "JobTitle", $"ALTER SEQUENCE \"JobTitle_JobTitle_Id_seq\" RESTART WITH  {this.JobTitles.Length}" },
                { "Procedure", $"ALTER SEQUENCE \"Procedure_procedure_id_seq\" RESTART WITH  {this.Procedures.Length}" },
                { "SocialStatus", $"ALTER SEQUENCE \"SocialStatus_SocialStatus_id_seq\" RESTART WITH  {this.SocialStatuses.Length}" },
                { "Department", $"ALTER SEQUENCE \"Department_Department_id_seq\" RESTART WITH {this.Departments.Length}" },
                { "Staff", $"ALTER SEQUENCE \"Staff_Staff_id_seq\" RESTART WITH  {this.Staff_list.Length}" },
                { "Patient", $"ALTER SEQUENCE \"Patient_Patient_Id_seq\" RESTART WITH  {this.Patients.Length}" },
                { "PatientDiseases", $"ALTER SEQUENCE \"PatientDeceases_PatientDeceases_Id_seq\" RESTART WITH  {this.PatientDiseases_list.Length}" },
                { "HospitalStay", $"ALTER SEQUENCE \"HospitalStay_HospitalStay_Id_seq\" RESTART WITH  {this.HospitalStays.Length}" },
                { "DoctorAppointment", $"ALTER SEQUENCE \"DoctorAppointment_DoctorAppointment_Id_seq\" RESTART WITH  {this.DoctorAppointments.Length}" }
            };

            var Sequences_next = new Dictionary<string, string>()
            {
                { "Category", "Select nextval ('\"Category_Category_Id_seq\"')" },
                { "Disease", "Select nextval ('\"Disease_Disease_Id_seq\"')" },
                { "JobTitle", "Select nextval ('\"JobTitle_JobTitle_Id_seq\"')" },
                { "Procedure", "Select nextval ('\"Procedure_procedure_id_seq\"')" },
                { "SocialStatus", "Select nextval ('\"SocialStatus_SocialStatus_id_seq\"')" },
                { "Department", "Select nextval ('\"Department_Department_id_seq\"')" },
                { "Staff", "Select nextval ('\"Staff_Staff_id_seq\"')" },
                { "Patient", "Select nextval ('\"Patient_Patient_Id_seq\"')" },
                { "PatientDiseases", "Select nextval ('\"PatientDeceases_PatientDeceases_Id_seq\"')" },
                { "HospitalStay", "Select nextval ('\"HospitalStay_HospitalStay_Id_seq\"')" },
                { "DoctorAppointment", "Select nextval ('\"DoctorAppointment_DoctorAppointment_Id_seq\"')" },
            };

            try
            {
                Console.WriteLine($"{DateTime.Now}: Установление связи с сервером");
                conn.Open();

                Console.WriteLine($"{DateTime.Now}: Очистка данных на сервере");
                foreach (var line in toClear)
                {
                    cmd = new NpgsqlCommand(line, conn);
                    cmd.ExecuteNonQuery();
                }

                sql = "SELECT \'DROP ROLE IF EXISTS \' || usename FROM pg_user WHERE usename NOT IN(\'postgres\')";
                cmd = new NpgsqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                List<string> dropQueries = new List<string>();

                while (reader.Read())
                {
                    string dropQuery = reader.GetString(0);
                    dropQuery = dropQuery.Replace("EXISTS ", "EXISTS \"") + "\"";
                    dropQueries.Add(dropQuery);
                }
                reader.Close();

                foreach (var line in dropQueries)
                {
                    cmd = new NpgsqlCommand(line, conn);
                    cmd.ExecuteNonQuery();
                }


                Console.WriteLine($"{DateTime.Now}: Генерация команд для Category");
                sql = firstParts["Category"] + String.Join(",\n", this.Categories.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для Category");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для Disease");
                sql = firstParts["Disease"] + String.Join(",\n", this.Diseases.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для Disease");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для JobTitle");
                sql = firstParts["JobTitle"] + String.Join(",\n", this.JobTitles.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для JobTitle");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для Procedure");
                sql = firstParts["Procedure"] + String.Join(",\n", this.Procedures.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для Procedure");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для SocialStatus");
                sql = firstParts["SocialStatus"] + String.Join(",\n", this.SocialStatuses.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для SocialStatus");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для Department");
                sql = firstParts["Department"] + String.Join(",\n", this.Departments.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для Department");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для Staff");
                sql = firstParts["Staff"] + String.Join(",\n", this.Staff_list.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для Staff");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для Patient");
                sql = firstParts["Patient"] + String.Join(",\n", this.Patients.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для Patient");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для PatientDiseases");
                sql = firstParts["PatientDiseases"] + String.Join(",\n", this.PatientDiseases_list.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для PatientDiseases");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для HospitalStay");
                sql = firstParts["HospitalStay"] + String.Join(",\n", this.HospitalStays.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для HospitalStay");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Генерация команд для DoctorAppointment");
                sql = firstParts["DoctorAppointment"] + String.Join(",\n", this.DoctorAppointments.Select(x => x.ToString()).ToArray());
                Console.WriteLine($"{DateTime.Now}: Выполнение команд для DoctorAppointment");
                new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                //Console.WriteLine($"{DateTime.Now}: Создание пользователей в БД");
                //sql = "Select " + String.Join(", ",
                //    this.Staff_list.Select(x => $"create_user('{x.staff_login}', '{x.staff_login}')"));
                //new NpgsqlCommand(sql, conn).ExecuteNonQuery();
                //sql = $"Select grant_user_admin(\'{this.Staff_list[1].staff_login}\')";
                //new NpgsqlCommand(sql, conn).ExecuteNonQuery();

                Console.WriteLine($"{DateTime.Now}: Выполнение команд для перестановки перечеслений");
                foreach (var line in Sequences.Values)
                {
                    cmd = new NpgsqlCommand(line, conn);
                    cmd.ExecuteNonQuery();
                }
                foreach (var line in Sequences_next.Values)
                {
                    cmd = new NpgsqlCommand(line, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception except)
            {
                Console.WriteLine($"{DateTime.Now}: Ошибка на сервере: {except.Message}\n\n{except.StackTrace}");
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
