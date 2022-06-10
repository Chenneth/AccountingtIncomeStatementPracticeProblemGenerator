using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AcctISGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //changed to a class from struct because we want shallow copies
    public class AccountVal
    {
        public readonly int Amount;

        public bool Visiting, //this value is used mainly when setting needsSolving
            NeedsSolving, //as in, this value was not given in the initial problem
            SolvingSet; //this value is used when setting needsSolving

        public readonly List<AccountVal[]> Substitutes;
        public string Name { get; set; }

        public AccountVal()
        {
            Name = "Not set";
            Substitutes = new List<AccountVal[]>(3);
            Amount = 0;
            SolvingSet = _isSolvableVisiting = Visiting = false;
            NeedsSolving = true; //defaulting to true because I'm lazy
        }

        public AccountVal(int amount)
        {
            Name = "Not set";
            Substitutes = new List<AccountVal[]>(3); //did 3 because i am lazy
            this.Amount = amount;
            SolvingSet = NeedsSolving = false; //defaulting
            _isSolvableVisiting = Visiting = false;
        }

        public AccountVal(int amount, bool needsSolving)
        {
            Name = "Not set";
            Substitutes = new List<AccountVal[]>(3);
            this.Amount = amount;
            SolvingSet = false;
            this.NeedsSolving = needsSolving;
            _isSolvableVisiting = Visiting = false;
        }

        private bool _isSolvableVisiting;

        public bool IsSolvableRecursive()
        {
            _isSolvableVisiting = true;
            if (!NeedsSolving)
            {
                _isSolvableVisiting = false;
                return true;
            }
            //iterate through substitutes

            /*//mmm it went past the forbidden line in my ide so i split it up a bit
            if ((from substituteGroup in substitutes
                    where !substituteGroup.Any(x => x._isSolvableVisiting)
                    select substituteGroup.All(
                        t => t is not null && t.IsSolvableRecursive())).Any(groupSolvable => groupSolvable))
            {
                _isSolvableVisiting = false;
                return true;
            }*/
            if (Substitutes.Any(substituteGroup => substituteGroup.Where(substitute => !substitute._isSolvableVisiting)
                    .All(substitute => substitute.IsSolvableRecursive())))
            {
                _isSolvableVisiting = false;
                return true;
            }


            _isSolvableVisiting = false;
            return false;
        }

        /*Only returns an expected value is SolvingSet is true*/
        public bool IsSolvable()
        {
            //either it doesn't need solving and the value has been set, or there is a substitute group where all values do not need to be solved for and the value has been properly set
            return (!this.NeedsSolving && this.SolvingSet) ||
                   Substitutes.Any(subGroup =>
                       subGroup.All(acct => (!acct.NeedsSolving && acct.SolvingSet) || acct.UsableSubstitute()));
        }

        public bool UsableSubstitute() //Essentially IsSolvableRecursive, but it doesn't check itself for anything
        {
            _isSolvableVisiting = true;
            /*if ((from substituteGroup in substitutes
                    where substituteGroup.Any(x => !x._isSolvableVisiting) //where none of them are currently being checked in the recursive functions
                    select substituteGroup.All(//select the groups where the substitute is not null and it is solvable recursively 
                        t => t is not null && t.IsSolvableRecursive())).Any(groupSolvable => groupSolvable))
            {
                _isSolvableVisiting = false;
                return true;
            }*/

            if (Substitutes.Any( //select the substitute groups that
                    substituteGroup => substituteGroup
                        .Where( //do not have a variable that is currently being visited by the recursive function calls
                            substitute => !substitute._isSolvableVisiting)
                        .All( //and then check if that group is solvable recursively
                            substitute => substitute.IsSolvableRecursive())
                ))
                _isSolvableVisiting = false;
            return false;
        }

        //the stupid operator overloads. this takes up too much space
        //we are not overloading the bitshift operators (there is literally no need for something like this)
        //it was taking up too much space and was a pain to scroll through so i removed some of the line breaks
        //this was 90+ lines before...
        public static AccountVal operator +(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.Amount + b.Amount);
        }

        public static AccountVal operator -(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.Amount - b.Amount);
        }

        public static AccountVal operator /(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.Amount / b.Amount);
        }

        public static AccountVal operator *(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.Amount * b.Amount);
        }

        public static AccountVal operator %(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.Amount % b.Amount);
        }

        public static AccountVal operator +(AccountVal a, int b)
        {
            return new AccountVal(a.Amount + b);
        }

        public static AccountVal operator -(AccountVal a, int b)
        {
            return new AccountVal(a.Amount - b);
        }

        public static AccountVal operator /(AccountVal a, int b)
        {
            return new AccountVal(a.Amount / b);
        }

        public static AccountVal operator *(AccountVal a, int b)
        {
            return new AccountVal(a.Amount * b);
        }

        public static AccountVal operator %(AccountVal a, int b)
        {
            return new AccountVal(a.Amount % b);
        }

        public static AccountVal operator +(int b, AccountVal a)
        {
            return new AccountVal(a.Amount + b);
        }

        public static AccountVal operator -(int b, AccountVal a)
        {
            return new AccountVal(b - a.Amount);
        }

        public static AccountVal operator /(int b, AccountVal a)
        {
            return new AccountVal(b / a.Amount);
        }

        public static AccountVal operator *(int b, AccountVal a)
        {
            return new AccountVal(a.Amount * b);
        }

        public static AccountVal operator %(int b, AccountVal a)
        {
            return new AccountVal(b % a.Amount);
        }

        public static bool operator ==(AccountVal a, AccountVal b)
        {
            return b != null && a != null && a.Amount == b.Amount;
        }

        public static bool operator !=(AccountVal a, AccountVal b)
        {
            return a != null && b != null && a.Amount != b.Amount;
        }

        public static bool operator ==(AccountVal a, int b)
        {
            return a != null && a.Amount == b;
        }

        public static bool operator !=(AccountVal a, int b)
        {
            return a != null && a.Amount != b;
        }

        public static bool operator !=(int b, AccountVal a)
        {
            return a != null && a.Amount != b;
        }

        public static bool operator ==(int b, AccountVal a)
        {
            return a != null && a.Amount == b;
        }

        public static bool operator >=(AccountVal a, AccountVal b)
        {
            return a.Amount >= b.Amount;
        }

        public static bool operator <=(AccountVal a, AccountVal b)
        {
            return a.Amount <= b.Amount;
        }

        public static bool operator >=(AccountVal a, int b)
        {
            return a.Amount >= b;
        }

        public static bool operator <=(AccountVal a, int b)
        {
            return a.Amount <= b;
        }

        public static bool operator >=(int b, AccountVal a)
        {
            return a.Amount >= b;
        }

        public static bool operator <=(int b, AccountVal a)
        {
            return a.Amount <= b;
        }

        public static bool operator >(AccountVal a, AccountVal b)
        {
            return a.Amount > b.Amount;
        }

        public static bool operator <(AccountVal a, AccountVal b)
        {
            return a.Amount < b.Amount;
        }

        public static bool operator >(AccountVal a, int b)
        {
            return a.Amount > b;
        }

        public static bool operator <(AccountVal a, int b)
        {
            return a.Amount < b;
        }

        public static bool operator >(int b, AccountVal a)
        {
            return b > a.Amount;
        }

        public static bool operator <(int b, AccountVal a)
        {
            return b < a.Amount;
        }

        public override bool Equals(object o)
        {
            AccountVal test = o as AccountVal;
            if (test is null)
                return false;
            return
                test.Amount == Amount && test.Name == Name; //not sure if i want this to also check the substitutes...
        }

        public override int GetHashCode()
        {
            return (Amount + Substitutes.GetHashCode()) * 7;
        }

        public override string ToString()
        {
            return $"{Name}: {Amount:C0}";
        }
    }

    /*
    public class QuestionFinishedEventArgs
    {
        public QuestionFinishedEventArgs(List<object> args) { Args = args; }
        public List<object> Args { get; }
    }

    public class IncorrectAnswerEventArgs
    {
        public IncorrectAnswerEventArgs(int expected, int actual)
        {
            Expected = expected;
            Actual = actual;
        }
        public int Expected { get; }
        public int Actual { get; }
    }
    */

    public class RandomDateTime //from stackov erflow.com/a/ 262 63669
        //without the spaces
    {
        readonly DateTime _start;
        readonly Random _gen;
        private readonly int _range;

        public RandomDateTime()
        {
            _start = new DateTime(1995, 1, 1);
            _gen = new Random();
            _range = (DateTime.Today - _start).Days;
        }

        public DateTime Next()
        {
            return _start.AddDays(_gen.Next(_range)).AddHours(_gen.Next(0, 24)).AddMinutes(_gen.Next(0, 60))
                .AddSeconds(_gen.Next(0, 60));
        }
    }


    public partial class MainWindow
    {
        private bool _showAnswerFlag;
        private bool _restartFlag;
        private bool _backToMainFlag;

        //used to determine whether to set the restart/quit buttons visible or the show answers/help/quit buttons
        private bool _questionsCompleteFlag;

        /*Events left commented in case it's needed*/
        //public delegate void QuestionFinishedEventHandler(object sender, QuestionFinishedEventArgs e);
        //public delegate void IncorrectAnswerEventHandler(object sender, IncorrectAnswerEventArgs e);
        //public event QuestionFinishedEventHandler QuestionFinishedEvent;
        //public event IncorrectAnswerEventHandler IncorrectAnswerEvent;

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

        //there are 15 accounts in this list (i know it's a pain to read this)
        private AccountVal _beginningInventory,
            _costOfDeliveredMerchandise,
            _costOfMerchandiseSold,
            _costOfMerchandiseAvaForSale,
            _endingInventory,
            _grossProfit,
            _netPurchases,
            _netSales,
            _purchases,
            _purchasesDiscounts,
            _purchasesRetAndAllow,
            _sales,
            _salesDiscounts,
            _salesRetAndAllow,
            _transportationIn;
        //15 "accounts" (i am aware that cost of delivered merchandise and coms are not accounts) total

        //i'd like to have better worded questions beyond "What is amount of x" but, not necessary not gonna t odo this
        /*private static Dictionary<string, string> accountQuestions = new Dictionary<string, string>()
        {
            {"Beginning Inventory", ""  }   
        };*/

        private AccountVal[] _accounts;
        private readonly List<AccountVal> _givenAccounts;
        private int _amountSolved;

        private HelpWindow _helpWindow;

        //this is separate so we can use it for the button checking
        //key will be the associated Button (in relation to the question and what account is being asked for)
        //...I'm not sure what else to do...
        private Dictionary<Button, TextBox> _buttonToInputBox;

        //key is the input textbox and it gives the AccountVal obj associated with it
        private Dictionary<TextBox, AccountVal> _notGivenAccts;
        private readonly Dictionary<TextBox, TextBox> _inputToWarningBox;
        private List<TextBox> _inputTextBoxes;

        private Dictionary<AccountVal, TextBox> _incomeStatementValueTextBoxes;


        //why yes, I did spend 1 hour thinking up of random words how could you tell
        private static string[] companyNames = new string[]
        {
            "Cygnet",
            "Enterprise",
            "Hokum",
            "Coffin",
            "Nebulous",
            "Rathskeller",
            "Kaizen",
            "Unmei",
            "Pagoda",
            "Basilisk",
            "Piebald",
            "Investments",
            "Dauntless",
            "Palatine",
            "Excelsis",
            "Paladin",
            "Phantasmal",
            "Deguchi",
            "Fichu",
            "Meuniere",
            "Grace",
            "Aegis",
            "Primordial",
            "Zenith",
            "Ephemeral",
            "Balmung",
            "Arondight",
            "Galaxy",
            "Enterprise",
            "Abyssal",
            "Midnight",
            "Rosemary",
            "Void",
            "Fragmented",
            "Arthropod",
            "Eternal",
            "Bat",
            "Western",
            "Integrity",
            "Shiba Inu",
            "Experience",
            "Oath",
            "Resistance",
            "Tantivy",
            "Saints",
            "Coalescent",
            "Cartridges",
            "Priority",
            "Factor",
            "Green",
            "Red",
            "Yellow",
            "Soother",
            "Sojourn",
            "Archaic",
            "Exquisite",
            "Thereupon",
            "Mukashi",
            "Aegis",
            "Seal",
            "Iron",
            "Uranium",
            "Quintillion",
            "Access",
            "Quest",
            "Excieo",
            "Jierda",
            "Consolidated",
            "Excess", //please do not use negative adjectives like this in your company's name
            "Python",
            "Magical",
            "Journals",
            "Eighty",
            "Heaven",
            "Insomnia",
            "Frames",
            "Inheritance",
            "Dispersion",
            "Immersive",
            "Mini",
            "Refactor",
            "Dragon",
            "Laser",
            "Beast",
            "Hard",
            "Charged",
            "Spirit",
            "Curse",
            "Monkey",
            "Retrieval",
            "Wolf",
            "Silver",
            "Kessen",
            "Apfel", //imagine being dumb like this - couldnt be me
            "Anchor",
            "Coarse",
            "Neo",
            "Counter",
            "Mallet",
            "Falsehood",
            "Divine",
            "Palace",
            "Collaborative",
            "Literature",
            "Absolute",
            "Titan",
            "Warlock",
            "Spade",
            "Presence",
            "Stamp",
            "Crossmark",
            "Lantern",
            "Dive",
            "Designer",
            "Urban",
            "Peak",
            "Boulder",
            "Bold",
            "Not",
            "Yuuki",
            "Drone",
            "Starry",
            "Sanguine",
            "Ninja",
            "Djinn",
            "Bodhi",
            "Strafe",
            "Summer",
            "Arrow",
            "Scope",
            "Moral",
            "Grail",
            "Fenrir",
            "Earthquake",
            "United",
            "Fair",
            "Imagine",
            "Hammer",
            "Blast",
            "Table",
            "Omelet",
            "Battery",
            "Impact",
            "Stella",
            "Color",
            "Neighbor",
            "Sofa",
            "Rover",
            "Giga",
            "Screaming",
            "Bevel",
            "Render",
            "Hidden",
            "Clash",
            "Parallel",
            "Bookworm",
            "Delta",
            "Ferris",
            "Pipe",
            "City",
            "Smith",
            "Jenkins",
            "Congruence",
            "Devoted",
            "Secret"
        };

        private static string[] companyTypes = new string[] //not gonna be a lot tbh
        {
            "Company",
            "LLC",
            "Corporation",
            "Incorporated",
            "Engineers", //i am aware this is not an actual business type, these are the "endings" for the company name
            "Partnership",
            "Excavations",
            "Industries",
            "Construction",
            "Squad",
            "Antiques",
            "Consortium",
            "Burial Services", //tfw burial services has merchandise
            "Protection Services", //merchandise
            "Research Laboratory", //why would a laboratory have merchandise
            "Group",
            "Supply",
            "Airlines",
            "Airways"
        };


        private bool _isIncomeStatementProblem;

        private DateTime _fiscalYearStart, _fiscalYearEnd;
        private RandomDateTime _randomDateTime;

        //i made a useless regex because i didn't see the tryparse function 
        //private static Regex validNumInput = new Regex(@"^(?:-?(?:[.,]\d{3}|\d)*|\((?:[.,]\d{3}|\d)*\))$", RegexOptions.Compiled);

        //idk what this is for
        /*private Dictionary<Button, TextBox> _buttonToString;*/
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
            _transportationIn = new AccountVal(500 + RandomNumberGenerator.GetInt32(500, 10000), true);

            _purchasesRetAndAllow = new AccountVal(RandomNumberGenerator.GetInt32(300, 8700), true);

            _purchasesDiscounts = new AccountVal(RandomNumberGenerator.GetInt32(300, 16500), true);

            _salesRetAndAllow = new AccountVal(RandomNumberGenerator.GetInt32(300, 14750), true);

            _salesDiscounts =
                new AccountVal(RandomNumberGenerator.GetInt32(300, 13440),
                    true); //haha our company does not provide the discount
            //jonathan you are causing me to lose potential business

            //it is bad practice to have a lot of inventory on hand (somethingsomething kaizen... love the fact that they have to use japanese just to say the word improvement)
            //>>proceeds to give the business a chance for high returns/allowances 
            _beginningInventory = new AccountVal(RandomNumberGenerator.GetInt32(10000, 30000), true);

            _endingInventory = new AccountVal(RandomNumberGenerator.GetInt32(10000, 30000), true);

            _purchases = new AccountVal(RandomNumberGenerator.GetInt32(80000, 300000), true);

            _sales = new AccountVal(RandomNumberGenerator.GetInt32(_purchases.Amount * 3 / 4, 375000), true);

            _costOfDeliveredMerchandise = _purchases + _transportationIn;
            _netPurchases = _costOfDeliveredMerchandise - _purchasesRetAndAllow - _purchasesDiscounts;
            _netSales = _sales - _salesDiscounts - _salesRetAndAllow;
            _costOfMerchandiseAvaForSale = _beginningInventory + _netPurchases;
            _costOfMerchandiseSold = _costOfMerchandiseAvaForSale - _endingInventory;
            _grossProfit = _netSales - _costOfMerchandiseSold;

            //setting up substitutes
            //by this point if you haven't realized, this stuff is incredibly inefficient (but it works)... i blame UNTs slow-paced CS program
            //also it's been a year or so since i programmed seriously (and in .net)
            _purchases.Substitutes.Add(new[] { _transportationIn, _costOfDeliveredMerchandise });

            _transportationIn.Substitutes.Add(new[] { _purchases, _costOfDeliveredMerchandise });
            _costOfDeliveredMerchandise.Substitutes.Add(new[] { _transportationIn, _purchases });
            _costOfDeliveredMerchandise.Substitutes.Add(new[]
                { _purchasesRetAndAllow, _purchasesDiscounts, _netPurchases });

            _purchasesRetAndAllow.Substitutes.Add(new[]
                { _costOfDeliveredMerchandise, _purchasesDiscounts, _netPurchases });
            _purchasesDiscounts.Substitutes.Add(new[]
                { _costOfDeliveredMerchandise, _purchasesRetAndAllow, _netPurchases });

            _netPurchases.Substitutes.Add(new[]
                { _costOfDeliveredMerchandise, _purchasesRetAndAllow, _purchasesDiscounts });
            _netPurchases.Substitutes.Add(new[] { _beginningInventory, _costOfMerchandiseAvaForSale });

            _beginningInventory.Substitutes.Add(new[] { _netPurchases, _costOfMerchandiseAvaForSale });

            _costOfMerchandiseAvaForSale.Substitutes.Add(new[] { _beginningInventory, _netPurchases });
            _costOfMerchandiseAvaForSale.Substitutes.Add(new[] { _endingInventory, _costOfMerchandiseSold });

            _endingInventory.Substitutes.Add(new[] { _costOfMerchandiseAvaForSale, _costOfMerchandiseSold });

            _costOfMerchandiseSold.Substitutes.Add(new[] { _costOfMerchandiseAvaForSale, _endingInventory });
            _costOfMerchandiseSold.Substitutes.Add(new[] { _netSales, _grossProfit });

            _sales.Substitutes.Add(new[] { _salesRetAndAllow, _salesDiscounts, _netSales });

            _salesRetAndAllow.Substitutes.Add(new[] { _sales, _salesDiscounts, _netSales });

            _salesDiscounts.Substitutes.Add(new[] { _sales, _salesRetAndAllow, _netSales });

            _netSales.Substitutes.Add(new[] { _sales, _salesRetAndAllow, _salesDiscounts });
            _netSales.Substitutes.Add(new[] { _costOfMerchandiseSold, _grossProfit });

            _grossProfit.Substitutes.Add(new[] { _netSales, _costOfMerchandiseSold });

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
            _accounts = new[]
            {
                _beginningInventory, _costOfDeliveredMerchandise, _costOfMerchandiseSold, //3
                _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit, _netPurchases, _netSales, _purchases, //6
                _purchasesDiscounts, _purchasesRetAndAllow, _sales, _salesDiscounts, _salesRetAndAllow, //5
                _transportationIn //1
            };
            do
            {
                if (_accounts.All(acct => acct.SolvingSet)) //for teh loops
                {
                    foreach (var acct in _accounts)
                    {
                        acct.SolvingSet = false;
                        acct.NeedsSolving = false;
                    }
                }

                List<AccountVal> tempArr = _accounts.ToList();
                //i definitely did not originally have a null check here... ha ha... ha.........................

                int tempRandIndex = RandomNumberGenerator.GetInt32(0, tempArr.Count);
                AccountVal tempRand = tempArr[tempRandIndex];
                tempRand.NeedsSolving = true;
                tempRand.SolvingSet = true;
                tempArr.RemoveAt(tempRandIndex);

                while (tempArr.Count > 0)
                {
                    int rand = RandomNumberGenerator.GetInt32(0, tempArr.Count);
                    if (!tempArr[rand].SolvingSet) //this will result in true for the first iteration 
                    {
                        //pass these objects by reference to decrease effect on stack
                        //setSolveStates(tempArr[rand]);
                        setSolveStatesAlternative(tempArr[rand]);
                    }

                    tempArr.RemoveAt(rand);
                }
            } while
                (_accounts.All(acct =>
                    !acct.NeedsSolving)); //in the event that we do have all account values given we want to redo the thing
        }

        //returns true if the account needs to be solved for, false if it does not
        //this function was made under the assumption that the initial states were set to false, so it may not work as intended otherwise
