using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MedNotes.Models
{
    public class NLP_TableModel
    {
        private string status;
        private string confidence;
        private string icd10;
        private string description;
        private string rej_reason;
        private string related_text;
        private string snomed_code;
        private string nlp_rejected_descr;
        private string nlp_accepted_descr;
        private string nlp_flag;

        private int valid_condition_id;
        private int rej_condition_id;
        private int related_start;
        private int related_end;



        public string Status
        {
            set { status = value; }
            get { return status; }
        }
        public string Confidence
        {
            set { confidence = value; }
            get { return confidence; }
        }
        public string ICD10
        {
            set { icd10 = value; }
            get { return icd10; }
        }
        public string Description
        {
            set { description = value; }
            get { return description; }
        }
        public int Valid_Condition_Id
        {
            set { valid_condition_id = value; }
            get { return valid_condition_id; }
        }
        public int Rej_Condition_Id
        {
            set { rej_condition_id = value; }
            get { return rej_condition_id; }
        }
        public string Rej_Reason
        {
            set { rej_reason = value; }
            get { return rej_reason; }
        }

        public string Nlp_Accepted_Descr
        {
            set { nlp_accepted_descr = value; }
            get { return nlp_accepted_descr; }
        }
        public string Nlp_Rejected_Descr
        {
            set { nlp_rejected_descr = value; }
            get { return nlp_rejected_descr; }
        }
        public string Nlp_Flag
        {
            set { nlp_flag = value; }
            get { return nlp_flag; }
        }
        public string Related_text
        {
            set { related_text = value; }
            get { return related_text; }
        }
        public string Snomed_code
        {
            set { snomed_code = value; }
            get { return snomed_code; }
        }
        public int Related_start
        {
            set { related_start = value; }
            get { return related_start; }
        }
        public int Related_end
        {
            set { related_end = value; }
            get { return related_end; }
        }


        public static readonly string Get_NLP_SQL =
        @"
          EXEC [Get_NLP_Data] @note_Id 
        ";


        public static readonly string Get_HTML_SQL =
        @"
          EXEC [Get_Highlight_Data] @note_Id
        ";


        public static NLP_TableModel NLP_FromDataReader(SqlDataReader dr)
        {
            return new NLP_TableModel
            {
                valid_condition_id = dr.GetInt32(0),
                rej_condition_id = dr.GetInt32(1),
                status = dr.GetString(2),
                confidence = dr.IsDBNull(3) ? "" : dr.GetString(3),
                icd10 = dr.GetString(4),
                description = dr.IsDBNull(5) ? "" : dr.GetString(5),
                rej_reason = dr.IsDBNull(6) ? "" : dr.GetString(6),
                related_text = dr.IsDBNull(7) ? "" : dr.GetString(7),
                related_start = dr.IsDBNull(8) ? -1 : dr.GetInt32(8),
                related_end = dr.IsDBNull(9) ? -1 : dr.GetInt32(9),
                snomed_code = dr.IsDBNull(10) ? "" : dr.GetString(10),
            };
        }

        public static NLP_TableModel HTML_NLP_FromDataReader(SqlDataReader dr)
        {
            return new NLP_TableModel
            {
                nlp_accepted_descr = dr.IsDBNull(0) ? "" : dr.GetString(0),
                nlp_rejected_descr = dr.IsDBNull(1) ? "" : dr.GetString(1),
                nlp_flag = dr.IsDBNull(2) ? "" : dr.GetString(2),
                related_text = dr.IsDBNull(3) ? "" : dr.GetString(3),
            };
        }

        public static string Store_NLPData_SQL(List<NLP_TableModel> nlpList, string note_id, string is_complete)
        {
            string full_sql="";
            string new_line = "\r\n";

            string valid_insert_sql = "";
            string reject_insert_sql = "";
            string update_sql = "";
            string delete_sql = "";

            int id_insert_num = 0;
            int id_reject_num = 0;
            bool isRejectInsertExists = false;
            bool isValidInsertExists = false;
            bool isInProgress = false;

            valid_insert_sql += @"
                 
                DECLARE @new_Valid_Id INT
                             SELECT @new_Valid_Id = ISNULL(MAX([note_condition__id]),0) FROM NOTE_CONDITION


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
                             SELECT @new_Reject_Id = ISNULL(MAX([rejected_condition__id]),0) FROM REJECTED_DIAGNOSIS


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


            foreach (var item in nlpList)
            {
                if (item != null)
                {
                    if (item.Rej_Condition_Id == -1)
                    {
                        if (item.Status == "Rejected")
                        {
                            id_reject_num++;

                            reject_insert_sql += (id_reject_num > 1 ? ", " : "") +
                                    @"
                                    (@new_Reject_Id + " + id_reject_num.ToString() +
                                    ", " + note_id +
                                    ", '" + item.ICD10 + "'" +
                                    ", (SELECT TOP 1 target_concept_id FROM Vocabulary..SOURCE_TO_CONCEPT_MAP WHERE source_code = '" + item.ICD10 + "')" +
                                    ", GETDATE()" +
                                    ", 1" +
                                    ", '" + item.Confidence + "'" +
                                    ", '" + item.Rej_Reason + "'" +
                                    ", '" + item.Related_text + "'" +
                                    ",  " + item.Related_start + 
                                    ",  " + item.Related_end + 
                                    ", '" + item.Snomed_code + "'" +
                                    ")";

                            delete_sql += @" 
                                             DELETE FROM NOTE_CONDITION WHERE note_condition__id = " + item.Valid_Condition_Id.ToString() +
                                                                                  " AND note_id = " + note_id;
                            isRejectInsertExists = true;
                            isInProgress = true;
                        }
                        else if (item.Status == "Accepted")
                        {
                            update_sql += @"
                                             UPDATE NOTE_CONDITION SET [condition_status_flag] = 'A'
                                                    WHERE note_condition__id = " + item.Valid_Condition_Id.ToString() +
                                                    " AND note_id = " + note_id;
                            isInProgress = true;
                        }
                    }
                    else if (item.Valid_Condition_Id == -1)
                    {
                        if (item.Status == "Rejected")
                        {
                            update_sql += @"
                                             UPDATE REJECTED_DIAGNOSIS SET [rejected_reason] =  '" + item.Rej_Reason + "'" +
                                                    " WHERE [rejected_condition__id] = " + item.Rej_Condition_Id.ToString() +
                                                    " AND [note_id] = " + note_id + "\r\n";
                            isInProgress = true;
                        }
                        else if (item.Status == "Accepted")
                        {
                            id_insert_num++;

                            valid_insert_sql += (id_insert_num > 1 ? ", " : "") +
                                    @"
                                    (@new_Valid_Id + " + id_insert_num.ToString() +
                                    ", " + note_id +
                                    ", '" + item.ICD10 + "'" +
                                    ", (SELECT TOP 1 target_concept_id FROM Vocabulary..SOURCE_TO_CONCEPT_MAP WHERE source_code = '" + item.ICD10.Trim() + "')" +
                                    ", GETDATE()" +
                                    ", 1" +
                                    ", 'A'" +
                                    ", '" + item.Confidence + "'" +
                                    ", NULL" +
                                    ", '" + item.Related_text + "'" +
                                    ",  " + item.Related_start +
                                    ",  " + item.Related_end +
                                    ", '" + item.Snomed_code + "'" +
                                    ")";

                            delete_sql += @" 
                                              DELETE FROM REJECTED_DIAGNOSIS WHERE [rejected_condition__id] = " + item.Rej_Condition_Id.ToString() +
                                                                                  " AND [note_id] = " + note_id;
                            isInProgress = true;
                            isValidInsertExists = true;
                        }
                    }
                }
            }


            if (is_complete == "1")
            {
                update_sql += @"
                                  UPDATE [NOTES] SET [note_status_type] = 'C'
                                        WHERE note_id = " + note_id + "\r\n";

            }
            else if (isInProgress)
            {
                update_sql += @"
                                  UPDATE [NOTES] SET [note_status_type] = 'I'
                                        WHERE note_id = " + note_id + "\r\n";
            }

            if (isRejectInsertExists)
            {
                full_sql += reject_insert_sql + new_line;
            }

            if (isValidInsertExists)
            {
                full_sql += valid_insert_sql + new_line;
            }

            full_sql += update_sql + new_line + delete_sql;

            return full_sql; 
        }
    }
}