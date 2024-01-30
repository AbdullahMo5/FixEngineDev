1) Install Mysql Database
2) Install Visual Studio
3) Clone the repository onto local environment
4) Open the solution folder and double click on FixEngine.sln
5) Build the solution from visual studio:
Open Build menu and click on build solution (or press ctl+B)
6) Open appsettubgs.json file
-> under DefaultConnection -> change the user and password to the user and password of the local mysql database
7) Install EF Core migration tools: https://learn.microsoft.com/en-us/ef/core/cli/
8) Run migration:
open Tools -> click on NuGet Package Manager -> click on package manager console
9) In the package manager console type command and hit enter (Need to be only done once):
Update-database
10) Click on the green Play button on press F5 button to run the application
11) Once the backend runs successfully it will open swagger ui page.
12) Scroll down to the User section and click on /api/Create (Need to be only done once)
-> Click on try it out button
-> In the request body, enter email, firstname, lastname, password, confirm password. This will create an account that can be used to login