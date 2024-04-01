using System.Collections;
using UnityEngine;
using System;
using Outsource.Outline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Minigames
{
    [RequireComponent(typeof(EventClick))]
    [RequireComponent(typeof(Outline))]
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

        private Outline outline;
        private EventClick eventClick;

        [SerializeField] private bool IsTesting;

        private int copyNeeded = 2;

        public int CopyNeeded => copyNeeded;

        public Action<MemoryCard> OnSelectedCard;
        public Action OnDeselect;

        private void Awake()
        {
            eventClick = GetComponent<EventClick>();
        }

        private void Start()
        {
            outline = GetComponent<Outline>();
            SetOutline(false);

            //transform.GetComponent<InteractableUnityEventWrapper>().WhenHover.AddListener(() => OnHover?.Invoke());
            //transform.GetComponent<InteractableUnityEventWrapper>().WhenUnhover.AddListener(() => UnHover?.Invoke());
            //transform.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(() => OnSelect?.Invoke());

            eventClick.OnHover += EnableOutline;
            eventClick.OnSelect += Select;
        }

        public void SetOutline(bool isEnabled)
        {
            outline.enabled = isEnabled;
        }

        private void EnableOutline()
        {
            if (!isEnabled) { return; }

            if (isFlippedOpen) { return; }
            eventClick.OnHover -= EnableOutline;
            SetOutline(true);
            eventClick.OnUnhover += DisableOutline;
        }
        private void DisableOutline()
        {
            if (!isEnabled) { return; }

            if (isFlippedOpen) { return; }
            eventClick.OnUnhover -= DisableOutline;
            SetOutline(false);
            eventClick.OnHover += EnableOutline;
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
                transform.localRotation = Quaternion.Euler(0f, 0f, i);
                yield return new WaitForSeconds(0.01f);
            }
            SetOutline(true);
            OnSelectedCard?.Invoke(this);
        }

        public IEnumerator FlipClose()
        {
            //tweening to flip down
            isFlippedOpen = false;
            for (float i = 0f; i <= 180f; i += 10f)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, i);
                yield return new WaitForSeconds(0.01f);
            }
            DisableOutline();
            OnDeselect?.Invoke();
        }

        public void ResetFlip()
        {
            StartCoroutine(FlipClose());
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
