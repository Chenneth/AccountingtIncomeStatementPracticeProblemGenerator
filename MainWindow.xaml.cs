using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcctISGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //changed to a class from struct because we want shallow copies
        public class AccountVal
        {
            public int amount;
            public bool visiting, //this value is used mainly when setting needsSolving
                needsSolving, //as in, this value was not given in the initial problem
                solvingSet;     //this value is used when setting needsSolving


            public List<AccountVal[]> substitutes;
            public string Name { get; set; }

            public AccountVal()
            {
                Name = "Not set";
                substitutes = new List<AccountVal[]>(3);
                amount = 0;
                solvingSet =  needsSolving = false;
            }
            public AccountVal(int amount)
            {
                Name = "Not set";
                substitutes = new List<AccountVal[]>(3);//did 3 because i am lazy
                this.amount = amount;
                solvingSet = needsSolving = false; //defaulting
                
            }
            public AccountVal(int amount, bool needsSolving)
            {
                Name = "Not set";
                substitutes = new List<AccountVal[]>(3);
                this.amount = amount;
                solvingSet = this.needsSolving = needsSolving;
            }

            public static AccountVal operator +(AccountVal a, AccountVal b)
            {
                return new AccountVal(a.amount+b.amount);
            }
            public static AccountVal operator -(AccountVal a, AccountVal b)
            {
                return new AccountVal(a.amount-b.amount);
            }
            public static AccountVal operator /(AccountVal a, AccountVal b)
            {
                return new AccountVal(a.amount/b.amount);
            }
            public static AccountVal operator *(AccountVal a, AccountVal b)
            {
                return new AccountVal(a.amount*b.amount);
            }
            public static AccountVal operator %(AccountVal a, AccountVal b)
            {
                return new AccountVal(a.amount%b.amount);
            }
            public static AccountVal operator +(AccountVal a, int b)
            {
                return new AccountVal(a.amount+b);
            }
            public static AccountVal operator -(AccountVal a, int b)
            {
                return new AccountVal(a.amount-b);
            }
            public static AccountVal operator /(AccountVal a, int b)
            {
                return new AccountVal(a.amount/b);
            }
            public static AccountVal operator *(AccountVal a, int b)
            {
                return new AccountVal(a.amount*b);
            }
            public static AccountVal operator %(AccountVal a, int b)
            {
                return new AccountVal(a.amount%b);
            }
            public static AccountVal operator +(int b,AccountVal a)
            {
                return new AccountVal(a.amount+b);
            }
            public static AccountVal operator -(int b,AccountVal a)
            {
                return new AccountVal(b-a.amount);
            }
            public static AccountVal operator /(int b,AccountVal a)
            {
                return new AccountVal(b/a.amount);
            }
            public static AccountVal operator *(int b,AccountVal a)
            {
                return new AccountVal(a.amount*b);
            }
            public static AccountVal operator %(int b,AccountVal a)
            {
                return new AccountVal(b%a.amount);
            }
            //we are not overloading the bitshift operators (there is literally no need for something simple like this)


            public static bool operator ==(AccountVal a, AccountVal b)
            {
                return a.amount == b.amount;
            }

            public static bool operator !=(AccountVal a, AccountVal b)
            {
                return a.amount != b.amount;
            }

            public static bool operator ==(AccountVal a, int b)
            {
                return a.amount == b;
            }
             public static bool operator !=(AccountVal a, int b)
             {
                return a.amount != b;
             }
            public static bool operator !=(int b,AccountVal a)
            {
                return a.amount != b;
            }
            public static bool operator ==(int b, AccountVal a)
            {
                return a.amount == b;
            }

            public static bool operator >=(AccountVal a, AccountVal b)
            {
                return a.amount >= b.amount;
            }
            public static bool operator <=(AccountVal a, AccountVal b)
            {
                return a.amount <= b.amount;
            }
            public static bool operator >=(AccountVal a, int b)
            {
                return a.amount >= b;
            }
            public static bool operator <=(AccountVal a, int b)
            {
                return a.amount <= b;
            }
            public static bool operator >=(int b,AccountVal a)
            {
                return a.amount >= b;
            }
            public static bool operator <=(int b,AccountVal a)//look at me, i just moved the parameter to the end so fancy
            {
                return a.amount <= b;
            }
          
            public static bool operator >(AccountVal a, AccountVal b)
            {
                return a.amount > b.amount;
            }

            public static bool operator <(AccountVal a, AccountVal b)
            {
                return a.amount < b.amount;
            }
            public static bool operator >(AccountVal a, int b)
            {
                return a.amount > b;
            }

            public static bool operator <(AccountVal a, int b)
            {
                return a.amount < b;
            }
            public static bool operator >( int b, AccountVal a)
            {
                return b>a.amount;
            }

            public static bool operator <( int b, AccountVal a)
            {
                return b < a.amount;
            }
            
        }
        
        //example account values (taken from 2018-District test of UIL Accounting
        /*Cost of delivered merchandise	64,295
            Cost of merchandise available for sale	81,112
            Gross profit	40,488
            Net purchases	58,612
            Net sales	96,400
            Purchases	           ?
            Purchases discounts	           ?
            Purchases returns and allowances	3,487
            Sales	101,810
            Sales discounts	            ?
            Sales returns and allowances	3,270
            Transportation in	4,288
        */
        //Purchases+Transportation in (or Freight in, pick whichever)
        private AccountVal _beginningInventory, _costOfDeliveredMerchandise,_costOfMerchandiseSold,
            _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit,_netPurchases,_netSales,_purchases,
            _purchasesDiscounts,_purchasesRetAndAllow,_sales,_salesDiscounts,_salesRetAndAllow,_transportationIn;

        private AccountVal[] accounts;
        private void InitializeAccounts()
        {
            /*Randomize:
                Purchases
                Transportation In
                P_Returns
                P_Allowances
                Beginning Inventory
                Ending Inventory
                Sales
                S_Returns
                S_Allowances
                
            Then the program should solve for the remaining accounts*/
            
            //initializing values
            RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[4];     //4 bytes is 32 bits... GetBytes throws an exception if the array is less than 4 bytes
            rg.GetBytes(bytes,0,2/*2 bytes is worth 65536 in base 10*/);
            //divide the byte to bit value by 10 because we don't need that (65k is a lot for returns and discounts btw)
            /*We set these in this order (instead of alphabetical or whatnot) so we don't have to reset any array indices after using the random value*/
            _transportationIn = new AccountVal(500+BitConverter.ToInt32(bytes, 0)/10, false/*todo*/);
            rg.GetBytes(bytes,0,2);
            _purchasesRetAndAllow = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,false/*todo*/);
            rg.GetBytes(bytes,0,2);
            _purchasesDiscounts = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,false/*todo*/);
            rg.GetBytes(bytes,0,2);
            _salesRetAndAllow = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,false/*todo*/);
            rg.GetBytes(bytes,0,2);
            _salesDiscounts = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,false/*todo*/);
            rg.GetBytes(bytes,0,2);
            //beginning inventory will be a range of 10k to 75536k
            _beginningInventory = new AccountVal(10000+BitConverter.ToInt32(bytes,0),false/*todo*/);
            rg.GetBytes(bytes,0,2);
            _endingInventory = new AccountVal(10000+BitConverter.ToInt32(bytes,0),false/*todo*/);

                //i am not worried about these numbers being *too* realistic... who cares if their COMS may exceed their sales by a ton?
            
            rg.GetBytes(bytes,0,3);
            //since 3 bytes gives us a max of 16mil in base10... we divide by 100 so we can't have a value or 160k
            _purchases = new AccountVal(80000+BitConverter.ToInt32(bytes,0)/100,false/*todo*/);
            rg.GetBytes(bytes,0,3);
            _sales = new AccountVal(80000+BitConverter.ToInt32(bytes,0)/100,false);//obligatory todo: set the 'needsSolving' var properly

            _costOfDeliveredMerchandise = _purchases + _transportationIn;
            _netPurchases = _costOfDeliveredMerchandise - _purchasesRetAndAllow - _purchasesDiscounts;
            _netSales = _sales - _salesDiscounts - _salesRetAndAllow;
            _costOfMerchandiseAvaForSale = _beginningInventory + _purchases;
            _costOfMerchandiseSold = _costOfMerchandiseAvaForSale - _endingInventory;
            _grossProfit = _netSales - _costOfMerchandiseSold;
            
            //setting up substitutes
            //by this point if you haven't realized, this stuff is incredibly inefficient (but it works)... blame UNTs slow-paced CS program
            _purchases.substitutes.Add(new AccountVal[]{_transportationIn,_costOfDeliveredMerchandise});
            
            _transportationIn.substitutes.Add(new AccountVal[]{_purchases,_costOfDeliveredMerchandise});
            _costOfDeliveredMerchandise.substitutes.Add(new []{_transportationIn,_purchases});
            _costOfDeliveredMerchandise.substitutes.Add(new []{_purchasesRetAndAllow,_purchasesDiscounts,_netPurchases});
            
            _purchasesRetAndAllow.substitutes.Add(new []{_costOfDeliveredMerchandise,_purchasesDiscounts,_netPurchases});
            _purchasesDiscounts.substitutes.Add(new[]{_costOfDeliveredMerchandise,_purchasesRetAndAllow,_netPurchases});
            
            _netPurchases.substitutes.Add(new []{_costOfDeliveredMerchandise,_purchasesRetAndAllow,_purchasesDiscounts});
            _netPurchases.substitutes.Add(new []{_beginningInventory,_costOfMerchandiseAvaForSale});
            
            _beginningInventory.substitutes.Add(new[]{_netPurchases,_costOfMerchandiseAvaForSale});
            
            _costOfMerchandiseAvaForSale.substitutes.Add(new[] {_beginningInventory, _netPurchases});
            _costOfMerchandiseAvaForSale.substitutes.Add(new []{_endingInventory,_costOfMerchandiseSold});
            
            _endingInventory.substitutes.Add(new []{_costOfMerchandiseAvaForSale,_costOfMerchandiseSold});
            
            _costOfMerchandiseSold.substitutes.Add(new []{_costOfMerchandiseAvaForSale,_endingInventory});
            _costOfMerchandiseSold.substitutes.Add(new []{_netSales,_grossProfit});
            
            _sales.substitutes.Add(new []{_salesRetAndAllow,_salesDiscounts,_netSales});
            
            _salesRetAndAllow.substitutes.Add(new []{_sales,_salesDiscounts,_netSales});
            
            _salesDiscounts.substitutes.Add(new []{_sales,_salesRetAndAllow,_netSales});
            
            _netSales.substitutes.Add(new []{_sales,_salesRetAndAllow,_salesDiscounts});
            _netSales.substitutes.Add(new []{_costOfMerchandiseSold,_grossProfit});
            
            _grossProfit.substitutes.Add(new []{_netSales,_costOfMerchandiseSold});
            
            //here are the equations:
            
            /*Purchases + Transportation In = Cost of Delivered Merchandise
                Cost of Delivered Merchandise - P_Returns - P_Discounts = Net Purchases

                Beginning inventory + Net purchases = Cost of Merchandise Available for Sale
                Cost of Merchandise Available for Sale - Ending Inventory = Cost of Merchandise Sold

                Sales - S_Returns - S_Discounts = Net Sales

                Net Sales - Cost of Merchandise Sold = Gross Profit*/
            //alphabet soup for those who prefer it:
            /*  A + B = C
                C - D - E = F

                G + F = H
                H - I = J

                K - L - M = N

                N - J = O

            */
            /* where
                A - Purchases
                B - Transportation In
                C - Cost of Delivered Merchandise
                D - Purchases Returns and Allowances
                E - Purchases Discounts
                F - Net Purchases
                G - Beginning Inventory
                H - Cost of Merchandise Available for Sale
                I - Ending Inventory
                J - Cost of Merchandise Sold
                K - Sales
                L - Sales Returns and Allowances
                M - Sales Discounts
                N - Net Sales
                O - Gross Profit 
             */
            
            _beginningInventory.Name = "Beginning Inventory";
            _costOfDeliveredMerchandise.Name = "Cost Of Delivered Merchandise";
            _costOfMerchandiseSold.Name = "Cost Of Merchandise Sold";
            _costOfMerchandiseAvaForSale.Name = "Cost Of Merchandise Available For Sale";
            _endingInventory.Name = "Ending Inventory";
            _grossProfit.Name = "Gross Profit";
            _netPurchases.Name = "Net Purchases";
            _netSales.Name = "Net Sales";
            _purchases.Name = "Purchases";
            _purchasesDiscounts.Name = "Purchases Discounts";
            _purchasesRetAndAllow.Name = "Purchases Returns and Allowances";
            _sales.Name = "Sales";
            _salesDiscounts.Name = "Sales Discounts";
            _salesRetAndAllow.Name = "Sales Returns and Allowances";
            _transportationIn.Name = "Transportation In";
            //Do this so we can select a value to initialize
            accounts = new[]
            {
                _beginningInventory, _costOfDeliveredMerchandise, _costOfMerchandiseSold,
                _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit, _netPurchases, _netSales, _purchases,
                _purchasesDiscounts, _purchasesRetAndAllow, _sales, _salesDiscounts, _salesRetAndAllow,
                _transportationIn
            };
            bytes[0] = bytes[1] = bytes[2] = bytes[3] = 0;
            
            
            for (int i = 0; i < accounts.Length; i++) //suggestion: visit these randomly as opposed to this because this makes latter variables less likely to be setor something
            {
                if (!accounts[i].solvingSet) //this will work for i=0... 
                {
                    //pass these objects by reference to decrease effect on stack
                    setSolveStates(ref accounts[i],ref rg,ref bytes);
                }
            }



        }

        /*
        private bool isSolvable(ref AccountVal original, ref AccountVal currentViewing)
        {
            if (!original.needsSolving)
            {
                return true;
            }

            
        }

        private bool recursiveSolveCheck(ref AccountVal original, ref AccountVal currentViewing)
        {
            if (!currentViewing.needsSolving)
            {
                return true;
            }
            foreach (AccountVal[] substituteArr in original.substitutes)
            {
                if (substituteArr.Contains(original)) //if substitutes only contains the original, then
                {
                    continue;
                }

                foreach (AccountVal account in substituteArr)
                {
                      
                }
            }

            return false;
        }*/
        
        
        //returns true if the account needs to be solved for, false if it does not
        private bool setSolveStates(ref AccountVal a, ref RNGCryptoServiceProvider rg, ref byte[] bytes)
        {
            a.visiting = true;
            
            rg.GetBytes(bytes,0,1);
            if (bytes[0] % 2 == 0) //if the value is even (so 50/50 chance of either option)
            {
                //this value does not need to be solved for and will be given in the problem
                a.needsSolving = false;
                a.solvingSet = true;
                a.visiting = false;
                return true;
            }
            //check if it can be set to unsolved state
            foreach (var substituteArr in a.substitutes)
            {
                //check to prevent infinite recursions
                if (substituteArr.Any(accountVal => accountVal.visiting))
                    break;
                    
                bool hasSolution = true; //
                for (var j = 0; j < substituteArr.Length; j++)
                {
                    AccountVal account = substituteArr[j];
                    //if the value needs to be solved for (this will check recursively ofcourse)
                    if (!setSolveStates(ref account, ref rg, ref bytes))
                    {
                        hasSolution = false;
                        break;
                    }
                    
                        
                        
                    //we check to see if the item has only one possible substitute
                    /*if (account.substitutes.Count == 1)
                        {
                            bool allSet = true;
                            foreach (AccountVal substitute in account.substitutes[0])
                            {
                                if (substitute.solvingSet)
                                {
                                    //if the value is needed to solve
                                    if(substitute.needsSolving)
                                    {
                                        allSet = false;
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }

                            if (allSet)
                            {
                                
                            }
                            
                        }*/
                }
                //if this substitue group could not be used to solve for this
                if (!hasSolution) continue;
                a.needsSolving = true;
                a.solvingSet = true;
                a.visiting = false;
                return false;
            }
            
            //reaching this point means the program could not find a suitable substitute with the given parameters
            a.needsSolving = false;
            a.solvingSet = true;
            a.visiting = false;
            return true;
        }
        
        public MainWindow()
        {
            Console.Out.WriteLine("testetstet");
            InitializeComponent();
        }
    }
}