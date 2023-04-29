CREATE TABLE public."Procedure"
(
    procedure_id serial NOT NULL,
    procedure_name text NOT NULL,
    PRIMARY KEY (procedure_id)
);

ALTER TABLE IF EXISTS public."Procedure"
    OWNER to postgres;


CREATE TABLE public."SocialStatus"
(
    "SocialStatus_id" serial NOT NULL,
    "SocialStatus_Name" text NOT NULL,
    PRIMARY KEY ("SocialStatus_id")
);

ALTER TABLE IF EXISTS public."SocialStatus"
    OWNER to postgres;


CREATE TABLE public."Disease"
(
    "Disease_Id" serial NOT NULL,
    "Disease_Name" text NOT NULL,
    PRIMARY KEY ("Disease_Id")
);

ALTER TABLE IF EXISTS public."Disease"
    OWNER to postgres;


CREATE TABLE public."Category"
(
    "Category_Id" serial NOT NULL,
    "Category_Name" text NOT NULL,
    PRIMARY KEY ("Category_Id")
);

ALTER TABLE IF EXISTS public."Category"
    OWNER to postgres;


CREATE TABLE public."JobTitle"
(
    "JobTitle_Id" serial NOT NULL,
    "JobTitle_Name" text NOT NULL,
    PRIMARY KEY ("JobTitle_Id")
);

ALTER TABLE IF EXISTS public."JobTitle"
    OWNER to postgres;


CREATE TABLE public."Department"
(
    "Department_id" serial NOT NULL,
    "Department_Name" text NOT NULL,
    "Department_Beds" integer NOT NULL,
    "Department_Phone" text NOT NULL,
    "Department_Exists" boolean NOT NULL DEFAULT True,
    PRIMARY KEY ("Department_id")
);

ALTER TABLE IF EXISTS public."Department"
    OWNER to postgres;


CREATE TABLE public."Staff"
(
    "Staff_id" serial NOT NULL,
    "Staff_Name" text NOT NULL,
    "Staff_Surname" text NOT NULL,
    "Staff_Patronymic" text NOT NULL,
    "Staff_Department_Id" integer,
    "Staff_Category_Id" integer,
    "Staff_EmploymentDate" date NOT NULL,
    "Staff_Salary" integer NOT NULL,
    "Staff_JobTitle_Id" integer NOT NULL,
    "Staff_login" text,
    PRIMARY KEY ("Staff_id"),
    CONSTRAINT "Staff_Department_id" FOREIGN KEY ("Staff_Department_Id")
        REFERENCES public."Department" ("Department_id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "Staff_Category_Id" FOREIGN KEY ("Staff_Category_Id")
        REFERENCES public."Category" ("Category_Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "Staff_JobTitle_Id" FOREIGN KEY ("Staff_JobTitle_Id")
        REFERENCES public."JobTitle" ("JobTitle_Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public."Staff"
    OWNER to postgres;


CREATE TABLE public."Patient"
(
    "Patient_Id" serial NOT NULL,
    "Patient_Name" text NOT NULL,
    "Patient_Surname" text NOT NULL,
    "Patient_Patronymic" text NOT NULL,
    "Patient_BirthDay" date NOT NULL,
    "Patient_SocialStatus_Id" integer NOT NULL,
    "Patient_CurrentDoctor_Id" integer NOT NULL,
    PRIMARY KEY ("Patient_Id"),
    CONSTRAINT "Patient_SocialStatus_Id" FOREIGN KEY ("Patient_SocialStatus_Id")
        REFERENCES public."SocialStatus" ("SocialStatus_id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "Patient_CurrentDoctor_Id" FOREIGN KEY ("Patient_CurrentDoctor_Id")
        REFERENCES public."Staff" ("Staff_id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public."Patient"
    OWNER to postgres;


CREATE TABLE public."DoctorAppointment"
(
    "DoctorAppointment_Id" serial NOT NULL,
    "DoctorAppointment_Procedure_Id" integer NOT NULL,
    "DoctorAppointment_Patient_id" integer NOT NULL,
    "DoctorAppointment_Doctor_Id" integer NOT NULL,
    "DoctorAppointment_Interval" interval NOT NULL,
    "DoctorAppointment_Start_Date" timestamp without time zone NOT NULL,
    "DoctorAppointment_End_Date" timestamp without time zone,
    PRIMARY KEY ("DoctorAppointment_Id"),
    CONSTRAINT "DoctorAppointment_Procedure_Id" FOREIGN KEY ("DoctorAppointment_Procedure_Id")
        REFERENCES public."Procedure" (procedure_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "DoctorAppointment_Patient_id" FOREIGN KEY ("DoctorAppointment_Patient_id")
        REFERENCES public."Patient" ("Patient_Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "DoctorAppointment_Doctor_Id" FOREIGN KEY ("DoctorAppointment_Doctor_Id")
        REFERENCES public."Staff" ("Staff_id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public."DoctorAppointment"
    OWNER to postgres;


CREATE TABLE public."PatientDeceases"
(
    "PatientDeceases_Id" serial NOT NULL,
    "PatientDeceases_Patient_Id" integer NOT NULL,
    "PatientDeceases_Decease_Id" integer NOT NULL,
    "PatientDeceases_IsCured" boolean NOT NULL DEFAULT False,
    "PatientDeceases_Start_Date" date NOT NULL,
    "PatientDeceases_End_Date" date,
    PRIMARY KEY ("PatientDeceases_Patient_Id"),
    CONSTRAINT "PatientDeceases_Patient_Id" FOREIGN KEY ("PatientDeceases_Patient_Id")
        REFERENCES public."Patient" ("Patient_Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "PatientDeceases_Decease_Id" FOREIGN KEY ("PatientDeceases_Decease_Id")
        REFERENCES public."Disease" ("Disease_Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public."PatientDeceases"
    OWNER to postgres;


CREATE TABLE public."HospitalStay"
(
    "HospitalStay_Id" serial NOT NULL,
    "HospitalStay_Patient_Id" integer NOT NULL,
    "HospitalStay_Start_Date" date NOT NULL,
    "HospitalStay_End_Date" date,
    "HospitalStay_Cost" integer NOT NULL,
    "HospitalStay_Department_Id" integer NOT NULL,
    PRIMARY KEY ("HospitalStay_Id"),
    CONSTRAINT "HospitalStay_Patient_Id" FOREIGN KEY ("HospitalStay_Patient_Id")
        REFERENCES public."Patient" ("Patient_Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT "HospitalStay_Department_Id" FOREIGN KEY ("HospitalStay_Department_Id")
        REFERENCES public."Department" ("Department_id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public."HospitalStay"
    OWNER to postgres;