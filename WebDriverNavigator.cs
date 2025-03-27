using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WebDriverNavigator : Form
{
    private IWebDriver _driver;
    private string _sessionId;
    private System.Windows.Forms.Timer _sessionTimer;
    private int _timeoutDuration = 300;
    private bool _sessionActive = false;

    private Button _startSessionBtn;
    private Button _truckBtn;
    private Button _refreshElementsBtn;
    private Button _openBtn;
    private Button _loginBtn;
    private NumericUpDown _timeoutInput;
    private Label _timerDisplay;
    private Label _sessionInfo;
    private TreeView _elementTree;

    public WebDriverNavigator()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "WebDriver GUI Navigator";
        this.Size = new Size(1000, 800);

        // Controls panel
        var controlsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(20)
        };

        _startSessionBtn = new Button
        {
            Text = "Start Session",
            Location = new Point(20, 20),
            AutoSize = true
        };
        _startSessionBtn.Click += StartSession_Click;

        _refreshElementsBtn = new Button
        {
            Text = "Refresh Elements",
            Location = new Point(150, 20),
            AutoSize = true,
            //Enabled = false
        };
        _refreshElementsBtn.Click += RefreshElements_Click;

        _truckBtn = new Button
        {
            Text = "Truck Transactions",
            Location = new Point(500, 20),
            AutoSize = true,
            //Enabled = false
        };
        _truckBtn.Click += gate_Click;

        _openBtn = new Button
        {
            Text = "Open (Ctrl+O)",
            Location = new Point(650, 20),
            AutoSize = true,
            //Enabled = false
        };
        _openBtn.Click += Open_Click;

        var timeoutLabel = new Label
        {
            Text = "Timeout (s):",
            Location = new Point(280, 23),
            AutoSize = true
        };

        _timeoutInput = new NumericUpDown
        {
            Value = 30,
            Width = 60,
            Location = new Point(360, 20)
        };

        _timerDisplay = new Label
        {
            Location = new Point(430, 23),
            AutoSize = true
        };

        _loginBtn = new Button
        {
            Text = "Login",
            Location = new Point(300, 20),
            AutoSize = true
        };
        _loginBtn.Click += Login_Click;

        controlsPanel.Controls.AddRange(new Control[] {
            _startSessionBtn, _refreshElementsBtn, _loginBtn, timeoutLabel, _timeoutInput, _timerDisplay, _truckBtn, _openBtn
        });

        // Session info
        _sessionInfo = new Label
        {
            Dock = DockStyle.Top,
            Text = "Session not started",
            Padding = new Padding(20),
            Height = 60,
            BackColor = SystemColors.ControlLight
        };

        // Element tree
        _elementTree = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            ShowNodeToolTips = true
        };

        this.Controls.AddRange(new Control[] {
            _elementTree, _sessionInfo, controlsPanel
        });

        // Initialize timer
        _sessionTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _sessionTimer.Tick += SessionTimer_Tick;
    }

    public class FlaUIDriverOptions : AppiumOptions
    {
        public static FlaUIDriverOptions ForApp(string path)
        {
            var options = new FlaUIDriverOptions()
            {
                PlatformName = "windows",
                AutomationName = "flaui",
                App = path
            };
            //options.AutomationName AddAdditionalAppiumOption("appium:automationName", "flaui");
            //options.AddAdditionalAppiumOption("appium:app", path);
            return options;
        }

        //public override ICapabilities ToCapabilities()
        //{
        //    return GenerateDesiredCapabilities(true);
        //}
    }


    private async void StartSession_Click(object sender, EventArgs e)
    {
        try
        {
            //var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            //_driver = new OpenQA.Selenium. Chrome.ChromeDriver(options);

            //var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Downloads\\CTOSTEST\\CTOS1TEST\\DIS\\CM.CTOS.WinUIAdmin.exe");
            var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Desktop\\DIS\\CM.CTOS.WinUIAdmin.exe");
            //_driver = new WindowsDriver(new Uri("http://localhost:5000"), opt);
            _driver = new WindowsDriver(new Uri("http://192.168.56.56:4723/"), opt);
            //_driver = new RemoteWebDriver(new Uri("http://localhost:5000"), FlaUIDriverOptions.ForApp("calc.exe"));

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            // _driver.FindElement(By.Id("txtUserName")).SendKeys("ahoy.abeltzac");
            // _driver.FindElement(By.Id("txtPassword")).SendKeys("Hunt93cexx33");
            // _driver.FindElement(By.Id("btnOK")).Click();



            _sessionId = ((OpenQA.Selenium.Remote.RemoteWebDriver)_driver).SessionId.ToString();
            _sessionActive = true;

            _sessionInfo.Text = $"Session ID: {_sessionId}";
            _startSessionBtn.Enabled = false;
            _refreshElementsBtn.Enabled = true;

            _timeoutDuration = (int)_timeoutInput.Value;
            ResetSessionTimer();
            await BuildElementTree();
        }
        catch (Exception ex)
        {
            _sessionInfo.Text = $"Error: {ex.Message}";
        }
    }

    private void ResetSessionTimer()
    {
        _sessionTimer.Stop();
        _timeoutDuration = (int)_timeoutInput.Value;
        UpdateTimerDisplay(_timeoutDuration);
        _sessionTimer.Start();
    }

    private void UpdateTimerDisplay(int secondsLeft)
    {
        var minutes = secondsLeft / 60;
        var seconds = secondsLeft % 60;
        _timerDisplay.Text = $"Time left: {minutes}:{seconds:D2}";
    }

    private void SessionTimer_Tick(object sender, EventArgs e)
    {
        _timeoutDuration--;
        UpdateTimerDisplay(_timeoutDuration);

        if (_timeoutDuration <= 0)
        {
            _sessionTimer.Stop();
            //EndSession("timed out");
        }
    }

    private void EndSession(string reason = "ended manually")
    {
        if (!_sessionActive) return;

        try
        {
            _driver?.Quit();
            _sessionInfo.Text = $"Session {_sessionId} {reason}.";
            _elementTree.Nodes.Clear();
            _timerDisplay.Text = "";
            _startSessionBtn.Enabled = true;
            //_refreshElementsBtn.Enabled = false;
            _sessionActive = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to end session: {ex.Message}");
        }
    }

    private async Task BuildElementTree()
    {
        //if (!_sessionActive) return;

        try
        {
            _elementTree.Nodes.Clear();
            var elements = _driver.FindElements(By.XPath("//*"));

            foreach (var element in elements)
            {
                try
                {
                    var location = element.Location;
                    var size = element.Size;
                    var enabled = element.Enabled;
                    var name = element.GetDomAttribute("Name");
                    var tagName = element.TagName;

                    var node = new TreeNode($"{name ?? "Unnamed"} ({tagName})")
                    {
                        Tag = element,
                        ToolTipText = $"ID: {element}\n" +
                                      $"Position: ({location.X}, {location.Y})\n" +
                                      $"Size: {size.Width}x{size.Height}\n" +
                                      $"Enabled: {enabled}"
                    };

                    // Add action buttons as child nodes
                    var clickNode = new TreeNode("Click") { Tag = new { Action = "click", Element = element } };
                    var setValueNode = new TreeNode("Set Value") { Tag = new { Action = "setValue", Element = element } };

                    node.Nodes.Add(clickNode);
                    node.Nodes.Add(setValueNode);

                    _elementTree.Nodes.Add(node);
                }
                catch (StaleElementReferenceException)
                {
                    continue;
                }
            }

            // Add event handler for node clicks
            _elementTree.NodeMouseClick += async (sender, e) =>
            {
                if (e.Node?.Tag is { } tag && tag.GetType().GetProperty("Action") != null)
                {
                    dynamic actionInfo = tag;
                    var element = (IWebElement)actionInfo.Element;
                    var action = (string)actionInfo.Action;

                    try
                    {
                        if (action == "click")
                        {
                            element.Click();
                            e.Node.BackColor = Color.LightGreen;
                        }
                        else if (action == "setValue")
                        {
                            var value = Microsoft.VisualBasic.Interaction.InputBox("Enter value:", "Set Value", "");
                            if (!string.IsNullOrEmpty(value))
                            {
                                element.SendKeys(value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error performing action: {ex.Message}");
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _elementTree.Nodes.Add(new TreeNode($"Error loading elements: {ex.Message}"));
        }
    }

    private async void RefreshElements_Click(object sender, EventArgs e)
    {
        var win = _driver.WindowHandles;
        _driver.SwitchTo().Window(win.First());
        await BuildElementTree();
        ResetSessionTimer();
    }

    private void Open_Click(object sender, EventArgs e)
    {
        try
        {
            var gateBtn = _driver.FindElement(By.Name("Gate Processing"));

            //if (gateBtn.Displayed)
            gateBtn.Click();

            if (_driver != null)
            {
                _driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Control + "o");

                new Actions(_driver)
                    .SendKeys("Truck Transaction")
                    .SendKeys(OpenQA.Selenium.Keys.ArrowDown)
                    .SendKeys(OpenQA.Selenium.Keys.Enter)
                    .SendKeys(OpenQA.Selenium.Keys.Enter)
                    .SendKeys(OpenQA.Selenium.Keys.Enter)
                    .Perform();



                //_driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Control + "o");
            }

           // var confirmBtn = _driver.FindElement(By.Name("Confirm"));

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending Ctrl+O: {ex.Message}");
        }
    }

    private async void gate_Click(object sender, EventArgs e)
    {
        try
        {
            var win = _driver.WindowHandles;
            _driver.SwitchTo().Window(win.First());

            var gateBtn = _driver.FindElement(By.Name("Gate Processing"));

            //if (gateBtn.Displayed)
            gateBtn.Click();

            //WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            //wait.Until(d => truckBtn.Displayed);

            var truckBtn = _driver.FindElement(By.Name("Truck Transaction"));

            //if (truckBtn.Displayed)
            truckBtn.Click();
        }
        catch(Exception ex)
        {
            MessageBox.Show($"Error performing action: {ex.Message}");
        }
    }

    private void Login_Click(object sender, EventArgs e)
    {
        var win = _driver.WindowHandles;
        _driver.SwitchTo().Window(win.First());

        try
        {
            _driver.FindElement(By.Id("txtUserName")).Click();
            //_driver.FindElement(By.Name("Login")).Click();

            _driver.FindElement(By.Id("txtUserName")).SendKeys("ahoy.abeltzac");
            _driver.FindElement(By.Id("txtPassword")).SendKeys("Hunt93cexx33");
            _driver.FindElement(By.Id("btnOK")).Click();

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(d => _driver.WindowHandles.Count > 0);

            //_driver.SwitchTo().Window("CTOS DIS");
            // _driver.SwitchTo().DefaultContent();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login failed: {ex.Message}");
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WebDriverNavigator());
    }
}