using Spectre.Console;
using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GasolineManagementSystem.Sales;

namespace GasolineManagementSystem
{
    //THE MAIN (YEAH RIGHT)
    class Program
    {
        static void Main(string[] args)
        {
            bool isRunning = true; // Keeps the program running

            while (isRunning) //Mo loop ni siya until mag exit ang user
            {
                Console.Clear(); 

                //  "PAGAS.CO" nga laysho-laysho
                var pagasCoText = new FigletText("PAGAS.CO").Centered().Color(Color.Red);
                AnsiConsole.Write(pagasCoText);

                System.Threading.Thread.Sleep(1500); 

                // Diri mag select ang user if manager ba or consumer
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Select your role:[/]")
                        .AddChoices("Manager", "Consumer", "Exit")
                        .HighlightStyle(new Style(Color.Yellow)));

                User user; // Para ma access ang user class

                if (choice == "Manager")
                {
                    user = new Manager(); // Mo access sa Manager class
                }
                else if (choice == "Consumer")
                {
                    user = new Consumer(); // Mo access sa Consumer class
                }
                else if (choice == "Exit")
                {
                    Console.Clear();
                    AnsiConsole.MarkupLine("[bold red]Exiting system. Goodbye![/]");
                    isRunning = false; // Mo exit na ang user
                    break;
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]Invalid selection. Try again.[/]");
                    continue; // Mo restart ang loop if invalid ang input
                }

