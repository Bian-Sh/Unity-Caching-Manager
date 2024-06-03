using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Ex;
using zFramework.UI;

public class ProgressPanel : MonoBehaviour, IBlockable
{
    public Text title;
    public Text description;
    public Text progress;
    public Button button;
    public bool IsCanceled { get; private set; }

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            IsCanceled = true;
        });
    }

    // content ��·��
    // porgress �ǽ��ȣ����磺 15/200 ����ʾ���ڴ���� 15 ���ļ����� 200 ���ļ�
    public async Task ShowAsync(string  content, string progress) 
    {
        title.text = "���ڴ���";
        description.text = content;
        this.progress.text = progress;
        transform.localScale = Vector3.one * 0.1f;
        gameObject.SetActive(true);
        await this.BlockAsync(Color.black, 0.8f, 0.3f, 0.1f);
        await transform.DoScaleAsync(Vector3.one, 0.5f, Ease.OutBack);
    }

    public async Task HideAsync()
    {
        _ = transform.DoScaleAsync(Vector3.one * 0.01f, 0.5f, Ease.InBack);
        await this.UnblockAsync(0.5f);
        gameObject.SetActive(false);
    }

    public void HandleBlockClickedAsync()
    {
        // do nothing
    }

    internal void UpdateProgress(string path, string v)
    {
        description.text = path;
        progress.text = v;
    }
}
