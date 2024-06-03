using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Ex;

namespace zFramework.UI
{
    public class NotificationPanel : MonoBehaviour, IBlockable
    {
        public Text title;
        public Text content;
        public Button confirmButton;
        public Button cancelButton;
        private CancellationTokenSource cts;

        public async Task<int> ShowAsync(string title, string content)
        {
            cts = new CancellationTokenSource();
            this.title.text = title;
            this.content.text = content;
            transform.localScale = Vector3.one * 0.1f;
            gameObject.SetActive(true);
            await this.BlockAsync(Color.black, 0.8f, 0.3f, 0.1f);
            await transform.DoScaleAsync(Vector3.one, 0.5f, Ease.OutBack);
            var index = await TaskExtension.WhenAny(confirmButton.OnClickAsync(cts.Token), cancelButton.OnClickAsync(cts.Token));
            _ = transform.DoScaleAsync(Vector3.one * 0.01f, 0.5f, Ease.InBack);
            await this.UnblockAsync(0.5f);
            gameObject.SetActive(false);
            cts?.Dispose();
            return index;
        }

        public async void HandleBlockClickedAsync()
        {
            await transform.DoShackPositionAsync(0.3f, Vector3.one * 20);
        }
    }
}