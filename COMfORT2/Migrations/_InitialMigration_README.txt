
---------------------------------------------------------------
 COMfORT2 (AMOS) Initial Database Configuration (for developers)
 ---------------------------------------------------------------

When initializing and populating the database for the first time:

1. In the Visual Studio menu bar, go to: Tools --> NuGet Package Manager --> Package Manager Console

2. In the console, type: update-database
	Note, adding the verbose flag (eg: update-database -Verbose) displays the SQL scripts being ran on the DB

The database is now created and initialized. 

After creating the tables, the migration will run the Seed method (Configuration.cs:15)
This will create one book (COMfORT V1 in-progress)

I am not sure how to Seed the files' binary data, so that's in a separate SQL file (_InitFiles.sql)
	
	NOTE: This is a very weird text file. 
	It contains binary data for many images (~18MB) 
	Handle it with care and don't get frustrated with it.

Run that script to generate the files used in the 9 "real" book pages.

The last piece is a video on one of the "real" pages. It's 40MB, so it's rough to include in the project.
See Tom to obtain this video, and then upload it on the assets of page 6. Tom will explain this.
