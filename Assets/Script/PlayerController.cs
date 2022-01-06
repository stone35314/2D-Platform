using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]    //将私有的变量显示在组件中
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D colBox;
    private CircleCollider2D colCicle;

    public float speed = 265f;
    public float jumpForce = 350f;
    public float horizontalMovement;
    public float faceDirection;

    public LayerMask ground;  //判断碰撞的层级

    [Header("樱桃总数")]
    [SerializeField]
    private int cherryCount;

    public Text cherryCountText;

    private bool isHurt = false; //判断当前是否是受伤状态

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        colBox = GetComponent<BoxCollider2D>();
        colCicle = GetComponent<CircleCollider2D>();
    }

    private void FixedUpdate()
    {
        if(isHurt == false)
        {
            Movement();
            Jump();
        }
        
        SwitchAnim();
    }

    void Movement()
    {
        horizontalMovement = Input.GetAxis("Horizontal");   //返回 -1 ～1的值，大于0 朝右，小于0 朝左
        faceDirection = Input.GetAxisRaw("Horizontal");    //返回 0,1,-1 三个值，用于判断角色的sprite 朝向
        if(faceDirection !=0)
        {
            transform.localScale = new Vector3(faceDirection, 1, 1);   //角色朝右 scale = 1, 角色朝左 scale = -1
        }

        rb.velocity = new Vector2(horizontalMovement * speed *Time.deltaTime , rb.velocity.y);
        anim.SetFloat("running", Mathf.Abs(faceDirection));               //使用返回的绝对值来判断是否运动了

    }

    void Jump()
    {
        if(Input.GetButtonDown("Jump") && colCicle.IsTouchingLayers(ground))           //防止无限跳跃
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.deltaTime);
            anim.SetBool("jumping", true);
        }
    }

    void SwitchAnim()
    {
        anim.SetBool("idle", false);        //一开始先将idle参数设置成false ，因为落到地面后 会一直是true
        anim.SetBool("crouch", false);
        colBox.isTrigger = false;

        if (anim.GetBool("jumping"))
        {
            if(rb.velocity.y < 0)      //跳跃后角色掉落 y轴速度将小于0，判断已经在下落
            {
                anim.SetBool("jumping", false);
                anim.SetBool("falling", true);
            }
        }
        else if(isHurt) //判断当前回弹的速度是否小于0.1f，是的话就切换回未受伤状态
        {
            anim.SetBool("hurt", true);
            if(Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                anim.SetBool("hurt", false);
                anim.SetBool("idle", true);
                isHurt = false;
            }
        }
        else if (colCicle.IsTouchingLayers(ground))    //判断是否接触到地面，接触到的话就从 fall状态切换回idle
        {
            anim.SetBool("falling", false);
            anim.SetBool("idle", true);

            if(Input.GetKey(KeyCode.LeftShift))
            {
                anim.SetBool("crouch", true);
                colBox.isTrigger = true;
            }
        }
    }

    //拾取物品，显示在UI中
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collectable")
        {
            Destroy(collision.gameObject);
            cherryCount += 1;
            cherryCountText.text = cherryCount.ToString();
        }
    }

    //碰到敌人
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            if(anim.GetBool("falling"))
            {
                Destroy(collision.gameObject);
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.deltaTime);
                anim.SetBool("jumping", true);
            }
            else if( transform.position.x < collision.gameObject.transform.position.x) //玩家的x值小于敌人的，说明在左侧碰到敌人
            {
                rb.velocity = new Vector2(-5, rb.velocity.y);
                isHurt = true;
            }
            else if (transform.position.x > collision.gameObject.transform.position.x) //玩家的x值大于敌人的，说明在右侧碰到敌人
            {
                rb.velocity = new Vector2(5, rb.velocity.y);
                isHurt = true;
            }
        }
    }


}
