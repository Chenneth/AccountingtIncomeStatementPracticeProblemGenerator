using System;
using System.Collections.Generic;
using System.Linq;
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
//remove below
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //changed to a class from struct because we want shallow copies
        private class AccountVal
        {
            public int amount;
            public bool needsSolving; //as in, this value was not given in the initial problem


            public AccountVal(int amount)
            {
                this.amount = amount;
                needsSolving = false; //defaulting
            }
            public AccountVal(int amount, bool needsSolving)
            {
                this.amount = amount;
                this.needsSolving = needsSolving;
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
        private AccountVal _beginningInventory, _costOfDeliveredMerchandise,_costOfGoodsSold,
            _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit,_netPurchases,_netSales,_purchases,
            _purchasesDiscounts,_purchasesRetAndAllow,_sales,_salesDiscounts,_salesRetAndAllow,_transportationIn;

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
                
            And solve for the remaining accounts*/
            RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[4];     //4 bytes is 32 bits... GetBytes throws an exception if the array is less than 4 bytes
            rg.GetBytes(bytes,0,2/*2 bytes is worth 65536 in base 10*/);
            //divide the byte to bit value by 10 because we don't need that (65k is a lot for returns and discounts btw)
            /*We set these in this order (instead of alphabetical or whatnot) so we don't have to reset any array indices after using the random value*/
            _transportationIn = new AccountVal(500+BitConverter.ToInt32(bytes, 0)/10, true/*todo*/);
            rg.GetBytes(bytes,0,2);
            _purchasesRetAndAllow = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,true/*todo*/);
            rg.GetBytes(bytes,0,2);
            _purchasesDiscounts = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,true/*todo*/);
            rg.GetBytes(bytes,0,2);
            _salesRetAndAllow = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,true/*todo*/);
            rg.GetBytes(bytes,0,2);
            _salesDiscounts = new AccountVal(300+BitConverter.ToInt32(bytes,0)/10,true/*todo*/);
            rg.GetBytes(bytes,0,2);
            //beginning inventory will be a range of 10k to 75536k
            _beginningInventory = new AccountVal(10000+BitConverter.ToInt32(bytes,0),true/*todo*/);
            rg.GetBytes(bytes,0,2);
            _endingInventory = new AccountVal(10000+BitConverter.ToInt32(bytes,0),true/*todo*/);

                //i am not worried about these numbers being *too* realistic... who cares if their COMS may exceed their sales by a ton?
            
            rg.GetBytes(bytes,0,3);
            //since 3 bytes gives us a max of 16mil in base10... we divide by 100 so we can't have a value or 160k
            _purchases = new AccountVal(80000+BitConverter.ToInt32(bytes,0)/100,true/*todo*/);
            rg.GetBytes(bytes,0,3);
            _sales = new AccountVal(80000+BitConverter.ToInt32(bytes,0)/100,true);//obligatory todo: set the 'needsSolving' var properly

            _costOfDeliveredMerchandise = _purchases + _transportationIn;
            _netPurchases = _costOfDeliveredMerchandise - _purchasesRetAndAllow - _purchasesDiscounts;
            _netSales = _sales - _salesDiscounts - _salesRetAndAllow;
            _costOfMerchandiseAvaForSale = _beginningInventory + _purchases;
            _costOfGoodsSold = _costOfMerchandiseAvaForSale - _endingInventory;
            _grossProfit = _netSales - _costOfGoodsSold;
            
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



        }
        
        public MainWindow()
        {
            InitializeComponent();
        }
        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
            
        }
    }
}