using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine : MonoBehaviour {
    private Dictionary<int, Func<int>> updateMap = new Dictionary<int, Func<int>>();
    private Dictionary<int, Action> beginMap = new Dictionary<int, Action>();
    private Dictionary<int, Action> endMap = new Dictionary<int, Action>();

    private int currentState;
    public int CurrentState {
        get {
            return currentState;
        }
        set {
            if (value != currentState) {
                Action end = endMap[currentState];
                Action begin = beginMap[value];
                if (end != null) end();
                if (begin != null) begin();
            }
            currentState = value;
        }
    }

    private void Update() {
        CurrentState = updateMap[CurrentState]();
    }

    public void RegisterState(int state, Func<int> update, Action begin, Action end) {
        updateMap.Add(state, update);
        beginMap.Add(state, begin);
        endMap.Add(state, end);
    }
}