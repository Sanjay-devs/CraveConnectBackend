using System.Globalization;

namespace Test.Utilities
{
    public static class AppHelper
    {
        public static bool IsDateTime(object value)
        {
            if (value == null)
                return false;

            if (value is DateTime)
                return true;

            DateTime result;
            return DateTime.TryParse(value.ToString(), out result);
        }
        public static string ConvertDateFormat(string date)
        {
            string[] dateara = date.Split(' ');
            // Parse the date string using the exact format
            DateTime parsedDate = DateTime.ParseExact(dateara[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Format the date to the desired format
            string formattedDate = parsedDate.ToString("yyyy-MM-dd");

            return formattedDate;
        }
        public static string CheckFolder(string folderPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(folderPath))
                {
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return folderPath;
        }
    }
}
