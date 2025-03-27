using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

public class WebDriverNavigator : Form
{
    // Session management
    private IWebDriver _driver;
    private string _sessionId;
    private bool _sessionActive = false;
    private System.Windows.Forms.Timer _sessionTimer;
    private int _timeoutDuration = 300;

    // UI Controls
    private RichTextBox _logTextBox;
    private TreeView _elementTree;
    private Label _sessionInfo;
    private Label _timerDisplay;

    // Buttons
    private Button _startSessionBtn;
    private Button _refreshElementsBtn;
    private Button _loginBtn;
    private Button _truckBtn;
    private Button _openBtn;
    private NumericUpDown _timeoutInput;

    public WebDriverNavigator()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Utils.WebDriverExtensions.LogAction = (message, color) => Log(message, color);

        this.Text = "WebDriver GUI Navigator";
        this.Size = new Size(1000, 800);

        // Main controls panel
        var controlsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(10)
        };

        // Session controls
        _startSessionBtn = new Button
        {
            Text = "Start Session",
            Location = new Point(10, 10),
            Size = new Size(120, 30)
        };
        _startSessionBtn.Click += StartSession_Click;

        _refreshElementsBtn = new Button
        {
            Text = "Refresh Elements",
            Location = new Point(140, 10),
            Size = new Size(120, 30)
        };
        _refreshElementsBtn.Click += RefreshElements_Click;

        // Timeout controls
        var timeoutLabel = new Label
        {
            Text = "Timeout (s):",
            Location = new Point(270, 15),
            AutoSize = true
        };

        _timeoutInput = new NumericUpDown
        {
            Value = 30,
            Width = 60,
            Location = new Point(340, 10)
        };

        _timerDisplay = new Label
        {
            Location = new Point(410, 15),
            AutoSize = true
        };

        // Action buttons
        // Action buttons row 1
        _loginBtn = new Button
        {
            Text = "Login",
            Location = new Point(490, 10),
            Size = new Size(80, 30)
        };
        _loginBtn.Click += Login_Click;

        _truckBtn = new Button
        {
            Text = "Truck",
            Location = new Point(580, 10),
            Size = new Size(80, 30)
        };
        _truckBtn.Click += gate_Click;

        _openBtn = new Button
        {
            Text = "Open",
            Location = new Point(670, 10),
            Size = new Size(80, 30)
        };
        _openBtn.Click += Open_Click;

        // Additional buttons (2 rows of 6)
        var button2 = new Button { Text = "Button 2", Location = new Point(10, 50), Size = new Size(100, 30) };
        button2.Click += (s, e) => { /* TODO: Implement */ };

        var button3 = new Button { Text = "Button 3", Location = new Point(120, 50), Size = new Size(100, 30) };
        button3.Click += (s, e) => { /* TODO: Implement */ };

        var button4 = new Button { Text = "Button 4", Location = new Point(230, 50), Size = new Size(100, 30) };
        button4.Click += (s, e) => { /* TODO: Implement */ };

        var button5 = new Button { Text = "Button 5", Location = new Point(340, 50), Size = new Size(100, 30) };
        button5.Click += (s, e) => { /* TODO: Implement */ };

        var button6 = new Button { Text = "Button 6", Location = new Point(450, 50), Size = new Size(100, 30) };
        button6.Click += (s, e) => { /* TODO: Implement */ };

        var button7 = new Button { Text = "Button 7", Location = new Point(560, 50), Size = new Size(100, 30) };
        button7.Click += (s, e) => { /* TODO: Implement */ };

        var button8 = new Button { Text = "Button 8", Location = new Point(10, 90), Size = new Size(100, 30) };
        button8.Click += (s, e) => { /* TODO: Implement */ };

        var button9 = new Button { Text = "Button 9", Location = new Point(120, 90), Size = new Size(100, 30) };
        button9.Click += (s, e) => { /* TODO: Implement */ };

        var button10 = new Button { Text = "Button 10", Location = new Point(230, 90), Size = new Size(100, 30) };
        button10.Click += (s, e) => { /* TODO: Implement */ };

        var button11 = new Button { Text = "Button 11", Location = new Point(340, 90), Size = new Size(100, 30) };
        button11.Click += (s, e) => { /* TODO: Implement */ };

        var button12 = new Button { Text = "Button 12", Location = new Point(450, 90), Size = new Size(100, 30) };
        button12.Click += (s, e) => { /* TODO: Implement */ };

        var button13 = new Button { Text = "Button 13", Location = new Point(560, 90), Size = new Size(100, 30) };
        button13.Click += (s, e) => { /* TODO: Implement */ };

        // Log panel
        // Log panel
        _logTextBox = new RichTextBox
        {
            Dock = DockStyle.Bottom,
            Height = 150,
            Multiline = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            ReadOnly = true
        };
        // Session info
        _sessionInfo = new Label
        {
            Dock = DockStyle.Top,
            Text = "Session not started",
            Padding = new Padding(10),
            Height = 40,
            BackColor = SystemColors.ControlLight
        };

        // Element tree
        _elementTree = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            ShowNodeToolTips = true
        };

        // Add controls to panels
        controlsPanel.Controls.AddRange(new Control[] {
            _startSessionBtn, _refreshElementsBtn, timeoutLabel, _timeoutInput,
            _timerDisplay, _loginBtn, _truckBtn, _openBtn,
            button2, button3, button4, button5, button6, button7,
            button8, button9, button10, button11, button12, button13
        });

        this.Controls.AddRange(new Control[] {
            _elementTree, _sessionInfo, controlsPanel, _logTextBox
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
        Log("Starting new session...");
        try
        {
            //var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            //_driver = new OpenQA.Selenium. Chrome.ChromeDriver(options);

            //var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Downloads\\CTOSTEST\\CTOS1TEST\\DIS\\CM.CTOS.WinUIAdmin.exe");
            var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Desktop\\DIS\\CM.CTOS.WinUIAdmin.exe");
            //_driver = new WindowsDriver(new Uri("http://localhost:5000"), opt);
            _driver = new WindowsDriver(new Uri("http://192.168.56.56:4723/"), opt);
            //_driver = new RemoteWebDriver(new Uri("http://localhost:5000"), FlaUIDriverOptions.ForApp("calc.exe"));

            //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            // _driver.FindElement(By.Id("txtUserName")).SendKeys("ahoy.abeltzac");
            // _driver.FindElement(By.Id("txtPassword")).SendKeys("Hunt93cexx33");
            // _driver.FindElement(By.Id("btnOK")).Click();



            //_sessionId = _driver.Manage(). SessionId.ToString();
            _sessionActive = true;

            _sessionInfo.Text = $"Session ID: {_sessionId}";
            _startSessionBtn.Enabled = false;
            _refreshElementsBtn.Enabled = true;
            Log("Session started successfully", Color.Green);

            _timeoutDuration = (int)_timeoutInput.Value;
            ResetSessionTimer();
            await BuildElementTree();
        }
        catch (Exception ex)
        {
            _sessionInfo.Text = $"Error: {ex.Message}";
            Log($"Session start failed: {ex.Message}");
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
            Log($"Ending session: {reason}");
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
            Log($"Failed to end session: {ex.Message}");
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

                    var node = new TreeNode($"{name ?? "Unnamed"} ({tagName}) - {element.GetAttribute("Value") ?? element.Text ?? "No value"}")
                    {
                        Tag = element,
                        ToolTipText = $"ID: {element}\n" +
                                      $"Value: {element.GetAttribute("Value") ?? element.Text ?? "None"}\n" +
                                      $"Position: ({location.X}, {location.Y})\n" +
                                      $"Size: {size.Width}x{size.Height}\n" +
                                      $"Enabled: {enabled}\n" +
                                      $"Visible: {element.Displayed}\n" +
                                      $"Selected: {element.Selected}"
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
                        Log($"ERROR: Error performing action: {ex.Message}", Color.Red);
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
        Log("Elements refreshed successfully", Color.Green);
    }

    private void Open_Click(object sender, EventArgs e)
    {
        try
        {
            var win = _driver.WindowHandles;
            _driver.SwitchTo().Window(win.First());

            //var gateBtn = _driver.FindElement(By.Name("Gate Processing"));

            ////if (gateBtn.Displayed)
            //gateBtn.Click();

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
           Log("Open operation completed successfully", Color.Green);



                //_driver.SwitchTo().ActiveElement().SendKeys(OpenQA.Selenium.Keys.Control + "o");
            }

           // var confirmBtn = _driver.FindElement(By.Name("Confirm"));

        }
        catch (Exception ex)
        {
            Log($"ERROR: Error sending Ctrl+O: {ex.Message}", Color.Red);
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
            Log("Truck transaction opened successfully", Color.Green);
        }
        catch(Exception ex)
        {
            Log($"ERROR: Error performing action: {ex.Message}", Color.Red);
        }
    }

    private void Login_Click(object sender, EventArgs e)
    {
        try
        {
            var win = _driver.WindowHandles;
            _driver.SwitchTo().Window(win.First());

            // Find elements with timeout
            var userNameField = _driver.FindElement(By.Id("txtUserName"), 10);
            var passwordField = _driver.FindElement(By.Id("txtPassword"), 10);
            var okButton = _driver.FindElement(By.Id("btnOK"), 10);

            // Wait for and interact with username field
            userNameField.WaitForElement(_driver);
            userNameField.Click();
            userNameField.SendKeys("ahoy.abeltzac");
            Log("Entered username", Color.Blue);

            // Wait for and interact with password field
            passwordField.WaitForElement(_driver);
            passwordField.SendKeys("Hunt93cexx33");
            Log("Entered password", Color.Blue);

            // Wait for and click OK button
            okButton.WaitForElement(_driver);
            okButton.Click();
            Log("Clicked OK button", Color.Blue);

            // Verify login success
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => _driver.WindowHandles.Count > 0);
            Log("Login successful", Color.Green);

            //_driver.SwitchTo().Window("CTOS DIS");
            // _driver.SwitchTo().DefaultContent();
        }
        catch (Exception ex)
        {
            Log($"ERROR: Login failed: {ex.Message}", Color.Red);
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WebDriverNavigator());
    }

    private void Log(string message, Color? color = null)
    {
        if (_logTextBox != null)
        {
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            if (color != null)
            {
                _logTextBox.SelectionColor = color.Value;
            }
            _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            _logTextBox.SelectionColor = _logTextBox.ForeColor;
        }
    }
}