                Console.Clear(); 
                user.ShowMenu(); //Mo access sa ShowMenu method sa user class
            }
        }
    }


    //SUPERCLASS USER
    public abstract class User
    {
        // Mao ni ang method nga gi access sa Main para ma show ang menu sa user
        public abstract void ShowMenu(); 
    }


    //MANAGER CLASS INHERITS FROM USER
    class Manager : User 
    {
        private const int MaxManagers = 3; //Para ma limit ang managers
        private List<ManagerAccount> managerAccounts = new List<ManagerAccount>(); // Mag store sa mga manager accounts
        private string existingManagerMessage; //Para ma display ang existing managers
        private string filePath = "managers.txt"; //FilePath Para ma save ang manager data

        public Manager() //Constructor
        {
            LoadManagers(); //para ma load ang mga managers mo read sa file
            UpdateExistingManagerMessage(); // Mag update sa message based sa existing managers
        }

        private void LoadManagers() //Para ma load ang mga managers
        {
            try
            {
                if (File.Exists(filePath)) //Ma access ang file kung naa
                {
                    string[] managerLines = File.ReadAllLines(filePath); // Ma read ang tanan nga lines sa file
                    foreach (string line in managerLines) //Mag loop sa tanan nga lines sa file
                    {
                        string[] managerData = line.Split('|'); //Para split ang line into parts, separator kunuhay ang "|"
                        if (managerData.Length == 4) //Para ma sure nga complete ang data
                        {
                            managerAccounts.Add(new ManagerAccount( //  Para ma add ang new manager account sa list
                                managerData[0], managerData[1], managerData[2], managerData[3])); //Para ma access ang mga parts sa line orayt
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No manager data file found."); // Mo print ug error if wala
                }
            }
            catch (Exception ex) // Mo handle ug tanan nga exceptions 
            {
                Console.WriteLine("Error loading managers: " + ex.Message); // Printerror message
            }
        }

        private void UpdateExistingManagerMessage() //Para ma update ang message based sa existing managers
        {
            //Mo check if naa ba
            existingManagerMessage = managerAccounts.Count > 0 //If naa
                //Kung naa, i list or print ang mga usernames
                ? "Managers: " + string.Join(", ", managerAccounts.Select(m => m.Username)) //mo combine sa mga usernames ug single string
                // kung walay manager kay mo appear ang message
                : "No managers available";
        }

        public override void ShowMenu() //diri na ang exiting part kay e show ang menu yey 
        {
            //Variable para ma keep ang menu ug run
            bool running = true;

            //Mo loop ni siya until mag exit ang user
            while (running)
            {
               
                Console.Clear();

                // charchar rani para ma display ang menu
                var menuPanel = new Panel(
                        
                        new Markup($"[bold yellow]Manager Menu[/]\n\n[bold red]{existingManagerMessage}[/]"))
                    .Border(BoxBorder.Heavy) 
                    .BorderStyle(new Style(Color.Yellow)) 
                    .Padding(1, 1) 
                    .Expand(); 

                
                AnsiConsole.Write(menuPanel);

                // show menu with options 
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Please select an option:[/]") 
                        .AddChoices("Create Account", "Login", "Forgot Passcode", "Back to User Menu", "Exit") // Menu options 
                        .HighlightStyle(new Style(Color.Orange1))); 

                
                switch (choice)
                {
                    case "Create Account":
                        // Call ang method to create a new account 
                        CreateAccount();
                        break;

                    case "Login":
                        // Call ang method to handle login
                        Login();
                        break;

                    case "Forgot Passcode":
                        // Call ang method to reset the passcode kung nalimtan char!
                        ForgotPasscode();
                        break;

                    case "Back to User Menu":
                        // E stop ang loop nya balik sa user menu
                        running = false;
                        break;

                    case "Exit":
                        //Mo exit sa program
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void CreateAccount()//create account method
        {
            Console.Clear();

            // mangayo ug username
            string username = AnsiConsole.Ask<string>("Enter [green]username[/]:");

            // mangayo ug password
            string password = AnsiConsole.Ask<string>("Enter [green]password[/]:");

            // pili ug question
            var questionPrompt = new SelectionPrompt<string>()
                .Title("Select a [blue]security question[/]:")
                .AddChoices("Best friend's name?", "Favorite color?", "Birthday?");
            string securityQuestion = AnsiConsole.Prompt(questionPrompt);

            // answeran ang question
            string securityAnswer = AnsiConsole.Ask<string>($"Answer for [blue]{securityQuestion}[/]:");

            //Ma save ang username, pass and etc and mahimo na ang acc sa file
            managerAccounts.Add(new ManagerAccount(username, password, securityQuestion, securityAnswer));
            SaveManagerAccounts();

            // If nahimo na print success message
            AnsiConsole.MarkupLine("[green]Account has been created successfully![/]");
            AnsiConsole.MarkupLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void SaveManagerAccounts() //save manager accounts method
        {
            try
            {
                // ma convert ni siya into a list of strings
                List<string> lines = managerAccounts
                    .Select(m => $"{m.Username}|{m.Password}|{m.SecurityQuestion}|{m.SecurityAnswer}")
                    .ToList();

                // Ma write ang tanan nga lines sa filepath sa manager accounts
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                //kung naay error sa pag save sa file e print ni siya
                Console.WriteLine("Error saving accounts: " + ex.Message);
            }
        }

        private void Login() //login method
        {
            
            Console.Clear();

            
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

           
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            // Mangita ug manager account nga parehas sa username ug password
            var manager = managerAccounts.FirstOrDefault(m => m.Username == username && m.Password == password);

            // Check if a matching account was found
            if (manager != null)
            {
                // If found, display success message and show the manager's main menu 
                Console.WriteLine("Login successful!");
                ShowManagerMainMenu();
            }
            else
            {
                // If no match, display an error message and wait for the user to press a key
                Console.WriteLine("Invalid credentials.");
                Console.ReadKey();
            }
        }

        private void ChangePassword()//change password method
        {
            
            Console.Clear();

             //pisliton ra unsa nga user name e change ang passcode
            var usernamePrompt = new SelectionPrompt<string>()
                .Title("Select your [green]username[/]:") 
                .PageSize(10) 
                .AddChoices(managerAccounts.Select(m => m.Username).ToList()); 

            // Get the selected username
            string username = AnsiConsole.Prompt(usernamePrompt);

            // Find the manager account that matches the selected username
            var managerAccount = managerAccounts.FirstOrDefault(m => m.Username == username);

            if (managerAccount != null)
            {
                // display ang security question
                Console.Clear();
                AnsiConsole.MarkupLine($"Security question: [blue]{managerAccount.SecurityQuestion}[/]");
                Console.Write("Answer: ");
                string securityAnswer = Console.ReadLine();

                //E check if sakto ang answer before mag change sa passcode
                if (securityAnswer == managerAccount.SecurityAnswer)
                {
                    Console.Clear();
                    Console.Write("Enter your old password: ");
                    string oldPassword = Console.ReadLine();

                    // Check if old password matches
                    if (managerAccount.Password == oldPassword)
                    {
                        Console.Clear();
                        Console.Write("Enter your new password: ");
                        string newPassword = Console.ReadLine();

                        // Update the password and save the account
                        managerAccount.Password = newPassword;
                        SaveManagerAccounts();

                        // Confirmed!!!
                        AnsiConsole.MarkupLine("[green]Password changed successfully![/]");
                    }
                    else
                    {
                        // Display an error if the old password is incorrect
                        AnsiConsole.MarkupLine("[red]Incorrect old password.[/]");
                    }
                }
                else
                {
                    // Display an error if the security answer is incorrect
                    AnsiConsole.MarkupLine("[red]Incorrect answer to security question.[/]");
                }
            }
            else
            {
                // Display an error if the account is not found
                AnsiConsole.MarkupLine("[red]Account not found.[/]");
            }

            // Pause and wait for the user to press a key before returning to the menu
            AnsiConsole.MarkupLine("\nPress any key to continue...");
            Console.ReadKey();
        }
       
        private void ShowAccountOptions()//account options method
        {
            //E keep ang account menu running hangtud sa user mo decide ug exit
            bool inAccountMenu = true;

            while (inAccountMenu)
            {
                
                Console.Clear();

                // Pili sa options
                var optionsPrompt = new SelectionPrompt<string>()
                    .Title("[green]Account Options:[/]") 
                    .PageSize(4) 
                    .AddChoices("Change Password", "Delete Account", "Back to Previous Page", "Exit"); // Menu options

                // E Show the prompt nya get the user's choice 
                string choice = AnsiConsole.Prompt(optionsPrompt);

                // Handle the user's choice
                switch (choice)
                {
                    case "Change Password":
                        // Call the method to change the password
                        ChangePassword();
                        break;

                    case "Delete Account":
                        // Call the method to delete the account
                        DeleteAccount();
                        // Exit the account menu after deleting 
                        inAccountMenu = false;
                        break;

                    case "Back to Previous Page":
                        // Exit the account menu and go back to the previous page
                        inAccountMenu = false;
                        break;

                    case "Exit":
                        // Exit the entire console
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private void DeleteAccount()//delete account method
        {
            Console.Clear(); 
            Console.Write("Enter your username: "); 
            string usernameToDelete = Console.ReadLine(); // Get the username input.

            // pangitaon if sakto, dapat parehas ang username sa file ug sa input
            var managerAccount = managerAccounts.FirstOrDefault(m => m.Username == usernameToDelete);

            if (managerAccount != null) // Check if the account exists.
            {
                // Ask the security question saved for the account.
                Console.WriteLine($"Security question: {managerAccount.SecurityQuestion}");
                Console.Write("Answer: "); // Ask the user to answer the security question.
                string securityAnswer = Console.ReadLine(); 

                if (securityAnswer == managerAccount.SecurityAnswer) // Check if correct.
                {
                    managerAccounts.Remove(managerAccount); //e remove na ang acc sa file/list
                    SaveManagerAccounts(); // e save ang updated list sa file
                    Console.WriteLine("Account deleted successfully!"); // yes na delete najud
                }
                else
                {
                    Console.WriteLine("Incorrect answer to security question."); // if ang answer sa security question kay wrong
                }
            }
            else
            {
                Console.WriteLine("Account not found."); // kung wala nakit an
            }
            Console.ReadKey(); // Wait for a key press before closing
        }

        private void ForgotPasscode()//forgot passcode method
        {
            Console.Clear(); 
            Console.Write("Enter your username: "); //enter the username.
            string username = Console.ReadLine();

            // pangitaon if sakto, dapat parehas ang username sa file ug sa input
            var managerAccount = managerAccounts.FirstOrDefault(m => m.Username == username);

            if (managerAccount != null) // Check if exists ang acc
            {
                // answeri ang question to verify the user's identity.
                Console.WriteLine($"Security question: {managerAccount.SecurityQuestion}");
                Console.Write("Answer: "); 
                string securityAnswer = Console.ReadLine(); 

                if (securityAnswer == managerAccount.SecurityAnswer) // Check if same ba sa nasave sa file
                {
                    Console.Write("Enter your new password: "); // new password.
                    string newPassword = Console.ReadLine(); // ge Store ang new password.
                    managerAccount.Password = newPassword; // ge Update account's password.

                    SaveManagerAccounts(); // Save the updated one sa file.

                    Console.WriteLine("Password reset successful!"); // Confiremed!!!
                }
                else
                {
                    Console.WriteLine("Incorrect answer to security question."); //if wrong 
                }
            }
            else
            {
                Console.WriteLine("Account not found."); // if wala nag exist
            }
            Console.ReadKey(); 
        }


        //MANAGER MAIN MENU
        private void ShowManagerMainMenu()//manager main menu method
        {
            bool inMainMenu = true; // Keep track if user still in the main menu

            while (inMainMenu) // repeat until mo log out or mo exit
            {
                Console.Clear(); 

                // Display manager menu 
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Manager Menu:[/]") 
                        .AddChoices("Manage Stocks", "View Sales", "Account Options", "Log Out", "Exit") // Menu options.
                        .HighlightStyle(new Style(Color.Yellow))); 

                switch (choice) // Handle user's choice.
                {
                    case "Manage Stocks":
                        ManageStocks(); // Call method  manage stocks
                        break;

                    case "View Sales":
                        ViewSales(); // Call method view sales
                        break;

                    case "Account Options":
                        ShowAccountOptions(); // Call method manage account option
                        break;

                    case "Log Out":
                        inMainMenu = false; // Exit menu loop, log out
                        break;

                    case "Exit":
                        Environment.Exit(0); // exit ang program
                        break;
                }
            }
        }

        private void ManageStocks() //manage stocks method
        {
            while (true) // e Loop ni siya hangtud mo exit ang user
            {
                string branch = SelectBranch(); 

                if (branch == null) break; //kung wala ni select ug branch, mo exit sa main menu

                Stock[] stocks = Stock.LoadStockFromFile(branch); //  e load ang stock data para sa branch

                if (stocks.Length == 0) //check if wala stock data nga nakit an sa file
                {
                    // Initialize default stock data with zero quantities if none exist. 
                    stocks = new Stock[]
                    {
                new Stock("Diesel", 50.30m, 0), 
                new Stock("Regular", 56.10m, 0), 
                new Stock("Premium", 55.10m, 0) 
                    };
                }

                ManageStocksForBranch(stocks, branch); // Manage the stocks for the selected branch.
            }
        }

        private string SelectBranch() //select branch method
        {
            while (true) 
            {
                Console.Clear(); 

                //  branch selection menu 
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold orange1]CHOOSE BRANCH:[/]") 
                        .AddChoices("Toledo", "Mandaue", "Cebu Proper", "Back to Main Menu") // Branch options.
                        .HighlightStyle(new Style(Color.Orange1))); 

                switch (choice) 
                {
                    case "Toledo":
                        return "Toledo"; // Return "Toledo" 

                    case "Mandaue":
                        return "Mandaue"; // Return "Mandaue" 

                    case "Cebu Proper":
                        return "CebuProper"; // Return "CebuProper" 

                    case "Back to Main Menu":
                        return null; //Return `null` to go back to the main menu
                }
            }
        }

        private void ManageStocksForBranch(Stock[] stocks, string branch) //manage stocks for branch method
        {
            bool inManageStocksMenu = true; 

            while (inManageStocksMenu) 
            {
                Console.Clear(); 

                // Manage Stocks menu 
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold red]Manage Stocks for {branch}:[/]") 
                        .AddChoices("Manage Diesel Stock", "Manage Regular Stock", "Manage Premium Stock", "Back to Branch Menu") // Options 
                        .HighlightStyle(new Style(Color.Red))); 

                switch (choice) 
                {
                    case "Manage Diesel Stock":
                        stocks[0].EditStock(stocks, branch); // Call method to edit Dieselstock
                        break;

                    case "Manage Regular Stock":
                        stocks[1].EditStock(stocks, branch); // Call method to edit Regularstock
                        break;

                    case "Manage Premium Stock":
                        stocks[2].EditStock(stocks, branch); // Call method to edit Premiumstock
                        break;

                    case "Back to Branch Menu":
                        inManageStocksMenu = false; // Exit
                        break;
                }
            }
        }




        //View sales
        private void ViewSales() //view sales method
        {
            // Create Sales objects for each fuel type - Toledo 
            var toledoDieselSales = new Sales(new Stock("Diesel", 50.30m, 0), "Toledo");
            var toledoRegularSales = new Sales(new Stock("Regular", 56.10m, 0), "Toledo");
            var toledoPremiumSales = new Sales(new Stock("Premium", 55.10m, 0), "Toledo");

            // Create Sales objects for each fuel type - Mandaue 
            var mandaueDieselSales = new Sales(new Stock("Diesel", 50.30m, 0), "Mandaue");
            var mandaueRegularSales = new Sales(new Stock("Regular", 56.10m, 0), "Mandaue");
            var mandauePremiumSales = new Sales(new Stock("Premium", 55.10m, 0), "Mandaue");

            // Create Sales objects for each fuel type - Cebu Proper 
            var cebuDieselSales = new Sales(new Stock("Diesel", 50.30m, 0), "CebuProper");
            var cebuRegularSales = new Sales(new Stock("Regular", 56.10m, 0), "CebuProper");
            var cebuPremiumSales = new Sales(new Stock("Premium", 55.10m, 0), "CebuProper");

            bool inSalesMenu = true; 

            while (inSalesMenu) // Loop until ang user mo exit
            {
                Console.Clear(); 

                // menu viewing sales 
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Select the fuel type to view sales for a specific month (past 6 months):[/]") 
                        .AddChoices("Diesel", "Regular", "Premium", "Back to Menu") // Options fuel types ug exit
                        .HighlightStyle(new Style(Color.Yellow))); 

                switch (choice) 
                {
                    case "Diesel":
                        // Display sales for Diesel - all branches
                        PromptMonthAndDisplaySales(toledoDieselSales, mandaueDieselSales, cebuDieselSales);
                        break;

                    case "Regular":
                        // Display sales for Regular - all branches
                        PromptMonthAndDisplaySales(toledoRegularSales, mandaueRegularSales, cebuRegularSales);
                        break;

                    case "Premium":
                        // Display sales for Premium - all branches
                        PromptMonthAndDisplaySales(toledoPremiumSales, mandauePremiumSales, cebuPremiumSales);
                        break;

                    case "Back to Menu":
                        inSalesMenu = false; // Exit
                        break;
                }
            }
        }

        private void ViewFuelSales(Sales toledoSales, Sales mandaueSales, Sales cebuSales) //view fuel sales method
        {
            Console.Clear(); 

            // Get totsales for each braanch
            decimal toledoTotalSales = toledoSales.GetTotalSales();
            decimal mandaueTotalSales = mandaueSales.GetTotalSales();
            decimal cebuTotalSales = cebuSales.GetTotalSales();

            // Calc totsales of all branches combined
            decimal totalBranchSales = toledoTotalSales + mandaueTotalSales + cebuTotalSales;

            // Calc sales percentage for each branch
            double toledoPercentage = totalBranchSales > 0 ? (double)(toledoTotalSales / totalBranchSales * 100) : 0;
            double mandauePercentage = totalBranchSales > 0 ? (double)(mandaueTotalSales / totalBranchSales * 100) : 0;
            double cebuPercentage = totalBranchSales > 0 ? (double)(cebuTotalSales / totalBranchSales * 100) : 0;

            //bar chart displays sales percentages
            var barChart = new Spectre.Console.BarChart()
                .Width(60) 
                .Label("[bold underline]Fuel Sales by Branch (%) - Last 6 Months[/]") 
                .AddItem("Toledo", toledoPercentage, Spectre.Console.Color.Blue) 
                .AddItem("Mandaue", mandauePercentage, Spectre.Console.Color.Green) 
                .AddItem("Cebu Proper", cebuPercentage, Spectre.Console.Color.Red); 

            Spectre.Console.AnsiConsole.Write(barChart); 

            //highest sales percentage 
            var highestSales = new[]
            {
        new { Branch = "Toledo", Percentage = toledoPercentage },
        new { Branch = "Mandaue", Percentage = mandauePercentage },
        new { Branch = "Cebu Proper", Percentage = cebuPercentage }
    }
            .OrderByDescending(s => s.Percentage) 
            .First(); 

           
            Spectre.Console.AnsiConsole.MarkupLine($"\n[bold yellow]Branch with Highest Sales:[/] [green]{highestSales.Branch}[/] - {highestSales.Percentage:F2}%");

            // Show detailed monthly sales 
            Console.WriteLine();
            DisplayBranchSales("Toledo", toledoSales); 
            DisplayBranchSales("Mandaue", mandaueSales); 
            DisplayBranchSales("Cebu Proper", cebuSales); 

            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(); 
        }

        private void DisplayBranchSales(string branchName, Sales sales) //display branch sales method
        {
            
            Spectre.Console.AnsiConsole.MarkupLine($"\n[bold yellow]{branchName} Branch[/]");

            // Display detailed sales records 
            sales.DisplaySalesRecords();

            
            var summaryTable = new Spectre.Console.Table()
                .Border(Spectre.Console.TableBorder.Rounded) 
                .AddColumn("[bold blue]Month[/]") 
                .AddColumn("[bold blue]Quantity Sold[/]") 
                .AddColumn("[bold blue]Sales Value[/]"); 

            //e get ang sales data from the -6 months.
            var monthlySales = sales.GetMonthlySales()
                .Where(m => DateTime.Parse(m.Key).AddMonths(6) >= DateTime.Now) // last six months.
                .OrderByDescending(m => m.Key); // recent month.

            decimal totalValue = 0; // Track totsales value
            int totalQuantity = 0; // Track totquant sold

            // eadd each month's sales data sa table.
            foreach (var month in monthlySales)
            {
                summaryTable.AddRow(
                    month.Key, 
                    month.Value.Quantity.ToString(), 
                    $"Php{month.Value.Total:F2}" 
                );
                totalValue += month.Value.Total; // Accumulate totsales value
                totalQuantity += month.Value.Quantity; // Accumulate totquant sold
            }

            
            summaryTable.AddRow(
                "[bold yellow]TOTAL[/]", 
                $"[bold yellow]{totalQuantity}[/]", 
                $"[bold yellow]Php{totalValue:F2}[/]" 
            );

            
            Spectre.Console.AnsiConsole.Write(summaryTable);
        }

        private void PromptMonthAndDisplaySales(Sales toledoSales, Sales mandaueSales, Sales cebuSales) //prompt month and display sales method
        {
            // Get the last 6 months
            var now = DateTime.Now;
            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(offset => now.AddMonths(-offset).ToString("yyyy-MM"))
                .ToList();

            
            var selectedMonth = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Select a month to view sales:[/]") 
                    .AddChoices(lastSixMonths) //last 6 months options
                    .HighlightStyle(new Style(Color.Green))); 

            // Get sales data for selected month - each branch
            var toledoData = FilterSalesForMonth(toledoSales, selectedMonth);
            var mandaueData = FilterSalesForMonth(mandaueSales, selectedMonth);
            var cebuData = FilterSalesForMonth(cebuSales, selectedMonth);

            // Display sales table - each branch
            DisplayBranchSalesTable("Toledo", toledoData);
            DisplayBranchSalesTable("Mandaue", mandaueData);
            DisplayBranchSalesTable("Cebu Proper", cebuData);

            // Calc the totsales all branches
            decimal totalSales = toledoData.Sum(kvp => kvp.Value.Total) +
                                 mandaueData.Sum(kvp => kvp.Value.Total) +
                                 cebuData.Sum(kvp => kvp.Value.Total);

            // Show bar chart if naay sale sales, ifwala  prints "no sales"
            if (totalSales > 0)
            {
                DisplaySalesBarChartForMonth(toledoData, mandaueData, cebuData, totalSales, selectedMonth);
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold red]No sales recorded for {selectedMonth}.[/]");
            }

            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        private Dictionary<string, (int Quantity, decimal Total)> FilterSalesForMonth(Sales sales, string month) //filter sales for month method
        {
            // Keeps only sales data for the chosen month.
            return sales.GetMonthlySales()
                .Where(kvp => kvp.Key == month) 
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Convert the filtered results back to a dictionary.
        }


        private void DisplayBranchSalesTable(string branchName, Dictionary<string, (int Quantity, decimal Total)> salesData) //display branch sales table method
        {
            
            var table = new Spectre.Console.Table()
                .Border(Spectre.Console.TableBorder.Rounded) 
                .AddColumn("[bold blue]Month[/]") 
                .AddColumn("[bold green]Quantity Sold[/]") 
                .AddColumn("[bold yellow]Total Sales (Php)[/]"); 

            
            foreach (var entry in salesData)
            {
                table.AddRow(
                    entry.Key, 
                    entry.Value.Quantity.ToString(), 
                    $"Php{entry.Value.Total:F2}" 
                );
            }

            
            AnsiConsole.MarkupLine($"\n[bold yellow]{branchName} Branch Sales:[/]");

            
            AnsiConsole.Write(table);
        }


        private void DisplaySalesBarChartForMonth( //display sales bar chart for month method
            //parameters
         Dictionary<string, (int Quantity, decimal Total)> toledoData, 
         Dictionary<string, (int Quantity, decimal Total)> mandaueData, 
         Dictionary<string, (int Quantity, decimal Total)> cebuData, 
         decimal totalSales, 
         string month 
         )
                {
                    // Calc totsales 
                    decimal toledoTotal = toledoData.Sum(kvp => kvp.Value.Total);
                    decimal mandaueTotal = mandaueData.Sum(kvp => kvp.Value.Total);
                    decimal cebuTotal = cebuData.Sum(kvp => kvp.Value.Total);

            
                    var barChart = new Spectre.Console.BarChart()
                        .Width(60) 
                        .Label($"[bold underline]Sales Distribution by Branch for {month}[/]") 
                        .AddItem("Toledo", (double)(toledoTotal / totalSales * 100), Spectre.Console.Color.Blue) 
                        .AddItem("Mandaue", (double)(mandaueTotal / totalSales * 100), Spectre.Console.Color.Green) 
                        .AddItem("Cebu Proper", (double)(cebuTotal / totalSales * 100), Spectre.Console.Color.Red); 

            
                    AnsiConsole.Write(barChart);
                }
      
        private class ManagerAccount //manager account class
        {
            // Properties sa manager account

            // Stores username (read-only after being set in the constructor)
            public string Username { get; }

            // Stores pass (can be changed after creation)
            public string Password { get; set; }

            // Stores security question (read-only after being set in the constructor)
            public string SecurityQuestion { get; }

            // Stores answer to the security question (read-only after being set in the constructor)
            public string SecurityAnswer { get; }

            // Constructor to make a ManagerAccount w/given details
            public ManagerAccount(string username, string password, string securityQuestion, string securityAnswer)
            {
                Username = username; // Set username
                Password = password; // Set password
                SecurityQuestion = securityQuestion; // Set security question
                SecurityAnswer = securityAnswer; // Set security answer
            }
        }

    }

    //CLASS STOCK
    public class Stock
    {
        // Property to store the type of fuel
        public string FuelType { get; set; }

        // Property to store the price 
        public decimal PricePerLiter { get; set; }

        // Property to store the quantity 
        public int Quantity { get; set; }


        public Stock(string fuelType, decimal pricePerLiter, int quantity) // Constructor
        {
            // Assign the fuel type to the FuelType property
            FuelType = fuelType;

            // Assign the price per liter to the PricePerLiter property
            PricePerLiter = pricePerLiter;

            // Assign the quantity to the Quantity property
            Quantity = quantity;
        }


        public void EditStock(Stock[] allStocks, string branch) // Edit stock method
        {
            while (true) 
            {
                Console.Clear();

                //  menu options 
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold yellow]Editing [cyan]{FuelType}[/] Stock for [green]{branch}[/]:[/]") 
                        .PageSize(4) 
                        .HighlightStyle(new Style(Color.Yellow)) 
                        .AddChoices(
                            "View Stock",         
                            "Add Stock",          
                            "Delete Stock",       
                            "Back to Manage Stocks Menu" 
                        )
                );

                
                switch (choice)
                {
                    case "View Stock":
                        // Call method view the current stock
                        ViewStock();
                        AnsiConsole.MarkupLine("[bold yellow]Press any key to return to the menu...[/]");
                        Console.ReadKey(); 
                        break;

                    case "Add Stock":
                        
                        Console.Write("Enter liters to add: ");
                        if (int.TryParse(Console.ReadLine(), out int litersToAdd) && litersToAdd > 0) // Validate input
                        {
                            AddStock(litersToAdd); // Add specified stock
                            Stock.SaveStockToFile(branch, allStocks); // Save updated stock to a file
                            AnsiConsole.MarkupLine($"[green]{litersToAdd} liters added successfully![/]");
                        }
                        else
                        {
                            
                            AnsiConsole.MarkupLine("[red]Invalid input. Please enter a positive number.[/]");
                        }
                        AnsiConsole.MarkupLine("[bold yellow]Press any key to return to the menu...[/]");
                        Console.ReadKey(); 
                        break;

                    case "Delete Stock":
                        
                        Console.Write("Enter liters to delete: ");
                        if (int.TryParse(Console.ReadLine(), out int litersToRemove) && litersToRemove > 0) // Validate input
                        {
                            RemoveStock(litersToRemove); 
                            Stock.SaveStockToFile(branch, allStocks); 
                            AnsiConsole.MarkupLine($"[green]{litersToRemove} liters deleted successfully![/]");
                        }
                        else
                        {
                            
                            AnsiConsole.MarkupLine("[red]Invalid input. Please enter a positive number.[/]");
                        }
                        AnsiConsole.MarkupLine("[bold yellow]Press any key to return to the menu...[/]");
                        Console.ReadKey(); 
                        break;

                    case "Back to Manage Stocks Menu":
                        
                        return;

                    default:
                        // This case won't happen because of controlled choices, but it's a safety check.
                        AnsiConsole.MarkupLine("[red]Invalid selection. Try again.[/]");
                        break;
                }
            }
        }


        public void AddStock(int liters) // Add stock method
        {
            
            Quantity += liters;

            
            Console.WriteLine($"{liters} liters added to {FuelType} stock.");
        }


        public void RemoveStock(int liters) // Remove stock method
        {
            if (liters <= Quantity)
            {
                Quantity -= liters;
                Console.WriteLine($"{liters} liters removed from {FuelType} stock.");
            }
            else
            {
                Console.WriteLine("Not enough stock to remove that quantity.");
            }
        }

        public void ViewStock() // View stock method
        {
            Console.WriteLine($"{FuelType}: {Quantity} liters available at P{PricePerLiter} per liter.");
        }

        public static void SaveStockToFile(string branch, Stock[] stocks) // Save stock to file method
        {
            // Define's file path using branch name
            string filePath = $"{branch}_stocks.txt";

            try
            {
                
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    
                    foreach (var stock in stocks)
                    {
                       
                        writer.WriteLine($"{stock.FuelType},{stock.PricePerLiter},{stock.Quantity}");
                    }
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error saving stock file: {ex.Message}");
            }
        }


        public static Stock[] LoadStockFromFile(string branch) // Load stock from file method
        {
            
            string filePath = $"{branch}_stocks.txt";

            try
            {
                // Check if the file exist
                if (!File.Exists(filePath))
                {
                    // If the file doesn't exist 
                    Console.WriteLine($"Stock file for {branch} not found. Returning empty stock.");
                    return new Stock[0]; // Empty array
                }

                // Read all lines from file into a string array
                var lines = File.ReadAllLines(filePath);

                // Convert each line into Stock object and return as array
                return lines.Select(line =>
                {
                    
                    var parts = line.Split(',');

                    
                    return new Stock(parts[0], decimal.Parse(parts[1]), int.Parse(parts[2]));
                }).ToArray(); // Convert's result to an array
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error loading stock file: {ex.Message}");

                
                return new Stock[0];
            }
        }

    }

    //CLASS SALES
    public class Sales
    {
        // Read-only file path to save or read sales data (cannot be changed after setup)
        private readonly string salesFilePath;

        // Property to get the Stock object for this instance
        public Stock Stock { get; }


        public Sales(Stock stock, string branch) // Constructor
        {
            // Assign the provided Stock object toStock properrt
            Stock = stock;

            // Construc file path for sales data 
            salesFilePath = $"{branch}_{stock.FuelType}_sales.txt";
        }

        public void RecordSale(int liters, decimal pricePerLiter) // Record sale method
        {
            // Check if stock is enough 
            if (liters > Stock.Quantity)
            {
                // if there is not enough fuel
                Console.WriteLine($"Not enough {Stock.FuelType} in stock for the sale.");
                return; 
            }

            // Calc totsale amount
            decimal totalSale = liters * pricePerLiter;

            // date and time for the sale 
            DateTime purchaseDate = DateTime.Now;

            try
            {
                // Create record sale 
                string record = $"{liters}|{totalSale}|{purchaseDate:yyyy-MM-dd HH:mm:ss}";

                // Save the sale record
                File.AppendAllText(salesFilePath, record + Environment.NewLine);

                // Reduce stock by sold liters
                Stock.Quantity -= liters;

                // Update the stock file
                UpdateStockFile();

                
                Console.WriteLine($"Sale recorded: {liters} liters of {Stock.FuelType} at Php{pricePerLiter:F2} per liter");
                Console.WriteLine($"Total: Php{totalSale:F2}");
                Console.WriteLine($"Date: {purchaseDate:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error saving sale: {ex.Message}");
            }
        }


        private void UpdateStockFile() // Update stock file method
        {
            try
            {
                // Get the branch path by removing the file name
                string branch = salesFilePath.Replace($"{Stock.FuelType}_sales.txt", "");

                // Load all stock data for the branch
                Stock[] branchStocks = Stock.LoadStockFromFile(branch);

                // Loop through stock until it finds the match fuel
                foreach (var stock in branchStocks)
                {
                    if (stock.FuelType == Stock.FuelType) // Check correct fue
                    {
                        stock.Quantity = Stock.Quantity; // Update the quantity 
                        break; 
                    }
                }

                // Save the updated stock data to the file.
                Stock.SaveStockToFile(branch, branchStocks);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error updating stock file: {ex.Message}");
            }
        }


        public decimal GetTotalSales() // Method to calculate total sales
        {
            // if file exist
            if (!File.Exists(salesFilePath))
            {
                //if file wala
                Console.WriteLine($"Sales file not found: {salesFilePath}");
                return 0; // Return 0 as no sales
            }

            try
            {
                // Reads all lines from file
                var salesRecords = File.ReadAllLines(salesFilePath);

                // Process: calc  tot sales return sum
                return salesRecords
                    .Select(line => line.Split('|')) 
                    .Where(parts => parts.Length == 3) // Only process valid records with 3 parts
                    .Sum(parts =>
                    {
                        decimal sale = decimal.Parse(parts[1]); // Convert the second part to a sale amount
                        return sale; // Add this sale to the total
                    });
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error reading sales file: {ex.Message}");
                return 0; 
            }
        }



        public Dictionary<string, (int Quantity, decimal Total)> GetMonthlySales() // Get sales grouped by month
        {
            // Dictionary to store sales by month (key: month, value: tot quantity and sales)
            var salesByMonth = new Dictionary<string, (int Quantity, decimal Total)>();

            // if exist
            if (!File.Exists(salesFilePath))
            {
                
                Console.WriteLine($"Sales file not found: {salesFilePath}");
                return salesByMonth; 
            }

            try
            {
               
                var salesRecords = File.ReadAllLines(salesFilePath);

                foreach (var line in salesRecords)
                {
                    
                    var parts = line.Split('|');

                    // Check record is valid (has at least 3 parts)
                    if (parts.Length < 3)
                    {
                        
                        Console.WriteLine($"Invalid record: {line}");
                        continue; 
                    }

                    // Parse date of the sale
                    DateTime saleDate = DateTime.Parse(parts[2]);

                    
                    string month = saleDate.ToString("yyyy-MM");

                    // Parse qnty sold and the total sale amount
                    int litersSold = int.Parse(parts[0]);
                    decimal totalSale = decimal.Parse(parts[1]);

                    // Check month already in dictionary
                    if (salesByMonth.ContainsKey(month))
                    {
                        // Update qnty and tot for this month
                        salesByMonth[month] = (
                            salesByMonth[month].Quantity + litersSold, 
                            salesByMonth[month].Total + totalSale
                        );
                    }
                    else
                    {
                        // Add a new entry for the current month's sale.
                        salesByMonth[month] = (litersSold, totalSale);
                    }
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error parsing monthly sales: {ex.Message}");
            }

            // Return sales grouped by month in dictionary.
            return salesByMonth;
        }



        public void DisplaySalesRecords() // Display sales records method
        {
            
            Console.WriteLine($"Sales Records for {Stock.FuelType.ToUpper()}:");
            Console.WriteLine(new string('-', 80)); // Add a separator line

            //if file exist
            if (!File.Exists(salesFilePath))
            {
                
                Console.WriteLine("No sales records available.");
                return; 
            }

           
            foreach (var line in File.ReadAllLines(salesFilePath))
            {
                
                var parts = line.Split('|');

                // Display sale: liters, total, and date
                Console.WriteLine($"Liters: {parts[0]}, Total: Php{parts[1]}, Date: {parts[2]}");
            }

            
            Console.WriteLine(new string('-', 80));
        }


        private decimal GetSalesForMonth(Dictionary<string, (int Quantity, decimal Total)> monthlySales, string monthString) // Get sales for a specific month
        {
            // Filter the sales dictionary for entries that match the given month
            return monthlySales
                .Where(entry => entry.Key.EndsWith($"-{monthString}")) //if month match
                .Sum(entry => entry.Value.Total); //sum of sales sa matching month
        }



    }

    //CLASS CONSUMER INHERITS FROM USER
    public class Consumer : User
    {
        public override void ShowMenu() // Show menu method
        {
            bool continueShopping = true; 

            while (continueShopping) 
            {
                Console.Clear(); 

                
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Consumer Menu:[/]") 
                        .AddChoices("Purchase Fuel", "Exit to Main Menu") // Menu options
                        .HighlightStyle(new Style(Color.Yellow)) 
                );

                
                switch (choice)
                {
                    case "Purchase Fuel":
                        PurchaseFuel(); // Call method purchase fuel
                        break;

                    case "Exit to Main Menu":
                        continueShopping = false; 
                        break;

                    default:
                       
                        AnsiConsole.MarkupLine("[bold red]Invalid selection. Please try again.[/]");
                        break;
                }
            }
        }

        private void PurchaseFuel() // Purchase fuel method
        {
            
            string branch = SelectBranch();

            
            if (branch == null)
                return;

            // Load fuel stocks and sales data for the chosen branch.
            Stock[] stocks = Stock.LoadStockFromFile(branch); // Load stock data
            Sales[] sales = stocks.Select(stock => new Sales(stock, branch)).ToArray(); 

            // Check if no stocks available
            if (stocks.Length == 0)
            {
                
                AnsiConsole.MarkupLine("[red]No fuel stocks available in this branch. Please contact the manager.[/]");
                AnsiConsole.MarkupLine("[yellow]Press any key to return to the menu...[/]");
                Console.ReadKey(); 
                return; 
            }

            
            while (true)
            {
                Console.Clear(); 

               
                var prompt = new SelectionPrompt<string>()
                    .Title($"[bold yellow]Purchase Fuel - {branch} Branch[/]") 
                    .PageSize(5) 
                    .HighlightStyle(new Style(Color.Yellow)) 
                    .AddChoices(
                        stocks.Select((stock, i) =>
                            $"{stock.FuelType} - Php{stock.PricePerLiter:F2} per liter") 
                        .ToList()
                        .Concat(new[] { "Back to Consumer Menu" }) 
                    );

                
                string selectedChoice = AnsiConsole.Prompt(prompt);

                
                if (selectedChoice == "Back to Consumer Menu")
                    return;

                // Find the selected fuel by matching it with the fuel type.
                int fuelIndex = Array.FindIndex(stocks, stock =>
                    selectedChoice.StartsWith(stock.FuelType));

                // If a valid fuel is selected, proceed purchase.
                if (fuelIndex != -1)
                {
                    // Give the stock and branch data to the purchase method
                    ProcessFuelPurchase(sales[fuelIndex], stocks[fuelIndex], stocks, branch);
                }
                else
                {
                    
                    AnsiConsole.MarkupLine("[red]Invalid selection. Please try again.[/]");
                    Console.ReadKey(); 
                }
            }
        }

        private void ProcessFuelPurchase(Sales selectedSales, Stock selectedStock, Stock[] stocks, string branch)
        {
            Console.Clear();

            Console.Write($"Enter the number of liters to purchase for {selectedStock.FuelType}: ");
            if (!int.TryParse(Console.ReadLine(), out int liters) || liters <= 0)
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a positive number.[/]");
                Console.ReadKey();
                return;
            }

            if (liters > selectedStock.Quantity)
            {
                AnsiConsole.MarkupLine("[red]Insufficient stock available.[/]");
                Console.ReadKey();
                return;
            }

            decimal totalCost = liters * selectedStock.PricePerLiter;
            Console.Write($"Total cost is Php{totalCost:F2}. Enter payment amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal payment) || payment < totalCost)
            {
                AnsiConsole.MarkupLine("[red]Kuwang imong kwarta choy!. Transaction canceled.[/]");
                Console.ReadKey();
                return;
            }

            // Deduct purchased quantity from stock
            selectedStock.Quantity -= liters;

            // Save the updated stock to the file
            Stock.SaveStockToFile(branch, stocks);

            // Record the sale
            selectedSales.RecordSale(liters, selectedStock.PricePerLiter);

            // Prepare the receipt content
            string receipt = $@"
      [bold orange3]Purchase Receipt:[/]
      [orange3]Branch:[/] {branch}
      [orange3]Fuel Type:[/] {selectedStock.FuelType}
      [orange3]Liters Purchased:[/] {liters}
      [orange3]Price per Liter:[/] Php{selectedStock.PricePerLiter:F2}
      [orange3]Total Cost:[/] Php{totalCost:F2}
      [orange3]Payment:[/] Php{payment:F2}
      [orange3]Change:[/] Php{payment - totalCost:F2}

      [bold orange3]Thank you for your purchase![/]";

            // Create and render the receipt panel
            var receiptPanel = new Panel(receipt)
                .Header("[yellow]Transaction Complete[/]", Justify.Left)
                .BorderColor(Color.Orange3);

            // Render the panel
            AnsiConsole.Clear();
            AnsiConsole.Write(receiptPanel);

            // Message to exit
            AnsiConsole.MarkupLine("\n[bold yellow]Press any key to exit the program...[/]");
            Console.ReadKey();

            // Exit the application
            Environment.Exit(0);
        }
        private string SelectBranch() // Select branch method
        {
          
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Select Branch:[/]") 
                    .AddChoices("Toledo", "Mandaue", "Cebu Proper", "Back to Consumer Menu") // Branch options
                    .HighlightStyle(new Style(Color.Orange1)) 
            );

            
            return choice switch
            {
                "Toledo" => "Toledo", 
                "Mandaue" => "Mandaue", 
                "Cebu Proper" => "CebuProper", 
                "Back to Consumer Menu" => null, 
                _ => throw new InvalidOperationException("Unexpected choice") 
            };
        }


    }


}