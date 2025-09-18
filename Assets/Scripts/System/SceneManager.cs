using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GameSystem
{
    /// <summary>
    /// 씬 전환 및 로딩 관리
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        [Header("씬 설정")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string springWorldScene = "SpringWorld";
        [SerializeField] private string summerWorldScene = "SummerWorld";
        [SerializeField] private string autumnWorldScene = "AutumnWorld";
        [SerializeField] private string winterWorldScene = "WinterWorld";
        [SerializeField] private string combatArenaScene = "CombatArena";
        [SerializeField] private string questHubScene = "QuestHub";
        
        [Header("로딩 설정")]
        [SerializeField] private float loadingDelay = 1f;
        [SerializeField] private bool showLoadingScreen = true;
        
        [Header("로딩 UI")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Slider loadingBar;
        [SerializeField] private TMPro.TextMeshProUGUI loadingText;
        
        private static SceneManager _instance;
        public static SceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SceneManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SceneManager");
                        _instance = go.AddComponent<SceneManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        #region 씬 전환 메서드
        
        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void LoadMainMenu()
        {
            StartCoroutine(LoadSceneAsync(mainMenuScene));
        }
        
        /// <summary>
        /// 봄 테마 월드로 이동
        /// </summary>
        public void LoadSpringWorld()
        {
            StartCoroutine(LoadSceneAsync(springWorldScene));
        }
        
        /// <summary>
        /// 여름 테마 월드로 이동
        /// </summary>
        public void LoadSummerWorld()
        {
            StartCoroutine(LoadSceneAsync(summerWorldScene));
        }
        
        /// <summary>
        /// 가을 테마 월드로 이동
        /// </summary>
        public void LoadAutumnWorld()
        {
            StartCoroutine(LoadSceneAsync(autumnWorldScene));
        }
        
        /// <summary>
        /// 겨울 테마 월드로 이동
        /// </summary>
        public void LoadWinterWorld()
        {
            StartCoroutine(LoadSceneAsync(winterWorldScene));
        }
        
        /// <summary>
        /// 전투 아레나로 이동
        /// </summary>
        public void LoadCombatArena()
        {
            StartCoroutine(LoadSceneAsync(combatArenaScene));
        }
        
        /// <summary>
        /// 퀘스트 허브로 이동
        /// </summary>
        public void LoadQuestHub()
        {
            StartCoroutine(LoadSceneAsync(questHubScene));
        }
        
        #endregion
        
        #region 비동기 씬 로딩
        
        /// <summary>
        /// 비동기 씬 로딩
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // 로딩 화면 표시
            if (showLoadingScreen && loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }
            
            // 로딩 시작
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // 로딩 진행률 표시
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (loadingBar != null)
                {
                    loadingBar.value = progress;
                }
                
                if (loadingText != null)
                {
                    loadingText.text = $"로딩 중... {Mathf.Round(progress * 100)}%";
                }
                
                // 로딩 완료 후 잠시 대기
                if (asyncLoad.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(loadingDelay);
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // 로딩 화면 숨기기
            if (showLoadingScreen && loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
            
            Debug.Log($"씬 로딩 완료: {sceneName}");
        }
        
        #endregion
        
        #region 편의 메서드
        
        /// <summary>
        /// 현재 씬 이름 반환
        /// </summary>
        public string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// 씬이 로드되었는지 확인
        /// </summary>
        public bool IsSceneLoaded(string sceneName)
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName;
        }
        
        #endregion
    }
}
