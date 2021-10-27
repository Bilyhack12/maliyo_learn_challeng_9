using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private const float LANE_DISTANCE = 2.5f;
    private const float TURN_SPEED = 0.05f;
    private bool isRunning = false;
    private Animator anim;
    private CharacterController controller;
    private float jumpForce = 4.0f;
    private float gravity = 12.0f;
    private float verticalVelocity;
    private float originalSpeed = 7.0f;
    private float speed = 7.0f;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 2.5f;
    private float speedIncreaseAmount = 0.1f;
    private int desiredLane = 1; //0=Left, 1=Middle, 2=Right

    private void Start(){
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    private void Update(){
        if(!isRunning) return;

        if(Time.time - speedIncreaseLastTick > speedIncreaseTime){
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }
        // Gather the inputs on which lane we should be
        if(MobileInput.Instance.SwipeLeft)
            MoveLane(false);
        if(MobileInput.Instance.SwipeRight)
            MoveLane(true);

        // Calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if(desiredLane == 0)
            targetPosition += Vector3.left * LANE_DISTANCE;
        else if(desiredLane == 2)
            targetPosition += Vector3.right * LANE_DISTANCE;
        
        // Calculate our move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).normalized.x * speed;

        bool isGrounded = IsGrounded();
        anim.SetBool("Grounded",isGrounded);
        // Calculate Y
        if(isGrounded){
            verticalVelocity = -0.1f;
            if(MobileInput.Instance.SwipeUp){
                anim.SetTrigger("Jump");
                verticalVelocity = jumpForce;
            }
            else if(MobileInput.Instance.SwipeDown){
                StartSliding();
                Invoke("StopSliding", 1.0f);
            }
        }
        else{
            verticalVelocity -= (gravity * Time.deltaTime);

            // Fast Falling mechanic
            if(MobileInput.Instance.SwipeDown){
                verticalVelocity = -jumpForce;
            }
        }
        
        moveVector.y = verticalVelocity;
        moveVector.z = speed;

        //Move the Pengu
        controller.Move(moveVector * Time.deltaTime);
    
        // Rotate the Pengu
        Vector3 dir  = controller.velocity;
        if(dir != Vector3.zero){
            dir.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);
        }

    }

    private void StopSliding(){
        anim.SetBool("Sliding",false);
    }

    private void StartSliding(){
        anim.SetBool("Sliding",true);
        controller.height /=2;
        controller.center = new Vector3(controller.center.x, controller.center.y/2, controller.center.z);
    }

    private void MoveLane(bool goingRight){
        desiredLane += (goingRight)? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);
        Debug.Log(desiredLane);
    }

    private bool IsGrounded(){
        Ray grounRay = new Ray(new Vector3(controller.bounds.center.x, (controller.bounds.center.y - controller.bounds.extents.y) + 0.2f, controller.bounds.center.x), Vector3.down);
        return (Physics.Raycast(grounRay, 0.2f + 0.1f));
    }

    public void StartRunning(){
        isRunning = true;
        anim.SetTrigger("StartRunning");
    }
  
    private void Crash(){
        anim.SetTrigger("Death");
        isRunning = false;
        GameManager.Instance.OnDeath();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit){
        switch(hit.gameObject.tag){
            case "Obstacle":
                Crash();
                break;

        }
    }
}
