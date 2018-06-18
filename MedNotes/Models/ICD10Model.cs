using System.Data.SqlClient;

namespace MedNotes.Models
{
    public class ICD10Model
    {
        private string icd10_code;
        private string icd10_description;

        public string ICD10_code { get { return icd10_code; } }
        public string ICD10_description { get { return icd10_description; } }

        public static readonly string Get_ICD10_SQL =
            @"
                 EXEC [Get_ICD10_List] @note_id, @query_val
            ";
        public static ICD10Model ICD10FromDataReader(SqlDataReader dr)
        {
            return new ICD10Model
            {
                icd10_code = dr.GetString(0),
                icd10_description = dr.GetString(1)
            };
        }

    }
}