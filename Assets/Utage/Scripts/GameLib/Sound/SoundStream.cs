using System;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// サウンド再生ストリーム
	/// 基本はシステム内部で使うので外から呼ばないこと
	/// </summary>
	[AddComponentMenu("Utage/Lib/Sound/SoundStream")]
	internal class SoundStream : MonoBehaviour
	{
		/// <summary>
		/// 状態
		/// </summary>
		enum SoundStreamStatus
		{
			None,
			Ready,
			Play,
			FadeIn,
			FadeOut,
		};
		SoundStreamStatus status = SoundStreamStatus.None;
		SoundStreamStatus Status { get { return status; } }

		AudioSource audioSource;
		AudioClip clip;
		float masterVolume = 1;

		//再生時に指定されたボリューム
		public float RequestVolume { get { return requestVolume; } }
		float requestVolume = 0;

		/// <summary>
		/// ループするかどうか
		/// </summary>
		public bool IsLoop { get { return isLoop; } }
		bool isLoop;

		/// <summary>
		/// ストリーム再生か
		/// </summary>
		public bool IsStreaming { get { return isStreaming; } }
		bool isStreaming;

		LinearValue fadeInValue = new LinearValue();
		LinearValue fadeOutValue = new LinearValue();
		Action CallBackEnd;

		/// <summary>
		/// ファイル名
		/// </summary>
		public string FileName
		{
			get
			{
				AssetFileReference file = this.GetComponent<AssetFileReference>();
				if (null != file)
				{
					return file.File.FileName;
				}
				return "";
			}
		}

		void Awake()
		{
			this.audioSource = this.gameObject.AddComponent<AudioSource>();
		}

		/// <summary>
		/// 再生するための準備
		/// </summary>
		/// <param name="clip">オーディクリップ</param>
		/// <param name="masterVolume">マスターボリューム</param>
		/// <param name="volume">再生ボリューム</param>
		/// <param name="isLoop">ループ再生するか</param>
		/// <param name="isStreaming">ストリーム再生するか</param>
		/// <param name="callBackEnd">再生終了時に呼ばれるコールバック</param>
		public void Ready(AudioClip clip, float masterVolume, float volume, bool isLoop, bool isStreaming, Action callBackEnd)
		{
			this.clip = clip;
			this.masterVolume = masterVolume;
			this.requestVolume = volume;
			this.isLoop = isLoop;
			this.isStreaming = isStreaming;
			this.CallBackEnd = callBackEnd;
			status = SoundStreamStatus.Ready;
		}

		/// <summary>
		/// 再生準備中か
		/// </summary>
		/// <returns></returns>
		public bool IsReady()
		{
			return (SoundStreamStatus.Ready == status);
		}

		/// <summary>
		/// 再生
		/// </summary>
		/// <param name="clip">オーディクリップ</param>
		/// <param name="masterVolume">マスターボリューム</param>
		/// <param name="volume">再生ボリューム</param>
		/// <param name="isLoop">ループ再生するか</param>
		/// <param name="callBackEnd">再生終了時に呼ばれるコールバック</param>
		public void Play(AudioClip clip, float masterVolume, float volume, bool isLoop, bool isStreaming, Action callBackEnd)
		{
			Ready(clip, masterVolume, volume, isLoop, isStreaming, callBackEnd);
			Play();
		}

		/// <summary>
		/// 再生
		/// </summary>
		public void Play()
		{
			if (!clip.isReadyToPlay)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.SoundNotReadyToPlay,clip.name));
			}
			audioSource.clip = clip;
			audioSource.loop = IsLoop;
			audioSource.volume = requestVolume * masterVolume;
			audioSource.Play();
			status = SoundStreamStatus.Play;
		}

		//終了
		public void End()
		{
			audioSource.Stop();
			if (null != CallBackEnd) CallBackEnd();
			GameObject.Destroy(this.gameObject);
		}

		//マスターボリュームの変更
		public void SetMasterVolume(float volume)
		{
			if (Mathf.Approximately(masterVolume, volume)) return;

			masterVolume = volume;
			if (SoundStreamStatus.Play == status)
			{
				audioSource.volume = requestVolume * masterVolume;
			}
		}

		//指定のサウンドかどうか
		public bool IsEqualClip(AudioClip clip)
		{
			return (audioSource.clip == clip);
		}

		//指定のサウンドが鳴っているか
		public bool IsPlaying(AudioClip clip)
		{
			if (IsEqualClip(clip))
			{
				return (status == SoundStreamStatus.Play);
			}
			return false;
		}
		//サウンドが鳴っているか
		public bool IsPlaying()
		{
			switch (status)
			{
				case SoundStreamStatus.FadeIn:
				case SoundStreamStatus.Play:
					return true;
				default:
					return false;
			}
		}

		//指定時間フェードアウトして終了
		public void FadeOut(float fadeTime)
		{
			CancelInvoke();
			if (fadeTime > 0 && !IsEnd())
			{
				status = SoundStreamStatus.FadeOut;
				fadeOutValue.Init(fadeTime, 1, 0);
			}
			else
			{
				End();
			}
		}

		//曲が終わっているか
		public bool IsEnd()
		{
			return (SoundStreamStatus.None == status);
		}

		public void Update()
		{
			switch (status)
			{
				case SoundStreamStatus.Play:
					UpdatePlay();
					break;
				case SoundStreamStatus.FadeIn:
					UpdateFadeIn();
					break;
				case SoundStreamStatus.FadeOut:
					UpdateFadeOut();
					break;
				default:
					break;
			}
		}
		//通常再生
		void UpdatePlay()
		{
			//再生終了
			if (!audioSource.isPlaying)
			{
				//ループしないなら終了
				if (!isLoop)
				{
					End();
				}
				else
				{
					GetComponent<AudioSource>().Play();
				}
			}
		}

		//フェードイン処理
		void UpdateFadeIn()
		{
			fadeInValue.IncTime();
			audioSource.volume = fadeInValue.GetValue() * requestVolume * masterVolume;
			if (fadeInValue.IsEnd())
			{
				status = SoundStreamStatus.Play;
			}
		}

		//フェードアウト処理
		void UpdateFadeOut()
		{
			fadeOutValue.IncTime();
			audioSource.volume = fadeOutValue.GetValue() * requestVolume * masterVolume;
			if (fadeOutValue.IsEnd())
			{
				End();
			}
		}
	};
}
