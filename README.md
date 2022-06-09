# What is this?
This is a Windows Forms application that gives partial information from a GAAP-based income statement and asks the user to solve for other related accounts. Information is currently only displayed in a list, but an income statement version may come soon.

This application is based around the Texas UIL Accounting exams which occasionally give a question group asking for contestants to solve for the remaining accounts given partial account information. 

Additionally, income statement information is based on a textbook's practice and mastery problems.

# Formulas used
Since there are various disagreeing sources on what Cost of Transported Goods (Cost of Delivered Merchandise) actually is, this is a list of what *this program* uses.
* Cost Of Delivered Merchandise = Purchases + Transportation In;
* Net Purchases = Cost Of DeliveredMerchandise - Purchases Returns And Allowances - Purchases Discounts
* Net Sales = Sales - Sales Discounts - Sales Returns And Allowances
* Cost Of Merchandise Available For Sale = Beginning Inventory + Net Purchases
* Cost Of Merchandise Sold = Cost Of Merchandise Available For Sale - Ending Inventory
* Gross Profit = Net Sales - Cost Of Merchandise Sold

This is also found in the code.

# How it works
The main bulk of the algorithm comes from a function `setSolveStates`. `setSolveStates` is a recursive function that uses its return values of a bool. A value of `true` means the variable needs solving under the current context. A value of `false` means the variable does not need to be solved (i.e. the initial problem statement will tell the user the value of the variable).

The steps of the algorithm are as follows:
1. Check if the *solve state* is already set.
2. If it is, then the function returns the *solve state*.
4. If it does not, then it iteratively runs through each *substitute group* for the variable.
5. Each substitute in the group is set using `setSolveStates` and the return value is checked.
6. If any of the substitutes in a group returns a `false`, then that *substitute group* is not usable to solve the current variable and the loop moves to the next group.
7. If there are no usable substitutes, then the current variable is set to a *given state*.
8. If there is a usable substitute, then the current variable is set to a *needs solving state*.

This can be used for non-accounting problems, but this specific implementation uses it for accounting. Additionally, this algorithm uses a variable specific to the variable object for the account values, a boolean called `visiting`. This prevents an infinite recursive call of `setSolveStates` since a a variable of a *substitute group* will be a potential substitue for those in the group.

There is another implementation which only checks if a variable is solvable under the current context, `IsSolvableRecursive`. This uses a similar algorithm, but does not change any values. This uses a separate boolean called `_isSolvableVisiting` which is necessary for the same reasons as above. This method is used in the event that `NeedsSolving` and `SolvingSet` are not used. This was condensed into a LINQ expression, but the main algorithm remains the same:
1. Check if this variable is in a *given state*
2. If it is, return `true`
3. If it is not, then check each *substitute group*.
4. Check each substitue in the group using `IsSolvable`.
5. If one of them returns `false`, then check the next group.
6. If the entire group has viable substitutes, then the method returns `true`.
7. Otherwise if it checks all substitutes without any working, then the method returns `false`.

Alternatively, there is the normal `IsSolvable` which uses a LINQ expression to check if there exists a viable substitute.

There is also a method which checks whether the variable has a usable substitute, `UsableSubstitute`. It is identical to `IsSolvableRecursive` except it does not check its own *solve state*.

## Terminology
**Given state** - The value of the variable will be given in the generated question set. A *solve state*.

**Needs solving state** - The value of the variable will not be given in the generated question set. The variable will instead be asked for in a question. A *solve state*.

**Solve state** - Represented with a boolean; whether a variable will be given \[true\] (i.e. does not need to be solved for) in the generated question set, or needs to be solved for \[false\].

**Substitute group** - A group of variables that can be used to substitute a different, single variable's value. See substitution for system of equations for more information.
