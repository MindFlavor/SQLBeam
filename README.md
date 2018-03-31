# SQLBeam

The purpose of this tool is to automate and centralize some common SQL Server tasks. With this tool you can collect data from multiple SQL Server instances concurrently. You can also automate some tasks (like backup or CHECKDB) so you can, from a single point of view, keep an eye of these critical tasks. 

## Architecture

The architecture is really simple. Just a process (a console one right now but it will be a windows service in the future) backed by a SQL Server database. The database provides process serialization so you can have multiple concurrent instances of the process splitting the tasks between them. 

The main entity in the database is the `Task`. A task represents an operation done by SQLBeam. It can be anything ranging from a backup, an ETL (more on that later) or even a non SQL task (like a WMI query, for example). Tasks are run concurrently: each instance of SQLBeam can run a configurable number at tasks at the same time. 
Tasks are run against `Destination`s. So, for example, you can run a CHECKDB against a specific database of a specific SQL Server instance. This decoupling allows for more flexibility than you can achieve with SQL Server agent. A `task` with a `Destination` is called `ExecutableTask`.

SQLBeam offers a number of predefined tasks but you can implement you tasks on your own. Let's say you want to collect the `sys.server_logins` of every server. You just write a `TSQLETL task` with the proper T-SQL (like `SELECT * FROM sys.server_logins`) and then run it against your `Destination`s. SQLBeam will take care of running in concurrently and collect the data for you.

### Task dependencies

ExecutableTasks can depend on other ExecutableTasks. This kind of precedence allows to serialize tasks. For example you might want to avoid performing a backup of every database of an instance at the same time. Specifying a dependency you can put an order on these backups. Also, an ExecutableTask can depend of more than one ExecutableTask giving more flexibility. Right now there is no discrimination between a task completed successfully or not but that will be addressed in the next release.





## Known issues

### HttpListenerException: Access is denied

This happens because the REST interface uses port 9000 and that port access has not been granted to the executing user. In order to correct it issue this command from an administrative prompt:

```
netsh http add urlacl url=http://+:9000/api user=DOMAIN\user
```
 
Where `DOMAIN\user` is the account starting the program (it will be you if running from VS)


