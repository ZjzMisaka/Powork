using System.Data.SQLite;
using Newtonsoft.Json;
using Powork.Model;

namespace Powork.Repository
{
    public class TeamMessageRepository
    {
        public static void InsertMessage(TeamMessage teamMessage)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string body = JsonConvert.SerializeObject(teamMessage.MessageBody);

            string sql = $"INSERT INTO TTeamMessage (body, type, time, fromIP, fromName, teamID) VALUES (@body, @type, @time, @fromIP, @fromName, @teamID)";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.Add(new SQLiteParameter("@body", body));
                command.Parameters.Add(new SQLiteParameter("@type", teamMessage.Type));
                command.Parameters.Add(new SQLiteParameter("@time", teamMessage.Time));
                command.Parameters.Add(new SQLiteParameter("@fromIP", teamMessage.SenderIP));
                command.Parameters.Add(new SQLiteParameter("@fromName", teamMessage.SenderName));
                command.Parameters.Add(new SQLiteParameter("@teamID", teamMessage.TeamID));
                command.ExecuteNonQuery();
            }
        }

        public static List<TeamMessage> SelectMessgae(string teamID, int id = -1)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<TeamMessage> teamMessageList = new List<TeamMessage>();

            string sql = $"SELECT * FROM TTeamMessage WHERE teamID = '{teamID}'";
            if (id != -1)
            {
                sql = $"{sql}  AND  id < {id}";
            }
            sql = $"{sql}  ORDER BY time DESC LIMIT 20";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teamMessageList.Add(new TeamMessage()
                        {
                            SenderIP = reader["fromIP"].ToString(),
                            SenderName = reader["fromName"].ToString(),
                            MessageBody = JsonConvert.DeserializeObject<List<TCPMessageBody>>(reader["body"].ToString()),
                            Type = (MessageType)(int.Parse(reader["type"].ToString())),
                            Time = reader["time"].ToString(),
                            ID = int.Parse(reader["id"].ToString()),
                            TeamID = teamID,
                        });
                    }
                    teamMessageList.Reverse();
                }
            }
            return teamMessageList;
        }
    }
}
