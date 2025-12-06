using Powork.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class HolidayRepository
    {
        public static void InsertOrUpdateHoliday(Holiday holiday)
        {
            using (SQLiteConnection connection = CommonRepository.GetConnection())
            {
                string sql = "INSERT OR REPLACE INTO THoliday (year, month, day, isHoliday) VALUES (@Year, @Month, @Day, @IsHoliday)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Year", holiday.Year);
                    command.Parameters.AddWithValue("@Month", holiday.Month);
                    command.Parameters.AddWithValue("@Day", holiday.Day);
                    command.Parameters.AddWithValue("@IsHoliday", holiday.IsHoliday);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<Holiday> SelectHolidays(int year, int month)
        {
            var holidays = new List<Holiday>();
            using (SQLiteConnection connection = CommonRepository.GetConnection())
            {
                string sql = "SELECT day, isHoliday FROM THoliday WHERE year = @Year AND month = @Month";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@Month", month);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            holidays.Add(new Holiday {
                                Year = year,
                                Month = month,
                                Day = Convert.ToInt32(reader["day"]),
                                IsHoliday = Convert.ToBoolean(reader["isHoliday"])
                            });
                        }
                    }
                }
            }
            return holidays;
        }

        public static List<Holiday> SelectHolidays(int year)
        {
            var holidays = new List<Holiday>();
            using (SQLiteConnection connection = CommonRepository.GetConnection())
            {
                string sql = "SELECT month, day, isHoliday FROM THoliday WHERE year = @Year";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            holidays.Add(new Holiday {
                                Year = year,
                                Month = Convert.ToInt32(reader["month"]),
                                Day = Convert.ToInt32(reader["day"]),
                                IsHoliday = Convert.ToBoolean(reader["isHoliday"])
                            });
                        }
                    }
                }
            }
            return holidays;
        }

        public static List<Holiday> SelectHolidaysForRange(DateTime start, DateTime end)
        {
            var holidays = new List<Holiday>();
            for (int year = start.Year; year <= end.Year; year++)
            {
                holidays.AddRange(SelectHolidays(year));
            }
            return holidays.Where(h => new DateTime(h.Year, h.Month, h.Day) >= start && new DateTime(h.Year, h.Month, h.Day) <= end).ToList();
        }


        public static void RemoveHoliday(int year, int month, int day)
        {
            using (SQLiteConnection connection = CommonRepository.GetConnection())
            {
                string sql = "DELETE FROM THoliday WHERE year = @Year AND month = @Month AND day = @Day";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    command.Parameters.AddWithValue("@Month", month);
                    command.Parameters.AddWithValue("@Day", day);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}