# What is this?
This is a Windows Forms application that gives partial information from a GAAP-based income statement and asks the user to solve for other related accounts. Information is currently only displayed in a list, but an income statement version may come soon.

This application is based around the Texas UIL Accounting exams which occasionally give a question group asking for contestants to solve for the remaining accounts given partial account information. 

Additionally, income statement information is based on a 3-column traditional income statement. This is not to be confused with a horizontal analysis income statement. 

# Formulas used
Since there are various disagreeing sources on what Cost of Transported Goods (Cost of Delivered Merchandise) actually is, this is a list of what *this program* uses.
* Cost Of Delivered Merchandise = Purchases + Transportation In
* Net Purchases = Cost Of DeliveredMerchandise - Purchases Returns And Allowances - Purchases Discounts
* Net Sales = Sales - Sales Discounts - Sales Returns And Allowances
* Cost Of Merchandise Available For Sale = Beginning Inventory + Net Purchases
* Cost Of Merchandise Sold = Cost Of Merchandise Available For Sale - Ending Inventory
* Gross Profit = Net Sales - Cost Of Merchandise Sold

# How it works

1. The initialization of the variables starts in the `InitializeAccounts` method, which generates random values for each variable.
2.  The program then moves on to initializing the *solve state* of each account in random order. This is done using the `SetSolveStatesAlternative` method. 
3.  `SetSolveStates` checks the list of substitutes to see if the variable can be set to a *needs solving state* by using the `IsSolvable` method for the `AccountVal` object.
4.  `IsSolvable` returns true if the variable has already been set to a *given state* or if the variable has a usable substitute using `UsableSubstitute`.
5.  `UsableSubstitute` checks if the variable's substitutes are solvable using `IsSolvableRecursive`, which does the same thing as `UsableSubstitute` but it checks whether the current variable is already in a *given state*.
6.  Thus, the program returns to `SetSolveStates` using the return value of `IsSolvable`, and if the variable "is solvable" then the method sets the variable to a *needs solving state*.

# Example account values

Based on the 2018-District UIL Accounting test, the following is an example of the accounts you would be required to solve for:

Cost of delivered merchandise             64,295
Cost of merchandise available for sale	  81,112
Gross profit	                            40,488
Net purchases	                            58,612
Net sales	                                96,400
Purchases	                                  ?
Purchases discounts	                        ?
Purchases returns and allowances	         3,487
Sales	                                   101,810
Sales discounts	                            ?
Sales returns and allowances	             3,270
Transportation in	                         4,288

## Terminology
**Given state** - The value of the variable will be given in the generated question set. A *solve state*.

**Needs solving state** - The value of the variable will not be given in the generated question set. The variable will instead be asked for in a question. A *solve state*.

**Solve state** - Represented with a boolean; whether a variable will be given \[true\] (i.e. does not need to be solved for) in the generated question set, or needs to be solved for \[false\].

**Substitute group** - A group of variables that can be used to substitute a different, single variable's value. See substitution for system of equations for more information.
