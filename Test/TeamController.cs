using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Test;

[Route("api/[controller]")]
    [ApiController]
    public class TeamMemberController : ControllerBase
    {
        private readonly string connectionString = "Default";

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                TeamMember teamMember = GetTeamMemberId(id);
                if (teamMember == null)
                    return NotFound(); 

                List<TaskData> assignedTasks = GetAssignedTasks(id);
                List<TaskData> createdTasks = GetCreatedTasks(id);

                return Ok(new
                {
                    TeamMember = teamMember,
                    AssignedTasks = assignedTasks,
                    CreatedTasks = createdTasks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error");
            }
        }

        [HttpDelete("{projectId}")]
        public IActionResult Delete(int projectId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    
                    DeleteTasksProjectId(projectId, connection, transaction);

                 
                    DeleteProjectId(projectId, connection, transaction);

                    transaction.Commit();

                    return Ok("Project and associated tasks deleted successfully");
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    return StatusCode(500, $"Error deleting project");
                }
            }
        }

        private TeamMember GetTeamMemberId(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM TeamMember WHERE IdTeamMember = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TeamMember
                            {
                                Id = Convert.ToInt32(reader["IdTeamMember"]),
                                Firstname = reader["FirstName"].ToString(),
                                Lastname = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString()
                            };
                        }
                        return null;
                    }
                }
            }
        }

        private List<TaskData> GetAssignedTasks(int memberId)
        {
            List<TaskData> tasks = new List<TaskData>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT t.Name, t.Description, t.Deadline, p.Name AS ProjectName, tt.Name AS TaskType " +
                               "FROM Task t " +
                               "JOIN Project p ON t.IdProject = p.IdProject " +
                               "JOIN TaskType tt ON t.IdTaskType = tt.IdTaskType " +
                               "WHERE t.IdAssignedTo = @MemberId " +
                               "ORDER BY t.Deadline DESC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberId", memberId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskData
                            {
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Deadline = Convert.ToDateTime(reader["Deadline"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                TaskType = reader["TaskType"].ToString()
                            });
                        }
                    }
                }
            }
            return tasks;
        }

        private List<TaskData> GetCreatedTasks(int memberId)
        {
            List<TaskData> tasks = new List<TaskData>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT t.Name, t.Description, t.Deadline, p.Name AS ProjectName, tt.Name AS TaskType " +
                               "FROM Task t " +
                               "JOIN Project p ON t.IdProject = p.IdProject " +
                               "JOIN TaskType tt ON t.IdTaskType = tt.IdTaskType " +
                               "WHERE t.IdCreator = @MemberId " +
                               "ORDER BY t.Deadline DESC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MemberId", memberId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskData
                            {
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Deadline = Convert.ToDateTime(reader["Deadline"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                TaskType = reader["TaskType"].ToString()
                            });
                        }
                    }
                }
            }
            return tasks;
        }

        private void DeleteTasksProjectId(int projectId, SqlConnection connection, SqlTransaction transaction)
        {
            string deleteQuery = "DELETE FROM Task WHERE IdProject = @ProjectId";
            using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@ProjectId", projectId);
                command.ExecuteNonQuery();
            }
        }

        private void DeleteProjectId(int projectId, SqlConnection connection, SqlTransaction transaction)
        {
            string deleteQuery = "DELETE FROM Project WHERE IdProject = @ProjectId";
            using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@ProjectId", projectId);
                command.ExecuteNonQuery();
            }
        }
    }


