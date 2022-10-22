using System;
using System.Collections.Generic;
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

    public class RandomDateTime //from stackov erflow.com/a/ 262 63669
        //without the spaces because i don't like search engine indexing
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
        private bool _questionsCompleteFlag;
        private bool _isIncomeStatementProblem;
        
        private int _amountSolved;
        
        //there are 15 AccountVal in this list
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
   
        private AccountVal[] _accounts;
        private readonly List<AccountVal> _givenAccounts;
        
        private HelpWindow _helpWindow;

        
        private readonly Dictionary<Button, TextBox> _submitButtonToInputBox;
        //input text box to AccountVal
        private readonly Dictionary<TextBox, AccountVal> _notGivenAccts;
        private readonly Dictionary<TextBox, TextBox> _inputToWarningBox;
        private readonly List<TextBox> _inputTextBoxes;
        private readonly Dictionary<AccountVal, TextBox> _ISAcctToTB;
        //constant reference means we don't need the resizing capabilities of List
        private readonly TextBox[] _incomeStatementValueTextBoxes;
        
        private DateTime _fiscalYearStart, _fiscalYearEnd;
        private readonly RandomDateTime _randomDateTime;
        
        private void InitializeAccounts()
        {
            //sets accounts' values to a random amount using RandomNumberGenerator
            //Sales must be at least 3/4 of Purchases to prevent an extremely low and unrealistic gross profit
            _transportationIn = new AccountVal(500 + RandomNumberGenerator.GetInt32(500, 10000), true);
            _purchasesRetAndAllow = new AccountVal(RandomNumberGenerator.GetInt32(300, 8700), true);
            _purchasesDiscounts = new AccountVal(RandomNumberGenerator.GetInt32(300, 16500), true);
            _salesRetAndAllow = new AccountVal(RandomNumberGenerator.GetInt32(300, 14750), true);
            _salesDiscounts = new AccountVal(RandomNumberGenerator.GetInt32(300, 13440), true);
            _beginningInventory = new AccountVal(RandomNumberGenerator.GetInt32(10000, 30000), true);
            _endingInventory = new AccountVal(RandomNumberGenerator.GetInt32(10000, 30000), true);
            _purchases = new AccountVal(RandomNumberGenerator.GetInt32(80000, 300000), true);
            _sales = new AccountVal(RandomNumberGenerator.GetInt32(_purchases.Amount * 3 / 4, 375000), true);

            //these AccountVals are based on the previous set values
            _costOfDeliveredMerchandise = _purchases + _transportationIn;
            _netPurchases = _costOfDeliveredMerchandise - _purchasesRetAndAllow - _purchasesDiscounts;
            _netSales = _sales - _salesDiscounts - _salesRetAndAllow;
            _costOfMerchandiseAvaForSale = _beginningInventory + _netPurchases;
            _costOfMerchandiseSold = _costOfMerchandiseAvaForSale - _endingInventory;
            _grossProfit = _netSales - _costOfMerchandiseSold;

            //setting up substitutes
            _purchases.Substitutes.Add(new[] { _transportationIn, _costOfDeliveredMerchandise });
            _transportationIn.Substitutes.Add(new[] { _purchases, _costOfDeliveredMerchandise });
            _costOfDeliveredMerchandise.Substitutes.Add(new[] { _transportationIn, _purchases });
            _costOfDeliveredMerchandise.Substitutes.Add(new[] { _purchasesRetAndAllow, _purchasesDiscounts, _netPurchases });
            _purchasesRetAndAllow.Substitutes.Add(new[] { _costOfDeliveredMerchandise, _purchasesDiscounts, _netPurchases });
            _purchasesDiscounts.Substitutes.Add(new[] { _costOfDeliveredMerchandise, _purchasesRetAndAllow, _netPurchases });
            _netPurchases.Substitutes.Add(new[] { _costOfDeliveredMerchandise, _purchasesRetAndAllow, _purchasesDiscounts });
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
            
            //setting the name of each AccountVal
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
            
            //Add them to the list containing all of the AccountVals
            _accounts = new[]
            {
                _beginningInventory, _costOfDeliveredMerchandise, _costOfMerchandiseSold,
                _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit, _netPurchases, _netSales, _purchases,
                _purchasesDiscounts, _purchasesRetAndAllow, _sales, _salesDiscounts, _salesRetAndAllow, 
                _transportationIn
            };
            
            //priming read to ensure at least one account needs to be solved for
            List<AccountVal> tempArr = _accounts.ToList();
            int tempRandIndex = RandomNumberGenerator.GetInt32(0, tempArr.Count);
            AccountVal tempRand = tempArr[tempRandIndex];
            tempRand.SolvingSet = true;
            tempArr.RemoveAt(tempRandIndex);
            while (tempArr.Count > 0)
            {
                int rand = RandomNumberGenerator.GetInt32(0, tempArr.Count);
                if (!tempArr[rand].SolvingSet)
                {
                    SetSolveStatesAlternative(tempArr[rand]);
                }
                tempArr.RemoveAt(rand);
            }
        }
        
        //returns true if the account needs to be solved for, false if it does not
        //assumes all NeedsSolving values are initially false
        [Obsolete(
            "Consider using setSolveStatesAlternative(AccountVal). This method assumes that initial value of NeedsSolving is false.")]
        private bool SetSolveStates(AccountVal a)
        {
            if (a.SolvingSet)
                return a.NeedsSolving;
            a.Visiting = true;
            
            //method may not function with this commented out
            //randomize this because otherwise it results in the same accounts being set to needs solving
            if (RandomNumberGenerator.GetInt32(1,4)==2) //1/3 chance to set the account value as given
            {
                //this value does not need to be solved for and will be given in the problem
                a.NeedsSolving = false;
                a.SolvingSet = true;
                a.Visiting = false;
                return true;
            }
            //check if it can be set to unsolved state
            foreach (var substituteArr in a.Substitutes)
            {
                //check to prevent infinite recursions
                if (substituteArr.Any(accountVal => accountVal.Visiting))
                    continue;
                
                //if any substitute is not solvable check the next substitute group
                if (substituteArr.Any(account => !account.IsSolvable())) continue;
                a.NeedsSolving = true;
                a.SolvingSet = true;
                a.Visiting = false;
                return false;
            }

            //reaching this point means the program could not find a suitable substitute with the given substitutes
            a.NeedsSolving = false;
            a.SolvingSet = true;
            a.Visiting = false;
            return true;
        }

        //assumes all NeedsSolving values are initially true
        //results in 5-6 questions
        private void SetSolveStatesAlternative(AccountVal a)
        {
            if (a.SolvingSet) return;
            a.Visiting = true;

            //check if it can be set to unsolved state
            if (a.Substitutes.Any(substituteArr => substituteArr.All(substitute => substitute.IsSolvable())))
            {
                a.NeedsSolving = true;
                a.SolvingSet = true;
                a.Visiting = false;
                return;
            }

            //reaching this point means the program could not find a suitable substitute with the given substitutes
            a.NeedsSolving = false;
            a.SolvingSet = true;
            a.Visiting = false;
        }

        public MainWindow()
        {
            //with 15 accounts, and 5-6 almost always being in the questions, this only needs 11 slots
            _givenAccounts = new List<AccountVal>(11);
            _submitButtonToInputBox = new Dictionary<Button, TextBox>(6); 
            _notGivenAccts = new Dictionary<TextBox, AccountVal>(6);
            _inputToWarningBox = new Dictionary<TextBox, TextBox>(6);
            _cancellationTokens = new Dictionary<TextBox, CancellationTokenSource>(6); 
            _warningCTS = new Dictionary<TextBox, CancellationTokenSource>(6);
            _inputTextBoxes = new List<TextBox>(6);
            _ISAcctToTB = new Dictionary<AccountVal, TextBox>(15);
            _randomDateTime = new RandomDateTime();
            
            
            InitializeComponent();
            _incomeStatementValueTextBoxes = new[]
            {
                BeginInventoryISValueTextBox, CoDMISValueTextBox, COMSISValueTextBox, COMAFSISValueTextBox,
                EndingInventoryISValueTextBox, GrossProfitISValueTextBox, NetPurchasesISValueTextBox, NetSalesISValueTextBox,
                PurchasesISValueTextBox, PurchasesDiscountsISValueTextBox, PurchasesReturnsISValueTextBox, SalesISValueTextBox,
                SalesDiscountsISValueTextBox, SalesReturnsISValueTextBox, TransportationInISValueTextBox
            };
            StartButton.Click += delegate {OpenSelectModeMenu();};
            ExitButton.Click += delegate {Application.Current.Shutdown();};
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
        
        private void OpenSelectModeMenu()
        {
            RestartQuitGrid.Visibility = StartGrid.Visibility = ShowAnswersTextBox.Visibility =
                FunctionButtonsGrid.Visibility = AllCorrectAnswersTextBox.Visibility = QuestionGrid.Visibility =
                IncomeStatementGrid.Visibility = AccountListGrid.Visibility = AccountListBorder.Visibility =
                    Visibility.Hidden;
            ModeSelectGrid.Visibility = Visibility.Visible;
        }

        private void ReturnToMainMenu()
        {
            StartGrid.Visibility = Visibility.Visible;
            FunctionGrid.Visibility = AccountListGrid.Visibility = AccountListBorder.Visibility =
                QuestionGrid.Visibility =
                    Visibility.Hidden;
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
            _submitButtonToInputBox.Clear();
            _notGivenAccts.Clear();
            _inputTextBoxes.Clear();
            AccountListGrid.RowDefinitions.Clear();
            AccountListGrid.Children.Clear();
            QuestionGrid.Children.Clear();
            QuestionGrid.RowDefinitions.Clear();
            _amountSolved = 0;
            _isIncomeStatementProblem = _showAnswerFlag = _restartFlag = _backToMainFlag = _questionsCompleteFlag =
                false;
            InitializeAccounts();
        }
        
        //adds all the information to the grid that holds the account information
        private void InitializeAccountList()
        {
            SetInitialState();
            foreach (AccountVal acct in _accounts)
                if (!acct.NeedsSolving)
                    AddToAccountList(acct);
                else
                    AddToQuestionList(acct);
            FixBottomBorderAList();
            FinishInitialization();
            AccountListGrid.Visibility = AccountListBorder.Visibility = Visibility.Visible;
        }

        private void FinishInitialization()
        {
            //add one last row definition to the QuestionGrid to reduce the sizes of the other rows
            QuestionGrid.RowDefinitions.Add(new RowDefinition()
                { Height = new GridLength(14 - QuestionGrid.RowDefinitions.Count, GridUnitType.Star) });
            RowDefinition endRow = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
            QuestionGrid.RowDefinitions.Add(endRow);
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
                _fiscalYearStart = _fiscalYearStart.AddMonths(1);
            }
            _fiscalYearEnd = _fiscalYearStart.AddYears(1).AddDays(-1);
            CompanyNameISTextBox.Text = GetRandomCompanyName();
            DateTextISBox.Text = $"For the Year Ended {_fiscalYearEnd:MMMM dd, yyyy}";
            _beginningInventory.Name = $"Inventory on {_fiscalYearStart:d}";
            _endingInventory.Name = $"Inventory on {_fiscalYearEnd:d}";
            BeginInventoryISNameTextBox.Text = $"\t{_beginningInventory.Name}";
            EndingInventoryISNameTextBox.Text = $"\t{_endingInventory.Name}";
            
            
            //first add the accounts to the dictionary correlating themselves to the income statement
            _ISAcctToTB.Add(_beginningInventory, BeginInventoryISValueTextBox);
            _ISAcctToTB.Add(_costOfDeliveredMerchandise, CoDMISValueTextBox);
            _ISAcctToTB.Add(_costOfMerchandiseSold, COMSISValueTextBox);
            _ISAcctToTB.Add(_costOfMerchandiseAvaForSale, COMAFSISValueTextBox);
            _ISAcctToTB.Add(_endingInventory, EndingInventoryISValueTextBox);
            _ISAcctToTB.Add(_grossProfit, GrossProfitISValueTextBox);
            _ISAcctToTB.Add(_netPurchases, NetPurchasesISValueTextBox);
            _ISAcctToTB.Add(_netSales, NetSalesISValueTextBox);
            _ISAcctToTB.Add(_purchases, PurchasesISValueTextBox);
            _ISAcctToTB.Add(_purchasesDiscounts, PurchasesDiscountsISValueTextBox);
            _ISAcctToTB.Add(_purchasesRetAndAllow, PurchasesReturnsISValueTextBox);
            _ISAcctToTB.Add(_sales, SalesISValueTextBox);
            _ISAcctToTB.Add(_salesDiscounts, SalesDiscountsISValueTextBox);
            _ISAcctToTB.Add(_salesRetAndAllow, SalesReturnsISValueTextBox);
            _ISAcctToTB.Add(_transportationIn, TransportationInISValueTextBox);

            //set the font color since it sometimes changes
            BeginInventoryISValueTextBox.Foreground = CoDMISValueTextBox.Foreground = COMSISValueTextBox.Foreground =
                COMAFSISValueTextBox.Foreground = EndingInventoryISValueTextBox.Foreground =
                GrossProfitISValueTextBox.Foreground = NetPurchasesISValueTextBox.Foreground =
                NetSalesISValueTextBox.Foreground = PurchasesISValueTextBox.Foreground =
                PurchasesISValueTextBox.Foreground = PurchasesDiscountsISValueTextBox.Foreground =
                PurchasesReturnsISValueTextBox.Foreground = SalesISValueTextBox.Foreground =
                SalesDiscountsISValueTextBox.Foreground = SalesReturnsISValueTextBox.Foreground =
                TransportationInISValueTextBox.Foreground = 
                    Brushes.Black;
            
            foreach (AccountVal acct in _accounts)
                if (!acct.NeedsSolving)
                    AddToIncomeStatement(acct);
                else
                    AddToQuestionList(acct);
            FinishInitialization();
            IncomeStatementGrid.Visibility = Visibility.Visible;
        }

        //adds the account to the Dictionary, _notGivenAccounts, and puts a question on the question grid
        private void AddToQuestionList(AccountVal acct)
        {
            QuestionGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            int rowIndex = QuestionGrid.RowDefinitions.Count - 1;
            TextBox questionBox = new TextBox
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                BorderThickness = new Thickness(0), TextWrapping = TextWrapping.Wrap, IsReadOnly = true,
                Text = $"What is the amount of {acct.Name}?"
            };
            TextBox inputBox = new TextBox
            {
                TextWrapping = TextWrapping.NoWrap, Margin = new Thickness(1, 0, 1, 0), CaretBrush = Brushes.Black,
                MinWidth = 125
            };
            inputBox.KeyDown += EnterOrReturnPressed;
            Button submitButton = new Button
            {
                Content = new TextBox()
                {
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                    TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, IsHitTestVisible = false,
                    IsReadOnly = true, Text = "Submit"
                }
            };
            submitButton.Click += SubmitButtonPressed;
            TextBox warningBox = new TextBox
            {
                TextWrapping = TextWrapping.Wrap, BorderThickness = new Thickness(0), FontSize = 10,
                Foreground = Brushes.Red, IsHitTestVisible = false, IsReadOnly = true
            };
            Grid inputGrid = new Grid
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
            QuestionGrid.Children.Add(submitButton);
            Grid.SetRow(questionBox, rowIndex);
            Grid.SetRow(inputGrid, rowIndex);
            Grid.SetRow(submitButton, rowIndex);
            Grid.SetColumn(questionBox, 0);
            Grid.SetColumn(inputGrid, 1);
            Grid.SetColumn(submitButton, 2);
            _notGivenAccts.Add(inputBox, acct);
            _submitButtonToInputBox.Add(submitButton, inputBox);
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
            Border boarder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = acctNameTextBox
            };
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
            Border acctValBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = acctValTextBox
            };
            AccountListGrid.Children.Add(acctValBorder);
            Grid.SetRow(acctValBorder, _givenAccounts.Count - 1);
            Grid.SetColumn(acctValBorder, 1);
        }

        private void AddToIncomeStatement(AccountVal acct)
        {
            TextBox incomeStatementValueTextBox = _ISAcctToTB[acct];
            incomeStatementValueTextBox.Text = acct.Amount.ToString("C0");
            /*incomeStatementValueTextBox.VerticalAlignment = VerticalAlignment.Bottom;*/
            _givenAccounts.Add(acct);
        }

        private string GetRandomCompanyName()
        {
            string name = "";
            name += CompanyNames[RandomNumberGenerator.GetInt32(0, CompanyNames.Length)];
            if (RandomNumberGenerator.GetInt32(1, 21) < 7)
            {
                name += $" {CompanyNames[RandomNumberGenerator.GetInt32(0, CompanyNames.Length)]}";
                if (RandomNumberGenerator.GetInt32(1, 21) == 1)
                {
                    name += $" {CompanyNames[RandomNumberGenerator.GetInt32(0, CompanyNames.Length)]}";
                }
            }
            name += $" {CompanyTypes[RandomNumberGenerator.GetInt32(0, CompanyTypes.Length)]}";
            return name;
        }

        //setting the border thickness properly for the bottom row. These should be the last two indices of the list
        private void FixBottomBorderAList()
        {
            int setCount = 0;
            for (var index = AccountListGrid.Children.Count - 1;
                 index >= 0;
                 index--) 
            {
                UIElement gridChild = AccountListGrid.Children[index];
                if (gridChild is not Border gridChildBorder ||
                    Grid.GetRow(gridChild) != AccountListGrid.RowDefinitions.Count - 1) continue;
                gridChildBorder.BorderThickness = new Thickness(0, 0, gridChildBorder.BorderThickness.Right, 0);
                if (++setCount == 2)
                    break;
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
        
        private void BackModeButtonPressed(object sender, RoutedEventArgs e)
        {
            ModeSelectGrid.Visibility = Visibility.Hidden;
            StartGrid.Visibility = Visibility.Visible;
            ISModeSelect.IsChecked = false;
            ALModeSelect.IsChecked = false;
            _modeWarningCTS?.Cancel();
        }
        
        private static readonly CultureInfo CultureInfo = new("en-US");

        private readonly Dictionary<TextBox, CancellationTokenSource> _cancellationTokens;
        private readonly Dictionary<TextBox, CancellationTokenSource> _warningCTS;
        
        private async void SubmitButtonPressed(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TextBox input = _submitButtonToInputBox[button];
            CancelThisTasks(input);
            await AsyncCheckAnswer(input);
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
            if (!int.TryParse(input.Text, NumberStyles.Currency, CultureInfo,
                    out var givenAnswer)) 
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
                if (_isIncomeStatementProblem)
                    PutAnswerIncomeStatement(_ISAcctToTB[notGivenAcct], givenAnswer, Brushes.YellowGreen);
                if (++_amountSolved == _notGivenAccts.Count)
                    AllQuestionsFinished();
            }
            else //answer is incorrect
                await AsyncIncorrectAnimation(input);
        }

        private void ShowAnswersButtonPressed(object sender, RoutedEventArgs e)
        {
            ShowConfirmationButtons();
            ConfirmationTextBox.Text = "Are you sure you want to show the answers?";
            _showAnswerFlag = true;
        }

        private void ShowConfirmationButtons()
        {
            RestartQuitGrid.Visibility = ShowAnswersButton.Visibility =
                FunctionHelpButton.Visibility = BackButton.Visibility =
                    Visibility.Hidden;
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
        
        private void YesConfirmationButtonPressed(object sender, RoutedEventArgs e)
        {
            if (_backToMainFlag)
            {
                if (_showAnswerFlag)
                    ResetShowAnswerState();
                ReturnToMainMenu();
                return;
            }

            if (_restartFlag)
            {
                if (_showAnswerFlag)
                    ResetShowAnswerState();
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
                        if (Int32.TryParse(inputTextBox.Text, NumberStyles.Currency, CultureInfo, out var answer)
                            && answer == expectedAnswer)
                            continue;
                        inputTextBox.Foreground = Brushes.DarkOrange;

                        inputTextBox.Text = expectedAnswer.ToString("C", CultureInfo);

                        PutAnswerIncomeStatement(_ISAcctToTB[notGivenAcct], expectedAnswer,
                            Brushes.DarkOrange);
                    }
                }
                else
                {
                    foreach (TextBox inputTextBox in _inputTextBoxes)
                    {
                        int expectedAnswer = _notGivenAccts[inputTextBox].Amount;

                        inputTextBox.IsReadOnly = true;
                        if (Int32.TryParse(inputTextBox.Text, NumberStyles.Currency, CultureInfo, out var answer)
                            && answer == expectedAnswer)
                            continue;
                        inputTextBox.Foreground = Brushes.YellowGreen;

                        inputTextBox.Text = expectedAnswer.ToString("C", CultureInfo);
                    }
                }

                AllQuestionsFinished();
            }
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

            if (_isIncomeStatementProblem)
            {
                foreach (TextBox textBox in _incomeStatementValueTextBoxes)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
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
                HideConfirmationButtons();
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
                FunctionHelpButton.Visibility = BackButton.Visibility =
                    Visibility.Visible;
        }

        private void HideConfirmationButtons()
        {
            YesButton.Visibility = NoButton.Visibility = ConfirmationTextBox.Visibility = Visibility.Hidden;
        }
        
        private void PutAnswerIncomeStatement(TextBox target, int amount, Brush fontColor)
        {
            //assume it's there... no null check 
            target.Text = amount.ToString("C0");
            target.Foreground = fontColor;
        }

        private async Task AsyncIncorrectAnimation(TextBox input)
        {
            input.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) { Opacity = 1.0 };
            CancellationTokenSource cts = new CancellationTokenSource();
            _cancellationTokens.Add(input, cts);
            if (await AsyncDecreaseOpacity(input, cts)) return;
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

        private static async Task<bool> AsyncDecreaseOpacity(TextBox input, CancellationTokenSource cts) 
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

        //placed at bottom for readability
        private static readonly string[] CompanyNames = {
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
            "Excess",
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
            "Apfel",
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
        //more of the ending word on the company name than the type
        private static readonly string[] CompanyTypes = {
            "Company",
            "LLC",
            "Corporation",
            "Incorporated",
            "Engineers",
            "Partnership",
            "Excavations",
            "Industries",
            "Construction",
            "Squad",
            "Antiques",
            "Consortium",
            "Burial Services",
            "Protection Services",
            "Research Laboratory",
            "Group",
            "Supply",
            "Airlines",
            "Airways"
        };
    }
}