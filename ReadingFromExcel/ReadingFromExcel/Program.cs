﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;
using OfficeOpenXml;
using System.Runtime.Serialization.Formatters.Binary;
using LinqToExcel;
using System.Net;
using System.Diagnostics;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace ReadingFromExcel
{
    class Program
    {
        private static int MimeSampleSize = 256;

        private static string DefaultMimeType = "application/octet-stream";

        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private static extern uint FindMimeFromData(
            uint pBc,
            [MarshalAs(UnmanagedType.LPStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] bytes,
            uint cbSize,
            [MarshalAs(UnmanagedType.LPStr)] string pwzMimeProposed,
            uint dwMimeFlags,
            out uint mimetype,
            uint dwReserverd
        );
        private static string GetMimeFromBytes(byte[] data)
        {
            try
            {
                uint mimeType;
                FindMimeFromData(0, null, data, (uint)MimeSampleSize, null, 0, out mimeType, 0);

                var mimePointer = new IntPtr(mimeType);
                var mime = Marshal.PtrToStringUni(mimePointer);
                Marshal.FreeCoTaskMem(mimePointer);

                return mime ?? DefaultMimeType;
            }
            catch
            {
                return DefaultMimeType;
            }
        }
        private static bool Exist(List<string> list, string value, out int index)
        {
            bool t = false;
            index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ToLower().Contains(value))
                {
                    t = true;
                    index = i;
                    break;
                }
            }
            return t;
        }

        private static bool Checking(List<string> allcolumns, ref List<string> columns)
        {
            int index = -1, index1 = -1, index2 = -1;

            if (allcolumns.Count < 6
                || (!Exist(allcolumns, "fullname", out index)
                && !Exist(allcolumns, "full name", out index))
                || (!Exist(allcolumns, "company", out index)
                && !Exist(allcolumns, "company name", out index)
                && !Exist(allcolumns, "companyname", out index))
                || !Exist(allcolumns, "position", out index)
                || !Exist(allcolumns, "country", out index)
                || (!Exist(allcolumns, "email", out index)
                && !Exist(allcolumns, "mail", out index))
                || (!Exist(allcolumns, "data inserted", out index)
                && !Exist(allcolumns, "datainserted", out index)))
            {
                return false;
            }
            if (Exist(allcolumns, "fullname", out index)
                || Exist(allcolumns, "full name", out index1))
            {
                if (index1 == -1)
                    columns.Add(allcolumns[index]);
                else
                    columns.Add(allcolumns[index1]);
            }
            if (Exist(allcolumns, "company", out index)
                || Exist(allcolumns, "companyname", out index1)
                || Exist(allcolumns, "company name", out index2))
            {
                if (index1 == -1 && index2 == -1)
                    columns.Add(allcolumns[index]);
                else if (index1 == -1)
                    columns.Add(allcolumns[index2]);
                else
                    columns.Add(allcolumns[index1]);
            }
            if (Exist(allcolumns, "position", out index))
                columns.Add(allcolumns[index]);
            if (Exist(allcolumns, "country", out index))
                columns.Add(allcolumns[index]);
            if (Exist(allcolumns, "email", out index)
                || Exist(allcolumns, "mail", out index1))
            {
                if (index1 == -1)
                    columns.Add(allcolumns[index]);
                else
                    columns.Add(allcolumns[index1]);
            }
            if (Exist(allcolumns, "data inserted", out index)
                || Exist(allcolumns, "datainserted", out index1))
            {
                if (index1 == -1)
                    columns.Add(allcolumns[index]);
                else
                    columns.Add(allcolumns[index1]);
            }

            return true;

        }
        static List<string> columns = new List<string>();
        //private static List<Contact> ReadFromExcel(byte[] bytes)
        //{
        //    List<Contact> contactslist = new List<Contact>();
        //    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "file.xlsx";
        //    try
        //    {
        //        File.WriteAllBytes(path, bytes);
        //        ExcelQueryFactory excel = new ExcelQueryFactory(path);
        //        var sheets = excel.GetWorksheetNames();
        //        var contacts = (from c in excel.Worksheet<Row>(sheets.First())
        //                        select c).ToList();
        //        var worksheetcolumns = excel.GetColumnNames(sheets.First()).ToList();

        //        if (!Checking(worksheetcolumns, ref columns))
        //            return null;

        //        foreach (var m in contacts)
        //        {
        //            Contact c = new Contact();
        //            c.FullName = m[columns[0]];
        //            c.CompanyName = m[columns[1]];
        //            c.Country = m[columns[2]];
        //            c.Position = m[columns[3]];
        //            c.Email = m[columns[4]];
        //            c.DateInserted = Convert.ToDateTime(m[columns[5]]);
        //            c.GuID = Guid.NewGuid();
        //            contactslist.Add(c);
        //        }
        //        File.Delete(path);
        //    }
        //    catch
        //    {
        //        File.Delete(path);
        //    }
        //    return contactslist;
        //}

        //private static List<Contact> ReadFromCsv(byte[] bytes)
        //{
        //    List<Contact> contactslist = new List<Contact>();
        //    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "file.csv";
        //    try
        //    {
        //        int index = -1, index1 = -1, index2 = -1;
        //        File.WriteAllBytes(path, bytes);
        //        string[] lines = File.ReadAllLines(path);
        //        string[] allcolumns = lines[0].Split(',');

        //        if (!Checking(allcolumns.ToList(), ref columns))
        //            return null;
        //        Dictionary<string, int> d = new Dictionary<string, int>();
        //        if (Exist(allcolumns.ToList(), "fullname", out index)
        //        || Exist(allcolumns.ToList(), "full name", out index1))
        //        {
        //            if (index1 == -1)
        //                d.Add("FullName", index);
        //            else
        //                d.Add("FullName", index1);
        //        }
        //        if (Exist(allcolumns.ToList(), "company", out index)
        //            || Exist(allcolumns.ToList(), "companyname", out index1)
        //            || Exist(allcolumns.ToList(), "company name", out index2))
        //        {
        //            if (index1 == -1 && index2 == -1)
        //                d.Add("Company", index);
        //            else if (index1 == -1)
        //                d.Add("Company", index2);
        //            else
        //                d.Add("Company", index1);
        //        }
        //        if (Exist(allcolumns.ToList(), "position", out index))
        //            d.Add("Position", index);
        //        if (Exist(allcolumns.ToList(), "country", out index))
        //            d.Add("Country", index);
        //        if (Exist(allcolumns.ToList(), "email", out index)
        //            || Exist(allcolumns.ToList(), "mail", out index1))
        //        {
        //            if (index1 == -1)
        //                d.Add("Email", index);
        //            else
        //                d.Add("Email", index1);
        //        }
        //        if (Exist(allcolumns.ToList(), "data inserted", out index)
        //            || Exist(allcolumns.ToList(), "datainserted", out index1))
        //        {
        //            if (index1 == -1)
        //                d.Add("DataInserted", index);
        //            else
        //                d.Add("DataInserted", index1);
        //        }

        //        for (int i = 1; i < lines.Length; i++)
        //        {
        //            Contact contact = new Contact();
        //            string[] values = lines[i].Split(',');
        //            for (int j = 0; j < values.Length; j++)
        //            {
        //                switch (j)
        //                {
        //                    case 0:
        //                        contact.FullName = values[d["FullName"]];
        //                        break;
        //                    case 1:
        //                        contact.CompanyName = values[d["Company"]];
        //                        break;
        //                    case 2:
        //                        contact.Position = values[d["Position"]];
        //                        break;
        //                    case 3:
        //                        contact.Country = values[d["Country"]];
        //                        break;
        //                    case 4:
        //                        contact.Email = values[d["Email"]];
        //                        break;
        //                    case 5:
        //                        contact.DateInserted = Convert.ToDateTime(values[d["DataInserted"]]);
        //                        break;
        //                }
        //            }
        //            contact.GuID = new Guid();
        //            contactslist.Add(contact);
        //        }
        //        File.Delete(path);
        //    }
        //    catch (Exception ex)
        //    {
        //        File.Delete(path);
        //        Console.WriteLine(ex.Message);
        //    }
        //    return contactslist;
        //}
        private static List<Contact> ReadFromExcel(byte[] bytes, string path)
        {
            var contactslist = new List<Contact>();
            try
            {
                File.WriteAllBytes(path, bytes);
                contactslist = ReadExcelFileXml(path);
                File.Delete(path);
            }
            catch
            {
                File.Delete(path);
                throw;
            }
            finally
            {
                File.Delete(path);
            }
            return contactslist;
        }

        static List<Contact> ReadExcelFileXml(string filename)
        {
            string[] strProperties = new string[5];
            List<Contact> list = new List<Contact>();
            Contact contact;
            int j = 0;
            using (SpreadsheetDocument myDoc = SpreadsheetDocument.Open(filename, true))
            {
                WorkbookPart workbookPart = myDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = myDoc.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>().Elements<Sheet>();
                string relationshipId = sheets?.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)myDoc.WorkbookPart.GetPartById(relationshipId);
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                int i = 1;
                string value;
                foreach (DocumentFormat.OpenXml.Spreadsheet.Row r in sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>())
                {
                    if (i != 1)
                    {
                        foreach (DocumentFormat.OpenXml.Spreadsheet.Cell c in r.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
                        {
                            if (c == null) continue;
                            value = c.InnerText;
                            if (c.DataType != null)
                            {
                                var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                                if (stringTable != null)
                                {
                                    value = stringTable.SharedStringTable.
                                        ElementAt(int.Parse(value)).InnerText;
                                }
                            }
                            strProperties[j] = value;
                            j = j + 1;
                        }
                    }
                    j = 0;
                    i = i + 1;
                    if (strProperties.Any(p => p == null)) continue; // checks all nulls
                    contact = new Contact();
                    contact.FullName = strProperties[0];
                    contact.CompanyName = strProperties[1];
                    contact.Position = strProperties[2];
                    contact.Country = strProperties[3];
                    contact.Email = strProperties[4];
                    list.Add(contact);
                }
                return list;
            }
        }

        public static List<Contact> GetContactsFromBytes(byte[] bytes, string path)
        {
            List<Contact> list = new List<Contact>();
            string p = GetMimeFromBytes(bytes);
            switch (p)
            {
                case "text/csv":
                case "text/plain":
                case "application/octet-stream":
                    bytes = CSVToExcel(bytes, path);
                    list = ReadFromExcel(bytes, path + "\\file.xlsx");
                    break;
                case "application/vnd.ms-excel":
                case "application/x-zip-compressed":
                    list = ReadFromExcel(bytes, path);
                    break;
                default:
                    list = null;
                    break;
            }
            return list;
        }

        public static byte[] CSVToExcel(byte[] csvbytes, string path)
        {
            string csvFileName = path + "\\csvfile.csv";
            string excelFileName = path + "\\excelfile.xlsx";
            byte[] bytes;
            try
            {
                File.WriteAllBytes(csvFileName, csvbytes);
                string worksheetsName = "sheet1";
                bool firstRowIsHeader = true;

                var format = new ExcelTextFormat();
                format.Delimiter = ',';
                format.EOL = "\r";

                using (ExcelPackage package = new ExcelPackage(new FileInfo(excelFileName)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetsName);
                    worksheet.Cells["A1"].LoadFromText(new FileInfo(csvFileName), format, OfficeOpenXml.Table.TableStyles.Medium27, firstRowIsHeader);
                    package.Save();
                    bytes = File.ReadAllBytes(excelFileName);
                }
                File.Delete(csvFileName);
                File.Delete(excelFileName);
            }
            catch
            {
                bytes = null;
                File.Delete(csvFileName);
                File.Delete(excelFileName);
            }
            return bytes;
        }
        static void Main(string[] args)
        {
            string path = @"C:\Users\Dell\Desktop";
            byte[] bytes = File.ReadAllBytes(@"C:\Users\Dell\Desktop\contacts.csv");



            List<Contact> contactslist = GetContactsFromBytes(bytes, path);
            if (ReferenceEquals(contactslist, null))
                Console.WriteLine("null");
            else
                foreach (var value in contactslist)
                {
                    Console.WriteLine($"{value.FullName} {value.CompanyName} {value.Position} {value.Country} {value.Email}");
                }
        }
    }
}
