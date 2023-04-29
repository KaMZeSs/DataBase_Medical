using System;
using System.Xml.Serialization;

namespace Randomize
{
    public class PartOfName
    {
        public string text;
        public string gender;

        override
        public string ToString()
        {
            return text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstNames"></param>
        /// <param name="midNames"></param>
        /// <param name="lastNames"></param>
        /// <returns>FIO (F, I, O)</returns>
        public static (string, string, string) GenerateFIO(PartOfName[] firstNames,
            PartOfName[] midNames, PartOfName[] lastNames)
        {
            PartOfName firstName, midName, lastName = new PartOfName();

            do
            {
                firstName = firstNames[new Random().Next(0, firstNames.Length - 1)];
            } while (firstName.gender == null);

            //Get midname according to gender of firstname
            if (firstName.gender == "u")
            {
                do
                {
                    midName = midNames[new Random().Next(0, midNames.Length - 1)];
                } while (midName.gender == null);
            }
            else
            {
                if (firstName.gender == "m")
                {
                    PartOfName[] temp = Array.FindAll(midNames, x => x.gender == "m" | x.gender == "u");
                    do
                    {
                        midName = temp[new Random().Next(0, temp.Length - 1)];
                    } while (midName.gender == null);
                }
                else
                {
                    PartOfName[] temp = Array.FindAll(midNames, x => x.gender == "f" | x.gender == "u");
                    do
                    {
                        midName = temp[new Random().Next(0, temp.Length - 1)];
                    } while (midName.gender == null);
                }
            }

            //Get lastname according to gender of firstname and midname
            if (firstName.gender == "u" & midName.gender == "u")
            {
                do
                {
                    lastName = lastNames[new Random().Next(0, lastNames.Length - 1)];
                } while (lastName.gender == null);
            }
            else if ((firstName.gender == "m" & midName.gender == "u") |
                (firstName.gender == "u" & midName.gender == "m") |
                (firstName.gender == "m" & midName.gender == "m"))
            {
                PartOfName[] temp = Array.FindAll(lastNames, x => x.gender == "m" | x.gender == "u");

                do
                {
                    lastName = temp[new Random().Next(0, temp.Length - 1)];
                } while (lastName.gender == null);
            }
            else if ((firstName.gender == "f" & midName.gender == "u") |
                (firstName.gender == "u" & midName.gender == "f") |
                (firstName.gender == "f" & midName.gender == "f"))
            {
                PartOfName[] temp = Array.FindAll(lastNames, x => x.gender == "f" | x.gender == "u");
                do
                {
                    lastName = temp[new Random().Next(0, temp.Length - 1)];
                } while (lastName.gender == null);
            }

            return (lastName.text, firstName.text, midName.text);
        }

        public static PartOfName[] DeserializePartsOfName(String path)
        {
            var serializer = new XmlSerializer(typeof(PartOfName[]));

            using (var reader = new StreamReader(path))
            {
                return (PartOfName[])serializer.Deserialize(reader);
            }
        }
    }
}