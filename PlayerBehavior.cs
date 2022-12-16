using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    Author: Bryan Epperson
    Player movement script for unity 2d games I'll make in the future.
*/
public class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    private int health = 100;
    public float moveSpeed = 20.0f;

    public float descentSpeed = 1.0f;
    public float jumpHeight = 40.0f;

    public float wallJumpCounter = 0f;

    private float coolDownTimer = 1.5f;
    public int dashCounter = 0;

    private float currentDirection;


    [SerializeField]
    private Vector2 dashPower = new Vector2(20.0f, 1.0f) ;

    [SerializeField]

    private bool dashAvailable = true;

    [SerializeField]
    private bool isWallJump = false;
    private Vector2 wallJumpForce = new Vector2(20f, 16f);

    private float wallJumpingDirection;

    [SerializeField]
    private float wallSlideSpeed = -0.5f;

    [SerializeField]
    private bool isWallSlinding = false;

    [SerializeField]
    private bool isGrounded = false;

    [SerializeField]
    private bool isTouchingWall = false;

    [SerializeField]
    private bool isAlive = true;

    [SerializeField]
    private bool isFacingLeft = false;

    [SerializeField]
    private bool isFacingRight = false;
    Rigidbody2D myRigidBody2D;

    GameObject player;
    
    Ray2D currentRay;

    Vector2 myVector;

    // Start is called before the first frame update
    void Start()
    {
        
        player = GetComponent<GameObject>();
        myVector = new Vector2(1.0f, 1.0f); // To allow maniuplation of movement!
        myRigidBody2D = GetComponent<Rigidbody2D>(); //get the component of a selected object! 
        
    }

    // Update is called once per frame
    void Update()
    {
        gameOver();
        playerMove();
        
    }

    void groundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.down),  0.8f, LayerMask.GetMask("Ground")); // continously draws a ray to see if player is touching the ground
        if(hit)
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.down) * 0.8f , Color.green);
            this.setGroundCheck(true);
        }
        else if(!hit)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.down) * 0.8f , Color.red);
            this.setGroundCheck(false);
        }
    }

    // Player loses once health reaches zero, simple stuff
    void gameOver()
    {
        getHealth();
        if(health <= 0)
        {
            setAlive(false);
            Debug.Log("GameOver");
        }
    }


    /*
        Notes
        I've done some simple counter movement with adding a coditional to check the players velocity
        to see if it exceeds the players movespeed, works as intended can be improved but for all
        intents and purposes it feels fine, a little sluggish though but enough
    */
    void playerMove()
    {
        // Debug.Log("Velocity of X:" + " " + myRigidBody2D.velocity.x);
        // Debug.Log("Velocity of Y:" + " " + myRigidBody2D.velocity.y);

        groundCheck();
         /*
            ---------------------------------------------------
            Right Movement such as moving right and wall slides
            ---------------------------------------------------
        */
        if(Input.GetKey(KeyCode.D) && myRigidBody2D.velocity.x <= moveSpeed) //we use velocity to control how fast we move in a current direction
        {
            Debug.Log("move right is pressed");
            setDirRight(true);
            setDirLeft(false);
            myRigidBody2D.AddForce(Vector2.right * moveSpeed); //player move right
            transform.localScale = new Vector3(1,1,1);
            currentDirection = transform.localScale.x;    
        }
        /*
            -------------------------------------------------
            Left Movement such as moving left and wall slides
            -------------------------------------------------
        */
        if(Input.GetKey(KeyCode.A) && myRigidBody2D.velocity.x >= -moveSpeed)
        {
            Debug.Log("move left is pressed");
            setDirLeft(true);
            setDirRight(false);
            myRigidBody2D.AddForce(Vector2.left * moveSpeed); //player move left
            transform.localScale = new Vector3(-1,1,-1); //this makes the player(objects and their children) turn left or right, make sure the scales match the current model you are doing.
            currentDirection = transform.localScale.x;
        }

        /*
            -------------------------------
            Movement involving player jumps
            -------------------------------
        */
        if(Input.GetKey(KeyCode.Space) && isGrounded == true) // player can only jump if they are grounded
        {
            Debug.Log("Jump key pressed");
            myRigidBody2D.velocity = new Vector2(0.0f, jumpHeight);
        }
        Dash();
        wallSlide();
        wallJump();
    }

    void Dash()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("Player dash!");
            useDash();
        }
    }

    void useDash()
    {
        if(!dashAvailable)
        {
            Debug.Log("cant dash!");
            return;
        }

        if(dashAvailable)
        {
            myRigidBody2D.velocity = new Vector2(currentDirection * dashPower.x, dashPower.y);
        }
        StartCoroutine(startDashCoolDown());
    }

    public IEnumerator startDashCoolDown()
    {
        dashAvailable = false;
        yield return new WaitForSeconds(coolDownTimer);
        dashAvailable = true;
    }

    void wallCheck()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.left),  0.8f, LayerMask.GetMask("Wall")); // continously draws a ray to see if player is touching the ground
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.right),  0.8f, LayerMask.GetMask("Wall"));
        if(hitLeft)
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.left) * 0.8f , Color.green);
            this.setWallCheck(true);
        }
        if(hitRight)
        {
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.right) * 0.8f , Color.green);
            this.setWallCheck(true);
        }
        else if(!hitLeft && !hitRight)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.left) * 0.8f , Color.red);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.right) * 0.8f , Color.red);
            this.setWallCheck(false);
        }
        /*
            This function constantly checks to see if the player is touching a wall to the left or right of player object.
        */
    }




    void wallSlide() // This function allows the player to wall slide.
    {
        wallCheck(); //A function called wallcheck is ran first to check to see if a player is touching a wall.
        if(isTouchingWall && !isGrounded && Input.GetKey(KeyCode.A))
        {
            isWallSlinding = true;
            setWallJump(false);
            myRigidBody2D.velocity = new Vector2(myRigidBody2D.velocity.x, Mathf.Clamp(myRigidBody2D.velocity.y, -wallSlideSpeed, float.MaxValue));
            transform.localScale = new Vector3(1,1,1);
        }
        else if(isTouchingWall && !isGrounded && Input.GetKey(KeyCode.D))
        {
            isWallSlinding = true;
            setWallJump(false);
            myRigidBody2D.velocity = new Vector2(myRigidBody2D.velocity.x, Mathf.Clamp(myRigidBody2D.velocity.y, -wallSlideSpeed, float.MaxValue));
            transform.localScale = new Vector3(-1,1,-1);

        }
        else
        {
            isWallSlinding = false;
        }
    }

    void wallJump() //This allows the player to do a wall jump
    {
        Debug.Log("Start of walljump function");
        if(isWallSlinding) // If the player is sliding along the wall, but they are not wall jumping the counter is set to 1
        {
            wallJumpCounter = 1.0f;
            isWallJump = false; //player is not walljumping
            wallJumpingDirection = -transform.localScale.x;
        }

        if(Input.GetKeyDown(KeyCode.Space) && wallJumpCounter != 0.0f) //if the player press down space and their wall jump counter is not zero then they will wall jump
        {
            Debug.Log("Wall Jump!");
            wallJumpCounter = 0.0f;
            isWallJump = true;
            myRigidBody2D.velocity = new Vector2(wallJumpingDirection * wallJumpForce.x, wallJumpForce.y);
        }

        /*
            TODO: Fix infinite wall jump bug.
            Details:
            There is a bug where the player can infinitely wall jump on the connected to. It seems even if the counter is zero they can still long jump. I'm
            using a temporary solution of just having the key pressed down since the majority of players do that anyways. I will fix this shortly.
        */
    }



    /*
        -------------------
        Getters and Setters
        -------------------
    */




    public bool getWallJump()
    {
        return isWallJump;
    }
    public void setWallJump(bool isWallJump)
    {
        this.isWallJump = isWallJump;
    }
    public bool getWallCheck()
    {
        return isTouchingWall;
    }
    public void setWallCheck(bool isTouchingWall)
    {
        this.isTouchingWall = isTouchingWall;
    }
    public bool getDirRight()
    {
        return isFacingRight;
    }

    public void setDirRight(bool isFacingRight)
    {
        this.isFacingRight = isFacingRight;
    }

    public void setDirLeft(bool isFacingLeft)
    {
        this.isFacingLeft = isFacingLeft;
    }
    public bool getDirLeft()
    {
        return isFacingLeft;
    }   
    public int getHealth()
    {
        return health;
    }

    public void setHealth(int health)
    {
        this.health = health;
    }

    public float getWallSlideSpeed()
    {
        return wallSlideSpeed;
    }

    public void setWallSlideSpeed(float wallSlideSpeed)
    {
        this.wallSlideSpeed = wallSlideSpeed;
    }

    public bool getAlive()
    {
        return isAlive;
    }

    public void setAlive(bool isAlive)
    {
        this.isAlive = isAlive;
    }

    public bool getGroundCheck()
    {
        return isGrounded;
    }

    public void setGroundCheck(bool isGrounded)
    {
        this.isGrounded = isGrounded;
    }

}
