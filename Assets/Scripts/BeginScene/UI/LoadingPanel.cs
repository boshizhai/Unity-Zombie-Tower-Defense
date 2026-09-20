using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : BasePanel
{
    public Image imgProgress;
    public Text txtProgress;
    public Text txtLoading;
    //是否加载脚本的标记
    private bool isLoading;

    public override void ShowMe()
    {
        base.ShowMe();
        //游戏暂停时无法淡入，直接显示加载面板
        GetComponent<CanvasGroup>().alpha = 1;
    }
    //提供异步加载场景的接口给外部
    public void LoadScene(string sceneName, UnityAction loadComplete = null)
    {
        //放置重复点击导致的通过加载多个场景
        if (isLoading)
            return;
        isLoading = true;
        StartCoroutine(LoadSceneAsync(sceneName, loadComplete));
    }

    //异步加载场景并更新进度条
    private IEnumerator LoadSceneAsync(string sceneName, UnityAction loadComplete)
    {
        imgProgress.fillAmount = 0;
        txtProgress.text = "0%";
        txtLoading.text = "游戏加载中";

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        //加载完成前暂停不自动进入新场景
        operation.allowSceneActivation = false;

        //进度条控制最少显示多少秒
        float minLoadingTime = 1.5f;
        float displayProgrress = 0f;

        while (operation.progress < 0.9f || displayProgrress < 1f)
        {
            //Unity异步加载进度最大显示为0.9 需要转换成0—1
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            //使用不受Time.timeScale影响的事件平滑增加
            displayProgrress = Mathf.MoveTowards(
                displayProgrress,
                realProgress,
                Time.unscaledDeltaTime / minLoadingTime
            );

            imgProgress.fillAmount = displayProgrress;
            txtProgress.text = Mathf.RoundToInt(displayProgrress * 100) + "%";
            yield return null;
        }

        //场景加载已经准备完毕
        imgProgress.fillAmount = 1f;
        txtProgress.text = 100 + "%";
        txtLoading.text = "加载完成!";

        //让100%显示稍微一段时间
        yield return new WaitForSecondsRealtime(0.5f);

        operation.allowSceneActivation = true;

        //等待场景真正切换完成
        while (!operation.isDone)
        {
            yield return null;
        }

        loadComplete?.Invoke();
        //切换场景完成后隐藏加载面板
        UIMgr.Instance.HidePanel<LoadingPanel>(false);
    }

    protected override void Init()
    {
    }
}