using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;

namespace Utility {
    public sealed class ExcelFileProcessor
    {
        private readonly HttpPostedFileBase _excelFile;
        private readonly IList<string> _requiredColumns;
        private readonly string[] _allowedFileTypes;
        private static string _uploadType;
        private string _connectionString;

        public ExcelFileProcessor(HttpPostedFileBase excelFile, IList<string> requiredColumns, string fileLocation, string uploadType)
        {
            _excelFile = excelFile;
            _requiredColumns = requiredColumns;
            _allowedFileTypes = new[] { ".xls", ".xlsx" };
            _uploadType = uploadType;
            Initialize(fileLocation);
        }

        private void Initialize(string location)
        {
            var fileExtension = Path.GetExtension(_excelFile.FileName);
            if (fileExtension == null || !_allowedFileTypes.Contains(fileExtension.ToLowerInvariant()))
                throw new FileLoadException("Incorrect file extension. Only .xls and .xlsx are allowed");

            var fileLocation = SaveFile(fileExtension, location);

            _connectionString =
                String.Format(
                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"",
                    fileLocation);

            if (fileExtension.ToLowerInvariant() == ".xls")
                _connectionString =
                    String.Format(
                        "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"",
                        fileLocation);
        }

        private string SaveFile(string fileExtension, string location)
        {
            var uniqueFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            var fileBytes = StreamToByteArray(_excelFile.InputStream);
            var fileName = SaveFile(uniqueFileName, fileExtension, fileBytes, location);
            return String.Format("{0}/{1}", location, fileName);
        }
		public static string SaveFile(string uniqueFileName, string extension, byte[] file, string path,
		   string prefix = null) {
			if (uniqueFileName == null) throw new ArgumentNullException("uniqueFileName");
			if (extension == null) throw new ArgumentNullException("extension");
			if (file == null) throw new ArgumentNullException("file");
			if (path == null) throw new ArgumentNullException("path");

			if (extension.StartsWith("."))
				extension = extension.TrimStart(new[] { '.' });

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			string fileName;
			var prefixWithDash = !String.IsNullOrWhiteSpace(prefix) ? String.Format("{0}-", prefix) : "";

			using (
				var newFile =
					File.Create(String.Format("{0}/{1}{2}.{3}", path, prefixWithDash, uniqueFileName, extension),
						file.Length)) {
				newFile.Write(file, 0, file.Length);
				fileName = Path.GetFileName(newFile.Name);
			}

			return fileName;
		}

		public static byte[] StreamToByteArray(Stream input) {
			if (input.CanSeek)
				input.Seek(0, SeekOrigin.Begin);

			using (var output = new MemoryStream()) {
				input.CopyTo(output);
				return output.ToArray();
			}
		}
		public DataTable ProcessFile(out string errorMessage)
        {
            errorMessage = null;

            OleDbConnection excelConnection = null;
            DataTable metaTable = null;
            try
            {
                excelConnection = new OleDbConnection(_connectionString);
                excelConnection.Open();

                metaTable = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });
                if (metaTable == null) return null;

                //Check each sheet in workbook
                foreach (DataRow dataRow in metaTable.Rows)
                {
                    var sheetName = dataRow["TABLE_NAME"].ToString();
                    //Exclude any "Tables" that don't end with $', the normal table ending terminator
                    if (!sheetName.TrimEnd(Convert.ToChar("'")).EndsWith("$")) continue;

                    var table = new DataTable();
                    var query = String.Format("SELECT * FROM [{0}]", sheetName);
                    using (var dataAdapter = new OleDbDataAdapter(query, excelConnection))
                        dataAdapter.Fill(table);
                    //var check = AreAllCellsEmpty(table);

                    //if (check)
                    //{
                    //    errorMessage =
                    //        String.Format(
                    //            "No " + _uploadType.ToLower() + " were uploaded because there are no rows in sheet {0}. You will need to resolve the issues on your spreadsheet and attempt to upload the " + _uploadType.ToLower() + " again.",
                    //            sheetName);

                    //    //continue;
                    //    return null;
                    //}

                    //Remove all empty rows
                    table =
                        table.Rows.Cast<DataRow>()
                            .Where(
                                row =>
                                    !row.ItemArray.All(
                                        field =>
                                            field is DBNull || (field as string) == null ||
                                            String.CompareOrdinal((field as string).Trim(), string.Empty) == 0))
                            .CopyToDataTable();



                    //if (IsMissingRequiredColumns(table))
                    //{
                    //    errorMessage =
                    //        String.Format("No " + _uploadType.ToLower() + " were uploaded. The sheet '{1}' is missing the following required columns: {0}. You will need to resolve the issues on your spreadsheet and attempt to upload the " + _uploadType.ToLower() + " again.",
                    //            String.Join(", ", _requiredColumns.Distinct()), sheetName);
                    //    //continue;
                    //    return null;
                    //}

                    //var errorMessages = CheckInvalidFormats(table);
                    //if (errorMessages.Any())
                    //{
                    //    errorMessage =
                    //        String.Format("The following rows have invalid values: {0} in sheet {1}",
                    //            String.Join(", ", errorMessages.Distinct()), sheetName);

                    //    return null;
                    //}

                    return table;
                }
            }
            finally
            {
                if (metaTable != null) metaTable.Dispose();
                if (excelConnection != null) excelConnection.Dispose();
            }

            return null;
        }

        public static bool AreAllCellsEmpty(DataTable table)
        {

            if (_uploadType.Equals("Location"))
            {
                return !(from DataRow row in table.Rows let city = row["City"] let state = row["State"] let zip = row["Zip"] let locationNumber = row["LocationNumber"] let address1 = row["Address1"] where !string.IsNullOrEmpty(locationNumber.ToString()) && !string.IsNullOrEmpty(address1.ToString()) && !string.IsNullOrEmpty(city.ToString()) && !string.IsNullOrEmpty(state.ToString()) && !string.IsNullOrEmpty(zip.ToString()) select locationNumber).Any();
            }

            

            return !(from DataRow row in table.Rows let retailerAcronym = row["RetailerAcronym"] let storeNumber = row["StoreNumber"] where !string.IsNullOrEmpty(retailerAcronym.ToString()) && !string.IsNullOrEmpty(storeNumber.ToString()) select retailerAcronym).Any();
        }


        private static IList<string> CheckInvalidFormats(DataTable table)
        {
            var errorMessages = new List<string>();
            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                var errorOccurred = false;

                foreach (DataColumn column in table.Columns)
                {
                    if (column.ColumnName.EndsWith("Date"))
                    {
                        DateTime date;

                        var dateString = Convert.ToString(row[column]);
                        if (!String.IsNullOrWhiteSpace(dateString) &&
                            !DateTime.TryParse(dateString, out date))
                            errorOccurred = true;
                    }

                    if (column.ColumnName.EndsWith("Time"))
                    {
                        DateTime date;
                        TimeSpan time;

                        var timeString = Convert.ToString(row[column]);
                        if (!String.IsNullOrWhiteSpace(timeString) &&
                            !TimeSpan.TryParse(timeString, out time) &&
                            !DateTime.TryParse(timeString, out date))
                        {
                            errorOccurred = true;
                        }
                    }

                    if (errorOccurred)
                    {
                        errorMessages.Add(String.Format("{0}", i + 2)); //Offset for header row
                        break;
                    }
                }
            }

            return errorMessages;
        }

        private bool IsMissingRequiredColumns(DataTable table)
        {
            return _requiredColumns.Any(requiredColumn => !table.Columns.Contains(requiredColumn));
        }
    }
}