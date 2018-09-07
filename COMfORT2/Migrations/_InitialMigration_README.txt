

For initializing and populating the database for the first time:

1. In the Visual Studio menu bar, go to: Tools --> NuGet Package Manager --> Package Manager Console

2. In the console, type: update-database
	Note, adding the verbose flag (eg: update-database -Verbose) displays the SQL scripts being ran on the DB

The database is now created and initialized. To populate it with one book (COMfORT V1 in-progress) run the SQL script: _Init.sql

