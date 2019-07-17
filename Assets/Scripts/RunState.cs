using UnityEngine;
using UnityEditor;

public class RunState : State {
    public RunState() { }

    public override void Update() {
        player.Run();
        if(System.Math.Abs(player.rBody.velocity.x) < 0.001) {
            machine.SwitchState<IdleState>();
        }
        if(Input.GetKeyDown(KeyCode.W)) {
            machine.SwitchState<JumpState>();
        }
    }

    public override void OnStateEnter() {
        base.OnStateEnter();
    }

    public override void OnStateExit() {
        base.OnStateExit();
    }
}