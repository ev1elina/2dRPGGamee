using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
	public InputActionReference MovementReference;
	public float Speed = 5;

	public Animator Animator;
	public SpriteRenderer SpriteRenderer;

	private InputAction _movementAction;
	private Rigidbody2D _rb;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_movementAction = MovementReference.action;
	}

	private void FixedUpdate()
	{
		Vector2 movement = _movementAction.ReadValue<Vector2>().normalized;

		MovementAnimation(movement);
		if (movement.magnitude > 0)
		{
			_rb.MovePosition(_rb.position + movement * Speed * Time.fixedDeltaTime);
		}
	}

	public void MovementAnimation(Vector2 movement)
	{
		if (movement.magnitude > 0)
		{
			Animator.SetBool("isMoving", true);

			if (movement.x > 0)
			{
				SpriteRenderer.flipX = false;
			}
			else if(movement.x < 0)
			{
				SpriteRenderer.flipX = true;
			}
		}
		else
		{
			Animator.SetBool("isMoving", false);
		}
	}
}
