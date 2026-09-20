using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PoolSound : MonoBehaviour
{
    private AudioSource audioSource;
    private GameObject sourcePrefab;

    public void Play(GameObject prefab, AudioClip clip, float volume)
    {
        // 记录回收时应该放进哪个池。
        sourcePrefab = prefab;

        // 对象取出时尚未激活，在这里获取组件。
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = volume;

        gameObject.SetActive(true);
        audioSource.Play();
    }

    private void Update()
    {
        // 还在播放，就继续等待。
        if (audioSource.isPlaying)
            return;

        // 播放结束，清空音频引用并回收。
        audioSource.clip = null;
        PoolMgr.Instance.PushObj(sourcePrefab, gameObject);
    }
}