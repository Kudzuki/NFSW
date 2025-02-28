using System.Globalization;
using UnityEngine;

public class PlayerConroller : MonoBehaviour
{
    [Header("Контроллер персонажа")]
    [Tooltip("Скорость игрока.")]
    [SerializeField] private float _speed = 5f;
   // private Animator animator;
    private Rigidbody2D rb;
    private Vector2 movementDirection = Vector2.zero;

    private void Awake()
    {
       // animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        //CameraPlayer.Instance.FollowPlayer(transform, gameObject);
    }
    private void Update()
    {
        Movement();
    }
    private void Movement()
    {
        float _moveX = Input.GetAxis("Horizontal");
        float _moveY = Input.GetAxis("Vertical");

        //animator.SetFloat("IsMovementX", Mathf.Abs(_moveX));
        //animator.SetFloat("IsMovementY", Mathf.Abs(_moveY));

        movementDirection = new Vector2(_moveX * Time.deltaTime, _moveY * Time.deltaTime).normalized;

        if (_moveX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (_moveX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementDirection * _speed * Time.deltaTime);
    }
}
