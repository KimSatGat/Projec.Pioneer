﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnumSpace;

public class Range_Player : Player
{
    public Joystick joystick;       // 조이스틱
    public Transform attackPoint;   // 공격 지점
    public Transform missilePoint;  // 미사일 발사 지점
    public Vector2 attackRange;     // 공격 범위
    public float missileSpeed;      // 미사일 스피드


    private PlayerState playerState;    // 플레이어 상태    

    private Vector3 moveVector; // 플레이어 이동벡터
    private Animator animator;  // 플레이어 애니메이터
    private new Rigidbody2D rigidbody2D;
    
    private HealthBarFade healthBarFade; // 체력바    

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void InitObject(int stageLevel)
    {        
        startingHP = 100f;
        HP = startingHP;
        damage = 30f;
        moveSpeed = 5f;
        attackSpeed = 1f;
        dead = false;
        playerState = PlayerState.IDLE;     // 플레이어 상태 초기화
        dir = 1;                            // 오른쪽 방향 할당
        moveVector = Vector3.zero;          // 플레이어 이동벡터 초기화
        missileSpeed = 50f;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        healthBarFade = GetComponentInChildren<HealthBarFade>();
    }

    void Start()
    {
        StartCoroutine("Attack");
    }

    void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        PlayerAnimation();
        PlayerMove();
    }

    // 조이스틱 입력값 받아오기
    public void HandleInput()
    {
        Vector2 moveDir = joystick.GetPlayerDir();

        moveVector = moveDir;
    }

    // 플레이어 이동    
    private void PlayerMove()
    {
        //transform.Translate(moveVector * moveSpeed * Time.deltaTime);  기존에 썻던 좌표값 변화를 통한 이동방식 -> 회전 했을때 반대 방향으로 가서 아래 방법을 씀
        rigidbody2D.velocity = new Vector2(moveVector.x * moveSpeed, moveVector.y * moveSpeed / 2);
    }

    // 플레이어 애니메이션
    private void PlayerAnimation()
    {
        // 오른쪽 이동
        if (moveVector.x > 0)
        {
            playerState = PlayerState.RUN;
            dir = 1;
            transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        }
        // 왼쪽 이동
        else if (moveVector.x < 0)
        {
            playerState = PlayerState.RUN;
            dir = -1;
            transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
        }
        else if (playerState == PlayerState.ATTACK)
        {
            return;
        }
        // 정지
        else
        {
            playerState = PlayerState.IDLE;
        }

        animator.SetInteger("State", (int)playerState);
    }

    // 공격 범위 기즈모
    public void OnDrawGizmos()
    {        
        Gizmos.color = new Color(1f, 1f, 1f, 150 / 255f);
        Gizmos.DrawCube(attackPoint.position, attackRange);
    }

    // 공격
    IEnumerator Attack()
    {
        while (true)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackRange, 0f);
            if (hits.Length > 0)
            {
                foreach (Collider2D hit in hits)
                {
                    if (hit.tag == "Enemy" && moveVector.x == 0f)
                    {
                        Enemy enemy = hit.GetComponent<Enemy>();
                        float hitPosY = enemy.GetPivot().y;

                        float offsetPosY = Mathf.Abs(hit.transform.position.y - transform.position.y);

                        if (offsetPosY <= 0.5f)
                        {
                            playerState = PlayerState.ATTACK;
                            animator.SetInteger("State", (int)playerState);
                            break;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(attackSpeed);
        }
    }

    public void FireMissile()
    {
        Player_Missile.Create(missilePoint.position, new Vector2(dir, 0f), missileSpeed, damage);
    }

    // 공격 모션이 끝 -> 적이 있다면 데미지 적용, 원래 상태로 복귀
    public void EndAttack()
    {        
        // 원래 상태로 복귀
        playerState = PlayerState.IDLE;
        animator.SetInteger("State", (int)playerState);
    }

    public override void OnDamage(float damage)
    {
        // 체력 감소
        base.OnDamage(damage);

        // HP UI 감소 효과
        healthBarFade.healthSystem.Damage((int)damage);
    }
}
