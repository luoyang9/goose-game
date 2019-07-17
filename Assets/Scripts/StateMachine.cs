using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine : MonoBehaviour {
    private List<State> statesList = new List<State>();
    private State currentState;

    private void Update() {
        currentState.Update();
    }

    /// <summary>
    /// Switch the currentState to a specific State object
    /// </summary>
    /// <param name="state">
    /// The state object to set as the currentState</param>
    /// <returns>Whether the state was changed</returns>
    protected virtual bool SwitchState(State state) {
        if (state != null && state != currentState) {
            if (currentState != null) {
                currentState.OnStateExit();
            }
            currentState = state;
            gameObject.name = "Player - " + state.GetType().Name;
            currentState.OnStateEnter();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Switch the currentState to a State of a the given type.
    /// </summary>
    /// <typeparam name="StateType">
    /// The type of state to use for the currentState</typeparam>
    /// <returns>Whether the state was changed</returns>
    public virtual bool SwitchState<StateType>() where StateType : State, new() {
        // check if states list already contains this state type
        foreach (State state in statesList) {
            if (state is StateType) {
                return SwitchState(state);
            }
        }
        //if state type not found in list, make a new instance
        State newState = new StateType();
        newState.OnStateInitialize(gameObject.GetComponent<PlayerController>(), this);
        statesList.Add(newState);
        return SwitchState(newState);
    }
}