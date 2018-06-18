using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MedNotes.Models
{
    public class Unidentified_TableModel
    {
        private string icd10;
        private string description;
        private string how_ident;
        private string related_text;
        private int related_start;
        private int related_end;

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
        public string How_Ident
        {
            set { how_ident = value; }
            get { return how_ident; }
        }
        public string Related_text
        {
            set { related_text = value; }
            get { return related_text; }
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

        public static string Store_Unidentified_Data_SQL(List<Unidentified_TableModel> usrList, string note_id)
        {


            string sql_delete = @"

                DELETE FROM NOTE_CONDITION 
                                      WHERE [note_id] =  " + note_id + @" 
                                        AND [condition_status_flag] = 'U';
                ";


            string sql_insert = @"

                DECLARE @NewId INT 
                             SELECT @NewId = MAX([note_condition__id]) FROM NOTE_CONDITION;
                             
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

            int idNum = 0;
            bool isInsertExists = false;

            foreach (Unidentified_TableModel item in usrList)
            {
                if (item != null)
                {
                    idNum++;
                    sql_insert += (idNum > 1 ? ", " : "") +
                            @"
                        (@NewId + " + idNum.ToString() +
                            ", " + note_id +
                            ", '" + item.ICD10 + "'" +
                            ", (SELECT TOP 1 target_concept_id FROM Vocabulary..SOURCE_TO_CONCEPT_MAP WHERE source_code = '" + item.ICD10 + "')" +
                            ", GETDATE()" +
                            ", 1" +
                            ", 'U'" +
                            ", 'High'" +
                            ", '" + item.How_Ident + "'" +
                            ", '" + item.Related_text + "'" +
                            ",  " + item.Related_start + 
                            ",  " + item.Related_end +
                            ",  (SELECT TOP 1 [snomed_code] FROM [MedNotes].[dbo].[SNOMED_TO_ICD10] WHERE [icd10_code] = '" + item.ICD10 + "' AND [icd10_code] IS NOT NULL )" +
                            ")";

                    isInsertExists = true;
                }
            }


            return isInsertExists ? sql_delete + " " + sql_insert : sql_delete;
        }


        public static readonly string Get_Unidentified_Data_SQL =
        @"
            EXEC [Get_Unidentified_Data] @note_id 
        ";

        public static Unidentified_TableModel UnidentifiedDataFromDataReader(SqlDataReader dr)
        {
            return new Unidentified_TableModel
            {
                icd10 = dr.GetString(0),
                how_ident = dr.IsDBNull(1) ? "" : dr.GetString(1),
                description = dr.IsDBNull(2) ? "" : dr.GetString(2),
                related_text = dr.IsDBNull(3) ? "" : dr.GetString(3),
                related_start = dr.IsDBNull(4) ? -1 : dr.GetInt32(4),
                related_end = dr.IsDBNull(5) ? -1 : dr.GetInt32(5),
            };
        }
    }
}