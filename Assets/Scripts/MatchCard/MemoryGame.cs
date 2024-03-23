using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bridge.Scripts.Minigames
{
    //[RequireComponent(typeof(InteractableUnityEventWrapper))]
    //[RequireComponent(typeof(OutlineController))]
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

        //private OutlineController outlineController;
        //public OutlineController OutlineController => outlineController;

        [SerializeField] private bool IsTesting;

        private int copyNeeded = 2;

        public int CopyNeeded => copyNeeded;

        private Action OnHover;
        private Action UnHover;
        private Action OnSelect;

        public Action<MemoryCard> OnSelectedCard;
        public Action OnDeselect;

        private void Awake()
        {
            OnHover += EnableOutline;
            OnSelect += Select;
        }

        private void Start()
        {
            //outlineController = GetComponent<OutlineController>();
            //outlineController.DisableColouredOutline();

            //transform.GetComponent<InteractableUnityEventWrapper>().WhenHover.AddListener(() => OnHover?.Invoke());
            //transform.GetComponent<InteractableUnityEventWrapper>().WhenUnhover.AddListener(() => UnHover?.Invoke());
            //transform.GetComponent<InteractableUnityEventWrapper>().WhenSelect.AddListener(() => OnSelect?.Invoke());
        }

        private void EnableOutline()
        {
            if (!isEnabled) { return; }

            if (isFlippedOpen) { return; }
            OnHover -= EnableOutline;
            //outlineController.EnableColouredOutline();
            UnHover += DisableOutline;
        }
        private void DisableOutline()
        {
            if (!isEnabled) { return; }

            if (isFlippedOpen) { return; }
            UnHover -= DisableOutline;
            //outlineController.DisableColouredOutline();
            OnHover += EnableOutline;
        }
        private void Select()
        {
            if (isDone) { return; }

            if (!isEnabled) { return; }

            //Check if this already flipped open card has been selected again
            if (isFlippedOpen)
            {
                StartCoroutine(FlipClose());
                //OnDeselect?.Invoke();
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
            //outlineController.EnableColouredOutline();
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
