using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    private float move = 0.0f;
    private Vector2 dash = Vector2.zero;
    private bool isDash = false;
    private bool isBounce = false;

    public float Move
    {
        get
        {
            return move;
        }
        set
        {
            move = value;
            if (move != 0.0f)
            {
                isBounce = false;
            }          
        }
    }  
    public Vector2 Dash
    {
        get
        {
            return dash;
        }
        set
        {
            dash = value;
            if (dash == Vector2.zero)
            {
                rb.linearVelocity = dash;
                isDash = false;
            }
            else
            {
                isDash = true;
            }    
        }
    }
    public bool IsDash
    {
        get
        {
            return isDash;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        if (isDash)
        {
            rb.linearVelocity = dash;
        }
        else
        {
            if (!isBounce)
            {
                rb.linearVelocityX = move;
            }
        }          
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBounce)
        {
            Vector2 normal = collision.contacts[0].normal;
            if (normal.y >= 0.4f)
            {
                isBounce = false;
            }
        }
    }

    public void Jump(float power)
    {
        if (!isDash)
        { 
            rb.AddForceY(power, ForceMode2D.Impulse);
        }
    }

    public void Bounce(float power, Vector2 direction)
    {
        isBounce = true;
        rb.AddForce(direction * power, ForceMode2D.Impulse);
    }
}
