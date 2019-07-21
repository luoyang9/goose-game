using UnityEngine;
using UnityEditor;

public class IdleState : State
{
	public IdleState() { }

	public override void Update()
	{
        player.Idle();
		if(player.direction != 0) {
            machine.SwitchState<RunState>();
        }
        if(Input.GetButtonDown(player.controller + "_Jump")) {
            machine.SwitchState<JumpState>();
        }
	}
}