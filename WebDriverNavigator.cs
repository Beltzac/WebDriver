using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    private Button _refreshElementsBtn;
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
            Enabled = false
        };
        _refreshElementsBtn.Click += RefreshElements_Click;

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

        controlsPanel.Controls.AddRange(new Control[] {
            _startSessionBtn, _refreshElementsBtn, timeoutLabel, _timeoutInput, _timerDisplay
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

    public class FlaUIDriverOptions : DriverOptions
    {
        public static FlaUIDriverOptions ForApp(string path)
        {
            var options = new FlaUIDriverOptions()
            {
                PlatformName = "windows"
            };
            options.AddAdditionalOption("appium:automationName", "flaui");
            options.AddAdditionalOption("appium:app", path);
            return options;
        }

        public override ICapabilities ToCapabilities()
        {
            return GenerateDesiredCapabilities(true);
        }
    }


    private async void StartSession_Click(object sender, EventArgs e)
    {
        try
        {
            //var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            //_driver = new OpenQA.Selenium. Chrome.ChromeDriver(options);

            _driver = new RemoteWebDriver(new Uri("http://localhost:5000"), FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Downloads\\CTOSTEST\\CTOS1TEST\\DIS\\CM.CTOS.WinUIAdmin.exe"));


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
            EndSession("timed out");
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
            _refreshElementsBtn.Enabled = false;
            _sessionActive = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to end session: {ex.Message}");
        }
    }

    private async Task BuildElementTree()
    {
        if (!_sessionActive) return;

        try
        {
            _elementTree.Nodes.Clear();
            var elements = _driver.FindElements(By.XPath("//*"));

            foreach (var element in elements)
            {
                var node = new TreeNode($"{element.TagName} - {element.Text}")
                {
                    Tag = element
                };

                try
                {
                    var location = element.Location;
                    var size = element.Size;
                    var enabled = element.Enabled;

                    node.ToolTipText = $"Position: ({location.X}, {location.Y})\n" +
                                       $"Size: {size.Width}x{size.Height}\n" +
                                       $"Enabled: {enabled}";
                }
                catch (StaleElementReferenceException)
                {
                    continue;
                }

                _elementTree.Nodes.Add(node);
            }
        }
        catch (Exception ex)
        {
            _elementTree.Nodes.Add(new TreeNode($"Error loading elements: {ex.Message}"));
        }
    }

    private async void RefreshElements_Click(object sender, EventArgs e)
    {
        await BuildElementTree();
        ResetSessionTimer();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WebDriverNavigator());
    }
}