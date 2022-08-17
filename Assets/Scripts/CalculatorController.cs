using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class CalculatorController : MonoBehaviour
{
    private UIDocument _activeUIDoc;
    private CalculatorWidget _calculatorWidget;
    
    public Action<string> OnPush;
    public Action OnPop;
    public Action<string> OnError;

    private int _calculationCounter;

    public Stack<float> EntryStack { get; private set; }

    void Awake()
    {
        _calculationCounter = 0;
        _activeUIDoc = gameObject.GetComponent<UIDocument>();
        _calculatorWidget = new CalculatorWidget(_activeUIDoc.rootVisualElement, this);
        EntryStack = new Stack<float>();
    }
    
    public void AddToStack(string value)
    {
        switch (value)
        {
            case "+":
                Calculate(CalcType.Add);
                break;
            case "-":
                Calculate(CalcType.Subtract);
                break;
            case "/":
                Calculate(CalcType.Divide);
                break;
            case "*":
                Calculate(CalcType.Multiply);
                break;
            default:
            {
                if (float.TryParse(value, out float result))
                    EntryStack.Push(result);
                break;
            }
        }
    }

    private IEnumerator WebRequest()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(@"https://www.randomnumberapi.com/api/v1.0/random?min=100&max=1000&count=1");
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                OnError?.Invoke("Connection Error" + ": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.DataProcessingError:
                OnError?.Invoke("Processing Error" + ": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                OnError?.Invoke("Protocol Error" + ": HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                int resultInt = GetValue(webRequest.downloadHandler.text);
                EntryStack.Push(resultInt);
                OnPush?.Invoke("RANDOM: " + resultInt);
                break;
        }
    }

    private int GetValue(string result)
    {
        string numbersOnly = Regex.Replace(result, "[^0-9]", "");
        int.TryParse(numbersOnly, out int value);
        return value;
    }

    public void ClearStack()
    {
        EntryStack.Clear();
        _calculationCounter = 0;
    }
    
    private void Calculate(CalcType calcType)
    {
        if (EntryStack.Count < 2)
        {
            OnError?.Invoke("Enter at least 2 values to make a operation");
            return;
        }

        if (_calculationCounter > 0)
        {
            if (_calculationCounter % 10 == 0)
            {
                StartCoroutine(WebRequest());
                _calculationCounter++;
                return;
            }
        }

        if (EntryStack.TryPop(out float last))
        {
            if (EntryStack.TryPop(out float secondToLast))
            {
                switch (calcType)
                {
                    case CalcType.Add:
                        OnPop?.Invoke();
                        OnPop?.Invoke();
                        float additionResult = (float) secondToLast + (float) last;
                        EntryStack.Push(additionResult);
                        OnPush?.Invoke(additionResult.ToString());
                        break;
                    case CalcType.Subtract:
                        OnPop?.Invoke();
                        OnPop?.Invoke();
                        float subtractionResult = secondToLast - last;
                        EntryStack.Push(subtractionResult);
                        OnPush?.Invoke(subtractionResult.ToString());
                        break;
                    case CalcType.Divide:
                        if (last != 0)
                        {
                            OnPop?.Invoke();
                            OnPop?.Invoke();
                            float divisionResult = last == 0 ? 0 : secondToLast / last;
                            EntryStack.Push(divisionResult);
                            OnPush?.Invoke(divisionResult.ToString());
                        }
                        else
                        {
                            OnError?.Invoke("Illegal - You can't divide by zero!");
                            return;
                        }
                        break;
                    case CalcType.Multiply:
                        OnPop?.Invoke();
                        OnPop?.Invoke();
                        float multiplicationResult = secondToLast * last;
                        EntryStack.Push(multiplicationResult);
                        OnPush?.Invoke(multiplicationResult.ToString());
                        break;
                }

                _calculationCounter++;
            }
        }
    }

    private enum CalcType
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}

