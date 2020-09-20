using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;

namespace test
{
    public class csv
    {


        public static DataTable ReadCsv(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            DataTable dt = new DataTable();
            for (int i = 0; i < 5; i++)
            {
                dt.Columns.Add(i.ToString());
            }


            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(' ');

                DataRow dr = dt.NewRow();
                for (int i = 0; i < 5; i++)
                {
                    dr[i] = rows[i];
                }
                dt.Rows.Add(dr);

            }
            sr.Dispose();
            return dt;
        }
        public static DataTable Manisa_ReadCsv(string fileName)
        {



            StreamReader sr = new StreamReader(fileName);

            string[] headers = sr.ReadLine().Split('\t');

            DataTable dt = new DataTable();
            foreach (string header in headers)
            {
                dt.Columns.Add(header.Replace(@"""", ""));
            }
            //fazladan bir tab kolon ekleniyor onu siliyoruz
            dt.Columns.Remove("Column1");
            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split('\t');
                var list = new List<string>(rows);
                list.RemoveAt(1);
                rows = list.ToArray();
                DataRow dr = dt.NewRow();
                if (rows.Length == dt.Columns.Count)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dr[i] = rows[i].Replace(@"""", "");
                    }
                    dt.Rows.Add(dr);
                }

            }
            sr.Dispose();
            return dt;






        }

        public static DataTable Csv(string file, char seperator, int numberof_skip_line, int columncount)
        {
            StreamReader sr = new StreamReader(file);
            DataTable dt = new DataTable();

            for (int i = 0; i < numberof_skip_line; i++)
            {
                sr.ReadLine();
            }
            if (columncount == 0)
            {
                string[] headers = sr.ReadLine().Replace(@"""", "").Split(seperator);
                foreach (string header in headers)

                {
                    dt.Columns.Add(header);
                }
            }
            else
            {
                for (int i = 0; i < columncount; i++)
                {

                    dt.Columns.Add(i.ToString());
                }
            }
            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(seperator);
                DataRow dr = dt.NewRow();
                if (dt.Columns.Count == rows.Length)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dr[i] = rows[i].Replace(@"""", "");
                    }
                    dt.Rows.Add(dr);
                }

            }
            sr.Close();

            return dt;


        }


    }
}