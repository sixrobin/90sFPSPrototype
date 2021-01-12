namespace Doomlike.Console
{
    using RSLib.Extensions;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This console system is used to access code at runtime and in build by referencing methods.
    /// It is very simple and has been made for "quick & dirty" implementation but useful results.
    /// The console does support :
    /// - Methods with no parameter,
    /// - Methods with one parameter of type : bool, float, integer or string,
    /// - Methods with two parameters of type : bool, float, integer or string (any order).
    /// 
    /// Methods can NOT have the same name AND the same count of parameters (the system checks the command ID and the parameters count before their types).
    /// This class is a MonoBehaviour Singleton that should have its instance created before anything else in the game.
    /// To use the console and use custom methods :
    /// - The AddCommand method adds a new command if possible, and has an extra parameter to specify if it should override an existing command of the same type (which might be useful for same scenes/classes reloading),
    /// - The OverrideCommand method is a shortcut to call the AddCommand method with the override system enabled,
    /// - The RemoveCommand method does remove a command by its ID and the possibility to specify the parameters count.
    /// To create a command, simply use **new SB.Debug.Console.DebugCommand<paramsTypes>(params)** constructor.
    /// 
    /// N.B.: This console system is NOT a good implementation of such a feature, simply a basic working one.
    /// </summary>
    [DisallowMultipleComponent]
    public class DebugConsole : RSLib.Framework.Singleton<DebugConsole>, IConsoleProLoggable
    {
        [Header("Console Datas")]
        [SerializeField] private bool _enabled = true;
        [SerializeField] private Color _consoleColor = new Color(0f, 0f, 0f, 0.9f);
        [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 1f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 1f);

        private GUIStyle _consoleStyle;
        private GUIStyle _helpTextStyle;
        private GUIStyle _invalidCmdTextStyle;
        private GUIStyle _autoCompletionTextStyle;

        private Vector2 _helpScroll;
        private Vector2 _historyScroll;

        private int _autoCompletionNavIndex = -1;
        private List<DebugCommandBase> _autoCompletionOptions = new List<DebugCommandBase>(16);
        private string _autoCompletionStr;
        private int _historyNavIndex = -1;
        private string _inputStr;
        private string _inputStrBeforeAutoComplete;
        private bool _showHelp;

        private List<(string cmd, bool isValid)> _cmdsHistory = new List<(string, bool)>();
        private List<DebugCommandBase> _registeredCmds = new List<DebugCommandBase>();

        public delegate void DebugConsoleToggledEventHandler(bool state);
        public event DebugConsoleToggledEventHandler DebugConsoleToggled;

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            private set
            {
                _isOpen = value;
                DebugConsoleToggled?.Invoke(_isOpen);
            }
        }

        public string ConsoleProPrefix => "Debug Console";

        public static void AddCommand(DebugCommandBase cmd, bool overrideIfCmdExists = false)
        {
            if (string.IsNullOrEmpty(cmd.Id))
            {
                ConsoleProLogger.LogError(Instance, "Command ID is null or empty!", Instance.gameObject);
                return;
            }

            if (cmd.Id.Contains(" "))
            {
                ConsoleProLogger.LogError(Instance, $"Command ID \"<b>{cmd.Id}</b>\" can not contain an empty space!", Instance.gameObject);
                return;
            }

            for (int i = Instance._registeredCmds.Count - 1; i >= 0; --i)
            {
                if (Instance._registeredCmds[i].Id == cmd.Id && Instance._registeredCmds[i].ParametersCount == cmd.ParametersCount)
                {
                    if (overrideIfCmdExists)
                    {
                        if (Instance._registeredCmds[i].IsConsoleNative)
                            ConsoleProLogger.LogError(Instance, "Can not override a console native command!", Instance.gameObject);
                        else
                            Instance._registeredCmds[i] = cmd;
                    }
                    else
                        ConsoleProLogger.LogWarning(Instance, $"A command with the same ID \"<b>{cmd.Id}</b>\" and the same parameters count (<b>{cmd.ParametersCount}</b>) is already registered!", Instance.gameObject);

                    return;
                }
            }

            Instance._registeredCmds.Add(cmd);
        }

        public static void OverrideCommand(DebugCommandBase cmd)
        {
            AddCommand(cmd, true);
        }

        public static void RemoveCommand(string id, int paramsCount = -1)
        {
            for (int i = Instance._registeredCmds.Count - 1; i >= 0; --i)
            {
                if (Instance._registeredCmds[i].Id == id)
                {
                    if (paramsCount != -1)
                    {
                        if (Instance._registeredCmds[i].ParametersCount == paramsCount)
                        {
                            if (Instance._registeredCmds[i].IsConsoleNative)
                            {
                                ConsoleProLogger.LogWarning(Instance, "Can not remove a console native command!", Instance.gameObject);
                                return;
                            }

                            Instance._registeredCmds.RemoveAt(i);
                            return;
                        }
                    }
                    else
                    {
                        if (Instance._registeredCmds[i].IsConsoleNative)
                        {
                            ConsoleProLogger.LogWarning(Instance, "Can not remove a console native command!", Instance.gameObject);
                            return;
                        }

                        Instance._registeredCmds.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        private GUIStyle ComputeConsoleStyle(int h, int w)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);

            Color[] pix = new Color[w * h];
            for (int i = pix.Length - 1; i >= 0; --i)
                pix[i] = _consoleColor;

            Texture2D result = new Texture2D(w, h);
            result.SetPixels(pix);
            result.Apply();
            style.normal.background = result;

            return style;
        }

        private void ComputeAutoCompletion()
        {
            _autoCompletionOptions.Clear();

            if (string.IsNullOrEmpty(_inputStrBeforeAutoComplete) || string.IsNullOrWhiteSpace(_inputStrBeforeAutoComplete))
                return;

            for (int i = 0, registeredCommandsCount = _registeredCmds.Count; i < registeredCommandsCount; ++i)
                if (_registeredCmds[i].Id.StartsWith(_inputStrBeforeAutoComplete))
                    _autoCompletionOptions.Add(_registeredCmds[i]);
        }

        private string ConvertAutoCompletionToString()
        {
            _autoCompletionStr = string.Empty;

            for (int i = 0, autoCompletionOptionsCount = _autoCompletionOptions.Count; i < autoCompletionOptionsCount; ++i)
                _autoCompletionStr += _autoCompletionNavIndex == i
                    ? $"{string.Format(Constants.AutoCompletionSelectedFormat, _autoCompletionOptions[i].GetFormat())}{Constants.AutoCompletionOptionsSplit}"
                    : $"{_autoCompletionOptions[i].GetFormat()}{Constants.AutoCompletionOptionsSplit}";

            if (!string.IsNullOrEmpty(_autoCompletionStr))
                _autoCompletionStr = _autoCompletionStr.RemoveLast(Constants.AutoCompletionOptionsSplit.Length);

            return _autoCompletionStr;
        }

        private void HandleInputCommand()
        {
            if (string.IsNullOrEmpty(_inputStr) || string.IsNullOrWhiteSpace(_inputStr))
                return;

            bool isCmdValid = false;

            DebugCommandBase currentCmd = null;
            string[] inputStrProperties = _inputStr.Split(' ');

            // Loop through all the registered commands and check if the ID matches the input ID.
            // If so, check if the parameters count also matches, and then try to parse the parameters according to the command type.
            // If the parsing is done correctly then the command is executed, else the command is displayed as "invalid".
            for (int i = _registeredCmds.Count - 1; i >= 0; --i)
            {
                if (inputStrProperties[0] == _registeredCmds[i].Id)
                {
                    currentCmd = _registeredCmds[i];

                    if (inputStrProperties.Length - 1 != currentCmd.ParametersCount)
                        continue;

                    switch (currentCmd.ParametersCount)
                    {
                        case 0:
                        {
                            if (currentCmd is DebugCommand cmd)
                            {
                                cmd.Execute();
                                isCmdValid = true;
                                break;
                            }

                            break;
                        }

                        case 1:
                        {
                            if (currentCmd is DebugCommand<bool> cmdBool)
                            {
                                if (bool.TryParse(inputStrProperties[1], out bool param))
                                {
                                    cmdBool.Execute(param);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<float> cmdFloat)
                            {
                                if (float.TryParse(inputStrProperties[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param))
                                {
                                    cmdFloat.Execute(param);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<int> cmdInt)
                            {
                                if (int.TryParse(inputStrProperties[1], out int param))
                                {
                                    cmdInt.Execute(param);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<string> cmdString)
                            {
                                cmdString.Execute(inputStrProperties[1]);
                                isCmdValid = true;

                                break;
                            }

                            break;
                        }

                        case 2:
                        {
                            if (currentCmd is DebugCommand<bool, bool> cmdBoolBool)
                            {
                                if (bool.TryParse(inputStrProperties[1], out bool param1)
                                    && bool.TryParse(inputStrProperties[2], out bool param2))
                                {
                                    cmdBoolBool.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<bool, float> cmdBoolFloat)
                            {
                                if (bool.TryParse(inputStrProperties[1], out bool param1)
                                    && float.TryParse(inputStrProperties[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param2))
                                {
                                    cmdBoolFloat.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<bool, int> cmdBoolInt)
                            {
                                if (bool.TryParse(inputStrProperties[1], out bool param1)
                                    && int.TryParse(inputStrProperties[2], out int param2))
                                {
                                    cmdBoolInt.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<bool, string> cmdBoolString)
                            {
                                if (bool.TryParse(inputStrProperties[1], out bool param1))
                                {
                                    cmdBoolString.Execute(param1, inputStrProperties[2]);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<float, bool> cmdFloatBool)
                            {
                                if (float.TryParse(inputStrProperties[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param1)
                                    && bool.TryParse(inputStrProperties[2], out bool param2))
                                {
                                    cmdFloatBool.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<float, float> cmdFloatFloat)
                            {
                                if (float.TryParse(inputStrProperties[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param1)
                                    && float.TryParse(inputStrProperties[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param2))
                                {
                                    cmdFloatFloat.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<float, int> cmdFloatInt)
                            {
                                if (float.TryParse(inputStrProperties[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param1)
                                    && int.TryParse(inputStrProperties[2], out int param2))
                                {
                                    cmdFloatInt.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<float, string> cmdFloatString)
                            {
                                if (float.TryParse(inputStrProperties[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param1))
                                {
                                    cmdFloatString.Execute(param1, inputStrProperties[2]);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<int, bool> cmdIntBool)
                            {
                                if (int.TryParse(inputStrProperties[1], out int param1)
                                    && bool.TryParse(inputStrProperties[2], out bool param2))
                                {
                                    cmdIntBool.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<int, float> cmdIntFloat)
                            {
                                if (int.TryParse(inputStrProperties[1], out int param1)
                                    && float.TryParse(inputStrProperties[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param2))
                                {
                                    cmdIntFloat.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<int, int> cmdIntInt)
                            {
                                if (int.TryParse(inputStrProperties[1], out int param1)
                                    && int.TryParse(inputStrProperties[2], out int param2))
                                {
                                    cmdIntInt.Execute(param1, param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<int, string> cmdIntString)
                            {
                                if (int.TryParse(inputStrProperties[1], out int param1))
                                {
                                    cmdIntString.Execute(param1, inputStrProperties[2]);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<string, bool> cmdStringBool)
                            {
                                if (bool.TryParse(inputStrProperties[2], out bool param2))
                                {
                                    cmdStringBool.Execute(inputStrProperties[1], param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<string, float> cmdStringFloat)
                            {
                                if (float.TryParse(inputStrProperties[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float param2))
                                {
                                    cmdStringFloat.Execute(inputStrProperties[1], param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<string, int> cmdStringInt)
                            {
                                if (int.TryParse(inputStrProperties[2], out int param2))
                                {
                                    cmdStringInt.Execute(inputStrProperties[1], param2);
                                    isCmdValid = true;
                                }

                                break;
                            }

                            if (currentCmd is DebugCommand<string, string> cmdStringString)
                            {
                                cmdStringString.Execute(inputStrProperties[1], inputStrProperties[2]);
                                isCmdValid = true;

                                break;
                            }

                            break;
                        }
                    }

                }
            }

            if (currentCmd == null || currentCmd.ShowInHistory)
                _cmdsHistory.Add((_inputStr, isCmdValid));

            // Reset console datas.
            _inputStr = string.Empty;
            _inputStrBeforeAutoComplete = string.Empty;
            ResetHistoryNavigation();
            _autoCompletionOptions.Clear();
        }

        private void NavigateThroughAutoCompletionOptions(int step)
        {
            _autoCompletionNavIndex = Mathf.Clamp(_autoCompletionNavIndex + step, -1, _autoCompletionOptions.Count - 1);
            _inputStr = _autoCompletionNavIndex == -1
                ? _inputStrBeforeAutoComplete
                : _inputStr = _autoCompletionOptions[_autoCompletionNavIndex].Id;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            // We clear the console style, else they won't be rendered correctly after a scene loading.
            // There might be a way to fix this bug other than just working around but it's not a problem since it is a debug tool.
            _consoleStyle = null;
            _helpTextStyle = null;
            _invalidCmdTextStyle = null;
            _autoCompletionTextStyle = null;
        }

        private void RegisterNativeCommands()
        {
            ////////////////////////////////////////////////////////////////////////////////////
            //// Commands that are inherent in the console system (clear, help, remove command).
            ////////////////////////////////////////////////////////////////////////////////////

            AddCommand(new DebugCommand("h", "Shows available commands and hotkeys.", false, true, () =>
            {
                _showHelp = !_showHelp;
                if (_showHelp)
                    _registeredCmds.Sort();
            }));

            AddCommand(new DebugCommand("help", "Shows available commands and hotkeys.", false, true, () =>
            {
                _showHelp = !_showHelp;
                if (_showHelp)
                    _registeredCmds.Sort();
            }));

            AddCommand(new DebugCommand("c", "Clears commands history.", false, true, () => _cmdsHistory.Clear()));
            AddCommand(new DebugCommand("clear", "Clears commands history.", false, true, () => _cmdsHistory.Clear()));
            //AddCommand(new DebugCommand<string, int>("removeCmd", "Removes a registered command by ID and parameters count.", true, true, (id, paramsCount) => RemoveCommand(id, paramsCount)));


            ////////////////////////////////////////////////////////////////////////////////////////////////
            //// Commands that are not inherent in the console but potentially useful in any Unity project :
            ////////////////////////////////////////////////////////////////////////////////////////////////

            AddCommand(new DebugCommand("r", "Reloads the currently active scene.", true, true,
                () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)));

            AddCommand(new DebugCommand("reloadActiveScene", "Reloads the currently active scene.", true, true,
                () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)));

            AddCommand(new DebugCommand<string>("loadSceneByName", "Loads a scene by its name.", true, true,
                (sceneName) => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName)));

            AddCommand(new DebugCommand<int>("loadSceneByIndex", "Loads a scene by its build settings index.", true, true,
                (sceneIndex) => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex)));
        }

        private void ResetHistoryNavigation()
        {
            _historyNavIndex = -1;
        }

        private void RewindCommandsHistory(int step)
        {
            _historyNavIndex = Mathf.Clamp(_historyNavIndex + step, -1, _cmdsHistory.Count - 1);
            _inputStr = _historyNavIndex != -1 ? _cmdsHistory[_cmdsHistory.Count - 1 - _historyNavIndex].cmd : string.Empty;
        }

        private void OnGUI()
        {
            if (!IsOpen)
                return;


            ///////////////////////////////
            //// GUI styles initialization.
            ///////////////////////////////

            if (_consoleStyle == null)
                _consoleStyle = ComputeConsoleStyle(Constants.EntryBoxHeight, Constants.Width);

            if (_helpTextStyle == null)
                _helpTextStyle = new GUIStyle()
                {
                    fontStyle = FontStyle.Italic,
                    alignment = TextAnchor.MiddleLeft,
                    normal = new GUIStyleState()
                    {
                        textColor = new Color(1f, 1f, 1f, 0.33f)
                    }
                };

            if (_invalidCmdTextStyle == null)
                _invalidCmdTextStyle = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal = new GUIStyleState()
                    {
                        textColor = new Color(1f, 0f, 0f, 0.8f)
                    }
                };

            if (_autoCompletionTextStyle == null)
                _autoCompletionTextStyle = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = new GUIStyleState()
                    {
                        textColor = new Color(1f, 1f, 1f, 0.5f)
                    }
                };


            ////////////////////
            //// Input handling.
            ////////////////////

            Event currentEvent = Event.current;

            if (currentEvent.Equals(Event.KeyboardEvent("quote")))
            {
                IsOpen = false;
                _showHelp = false;
                _inputStr = string.Empty;
                _autoCompletionNavIndex = -1;
                ResetHistoryNavigation();
            }

            // Return : validate command OR validate autocompletion.
            if (currentEvent.Equals(Event.KeyboardEvent("return")))
            {
                if (_autoCompletionNavIndex == -1)
                    HandleInputCommand();
                else
                {
                    _inputStrBeforeAutoComplete = _inputStr;
                    _autoCompletionNavIndex = -1;
                }
            }

            if (currentEvent.Equals(Event.KeyboardEvent("tab")))
                NavigateThroughAutoCompletionOptions(1);

            if (currentEvent.Equals(Event.KeyboardEvent("#tab"))) // Shift + tab.
                NavigateThroughAutoCompletionOptions(-1);

            // Escape : erase command OR cancel autocompletion.
            if (currentEvent.Equals(Event.KeyboardEvent("escape")))
            {
                if (_autoCompletionNavIndex == -1)
                {
                    _inputStr = string.Empty;
                    ResetHistoryNavigation();
                }
                else
                {
                    _inputStr = _inputStrBeforeAutoComplete;
                    _autoCompletionNavIndex = -1;
                }
            }

            // Cancel autocompletion.
            if (currentEvent.Equals(Event.KeyboardEvent("backspace"))
                || currentEvent.type == EventType.KeyDown
                && (currentEvent.keyCode.ToString().Length == 1 && char.IsLetter(currentEvent.keyCode.ToString()[0]) || char.IsDigit(currentEvent.keyCode.ToString()[currentEvent.keyCode.ToString().Length - 1])))
            {
                if (_autoCompletionNavIndex != -1)
                {
                    _inputStr = _inputStrBeforeAutoComplete;
                    _autoCompletionNavIndex = -1;
                    return;
                }
            }

            if (currentEvent.Equals(Event.KeyboardEvent("up")))
                RewindCommandsHistory(1);

            if (currentEvent.Equals(Event.KeyboardEvent("down")))
                RewindCommandsHistory(-1);


            /////////////////////
            //// Console display.
            /////////////////////

            float y = Screen.height;

            // Help box.
            if (_showHelp)
            {
                float helpBoxPosY = y - Constants.HelpBoxHeight - Constants.EntryBoxHeight - Constants.HistoryBoxHeight - Constants.BoxesSpacing * 2;

                GUI.Box(new Rect(0f, helpBoxPosY, Constants.Width, Constants.HelpBoxHeight), string.Empty, _consoleStyle);
                Rect helpViewport = new Rect(0f, 0f, Constants.Width - 30f, Constants.LinesSpacing * (_registeredCmds.Count + Constants.HotkeyHelps.Length + 1));

                float helpScrollPosY = y - Constants.HelpBoxHeight - 25f - Constants.HistoryBoxHeight;

                _helpScroll = GUI.BeginScrollView(new Rect(5f, helpScrollPosY, Constants.Width - 10f, Constants.HelpBoxHeight - 15f), _helpScroll, helpViewport);

                // Hotkeys.
                for (int i = Constants.HotkeyHelps.Length - 1; i >= 0; --i)
                {
                    Rect hotkeysRect = new Rect(5f, Constants.LinesSpacing * i, helpViewport.width - 100f, 20f);
                    GUI.Label(hotkeysRect, Constants.HotkeyHelps[i]);
                }

                for (int i = _registeredCmds.Count - 1; i >= 0; --i)
                {
                    DebugCommandBase cmd = _registeredCmds[i];
                    string cmdHelp = string.Format(Constants.CmdHelpFormat, cmd.GetFormat(), (cmd.IsConsoleNative ? "(Native) " : "") + cmd.Description);
                    Rect cmdHelpRect = new Rect(5f, Constants.LinesSpacing * (i + Constants.HotkeyHelps.Length + 1), helpViewport.width - 100f, 20f);
                    GUI.Label(cmdHelpRect, cmdHelp);
                }

                GUI.EndScrollView();
            }

            // History box.
            GUI.Box(new Rect(0f, y - Constants.HistoryBoxHeight - Constants.EntryBoxHeight - Constants.BoxesSpacing, Constants.Width, Constants.HistoryBoxHeight), string.Empty, _consoleStyle);
            Rect historyViewport = new Rect(0f, 0f, Constants.Width - 30f, Constants.LinesSpacing * _cmdsHistory.Count);
            _historyScroll = GUI.BeginScrollView(new Rect(5f, y - Constants.HistoryBoxHeight - 25f, Constants.Width - 10f, Constants.HistoryBoxHeight - 15f), _historyScroll, historyViewport);

            for (int i = _cmdsHistory.Count - 1; i >= 0; --i)
            {
                string cmdDisplay = _cmdsHistory[i].isValid
                    ? string.Format(Constants.ValidCommandFormat, _cmdsHistory[i].cmd).ToColored(_validColor)
                    : string.Format(Constants.InvalidCommandFormat, _cmdsHistory[i].cmd).ToColored(_invalidColor);

                if (_historyNavIndex != -1 && i == _cmdsHistory.Count - 1 - _historyNavIndex)
                    cmdDisplay += Constants.CurrentNavigatedInHistoryMarker;

                Rect cmdHelpRect = new Rect(5f, Constants.LinesSpacing * i, historyViewport.width - 100f, 20f);
                GUI.Label(cmdHelpRect, cmdDisplay);
            }

            GUI.EndScrollView();

            // Entry box.
            GUI.Box(new Rect(0f, y - 30f, Constants.Width, Constants.EntryBoxHeight), string.Empty, _consoleStyle);
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0f);

            GUI.SetNextControlName(Constants.ControlName);
            Rect logEntryRect = new Rect(5f, y - 25f, Constants.Width - 20f, Constants.EntryBoxHeight - 5f);
            _inputStr = GUI.TextField(logEntryRect, _inputStr);
            GUI.FocusControl(Constants.ControlName);

            // Reset history navigation if something has been typed.
            if (_historyNavIndex != -1 && _inputStr != _cmdsHistory[_cmdsHistory.Count - 1 - _historyNavIndex].cmd)
                ResetHistoryNavigation();

            // Reset autocompletion navigation if something has been typed.
            if (_autoCompletionNavIndex != -1 && _inputStr != _autoCompletionOptions[_autoCompletionNavIndex].Id)
            {
                _inputStr = _inputStrBeforeAutoComplete;
                _autoCompletionNavIndex = -1;
            }

            // Auto completion.
            if (!string.IsNullOrEmpty(_inputStr) && !string.IsNullOrWhiteSpace(_inputStr))
            {
                if (_autoCompletionNavIndex == -1)
                    _inputStrBeforeAutoComplete = _inputStr;

                ComputeAutoCompletion();
                GUI.Label(logEntryRect, ConvertAutoCompletionToString(), _autoCompletionTextStyle);
            }

            // Display help tooltip message.
            if (!_showHelp && (string.IsNullOrEmpty(_inputStr) || string.IsNullOrWhiteSpace(_inputStr)))
                GUI.Label(logEntryRect, Constants.HelpText, _helpTextStyle);
        }

        protected override void Awake()
        {
            base.Awake();

            if (!Valid)
                return;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            RegisterNativeCommands();
        }

        private void Update()
        {
            if (_enabled && !IsOpen && Input.GetKeyDown(KeyCode.Quote))
                IsOpen = true;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public class Constants
        {
            // Used to display the parameters types in a more readable way that something like **System.Int32**.
            public static readonly Dictionary<System.Type, string> TypesFormats = new Dictionary<System.Type, string>()
            {
                { typeof(bool), "bool" },
                { typeof(float), "float" },
                { typeof(int), "int" },
                { typeof(string), "string" }
            };

            // Set as an array of strings to handle lines spacing.
            public static readonly string[] HotkeyHelps = new string[]
            {
                ">>> Use <b>UP</b> and <b>DOWN</b> arrows to navigate through history.",
                ">>> Use <b>ESCAPE</b> to erase your current command <b>or</b> to cancel autocompletion.",
                ">>> Use <b>TAB</b> and <b>SHIFT+TAB</b> to navigate through autocompletion.",
                ">>> Use <b>RETURN</b> to validate autocompletion and <b>BACKSPACE</b> to cancel it."
            };

            public const string AutoCompletionOptionsSplit = "  |  ";
            public const string AutoCompletionSelectedFormat = "<color=white><b>[ {0} ]</b></color>"; // Highlight the autocompletion selection.
            public const string CmdHelpFormat = "<b><i>{0}</i></b>  -  <i>{1}</i>"; // 0 is command format, 1 is description.
            public const string ControlName = "ConsoleInputEntry";
            public const string CurrentNavigatedInHistoryMarker = "<b>  <<<</b>";
            public const string HelpText = "Type <b>\"h\"</b> or <b>\"help\"</b> to display available commands and console hotkeys.";
            public const string InvalidCommandFormat = "<b>> {0}  -  Invalid command!</b>";
            public const string ValidCommandFormat = "<b>> {0}</b>";

            public const int BoxesSpacing = 1;
            public const int LinesSpacing = 20;

            public const int EntryBoxHeight = 30;
            public const int HelpBoxHeight = 96;
            public const int HistoryBoxHeight = 146;
            public const int Width = 640;
        }
    }
}