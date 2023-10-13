using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NavigationDrawer.Core
{
    public class NavigationDrawerPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum NavigationType
        {
            Left,
            Right
        }

        [SerializeField, Header("ROOT")]
        private Image backgroundImage;

        [SerializeField]
        private GameObject panelObject;

        [SerializeField]
        private Canvas canvas;

        [SerializeField, Header("SETTINGS")]
        private NavigationType navigationType;

        [SerializeField]
        private bool darkenBackground = true;

        [SerializeField]
        private bool tapBackgroundToClose = true;

        [SerializeField]
        private bool openOnStart;

        [SerializeField]
        private float animationDuration = 0.5f;

        private int animState;
        private float maxPosition;
        private float minPosition;
        private float animStartTime;
        private float animDeltaTime;
        private float currentBackgroundAlpha;

        private RectTransform rectTransform;
        private RectTransform backgroundRectTransform;
        private GameObject backgroundGameObject;
        private CanvasGroup backgroundCanvasGroup;

        private Vector2 currentPos;
        private Vector2 tempVector2;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            rectTransform = gameObject.GetComponent<RectTransform>();
            backgroundRectTransform = backgroundImage.GetComponent<RectTransform>();
            backgroundCanvasGroup = backgroundImage.GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (navigationType == NavigationType.Left)
            {
                maxPosition = rectTransform.rect.width / 2;
            }
            else if (navigationType == NavigationType.Right)
            {
                maxPosition = -rectTransform.rect.width / 2;
            }

            minPosition = -maxPosition;

            RefreshBackgroundSize();

            backgroundGameObject = backgroundImage.gameObject;

            if (openOnStart)
            {
                Open();
            }
            else
            {
                backgroundGameObject.SetActive(false);
                panelObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (animState == 1)
            {
                animDeltaTime = Time.realtimeSinceStartup - animStartTime;

                if (animDeltaTime <= animationDuration)
                {
                    rectTransform.anchoredPosition = QuintOut(currentPos, new Vector2(maxPosition, rectTransform.anchoredPosition.y), animDeltaTime, animationDuration);
                    if (darkenBackground)
                    {
                        backgroundCanvasGroup.alpha = QuintOut(currentBackgroundAlpha, 1f, animDeltaTime, animationDuration);
                    }
                }
                else
                {
                    rectTransform.anchoredPosition = new Vector2(maxPosition, rectTransform.anchoredPosition.y);
                    if (darkenBackground)
                    {
                        backgroundCanvasGroup.alpha = 1f;
                    }

                    animState = 0;
                }
            }
            else if (animState == 2)
            {
                animDeltaTime = Time.realtimeSinceStartup - animStartTime;
                if (animDeltaTime <= animationDuration)
                {
                    rectTransform.anchoredPosition = QuintOut(currentPos, new Vector2(minPosition, rectTransform.anchoredPosition.y), animDeltaTime, animationDuration);
                    if (darkenBackground)
                    {
                        backgroundCanvasGroup.alpha = QuintOut(currentBackgroundAlpha, 0f, animDeltaTime, animationDuration);
                    }
                }
                else
                {
                    rectTransform.anchoredPosition = new Vector2(minPosition, rectTransform.anchoredPosition.y);
                    if (darkenBackground)
                    {
                        backgroundCanvasGroup.alpha = 0f;
                    }

                    backgroundGameObject.SetActive(false);
                    panelObject.SetActive(false);

                    animState = 0;
                }
            }

            if (navigationType == NavigationType.Left)
            {
                rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(rectTransform.anchoredPosition.x, minPosition, maxPosition), rectTransform.anchoredPosition.y);
            }
            else if (navigationType == NavigationType.Right)
            {
                rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(rectTransform.anchoredPosition.x, maxPosition, minPosition), rectTransform.anchoredPosition.y);
            }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            RefreshBackgroundSize();

            animState = 0;

            backgroundGameObject.SetActive(true);
            panelObject.SetActive(true);
        }

        public void OnDrag(PointerEventData data)
        {
            tempVector2 = rectTransform.anchoredPosition;
            tempVector2.x += data.delta.x;

            rectTransform.anchoredPosition = tempVector2;

            if (darkenBackground)
            {
                backgroundCanvasGroup.alpha = 1 - (maxPosition - rectTransform.anchoredPosition.x) / (maxPosition - minPosition);
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (navigationType == NavigationType.Left)
            {
                if (Mathf.Abs(data.delta.x) >= 0.5f)
                {
                    if (data.delta.x > 0.5f)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    if ((rectTransform.anchoredPosition.x - minPosition) > (maxPosition - rectTransform.anchoredPosition.x))
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else if (navigationType == NavigationType.Right)
            {
                if (Mathf.Abs(data.delta.x) >= 0.5f)
                {
                    if (data.delta.x < 0.5f)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    if ((rectTransform.anchoredPosition.x - minPosition) < (maxPosition - rectTransform.anchoredPosition.x))
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }

        public void BackgroundTap()
        {
            if (tapBackgroundToClose)
            {
                Close();
            }
        }

        public void Open()
        {
            RefreshBackgroundSize();
            backgroundGameObject.SetActive(true);
            panelObject.SetActive(true);
            currentPos = rectTransform.anchoredPosition;
            currentBackgroundAlpha = backgroundCanvasGroup.alpha;
            backgroundCanvasGroup.blocksRaycasts = true;
            animStartTime = Time.realtimeSinceStartup;
            animState = 1;
        }

        public void Close()
        {
            currentPos = rectTransform.anchoredPosition;
            currentBackgroundAlpha = backgroundCanvasGroup.alpha;
            backgroundCanvasGroup.blocksRaycasts = false;
            animStartTime = Time.realtimeSinceStartup;
            animState = 2;
        }

        protected virtual float QuintOut(float startValue, float endValue, float time, float duration)
        {
            var differenceValue = endValue - startValue;
            time = Mathf.Clamp(time, 0f, duration);
            time /= duration;

            if (time == 0f)
            {
                return startValue;
            }

            if (time == 1f)
            {
                return endValue;
            }

            time--;
            return differenceValue * (time * time * time * time * time + 1) + startValue;
        }

        private void RefreshBackgroundSize()
        {
            if (navigationType == NavigationType.Left)
            {
                var width = canvas.GetComponent<RectTransform>().rect.width;
                backgroundRectTransform.sizeDelta = new Vector2(width, backgroundRectTransform.sizeDelta.y);
            }
            else if (navigationType == NavigationType.Right)
            {
                var width = canvas.GetComponent<RectTransform>().rect.width;
                backgroundRectTransform.sizeDelta = new Vector2(width, backgroundRectTransform.sizeDelta.y);
                backgroundRectTransform.localPosition = new Vector2(-(rectTransform.rect.width / 2), 0);
            }
        }

        private Vector2 QuintOut(Vector2 startValue, Vector2 endValue, float time, float duration)
        {
            var value = startValue;
            value.x = QuintOut(startValue.x, endValue.x, time, duration);
            value.y = QuintOut(startValue.y, endValue.y, time, duration);
            return value;
        }
    }
}