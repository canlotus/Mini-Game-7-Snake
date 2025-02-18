using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Snake : MonoBehaviour
{
    public Transform segmentPrefab;
    public Vector2Int direction = Vector2Int.right;
    public float speed = 20f;
    public float speedMultiplier = 1f;
    public float speedIncreaseFactor = 0.05f; 
    public int initialSize = 4;
    public bool moveThroughWalls = false;

    private Vector2Int input;
    private float nextUpdate;
    private Vector2 touchStart;        
    private float swipeThreshold = 50f;     

    private readonly List<Transform> segments = new List<Transform>();

    private void Start()
    {
        int difficulty = SnakeManager.Instance.GetDifficulty();

        if (difficulty == 0) 
            speed = 5f;
        else if (difficulty == 1)
            speed = 7f;
        else if (difficulty == 2)
            speed = 10f;

        ResetState();
    }

    private void Update()
    {
        HandleSwipeInput();  

    }

    private void HandleSwipeInput()
    {

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 swipeDelta = touch.position - touchStart;
                DetectSwipeDirection(swipeDelta);
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 swipeDelta = (Vector2)Input.mousePosition - touchStart;
            DetectSwipeDirection(swipeDelta);
        }
    }

    private void DetectSwipeDirection(Vector2 swipeDelta)
    {
        if (swipeDelta.magnitude > swipeThreshold)
        {
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                input = (swipeDelta.x > 0) ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                input = (swipeDelta.y > 0) ? Vector2Int.up : Vector2Int.down;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.time < nextUpdate)
        {
            return;
        }

        if (input != Vector2Int.zero)
        {
            direction = input;
            RotateHead();
        }

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        int x = Mathf.RoundToInt(transform.position.x) + direction.x;
        int y = Mathf.RoundToInt(transform.position.y) + direction.y;
        transform.position = new Vector2(x, y);

        nextUpdate = Time.time + (1f / (speed * speedMultiplier));
    }

    public void Grow()
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = segments[segments.Count - 1].position;
        segments.Add(segment);

        speedMultiplier += speedIncreaseFactor;

        SnakeManager.Instance.AddScore();
    }

    public void ResetState()
    {
        if (segments.Count > 1)
        {
            SnakeManager.Instance.GameOver();
            return;
        }

        direction = Vector2Int.right;
        transform.position = Vector3.zero;

        for (int i = 1; i < segments.Count; i++)
        {
            Destroy(segments[i].gameObject);
        }

        segments.Clear();
        segments.Add(transform);

        for (int i = 0; i < initialSize - 1; i++)
        {
            Grow();
        }

        speedMultiplier = 1f;
        SnakeManager.Instance.ResetGame();
        RotateHead();
    }

    public bool Occupies(int x, int y)
    {
        foreach (Transform segment in segments)
        {
            if (Mathf.RoundToInt(segment.position.x) == x &&
                Mathf.RoundToInt(segment.position.y) == y)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            Grow();
        }
        else if (other.gameObject.CompareTag("Obstacle2"))
        {
            SnakeManager.Instance.GameOver();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            if (moveThroughWalls)
            {
                Traverse(other.transform);
            }
            else
            {
                SnakeManager.Instance.GameOver();
            }
        }
    }

    private void Traverse(Transform wall)
    {
        Vector3 position = transform.position;

        if (direction.x != 0f)
        {
            position.x = Mathf.RoundToInt(-wall.position.x + direction.x);
        }
        else if (direction.y != 0f)
        {
            position.y = Mathf.RoundToInt(-wall.position.y + direction.y);
        }

        transform.position = position;
    }

    private void RotateHead()
    {
        float angle = 0f;

        if (direction == Vector2Int.up)
        {
            angle = 90f;
        }
        else if (direction == Vector2Int.down)
        {
            angle = -90f;
        }
        else if (direction == Vector2Int.left)
        {
            angle = 180f;
        }
        else if (direction == Vector2Int.right)
        {
            angle = 0f;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}