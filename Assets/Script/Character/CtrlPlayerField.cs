using UnityEngine;
using System.Collections;

public class CtrlPlayerField : CtrlPlayerBase {

	public enum STEP {
		NONE 			= 0,
		IDLE			,
		MOVE_AXIS_V		,
		MOVE_AXIS_H		,
		MAX				,
	}
	public STEP m_eStep;
	public STEP m_eStepPre;

	public Vector3 m_v3TargetPos;
	private Vector3 m_v3MoveDir;

	public override void start(){
		base.start();
		m_v3MoveDir = Vector3.zero;
		m_eStep = STEP.IDLE;
		m_eStepPre = STEP.MAX;
	}

	public override void move ()
	{
		//地面についているかどうか
		if( Chara.isGrounded ){
			//移動方向を取得
			//ジャンプ
//			if (EasyTouchManager.Instance.GetButton("B-Button") == ETCAxis.AxisState.Down) {
//				m_fVelocityY = m_fJumpSpeed;
//			}

		} else {
			// 重力を計算
			m_fVelocityY -= GRAVITY * Time.deltaTime;
		}
		m_v3MoveDir.x = m_v3MoveDir.x;
		m_v3MoveDir.z = m_v3MoveDir.z;
		m_v3MoveDir.y = m_fVelocityY;

		// 移動
		Chara.Move(m_v3MoveDir*m_fSpeed * Time.deltaTime);
	}

	public override void exe ()
	{
		bool bInit = false;
		if (m_eStepPre != m_eStep) {
			m_eStepPre  = m_eStep;
			bInit = true;
		}
		switch (m_eStep) {
		case STEP.IDLE:
			if (bInit) {
				m_v3MoveDir.x = 0.0f;
				m_v3MoveDir.z = 0.0f;
			}
			break;
		case STEP.MOVE_AXIS_V:
			if (bInit) {
				m_v3MoveDir = Vector3.zero;
				if (m_v3TargetPos.z - myTransform.localPosition.z < 0.0f) {
					m_v3MoveDir.z = -1.0f;
				} else {
					m_v3MoveDir.z = 1.0f;
				}
			}
			if ((m_v3TargetPos.z - myTransform.localPosition.z ) * m_v3MoveDir.z < 0.0f) {
				m_eStep = STEP.MOVE_AXIS_H;
			}
			break;
		case STEP.MOVE_AXIS_H:

			if (bInit) {
				m_v3MoveDir = Vector3.zero;
				if (m_v3TargetPos.x - myTransform.localPosition.x < 0.0f) {
					m_v3MoveDir.x = -1.0f;
					myTransform.eulerAngles = new Vector3 (0.0f, 0.0f, 0.0f);
				} else {
					myTransform.eulerAngles = new Vector3 (0.0f, 180.0f, 0.0f);
					m_v3MoveDir.x = 1.0f;
				}
			}
			if ((m_v3TargetPos.x - myTransform.localPosition.x ) * m_v3MoveDir.x < 0.0f) {
				m_eStep = STEP.IDLE;
			}
			break;

		case STEP.MAX:
		default:
			break;
		}
		return;
	}

	public void Move( Vector3 _v3TargetPos ){
		m_v3TargetPos = _v3TargetPos;
		m_eStep = STEP.MOVE_AXIS_V;
		m_eStepPre = STEP.MAX;
		return;
	}




}
