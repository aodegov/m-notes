using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cTakesXMLParser
{
    class SQLService
    {
        static string conn_string = ConfigurationManager.AppSettings["NotesConnStr"];

        public static List<dynamic> ExecuteCompareSQL(List<dynamic> snomed_codes_list)
        {

            List<dynamic> compare_lst;

            using (var con = new SqlConnection(conn_string))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("exec [dbo].[Get_ICD10_By_SNOMED] @SNOMED_LST", con))
                {
                    using (var table = new DataTable())
                    {
                        table.Columns.Add("Code", typeof(string));

                        foreach (var item in snomed_codes_list)
                        {
                            table.Rows.Add(item.snomed_code);
                        }


                        var pList = new SqlParameter("@SNOMED_LST", SqlDbType.Structured);
                        pList.TypeName = "dbo.SnomedCodesList";
                        pList.Value = table;

                        cmd.Parameters.Add(pList);

                        compare_lst = new List<dynamic>();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                compare_lst.Add(new
                                {
                                    snomed_code = dr["snomed_code"],
                                    snomed_description = dr["snomed_description"],
                                    icd10_code = dr["icd10_code"],
                                    icd10_description = dr["icd10_description"]
                                });
                                Console.WriteLine("SNOMED: " + dr["snomed_code"] + " " + dr["snomed_description"] + "  |  ICD10: " + dr["icd10_code"] + " " + dr["icd10_description"]);
                            }
                        }


                    }
                }
            }

            if (compare_lst.Count > 0)
            {
                var joinedList = (from smed in snomed_codes_list
                                  join comp in compare_lst on smed.snomed_code equals comp.snomed_code
                                  select new
                                  {
                                      smed.snomed_code,
                                      smed.text_descr,
                                      smed.pos_start,
                                      smed.pos_end,
                                      smed.confidence,
                                      smed.status,
                                      comp.icd10_code,
                                      comp.icd10_description

                                  }).Distinct().ToList<dynamic>();

                return joinedList;
            }
            else
            {
                return null;
            }

        }

        public static void CreateNoteConditions(List<dynamic> lst_result, string html_file_name, int note_id)
        {

            using (SqlConnection conn = new SqlConnection(conn_string))
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    using (SqlTransaction transaction = conn.BeginTransaction("CreateNewNote_Transaction"))
                    {
                        try
                        {
                            cmd.Transaction = transaction;

                            // create new note
                            //cmd.CommandText = "exec Create_Note @note_type, @person_id, @provider_id, @input_file";

                            //cmd.Parameters.Add(new SqlParameter("@note_type ", System.Data.SqlDbType.VarChar) { Value = "Summary" });
                            //cmd.Parameters.Add(new SqlParameter("@person_id ", System.Data.SqlDbType.Int) { Value = 10 });
                            //cmd.Parameters.Add(new SqlParameter("@provider_id ", System.Data.SqlDbType.Int) { Value = 1 });
                            //cmd.Parameters.Add(new SqlParameter("@file_name ", System.Data.SqlDbType.VarChar) { Value = file_name });

                            //cmd.ExecuteNonQuery();


                            // save notes conditions
                            cmd.CommandText = Store_NLPData_SQL(lst_result, note_id, html_file_name);
                            cmd.ExecuteNonQuery();

                            transaction.Commit();
                        }
                        catch (Exception ex_tran)
                        {

                            LogHelper.SaveLogInfo(string.Format("Exception: Create New Note Transaction is failed: \r\n  {0}", ex_tran.Message), ex_tran.StackTrace);

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


        public static string Store_NLPData_SQL(List<dynamic> lst_result, int note_id, string html_file_name)
        {
            string valid_insert_sql = "";
            string reject_insert_sql = "";
            string full_sql = "";
            string new_line = "\r\n";

            int id_valid_num = 0;
            int id_reject_num = 0;

            bool isRejectInsertExists = false;
            bool isValidInsertExists = false;

            valid_insert_sql += @"
                 
                DECLARE @new_valid_Id INT
                             SELECT @new_valid_Id = ISNULL(MAX([note_condition__id]), 0) + 1 FROM NOTE_CONDITION


                INSERT INTO NOTE_CONDITION(
                                           [note_condition__id]
                                          ,[note_id]
                                          ,[condition_source_value]
                                          ,[condition_concept_id]
                                          ,[condition_entry_date]
                                          ,[user_id]
                                          ,[condition_status_flag]
                                          ,[confidence]
                                          ,[how_identified]
                                          ,[related_text]
                                          ,[related_start]
                                          ,[related_end]
                                          ,[snomed_code]
                                           ) 
                                    VALUES ";

            reject_insert_sql += @"
                 
                DECLARE @new_Reject_Id INT
                             SELECT @new_Reject_Id = ISNULL(MAX([rejected_condition__id]), 0) + 1 FROM REJECTED_DIAGNOSIS


                INSERT INTO REJECTED_DIAGNOSIS (
                                                   [rejected_condition__id]
                                                  ,[note_id]
                                                  ,[condition_source_value]
                                                  ,[condition_concept_id]
                                                  ,[condition_entry_date]
                                                  ,[user_id]
                                                  ,[confidence]
                                                  ,[rejected_reason]
                                                  ,[related_text]
                                                  ,[related_start]
                                                  ,[related_end]
                                                  ,[snomed_code]
                                               ) 
                                        VALUES ";


            foreach (var item in lst_result)
            {
                if (item.status != null && item.status == "Rejected")
                {
                    reject_insert_sql += (id_reject_num > 0 ? ", " : " ") +
                      @"
                        (@new_Reject_Id + " + id_reject_num.ToString() +
                      //", (SELECT MAX([note_id]) FROM NOTES) " +
                      ",  " + note_id +
                      ", '" + item.icd10_code + "'" +
                      ", (SELECT TOP 1 target_concept_id FROM Vocabulary..SOURCE_TO_CONCEPT_MAP WHERE source_code = '" + item.icd10_code + "')" +
                      ",  GETDATE()" +
                      ",  1 " +
                      ",    " + (item.confidence == null || item.confidence == "-" ? "''" : "'" + item.confidence.Trim() + "'") +
                      ",  NULL " +
                      ",  '" + item.text_descr.Trim() + "'" +
                      ",   " + item.pos_start +
                      ",   " + item.pos_end +
                      ",   " + item.snomed_code +
                      ")";

                    id_reject_num++;
                    isRejectInsertExists = true;
                }
                else
                {
                    valid_insert_sql += (id_valid_num > 0 ? ", " : " ") +
                       @"
                        (@new_valid_Id + " + id_valid_num.ToString() +
                       //", (SELECT MAX([note_id]) FROM NOTES) " +
                       ",  " + note_id +
                       ", '" + item.icd10_code + "'" +
                       ", (SELECT TOP 1 target_concept_id FROM Vocabulary..SOURCE_TO_CONCEPT_MAP WHERE source_code = '" + item.icd10_code + "')" +
                       ",  GETDATE()" +
                       ",  1 " +
                       ", 'N'" +
                       ",    " + (item.confidence == null || item.confidence == "-" ? "''" : "'" + item.confidence.Trim() + "'") +
                       ",  NULL " +
                       ",  '" + item.text_descr.Trim() + "'" +
                       ",   " + item.pos_start +
                       ",   " + item.pos_end +
                       ",   " + item.snomed_code +
                       ")";

                    id_valid_num++;
                    isValidInsertExists = true;
                }
            }

            if (isRejectInsertExists)
            {
                full_sql += reject_insert_sql + new_line;
            }

            if (isValidInsertExists)
            {
                full_sql += valid_insert_sql + new_line;
            }

            if (isValidInsertExists || isRejectInsertExists)
            {
                full_sql += @" 
                              UPDATE [MedNotes].[dbo].[NOTES] SET [process_date] = GETDATE(), [output_html_file] = '" + html_file_name + "' WHERE [note_id] = " + note_id;
            }


            return full_sql;
        }

        public static Dictionary<string, int> GetInputFileNames()
        {

            Dictionary<string, int> dict_names = new Dictionary<string, int>();

            using (var con = new SqlConnection(conn_string))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("exec [dbo].[Get_Input_File_Names] ", con))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            dict_names.Add(dr.IsDBNull(1) ? "" : dr.GetString(1), dr.IsDBNull(0) ? -1 : dr.GetInt32(0));
                        }
                    }

                }
            }
            return dict_names;
        }


    }
}
