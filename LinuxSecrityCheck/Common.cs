using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Threading.Tasks;

namespace LinuxSecrityCheck
{
    class Common
    {
        //查询目录结构，获取目录文件
        public class FileGet
        {            
            public List<string> GetIPsDirPath(string path)
            {
                List<string> dirPath = new List<string>();
                try
                {
                    string[] dirs = Directory.GetDirectories(path); //文件夹列表
                    foreach (string dir in dirs)
                    {
                        string ipdir = dir.Split('\\').Last().Trim();
                        if (IsIP(ipdir))
                        {
                            dirPath.Add(dir);
                        }
                    }
                }
                catch { }

                    return dirPath;
            }
        }

        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))");
        }

        public static List<string> FileSplit(string filePath,string key,char spChar)
        {
            //读取file中每一行
            List<string> txtRows = ReadTxtContent(filePath);
            //提取符合key的行,并按分隔符spChar分切到List<string>
            List<string> txtSplit = new List<string>();
            foreach(string txt in txtRows)
            {
                if (txt.Contains(key))
                    txtSplit.Add(txt);
            }
            return txtSplit;
        }

        public static List<string> ReadTxtContent(string Path)
        {
            StreamReader sr = new StreamReader(Path, Encoding.Default);
            List<string> txtRows = new List<string>();
            string content;
            while ((content = sr.ReadLine()) != null)
            {
                txtRows.Add(content);
            }
            return txtRows;
        }

        //读取excel单元格
        public static string ReadExcelCell(string excelFile,int row,int col)
        {
            DataTable dt = Excel2DataTable(excelFile,0);
            return dt.Rows[row][col].ToString();
        }

        public static DataTable Excel2DataTable(string filePath, int startRow)
        {
            DataTable dataTable = null;
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            try
            {
                using (fs = File.OpenRead(filePath))
                {
                    // 2007版本
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheetAt(0);//读取第一个sheet，当然也可以循环读取每个sheet
                        dataTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum;//总行数
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0);//第一行
                                int cellCount = firstRow.LastCellNum;//列数

                                //构建datatable的列
                                for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                {
                                    column = new DataColumn("column" + (i + 1));
                                    dataTable.Columns.Add(column);
                                }


                                //填充行
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = dataTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                return null;
            }
        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name=”timeStamp”></param>
        /// <returns></returns>
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        }

        public static void DataTableToExcel(DataTable dt, string excelFile)
        {

        }
    }
}
