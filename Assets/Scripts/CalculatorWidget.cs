using UnityEngine;
using UnityEngine.UIElements;

public class CalculatorWidget : UIBase
{
    private readonly VisualElement _root;
    private readonly CalculatorController _controller;
    private readonly VisualTreeAsset _entryTemplate;
    
    private string _currentInput;
    private bool _isPushing;
    private ClearMode _clearMode;
    
    #region UIElements
    private VisualElement _naughtVe;
    private VisualElement _oneVe;
    private VisualElement _twoVe;
    private VisualElement _threeVe;
    private VisualElement _fourVe;
    private VisualElement _fiveVe;
    private VisualElement _sixVe;
    private VisualElement _sevenVe;
    private VisualElement _eightVe;
    private VisualElement _nineVe;

    private VisualElement _acVe;
    private Label _acLb;
    private VisualElement _plusVe;
    private VisualElement _minusVe;
    private VisualElement _timesVe;
    private VisualElement _divideVe;

    private TextField _inputTF;
    private ScrollView _inputSV;
    private VisualElement _pushVE;
    #endregion

    public CalculatorWidget(VisualElement root, CalculatorController controller)
    {
        _root = root;
        _controller = controller;
        _currentInput = "";
        
        _controller.OnPop += OnPop;
        _controller.OnPush += OnPush;
        _controller.OnError += OnError;
        
        _entryTemplate = Resources.Load("CalculatorEntry") as VisualTreeAsset;
        Setup();
    }

    private void OnError(string errorMsg)
    {
        _currentInput = "";
        _inputTF.value = errorMsg;
    }

    /// <summary>
    /// Setup region for UIElements (caching items from the root-element and registering callbacks to them!)
    /// </summary>
    #region Setup
    public override void Setup()
    {
        base.Setup();
        SetVisualElements();
    }

    private void SetVisualElements()
    {
        _naughtVe = _root.Q<VisualElement>("0VE");
        _oneVe    = _root.Q<VisualElement>("1VE");
        _twoVe    = _root.Q<VisualElement>("2VE");
        _threeVe  = _root.Q<VisualElement>("3VE");
        _fourVe   = _root.Q<VisualElement>("4VE");
        _fiveVe   = _root.Q<VisualElement>("5VE");
        _sixVe    = _root.Q<VisualElement>("6VE");
        _sevenVe  = _root.Q<VisualElement>("7VE");
        _eightVe  = _root.Q<VisualElement>("8VE");
        _nineVe   = _root.Q<VisualElement>("9VE");
        _acVe     = _root.Q<VisualElement>("acVE");
        _acLb     = _acVe.Q<Label>("acLB");
        _plusVe   = _root.Q<VisualElement>("plusVE");
        _minusVe  = _root.Q<VisualElement>("minusVE");
        _timesVe  = _root.Q<VisualElement>("timesVE");
        _divideVe = _root.Q<VisualElement>("divideVE");

        _inputTF = _root.Q<TextField>("inputTF");
        _inputSV = _root.Q<ScrollView>("inputSV");
        _pushVE = _root.Q<VisualElement>("pushVE");

        //_inputTF.textSelection.cursorColor = Color.white;
        
        EventHelper.RegisterCallback<ClickEvent>(_naughtVe,(evt => AddNum("0")));
        EventHelper.RegisterCallback<ClickEvent>(_oneVe,(evt => AddNum("1")));
        EventHelper.RegisterCallback<ClickEvent>(_twoVe,(evt => AddNum("2")));
        EventHelper.RegisterCallback<ClickEvent>(_threeVe,(evt => AddNum("3")));
        EventHelper.RegisterCallback<ClickEvent>(_fourVe,(evt => AddNum("4")));
        EventHelper.RegisterCallback<ClickEvent>(_fiveVe,(evt => AddNum("5")));
        EventHelper.RegisterCallback<ClickEvent>(_sixVe,(evt => AddNum("6")));
        EventHelper.RegisterCallback<ClickEvent>(_sevenVe,(evt => AddNum("7")));
        EventHelper.RegisterCallback<ClickEvent>(_eightVe,(evt => AddNum("8")));
        EventHelper.RegisterCallback<ClickEvent>(_nineVe,(evt => AddNum("9")));
        EventHelper.RegisterCallback<ClickEvent>(_pushVE, evt => PushToStack());

        EventHelper.RegisterValueChangedCallback(_inputTF, evt => _acLb.text = _clearMode is ClearMode.Clear ? "C" : "AC");
        EventHelper.RegisterCallback<ClickEvent>(_acVe, evt => Clear(string.IsNullOrEmpty(_currentInput)));
        EventHelper.RegisterCallback<ClickEvent>(_plusVe, evt => AddOperator("+"));
        EventHelper.RegisterCallback<ClickEvent>(_minusVe, evt => AddOperator("-"));
        EventHelper.RegisterCallback<ClickEvent>(_divideVe, evt => AddOperator("/"));
        EventHelper.RegisterCallback<ClickEvent>(_timesVe, evt => AddOperator("*"));
    }
    #endregion

    private void AddNum(string num)
    {
        _currentInput += num;
        _inputTF.value = _currentInput;

        if (!string.IsNullOrEmpty(_currentInput))
            _clearMode = ClearMode.Clear;
    }

    private void AddOperator(string op)
    {
        
        //If there was already input in the textfield we should push that input to the stack before 
        //executing our calculation for the clicked operator!
        if (!string.IsNullOrEmpty(_currentInput))
        {
            PushToStack();
        }
        Clear();
        _controller.AddToStack(op);
    }
    
    private void PushToStack()
    {
        if (_isPushing || string.IsNullOrEmpty(_currentInput))
            return;

        var pushedEntry = _entryTemplate.Instantiate();
        var entryLogic = new CalculatorEntry();
        pushedEntry.userData = entryLogic;
        entryLogic.SetVisualElement(pushedEntry);
        entryLogic.SetData(_currentInput);
        _inputSV.Add(pushedEntry);
        _controller.AddToStack(_currentInput);
        Clear();
        _currentInput = "";

        _isPushing = false;
    }

    private void OnPop()
    {
        var index = _inputSV.childCount == 0 ? 0 : _inputSV.childCount - 1;
        if (_inputSV[index] == null)
            return;
        
        _inputSV[index].RemoveFromHierarchy();
    }

    private void OnPush(string value)
    {
        var pushedEntry = _entryTemplate.Instantiate();
        var entryLogic = new CalculatorEntry();
        pushedEntry.userData = entryLogic;
        entryLogic.SetVisualElement(pushedEntry);
        entryLogic.SetData(value);
        _inputSV.Add(pushedEntry);
    }

    private void Clear(bool clearAll = false)
    {
        _inputTF.value = "";
        _clearMode = ClearMode.AllClear;
        _currentInput = "";

        if (clearAll)
        {
            for (int i = 0; i < _inputSV.childCount; i++)
            {
                (_inputSV[i].userData as CalculatorEntry)?.Dispose();
                _inputSV[i].RemoveFromHierarchy();
            }
            _inputSV.Clear();
            _controller.ClearStack();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _controller.OnPop += OnPop;
        _controller.OnPush += OnPush;
        _controller.OnError -= OnError;
        _controller.ClearStack();
    }

    private enum ClearMode
    {
        AllClear,
        Clear
    }
}
