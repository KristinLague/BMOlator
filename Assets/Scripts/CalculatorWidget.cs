using UnityEngine;
using UnityEngine.UIElements;

public class CalculatorWidget : UIBase
{
    private string _currentInput;

    private readonly CalculatorController _controller;
    private readonly VisualElement _root;
    private readonly VisualTreeAsset _entryTemplate;
    private Label _clearLabel;
    private TextField _inputTextField;
    private ScrollView _inputScrollView;
    private VisualElement _pushVe;

    public CalculatorWidget(VisualElement root, CalculatorController controller)
    {
        _root = root;
        _controller = controller;
        _entryTemplate = Resources.Load("CalculatorEntry") as VisualTreeAsset;
        _currentInput = "";

        _controller.OnPop += RemovePreviousEntry;
        _controller.OnPush += AddNewEntry;
        _controller.OnError += DisplayError;
        
        Setup();
    }
    
    #region Setup
    public override void Setup()
    {
        base.Setup();
        SetVisualElements();
    }

    /// <summary>
    /// Caching all the UI Elements and hooking them up to their callback events!
    /// </summary>
    private void SetVisualElements()
    {
        for (int i = 0; i < 10; i++)
        {
            string digit = i + "";
            EventHelper.RegisterCallback<ClickEvent>(_root, digit + "VE", evt => TryAddValue(digit));
        }
        
        EventHelper.RegisterCallback<ClickEvent>(_root, "commaVE",(evt => TryAddValue(".")));
        EventHelper.RegisterCallback<ClickEvent>(_root, "plusVE", evt => TryApplyOperator("+"));
        EventHelper.RegisterCallback<ClickEvent>(_root, "minusVE", evt => TryApplyOperator("-"));
        EventHelper.RegisterCallback<ClickEvent>(_root, "divideVE", evt => TryApplyOperator("/"));
        EventHelper.RegisterCallback<ClickEvent>(_root, "timesVE", evt => TryApplyOperator("*"));
        EventHelper.RegisterCallback<ClickEvent>(_root, "plusminusVE",(evt => InvertInput()));
        
        EventHelper.RegisterCallback<ClickEvent>(_root, "pushVE", evt => PushToStack());
        EventHelper.RegisterCallback<ClickEvent>(_root, "acVE", evt => Clear(string.IsNullOrEmpty(_currentInput)));
        
        _clearLabel  = _root.Q<Label>("acLB");
        _inputTextField = _root.Q<TextField>("inputTF");
        _inputScrollView = _root.Q<ScrollView>("inputSV");
        EventHelper.RegisterValueChangedCallback(_inputTextField, evt => _clearLabel.text = string.IsNullOrEmpty(_currentInput) ? "AC" : "C");
        EventHelper.RegisterCallback<GeometryChangedEvent>(_inputScrollView.contentContainer, evt => _inputScrollView.verticalScroller.value = _inputScrollView.verticalScroller.highValue );
    }
    #endregion

    /// <summary>
    /// Adding numerical values or comma to the current string.
    /// </summary>
    /// <param name="value"></param>
    private void TryAddValue(string value)
    {
        //Ensuring not more than one comma can be set!
        if (value == "." && _currentInput.Contains('.'))
            return;
        
        _currentInput += value;
        _inputTextField.value = _currentInput;
    }

    /// <summary>
    /// Tries to apply an operation. If the current input isn't cleared at that stage, then push
    /// the current input first and then run the operation
    /// </summary>
    /// <param name="op"></param>
    private void TryApplyOperator(string op)
    {
        if (!string.IsNullOrEmpty(_currentInput))
            PushToStack();
        
        Clear();
        _controller.TryApplyOperator(op);
    }

    /// <summary>
    /// Inverts the current input from a negative to a positive number and vise-versa.
    /// </summary>
    private void InvertInput()
    {
        if (_currentInput.StartsWith('-'))
        {
            _currentInput = _currentInput.Substring(1, _currentInput.Length - 1);
        }
        else
        {
            FormatInput();
            _currentInput = "-" + _currentInput;
        }

        _inputTextField.value = _currentInput;
    }
    
    /// <summary>
    /// Pushes the current input to the stack history!
    /// </summary>
    private void PushToStack()
    {
        FormatInput();

        if (ValidateInput())
        {
            AddNewEntry(_currentInput);
            _controller.TryPushToStack(_currentInput);
            Clear();
        }
    }

    /// <summary>
    /// Removes the last entry from the UI
    /// </summary>
    private void RemovePreviousEntry()
    {
        if (_inputScrollView.childCount > 0)
        {
            _inputScrollView[_inputScrollView.childCount-1].RemoveFromHierarchy();
        }
    }

    /// <summary>
    /// Pushes the latest entry to the UI
    /// </summary>
    /// <param name="value"></param>
    private void AddNewEntry(string value)
    {
        var pushedEntry = _entryTemplate.Instantiate();
        var entryLogic = new CalculatorEntry();
        pushedEntry.userData = entryLogic;
        entryLogic.SetVisualElement(pushedEntry);
        entryLogic.SetData(value);
        _inputScrollView.Add(pushedEntry);
    }
    
    /// <summary>
    /// Displays incoming error messages in the Textfield!
    /// </summary>
    /// <param name="errorMsg"></param>
    private void DisplayError(string errorMsg)
    {
        _currentInput = "";
        _inputTextField.value = errorMsg;
    }

    #region Helpers
    /// <summary>
    /// Format the input with leading/trailing zeros
    /// </summary>
    private void FormatInput()
    {
        if (_currentInput.StartsWith('.'))
            _currentInput = "0" + _currentInput;

        if (_currentInput.EndsWith('.'))
            _currentInput += "0";
    }
    
    /// <summary>
    /// Check if the input is valid to be pushed to the stack
    /// </summary>
    private bool ValidateInput()
    {
        if (_currentInput.StartsWith('-') && _currentInput.Length == 1)
            return false;
        if (string.IsNullOrEmpty(_currentInput))
            return false;
        
        return true;
    }

    /// <summary>
    /// Clearing input and all history (if we are in clearAll-mode).
    /// </summary>
    /// <param name="clearAll"></param>
    private void Clear(bool clearAll = false)
    {
        _inputTextField.value = "";
        _currentInput = "";

        if (clearAll)
        {
            for (int i = 0; i < _inputScrollView.childCount; i++)
            {
                (_inputScrollView[i].userData as CalculatorEntry)?.Dispose();
                _inputScrollView[i].RemoveFromHierarchy();
            }
            _inputScrollView.Clear();
            _controller.ClearStack();
        }
    }
    #endregion

    public override void Dispose()
    {
        base.Dispose();
        _controller.OnPop += RemovePreviousEntry;
        _controller.OnPush += AddNewEntry;
        _controller.OnError -= DisplayError;
        _controller.ClearStack();
    }
}
