using System.Collections;
using UnityEngine;
using System;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Minigames
{
    [RequireComponent(typeof(EventClick))]
    public class MemoryCard : MonoBehaviour
    {
        private bool isFlippedOpen = false;
        public bool IsFlippedOpen => isFlippedOpen;

        private bool isDone = false;
        public bool IsDone
        {
            get { return isDone; }
            set { isDone = value; }
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        private EventClick eventClick;

        [SerializeField] private bool IsTesting;

        private int copyNeeded = 2;

        public int CopyNeeded => copyNeeded;

        private Vector3 originalScale;
        [SerializeField] [Range(0f, 0.2f)] private float scaleAdder;

        public Action<MemoryCard> OnSelectedCard;
        public Action OnDeselect;

        private void Awake()
        {
            eventClick = GetComponent<EventClick>();
            scaleAdder = 0.15f;
        }

        private void Start()
        {
            originalScale = transform.parent.localScale;
            eventClick.OnHover += EnableHighlightEffect;
            eventClick.OnSelect += Select;
        }

        private void EnableHighlightEffect()
        {
            if (!isEnabled) { return; }

            eventClick.OnHover -= EnableHighlightEffect;
            ShowHighlightEffect(false);
        }
        private void DisableHighlightEffect()
        {
            if (!isEnabled) { return; }

            eventClick.OnUnhover -= DisableHighlightEffect;
            ShowHighlightEffect(true);
        }
        private void Select()
        {
            if (isDone) { return; }

            if (!isEnabled) { return; }

            //Check if this already flipped open card has been selected again
            if (isFlippedOpen)
            {
                StartCoroutine(FlipClose());
                return;
            }
            StartCoroutine(FlipOpen());
        }

        public IEnumerator FlipOpen()
        {
            //tweening to flip open
            isFlippedOpen = true;
            for (float i = 180f; i >= 0f; i -= 10f)
            {
                transform.parent.localRotation = Quaternion.Euler(0f, i, 0f);
                yield return new WaitForSeconds(0.01f);
            }
            OnSelectedCard?.Invoke(this);
        }

        public IEnumerator FlipClose()
        {
            //tweening to flip down
            isFlippedOpen = false;
            for (float i = 0f; i <= 180f; i += 10f)
            {
                transform.parent.localRotation = Quaternion.Euler(0f, i, 0f);
                yield return new WaitForSeconds(0.01f);
            }
            DisableHighlightEffect();
            OnDeselect?.Invoke();
        }

        public void ResetFlip()
        {
            StartCoroutine(FlipClose());
        }

        public void ShowHighlightEffect(bool isUndo)
        {
            if (!isUndo)
            {
                var endScale = new Vector3(originalScale.x + scaleAdder, originalScale.y + scaleAdder, originalScale.z + scaleAdder);
                transform.parent.DOScale(endScale, .6f);
                eventClick.OnUnhover += DisableHighlightEffect;
                return;
            }
            transform.parent.DOScale(originalScale, .6f);
            eventClick.OnHover += EnableHighlightEffect;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(MemoryCard))]
        public class MemoryCard_Editor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector(); // for other non-HideInInspector fields

                MemoryCard script = (MemoryCard)target;

                if (script.IsTesting)
                {
                    if (GUILayout.Button("Flip Up (testing)"))
                    {
                        script.StartCoroutine(script.FlipOpen());
                    }
                    if (GUILayout.Button("Flip Down (testing)"))
                    {
                        script.StartCoroutine(script.FlipClose());
                    }
                }
            }
        }
#endif
    }
}
