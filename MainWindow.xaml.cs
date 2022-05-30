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
                _isSolvableVisiting = visiting = false;
            }
            public AccountVal(int amount)
            {
                Name = "Not set";
                substitutes = new List<AccountVal[]>(3);//did 3 because i am lazy
                this.amount = amount;
                solvingSet = needsSolving = false; //defaulting
                _isSolvableVisiting = visiting = false;
            }
            public AccountVal(int amount, bool needsSolving)
            {
                Name = "Not set";
                substitutes = new List<AccountVal[]>(3);
                this.amount = amount;
                solvingSet = this.needsSolving = needsSolving;
                _isSolvableVisiting = visiting = false;
            }

            private bool _isSolvableVisiting;
            
            public bool IsSolvable()
            {
                _isSolvableVisiting = true;
                if (needsSolving)
                {
                    _isSolvableVisiting = false;
                    return true;
                }
                //iterate through substitutes

                //mmm it went past the forbidden line in my ide so i split it up a bit
                if ((from substituteGroup in substitutes 
                        where !substituteGroup.Any(x=>x._isSolvableVisiting) 
                        select substituteGroup.All(
                            t => t is not null && t.IsSolvable())).Any(groupSolvable => groupSolvable))
                {
                    _isSolvableVisiting = false;
                    return true;
                }

                _isSolvableVisiting = false;
                return false;
            }

            //the stupid operator overloads. this takes up too much space
            //we are not overloading the bitshift operators (there is literally no need for something like this)
            //it was taking up too much space and was a pain to scroll through so i removed some of the line breaks
                    //this was 90+ lines before...
            public static AccountVal operator +(AccountVal a, AccountVal b) { return new AccountVal(a.amount+b.amount); }
            public static AccountVal operator -(AccountVal a, AccountVal b) { return new AccountVal(a.amount-b.amount); }
            public static AccountVal operator /(AccountVal a, AccountVal b) { return new AccountVal(a.amount/b.amount); }
            public static AccountVal operator *(AccountVal a, AccountVal b) { return new AccountVal(a.amount*b.amount); }
            public static AccountVal operator %(AccountVal a, AccountVal b) { return new AccountVal(a.amount%b.amount); }
            public static AccountVal operator +(AccountVal a, int b) { return new AccountVal(a.amount+b); }
            public static AccountVal operator -(AccountVal a, int b) { return new AccountVal(a.amount-b); }
            public static AccountVal operator /(AccountVal a, int b) { return new AccountVal(a.amount/b); }
            public static AccountVal operator *(AccountVal a, int b) { return new AccountVal(a.amount*b); }
            public static AccountVal operator %(AccountVal a, int b) { return new AccountVal(a.amount%b); }
            public static AccountVal operator +(int b,AccountVal a) { return new AccountVal(a.amount+b); }
            public static AccountVal operator -(int b,AccountVal a) { return new AccountVal(b-a.amount); }
            public static AccountVal operator /(int b,AccountVal a) { return new AccountVal(b/a.amount); }
            public static AccountVal operator *(int b,AccountVal a) { return new AccountVal(a.amount*b); }
            public static AccountVal operator %(int b,AccountVal a) { return new AccountVal(b%a.amount); }
            public static bool operator ==(AccountVal a, AccountVal b) { return a.amount == b.amount; }
            public static bool operator !=(AccountVal a, AccountVal b) { return a.amount != b.amount; }
            public static bool operator ==(AccountVal a, int b) { return a.amount == b; }
            public static bool operator !=(AccountVal a, int b) { return a.amount != b; }
            public static bool operator !=(int b,AccountVal a) { return a.amount != b; }
            public static bool operator ==(int b, AccountVal a) { return a.amount == b; }
            public static bool operator >=(AccountVal a, AccountVal b) { return a.amount >= b.amount; }
            public static bool operator <=(AccountVal a, AccountVal b) { return a.amount <= b.amount; }
            public static bool operator >=(AccountVal a, int b) { return a.amount >= b; }
            public static bool operator <=(AccountVal a, int b) { return a.amount <= b; }
            public static bool operator >=(int b,AccountVal a) { return a.amount >= b; }
            public static bool operator <=(int b,AccountVal a) { return a.amount <= b; }
            public static bool operator >(AccountVal a, AccountVal b) { return a.amount > b.amount; }
            public static bool operator <(AccountVal a, AccountVal b) { return a.amount < b.amount; }
            public static bool operator >(AccountVal a, int b) { return a.amount > b; }
            public static bool operator <(AccountVal a, int b) { return a.amount < b; }
            public static bool operator >( int b, AccountVal a) { return b>a.amount; }
            public static bool operator <( int b, AccountVal a) { return b < a.amount; }
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

            //endpoints are arbitrary... don't think too much about them
            //trying to make this seem like a medium-sized business (aka not Walmart)
            _transportationIn = new AccountVal(500+RandomNumberGenerator.GetInt32(500,10000), false);

            _purchasesRetAndAllow = new AccountVal(RandomNumberGenerator.GetInt32(300,8700),false);

            _purchasesDiscounts = new AccountVal(RandomNumberGenerator.GetInt32(300,16500),false);

            _salesRetAndAllow = new AccountVal(RandomNumberGenerator.GetInt32(300,14750),false);

            _salesDiscounts = new AccountVal(RandomNumberGenerator.GetInt32(300,13440),false);   //haha our company does not provide the discount
                                                                                                                        //jonathan you are causing me to lose potential business

            //it is bad practice to have a lot of inventory on hand (somethingsomething kaizen... love the fact that they have to use japanese just to say the word improvement)
                //>>proceeds to give the business a chance for high returns/allowances 
            _beginningInventory = new AccountVal(RandomNumberGenerator.GetInt32(10000,30000),false);
            
            _endingInventory = new AccountVal(RandomNumberGenerator.GetInt32(10000,30000),false);
            
            _purchases = new AccountVal(RandomNumberGenerator.GetInt32(80000,300000),false/*todo*/);
           
            _sales = new AccountVal(RandomNumberGenerator.GetInt32(_purchases.amount*3/4,375000),false);

            _costOfDeliveredMerchandise = _purchases + _transportationIn;
            _netPurchases = _costOfDeliveredMerchandise - _purchasesRetAndAllow - _purchasesDiscounts;
            _netSales = _sales - _salesDiscounts - _salesRetAndAllow;
            _costOfMerchandiseAvaForSale = _beginningInventory + _purchases;
            _costOfMerchandiseSold = _costOfMerchandiseAvaForSale - _endingInventory;
            _grossProfit = _netSales - _costOfMerchandiseSold;
            
            //setting up substitutes
            //by this point if you haven't realized, this stuff is incredibly inefficient (but it works)... i blame UNTs slow-paced CS program
                                                                                                //also it's been a year or so since i programmed seriously (and in .net)
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
            
            //here are the equations for those of you who have never taken accounting before (it's fun please take it):
            
            /*Purchases + Transportation In = Cost of Delivered Merchandise
                Cost of Delivered Merchandise - P_Returns - P_Discounts = Net Purchases

                Beginning inventory + Net purchases = Cost of Merchandise Available for Sale
                Cost of Merchandise Available for Sale - Ending Inventory = Cost of Merchandise Sold

                Sales - S_Returns - S_Discounts = Net Sales

                Net Sales - Cost of Merchandise Sold = Gross Profit*/
            
            //alphabet soup for those who prefer it over fun accounting concepts:
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
            AccountVal[] tempArr = (AccountVal[])accounts.Clone();
            //i am not putting a nullcheck since that doesn't make sense


            while (tempArr.Length>0)
            {
                int rand = RandomNumberGenerator.GetInt32(0, tempArr.Length);
                if (!tempArr[rand].solvingSet) //this will result in true for the first iteration 
                {
                    //pass these objects by reference to decrease effect on stack
                    setSolveStates(ref tempArr[rand]);
                }
            }
        }

        //returns true if the account needs to be solved for, false if it does not
        private bool setSolveStates(ref AccountVal a)
        {
            a.visiting = true;
            
            //randomize this because... idk
            /* Honestly this could probably be removed... i had this before the
             *
             *      int rand = RandomNumberGenerator.GetInt32(0, tempArr.Length);
             *      if (!tempArr[rand].solvingSet)
             *      was there. Whatever, it works as is; no point in fixing it when this isn't even an official job/
             */
            if (RandomNumberGenerator.GetInt32(1,4)==2) //1/3 chance to set the account value as given
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
                    //if the value needs to be solved for (this will check recursively, ofcourse)
                    if (!setSolveStates(ref account))
                    {
                        hasSolution = false;
                        break;
                    }
                }
                //if this substitue group could not be used to solve for this
                if (!hasSolution) continue;
                a.needsSolving = true;
                a.solvingSet = true;
                a.visiting = false;
                return false;
            }
            
            //reaching this point means the program could not find a suitable substitute with the given substitutes
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