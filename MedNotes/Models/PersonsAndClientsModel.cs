using System.Data.SqlClient;

namespace MedNotes.Models
{
    public class PersonsAndClientsModel
    {
        private int client_id;
        private string client_name;
        private int person_id;
        private string provider_name;
        private string person_name;
        private string mrn;
        private string dob;
        private string age;
        private string gender;
        private string race;
        private string note_date;
        //Clients
        public int Client_Id { get { return client_id; } }
        public string Client_Name { get { return client_name; } }
        //Person
        public int Person_Id { get { return person_id; } }
        public string Provider_Name { get { return provider_name; } }
        public string Person_Name { get { return person_name; } }
        public string MRN { get { return mrn; } }
        public string DOB { get { return dob; } }
        public string Age { get { return age; } }
        public string Gender { get { return gender; } }
        public string Race { get { return race; } }
        public string NoteDate { get { return note_date; } }

        public static readonly string Get_Clients_SQL =
            @"
                 EXEC [Get_Clients_Data]
            ";

        public static PersonsAndClientsModel ClientsDataFromDataReader(SqlDataReader dr)
        {
            return new PersonsAndClientsModel
            {
                client_id = dr.GetInt32(0),
                client_name = dr.GetString(1)
            };
        }

        public static readonly string Get_Person_SQL =
          @"
                 EXEC [Get_Person_Data] @note_id
            ";

        public static PersonsAndClientsModel PersonDataFromDataReader(SqlDataReader dr)
        {
            return new PersonsAndClientsModel
            {
                person_id = dr.GetInt32(0),
                provider_name = dr.GetString(1),
                mrn = dr.GetString(2),
                person_name = dr.GetString(3),
                dob = dr.GetString(4),
                age = dr.GetString(5),
                gender = dr.GetString(6),
                race = dr.GetString(7),
                note_date = dr.GetString(8),
            };
        }

    }
}