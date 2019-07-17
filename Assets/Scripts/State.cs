using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {
    protected PlayerController player;
    protected StateMachine machine;

    public State() {}

    public static implicit operator bool(State state) {
        return state != null;
    }

    public virtual void OnStateInitialize(PlayerController player, StateMachine machine) {
        this.player = player;
        this.machine = machine;
    }

    public virtual void OnStateEnter() {}

    public virtual void OnStateExit() {}

    public virtual void Update() {}
}