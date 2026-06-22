using UnityEngine;

public class JumpAbility : MonoBehaviour
{
    private MovementSystem movementSystem;
    private TagJump tagJump;

    private bool onGround = false;

    public float Power
    {
        get
        {
            return tagJump.Power;
        }
    }

    private void Awake()
    {
        enabled = false;
    }
    public void Init(TagJump tagJump)
    {
        this.tagJump = tagJump;

        movementSystem = GetComponent<MovementSystem>();

        enabled = true;
    }

    public void Jump()
    {
        if (onGround)
        {
            movementSystem.Jump(tagJump.Power);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        if (normal.y >= 0.4f)
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.contacts.Length != 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            if (normal.y == 0)
            {
                onGround = false;
            }
        }
        else
        {
            onGround = false;
        }
    }
}
