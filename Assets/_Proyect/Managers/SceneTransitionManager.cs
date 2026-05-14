using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonLegacy.Managers
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 0.5f;

        private void Awake()
        {
            ServiceLocator.Register<SceneTransitionManager>(this);
            _fadeImage.gameObject.SetActive(true);
            _fadeImage.color = Color.black;
            StartCoroutine(FadeIn());
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(FadeAndLoad(sceneName));
        }

        private IEnumerator FadeIn()
        {
            float t = 0;
            while (t < _fadeDuration)
            {
                t += Time.deltaTime;
                _fadeImage.color = new Color(0, 0, 0, 1 - t / _fadeDuration);
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, 0);
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            float t = 0;
            while (t < _fadeDuration)
            {
                t += Time.deltaTime;
                _fadeImage.color = new Color(0, 0, 0, t / _fadeDuration);
                yield return null;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}