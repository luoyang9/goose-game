using UnityEngine;
using UnityEditor;

public class IdleState : State
{
	public IdleState() { }

	public override void Update() {
        player.Idle();
		if (player.facing != 0) {
            machine.SwitchState<RunState>();
        }
        player.HandleJump();
	}
}