//        [Obsolete(
//            "Consider using setSolveStatesAlternative(AccountVal). This method assumes that initial value of NeedsSolving is false.")]
//        private bool SetSolveStates(AccountVal a)
//        {
//            if (a.SolvingSet)
//                return a.NeedsSolving;
//            a.Visiting = true;
//
//            //randomize this because otherwise it results in the same accounts being set to needs solving
//            /*if (RandomNumberGenerator.GetInt32(1,4)==2) //1/3 chance to set the account value as given
//            {
//                //this value does not need to be solved for and will be given in the problem
//                a.needsSolving = false;
//                a.solvingSet = true;
//                a.visiting = false;
//                return true;
//            }*/
//            //check if it can be set to unsolved state
//            foreach (var substituteArr in a.Substitutes)
//            {
//                //check to prevent infinite recursions
//                if (substituteArr.Any(accountVal => accountVal.Visiting))
//                    continue; //changed from a break statement, may [ironically] break the algorithm
//
//                bool hasSolution = true; //we assume that there is a solution
//                for (var j = 0; j < substituteArr.Length; j++)
//                {
//                    AccountVal account = substituteArr[j];
//                    //if the value needs to be solved for
//                    if (!account.IsSolvable())
//                    {
//                        hasSolution = false;
//                        break;
//                    }
//                }
//
//                //if this substitute group could not be used to solve for this
//                if (!hasSolution) continue;
//                a.NeedsSolving = true;
//                a.SolvingSet = true;
//                a.Visiting = false;
//                return false;
//            }
//
//            //reaching this point means the program could not find a suitable substitute with the given substitutes
//            a.NeedsSolving = false;
//            a.SolvingSet = true;
//            a.Visiting = false;
//            return true;
//        }

        //this version assumes all states are already set?
        private void setSolveStatesAlternative(AccountVal a)
        {
            if (a.SolvingSet) return;
            a.Visiting = true;

            //check if it can be set to unsolved state
            foreach (var substituteArr in a.Substitutes)
            {
                /*//check to prevent infinite recursions... may not be needed with alternative method?
                if (substituteArr.Any(accountVal => accountVal.Visiting))
                    continue;
                    */

                bool hasSolution = substituteArr.All(substitute => substitute.IsSolvable());

                //if this substitute group could not be used to solve for this
                if (hasSolution)
                {
                    a.NeedsSolving = true;
                    a.SolvingSet = true;
                    a.Visiting = false;
                    return;
                }
            }

            //reaching this point means the program could not find a suitable substitute with the given substitutes
            a.NeedsSolving = false;
            a.SolvingSet = true;
            a.Visiting = false;
        }

        public MainWindow()
        {
            //i don't know the max unsolved accounts we can have, but I know you need at least 3 accounts to solve most things
            _givenAccounts =
                new List<AccountVal>(
                    12); //original algorithm had a tendency to give this much at most, so we'll keep like this if we want to use said algorithm
            /*_buttonToString = new Dictionary<Button, TextBox>(5);*/
            _buttonToInputBox =
                new Dictionary<Button, TextBox>(
                    6); //6 has been the max i've gotten when running the new algorithm, about 15 times, so I highly doubt that the performance impact of allocating more for 7 will make a difference compared to storing a 7th empty slot
            _notGivenAccts = new Dictionary<TextBox, AccountVal>(6);
            _inputToWarningBox = new Dictionary<TextBox, TextBox>(6);
            _cancellationTokens =
                new Dictionary<TextBox, CancellationTokenSource>(
                    6); //shouldn't need more than 6 since that's the max amount of question boxes 
            _warningCTS = new Dictionary<TextBox, CancellationTokenSource>(6);
            _inputTextBoxes = new List<TextBox>(6);
            _incomeStatementValueTextBoxes =
                new Dictionary<AccountVal, TextBox>(
                    15); //the amount of accounts is 15, we will be placing the correct answer into the IS when the correct answer is shown :) todo i need more than 6 hours of consecutive sleep for a year

            _randomDateTime = new RandomDateTime();

            InitializeComponent();

            StartButton.Click += delegate { this.OpenSelectModeMenu(); };
            ExitButton.Click += delegate
            {
                Application.Current.Shutdown();
            }; //might consider adding a confirmation here idk

            ShowAnswersButton.Click += ShowAnswersButtonPressed;
            HelpButton.Click += HelpButtonPressed;
            FunctionHelpButton.Click += HelpButtonPressed;

            BackButton.Click += QuitButtonPressed;
            RestartQuitGridBackButton.Click += QuitButtonPressed;

            RestartButton.Click += RestartButtonPressed;
            YesButton.Click += YesConfirmationButtonPressed;
            NoButton.Click += NoConfirmationButtonPressed;

            StartModeButton.Click += CheckModeSelected;
            BackModeButton.Click += BackModeButtonPressed;
        }

        //i am thinking that this can be called to restart the thing too...?
        private void OpenSelectModeMenu()
        {
            RestartQuitGrid.Visibility = StartGrid.Visibility = ShowAnswersTextBox.Visibility =
                FunctionButtonsGrid.Visibility =
                    AllCorrectAnswersTextBox.Visibility = QuestionGrid.Visibility =
                        IncomeStatementGrid.Visibility = AccountListGrid.Visibility = AccountListBorder.Visibility =
                            Visibility.Hidden;

            //show the thingy grid
            ModeSelectGrid.Visibility = Visibility.Visible;
            /*           
            //stuff
            FunctionButtonsGrid.Visibility = FunctionGrid.Visibility = AccountListGrid.Visibility = AccountListBorder.Visibility = QuestionGrid.Visibility = Visibility.Visible;
            ShowFunctionButtons();
            HideConfirmationButtons();*/
        }

        private void ReturnToMainMenu() //idk what else this needs, but it doesn't feel complete?
        {
            StartGrid.Visibility = Visibility.Visible;

            FunctionGrid.Visibility = AccountListGrid.Visibility =
                AccountListBorder.Visibility = QuestionGrid.Visibility = Visibility.Hidden;
            HideConfirmationButtons();
        }

        private void AllQuestionsFinished()
        {
            _questionsCompleteFlag = true;
            HideConfirmationButtons();
            RestartQuitGrid.Visibility = Visibility.Visible;
            if (_showAnswerFlag) ShowAnswersTextBox.Visibility = Visibility.Visible;
            else AllCorrectAnswersTextBox.Visibility = Visibility.Visible;
        }

        private void SetInitialState()
        {
            _givenAccounts.Clear();
            _buttonToInputBox.Clear();
            _notGivenAccts.Clear();
            _inputTextBoxes.Clear();

            AccountListGrid.RowDefinitions.Clear();
            AccountListGrid.Children.Clear();

            QuestionGrid.Children.Clear();
            QuestionGrid.RowDefinitions.Clear();

            _amountSolved = 0;

            _isIncomeStatementProblem =
                _showAnswerFlag = _restartFlag = _backToMainFlag = _questionsCompleteFlag = false;
            InitializeAccounts();
        }


        //adds all the information to the grid that holds the account information
        private void InitializeAccountList()
        {
            SetInitialState();
            //_accounts is always alphabetically sorted so we shouldn't need to care about resorting this in the grid
            foreach (AccountVal acct in _accounts)
                if (!acct.NeedsSolving)
                    AddToAccountList(acct);
                else
                    AddToQuestionList(acct);

            //since the questions have no border, this does not affect them
            FixBottomBorderAList();

            FinishInitialization();
            AccountListGrid.Visibility =
                AccountListBorder.Visibility = Visibility.Visible;
        }

        private void FinishInitialization()
        {
            //we add one last row definition to the QuestionGrid to help with the sizes of the other rows
            QuestionGrid.RowDefinitions.Add(new RowDefinition()
                { Height = new GridLength(14 - QuestionGrid.RowDefinitions.Count, GridUnitType.Star) });
            RowDefinition endRow = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
            QuestionGrid.RowDefinitions.Add(endRow);
            //personally, 15* looked most appealing to me... but that was before I added a second grid below it

            FunctionButtonsGrid.Visibility = FunctionGrid.Visibility = QuestionGrid.Visibility = Visibility.Visible;
            ShowFunctionButtons();
            HideConfirmationButtons();
        }

        private void InitializeIncomeStatement()
        {
            SetInitialState();
            _isIncomeStatementProblem = true;

            _fiscalYearStart = _randomDateTime.Next();
            if (_fiscalYearStart.Month == 2 && _fiscalYearStart.Day == 29)
            {
                _fiscalYearStart =
                    _fiscalYearStart
                        .AddMonths(1); //i chose this on a whim but i don't want leap years messing up the year end calc
                //(no one chooses a leap day as their fiscal start anyways)
            }

            _fiscalYearEnd =
                _fiscalYearStart.AddYears(1)
                    .AddDays(-1); //ideally this gives us a start date end date with a year's difference?
            //e.g. start at jan 1 2018, end at dec 31 2018
            //e.g. start at mar 10 2002 end at mar 9 2003
            //doing this instead of add 364 days because leap year

            //generate random company and date
            CompanyNameISTextBox.Text = GetRandomCompanyName();
            DateTextISBox.Text = $"For the Year Ended {_fiscalYearEnd:MMMM dd, yyyy}";
            BeginInventoryISNameTextBox.Text = $"Inventory on {_fiscalYearStart:d}";
            EndingInventoryISNameTextBox.Text = $"Inventory on {_fiscalYearEnd:d}";
            /*_costOfDeliveredMerchandise,_costOfMerchandiseSold,
            _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit,
            _netPurchases,_netSales,_purchases,
            _purchasesDiscounts,_purchasesRetAndAllow,_sales,
            _salesDiscounts,_salesRetAndAllow,_transportationIn;*/
            //first add the accounts to the dictionary correlating themselves to the incomestatement
            _incomeStatementValueTextBoxes.Add(_beginningInventory, BeginInventoryISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_costOfDeliveredMerchandise, CoDMISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_costOfMerchandiseSold, COMSISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_costOfMerchandiseAvaForSale, COMAFSISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_endingInventory, EndingInventoryISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_grossProfit, GrossProfitISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_netPurchases, NetPurchasesISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_netSales, NetSalesISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_purchases, PurchasesISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_purchasesDiscounts, PurchasesDiscountsISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_purchasesRetAndAllow, PurchasesReturnsISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_sales, SalesISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_salesDiscounts, SalesDiscountsISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_salesRetAndAllow, SalesReturnsISValueTextBox);
            _incomeStatementValueTextBoxes.Add(_transportationIn, TransportationInISValueTextBox);

            BeginInventoryISValueTextBox.Foreground = CoDMISValueTextBox.Foreground = COMSISValueTextBox.Foreground =
                COMAFSISValueTextBox.Foreground = EndingInventoryISValueTextBox.Foreground =
                    GrossProfitISValueTextBox.Foreground = NetPurchasesISValueTextBox.Foreground =
                        NetSalesISValueTextBox.Foreground = PurchasesISValueTextBox.Foreground =
                            PurchasesISValueTextBox.Foreground = PurchasesDiscountsISValueTextBox.Foreground =
                                PurchasesReturnsISValueTextBox.Foreground = SalesISValueTextBox.Foreground =
                                    SalesDiscountsISValueTextBox.Foreground = SalesReturnsISValueTextBox.Foreground =
                                        TransportationInISValueTextBox.Foreground = Brushes.Black;
            foreach (AccountVal acct in _accounts)
                if (!acct.NeedsSolving)
                    AddToIncomeStatement(acct);
                else
                    AddToQuestionList(acct); //this can remain the exact same

            FinishInitialization();
            IncomeStatementGrid.Visibility = Visibility.Visible;
        }

        //adds the account to the Dictionary, _notGivenAccounts, and puts a question on the question grid
        private void AddToQuestionList(AccountVal acct)
        {
            /*_notGivenAccounts.Add(acct);*/
            /* <TextBox Grid.Row="0" Grid.Column="0" BorderThickness="0" TextWrapping="Wrap" IsReadOnly="True">What is the amount of Inventory on January 1, 2022?</TextBox>
                        <TextBox Grid.Row="0" Grid.Column="1" TextWrapping="NoWrap"></TextBox>
                        <Button Grid.Row="0" Grid.Column="2">Submit</Button>*/
            //Above is the xaml we want to create for each question
            QuestionGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            int rowIndex = QuestionGrid.RowDefinitions.Count - 1;

            TextBox questionBox = new TextBox()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                BorderThickness = new Thickness(0), TextWrapping = TextWrapping.Wrap, IsReadOnly = true,
                Text = $"What is the amount of {acct.Name}?"
            };
            /*questionBox.TextChanged += new TextChangedEventHandler();*/
            TextBox inputBox = new TextBox()
            {
                TextWrapping = TextWrapping.NoWrap, Margin = new Thickness(1, 0, 1, 0), CaretBrush = Brushes.Black
            }; //not sure why, but the caret is set to an aqua for some reason
            inputBox.KeyDown += EnterOrReturnPressed;
            Button submitButton = new Button()
            {
                Content = new TextBox()
                {
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, IsHitTestVisible = false,
                    IsReadOnly = true, Text = "Submit"
                }
            };
            submitButton.Click += SubmitButtonPressed;
            TextBox warningBox = new TextBox() //maybe consider adding a tooltip?
            {
                TextWrapping = TextWrapping.Wrap, BorderThickness = new Thickness(0), FontSize = 10,
                Foreground = Brushes.Red, IsHitTestVisible = false, IsReadOnly = true
            };

            Grid inputGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() { Height = new GridLength(3, GridUnitType.Star) },
                    new RowDefinition() { Height = new GridLength(3, GridUnitType.Star) }
                },
                Children = { inputBox, warningBox }
            };
            Grid.SetRow(inputBox, 0);
            Grid.SetRow(warningBox, 1);

            QuestionGrid.Children.Add(inputGrid);
            QuestionGrid.Children.Add(questionBox);
            //QuestionGrid.Children.Add(inputBox);
            QuestionGrid.Children.Add(submitButton);
            Grid.SetRow(questionBox, rowIndex);
            Grid.SetRow(inputGrid, rowIndex);
            //Grid.SetRow(inputBox, rowIndex);
            Grid.SetRow(submitButton, rowIndex);
            Grid.SetColumn(questionBox, 0);
            //Grid.SetColumn(inputBox, 1);
            Grid.SetColumn(inputGrid, 1);
            Grid.SetColumn(submitButton, 2);

            _notGivenAccts.Add(inputBox, acct);
            _buttonToInputBox.Add(submitButton, inputBox);
            _inputToWarningBox.Add(inputBox, warningBox);
            _inputTextBoxes.Add(inputBox);
        }

        private void AddToAccountList(AccountVal acct)
        {
            _givenAccounts.Add(acct);
            AccountListGrid.RowDefinitions.Add(new RowDefinition());
            TextBox acctNameTextBox = new TextBox
            {
                Text = acct.Name,

                Margin = new Thickness(1, 0, 0, 1),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Left,
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            /*Console.Out.WriteLine(acctNameTextBox.Text);*/
            Border boarder = new Border()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1)
            };
            boarder.Child = acctNameTextBox;
            AccountListGrid.Children.Add(boarder);
            Grid.SetRow(boarder, _givenAccounts.Count - 1);
            Grid.SetColumn(boarder, 0);
            TextBox acctValTextBox = new TextBox()
            {
                Text = acct.Amount.ToString("N"),

                Margin = new Thickness(0, 0, 1, 1),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Right,
                BorderThickness = new Thickness(0),
                IsReadOnly = true
            };
            Border acctValBorder = new Border()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            acctValBorder.Child = acctValTextBox;
            AccountListGrid.Children.Add(acctValBorder);
            Grid.SetRow(acctValBorder, _givenAccounts.Count - 1);
            Grid.SetColumn(acctValBorder, 1);
        }

        private void AddToIncomeStatement(AccountVal acct)
        {
            //we will not use TryGetValue for the dictionary since it has to always be in there
            _incomeStatementValueTextBoxes[acct].Text = acct.Amount.ToString("C0");
            _givenAccounts.Add(acct);
        }

        private string GetRandomCompanyName()
        {
            string name = "";

            //the alternative to prevent repeated words is to use a list, but that takes up space
            name += companyNames[RandomNumberGenerator.GetInt32(0, companyNames.Length)];
            if (RandomNumberGenerator.GetInt32(1, 21) < 7) //30% chance? number 1-20, 6 out of 20
            {
                name += $" {companyNames[RandomNumberGenerator.GetInt32(0, companyNames.Length)]}";
                if (RandomNumberGenerator.GetInt32(1, 21) == 1)
                {
                    name += $" {companyNames[RandomNumberGenerator.GetInt32(0, companyNames.Length)]}";
                }
            }

            name += $" {companyTypes[RandomNumberGenerator.GetInt32(0, companyTypes.Length)]}";
            return name;
        }

        //setting the border thickness properly for the bottom row. Ideally, these are the last two indices of the list, but we don't know for sure.
        private void FixBottomBorderAList()
        {
            int setCount = 0;
            for (var index = AccountListGrid.Children.Count - 1;
                 index >= 0;
                 index--) //thus we start at the end of the list instead
            {
                UIElement gridChild = AccountListGrid.Children[index];
                if (gridChild is Border gridChildBorder &&
                    Grid.GetRow(gridChild) == AccountListGrid.RowDefinitions.Count - 1)
                {
                    gridChildBorder.BorderThickness = new Thickness(0, 0, gridChildBorder.BorderThickness.Right, 0);
                    if (++setCount == 2) //there are only 2 columns, so we only need to set things twice
                        break; //so we can simply end the for loop as is
                } //theoretically that makes this take less than O(N) time.... but idk because I haven't learned much about O-notation
            }
        }

        private CancellationTokenSource _modeWarningCTS;

        private async void CheckModeSelected(object sender, RoutedEventArgs e)
        {
            _modeWarningCTS?.Cancel();
            if (ISModeSelect.IsChecked == true)
            {
                ModeSelectGrid.Visibility = Visibility.Hidden;
                ISModeSelect.IsChecked = false;
                InitializeIncomeStatement();
            }
            else if (ALModeSelect.IsChecked == true)
            {
                ModeSelectGrid.Visibility = Visibility.Hidden;
                ALModeSelect.IsChecked = false;
                InitializeAccountList();
            }
            else
            {
                _modeWarningCTS = new CancellationTokenSource();
                await AsyncDisplayWarningMessage(ModeWarningTextBox, _modeWarningCTS.Token, "Please select a mode.");
            }
        }

        //all
        private void BackModeButtonPressed(object sender, RoutedEventArgs e)
        {
            ModeSelectGrid.Visibility = Visibility.Hidden;
            StartGrid.Visibility = Visibility.Visible;
            ISModeSelect.IsChecked = false;
            ALModeSelect.IsChecked = false;
            _modeWarningCTS?.Cancel();
        }

        //used to verify answer as a valid input and so the program doesn't constantly create a new cultureinfo (don't want to use CurrentCulture either because idk)
        private static CultureInfo _cultureInfo = new CultureInfo("en-US");

        private Dictionary<TextBox, CancellationTokenSource> _cancellationTokens;
        private Dictionary<TextBox, CancellationTokenSource> _warningCTS; //is this bad practice

        //used for the question submission
        private async void SubmitButtonPressed(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            //we will assume that it always is a button because... only a button's event uses this
            //so i am not putting a null check since it's a waste of space

            Debug.Assert(button != null, nameof(button) + " != null");
            TextBox input = _buttonToInputBox[button];
            CancelThisTasks(input);
            await AsyncCheckAnswer(input);
        }

        private void CancelThisTasks(TextBox input)
        {
            if (_warningCTS.TryGetValue(input, out var cts))
            {
                cts.Cancel();
                _warningCTS.Remove(input);
            }

            if (_cancellationTokens.TryGetValue(input, out var prevTokenSource))
            {
                prevTokenSource.Cancel();
                _cancellationTokens.Remove(input);
                input.Background.Opacity = 1.0;
                input.Background = Brushes.Transparent;
            }
        }

        private async void EnterOrReturnPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return && e.Key != Key.Enter)
                return;
            TextBox input = sender as TextBox;
            CancelThisTasks(input);
            await AsyncCheckAnswer(input);
        }

        private async Task AsyncCheckAnswer(TextBox input)
        {
            if (_warningCTS.TryGetValue(input, out var tempCTS))
            {
                tempCTS.Cancel();
                _warningCTS.Remove(input);
            }
            if (!int.TryParse(input.Text, NumberStyles.Currency, _cultureInfo,
                    out var givenAnswer)) //if the value is not in a num format
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                _warningCTS.Add(input, cts);
                await AsyncDisplayWarningMessage(_inputToWarningBox[input], cts.Token, "Input is not a number.");
                return;
            }

            AccountVal notGivenAcct = _notGivenAccts[input];
            int actualAnswer = notGivenAcct.Amount;
            if (givenAnswer == actualAnswer)
            {
                input.Background = new SolidColorBrush(Color.FromRgb(127, 255, 0));
                input.IsReadOnly = true;
                if (++_amountSolved == _notGivenAccts.Count) //if the user has successfully solved all questions
                {
                    //QuestionFinishedEvent?.Invoke(QuestionGrid, new QuestionFinishedEventArgs(new List<object>(){_amountSolved}));


                    AllQuestionsFinished();
                }

                if (_isIncomeStatementProblem)
                {
                    PutAnswerIncomeStatement(_incomeStatementValueTextBoxes[notGivenAcct], givenAnswer,
                        Brushes.YellowGreen);
                }
            }
            else //answer is incorrect
            {
                //IncorrectAnswerEvent?.Invoke(input, new IncorrectAnswerEventArgs(actualAnswer,givenAnswer));
                await AsyncIncorrectAnimation(input);
            }
        }

        //very crappy animation
        private async Task AsyncIncorrectAnimation(TextBox input)
        {
            input.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) { Opacity = 1.0 };
            CancellationTokenSource cts = new CancellationTokenSource();
            _cancellationTokens.Add(input, cts);
            if (await AsyncDecreaseOpacity(input, cts)) return;

            /*Console.Out.WriteLine("otu loop");*/
            while (input.Background.Opacity < 1.0)
            {
                input.Background.Opacity += .035;
                try
                {
                    await Task.Delay(25, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
            await AsyncDecreaseOpacity(input, cts);
        }

        private static async Task<bool>
            AsyncDecreaseOpacity(TextBox input, CancellationTokenSource cts) //consider adding a speed parameter
        {
            while (input.Background.Opacity > 0)
            {
                input.Background.Opacity -= .035;
                try
                {
                    await Task.Delay(25, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    input.Background.Opacity = 1.0;
                    return true;
                }
            }

            return false;
        }

        private async Task AsyncDisplayWarningMessage(TextBox warningBox, CancellationToken ct,
            string message = "Warning.")
        {
            warningBox.Text = message;
            warningBox.IsHitTestVisible = true;
            try
            {
                await Task.Delay(5000, ct);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            warningBox.IsHitTestVisible = false;
            warningBox.Text = "";
        }

        private void
            ShowAnswersButtonPressed(object sender,
                RoutedEventArgs e) //consider removing the params and encapsulating the event subscription with a delegate
        {
            ShowConfirmationButtons();
            ConfirmationTextBox.Text = "Are you sure you want to show the answers?";
            _showAnswerFlag = true;
        }

        private void ShowConfirmationButtons()
        {
            RestartQuitGrid.Visibility = ShowAnswersButton.Visibility =
                FunctionHelpButton.Visibility = BackButton.Visibility = Visibility.Hidden;
            YesButton.Visibility = NoButton.Visibility = ConfirmationTextBox.Visibility = Visibility.Visible;
        }

        private void HelpButtonPressed(object sender, RoutedEventArgs e)
        {
            _helpWindow = new HelpWindow();
            _helpWindow.Show();
        }

        private void QuitButtonPressed(object sender, RoutedEventArgs e)
        {
            ShowConfirmationButtons();
            ConfirmationTextBox.Text =
                "Are you sure you want to return to the main menu? Questions and answers will not be saved.";
            _backToMainFlag = true;
        }

        private void RestartButtonPressed(object sender, RoutedEventArgs e)
        {
            ShowConfirmationButtons();
            ConfirmationTextBox.Text = "Are you sure you want to restart?";
            _restartFlag = true;
        }

        //i am aware that this isn't ideal checking
        //todo: add configs to skip confirmations...
        //i'm not doing that unless this is given to someone for actual use
        private void YesConfirmationButtonPressed(object sender, RoutedEventArgs e)
        {
            if (_backToMainFlag)
            {
                if (_showAnswerFlag)
                {
                    ResetShowAnswerState();
                }

                ReturnToMainMenu();
                return;
            }

            if (_restartFlag)
            {
                if (_showAnswerFlag)
                {
                    ResetShowAnswerState();
                }

                //haha very funny you'd think this would be more complicated somehow
                //tfw 10% of the lines is random words....
                OpenSelectModeMenu();
                return;
            }

            if (_showAnswerFlag)
            {
                if (_isIncomeStatementProblem)
                {
                    foreach (TextBox inputTextBox in _inputTextBoxes)
                    {
                        AccountVal notGivenAcct = _notGivenAccts[inputTextBox];
                        int expectedAnswer = notGivenAcct.Amount;


                        inputTextBox.IsReadOnly = true;
                        if (Int32.TryParse(inputTextBox.Text, NumberStyles.Currency, _cultureInfo, out var answer)
                            && answer == expectedAnswer)
                            continue;
                        inputTextBox.Foreground = Brushes.DarkOrange;

                        inputTextBox.Text = expectedAnswer.ToString("C", _cultureInfo);

                        PutAnswerIncomeStatement(_incomeStatementValueTextBoxes[notGivenAcct], expectedAnswer,
                            Brushes.DarkOrange);
                    }
                }
                else
                {
                    foreach (TextBox inputTextBox in _inputTextBoxes)
                    {
                        int expectedAnswer = _notGivenAccts[inputTextBox].Amount;

                        inputTextBox.IsReadOnly = true;
                        if (Int32.TryParse(inputTextBox.Text, NumberStyles.Currency, _cultureInfo, out var answer)
                            && answer == expectedAnswer)
                            continue;
                        inputTextBox.Foreground = Brushes.YellowGreen;

                        inputTextBox.Text = expectedAnswer.ToString("C", _cultureInfo);
                    }
                }

                AllQuestionsFinished();
            }
        }

        private void PutAnswerIncomeStatement(TextBox target, int amount, Brush fontColor)
        {
            //assume it's there... no null check 
            target.Text = amount.ToString("C0");
            target.Foreground = fontColor;
        }

        private void ResetShowAnswerState()
        {
            _showAnswerFlag = false;
            foreach (TextBox inputTextBox in _inputTextBoxes)
            {
                inputTextBox.IsReadOnly = false;
                inputTextBox.Foreground = Brushes.Black;
                inputTextBox.Text = "";
            }
        }

        private void NoConfirmationButtonPressed(object sender, RoutedEventArgs e)
        {
            if (_backToMainFlag)
            {
                _backToMainFlag = false;
                HideConfirmationButtons();
                if (_questionsCompleteFlag) RestartQuitGrid.Visibility = Visibility.Visible;
                else ShowFunctionButtons();
                return;
            }

            if (_restartFlag)
            {
                _restartFlag = false;
                HideConfirmationButtons(); //i'm too lazy to extract this method and this is getting redundant plus i don't know if it's good practice with how much i use it
                if (_questionsCompleteFlag) RestartQuitGrid.Visibility = Visibility.Visible;
                else ShowFunctionButtons();
                return;
            }

            if (_showAnswerFlag)
            {
                _showAnswerFlag = false;
                HideConfirmationButtons();
                ShowFunctionButtons();
            }
        }

        private void ShowFunctionButtons()
        {
            ShowAnswersButton.Visibility =
                FunctionHelpButton.Visibility = BackButton.Visibility = Visibility.Visible;
        }

        private void HideConfirmationButtons()
        {
            YesButton.Visibility = NoButton.Visibility = ConfirmationTextBox.Visibility = Visibility.Hidden;
        }
    }
}