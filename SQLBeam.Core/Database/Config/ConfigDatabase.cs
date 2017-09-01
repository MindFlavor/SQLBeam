using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBeam.LoggingExtensions;
using System.Data.SqlClient;
using SQLBeam.Core.Database.Config;

namespace SQLBeam.Core.Database
{
    public class ConfigDatabase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConfigDatabase));
        private Configuration Configuration;

        public ConfigDatabase(Configuration Configuration)
        {
            this.Configuration = Configuration;
        }

        public static string GetText(string name)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name)))
            {
                return sr.ReadToEnd();
            }
        }

        public List<Tasks.ExecutableTask> ScheduleTasks(Guid reactorGUID, long lTaskToSchedule)
        {
            List<TaskInState> lTaskInStates = new List<TaskInState>();

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                string tsql_Retrieve = GetText("SQLBeam.Core.Database.Config.RetrieveWaitingTasks.sql");
                string tsql_Schedule = GetText("SQLBeam.Core.Database.Config.InsertIntoScheduled.sql");

                using (var transaction = conn.BeginTransaction())
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    using (SqlCommand cmd = new SqlCommand(tsql_Retrieve, conn, transaction))
                    {
                        SqlParameter param = new SqlParameter("@tasksToSchedule", System.Data.SqlDbType.Int, 4);
                        param.Value = (int)lTaskToSchedule;
                        cmd.Parameters.Add(param);

                        using (SqlDataAdapter ada = new SqlDataAdapter(cmd))
                        {
                            int iRows = ada.Fill(dt);
                            if (iRows > 0)
                                log.DebugFormat("Found {0:N} rows", iRows);
                        }
                    }

                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        TaskInState scheduledTask = new TaskInState()
                        {
                            TaskState = TaskState.Scheduled,
                            Guid = (Guid)row[0],
                            Server = reactorGUID,
                            DestinationID = (int)row[1],
                            TaskID = (int)row[2],
                            WaitStartTime = (DateTime)row[4]
                        };
                        if (!(row[3] is DBNull))
                            scheduledTask.Parameters = (string)row[3];

                        using (SqlCommand cmd = new SqlCommand(tsql_Schedule, conn, transaction))
                        {
                            SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier, 16);
                            param.Value = scheduledTask.Guid;
                            cmd.Parameters.Add(param);

                            param = new SqlParameter("@Server", System.Data.SqlDbType.UniqueIdentifier, 16);
                            param.Value = scheduledTask.Server;
                            cmd.Parameters.Add(param);

                            param = new SqlParameter("@Destination_ID", System.Data.SqlDbType.Int, 4);
                            param.Value = scheduledTask.DestinationID;
                            cmd.Parameters.Add(param);

                            param = new SqlParameter("@Task_ID", System.Data.SqlDbType.Int, 4);
                            param.Value = scheduledTask.TaskID;
                            cmd.Parameters.Add(param);

                            param = new SqlParameter("@Parameters", System.Data.SqlDbType.NVarChar, -1);
                            param.Value = string.IsNullOrEmpty(scheduledTask.Parameters) ? (object)DBNull.Value : scheduledTask.Parameters;
                            cmd.Parameters.Add(param);

                            param = new SqlParameter("@WaitStartTime", System.Data.SqlDbType.DateTime2, 16);
                            param.Value = scheduledTask.WaitStartTime;
                            cmd.Parameters.Add(param);

                            cmd.ExecuteNonQuery();

                            log.Trace("Inserted one scheduled task");
                        }


                        lTaskInStates.Add(scheduledTask);
                    }

                    transaction.Commit();
                }
            }

            List<Tasks.ExecutableTask> lExecutables = new List<Tasks.ExecutableTask>();
            lTaskInStates.ForEach(waitingTask =>
            {
                log.Trace("Creating instance via reflection");
                Tasks.ITask task = this.CreateTaskReflecting(waitingTask.TaskID);

                Tasks.ExecutableTask et = new Tasks.ExecutableTask()
                {
                    Task = task,
                    Destination = this.GetDestinationByID(waitingTask.DestinationID),
                    Guid = waitingTask.Guid
                };

                et.Task.Personalize(waitingTask.Parameters);

                lExecutables.Add(et);
            });

            return lExecutables;
        }

        #region Tasks.ITask
        private Tasks.ITask TaskFromReader(SqlDataReader reader)
        {
            Tasks.TaskBase tb = new Tasks.TaskBase()
            {
                ID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Class = reader.GetString(2),
                TaskParameters = null,
                IsDebug = reader.GetBoolean(4),
                Configuration = Configuration
            };

            if (!(reader.IsDBNull(3)))
                tb.TaskParameters = reader.GetString(3);

            return tb;
        }

        public List<Tasks.ITask> GetTasks()
        {
            List<Tasks.ITask> lTasks = new List<Tasks.ITask>();
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetTasks.sql"), conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            lTasks.Add(TaskFromReader(reader));
                    }
                }
            }

            return lTasks;
        }

        public Tasks.ITask GetTaskByID(int taskID)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetTaskByID.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@ID", System.Data.SqlDbType.Int, 4);
                    param.Value = taskID;
                    cmd.Parameters.Add(param);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exceptions.IDNotFoundException<int>(taskID, "Task");
                        else
                            return TaskFromReader(reader);
                    }
                }
            }
        }

        public Tasks.ITask GetTaskByName(string taskName)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetTaskByName.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@Name", System.Data.SqlDbType.NVarChar, 255);
                    param.Value = taskName;
                    cmd.Parameters.Add(param);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exceptions.TaskNotFoundException(taskName);
                        else
                            return TaskFromReader(reader);
                    }
                }
            }
        }

        public Tasks.ITask CreateTaskReflecting(int taskID)
        {
            // retrieve task details
            var taskBase = GetTaskByID(taskID);

            log.DebugFormat("Reflect lookup for {0:S}", taskBase.Class);

            // look for Class type
            var type = System.Reflection.Assembly.GetExecutingAssembly().GetType(taskBase.Class);
            if (type == null)
            {
                log.WarnFormat("Type not found: {0:S}", taskBase.Class);
                throw new Exceptions.TypeNotFoundException(taskBase.Class);
            }
            log.DebugFormat("Reflected type {0:S}", type.ToString());

            Tasks.ITask it = (Tasks.ITask)Activator.CreateInstance(type);

            if (it == null)
            {
                throw new Exception(string.Format("Type {0:S} does not implements Tasks.ITask!", type.ToString()));
            }
            log.DebugFormat("Created task {0:S}", it.ToString());

            it.ID = taskBase.ID;
            it.Class = taskBase.Class;
            it.TaskParameters = taskBase.TaskParameters;
            it.Configuration = Configuration;

            it.Initialize();

            log.DebugFormat("Completed creation of class instance {0:S}", it.ToString());

            return it;
        }
        #endregion

        #region Destination
        private Destination DestinationFromReader(SqlDataReader reader)
        {
            return new Destination()
            {
                ID = reader.GetInt32(0),
                Name = reader.GetString(1),
                ConnectionString = reader.GetString(2)
            };
        }

        public List<Destination> GetDestinations()
        {
            List<Destination> lDests = new List<Destination>();

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetDestinations.sql"), conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            lDests.Add(DestinationFromReader(reader));
                    }
                }
            }

            return lDests;
        }

        public Destination GetDestinationByName(string destinationName)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetDestinationByName.sql"), conn))
                {

                    SqlParameter param = new SqlParameter("@Name", System.Data.SqlDbType.NVarChar, 255);
                    param.Value = destinationName;
                    cmd.Parameters.Add(param);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exceptions.DestinationNotFoundException(destinationName);
                        else
                            return DestinationFromReader(reader);
                    }
                }
            }
        }

        public Destination GetDestinationByID(int destinationID)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetDestinationByID.sql"), conn))
                {

                    SqlParameter param = new SqlParameter("@ID", System.Data.SqlDbType.Int, 4);
                    param.Value = destinationID;
                    cmd.Parameters.Add(param);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exceptions.IDNotFoundException<int>(destinationID, "Destination");
                        else
                            return DestinationFromReader(reader);
                    }
                }
            }
        }
        #endregion


        #region Tasks.ExecutableTask
        public void MoveExecutableTaskToRunning(Tasks.ExecutableTask et)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.MoveExecutableTaskToRunning.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier, 16);
                    param.Value = et.Guid;
                    cmd.Parameters.Add(param);

                    int ret = cmd.ExecuteNonQuery();
                }
            }
        }
        public void MoveRunningTaskToCompleted(Tasks.ExecutableTask et)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.MoveRunningTaskToCompleted.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier, 16);
                    param.Value = et.Guid;
                    cmd.Parameters.Add(param);

                    int ret = cmd.ExecuteNonQuery();
                }
            }
        }

        public void MoveRunningTaskToError(Tasks.ExecutableTask et, Exception exce)
        {
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.MoveRunningTaskToErrored.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier, 16);
                    param.Value = et.Guid;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@ErrorText", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = exce.Message;
                    cmd.Parameters.Add(param);

                    int ret = cmd.ExecuteNonQuery();
                }
            }
        }

        public Guid AddTaskToWait(Destination destination, Tasks.ITask task)
        {
            log.Trace($"Called AddTaskToWait(destination == {destination.ToString():S}, task == {task.ToString():S})");
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.AddTaskToWait.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@Destination_ID", System.Data.SqlDbType.Int, 4);
                    param.Value = destination.ID;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@Task_ID", System.Data.SqlDbType.Int, 4);
                    param.Value = task.ID;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@Parameters", System.Data.SqlDbType.NVarChar, -1);
                    param.Value = string.IsNullOrEmpty(task.TaskParameters) ? (object)DBNull.Value : task.TaskParameters;
                    cmd.Parameters.Add(param);

                    return (Guid)cmd.ExecuteScalar();
                }
            }
        }

        private static TaskInState TaskInStateFromReader(SqlDataReader reader, int skew = 0)
        {
            string status = reader.GetString(0 + skew);

            TaskState ts = (TaskState)Enum.Parse(typeof(TaskState), status);

            TaskInState tis = new TaskInState
            {
                TaskState = ts,
                Guid = reader.GetGuid(1 + skew),
                DestinationID = reader.GetInt32(3 + skew),
                TaskID = reader.GetInt32(4 + skew),
            };

            if (!(reader[2 + skew] is DBNull))
                tis.Server = reader.GetGuid(2 + skew);

            if (!(reader[5 + skew] is DBNull))
                tis.Parameters = reader.GetString(5 + skew);

            if (!(reader[6 + skew] is DBNull))
                tis.WaitStartTime = reader.GetDateTime(6 + skew);
            if (!(reader[7 + skew] is DBNull))
                tis.ScheduledTime = reader.GetDateTime(7 + skew);
            if (!(reader[8 + skew] is DBNull))
                tis.StartTime = reader.GetDateTime(8 + skew);
            if (!(reader[9 + skew] is DBNull))
                tis.CompleteTime = reader.GetDateTime(9 + skew);
            if (!(reader[10 + skew] is DBNull))
                tis.ErrorTime = reader.GetDateTime(10 + skew);
            if (!(reader[11 + skew] is DBNull))
                tis.ErrorText = reader.GetString(11 + skew);

            return tis;
        }

        public List<TaskInState> GetTasksInStates(int putInWaitMinutesAgo)
        {
            log.Trace($"Called GetTasksInState({putInWaitMinutesAgo:N0})");
            List<TaskInState> lTaskInStates = new List<TaskInState>();
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetTasksInStates.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@minutesAgo", System.Data.SqlDbType.Int, 4);
                    param.Value = putInWaitMinutesAgo;
                    cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lTaskInStates.Add(TaskInStateFromReader(reader));
                        }
                    }
                }
            }

            return lTaskInStates;
        }

        public TaskInState GetTaskInState(Guid guid)
        {
            log.Trace($"Called GetTaskInState({guid.ToString():S})");
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetTaskInStateByGUID.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@GUID", System.Data.SqlDbType.UniqueIdentifier, 16);
                    param.Value = guid;
                    cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return TaskInStateFromReader(reader);
                        }
                        else
                        {
                            log.Trace($"ID not found in [core].[AllTasks]: GUID == {guid}");
                            throw new Exceptions.IDNotFoundException<Guid>(guid, "[core].[AllTasks]");
                        }
                    }
                }
            }
        }

        public void CleanTaskInState(TaskState ts)
        {
            string tsql = "TRUNCATE TABLE ";
            switch (ts)
            {
                case TaskState.Completed:
                    tsql += " [core].[CompletedTasks];";
                    break;
                case TaskState.Error:
                    tsql += " [core].[ErroredTasks];";
                    break;

                default:
                    throw new NotImplementedException($"Cannot clean tasks of type {ts.ToString()}");
            }

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(tsql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private BatchWithTasks NewBatch()
        {
            log.Trace($"Called NewBatch()");

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.NewBatch.sql"), conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new BatchWithTasks()
                            {
                                GUID = reader.GetGuid(0),
                                CreationTime = reader.GetDateTime(1)
                            };
                        }
                        else
                        {
                            throw new Exceptions.DatabaseException("Cannot create a new batch");
                        }
                    }
                }
            }
        }
        #endregion

        #region Batch
        public BatchWithTasks AddBatchTasks(Tasks.ITask t, int[] destinationIDs)
        {
            log.Trace($"Called AddBatchTask({t.ToString():S})");

            BatchWithTasks batch = NewBatch();

            System.Data.DataTable dtTasks = new System.Data.DataTable();
            dtTasks.Columns.Add(new System.Data.DataColumn("GUID", typeof(Guid)));
            dtTasks.Columns.Add(new System.Data.DataColumn("Destination_ID", typeof(int)));
            dtTasks.Columns.Add(new System.Data.DataColumn("Task_ID", typeof(int)));
            dtTasks.Columns.Add(new System.Data.DataColumn("WaitStartTime", typeof(DateTime)));

            System.Data.DataTable dtBatchTasks = new System.Data.DataTable();
            dtBatchTasks.Columns.Add(new System.Data.DataColumn("Batch_GUID", typeof(Guid)));
            dtBatchTasks.Columns.Add(new System.Data.DataColumn("Task_GUID", typeof(Guid)));

            DateTime dtStart = DateTime.Now;

            foreach (int destinationID in destinationIDs)
            {
                Guid g = Guid.NewGuid();

                dtTasks.Rows.Add(new object[] {
                    g,
                    destinationID,
                    t.ID,
                    dtStart
                });

                dtBatchTasks.Rows.Add(new object[]
                {
                    batch.GUID,
                    g
                });

                batch.Task_GUIDs.Add(g);
            }

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();

                using (SqlBulkCopy bcp = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, null))
                {
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("GUID", "GUID"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Destination_ID", "Destination_ID"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Task_ID", "Task_ID"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("WaitStartTime", "WaitStartTime"));

                    bcp.DestinationTableName = "[core].WaitingTasks";

                    bcp.EnableStreaming = true;

                    bcp.WriteToServer(dtTasks);
                }
            }

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();

                using (SqlBulkCopy bcp = new SqlBulkCopy(conn, SqlBulkCopyOptions.KeepIdentity, null))
                {
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Batch_GUID", "Batch_GUID"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Task_GUID", "Task_GUID"));

                    bcp.DestinationTableName = "[core].[BatchTasks]";

                    bcp.EnableStreaming = true;

                    bcp.WriteToServer(dtBatchTasks);
                }
            }

            return batch;
        }


        private static Batch GetBatchFromReader(SqlDataReader reader)
        {
            return new Batch(reader.GetGuid(0), reader.GetDateTime(1));
        }

        public List<Batch> GetBatches(int putInWaitMinutesAgo)
        {
            log.Trace($"Called GetBatches({putInWaitMinutesAgo:N0})");
            List<Batch> lBatches = new List<Batch>();
            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetText("SQLBeam.Core.Database.Config.GetBatches.sql"), conn))
                {
                    SqlParameter param = new SqlParameter("@minutesAgo", System.Data.SqlDbType.Int, 4);
                    param.Value = putInWaitMinutesAgo;
                    cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lBatches.Add(GetBatchFromReader(reader));
                        }
                    }
                }
            }

            return lBatches;
        }

        public List<BatchWithTaskInStates> GetBatchWithTaskInStatesByGUIDs(IEnumerable<Guid> guids)
        {
            #region prepare dt with guids
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add(new System.Data.DataColumn("GUID", typeof(Guid)));

            foreach (Guid g in guids)
                dt.Rows.Add(g);
            #endregion

            List<BatchWithTaskInStates> lBwts = new List<BatchWithTaskInStates>();
            BatchWithTaskInStates curBwts = null;

            using (SqlConnection conn = new SqlConnection(Configuration.ConfigConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("[core].[GetBatchWithTaskInStatesByGUIDs]", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    SqlParameter param = new SqlParameter("@guids", System.Data.SqlDbType.Structured);
                    param.Value = dt;
                    cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (curBwts == null || curBwts.GUID != reader.GetGuid(0))
                            {
                                curBwts = new BatchWithTaskInStates(
                                    (Guid)reader["Batch_GUID"],
                                    (DateTime)reader["CreationTime"]);
                                lBwts.Add(curBwts);
                            }

                            curBwts.TaskInStates.Add(TaskInStateFromReader(reader, skew: 4));
                        }
                    }
                }
            }

            return lBwts;
        }
        #endregion
    }
}
