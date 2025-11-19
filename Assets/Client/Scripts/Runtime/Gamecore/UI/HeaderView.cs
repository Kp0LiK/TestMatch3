using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.UI
{
    public class HeaderView : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private SettingWindowView _settingWindow;

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
        }

        private void OnDisable()
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);
        }

        private void OnPauseButtonClick()
        {
            _settingWindow.Open();
        }
    }
}