using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MedNotes.Models
{
    public class SummaryModel
    {
        private string status;
        private string mrn;
        private string person_name;
        private string dob;
        private string note_type;
        private string provider_name;
        private int dx_found;
        private int dx_accepted;
        private int dx_rejected;
        private int dx_added;
        private string inner_data;
        private string note_date;
        private int note_id;
        private int client_id;
        private string client_name;

        public string Status { get { return status; } }
        public string MRN { get { return mrn; } }
        public string PersonName { get { return person_name; } }
        public string DOB { get { return dob; } }
        public string NoteType { get { return note_type; } }
        public string Provider { get { return provider_name; } }
        public int DxFound { get { return dx_found; } }
        public int DxAccepted { get { return dx_accepted; } }
        public int DxRejected { get { return dx_rejected; } }
        public int DxAdded { get { return dx_added; } }
        public string InnerData { get { return inner_data; } }
        public string NoteDate { get { return note_date; } }
        public int Note_id { get { return note_id; } }
        public int Client_Id { get { return client_id; } }
       // public string Client_Name { get { return client_name; } }

        public static readonly string SummarySQL =
            @"
                 EXEC SUMMARY
            ";

        public static SummaryModel GetSummaryFromDataReader(SqlDataReader dr)
        {
            return new SummaryModel()
            {
                status = dr.GetString(0),
                mrn = dr.GetString(1),
                person_name = dr.GetString(2),
                dob = dr.GetString(3),
                note_type = dr.GetString(4),
                provider_name = dr.GetString(5),
                dx_found = dr.GetInt32(6),
                dx_accepted = dr.GetInt32(7),
                dx_rejected = dr.GetInt32(8),
                dx_added = dr.GetInt32(9),
                inner_data = dr.GetString(10),
                note_date = dr.GetDateTime(11).ToString("MM/dd/yyyy", CultureInfo.CreateSpecificCulture("en-US")),
                note_id = dr.GetInt32(12),
                client_id = dr.GetInt32(13),
           //     client_name = dr.GetString(14),
            };
        }
    }
}

