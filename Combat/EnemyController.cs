using System.Collections;
using UnityEngine;

public enum MovementState { Idle, Moving, Hiding }
public enum MovementType { Never, Always, StartStop }
public class EnemyController : MonoBehaviour
{
    [ReadOnly] public MovementState movementState;
    public MovementType movementType;

    [ReadOnly] public bool isGrounded;
   // public GroundCheck groundCheck;
    public float moveSpeed = 5f;
    public bool isMovingRight = true;
    public bool shouldMove;

    public float ledgeDetectionDistance = 1f, wallDetectionDistance = 0.5f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    Rigidbody rb;


    public Transform[] ledgeDetectors;

    [Header("MoveTimes")]
    public float moveTimeMin = 1;
    public float moveTimeMax = 3, waitTimeMin = 1, waitTimeMax = 3;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementState = MovementState.Idle;

        // Random short start delay
        var randomStartDelay = Random.Range(0, 1);
        Invoke(nameof(DetermineAction), randomStartDelay);
    }

    public void DetermineAction()
    {
        switch (movementType)
        {
            case MovementType.Always:
                shouldMove = true;
                movementState = MovementState.Moving;
                break;

            case MovementType.StartStop:
                StartCoroutine(BeginStartStopMovement());
                break;

            case MovementType.Never:
                shouldMove = false;
                movementState = MovementState.Idle;
                break;

        }
    }

    private void Update()
    {
        // TODO
        //HandleAnimation();

        // TEST
        //if (Input.GetKey(KeyCode.Keypad4))
        //{
        //    MoveHorizontal(-1);
        //}
        //else if (Input.GetKey(KeyCode.Keypad6))
        //{
        //    MoveHorizontal(1);
        //}
    }



    void FixedUpdate()
    {
       // isGrounded = groundCheck.isGrounded;

        if (shouldMove)
        {
            movementState = MovementState.Moving;
            MoveHorizontal(isMovingRight ? 1 : -1);

            // Check for ledges
            RaycastHit groundHit;
            foreach (var detector in ledgeDetectors)
            {
                if (!Physics.Raycast(detector.position, Vector3.down, out groundHit, ledgeDetectionDistance, groundLayer))
                {
                    ReverseDirection();
                }
            }

            // Check for walls
            RaycastHit wallHit;
            if (Physics.Raycast(transform.position, isMovingRight ? Vector3.right : Vector3.left, out wallHit, wallDetectionDistance, wallLayer))
            {
                ReverseDirection();
            }
            
        }
    }

    IEnumerator BeginStartStopMovement()
    {
        var moveTime = Random.Range(moveTimeMin, moveTimeMax);
        var waitTime = Random.Range(waitTimeMin, waitTimeMax);

        shouldMove = true;
        movementState = MovementState.Moving;
        yield return new WaitForSeconds(moveTime);

        shouldMove = false;
        movementState = MovementState.Idle;
        yield return new WaitForSeconds(waitTime);

        DetermineAction();
    }

    void MoveHorizontal(int moveDir)
    {
        rb.velocity = new Vector3(moveDir * moveSpeed, rb.velocity.y, 0f);
    }

    void ReverseDirection()
    {
        print($"{name} reversed direction");
        isMovingRight = !isMovingRight;
    }

    public void Stop()
    {
        rb.velocity = Vector3.zero;
    }

}
