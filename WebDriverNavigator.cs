using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

public class WebDriverNavigator : Form
{
    private IWebDriver _driver;
    private bool _sessionActive = false;

    private RichTextBox _logTextBox;
    private TreeView _elementTree;
    private Label _sessionInfo;
    private TextBox _usernameInput;
    private TextBox _passwordInput;
    private TextBox _xpathInput;

    // Buttons
    private Button _startSessionBtn;
    private Button _refreshElementsBtn;
    private Button _loginBtn;
    private Button _truckBtn;
    private Button _openBtn;
    private Button _containerManagementBtn;
    private Button _showcaseSequenceBtn;

    public WebDriverNavigator()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        WebDriverExtensions.LogAction = (message, color) => Log(message, color);

        this.Text = "POC - Automação CTOS - Selenium + FlaUI WebDriver";
        this.Size = new Size(1000, 800);
        this.Padding = new Padding(10);

        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 10)
        };

        _startSessionBtn = new Button { Text = "Iniciar Sessão", Size = new Size(120, 30), Margin = new Padding(5) };
        _startSessionBtn.Click += StartSession_Click;

        _refreshElementsBtn = new Button { Text = "Atualizar Elementos", Size = new Size(120, 30), Margin = new Padding(5) };
        _refreshElementsBtn.Click += RefreshElements_Click;

        var xpathLabel = new Label { Text = "XPath:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(10, 8, 0, 0) };
        _xpathInput = new TextBox { Text = "//Button", Width = 200, Margin = new Padding(5) };

        topPanel.Controls.AddRange(new Control[] { _startSessionBtn, _refreshElementsBtn, xpathLabel, _xpathInput });

        var middlePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(0, 0, 0, 10)
        };

        _loginBtn = new Button { Text = "Login", Size = new Size(80, 30), Margin = new Padding(5) };
        _loginBtn.Click += Login_Click;

        _truckBtn = new Button { Text = "Truck Transaction", Size = new Size(120, 30), Margin = new Padding(5) };
        _truckBtn.Click += gate_Click;

        _openBtn = new Button { Text = "Conteiner Size", Size = new Size(140, 30), Margin = new Padding(5) };
        _openBtn.Click += Open_Click;

        _containerManagementBtn = new Button { Text = "Conteiner Management", Size = new Size(200, 30), Margin = new Padding(5) };
        _containerManagementBtn.Click += async (s, e) => { await TryOpenMenuAsync("Management", "Management[CI002]"); };

        _showcaseSequenceBtn = new Button { Text = "Showcase Sequence", Size = new Size(150, 30), Margin = new Padding(5) };
        _showcaseSequenceBtn.Click += ShowcaseSequence_Click;

        middlePanel.Controls.AddRange(new Control[] { _loginBtn, _truckBtn, _openBtn, _containerManagementBtn, _showcaseSequenceBtn });

        var credentialsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Right,
            AutoSize = true,
            Width = 200,
            Padding = new Padding(10, 0, 0, 0)
        };

        var usernameLabel = new Label { Text = "Usuário:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
        _usernameInput = new TextBox { Width = 180, Margin = new Padding(0, 0, 0, 5) };

        var passwordLabel = new Label { Text = "Senha:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
        _passwordInput = new TextBox { Width = 180, UseSystemPasswordChar = true, Margin = new Padding(0, 0, 0, 5) };

        credentialsPanel.Controls.AddRange(new Control[] { usernameLabel, _usernameInput, passwordLabel, _passwordInput });

        _logTextBox = new RichTextBox
        {
            Dock = DockStyle.Bottom,
            Height = 300,
            Multiline = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            ReadOnly = true
        };

        _elementTree = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            ShowNodeToolTips = true,
            Scrollable = true
        };

        _sessionInfo = new Label { Text = "Sessão não iniciada", Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleLeft, Height = 20 };

        this.Controls.Add(_elementTree);
        this.Controls.Add(_sessionInfo);
        this.Controls.Add(_logTextBox);
        this.Controls.Add(credentialsPanel);
        this.Controls.Add(middlePanel);
        this.Controls.Add(topPanel);
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
            return options;
        }
    }


    private bool TryStartSession()
    {
        Log("Tentando iniciar sessão...", Color.Blue);
        try
        {
            if (_driver != null)
            {
                Log("Descartando sessão anterior do driver.", Color.Orange);
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
                _sessionActive = false;
            }

            var opt = FlaUIDriverOptions.ForApp("C:\\Users\\Beltzac\\Desktop\\DIS\\CM.CTOS.WinUIAdmin.exe");

            _driver = new WindowsDriver(new Uri("http://192.168.56.56:4723/"), opt, TimeSpan.FromSeconds(60));
            _sessionActive = true;


            if (_sessionInfo != null) _sessionInfo.Text = $"Sessão Ativa: Sim";
            Log("Sessão iniciada com sucesso.", Color.Green);
            return true;
        }
        catch (Exception ex)
        {
            if (_sessionInfo != null) _sessionInfo.Text = $"Erro ao iniciar: {ex.Message}";
            Log($"Falha ao iniciar sessão: {ex.Message}", Color.Red);
            _sessionActive = false;

            if (_driver != null)
            {
                try { _driver.Quit(); } catch { }
                try { _driver.Dispose(); } catch { }
                _driver = null;
            }

            return false;
        }
    }

    private bool TryLogin()
    {
        if (!_sessionActive || _driver == null)
        {
            Log("Login falhou: Sessão não ativa.", Color.Red);
            return false;
        }

        Log("Tentando login...", Color.Blue);
        try
        {
            _driver.ChangeToWindow("Login");

            Log($"Trocado para janela de login: {_driver.Title}", Color.Blue);

            var userNameField = _driver.FindElement(By.Id("txtUserName"), 10);
            var passwordField = _driver.FindElement(By.Id("txtPassword"), 10);
            var okButton = _driver.FindElement(By.Id("btnOK"), 10);

            userNameField.Clear();
            userNameField.SendKeys(_usernameInput.Text);
            Log("Usuário inserido.", Color.Blue);

            passwordField.Clear();
            passwordField.SendKeys(_passwordInput.Text);
            Log("Senha inserida.", Color.Blue);

            okButton.Click();
            Log("Botão OK clicado.", Color.Blue);

            _driver.ChangeToWindow("CTOS DIS", 20);

            Log($"Trocado para janela principal: {_driver.Title}", Color.Blue);
            Log("Login realizado com sucesso.", Color.Green);

            return true;
        }
        catch (Exception ex)
        {
            Log($"ERRO: Login falhou: {ex.Message}", Color.Red);
            try { Log($"Título da janela atual durante falha no login: {_driver?.Title}", Color.Orange); } catch { }
            return false;
        }
    }

    private bool TryOpenMenu(string menuSearchText, string expectedWindowTitlePart)
    {
        if (!_sessionActive || _driver == null)
        {
            Log($"Abrir Menu '{menuSearchText}' falhou: Sessão não ativa.", Color.Red);
            return false;
        }

        Log($"Tentando abrir menu: {menuSearchText}", Color.Blue);
        try
        {
            new Actions(_driver)
                .KeyDown(OpenQA.Selenium.Keys.Control)
                .SendKeys("o")
                .KeyUp(OpenQA.Selenium.Keys.Control)
                .Perform();

            Log("Enviado Ctrl+O", Color.Blue);

            new Actions(_driver)
                .SendKeys(menuSearchText)
                .SendKeys(OpenQA.Selenium.Keys.ArrowDown)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .SendKeys(OpenQA.Selenium.Keys.Enter)
                .Perform();

            Log($"Enviado texto de busca '{menuSearchText}' e Enter.", Color.Blue);

            _driver.ChangeToWindow(expectedWindowTitlePart);

            Log($"Menu aberto com sucesso: {_driver.Title}", Color.Green);
            return true;
        }
        catch (Exception ex)
        {
            Log($"ERRO: Falha ao abrir menu '{menuSearchText}': {ex.Message}", Color.Red);
             try { Log($"Título da janela atual durante falha ao abrir menu: {_driver?.Title}", Color.Orange); } catch { }
            return false;
        }
    }

     private bool TryParseDataPanel()
    {
         if (!_sessionActive || _driver == null)
        {
            Log("Analisar Painel de Dados falhou: Sessão não ativa.", Color.Red);
            return false;
        }
        Log("Tentando analisar Painel de Dados...", Color.Blue);
        var tableData = new List<Dictionary<string, string>>();
        try
        {
            _driver.SwitchTo().Window(_driver.WindowHandles.Last());
            Log($"Analisando painel de dados na janela: {_driver.Title}", Color.Blue);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var headerPanel = wait.Until(d => d.FindElement(By.XPath("//*[@Name='Header Panel']")));
            var dataPanel = wait.Until(d => d.FindElement(By.XPath("//*[@Name='Data Panel']")));

            var headers = headerPanel.FindElements(By.XPath("./*"))
                                     .Select(h => h.Text.Trim())
                                     .ToList();

            var rows = dataPanel.FindElements(By.XPath("./*"));

            if (!headers.Any())
            {
                 Log("Aviso: Cabeçalhos não encontrados no Painel de Dados.", Color.Orange);
            }
             if (!rows.Any())
            {
                 Log("Aviso: Linhas não encontradas no Painel de Dados.", Color.Orange);
            }

            Log("==== CONTEÚDO DO PAINEL DE DADOS ====", Color.Blue);
            Log($"Cabeçalhos: {string.Join(" | ", headers)}", Color.DarkBlue);

            foreach (var row in rows)
            {
                try
                {
                    var cellElements = row.FindElements(By.XPath("./*"));
                    var cells = cellElements.Select(c => c.Text.Trim()).ToList();


                    var rowData = new Dictionary<string, string>();
                    for (int i = 0; i < Math.Min(headers.Count, cells.Count); i++)
                    {
                        if (!string.IsNullOrWhiteSpace(headers[i]))
                        {
                             rowData[headers[i]] = cells[i];
                        }
                        else
                        {
                            Log($"Aviso: Cabeçalho vazio no índice {i}", Color.Orange);
                        }
                    }
                    tableData.Add(rowData);
                    Log($"Linha: {string.Join(" | ", rowData.Values)}", Color.DarkBlue);
                }
                catch (StaleElementReferenceException)
                {
                    Log("Pulando linha devido a StaleElementReferenceException.", Color.Orange);
                    continue;
                }
                catch (Exception ex)
                {
                    Log($"Erro ao analisar linha: {ex.Message}", Color.Red);
                }
            }
            Log("============================", Color.Blue);
            Log("Painel de Dados analisado com sucesso.", Color.Green);
            return true;
        }
        catch (Exception ex)
        {
            Log($"ERRO: Falha ao encontrar ou analisar Painel de Dados: {ex.Message}", Color.Red);
            try { Log($"Título da janela atual durante falha na análise: {_driver?.Title}", Color.Orange); } catch { }
            return false;
        }
    }



    private void StartSession_Click(object sender, EventArgs e)
    {
        if (TryStartSession())
        {
            Log("StartSession_Click: Sessão iniciada com sucesso via botão.", Color.Green);
        }
        else
        {
            Log("StartSession_Click: Falha ao iniciar sessão via botão.", Color.Red);
        }
    }

    private void Login_Click(object sender, EventArgs e)
    {
        if (TryLogin())
        {
             Log("Login_Click: Login realizado com sucesso via botão.", Color.Green);
        }
         else
        {
             Log("Login_Click: Falha no login via botão.", Color.Red);
        }
    }

     private async void Open_Click(object sender, EventArgs e)
    {
        if (await TryOpenMenuAsync("Size", "Size[RM022]"))
        {
            Log("Open_Click: Menu 'Tamanho do Contêiner' aberto com sucesso via botão.", Color.Green);

            if(TryParseDataPanel())
            {
                 Log("Open_Click: Painel de dados analisado com sucesso após abrir menu.", Color.Green);
            }
            else
            {
                 Log("Open_Click: Falha ao analisar painel de dados após abrir menu.", Color.Red);
            }
        }
         else
        {
             Log("Open_Click: Falha ao abrir menu 'Tamanho do Contêiner' via botão.", Color.Red);
        }
    }

    private async void gate_Click(object sender, EventArgs e)
    {
         if (await TryOpenMenuAsync("Truck Transaction", "Truck Transaction[ER025]"))
         {
              Log("gate_Click: Menu 'Transação de Caminhão' aberto com sucesso via botão.", Color.Green);
         }
         else
         {
              Log("gate_Click: Falha ao abrir menu 'Transação de Caminhão' via botão.", Color.Red);
         }
    }

    private async void ShowcaseSequence_Click(object sender, EventArgs e)
    {
        Log("Iniciando sequência de demonstração...", Color.Magenta);

        Log("Passo 1: Iniciando Sessão...", Color.Cyan);
        if (!TryStartSession())
        {
            Log("Passo 1 FALHOU. Abortando sequência.", Color.Red);
            return;
        }
        Log("Passo 1 SUCESSO.", Color.Green);

        Log("Passo 2: Realizando Login...", Color.Cyan);
        if (!TryLogin())
        {
            Log("Passo 2 FALHOU. Abortando sequência.", Color.Red);
            return;
        }
        Log("Passo 2 SUCESSO.", Color.Green);

        Log("Passo 3: Abrindo Menu 'Tamanho do Contêiner'...", Color.Cyan);
        if (!await TryOpenMenuAsync("Size", "Size[RM022]"))
        {
            Log("Passo 3 FALHOU. Abortando sequência.", Color.Red);
            return;
        }
        Log("Passo 3 SUCESSO.", Color.Green);

        Log("Passo 4: Analisando Painel de Dados...", Color.Cyan);
        if (!TryParseDataPanel())
        {
            Log("Passo 4 FALHOU. Sequência incompleta.", Color.Red);
        }
        else
        {
            Log("Passo 4 SUCESSO.", Color.Green);
        }

        Log("Sequência de demonstração finalizada.", Color.Magenta);
    }

    private async Task<bool> TryOpenMenuAsync(string menuSearchText, string expectedWindowTitlePart)
    {
        return await Task.Run(() => TryOpenMenu(menuSearchText, expectedWindowTitlePart));
    }

    private async void RefreshElements_Click(object sender, EventArgs e)
    {
         if (!_sessionActive || _driver == null)
        {
            Log("Atualizar Elementos falhou: Sessão não ativa.", Color.Red);
            return;
        }
        try
        {
            var xpath = string.IsNullOrWhiteSpace(_xpathInput.Text) ? "//*" : _xpathInput.Text;
            Log($"Atualizando elementos com XPath: {xpath}", Color.Blue);
            await Task.Run(() => BuildElementTree(xpath));
            Log($"Elementos atualizados com sucesso (XPath: {xpath})", Color.Green);
        }
        catch (Exception ex)
        {
            Log($"Erro ao atualizar elementos: {ex.Message}", Color.Red);
        }
    }

    private void BuildElementTree(string xpath)
    {
        if (!_sessionActive || _driver == null)
        {
            Log("Construir Árvore de Elementos falhou: Sessão não ativa.", Color.Red);
             _elementTree.Nodes.Clear();
             _elementTree.Nodes.Add(new TreeNode("Sessão não ativa."));
            return;
        }

        if (_elementTree.InvokeRequired)
        {
            _elementTree.Invoke(new Action(() => BuildElementTree(xpath)));
            return;
        }

        try
        {
            _elementTree.Nodes.Clear();
            Log("Construindo árvore de elementos com XPath: " + xpath);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var elements = wait.Until(d => d.FindElements(By.XPath(xpath)));

            Log($"Encontrados {elements.Count} elementos");

            if (!elements.Any())
            {
                 _elementTree.Nodes.Add(new TreeNode($"Nenhum elemento encontrado para XPath: {xpath}"));
                 return;
            }

            foreach (var element in elements)
            {
                try
                {
                    _ = element.TagName;

                    var name = element.GetDomAttribute("Name");
                    var tipo = element.GetDomAttribute("ControlType");
                    var value = element.GetAttribute("Value") ?? element.Text;

                    var nodeText = $"{name ?? "Sem Nome"} ({tipo ?? "Sem Tipo"})";
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        nodeText += $" - Valor: {value}";
                    }

                    var node = new TreeNode(nodeText) { Tag = element };

                    var clickNode = new TreeNode("Click") { Tag = new ElementAction { Action = "click", Element = element } };
                    var setValueNode = new TreeNode("Set Value") { Tag = new ElementAction { Action = "setValue", Element = element } };
                    var clearValueNode = new TreeNode("Clear") { Tag = new ElementAction { Action = "clear", Element = element } };
                    var detailsNode = new TreeNode("Details") { Tag = new ElementAction { Action = "showDetails", Element = element, Loaded = false } };

                    node.Nodes.Add(clickNode);
                    node.Nodes.Add(setValueNode);
                    node.Nodes.Add(clearValueNode);
                    node.Nodes.Add(detailsNode);

                    _elementTree.Nodes.Add(node);
                }
                catch (StaleElementReferenceException)
                {
                    Log("Pulando elemento obsoleto durante construção da árvore.", Color.Orange);
                    continue;
                }
                 catch (Exception ex)
                {
                     Log($"Erro ao processar elemento durante construção da árvore: {ex.Message}", Color.Orange);
                }
            }

            _elementTree.NodeMouseClick -= ElementTree_NodeMouseClick;
            _elementTree.NodeMouseClick += ElementTree_NodeMouseClick;

        }
        catch (WebDriverTimeoutException)
        {
             Log($"Timeout esperando por elementos com XPath: {xpath}", Color.Orange);
             _elementTree.Nodes.Add(new TreeNode($"Timeout encontrando elementos: {xpath}"));
        }
        catch (Exception ex)
        {
            Log($"Erro ao construir árvore de elementos: {ex.Message}", Color.Red);
            _elementTree.Nodes.Add(new TreeNode($"Erro ao carregar elementos: {ex.Message}"));
        }
    }

    private async void ElementTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node?.Tag is ElementAction actionInfo)
        {
            var element = actionInfo.Element;
            var action = actionInfo.Action;

            try
            {
                 _ = element.TagName;

                if (action == "click")
                {
                    Log($"Tentando clicar no elemento: {element.GetDomAttribute("Name") ?? element.TagName}", Color.Blue);
                    element.Click();
                    e.Node.BackColor = Color.LightGreen;
                    Log("Clique realizado com sucesso.", Color.Green);
                }
                else if (action == "setValue")
                {
                    var value = Microsoft.VisualBasic.Interaction.InputBox("Digite o valor:", "Set Value", "");
                    if (!string.IsNullOrEmpty(value))
                    {
                         Log($"Tentando definir valor '{value}' para o elemento: {element.GetDomAttribute("Name") ?? element.TagName}", Color.Blue);
                        element.SendKeys(value);
                        e.Node.BackColor = Color.LightYellow;
                        Log("Valor definido com sucesso.", Color.Green);
                    }
                }
                else if (action == "clear")
                {
                    Log($"Tentando limpar o valor para o elemento: {element.GetDomAttribute("Name") ?? element.TagName}", Color.Blue);
                    element.Clear();
                    e.Node.BackColor = Color.LightYellow;
                    Log("Valor limpo com sucesso.", Color.Green);
                }
                else if (action == "showDetails" && !actionInfo.Loaded)
                {
                    e.Node.Nodes.Clear();
                    Log($"Carregando detalhes para o elemento: {element.GetDomAttribute("Name") ?? element.TagName}", Color.Blue);
                    try
                    {
                        var details = await Task.Run(() => GetElementDetails(element));

                        foreach(var detail in details)
                        {
                            e.Node.Nodes.Add(new TreeNode(detail));
                        }

                        actionInfo.Loaded = true;
                        e.Node.Tag = actionInfo;
                        e.Node.Expand();
                         Log("Detalhes carregados.", Color.Green);
                    }
                    catch (Exception detailEx)
                    {
                        e.Node.Nodes.Add(new TreeNode($"Erro ao carregar detalhes: {detailEx.Message}"));
                        Log($"ERRO: Falha ao obter detalhes do elemento: {detailEx.Message}", Color.Red);
                    }
                }
            }
            catch (StaleElementReferenceException)
            {
                 Log($"ERRO: Ação '{action}' falhou. Elemento obsoleto.", Color.Red);
                 e.Node.BackColor = Color.Gray;
                 e.Node.Text += " (Obsoleto)";
            }
            catch (Exception ex)
            {
                Log($"ERRO: Falha ao executar ação '{action}': {ex.Message}", Color.Red);
                e.Node.BackColor = Color.LightCoral;
            }
        }
    }

    private List<string> GetElementDetails(IWebElement element)
    {
         var details = new List<string>();
         try{ details.Add($"Nome: {element.GetDomAttribute("Name")}"); } catch{}
         try{ details.Add($"Valor: {element.GetAttribute("Value") ?? element.Text ?? "N/D"}"); } catch{}
         try{ details.Add($"Tipo Controle: {element.GetDomAttribute("ControlType")}"); } catch{}
         try{ details.Add($"Tag: {element.TagName}"); } catch{}
         try{ details.Add($"Localização: ({element.Location.X}, {element.Location.Y})"); } catch{}
         try{ details.Add($"Tamanho: {element.Size.Width}x{element.Size.Height}"); } catch{}
         try{ details.Add($"Habilitado: {element.Enabled}"); } catch{}
         try{ details.Add($"Visível: {element.Displayed}"); } catch{}
         try{ details.Add($"Selecionado: {element.Selected}"); } catch{}
         return details;
    }

    private class ElementAction
    {
        public string Action { get; set; }
        public IWebElement Element { get; set; }
        public bool Loaded { get; set; }
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
             if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action(() => LogInternal(message, color)));
            }
            else
            {
                LogInternal(message, color);
            }
        }
    }

     private void LogInternal(string message, Color? color)
    {
        try
        {
            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.SelectionLength = 0;

            if (color.HasValue)
            {
                _logTextBox.SelectionColor = color.Value;
            }
            else
            {
                 _logTextBox.SelectionColor = _logTextBox.ForeColor;
            }

            _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            _logTextBox.SelectionColor = _logTextBox.ForeColor;
            _logTextBox.ScrollToCaret();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO DE LOG: {ex.Message}");
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {message}");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (_driver != null)
        {
            Log("Fechando sessão...", Color.Orange);
            try
            {
                _driver.Quit();
                _driver.Dispose();
                 Log("Sessão fechada.", Color.Orange);
            }
            catch (Exception ex)
            {
                 Log($"Erro ao fechar sessão: {ex.Message}", Color.Red);
            }
            _driver = null;
            _sessionActive = false;
        }
    }
}