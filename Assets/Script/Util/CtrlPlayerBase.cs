using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CtrlAnimation))]
public class CtrlPlayerBase : MonoBehaviour {

	public enum STATE
	{
		IDLE			= 0,
		WALK_SIDE		,
		WALK_FRONT		,
		WALK_BACK		,
		RUN				,
		JUMP_READY		,
		JUMP_UP			,
		JUMP_TOP		,
		JUMP_DOWN		,
		JUMP_RANDING	,
	};
	public STATE m_eState;
	public STATE m_eStatePre;

	public float m_fSpeed  = 6.0f;
	public float m_fJumpSpeed = 8.0f;
	public float GRAVITY = 20.0f;

	public Vector3 m_v3MoveDirection = Vector3.zero;

	public CtrlAnimation m_CtrlAnimation;
	public CtrlAnimation ctrlAnimation{
		get{
			if (m_CtrlAnimation == null) {
				m_CtrlAnimation = GetComponent<CtrlAnimation> ();
			}
			return m_CtrlAnimation;
		}
	}

	public CharacterController m_CharacterController;
	public CharacterController Chara{
		get{ 
			if (m_CharacterController == null) {
				m_CharacterController = GetComponent<CharacterController> ();
			}
			return m_CharacterController;
		}
	}

	// Update is called once per frame
	void Update () {
		move ();
		stateCheck ();
		return;
	}

	public virtual void move(){

		//地面についているかどうか
		if (Chara.isGrounded) {
			//移動方向を取得
			m_v3MoveDirection = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
			m_v3MoveDirection = transform.TransformDirection (m_v3MoveDirection);
			m_v3MoveDirection *= m_fSpeed;

			//ジャンプ
			if (Input.GetButton ("Jump")) {
				m_v3MoveDirection.y = m_fJumpSpeed;
			}
		} else {

			// 重力を計算
			m_v3MoveDirection.y -= GRAVITY * Time.deltaTime;
		}

		// 移動
		Chara.Move(m_v3MoveDirection * Time.deltaTime);
	}

	public virtual void stateCheck(){

		bool bInit = false;
		if (m_eStatePre != m_eState) {
			m_eStatePre  = m_eState;
			ctrlAnimation.SetMotion (m_eState);
			bInit = true;
		}

		switch (m_eState) {
		case STATE.IDLE:
			if (Chara.velocity.x < 0.0f) {
				m_eState = STATE.WALK_SIDE;
			} else if (0.0f < Chara.velocity.x) {
				m_eState = STATE.WALK_SIDE;
			} else {
			}
			break;
		case STATE.WALK_SIDE:
			if (Chara.velocity.x == 0.0f) {
				m_eState = STATE.IDLE;
			}
			break;
		case STATE.WALK_FRONT:
			break;
		case STATE.WALK_BACK:
			break;
		case STATE.JUMP_READY:
			break;
		case STATE.JUMP_UP:
			break;
		case STATE.JUMP_TOP:
			break;
		case STATE.JUMP_DOWN:
			break;
		case STATE.JUMP_RANDING:
			break;
		default:
			break;
		}



	}



}




















