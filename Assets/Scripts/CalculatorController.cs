using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CalculatorController : MonoBehaviour
{
    public Action OnPop;
    public Action<string> OnPush;
    public Action<string> OnError;
    
    private UIDocument _activeUIDoc;
    private Stack<float> _history;
    private CalculatorWidget _widget;
    
    private int _calculationsSinceLastRandomNumber;
    private bool _isFetchingRandomNumber;

    private float _last;
    private float _secondToLast;

    private void Awake()
    {
        _activeUIDoc = gameObject.GetComponent<UIDocument>();
        _widget = new CalculatorWidget(_activeUIDoc.rootVisualElement, this);
        _history = new Stack<float>();
    }
    
    /// <summary>
    /// Try push the given value to the stack
    /// </summary>
    /// <param name="enteredValue"></param>
    public void TryPushToStack(string enteredValue)
    {
        if (_isFetchingRandomNumber)
        {
            OnError?.Invoke("Couldn't push value while fetching random number!");
            return;
        }

        if (float.TryParse(enteredValue, out float result))
        {
            _history.Push(result);
        }
        else
        {
            OnError?.Invoke("Couldn't parse entered value to float!");
        }
    }
    
    /// <summary>
    /// Try applying the given operation (+, -, \, *)
    /// </summary>
    /// <param name="op"></param>
    public void TryApplyOperator(string op)
    {
        if (_isFetchingRandomNumber)
        {
            OnError?.Invoke("Couldn't perform operation while fetching random number!");
            return;
        }
        
        if (_history.Count < 2)
        {
            OnError?.Invoke("Enter at least 2 values to perform an operation");
            return;
        }

        _last = _history.Pop();
        OnPop?.Invoke();
        _secondToLast = _history.Pop();
        OnPop?.Invoke();
        
        // Random number time!
        if (_calculationsSinceLastRandomNumber == 9)
        {
            _isFetchingRandomNumber = true;
            StartCoroutine(WebRequest());
            _calculationsSinceLastRandomNumber = 0;
            return;
        }
        
        float calculationResult = 0;
        
        switch (op)
        {
            case "+":
                calculationResult = _secondToLast + _last;
                break;
            case "-":
                calculationResult = _secondToLast - _last;
                break;
            case "/":
                if (_last != 0)
                {
                    calculationResult = _secondToLast / _last;
                }
                else
                {
                    //Restoring the _history and then reporting the error!
                    RestoreLastPopped();
                    OnError?.Invoke("Illegal - You can't divide by zero!");
                    return;
                }
                break;
            case "*":
                calculationResult = _secondToLast * _last;
                break;
        }
        
        _history.Push(calculationResult);
        OnPush?.Invoke(calculationResult.ToString());

        _calculationsSinceLastRandomNumber++;
    }

    /// <summary>
    /// Executing a web request to receive a random value that is then added to the _history stack.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WebRequest()
    {
        OnPush?.Invoke("Fetching a random result..");
        UnityWebRequest webRequest = UnityWebRequest.Get(@"https://www.randomnumberapi.com/api/v1.0/random?min=100&max=1000&count=1");
        yield return webRequest.SendWebRequest();
        
        // Remove the "Fetching random result" message
        OnPop?.Invoke();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                OnError?.Invoke("Connection Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.DataProcessingError:
                OnError?.Invoke("Processing Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                OnError?.Invoke("HTTP Error:" + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                if (TryParseWebResponse(webRequest.downloadHandler.text, out float randomValue))
                {
                    _history.Push(randomValue);
                    OnPush?.Invoke("RANDOM: " + randomValue);
                }
                else
                {
                    RestoreLastPopped();
                    OnError?.Invoke("Couldn't parse random number to float!");
                }
                break;
        }
        
        if (webRequest.result != UnityWebRequest.Result.Success)
            RestoreLastPopped();
        

        _isFetchingRandomNumber = false;
    }

    private void RestoreLastPopped()
    {
        _history.Push(_secondToLast);
        OnPush?.Invoke(_secondToLast.ToString());
        _history.Push(_last);
        OnPush?.Invoke(_last.ToString());
    }
    
    private void OnApplicationQuit()
    {
        _widget.Dispose();
        ClearStack();
    }
    
    /// <summary>
    /// Parse the random web result to a float!
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private bool TryParseWebResponse(string response, out float randomValue)
    {
        string numbersOnly = Regex.Replace(response, "[^0-9]", "");
        return float.TryParse(numbersOnly, out randomValue);
    }

    public void ClearStack()
    {
        _history.Clear();
        _calculationsSinceLastRandomNumber = 0;
    }

}

