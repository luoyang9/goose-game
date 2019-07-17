using UnityEngine;
using UnityEditor;

public class GrapplingHookState : State {

    public GrapplingHookState() { }

    public override void Update() {
        player.Swing();
    }

    public override void OnStateEnter() {
        base.OnStateEnter();
    }

    public override void OnStateExit() {
        base.OnStateExit();
    }
}