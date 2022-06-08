using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

namespace AcctISGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //changed to a class from struct because we want shallow copies
    public class AccountVal
    {
        public int amount;

        public bool Visiting, //this value is used mainly when setting needsSolving
            NeedsSolving, //as in, this value was not given in the initial problem
            SolvingSet; //this value is used when setting needsSolving

        public List<AccountVal[]> substitutes;
        public string Name { get; set; }

        public AccountVal()
        {
            Name = "Not set";
            substitutes = new List<AccountVal[]>(3);
            amount = 0;
            SolvingSet = NeedsSolving = false;
            _isSolvableVisiting = Visiting = false;
        }

        public AccountVal(int amount)
        {
            Name = "Not set";
            substitutes = new List<AccountVal[]>(3); //did 3 because i am lazy
            this.amount = amount;
            SolvingSet = NeedsSolving = false; //defaulting
            _isSolvableVisiting = Visiting = false;
        }

        public AccountVal(int amount, bool needsSolving)
        {
            Name = "Not set";
            substitutes = new List<AccountVal[]>(3);
            this.amount = amount;
            SolvingSet = this.NeedsSolving = needsSolving;
            _isSolvableVisiting = Visiting = false;
        }

        private bool _isSolvableVisiting;

        public bool IsSolvableRecursive()
        {
            _isSolvableVisiting = true;
            if (NeedsSolving)
            {
                _isSolvableVisiting = false;
                return true;
            }
            //iterate through substitutes

            //mmm it went past the forbidden line in my ide so i split it up a bit
            if ((from substituteGroup in substitutes
                    where !substituteGroup.Any(x => x._isSolvableVisiting)
                    select substituteGroup.All(
                        t => t is not null && t.IsSolvableRecursive())).Any(groupSolvable => groupSolvable))
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
            return (this.NeedsSolving && this.SolvingSet) ||
                   substitutes.Any(subGroup => subGroup.All(acct => acct.NeedsSolving && acct.SolvingSet));
        }

        public bool UsableSubstitute() //Essentially IsSolvableRecursive, but it doesn't check itself for anything
        {
            _isSolvableVisiting = true;
            if ((from substituteGroup in substitutes
                    where !substituteGroup.Any(x => x._isSolvableVisiting)
                    select substituteGroup.All(
                        t => t is not null && t.IsSolvableRecursive())).Any(groupSolvable => groupSolvable))
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
        public static AccountVal operator +(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.amount + b.amount);
        }

        public static AccountVal operator -(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.amount - b.amount);
        }

        public static AccountVal operator /(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.amount / b.amount);
        }

        public static AccountVal operator *(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.amount * b.amount);
        }

        public static AccountVal operator %(AccountVal a, AccountVal b)
        {
            return new AccountVal(a.amount % b.amount);
        }

        public static AccountVal operator +(AccountVal a, int b)
        {
            return new AccountVal(a.amount + b);
        }

        public static AccountVal operator -(AccountVal a, int b)
        {
            return new AccountVal(a.amount - b);
        }

        public static AccountVal operator /(AccountVal a, int b)
        {
            return new AccountVal(a.amount / b);
        }

        public static AccountVal operator *(AccountVal a, int b)
        {
            return new AccountVal(a.amount * b);
        }

        public static AccountVal operator %(AccountVal a, int b)
        {
            return new AccountVal(a.amount % b);
        }

        public static AccountVal operator +(int b, AccountVal a)
        {
            return new AccountVal(a.amount + b);
        }

        public static AccountVal operator -(int b, AccountVal a)
        {
            return new AccountVal(b - a.amount);
        }

        public static AccountVal operator /(int b, AccountVal a)
        {
            return new AccountVal(b / a.amount);
        }

        public static AccountVal operator *(int b, AccountVal a)
        {
            return new AccountVal(a.amount * b);
        }

        public static AccountVal operator %(int b, AccountVal a)
        {
            return new AccountVal(b % a.amount);
        }

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

        public static bool operator !=(int b, AccountVal a)
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

        public static bool operator >=(int b, AccountVal a)
        {
            return a.amount >= b;
        }

        public static bool operator <=(int b, AccountVal a)
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

        public static bool operator >(int b, AccountVal a)
        {
            return b > a.amount;
        }

        public static bool operator <(int b, AccountVal a)
        {
            return b < a.amount;
        }
    }

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
    
    public partial class MainWindow : Window
    {
        private bool _showAnswerFlag = false;
        private bool _restartFlag = false;
        private bool _backToMainFlag = false;
        
        //used to determine whether to set the restart/quit buttons visible or the show answers/help/quit buttons
        private bool _questionsCompleteFlag = false;
        
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
        private AccountVal _beginningInventory, _costOfDeliveredMerchandise,_costOfMerchandiseSold,
            _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit,
            _netPurchases,_netSales,_purchases,
            _purchasesDiscounts,_purchasesRetAndAllow,_sales,
            _salesDiscounts,_salesRetAndAllow,_transportationIn;
        //15 "accounts" (i am aware that cost of delivered merchandise and coms are not accounts) total

        //i'd like to have better worded questions beyond "What is amount of x" but, not necessary not gonna t odo this
        /*private static Dictionary<string, string> accountQuestions = new Dictionary<string, string>()
        {
            {"Beginning Inventory", ""  }   
        };*/

        private AccountVal[] _accounts;
        private List<AccountVal> _givenAccounts;
        private int _amountSolved;
        
        //this is separate so we can use it for the button checking
        //key will be the associated Button (in relation to the question and what account is being asked for)
        //...I'm not sure what else to do...
        private Dictionary<Button, TextBox> _buttonToInputBox;
        
        //key is the input textbox and it gives the AccountVal obj associated with it
        private Dictionary<TextBox, AccountVal> _notGivenAccts; 
        private Dictionary<TextBox, TextBox> _inputToWarningBox;
        private List<TextBox> _inputTextBoxes;

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
            
            _purchases = new AccountVal(RandomNumberGenerator.GetInt32(80000,300000),false);
           
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
            _accounts = new[]
            {
                _beginningInventory, _costOfDeliveredMerchandise, _costOfMerchandiseSold,
                _costOfMerchandiseAvaForSale, _endingInventory, _grossProfit, _netPurchases, _netSales, _purchases,
                _purchasesDiscounts, _purchasesRetAndAllow, _sales, _salesDiscounts, _salesRetAndAllow,
                _transportationIn
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
                        setSolveStates(tempArr[rand]);

                    }

                    tempArr.RemoveAt(rand);
                }
            } while (_accounts.All(acct => !acct.NeedsSolving)); //in the event that we do have all account values given we want to redo the thing
        }

        //returns true if the account needs to be solved for, false if it does not
        private bool setSolveStates(AccountVal a)
        {
            if (a.SolvingSet)
                return a.NeedsSolving;
            a.Visiting = true;
            
            //randomize this because otherwise it results in the same accounts being set to needs solving
            /*if (RandomNumberGenerator.GetInt32(1,4)==2) //1/3 chance to set the account value as given
            {
                //this value does not need to be solved for and will be given in the problem
                a.needsSolving = false;
                a.solvingSet = true;
                a.visiting = false;
                return true;
            }*/
            //check if it can be set to unsolved state
            foreach (var substituteArr in a.substitutes)
            {
                //check to prevent infinite recursions
                if (substituteArr.Any(accountVal => accountVal.Visiting))
                    break;
                    
                bool hasSolution = true; //
                for (var j = 0; j < substituteArr.Length; j++)
                {
                    AccountVal account = substituteArr[j];
                    //if the value needs to be solved for (this will check recursively, ofcourse)
                    //using account.isSolvable() results in only one account being unsolved... so that part of the program may be broken
                    if (setSolveStates(account))
                    {
                        hasSolution = false;
                        break;
                    }
                }
                //if this substitute group could not be used to solve for this
                if (!hasSolution) continue;
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
        
        public MainWindow()
        {
            //i don't know the max unsolved accounts we can have, but I know you need at least 3 accounts to solve most things
            _givenAccounts = new List<AccountVal>(14); //the amount of accounts given should never be more than that (hopefully)
            /*_buttonToString = new Dictionary<Button, TextBox>(5); I do not think it is possible for the program to give more than 5 because of how bad my algorithm is*/
            _buttonToInputBox = new Dictionary<Button, TextBox>(5);
            _notGivenAccts = new Dictionary<TextBox, AccountVal>(5);
            _inputToWarningBox = new Dictionary<TextBox, TextBox>(5);
            _cancellationTokens =
                new Dictionary<TextBox, CancellationTokenSource>(10);
            _warningCTS = new Dictionary<TextBox, CancellationTokenSource>(10);
            _inputTextBoxes = new List<TextBox>(5);
            InitializeComponent();
            StartButton.Click += delegate {this.StartProgram();};
            ShowAnswersButton.Click += ShowAnswersButtonPressed;
            HelpButton.Click += HelpButtonPressed;
            FunctionHelpButton.Click += HelpButtonPressed;

            BackButton.Click += QuitButtonPressed;
            RestartQuitGridBackButton.Click += QuitButtonPressed;

            RestartButton.Click += RestartButtonPressed;
            YesButton.Click += YesConfirmationButtonPressed;
            NoButton.Click += NoConfirmationButtonPressed;
        }

        //i am thinking that this can be called to restart the thing too...?
        private void StartProgram()
        {
            RestartQuitGrid.Visibility = StartGrid.Visibility = Visibility.Hidden;
            
            //clear any potential remaining information from previous runs
            _givenAccounts.Clear();
            _buttonToInputBox.Clear();
            _notGivenAccts.Clear();
            AccountListGrid.RowDefinitions.Clear();
            AccountListGrid.Children.Clear();
            
            QuestionGrid.Children.Clear();
            
            _showAnswerFlag = _restartFlag = _backToMainFlag = _questionsCompleteFlag = false;
            
            InitializeAccounts();
            InitializeAccountList();
            
            //stuff
            FunctionGrid.Visibility = AccountListGrid.Visibility = AccountListBorder.Visibility = QuestionGrid.Visibility = Visibility.Visible;
            HideConfirmationButtons();
        }

        private void ReturnToMainMenu() //idk what else this needs, but it doesn't feel complete?
        {
            StartGrid.Visibility = Visibility.Visible;
            
            FunctionGrid.Visibility = AccountListGrid.Visibility = AccountListBorder.Visibility = QuestionGrid.Visibility = Visibility.Hidden;
            HideConfirmationButtons();
        }

        private void AllQuestionsFinished()
        {
            _questionsCompleteFlag = true;
            HideConfirmationButtons();
            RestartQuitGrid.Visibility = Visibility.Visible;
        }

        //adds all the information to the grid that holds the account information
        private void InitializeAccountList()
        {
            _amountSolved = 0;
            //_accounts is always alphabetically sorted so we shouldn't need to care about resorting this in the grid
            foreach (AccountVal acct in _accounts)
                if (!acct.NeedsSolving) 
                    AddToAccountList(acct);
                else
                    AddToQuestionList(acct);
            
            //since the questions have no border, this does not affect them
            FixBottomBorderAList();
            
            //we add one last row definition to the QuestionGrid to help with the sizes of the other rows
            QuestionGrid.RowDefinitions.Add(new RowDefinition(){Height = new GridLength(14-QuestionGrid.RowDefinitions.Count,GridUnitType.Star)});
            RowDefinition endRow = new RowDefinition(){Height = new GridLength(1,GridUnitType.Star)};
            QuestionGrid.RowDefinitions.Add(endRow);
            //personally, 15* looked most appealing to me... but that was before I added a second grid below it
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
            TextBox inputBox = new TextBox() { TextWrapping = TextWrapping.NoWrap, Margin = new Thickness(1,0,1,0)};
            inputBox.KeyDown += EnterOrReturnPressed;
            Button submitButton = new Button() { Content = new TextBox(){Background = Brushes.Transparent, BorderThickness = new Thickness(0),TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, IsReadOnly = true, Text = "Submit"} };
            submitButton.Click += SubmitButtonPressed;
            TextBox warningBox = new TextBox()
            {
                TextWrapping = TextWrapping.Wrap, BorderThickness = new Thickness(0), FontSize = 10,
                Foreground = Brushes.Red
            };

            Grid inputGrid = new Grid(){
                RowDefinitions =
                {
                    new RowDefinition(){Height = new GridLength(3,GridUnitType.Star)},
                    new RowDefinition(){Height = new GridLength(3,GridUnitType.Star)}
                },
                Children = { inputBox, warningBox }
            };
            Grid.SetRow(inputBox,0);
            Grid.SetRow(warningBox,1);

            QuestionGrid.Children.Add(inputGrid);
            QuestionGrid.Children.Add(questionBox);
            //QuestionGrid.Children.Add(inputBox);
            QuestionGrid.Children.Add(submitButton);
            Grid.SetRow(questionBox, rowIndex);
            Grid.SetRow(inputGrid,rowIndex);
            //Grid.SetRow(inputBox, rowIndex);
            Grid.SetRow(submitButton, rowIndex);
            Grid.SetColumn(questionBox, 0);
            //Grid.SetColumn(inputBox, 1);
            Grid.SetColumn(inputGrid,1);
            Grid.SetColumn(submitButton, 2);
            
            _notGivenAccts.Add(inputBox,acct);
            _buttonToInputBox.Add(submitButton, inputBox);
            _inputToWarningBox.Add(inputBox,warningBox);
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
                Text = acct.amount.ToString("N"),

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

        //used to verify answer as a valid input and so the program doesn't constantly create a new cultureinfo (don't want to use CurrentCulture either because idk)
        private static CultureInfo _cultureInfo = new CultureInfo("en-US");
        
        private Dictionary<TextBox, CancellationTokenSource> _cancellationTokens;
        private Dictionary<TextBox, CancellationTokenSource> _warningCTS; //is this bad practice
        private async void SubmitButtonPressed(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
           
            //we will assume that it always is a button because... only a button's event uses this
            //so i am not putting a null check since it's a waste of space
            
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
            if (!Int32.TryParse(input.Text,NumberStyles.Currency, _cultureInfo, out var givenAnswer)) //if the value is not in a num format
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                _warningCTS.Add(input,cts);
                await AsyncDisplayWarningMessage(_inputToWarningBox[input], cts.Token, "Input is not a number.");
                return;
            }

            int actualAnswer = _notGivenAccts[input].amount;
            if (givenAnswer == actualAnswer)
            {
                input.Background = new SolidColorBrush(Color.FromRgb(127, 255, 0));
                input.IsReadOnly = true;
                if (++_amountSolved == _notGivenAccts.Count) //if the user has successfully solved all questions
                {
                    //QuestionFinishedEvent?.Invoke(QuestionGrid, new QuestionFinishedEventArgs(new List<object>(){_amountSolved}));
                    

                    AllQuestionsFinished();
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

            Console.Out.WriteLine("otu loop");
            while (input.Background.Opacity < 1.0)
            {
                input.Background.Opacity += .035;
                try
                {
                    await Task.Delay(25, cts.Token);
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    return;
                }
            }

            if(await AsyncDecreaseOpacity(input,cts)) return;
        }

        private static async Task<bool> AsyncDecreaseOpacity(TextBox input, CancellationTokenSource cts) //consider adding a speed parameter
        {
            while (input.Background.Opacity > 0)
            {
                input.Background.Opacity -= .035;
                try
                {
                    await Task.Delay(25, cts.Token);
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task AsyncDisplayWarningMessage(TextBox warningBox, CancellationToken ct, string message="Warning.")
        {
            warningBox.Text = message;
            try
            {
                await Task.Delay(5000, ct);
            }
            catch (TaskCanceledException e)
            {
                return;
            }

            warningBox.Text = "";
        }

        private void ShowAnswersButtonPressed(object sender, RoutedEventArgs e) //consider removing the params and encapsulating the event subscription with a delegate
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
            //todo: this should display a new window (oh boy this should be fun making another xaml window from scratch
        }

        private void QuitButtonPressed(object sender, RoutedEventArgs e)
        {
            ShowConfirmationButtons();
            ConfirmationTextBox.Text = "Are you sure you want to show the return to the main menu? Questions and answers will not be saved.";
            _backToMainFlag = true;
        }

        private void RestartButtonPressed(object sender, RoutedEventArgs e)
        {
            ShowConfirmationButtons();
            ConfirmationTextBox.Text = "Are you sure you want to restart?";
            _restartFlag = true;
        }
        
        //i am aware that this isn't ideal checking
        //todo: add configs to skip confirmations
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
                //this method resets all flags, so we don't need to change the flags here
                StartProgram();
                return;
            }
            
            if (_showAnswerFlag)
            {
                foreach (TextBox inputTextBox in _inputTextBoxes)
                {
                    inputTextBox.IsReadOnly = true;
                    inputTextBox.Foreground = Brushes.YellowGreen;
                    inputTextBox.Text = _notGivenAccts[inputTextBox].amount.ToString("C",_cultureInfo);
                }
                AllQuestionsFinished();
                return;
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
        }

        private void NoConfirmationButtonPressed(object sender, RoutedEventArgs e)
        {
            

            if (_backToMainFlag)
            {
                _backToMainFlag = false;
                HideConfirmationButtons();
                if (_questionsCompleteFlag) RestartQuitGrid.Visibility = Visibility.Visible;
                else
                {
                    ShowFunctionButtons();
                    Console.Out.WriteLine("test");
                }
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
                return;
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