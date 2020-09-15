using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControlFox : MonoBehaviour
{
    [Header("Components")]

    public Rigidbody2D rb;
    public Animator animator;
    public GameObject foxTail; //The foxes tail component
    public ParticleSystem teleportParticles; //The particle system when the fox teleports/dashes
    public CameraShake cameraShake; //The component that causes cera shakes

    public ParticleSystem jump; 
    public ParticleSystem landJump;
    public ParticleSystem run;
    public ParticleSystem wallJump;
    public ParticleSystem airJump;
    public ParticleSystem wallSlide;

    //These are all for the particles emmited when the fox performs an action

    [Space]
    [Header("Information")]

    public bool jumping; //is the fox jumping
    public bool teleport; //is the fox teleporting/dashing
    public int allowedJumps; // are we allowed mid-air jumps, and how many
    public int remainingjumps; // jumps remaining, resets when landing

    [Space]
    [Header("Basic Stats")]
    public float speed; //Run speed of the fox
    public float jumpForce; //Upwards force of a ground jump
    public float secondJumpForce; //Upwards force from an air jump
    public int maxTeleports; //what is the most dash/teleport crystals that can be collected/used in one go
    public float gravity; //force of gravity on the fox
    public float drag; //air drag force on the fox

    public LayerMask groundLayers; //The layers that we allow the fox to consider as the ground, so it know when it can jump and land

    [Space]
    [Header("Advanced Stats")]
    public float accelerationTime; //how quickly the fox speeds up when running to it's max speed - speed
    public float decelerationTime; //how quick the fox will slow down when we stop the input
    public float accelerationTimeAir; //the foxes acceleration in the air
    public float decelerationTimeAir; //the foxes deceleration in the air
    public float teleportTime; // how long a teleport/dash lasts
    public float teleportSpeed; //how fast the fox movess when teleporting/dashing
    public float teleportStartDelay; //a slight delay before the dash begins
    public float teleportEndDelay; //a slight delay as the dash ends
    public float wallJumpSpeed; //How fast the fox will jump horizontally from a wall
    public float wallJumpSpeedVertical; //How fast vertically the fox will jump from a wall



    public float earlyJumpTime; //If the player presses jump before they hit the ground, if they hit the ground within this time, they will still jump
    public float lateJumpTime; // If the player hits jump after leaving a ground layer, if it's within this time since they stepped off the platform, they will still jump
    public float lateWallJumpTime; //Same as lateJumpTime but for wall jumping


    [Space]
    [Header("Other Stats")]
    public float cameraShakeDuration;
    public float cameraShakeMagnitude;
    public float stepTime;
    public float stepOffset;


    [Space]
    [Header("Debug Info")]
    public float accelerationtimer;
    public float decelerationtimer;
    public float teleporttimer;
    public Vector2 teleportDirection;
    public int teleportPhase;
    public int remainingTeleports;
    public bool ableToWallJump;
    public float earlyjumptimer;
    public float latejumptimer;
    public float latewalljumptimer;




    Vector2 dir;

    public bool canMove;
    public int direction;

    public bool isGrounded;
    public bool isAgainstWallLeft;
    public bool isAgainstWallRight;
    public bool wasGrounded;
    public bool wasAgainstWallLeft;
    public bool wasAgainstWallRight;

    public bool notAbleToClimbLedge;


    float xRaw;
    float yRaw;
    float x;
    float y;

    private float steptimer;


    private bool airJumpCheck;

    bool landing;
    public float landTime;
    private float landtimer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        rb.drag = drag;

        earlyjumptimer = earlyJumpTime;
        latejumptimer = lateJumpTime;
    }

    void Update()
    {
        //This little section considers the sound of the foxes footsteps and when to play the sounds

        if (xRaw != 0&&isGrounded)
        {
            steptimer += Time.deltaTime;
            if (steptimer > stepTime - 0.05) 
            {
                FindObjectOfType<SFXManager>().Play("Step2");
                
            }

            if (steptimer > stepTime)
            {
                FindObjectOfType<SFXManager>().Play("Step");
                steptimer = 0;
            }
        }
        else
        {
            steptimer = stepOffset;
        }

        

        //Taking X and Y imputs

        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");



        //Checking if this frame the player is touching the ground, against a wall, able to climb up a ledge

        bool groundtest = isGrounded;

        isGrounded = Physics2D.OverlapArea(new Vector2(transform.position.x - this.GetComponent<Collider2D>().bounds.size.x / 2.5f , transform.position.y - this.GetComponent<Collider2D>().bounds.size.y / 2+ this.GetComponent<Collider2D>().offset.y),
        new Vector2(transform.position.x + this.GetComponent<Collider2D>().bounds.size.x / 2.5f, transform.position.y - this.GetComponent<Collider2D>().bounds.size.y / 2-0.01f + this.GetComponent<Collider2D>().offset.y), groundLayers) ;

        isAgainstWallLeft = Physics2D.OverlapArea(new Vector2(transform.position.x - this.GetComponent<Collider2D>().bounds.size.x / 2 - 0.1f, transform.position.y - this.GetComponent<Collider2D>().bounds.size.y / 2.5f),
        new Vector2(transform.position.x - this.GetComponent<Collider2D>().bounds.size.x / 2, transform.position.y), groundLayers);

        isAgainstWallRight = Physics2D.OverlapArea(new Vector2(transform.position.x + this.GetComponent<Collider2D>().bounds.size.x / 2f, transform.position.y - this.GetComponent<Collider2D>().bounds.size.y / 2.5f),
        new Vector2(transform.position.x + this.GetComponent<Collider2D>().bounds.size.x / 2 + 0.1f, transform.position.y), groundLayers);


        notAbleToClimbLedge = true;
        if (isAgainstWallRight)
        {
            notAbleToClimbLedge= Physics2D.OverlapArea(new Vector2(transform.position.x + this.GetComponent<Collider2D>().bounds.size.x / 2f, transform.position.y + this.GetComponent<Collider2D>().bounds.size.y / 1f),
        new Vector2(transform.position.x + this.GetComponent<Collider2D>().bounds.size.x / 2 + 0.1f, transform.position.y + this.GetComponent<Collider2D>().bounds.size.y /2f), groundLayers);
            
        }
        else if (isAgainstWallLeft)
        {
            notAbleToClimbLedge = Physics2D.OverlapArea(new Vector2(transform.position.x - this.GetComponent<Collider2D>().bounds.size.x / 2 - 0.1f, transform.position.y + this.GetComponent<Collider2D>().bounds.size.y / 1f),
        new Vector2(transform.position.x - this.GetComponent<Collider2D>().bounds.size.x / 2, transform.position.y + this.GetComponent<Collider2D>().bounds.size.y /2f), groundLayers);
        }


        //If we've just landed on a wall, play a sound

        if (wasAgainstWallLeft == false && isAgainstWallLeft)
        {
            FindObjectOfType<SFXManager>().Play("WallLand");
        }

        if (wasAgainstWallRight == false && isAgainstWallRight)
        {
            FindObjectOfType<SFXManager>().Play("WallLand");
        }


        //Update prescision timers

        if (earlyjumptimer < earlyJumpTime)
        {
            earlyjumptimer += Time.deltaTime;
        }
        if (latejumptimer < lateJumpTime)
        {
            latejumptimer += Time.deltaTime;
        }
        if (latewalljumptimer < lateWallJumpTime)
        {
            latewalljumptimer += Time.deltaTime;
        }
        else
        {
            wasAgainstWallLeft = false;
            wasAgainstWallRight = false;
        }




        //Check if he fox has just left the ground but not by jumping, if so we start the late jump timer, to check if the player presses it slightly late

        if (groundtest == true && isGrounded == false&&jumping==false)
        {
            latejumptimer = 0;
        }



        if (isGrounded)
        {

            //if the fox is now on the ground, but the player pressed jump a bit early, yet still inside the early jump threshold, we tell the fox to jump
            if (earlyjumptimer < earlyJumpTime)
            {
                jumping = true;
                this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x, jumpForce);
                latejumptimer = lateJumpTime;
                earlyjumptimer = earlyJumpTime;
            }

            //if the was is now on the ground, but wasn't in the last frame (so it's just landed) then we emit some particles and make a landing sound
            if (wasGrounded == false)
            {
                GameObject jumpparticles = Instantiate(landJump.gameObject, landJump.transform.position, landJump.transform.rotation);
                jumpparticles.SetActive(true);
                FindObjectOfType<SFXManager>().Play("Landing");
                landing = true;
            }
        }


        //if the fox is againt a wall, note for the next frame it was agains a wall

            if (isAgainstWallLeft)
            {
                wasAgainstWallLeft = true;
            }

            if (isAgainstWallRight)
            {
                wasAgainstWallRight = true;
            }


        if (isAgainstWallRight && isGrounded == false|| isAgainstWallLeft && isGrounded == false) //If the fox is against a wall but on the ground
        {
            wallSlide.Play();
            if (teleport == false) //We wont be able to hold or slide down wall when teleporting
            {


                if (Input.GetButton("Hold Wall")) //If the player is pressing the hold wall button
                {

                        ableToWallJump = true;
                        animator.SetInteger("Wall", 1);
                        rb.velocity = new Vector2(0, 0);
                        rb.gravityScale = 0;
                        ableToWallJump = true;

                }
                else //If it's not we do a similar thing, but if they are leaning their horizontal direction into teh wall their fall is damp
                {

                    rb.gravityScale = gravity;
                    if (isAgainstWallRight)
                    {
                        if (xRaw > 0.1)
                        {
                            if (rb.velocity.y <= 0)
                            {
                                ableToWallJump = true;
                                animator.SetInteger("Wall", 2);
                                rb.velocity = new Vector2(0, 0);
                                }
                            }
                    }

                        else
                        {
                            if (xRaw < -0.1)
                            {
                                if (rb.velocity.y <= 0)
                                {
                                    ableToWallJump = true;
                                    animator.SetInteger("Wall", 2);
                                    rb.velocity = new Vector2(0, 0);

                                }
                            }
                        }
                    
                }



                //If they are against a wall but are pushing away, we start the late walljump timer and diable wall jumping after that

                if (isAgainstWallRight)
                {
                    if (xRaw < -0.1)
                    {
                        latewalljumptimer = 0;
                        ableToWallJump = false;
                    }
                }
                else
                {
                    if (xRaw > 0.1)
                    {
                        latewalljumptimer = 0;
                        ableToWallJump = false;
                    }
                }


            }


            //if the fox is in a position to climp ledges

            if (notAbleToClimbLedge == false&&teleport==false)
            {
                if (isAgainstWallRight)
                {
                    animator.SetBool("Climbing", true);
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x+20, 37);
                }
                else if
                    (isAgainstWallLeft)
                {
                    animator.SetBool("Climbing", true);
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x - 20, 37);
                }

            }
            else
            {
                animator.SetBool("Climbing", false);
            }




        }
        else
        {
            wallSlide.Stop();
            rb.gravityScale = gravity;
            animator.SetInteger("Wall", 0);
            ableToWallJump = false;
            animator.SetBool("Climbing", false);
        }




        //If the fox has just landed

        if (landing)
        {
            animator.SetBool("Landing", true);

            landtimer += Time.deltaTime;

            if (landtimer > landTime)
            {
                landing = false;
                landtimer = 0;
                animator.SetBool("Landing", false);
            }

        }



        if (Input.GetButtonDown("Jump")) // if the plater presses jump
        {
            if (isGrounded) //We are on the ground when jump is pressed
            {
                FindObjectOfType<SFXManager>().Play("Jump");
                jumping = true;
                this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x, jumpForce);
                latejumptimer = lateJumpTime;

                GameObject jumpparticles= Instantiate(jump.gameObject, jump.transform.position, jump.transform.rotation);
                jumpparticles.SetActive(true);
                //jump.Play();
            }
            else if (ableToWallJump) //we are able to do a wall jump
            {

                wallSlide.Stop();
                print("WALL JUMP");
                FindObjectOfType<SFXManager>().Play("Jump");
                jumping = true;
                //wallJump.Play();
                GameObject jumpparticles = Instantiate(wallJump.gameObject, wallJump.transform.position, wallJump.transform.rotation);
                jumpparticles.SetActive(true);
                if (wasAgainstWallLeft)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-wallJumpSpeed, wallJumpSpeedVertical);
                }
                else
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(wallJumpSpeed, wallJumpSpeedVertical);
                }
                //print(this.gameObject.GetComponent<Rigidbody2D>().velocity.x);
                //this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x, jumpForce);

                //direction = -direction;
                ResetSpeedAir();


                
                
            }
            else if (latejumptimer<lateJumpTime) //We've just left the platform but able to jump still 
            {
                FindObjectOfType<SFXManager>().Play("Jump");
                GameObject jumpparticles = Instantiate(jump.gameObject, jump.transform.position, jump.transform.rotation);
                jumpparticles.SetActive(true);
                jumping = true;
                this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x, jumpForce);
                latejumptimer = lateJumpTime;
            }
            else if (latewalljumptimer < lateWallJumpTime) //We've just left the wall but able to jump still
            {
                FindObjectOfType<SFXManager>().Play("Jump");
                wallSlide.Stop();
                GameObject jumpparticles = Instantiate(wallJump.gameObject, wallJump.transform.position, wallJump.transform.rotation);
                jumpparticles.SetActive(true);
                jumping = true;
                //print("                                                           LATE WALL JUMP");
                if (wasAgainstWallLeft)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-wallJumpSpeed, wallJumpSpeedVertical);
                }
                else
                {
                    this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(wallJumpSpeed, wallJumpSpeedVertical);
                }

                latewalljumptimer = lateJumpTime;
                //this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x, jumpForce);



                //direction = -direction;
                ResetSpeedAir();
                print("ACCELERATION TIMER: " + accelerationtimer);
            }
            else if(remainingjumps>0) //We have midair jumps remaining
            {
                FindObjectOfType<SFXManager>().Play("Jump2");
                GameObject jumpparticles = Instantiate(airJump.gameObject, airJump.transform.position, airJump.transform.rotation);
                jumpparticles.SetActive(true);
                remainingjumps -= 1;
                jumping = true;

                this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(this.gameObject.GetComponent<Rigidbody2D>().velocity.x, secondJumpForce);


                airJumpCheck = true;


            }
            else
            {
                earlyjumptimer = 0;
            }



        }


        

        if (Input.GetButtonDown("Teleport")&&remainingTeleports>0) //If we teleport and are allowed to teleport
        {
            //Start Teleport

            remainingTeleports -= 1;
            isGrounded = false;
            teleport = true;
            print("Teleport");
            this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            teleporttimer = 0;
            teleportPhase = 0;
           
        }


        //Set the direction of the fox, based on it's x veloity

        if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x > 0) { direction = 1;}
        if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x == 0) { direction = 0;}
        if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x < 0) { direction = -1;}

        if (this.transform.localScale.x == -1)
        {
            if (xRaw > 0)
            {
                this.transform.localScale = new Vector2(1, 1);
            }
        }
        else if(this.transform.localScale.x == 1)
        {
            if (xRaw < 0)
            {
                this.transform.localScale = new Vector2(-1, 1);
            }
        }


        //Here we control the velocity of the fox if not teleporting, both in the air and on the ground. This dictates the acceleration and speed depending on the playes inputs

        if (!teleport)
        {
            if (isGrounded == false)
            {
                if (wasGrounded)
                {
                    wasGrounded = false;
                    accelerationtimer = accelerationtimer * accelerationTimeAir / accelerationTime;
                    decelerationtimer = decelerationtimer * decelerationTimeAir / decelerationTime;
                    print("Took Off");
                }

                ControlAirSpeed();

            }
            else
            {

                if (wasGrounded==false)
                {
                    wasGrounded = true;
                    accelerationtimer = accelerationtimer * accelerationTime / accelerationTimeAir;
                    decelerationtimer = decelerationtimer * decelerationTime / decelerationTimeAir;
                    print("Landed");
                }

                ControlRunSpeed();
            }
        }
        else
        {
            if (isGrounded == false)
            {
                ResetSpeedAir();
            }
            else
            {
                ResetSpeedGround();
            }
        }

        animator.SetFloat("Speed",Mathf.Abs(xRaw));


        //Now controlling animation layers

        if (isGrounded == false)
        {
            animator.SetLayerWeight(0, 0);
            animator.SetLayerWeight(1, 1);

            animator.SetFloat("VerticalSpeed", GetComponent<Rigidbody2D>().velocity.y);
        }
        else
        {
            remainingjumps = allowedJumps;
            animator.SetLayerWeight(0, 1);
            animator.SetLayerWeight(1, 0);
        }

        if (airJumpCheck)
        {
            animator.SetLayerWeight(0, 1);
            animator.SetLayerWeight(1, 0);
            airJumpCheck = false;
            animator.SetFloat("VerticalSpeed", -1);
        }
        



        if (teleport) //If the fox is dashing/teleporting
        {
            teleporttimer += Time.deltaTime;

            if (teleportPhase == 0)
            {
                TeleportDirection();
                if (teleporttimer > teleportStartDelay)
                {
                    //Start Teleport
                    TeleportDirection();
                    print(teleportDirection);
                    rb.velocity = teleportDirection * teleportSpeed;
                    teleportPhase = 1;
                    rb.gravityScale = 0;
                    rb.drag = 0;
                    animator.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    foxTail.GetComponent<SpriteRenderer>().enabled = false;
                    teleportParticles.Play();
                    StartCoroutine(cameraShake.Shake(cameraShakeDuration, cameraShakeMagnitude));
                }
            }
            else if (teleportPhase == 1)
            {
                if (teleporttimer > teleportTime - teleportEndDelay)
                {
                    rb.velocity = new Vector2(rb.velocity.x/4, rb.velocity.y / 4);
                    teleportPhase = 2;
                    animator.gameObject.GetComponent<SpriteRenderer>().enabled = true;
                    foxTail.GetComponent<SpriteRenderer>().enabled = true;
                    teleportParticles.Stop();
                }
            }
            else if (teleportPhase == 2)
            {
                if (teleporttimer > teleportTime)
                {
                    rb.gravityScale = gravity;
                    rb.drag=drag;
                    teleport = false;
                }
            }
            
        }


        

    }



    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);

    }


    

    public void TeleportDirection()
    {
        if (xRaw < 0.5 && xRaw > -0.5 && yRaw > 0.5)
        {
            //Up
            teleportDirection = new Vector2(0, 1);
        }

        if (xRaw > 0.5 && yRaw > 0.5)
        {
            //Up-Right
            teleportDirection = new Vector2(0.66f, 0.66f);
        }

        if (xRaw > 0.5 && -0.5 < yRaw && yRaw < 0.5)
        {
            //Right
            teleportDirection = new Vector2(1, 0);
        }

        if (xRaw > 0.5 && yRaw < -0.5)
        {
            //Down-Right
            teleportDirection = new Vector2(0.66f, -0.66f);
        }

        if (xRaw < 0.5 && xRaw > -0.5 && yRaw < -0.5)
        {
            //Down
            teleportDirection = new Vector2(0, -1);
        }

        if (xRaw < -0.5 && yRaw < -0.5)
        {
            //Down-Left
            teleportDirection = new Vector2(-0.66f, -0.66f);
        }

        if (xRaw < -0.5 && yRaw < 0.5 && yRaw > -0.5)
        {
            //Left
            teleportDirection = new Vector2(-1, 0);
        }

        if (xRaw < -0.5 && yRaw > 0.5)
        {
            //Up-Left
            teleportDirection = new Vector2(-0.66f, 0.66f);
        }

    }


    void ControlRunSpeed()
    {


        if (xRaw > 0.01)
        {
            if (direction == 1 || direction == 0)
            {
                direction = 1;
                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    decelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * decelerationTime;
                }


                if (accelerationtimer < accelerationTime)
                {
                    accelerationtimer += Time.deltaTime;
                    dir = new Vector2(direction * accelerationtimer / accelerationTime, y);
                }
                else
                {
                    //dir = new Vector2(direction, y);
                    dir = new Vector2(direction * accelerationtimer / accelerationTime, y);
                }
            }
            if (direction == -1)
            {

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTime;
                }

                if (decelerationtimer > 0)
                {
                    decelerationtimer -= Time.deltaTime * 2;
                    dir = new Vector2(direction * ((decelerationtimer / decelerationTime)), y);
                }
                else
                {
                    dir = new Vector2(0, y);
                    direction = 0;
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    accelerationtimer = 0;
                    decelerationtimer = 0;
                }
            }
        }


        if (xRaw < -0.01)
        {
            if (direction == -1 || direction == 0)
            {
                direction = -1;

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    decelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * decelerationTime;
                }


                if (accelerationtimer < accelerationTime)
                {
                    accelerationtimer += Time.deltaTime;
                    dir = new Vector2(direction * accelerationtimer / accelerationTime, y);
                    
                }
                else
                {
                    //dir = new Vector2(direction, y);
                    dir = new Vector2(direction * accelerationtimer / accelerationTime, y);

                }
            }
            if (direction == 1)
            {

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTime;
                }


                if (decelerationtimer > 0)
                {
                    decelerationtimer -= Time.deltaTime * 2;
                    dir = new Vector2(direction * ((decelerationtimer / decelerationTime)), y);
                }
                else
                {
                    dir = new Vector2(0, y);
                    direction = 0;
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    accelerationtimer = 0;
                    decelerationtimer = 0;
                }
            }
        }


        if (xRaw == 0)
        {
            if (direction == -1 || direction == 1)
            {

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTime;
                }


                decelerationtimer -= Time.deltaTime;
                if (decelerationtimer > 0)
                {

                    dir = new Vector2(direction * ((decelerationtimer / decelerationTime)), y);
                }
                else
                {
                    dir = new Vector2(0, y);
                    direction = 0;
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    accelerationtimer = 0;
                    decelerationtimer = 0;
                }

            }
            if (direction == 0)
            {
                decelerationtimer = 0;
                accelerationtimer = 0;
                rb.velocity = new Vector2(0, rb.velocity.y);

            }
        }
        Walk(dir);
    }



    void ControlAirSpeed()
    {
        ResetSpeedAir();

        if (xRaw > 0.01)
        {
            if (direction == 1 || direction == 0)
            {
                direction = 1;
                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    decelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * decelerationTimeAir;
                }


                if (accelerationtimer < accelerationTimeAir)
                {
                    accelerationtimer += Time.deltaTime;
                    dir = new Vector2(direction * accelerationtimer / accelerationTimeAir, y);
                }
                else
                {

                    //dir = new Vector2(direction, y);
                    dir = new Vector2(direction * accelerationtimer / accelerationTimeAir, y);
                }
            }
            if (direction == -1)
            {

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTimeAir;
                }

                if (decelerationtimer > 0)
                {
                    decelerationtimer -= Time.deltaTime * 2;
                    dir = new Vector2(direction * ((decelerationtimer / decelerationTimeAir)), y);
                }
                else
                {
                    //dir = new Vector2(0, y);
                    direction = 0;
                    //rb.velocity = new Vector2(0, rb.velocity.y);
                    accelerationtimer = 0;
                    decelerationtimer = 0;
                }
            }
        }


        if (xRaw < -0.01)
        {
            if (direction == -1 || direction == 0)
            {
                direction = -1;

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    decelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * decelerationTimeAir;
                }


                if (accelerationtimer < accelerationTimeAir)
                {
                    accelerationtimer += Time.deltaTime;
                    dir = new Vector2(direction * accelerationtimer / accelerationTimeAir, y);
                }
                else
                {
                    
                    //dir = new Vector2(direction, y);
                    dir = new Vector2(direction * accelerationtimer / accelerationTimeAir, y);
                }
            }
            if (direction == 1)
            {

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTimeAir;
                }


                if (decelerationtimer > 0)
                {
                    decelerationtimer -= Time.deltaTime * 2;
                    dir = new Vector2(direction * ((decelerationtimer / decelerationTimeAir)), y);
                }
                else
                {
                    //dir = new Vector2(0, y);
                    direction = 0;
                    //rb.velocity = new Vector2(0, rb.velocity.y);
                    accelerationtimer = 0;
                    decelerationtimer = 0;
                }
            }
        }


        if (xRaw == 0)
        {
            if (direction == -1 || direction == 1)
            {

                if (this.gameObject.GetComponent<Rigidbody2D>().velocity.x != 0)
                {
                    accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTimeAir;
                }


                decelerationtimer -= Time.deltaTime;

                if (decelerationtimer > 0)
                {

                    dir = new Vector2(direction * ((decelerationtimer / decelerationTimeAir)), y);
                }
                else
                {
                    //dir = new Vector2(0, y);
                    direction = 0;
                    //rb.velocity = new Vector2(0, rb.velocity.y);
                    accelerationtimer = 0;
                    decelerationtimer = 0;
                }

            }
            if (direction == 0)
            {
                decelerationtimer = 0;
                accelerationtimer = 0;
                rb.velocity = new Vector2(0, rb.velocity.y);

            }
        }



        Walk(dir);


    }

    public void ResetSpeedAir()
    {
        decelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * decelerationTimeAir;
        accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTimeAir;
    }

    public void ResetSpeedGround()
    {
        decelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * decelerationTime;
        accelerationtimer = Mathf.Abs(this.gameObject.GetComponent<Rigidbody2D>().velocity.x) / speed * accelerationTime;
    }


    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(new Vector2(transform.position.x + this.GetComponent<Collider2D>().bounds.size.x / 2 +0.05f, transform.position.y - this.GetComponent<Collider2D>().bounds.size.y / 2.5f),
            new Vector2(0.05f,0.05f));
    }

}
