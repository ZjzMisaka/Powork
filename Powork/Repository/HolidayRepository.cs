using Powork.Model;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class HolidayRepository
    {
        public static void InsertOrUpdateHoliday(Holiday holiday)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT OR REPLACE INTO THoliday (year, month, day, isHoliday) VALUES ({holiday.Year}, {holiday.Month}, {holiday.Day}, {holiday.IsHoliday})";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveHoliday(int year, int month, int day)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM THoliday WHERE year = {year} AND month = {month} AND day = {day}";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
