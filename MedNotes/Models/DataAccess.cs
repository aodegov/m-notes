using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MedNotes.Models
{
    public class DataAccess
    {
        private static SqlConnection MedNotesConnection()
        {
            SqlConnection result = new SqlConnection(ConfigurationManager.AppSettings["NotesConnStr"]);
            result.Open();
            return result;
        }

        public static List<SummaryModel> GetSummaries()
        {
            List<SummaryModel> result = new List<SummaryModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = SummaryModel.SummarySQL;
                cmd.CommandType = CommandType.Text;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(SummaryModel.GetSummaryFromDataReader(dr));
            }

            return result;
        }

        public static List<ICD10Model> GetICD10(int note_id, string query_val)
        {
            List<ICD10Model> result = new List<ICD10Model>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = ICD10Model.Get_ICD10_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@note_id ", System.Data.SqlDbType.Int) { Value = note_id });
                cmd.Parameters.Add(new SqlParameter("@query_val ", System.Data.SqlDbType.VarChar) { Value = query_val });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(ICD10Model.ICD10FromDataReader(dr));
            }

            return result;
        }

        public static void StoreDataTransaction(string NLPData, string UserData, string note_id, string is_complete)
        {
            List<Unidentified_TableModel> usrList = JsonConvert.DeserializeObject<List<Unidentified_TableModel>>(UserData);
            List<NLP_TableModel> nlpList = JsonConvert.DeserializeObject<List<NLP_TableModel>>(NLPData);
            string sqlUnident = Unidentified_TableModel.Store_Unidentified_Data_SQL(usrList, note_id);

            using (SqlConnection conn = MedNotesConnection())
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    using (SqlTransaction transaction = conn.BeginTransaction("Store_Data_Transaction"))
                    {
                        try
                        {
                            cmd.Transaction = transaction;
                            // Save NLP SQL
                            cmd.CommandText = NLP_TableModel.Store_NLPData_SQL(nlpList, note_id, is_complete);
                            cmd.ExecuteNonQuery();
                            // Save User SQL
                            cmd.CommandText = sqlUnident;
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex_tran)
                        {
                            LogHelper.SaveLogInfo(string.Format("Exception: Store Data Transaction is failed: \r\n  {0}", ex_tran), ex_tran.StackTrace);
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception ex_roll)
                            {
                                LogHelper.SaveLogInfo(string.Format("Exception: Rollback Exception: \r\n  {0}", ex_roll), ex_roll.StackTrace);
                            }
                        }

                    }
                }
            }

        }

        public static List<NLP_TableModel> Get_NLP_Records(int note_id)
        {
            List<NLP_TableModel> result = new List<NLP_TableModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = NLP_TableModel.Get_NLP_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@note_id  ", System.Data.SqlDbType.Int) { Value = note_id });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(NLP_TableModel.NLP_FromDataReader(dr));
            }

            return result;
        }

        public static List<Unidentified_TableModel> Get_Unidentified_Records(int note_id)
        {
            List<Unidentified_TableModel> result = new List<Unidentified_TableModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = Unidentified_TableModel.Get_Unidentified_Data_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@note_id", System.Data.SqlDbType.Int) { Value = note_id });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(Unidentified_TableModel.UnidentifiedDataFromDataReader(dr));
            }

            return result;
        }

        public static string GetHtmlFileName(int note_id)
        {
            FileModel result = null;

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = FileModel.Get_HTML_File_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@note_id ", System.Data.SqlDbType.Int) { Value = note_id });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result = FileModel.FileNameDataReader(dr);
            }

            return result.File_Name;
        }

        public static List<NLP_TableModel> Get_NLP_HTML_Highlight(int note_id)
        {
            List<NLP_TableModel> result = new List<NLP_TableModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = NLP_TableModel.Get_HTML_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@note_id ", System.Data.SqlDbType.Int) { Value = note_id });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(NLP_TableModel.HTML_NLP_FromDataReader(dr));
            }

            return result;
        }

        public static List<PersonsAndClientsModel> GetClients()
        {
            List<PersonsAndClientsModel> result = new List<PersonsAndClientsModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = PersonsAndClientsModel.Get_Clients_SQL;
                cmd.CommandType = CommandType.Text;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(PersonsAndClientsModel.ClientsDataFromDataReader(dr));
            }

            return result;
        }

        public static List<PersonsAndClientsModel> GetPerson(int note_id)
        {
            List<PersonsAndClientsModel> result = new List<PersonsAndClientsModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = PersonsAndClientsModel.Get_Person_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@note_id", System.Data.SqlDbType.Int) { Value = note_id });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(PersonsAndClientsModel.PersonDataFromDataReader(dr));
            }

            return result;
        }


        public static List<FileModel> GetPatientFiles(int patient_id)
        {
            List<FileModel> result = new List<FileModel>();

            using (SqlConnection conn = MedNotesConnection())
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = FileModel.Get_Patient_Documents_SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@patient_id ", System.Data.SqlDbType.Int) { Value = patient_id });
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    result.Add(FileModel.PatientFiles_From_DataReader(dr));
            }

            return result;
        }
    }
}