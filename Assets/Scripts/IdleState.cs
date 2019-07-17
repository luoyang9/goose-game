using UnityEngine;
using UnityEditor;

public class IdleState : State
{
	public IdleState() { }

	public override void Update()
	{
		if(player.direction != 0) {
            machine.SwitchState<RunState>();
        }
        if(Input.GetKey(KeyCode.W)) {
            machine.SwitchState<JumpState>();
        }
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
	}
}