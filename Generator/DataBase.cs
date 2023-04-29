using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualBasic;

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

            var first_names = PartOfName.DeserializePartsOfName(path + "\\Data\\Firstnames.xml");
            var second_names = PartOfName.DeserializePartsOfName(path + "\\Data\\Midnames.xml");
            var last_names = PartOfName.DeserializePartsOfName(path + "\\Data\\Lastnames.xml");

            db.Categories = Category.GetCategories(File.ReadAllLines(path + "\\Data\\Category.txt"));
            db.Diseases = Disease.GetDiseases(File.ReadAllLines(path + "\\Data\\Diseases.txt"));
            db.JobTitles = JobTitle.GetJobTitles(File.ReadAllLines(path + "\\Data\\JobTitle.txt"));
            db.Procedures = Procedure.GetProcedures(File.ReadAllLines(path + "\\Data\\Procedures.txt"));
            db.SocialStatuses = SocialStatus.GetSocialStatuses(File.ReadAllLines(path + "\\Data\\SocialStatus.txt"));
            
            db.Departments = Department.GenerateDepartments(10, File.ReadAllLines(path + "\\Data\\Department.txt"), 15, 50);
            db.Staff_list = Staff.GenerateStaffList(150, db.Departments, db.Categories, db.JobTitles, first_names, second_names, last_names);
            db.Patients = Patient.GeneratePatients(500, db.SocialStatuses, db.Staff_list, first_names, second_names, last_names, db.JobTitles);
            db.PatientDiseases_list = PatientDiseases.GeneratePatientDiseases(5, db.Patients, db.Diseases);
            db.HospitalStays = HospitalStay.GenerateHospitalStays(5, db.Patients, db.Departments);
            db.DoctorAppointments = DoctorAppointment.GenerateDoctorAppointments(10, db.Patients, db.Staff_list, db.Procedures, db.JobTitles);

            return db;
        }
    }
}
